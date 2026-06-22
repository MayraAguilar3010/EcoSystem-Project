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
    }
}