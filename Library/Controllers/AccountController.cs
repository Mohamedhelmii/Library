using Library.DTO;
using Library.Model;
using Library.Repo;
using Library.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UnitOfWork unit;
        private readonly UserManager<ApplicationUser> user;
        private readonly IConfiguration config;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly RoleManager<IdentityRole> role;

        public AccountController
            (UnitOfWork unit,
            UserManager<ApplicationUser> user, RoleManager<IdentityRole> role,
            IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            this.unit = unit;
            this.user = user;
            this.role = role;
            this.config = config;
            this.httpContextAccessor = httpContextAccessor;
        }
        
        [HttpPost("Register")]
        public async Task<ActionResult<GeneralResponse>> Register(RegisterDTO registerDTO) 
        {
            //بعد كده خلى اليوزر نيم هو الايميل علشان تضمن انه مش مكرر
            var appUser = new ApplicationUser();
            appUser.UserName = registerDTO.FirstName + registerDTO.LastName;
            appUser.Email = registerDTO.Email;
            var res = await user.CreateAsync(appUser, registerDTO.Password);
            if (ModelState.IsValid)
            {
                if(res.Succeeded)
                {
                    await user.AddToRoleAsync(appUser, "Member");
                    unit.Save();
                    return new GeneralResponse { code = 200, message = "User registered successfully! Now You Are Member...", data = registerDTO.FirstName + " " + registerDTO.LastName };
                }
                foreach (var item in res.Errors) 
                {
                    ModelState.AddModelError("", item.Description);
                }
            }
            return BadRequest(ModelState);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<GeneralResponse>> Login(LoginDTO loginDTO)
        {
            if (ModelState.IsValid)
            {
                var userFromDb = await user.FindByEmailAsync(loginDTO.Email);
                if ((userFromDb != null)) 
                {
                    bool found = await user.CheckPasswordAsync(userFromDb, loginDTO.Password);
                    if (found) 
                    {
                        if(userFromDb.isDeleted)
                        {
                            return Unauthorized(new GeneralResponse { message = "your account has been disapled.."});
                        }
                        //Generate Token

                        List<Claim> UserClaims = new List<Claim>();

                        //Token Genrated id change (JWT Predefind Claims )
                        UserClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                        UserClaims.Add(new Claim(ClaimTypes.NameIdentifier, userFromDb.Id));
                        UserClaims.Add(new Claim(ClaimTypes.Name, userFromDb.UserName));

                        var UserRoles = await user.GetRolesAsync(userFromDb);

                        foreach (var roleNAme in UserRoles)
                        {
                            UserClaims.Add(new Claim(ClaimTypes.Role, roleNAme));
                        }

                        var SignInKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                                config["JWT:SecritKey"]));

                        SigningCredentials signingCred =
                            new SigningCredentials
                            (SignInKey, SecurityAlgorithms.HmacSha256);

                        //design token
                        JwtSecurityToken mytoken = new JwtSecurityToken(
                            audience: config["JWT:AudienceIP"],
                            issuer: config["JWT:IssuerIP"],
                            expires: DateTime.Now.AddHours(1),
                            claims: UserClaims,
                            signingCredentials: signingCred

                            );
                        //generate token response

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(mytoken),
                            expiration = DateTime.Now.AddHours(1)//mytoken.ValidTo
                            //
                        });
                    }
                }
                ModelState.AddModelError("Username", "Username OR Password  Invalid");
            }
            return BadRequest(ModelState);
        }

        
    }
}
