using Library.DTO;
using Library.Model;
using Library.Repo;
using Library.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly UnitOfWork unit;

        public AuthorController(UnitOfWork unit)
        {
            this.unit = unit;
        }

        [HttpGet("GetAllAuthorWithBooks")]
        public async Task<ActionResult<GeneralResponse>> GetAllAuthorWithBooks()
        {
            var a = unit.AuthorRepo.GetAllDetails();
            if (!User.IsInRole("Admin"))
            {
                a = a.Where(a => a.IsDeleted == false).ToList();
            }
            var authorsDTOs = a.Select(author => new AuthorDetailsDTO
                               {
                                 AuthorName = author.AuthorName,
                                 Books = author.AuthorBooks
                                 .Where(b => User.IsInRole("Admin") || b.Book.IsDeleted == false)
                                          .Select(ab => new BDTO
                                          {
                                              Title = ab.Book.Title
                                          })
                                          .ToList()
                               }).ToList();
            #region MY step
            //var a = unit.AuthorRepo.GetAllDetails();
            //var ADTO = new List<AuthorDetailsDTO>();
            //foreach (var author in a)
            //{
            //    var authorDTO = new AuthorDetailsDTO();
            //    authorDTO.AuthorName = author.AuthorName;
            //    foreach(var b in author.AuthorBooks)
            //    {
            //        var book = new BDTO
            //        {
            //            Title = b.Book.Title
            //        };
            //        authorDTO.Books.Add(book);

            //    }
            //    ADTO.Add(authorDTO);
            //}
            #endregion
            return new GeneralResponse
            {
                code = 200,
                data = authorsDTOs
            };

        }

        #region MyRegion
        [HttpPost("GetAuthorBySearch")]
        /*
         * if id string (GUID)
         * public ActionResult<GeneralResponse> GetAuthorBySearch(string searchValue)
        {
            var query = unit.AuthorRepo.GetAllDetails().Where(a => a.Id.ToString() == searchValue ||
                                     a.AuthorName.ToUpper().Contains(searchValue.ToUpper()));

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(a => a.IsDeleted == false);
            }

            var author = query.Select(data => new AuthorDetailsDTO
            {
                AuthorName = data.AuthorName,
                Books = data.AuthorBooks
                            .Where(b => User.IsInRole("Admin") || b.Book.IsDeleted == false)
                            .Select(b => new BDTO { Title = b.Book.Title })
                            .ToList()
            }).FirstOrDefault();

            if (author != null)
                return new GeneralResponse { code = 200, data = author };

            return NotFound(new GeneralResponse { message = "Author Not Found!" });
        }
        */
        [HttpPost("GetAuthorByIdWithBooks")]
        public ActionResult<GeneralResponse> GetAuthorByIdWithBooks(int id)
        {
            var a = unit.AuthorRepo.GetAllDetails().Where(a => a.Id == id);
            if (!User.IsInRole("Admin"))
            {
                a = a.Where(a => a.IsDeleted == false);
            }
            var author = a.Select(data => new AuthorDetailsDTO
            {
                AuthorName = data.AuthorName,
                Books = data.AuthorBooks.Where(b => User.IsInRole("Admin") || b.Book.IsDeleted == false)
                        .Select(b => new BDTO
                        {
                            Title = b.Book.Title,
                        }).ToList()
            }).FirstOrDefault();
            if (a != null)
            {
                return new GeneralResponse { code = 200, data = author };
            }
            return BadRequest(new GeneralResponse { message = "Not Found!" });
        }

        [HttpPost("GetAuthorByNameWithBooks")]
        public ActionResult<GeneralResponse> GetAythorByNameWithBooks(string name)
        {
            var a = unit.AuthorRepo.GetAllDetails().Where(a => a.AuthorName.ToUpper() == name.ToUpper());
            if (!User.IsInRole("Admin"))
            {
                a = a.Where(a => a.IsDeleted == false);
            }
            var author = a.Select(data => new AuthorDetailsDTO
            {
                AuthorName = data.AuthorName,
                Books = data.AuthorBooks.Where(b => User.IsInRole("Admin") || b.Book.IsDeleted == false)
                        .Select(b => new BDTO
                        {
                            Title = b.Book.Title,
                        }).ToList()
            }).FirstOrDefault();
            if (a != null)
            {
                return new GeneralResponse { code = 200, data = author };
            }
            return BadRequest(new GeneralResponse { message = "Not Found!" });
        }
        #endregion

        #region Author With admin only
        [Authorize(Roles = "Admin")]
        [HttpPost("AddAuthorBasicInfo")]
        public ActionResult<GeneralResponse> AddAuthorBasicInfo(AuthorDTO aDTO)
        {
            var exist = unit.AuthorRepo.GetByFilter(a => a.AuthorName == aDTO.name);
            if (exist == null)
            {
                var a = new Author
                {
                    AuthorName = aDTO.name,
                    BioGraphy = aDTO.BioGraphy,
                    nationality = aDTO.nationality,
                };
                if (ModelState.IsValid)
                {
                    unit.AuthorRepo.Add(a);
                    unit.Save();
                    return new GeneralResponse { code = 200, message = "Added Succsessfully...", data = aDTO.name };
                }
                return BadRequest(new GeneralResponse { message = "Invalid Data!" });
            }
            return BadRequest(new GeneralResponse { message = "Already Exist!" });
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("AddAuthorWithBook")]
        public ActionResult<GeneralResponse> AddAuthorWithBook(AuthorWithBookDTO abDTO)
        {
            var exist = unit.AuthorRepo.GetByFilter(a => a.AuthorName == abDTO.AuthorName && a.BioGraphy == abDTO.BioGraphy);
            if (exist == null)
            {
                var ab = new Author
                {
                    AuthorName = abDTO.AuthorName,
                    BioGraphy = abDTO.BioGraphy,
                    nationality = abDTO.nationality,
                };

                var AuthorBookss = new List<AuthorBook>();
                foreach (var bb in abDTO.Books)
                {
                    var b = new Book();
                    b.Title = bb.Title;
                    b.Description = bb.Description;
                    b.Category = bb.Category;
                    b.TotalCopies = bb.TotalCopies;
                    b.AvaliableCopies = bb.AvaliableCopies;
                    var authorbook = new AuthorBook
                    {
                        Author = ab,
                        Book = b,
                    };
                    AuthorBookss.Add(authorbook);
                }

                unit.AuthorRepo.Add(ab);
                //The AddRAnge method is used in the repository to add a list of models
                unit.AuthorBookRepo.AddRAnge(AuthorBookss);
                unit.Save();
                return new GeneralResponse { code = 200, message = "Author And Books Addedsuccsessfully.." };
            }
            return BadRequest(new GeneralResponse { message = "Can't add this author" });
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateAuthorBasicInfo")]
        public ActionResult<GeneralResponse> UpdateAuthorBasicInfo(int id, AuthorDTO aDTO)
        {
            var a = unit.AuthorRepo.GetByFilter(a => a.Id == id);
            if (a != null)
            {
                a.AuthorName = aDTO.name;
                a.BioGraphy = aDTO.BioGraphy;
                a.nationality = aDTO.nationality;
                a.IsDeleted = a.IsDeleted;
                if (ModelState.IsValid)
                {
                    unit.AuthorRepo.Update(a);
                    unit.Save();
                    return new GeneralResponse { code = 200, message = "Updated Succsessfully....", data = aDTO.name };
                }
                return BadRequest(new GeneralResponse { message = "Invalid Data!" });
            }
            return BadRequest(new GeneralResponse { message = "Author not found!!" });
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateAuthorWithBook")]
        public ActionResult<GeneralResponse> UpdateAuthorWithBook(int id, AuthorWithBookDTO abDTOFromDB)
        {
            var a = unit.AuthorRepo.GetAllDetails().Where(a => a.Id == id).FirstOrDefault();
            if (a != null)
            {
                a.AuthorName = abDTOFromDB.AuthorName;
                a.BioGraphy = abDTOFromDB.BioGraphy;
                a.nationality = abDTOFromDB.nationality;
                foreach (var b in abDTOFromDB.Books)
                {
                    var book = unit.BookRepo.GetByFilter(bo => bo.Id == b.Id);
                    if (book != null)
                    {
                        book.Title = b.Title;
                        book.Description = b.Description;
                        book.Category = b.Category;
                        book.TotalCopies = b.TotalCopies;
                        book.AvaliableCopies = b.AvaliableCopies;
                    }
                    else
                    {
                        var newbook = new Book();
                        newbook.Title = b.Title;
                        newbook.Description = b.Description;
                        newbook.Category = b.Category;
                        newbook.TotalCopies = b.TotalCopies;
                        newbook.AvaliableCopies = b.AvaliableCopies;
                        var authorbook = new AuthorBook
                        {
                            Author = a,
                            Book = newbook
                        };
                        unit.AuthorBookRepo.Add(authorbook);
                    }
                    ;

                }
                unit.AuthorRepo.Update(a);
                unit.Save();
                return new GeneralResponse { code = 200, message = "Updated Succsessfully..", data = a.AuthorName };
            }
            return BadRequest(new GeneralResponse { message = "Can't Find this author" });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteAuthor")]
        public ActionResult<GeneralResponse> DeleteAuthor(int id)
        {
            var a = unit.AuthorRepo.GetAllDetails().Where(a => a.Id == id && a.IsDeleted == false).FirstOrDefault();
            if (a != null)
            {
                a.IsDeleted = true;
                foreach (var b in a.AuthorBooks)
                {
                    b.Book.IsDeleted = true;
                }
                unit.Save();
                return new GeneralResponse { code = 200, message = "Deleted Succsesfully" };
            }
            return BadRequest(new GeneralResponse { message = "Not Found!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("RestoreAuthor")]
        public ActionResult<GeneralResponse> RestoreAuthor(int id)
        {
            var a = unit.AuthorRepo.GetByFilter(a => a.Id == id);
            if (a == null)
            {
            return BadRequest(new GeneralResponse { message = "This Author Not Found!!" });
            }
            a.IsDeleted = false;
            unit.AuthorRepo.Update(a);
            unit.Save();
            return new GeneralResponse { code = 200, message = "Restored Succsessfully.."};
        }
        #endregion
        #region OldVersion  
        //[Authorize(Roles = "Admin,Member")]
        //[HttpGet("GetAllAuthor")]
        //public ActionResult<GeneralResponse> GetAllAuthor()
        //{
        //    var a = unit.AuthorRepo.GetAll();
        //    if(!User.IsInRole("Admin"))
        //    {
        //        a = a.Where(a=>a.IsDeleted ==  false).ToList();
        //    }
        //    if (a != null)
        //    {
        //        return new GeneralResponse { code = 200, message = "OK", data = a };
        //    }
        //     return BadRequest(new GeneralResponse { message = "Not Found!"});
        //}


        //[Authorize(Roles = "Admin")]
        //[HttpGet("GetAllAuthorWithBooks")]
        //public ActionResult<GeneralResponse> GetAllAuthorWithBooks()
        //{
        //    var authorsDTOs = unit.AuthorRepo.GetAllDetails()
        //             .Select(author => new AuthorDetailsDTO
        //             {
        //                 AuthorName = author.AuthorName,
        //                 Books = author.AuthorBooks
        //                     .Select(ab => new BDTO
        //                     {
        //                         Title = ab.Book.Title
        //                     })
        //                     .ToList()
        //             }).ToList();
        //    #region MY step
        //    //var a = unit.AuthorRepo.GetAllDetails();
        //    //var ADTO = new List<AuthorDetailsDTO>();
        //    //foreach (var author in a)
        //    //{
        //    //    var authorDTO = new AuthorDetailsDTO();
        //    //    authorDTO.AuthorName = author.AuthorName;
        //    //    foreach(var b in author.AuthorBooks)
        //    //    {
        //    //        var book = new BDTO
        //    //        {
        //    //            Title = b.Book.Title
        //    //        };
        //    //        authorDTO.Books.Add(book);

        //    //    }
        //    //    ADTO.Add(authorDTO);
        //    //}
        //    #endregion
        //    return new GeneralResponse
        //    {
        //        code = 200,
        //        data = authorsDTOs
        //    };

        //}
        //[Authorize(Roles = "Admin")]
        //[HttpPost("GetAuthorById")]
        //public ActionResult<GeneralResponse> GetAuthorById(int id)
        //{
        //    var a = unit.AuthorRepo.GetByFilter(a => a.Id == id);
        //    if (a != null)
        //    {
        //        return new GeneralResponse { code = 200, data = a };
        //    }
        //    return BadRequest(new GeneralResponse { message = "Not Found!" });
        //}



        //[Authorize(Roles = "Admin")]
        //[HttpPost("GetAuthorByName")]
        //public ActionResult<GeneralResponse> GetAythorByName(string name)
        //{
        //    var a = unit.AuthorRepo.GetByFilter(a => a.AuthorName.ToUpper() == name.ToUpper());
        //    if (a != null)
        //    {
        //        return new GeneralResponse { code = 200, data = a };
        //    }
        //    return BadRequest(new GeneralResponse { message = "NotFound" });
        //}

        //[HttpGet("GetAllAuthor")]
        //public ActionResult<GeneralResponse> GetAllAuthor()
        //{
        //    var a = unit.AuthorRepo.GetAll();
        //    if (!User.IsInRole("Admin"))
        //    {
        //        a = a.Where(a => a.IsDeleted == false).ToList();
        //    }
        //    if (a != null)
        //    {
        //        return new GeneralResponse { code = 200, message = "OK", data = a };
        //    }
        //    return BadRequest(new GeneralResponse { message = "Not Found!" });
        //}
        #endregion
    }
}
