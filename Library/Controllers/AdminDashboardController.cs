using Library.DTO;
using Library.Model;
using Library.Repo;
using Library.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading.Tasks;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDashboardController : ControllerBase
    {
        private readonly UnitOfWork unit;
        private readonly UserManager<ApplicationUser> user;
        private readonly RoleManager<IdentityRole> role;

        public AdminDashboardController(UnitOfWork unit, UserManager<ApplicationUser> user, RoleManager<IdentityRole> role)
        {
            this.unit = unit;
            this.user = user;
            this.role = role;
        }


        #region role
        [Authorize(Roles = "Admin")]
        [HttpPost("AddRole")]
        public async Task<ActionResult<GeneralResponse>> AddRole(string RoleName)
        {
            var res = await role.CreateAsync(new IdentityRole { Name = RoleName });
            if (res.Succeeded) return new GeneralResponse { code = 200, message = "Role Added Successfully" };
            foreach (var erorr in res.Errors)
                ModelState.AddModelError("", erorr.Description);
            return BadRequest(ModelState);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("AssignRole")]
        public async Task<ActionResult<GeneralResponse>> AssignRole(string UserId, string RoleId)
        {
            var User = await user.FindByIdAsync(UserId);
            var Role = await role.FindByIdAsync(RoleId);
            await user.AddToRoleAsync(User, Role.Name);
            return new GeneralResponse { code = 201, message = "Role assigned Successfully" };
        }
        #endregion


        [Authorize(Roles = "Admin")]
        [HttpGet("GetUserCount")]
        public async Task<ActionResult<GeneralResponse>> GetUserCount()
        {
            var U = await user.Users.Where(u=> u.isDeleted == false).CountAsync();
            var Admin = await user.GetUsersInRoleAsync("Admin");
            var adminCount = Admin.Count();
            return new GeneralResponse { code = 200, data = "User count  = " + (U - adminCount).ToString() + " (Not Included Admin), " +
                $" Admin Count = {adminCount}" };
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("DeleteUser")]
        public async Task<ActionResult<GeneralResponse>> DeleteUser(string UserId) 
        {
            var u = await user.FindByIdAsync(UserId);
            if (u == null) return new GeneralResponse { message = "Not Found!!" };
            u.isDeleted = true;
            await user.SetLockoutEnabledAsync(u, true);
            await user.SetLockoutEndDateAsync(u, DateTime.UtcNow.AddYears(100));
            var result = await user.UpdateAsync(u);
            if (result.Succeeded)
            {
                return new GeneralResponse { code = 200, message = "User Soft Deleted Succsessfully" };
            }
            return BadRequest(result.Errors);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("RestoreUser")]
        public async Task<ActionResult<GeneralResponse>> RestoreUser(string UserId)
        {
            var u = await user.FindByIdAsync(UserId);
            if (u == null) return new GeneralResponse { message = "Not Found!!" };
            u.isDeleted = false; 
            await user.SetLockoutEndDateAsync(u, null);
            var result = await user.UpdateAsync(u);
            if (result.Succeeded)
            {
                return new GeneralResponse { code = 200, message = "User restored Succsessfully" };
            }
            return BadRequest(result.Errors);
        }

        #region MyRegion
        //[Authorize(Roles = "Admin")]
        //[HttpGet("GetAllAuthorWithDeletedForAdmins")]
        //public ActionResult<GeneralResponse> GetAllAuthorWithDeletedForAdmins()
        //{
        //    var a = unit.AuthorRepo.GetAllAuthorwITHDetails(false);
        //    var aDTO = new List<AuthorWithBookDTO>();
        //    foreach (var author in a)
        //    {
        //        var aDTO2 = new AuthorWithBookDTO();
        //        aDTO2.AuthorName = author.AuthorName;
        //        aDTO2.BioGraphy = author.BioGraphy;
        //        aDTO2.nationality = author.nationality;
        //        aDTO2.Books = author.AuthorBooks.Select(b => new AuthorBooksDTO
        //        {
        //            Title = b.Book.Title,
        //            Description = b.Book.Description,
        //            Category = b.Book.Category,
        //            TotalCopies = b.Book.TotalCopies,
        //            AvaliableCopies = b.Book.AvaliableCopies,
        //        }).ToList();
        //        aDTO.Add(aDTO2);
        //    }
        //    return Ok(new GeneralResponse { data = aDTO});
        //}
        #endregion



    }
}
