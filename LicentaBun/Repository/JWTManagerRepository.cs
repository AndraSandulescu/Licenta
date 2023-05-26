using LicentaBun.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LicentaBun.Repository
{
    public class JWTManagerRepository : IJWTManagerRepository
    {

        private readonly IConfiguration _iconfiguration;
        private IUserRepository _userRepository;
        public JWTManagerRepository(IConfiguration iconfiguration)
        {
            _iconfiguration = iconfiguration;
            _userRepository = new UserRepository(new LicentaContext());
        }
        public Tokens Authenticate(LoginRequest users)
        {

            IEnumerable<User> usersRecords = _userRepository.GetUsers();

            if (!usersRecords.Any(x => x.Nickname == users.Nickname && x.Password == users.Password))
            {
                return null;
            }

            // Else we generate JSON Web Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_iconfiguration["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                  {
                    new Claim(ClaimTypes.Name, users.Nickname)
                  }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new Tokens { Token = tokenHandler.WriteToken(token) };
        }
    }
}
