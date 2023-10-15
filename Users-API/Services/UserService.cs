using System.Security.Cryptography;
using System.Text;
using UsersAPI.Models;
using UsersAPI.Repository;

namespace UsersAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
        private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
        private static string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }
        public bool CheckExistingUser(string email)
        {
            var users = _userRepository.GetAllUsers().Result;
            return users.Any(u => u.Email == email);
        }

        public async Task<User> RegisterUser(UserRegisterRequest request)
        {
            User user = new();

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.VerificationToken = CreateRandomToken();

            await _userRepository.RegisterUser(user);

            return user;
        }

        public async Task<User> LoginUser(UserLoginRequest request)
        {
            User user = new()
            {
                Email = request.Email
            };

            var userLogin = await _userRepository.LoginUser(user);

            if (userLogin == null)
            {
                return null;
            }

            var verified = VerifyPasswordHash(request.Password, userLogin.PasswordHash, userLogin.PasswordSalt);

            if (!verified)
            {
                return null;
            }

            return userLogin;
        }

        public async Task<User> VerifyUser(string token)
        {
            var user  = await _userRepository.VerifyUser(token);

            return user;
        }

        public async Task<User> ForgotPassword(string email)
        {
            User user = new()
            {
                Email = email,
                ResetPasswordToken = CreateRandomToken()
            };

            var _user = await _userRepository.ForgotPassword(user);

            return _user;
        }

        public async Task<User> ResetPassword(UserResetPasswordRequest request)
        {
            User user = new();

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.ResetPasswordToken = request.Token;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            var _user = await _userRepository.ResetPassword(user);

            return _user;
        }
    }
}
