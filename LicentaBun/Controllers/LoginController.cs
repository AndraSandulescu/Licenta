using LicentaBun.Models;
using LicentaBun.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace LicentaBun.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase, IDisposable
    {

        private readonly IJWTManagerRepository _jWTManager;
        private LicentaContext _dbContext;
        private IUserRepository _userRepository;

        public LoginController(IJWTManagerRepository jWTManager)
        {
            _jWTManager = jWTManager;
            _dbContext = new LicentaContext();
            _userRepository = new UserRepository(_dbContext);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Authenticate(LoginRequest loginData)
        {
            var token = _jWTManager.Authenticate(loginData);

            if (token == null)
            {
                return Unauthorized();
            }


            User user = _userRepository.GetUserByNickname(loginData.Nickname);
            LoginResponse loginResponse = new LoginResponse(
                    user.Nickname,
                    user.PkUsers,
                    token.Token
                );

            // Set multiple CORS headers
            Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

            return Ok(loginResponse);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public IActionResult Register(RegisterRequest registerData)
        {

            IEnumerable<User> userRecords = _userRepository.GetUsers();
            if (userRecords.Any(x => x.Nickname == registerData.Nickname))
            {

                return Unauthorized();
            }


            User newUser = new User()
            {
                Nickname = registerData.Nickname,
                Password = registerData.Password
            };
            _userRepository.InsertUser(newUser);
            _userRepository.Save();

            return Ok();
        }

        [HttpGet]
        [Route("test")]
        public IActionResult Test()
        {



            return Ok("da");
        }

        public void Dispose()
        {
            _userRepository.Dispose();
        }


    }
}
