using Library.DTO;
using Library.Model;
using Library.Repo;
using Library.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Security.Claims;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanandReservationController : ControllerBase
    {
        private readonly UnitOfWork unit;
        private readonly UserManager<ApplicationUser> user;

        public LoanandReservationController(UnitOfWork unit, UserManager<ApplicationUser> user)
        {
            this.unit = unit;
            this.user = user;
        }


        #region Loan
        [Authorize(Roles = "Member")]
        [HttpPost("LoanBook")]
        public ActionResult<GeneralResponse> LoanBook(int BookId, string UserId)
        {
            var b = unit.BookRepo.GetByFilter(b => b.Id == BookId && b.IsDeleted == false);
            if (b != null)
            {
                if (b.AvaliableCopies >= 0)
                {
                    var ExistingOnLoan = unit.LoanRepo.GetByFilter(l => l.UserId == UserId && l.BookId == BookId && l.IsReturned == false);
                    if (ExistingOnLoan == null)
                    {
                        var Loan = new Loan
                        {
                            UserId = UserId,
                            BookId = BookId,
                            DueDate = DateTime.Now.AddDays(14),
                            ReturnDate = null
                        };
                        b.AvaliableCopies--;
                        unit.LoanRepo.Add(Loan);
                        unit.Save();
                        return new GeneralResponse { code = 200, message = $"OK, return it at {Loan.DueDate}", data = Loan.Id };
                    }
                    else
                    {
                        return BadRequest(new GeneralResponse { message = "Already loan It!" });
                    }
                }
                else
                {
                    return BadRequest(new GeneralResponse { message = "don't have avaliable coobies, you are able to reserve it.." });
                }
            }
            return BadRequest(new GeneralResponse { message = "This Book Not Found!"});
        }


        [Authorize(Roles = "Librarian,Admin")]
        [HttpPost("ReturnBook")]
        public ActionResult<GeneralResponse> ReturnBook(int LoanId)
        {
            var l = unit.LoanRepo.GetLoanWithBook(l => l.Id == LoanId);
            if(l != null)
            {
                var BookId = l.BookId;
                var UserId = l.UserId;
                if (l.IsReturned == false && l.ReturnDate == null) 
                {
                    l.ReturnDate = DateTime.Now;
                    l.IsReturned = true;
                    l.Book.AvaliableCopies++;
                    unit.Save();
                    return new GeneralResponse { code = 200, message = "Returned Succsessfully", data = l.Book.AvaliableCopies };
                }
            }
            return BadRequest(new GeneralResponse { message = "This User Already returned this book!"});
        }


        [Authorize(Roles = "Member")]
        [HttpPost("RenewLoan")]
        public ActionResult<GeneralResponse> RenewLoan(int LoanId)
        {
            var l = unit.LoanRepo.GetByFilter(l => l.Id == LoanId && l.ReturnDate == null);
            var BookId = l.BookId;
            if (l != null) 
            {
                //عاوز لما اعمل الحجوزات اتاكد ان الكتاب مش محجوز
                var b = unit.BookRepo.GetByFilter(b => b.Id == BookId);
                if(b.AvaliableCopies == 0)
                {
                    var b2 = unit.ReservationRepo.GetAllWithBooks()
                        .Where(b2 => b2.Book.Id == BookId && b2.IsActive == true)
                        .FirstOrDefault();
                    if(b2 != null)
                    {
                        return BadRequest( new GeneralResponse { message = "Sorry, This book is reserved."});
                    }
                }
                l.DueDate = DateTime.Now.AddDays(14);
                unit.LoanRepo.Update(l);
                unit.Save();
                return new GeneralResponse { code = 200, message = "Renew Loan Succsessfully", data=l.DueDate };
            }
            return BadRequest(new GeneralResponse { message = "Invalid Code!!" });
        }


        [Authorize(Roles = "Librarian,Admin")]
        [HttpGet("GetAllActiveLoans")] 
        public ActionResult<GeneralResponse> GetAllActiveLoans()
        {
            var l = unit.LoanRepo.GetAllWithBooksAndUser().Where(l => l.IsReturned == false);
            if (l != null) 
            {
                var LoanDTOList = new List<LoanDTO>();
                foreach(var item in l)
                {
                    var LoanDTO = new LoanDTO();
                    LoanDTO.UserName = item.User.UserName;
                    LoanDTO.BookTitle = item.Book.Title;
                    LoanDTO.LoanDate = item.LoanDate;
                    LoanDTO.DueDate = item.DueDate;
                    LoanDTOList.Add(LoanDTO);
                }    
                return new GeneralResponse { code = 200, data = LoanDTOList };
            }
            return BadRequest(new GeneralResponse { message = "Not Found!!"});
        }


        [Authorize(Roles = "Librarian,Admin")]
        [HttpGet("GetAllOverDueDate")]
        public ActionResult<GeneralResponse> GetAllOverDueDate()
        {
            var l = unit.LoanRepo.GetAllWithBooksAndUser().Where(l => DateTime.Now > l.DueDate && l.IsReturned == false);
            if (l != null)
            {
                var LoanDTOList = new List<LoanDTO>();
                foreach (var item in l)
                {
                    var LoanDTO = new LoanDTO();
                    LoanDTO.UserName = item.User.UserName;
                    LoanDTO.BookTitle = item.Book.Title;
                    LoanDTO.LoanDate = item.LoanDate;
                    LoanDTO.DueDate = item.DueDate;
                    LoanDTOList.Add(LoanDTO);
                }
                return new GeneralResponse { code = 200, data = LoanDTOList };
            }
            return BadRequest(new GeneralResponse { message = "Not Found!!" });
        }

        [Authorize(Roles = "Librarian,Admin")]
        [HttpPost("GetAllLoansForSpecficUser")]
        public async Task<ActionResult<GeneralResponse>> GetAllLoansForSpecficUser(string UserId)
        {
            var u = await user.FindByIdAsync(UserId);
            if (u != null)
            {
                var LoanDTOList = new List<LoanDTO>();
                var LoanForUser = unit.LoanRepo.GetAllWithBooks().Where(lu => lu.UserId == UserId);
                foreach (var l in LoanForUser)
                {
                    var LoanDTO = new LoanDTO();
                    LoanDTO.BookTitle = l.Book.Title;
                    LoanDTO.LoanDate = l.LoanDate;
                    LoanDTO.DueDate = l.DueDate;
                    LoanDTOList.Add(LoanDTO);
                }
                return new GeneralResponse { code = 200, data = LoanDTOList };
            }
            return BadRequest(new GeneralResponse { message = "user not exist" });
        }

        [Authorize(Roles = "Member")]
        [HttpGet("MyLoans")]
        public ActionResult<GeneralResponse> MyLoans()
        {
            var u = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (u != null)
            {
                var LoanForUser = unit.LoanRepo.GetAllWithBooks().Where(lu => lu.UserId == u && lu.IsReturned == false);
                var LoanDTOList = new List<LoanDTO>();
                foreach (var l in LoanForUser)
                {
                    var LoanDTO = new LoanDTO();
                    LoanDTO.BookTitle= l.Book.Title;
                    LoanDTO.LoanDate = l.LoanDate;
                    LoanDTO.DueDate= l.DueDate;
                    LoanDTOList.Add(LoanDTO);
                }
                return new GeneralResponse { code = 200, data = LoanDTOList };
            }
            return BadRequest(new GeneralResponse { message = "user not exist" });
        }

        [Authorize(Roles = "Admin,Member")]
        [HttpGet("MostLoans")]
        public ActionResult<GeneralResponse> MostLoans()
        {
            var l = unit.LoanRepo.GetAll().Where(l => l.IsReturned == false)
                .GroupBy(l => l.BookId)
                .Select(Group => new
                {
                    BookId = Group.Key,
                    Count = Group.Count(),
                })
                .OrderByDescending( res => res.Count)
                .Take(5).ToList();
            return new GeneralResponse { code = 200, data= l };
        }


        #endregion

        #region Reservation
        [Authorize(Roles = "Member")]
        [HttpPost("CreateReservation")]
        public ActionResult<GeneralResponse> CreateReservation(int BookId)
        {
            var b = unit.BookRepo.GetByFilter(b => b.Id == BookId && b.IsDeleted == false);
            if(b != null)
            {
                if (b.AvaliableCopies == 0)
                {
                    var rDTO = new ReservationDTO
                    {
                        BookId = BookId,
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        ReservationDate = DateTime.Now,
                        IsActive = true,
                    };
                    var r = new Reservation 
                    {
                        BookId = rDTO.BookId,
                        UserId = rDTO.UserId,
                        ReservationDate = DateTime.Now,
                        IsActive = true,
                    };
                    unit.ReservationRepo.Add(r);
                    unit.Save();
                    return new GeneralResponse { code=200, data = rDTO };
                }
                else
                {
                    return BadRequest(new GeneralResponse { message = "We have avaliable copies, you can Loan this book now.." });
                }
            }
            return BadRequest(new GeneralResponse { message = "This Book Not Found!" });
        }


        [Authorize(Roles = "Member")]
        [HttpPost("CancelReservation")]
        public ActionResult<GeneralResponse> CancelReservation(int reservationId)
        {
            var r = unit.ReservationRepo.GetByFilter(r => r.Id == reservationId);
            if (r != null) 
            {
                r.IsActive = false;
                unit.ReservationRepo.Update(r);
                unit.Save();
                return new GeneralResponse { code = 200, message = "This reservation canceled Succsessfully..."};
            }
            return BadRequest(new GeneralResponse { message = "Invalid Code!"});
        }


        [Authorize(Roles = "Member")]
        [HttpGet("MyReservation")]
        public ActionResult<GeneralResponse> MyReservation()
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user != null) 
            {
                var r = unit.ReservationRepo.GetAllWithBooks();
                var rDTOList = new List<ReservationDTO>();
                foreach (var item in r)
                {
                    var rDTO = new ReservationDTO();
                    rDTO.BookId = item.BookId;
                    rDTO.UserId = item.UserId;
                    rDTO.ReservationDate = item.ReservationDate;
                    rDTO.IsActive = item.IsActive;
                    rDTO.title = item.Book.Title;
                    rDTOList.Add(rDTO);
                }
               return new GeneralResponse { code = 200, data = rDTOList };
            }
            return BadRequest(new GeneralResponse { message = "Please LogIn at First!" });
        }

        [Authorize(Roles = "Admin,Member")]
        [HttpPost("MostReservation")]
        public ActionResult<GeneralResponse> MostReservation()
        {
            var r = unit.ReservationRepo.GetAll().Where(r => r.IsActive == true)
                .GroupBy(r => r.BookId)
                .Select(group => new
                {
                    ReservationID = group.Key,
                    Count = group.Count(),
                }).OrderByDescending(r => r.Count)
                .Take(5).ToList();
            return new GeneralResponse { code = 200, data = r };
        }
        #endregion
    }
}
