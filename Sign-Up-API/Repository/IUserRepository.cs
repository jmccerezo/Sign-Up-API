using SignUpAPI.Models;

namespace SignUpAPI.Repository
{
    public interface IUserRepository
    {
        Task RegisterUser(User user);
        Task<User> LoginUser(User user);
        Task<User> VerifyUser(string token);
        Task<User> ForgotPassword(User user);
        Task<User> ResetPassword(User user);
        Task<List<User>> GetAllUsers();
    }
}