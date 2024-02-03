using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace questionmark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly DataContext _context;
        public ClientsController(DataContext context)
        {
            _context = context;
        }
        [HttpGet("GetAll")]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            List<User> response = new List<User>();
            response = await _context.Users.ToListAsync();
            return Ok(response);
        }
        [HttpGet("GetOne"), Authorize(Roles ="Admin")]
        public async Task<ActionResult<User>> GetOne(string username) 
        {
            var response= await _context.Users.FindAsync(username);
            return Ok(response);
        }


    }
}
