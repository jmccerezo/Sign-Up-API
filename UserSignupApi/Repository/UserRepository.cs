using UserSignupApi.Data;
using UserSignupApi.Models;

namespace UserSignupApi.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _dataContext;
        public UserRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task RegisterUser(User user)
        {
            await _dataContext.Users.AddAsync(user);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<User> LoginUser(User user)
        {
            return await _dataContext.Users.SingleOrDefaultAsync(u => u.Email == user.Email);
        }

        public async Task<User> VerifyUser(string token)
        {
            var user = await _dataContext.Users.SingleOrDefaultAsync(u => u.VerificationToken == token);

            if (user != null && user.VerifiedAt == null)
            {
                user.VerifiedAt = DateTime.Now;

                await _dataContext.SaveChangesAsync();
            }

            return user;
        }

        public async Task<User> ForgotPassword(User user)
        {
            var _user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (_user != null)
            {
                _user.ResetPasswordToken = user.ResetPasswordToken;
                _user.ResetTokenExpiry = DateTime.Now.AddDays(1);

                await _dataContext.SaveChangesAsync();
            }

            return _user;
        }

        public async Task<User> ResetPassword(User user)
        {
            var _user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == user.ResetPasswordToken);

            if (_user != null)
            {
                _user.PasswordHash = user.PasswordHash;
                _user.PasswordSalt = user.PasswordSalt;
                _user.ResetPasswordToken = null;
                _user.ResetTokenExpiry = null;

                await _dataContext.SaveChangesAsync();
            }

            return _user;
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _dataContext.Users.ToListAsync();
        }
    }
}
