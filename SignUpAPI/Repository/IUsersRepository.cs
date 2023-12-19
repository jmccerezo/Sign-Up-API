using SignUpAPI.Models;

namespace SignUpAPI.Repository
{
    public interface IUsersRepository
    {
        Task RegisterUser(User user);
        Task<User?> LoginUser(string email);
        Task<User?> VerifyUser(string token);
        Task<User?> ForgotPassword(User user);
        Task<User?> ResetPassword(User user);
        Task<List<User>> GetAllUsers();
    }
}