using Library.DTO;
using Library.Model;
using Library.Repo;
using Library.Response;
using Library.Services;
using Library.Verification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccounttController : ControllerBase
    {
        private readonly UnitOfWork unit;
        private readonly UserManager<ApplicationUser> user;
        private readonly IConfiguration config;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly RoleManager<IdentityRole> role;
        private readonly IEmailService emailService;

        public AccounttController
            (UnitOfWork unit,
            UserManager<ApplicationUser> user, RoleManager<IdentityRole> role,
            IConfiguration config, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            this.unit = unit;
            this.user = user;
            this.role = role;
            this.config = config;
            this.httpContextAccessor = httpContextAccessor;
            this.emailService = emailService;
        }
       
        [HttpPost("Register")]
        public async Task<ActionResult<GeneralResponse>> Register(RegisterDTO registerDTO)
        {
            var appUser = new ApplicationUser();
            appUser.UserName = registerDTO.Email;
            appUser.Email = registerDTO.Email;
            var res = await user.CreateAsync(appUser, registerDTO.Password);
            if (ModelState.IsValid)
            {
                if (res.Succeeded)
                {
                    var token = await user.GenerateEmailConfirmationTokenAsync(appUser);
                    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes((token)));
                    //var encodedEmail = WebUtility.UrlEncode(ApplicationUser.Email);
                    var context = httpContextAccessor.HttpContext?.Request;
                    var baseUrl = $"{context?.Scheme}://{context?.Host}";
                    var confirmationLink = $"{baseUrl}/api/Account/VerifyUser?token={encodedToken}&emailTo={appUser.Email}";
                    var htmlContent = $"<a href='{confirmationLink}'>clicking here</a>";

                    new VerificationConfirmation().SendEmailAsync(appUser.Email, "Verification", htmlContent);
                    return new GeneralResponse { message = "Created Successfully",  code = 201 , data = token};
                }
                foreach (var item in res.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
            }
            return BadRequest(ModelState);
        }


        [HttpGet("VerifyUser")]
        public async Task<ActionResult<GeneralResponse>> VerifyUser(string token, string emailTo)
        {
            // var decodedEmail = WebUtility.UrlDecode(emailTO);
            var userr = await user.FindByEmailAsync(emailTo);
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var res = await user.ConfirmEmailAsync(userr, decodedToken);
            if (res.Succeeded)
                return new GeneralResponse { code = 200,  message = "Verified", data = userr.Id };

            return BadRequest(new GeneralResponse { code = 400, message = "Unverified", data = "Unfound User" });
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
                        if (userFromDb.isDeleted)
                        {
                            return Unauthorized(new GeneralResponse { message = "your account has been disapled.." });
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
