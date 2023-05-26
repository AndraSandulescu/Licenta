using LicentaBun.Models;

namespace LicentaBun.Repository
{
    public interface IUserRepository : IDisposable
    {

        IEnumerable<User> GetUsers();
        User GetUserById(int userID);
        User GetUserByNickname(string nickname);
        void InsertUser(User user);
        void DeleteUser(int userID);
        void UpdateUser(User user);
        void Save();
    }
}
