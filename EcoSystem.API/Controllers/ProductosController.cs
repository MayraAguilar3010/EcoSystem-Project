using EcoSystem.Data;
using EcoSystem.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcoSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.Productos.ToList());
        }

        [HttpPost]
        public IActionResult Post(Producto producto)
        {
            _context.Productos.Add(producto);
            _context.SaveChanges();

            return Ok(producto);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, Producto productoActualizado)
        {
            var producto = _context.Productos.Find(id);

            if (producto == null)
                return NotFound();

            producto.Nombre = productoActualizado.Nombre;
            producto.Precio = productoActualizado.Precio;
            producto.Stock = productoActualizado.Stock;

            _context.SaveChanges();

            return Ok(producto);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var producto = _context.Productos.Find(id);

            if (producto == null)
                return NotFound();

            _context.Productos.Remove(producto);
            _context.SaveChanges();

            return Ok();
        }
    }
}