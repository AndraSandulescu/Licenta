using LicentaBun.Models;


namespace LicentaBun.Repository
{
    public interface IJWTManagerRepository
    {
        Tokens Authenticate(LoginRequest users);

    }
}
