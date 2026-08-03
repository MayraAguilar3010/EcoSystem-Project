const API_BASE = 'https://ecosystem-connect-api.onrender.com/';
const state = { token: localStorage.getItem('ecosystem.token'), username: localStorage.getItem('ecosystem.username'), role: localStorage.getItem('ecosystem.role'), productos: [] };

const $ = (id) => document.getElementById(id);
const loginPage = $('loginPage');
const productosPage = $('productosPage');

function setBusy(isBusy) {
  ['loginButton', 'saveButton', 'reloadButton'].forEach(id => $(id).disabled = isBusy);
  $('loadingLabel').textContent = isBusy ? 'Cargando...' : '';
}

function showMessage(id, text, success = false) {
  const el = $(id);
  el.textContent = text || '';
  el.classList.toggle('success', success);
}

function showScreen(name) {
  loginPage.classList.toggle('active', name === 'login');
  productosPage.classList.toggle('active', name === 'productos');
}

async function api(path, options = {}) {
  const headers = { ...(options.headers || {}) };
  if (options.body) headers['Content-Type'] = 'application/json';
  if (state.token) headers.Authorization = `Bearer ${state.token}`;
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), 30000);
  try {
    const response = await fetch(API_BASE + path, { ...options, headers, signal: controller.signal });
    if (!response.ok) throw new Error(messageForStatus(response.status));
    if (response.status === 204) return null;
    const text = await response.text();
    return text ? JSON.parse(text) : null;
  } catch (error) {
    if (error.name === 'AbortError') throw new Error('La solicitud tardo demasiado.');
    throw error;
  } finally {
    clearTimeout(timeout);
  }
}

function messageForStatus(status) {
  if (status === 400) return 'Datos invalidos. Revisa el formulario.';
  if (status === 401) return 'Sesion vencida o credenciales incorrectas.';
  if (status === 403) return 'Tu usuario no tiene permiso para esta operacion.';
  if (status === 404) return 'Producto no encontrado.';
  if (status >= 500) return 'Error del servidor.';
  return `La API respondio con codigo ${status}.`;
}

async function login() {
  const username = $('username').value.trim();
  const password = $('password').value;
  if (!username || !password) return showMessage('loginMessage', 'Captura usuario y contrasena.');
  setBusy(true);
  showMessage('loginMessage', '');
  try {
    const result = await api('api/Auth/login', { method: 'POST', body: JSON.stringify({ username, password }) });
    state.token = result.token;
    state.username = result.username;
    state.role = result.role;
    localStorage.setItem('ecosystem.token', state.token);
    localStorage.setItem('ecosystem.username', state.username);
    localStorage.setItem('ecosystem.role', state.role);
    $('password').value = '';
    showScreen('productos');
    await loadProductos();
  } catch (error) {
    showMessage('loginMessage', error.message);
  } finally {
    setBusy(false);
  }
}

function logout() {
  localStorage.removeItem('ecosystem.token');
  localStorage.removeItem('ecosystem.username');
  localStorage.removeItem('ecosystem.role');
  state.token = state.username = state.role = null;
  state.productos = [];
  renderProductos();
  showScreen('login');
}

async function loadProductos() {
  setBusy(true);
  showMessage('formMessage', '');
  try {
    state.productos = await api('api/Productos');
    $('sessionInfo').textContent = `Usuario: ${state.username || ''} | Rol: ${state.role || ''}`;
    renderProductos();
  } catch (error) {
    showMessage('formMessage', error.message);
    if (error.message.includes('Sesion')) logout();
  } finally {
    setBusy(false);
  }
}

function renderProductos() {
  const list = $('productosList');
  if (!state.productos.length) {
    list.innerHTML = '<p class="muted">No hay productos para mostrar.</p>';
    return;
  }
  list.innerHTML = state.productos.map(p => `
    <article class="product-card">
      <div>
        <h3>${escapeHtml(p.nombre)}</h3>
        <p class="product-meta">Precio: $${Number(p.precio).toFixed(2)} | Existencias: ${p.stock}</p>
      </div>
      <div class="actions">
        <button type="button" onclick="editProducto(${p.id})">Editar</button>
        <button type="button" class="danger" onclick="deleteProducto(${p.id})">Eliminar</button>
      </div>
    </article>`).join('');
}

function editProducto(id) {
  const producto = state.productos.find(p => p.id === id);
  if (!producto) return;
  $('formTitle').textContent = 'Editar producto';
  $('productId').value = producto.id;
  $('nombre').value = producto.nombre;
  $('precio').value = producto.precio;
  $('stock').value = producto.stock;
}

function clearForm() {
  $('formTitle').textContent = 'Nuevo producto';
  $('productId').value = '';
  $('nombre').value = '';
  $('precio').value = '';
  $('stock').value = '';
  showMessage('formMessage', '');
}

async function saveProducto() {
  const id = Number($('productId').value);
  const producto = { nombre: $('nombre').value.trim(), precio: Number($('precio').value), stock: Number($('stock').value) };
  if (!producto.nombre) return showMessage('formMessage', 'El nombre es obligatorio.');
  if (!Number.isFinite(producto.precio) || producto.precio <= 0) return showMessage('formMessage', 'El precio debe ser mayor que cero.');
  if (!Number.isInteger(producto.stock) || producto.stock < 0) return showMessage('formMessage', 'Las existencias no pueden ser negativas.');
  setBusy(true);
  try {
    if (id) await api(`api/Productos/${id}`, { method: 'PUT', body: JSON.stringify({ id, ...producto }) });
    else await api('api/Productos', { method: 'POST', body: JSON.stringify(producto) });
    showMessage('formMessage', id ? 'Producto actualizado.' : 'Producto creado.', true);
    clearForm();
    await loadProductos();
  } catch (error) {
    showMessage('formMessage', error.message);
  } finally {
    setBusy(false);
  }
}

async function deleteProducto(id) {
  const producto = state.productos.find(p => p.id === id);
  if (!producto || !confirm(`Deseas eliminar ${producto.nombre}?`)) return;
  setBusy(true);
  try {
    await api(`api/Productos/${id}`, { method: 'DELETE' });
    state.productos = state.productos.filter(p => p.id !== id);
    renderProductos();
    showMessage('formMessage', 'Producto eliminado.', true);
  } catch (error) {
    showMessage('formMessage', error.message);
  } finally {
    setBusy(false);
  }
}

function escapeHtml(value) {
  return String(value).replace(/[&<>'"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[c]));
}

$('loginButton').addEventListener('click', login);
$('logoutButton').addEventListener('click', logout);
$('reloadButton').addEventListener('click', loadProductos);
$('saveButton').addEventListener('click', saveProducto);
$('cancelEditButton').addEventListener('click', clearForm);

if (state.token) {
  showScreen('productos');
  loadProductos();
} else {
  showScreen('login');
}
