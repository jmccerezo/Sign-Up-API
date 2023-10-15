using Microsoft.AspNetCore.Mvc;
using UsersAPI.Models;
using UsersAPI.Services;

namespace UsersAPI.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> RegisterUser(UserRegisterRequest request)
        {
            var userExists = _userService.CheckExistingUser(request.Email);

            if (userExists)
            {
                return BadRequest("This email is already registered, please use another email.");
            }

            var newUser = await _userService.RegisterUser(request);

            return Ok(newUser);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(UserLoginRequest request)
        {
            var user = await _userService.LoginUser(request);

            if (user == null)
            {
                return BadRequest("Incorrect Username or Password");
            }

            if (user.VerifiedAt == null)
            {
                return BadRequest("Please verify first.");
            }

            return Ok($"Welcome, {user.FirstName}.");
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyUser(string token)
        {
            var user = await _userService.VerifyUser(token);

            if (user == null)
            {
                return BadRequest("Invalid token.");
            }

            return Ok("User verified successfully.");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userService.ForgotPassword(email);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok($"You may reset your password. This is your token: {user.ResetPasswordToken}");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(UserResetPasswordRequest request)
        {
            var user = await _userService.ResetPassword(request);

            if (user == null || user.ResetTokenExpiry < DateTime.Now)
            {
                return BadRequest("Invalid token.");
            }

            return Ok("Your password has been reset.");
        }
    }
}
