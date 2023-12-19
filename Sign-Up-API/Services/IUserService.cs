using SignUpAPI.Models;

namespace SignUpAPI.Services
{
    public interface IUserService
    {
        bool CheckExistingUser(string email);
        Task<User> RegisterUser(UserRegisterRequest request);
        Task<User?> LoginUser(UserLoginRequest request);
        Task<User?> VerifyUser(string token);
        Task<User?> ForgotPassword(string email);
        Task<User?> ResetPassword(UserResetPasswordRequest request);
    }
}