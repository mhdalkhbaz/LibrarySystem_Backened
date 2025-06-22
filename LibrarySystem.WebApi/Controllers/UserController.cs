using LibrarySystem.Application.Dto;
using LibrarySystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDto user)
        {
            var userId = await _userService.RegisterUserAsync(user.UserName);
            return Ok(userId);
        }
    }
}
