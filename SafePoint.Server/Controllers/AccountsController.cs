using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SafePoint.Data.Entities;

namespace SafePoint.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IUserStore<ApplicationUser> _userStore;

        public AccountsController(IUserStore<ApplicationUser> userStore)
        {
            _userStore = userStore;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(ApplicationUser user)
        {
            var result = await _userStore.CreateAsync(user, HttpContext.RequestAborted);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return NoContent();
        }
    }
}