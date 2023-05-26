using LicentaBun.Models;
using Microsoft.EntityFrameworkCore;

namespace LicentaBun.Repository
{
    public class UserRepository : IUserRepository
    {

        private LicentaContext context;
        private bool disposed = false;

        public UserRepository(LicentaContext context)
        {
            this.context = context;
        }

        public IEnumerable<User> GetUsers()
        {

            return context.Users.ToList();
        }

        public User GetUserById(int userID)
        {

            return context.Users.Find(userID);
        }

        public User GetUserByNickname(string nickname)
        {

            var result = context.Users.Where(user => user.Nickname == nickname).ToList();
            if (result.Count() > 0)
                return result[0];
            return null;
        }

        public void InsertUser(User user)
        {

            context.Users.Add(user);
        }

        public void DeleteUser(int userID)
        {

            User user = context.Users.Find(userID);
            context.Users.Remove(user);
        }

        public void UpdateUser(User user)
        {

            context.Entry(user).State = EntityState.Modified;
        }

        public void Save()
        {

            context.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
