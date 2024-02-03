using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace questionmark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        public AuthController(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpPost("Register")]
        public async Task<ActionResult<string>> Register(UserDto user)
        {
            HashPassword(user.Password, out byte[] passwordSalt, out byte[] passwordHash);
            var _user = new User();
            _user.Username = user.Username;
            _user.PasswordSalt = passwordSalt;
            _user.PasswordHash = passwordHash;
            if (user.Username == "admin") {
                _user.Role = "Admin";
            }
            _context.Users.Add(_user);
            await _context.SaveChangesAsync();
            return Ok("You are registered succesfully");
        }
        private void HashPassword(string password, out byte[] passwordSalt, out byte[] passwordHash)
        {
            using (var hmac = new HMACSHA512()) {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login(UserDto user)
        {
            var _user = _context.Users.Find(user.Username);
            if (_user == null) {
                return BadRequest("User not found");
            }

            if (!VerifyPassword(user.Password, _user.PasswordSalt, _user.PasswordHash)) {
                return BadRequest("Wrong Password");
            }
            string token = CreateToken(_user);
            return Ok(token);
        }
        private bool VerifyPassword(string password, byte[] passwordSalt, byte[] passwordHash)
        {
            using (var hmac = new HMACSHA512(passwordSalt)) {
                var passwordToVerify = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return passwordToVerify.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim> { 
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
                };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:DefaultToken").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials:creds
                );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;

        }
    }
}
