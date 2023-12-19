﻿using System.Security.Cryptography;
using System.Text;
using SignUpAPI.Models;
using SignUpAPI.Repository;

namespace SignUpAPI.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _userRepository;
        public UsersService(IUsersRepository userRepository)
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
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User user = new()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                VerificationToken = CreateRandomToken(),
            };

            await _userRepository.RegisterUser(user);

            return user;
        }

        public async Task<User?> LoginUser(UserLoginRequest request)
        {
            var userLogin = await _userRepository.LoginUser(request.Email);

            if (userLogin == null) return null;

            var verified = VerifyPasswordHash(request.Password, userLogin.PasswordHash!, userLogin.PasswordSalt!);

            if (!verified) return null;

            return userLogin;
        }

        public async Task<User?> VerifyUser(string token)
        {
            var user  = await _userRepository.VerifyUser(token);

            return user;
        }

        public async Task<User?> ForgotPassword(string email)
        {
            User user = new()
            {
                Email = email,
                ResetPasswordToken = CreateRandomToken()
            };

            var _user = await _userRepository.ForgotPassword(user);

            return _user;
        }

        public async Task<User?> ResetPassword(UserResetPasswordRequest request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User user = new()
            {
                ResetPasswordToken = request.Token,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
            };

            var _user = await _userRepository.ResetPassword(user);

            return _user;
        }
    }
}
