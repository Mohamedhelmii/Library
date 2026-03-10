using Library.DTO;
using Library.Model;
using Library.Repo;
using Library.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly UnitOfWork unit;

        public BookController(UnitOfWork unit)
        {
            this.unit = unit;
        }


        [HttpGet("GetAllBookWithAuthorName")]
        public ActionResult<GeneralResponse> GetAllBookWithAuthorName()
        {
            var b = unit.BookRepo.GetBooksWithAuthor();
            if(!User.IsInRole("Admin"))
            {
               b = b.Where(b => b.IsDeleted == false).ToList();
            }
            
            var b2 =  b.Select(Item => new BookDTO
                       {
                           Title = Item.Title,
                           Description = Item.Description,
                           Category = Item.Category,
                           TotalCopies = Item.TotalCopies,
                           AvaliableCopies = Item.AvaliableCopies,
                           AuthorName = Item.AuthorBooks.FirstOrDefault().Author.AuthorName
                       });
                #region C#STEP
            /*
            var newbooks = new List<BookDTO>();
            foreach (var item in b)
            {
                var newbook = new BookDTO();
                newbook.Title = item.Title;
                newbook.Description = item.Description;
                newbook.Category = item.Category;
                newbook.TotalCopies = item.TotalCopies;
                newbook.AvaliableCopies = item.AvaliableCopies;
                foreach (var item2 in item.AuthorBooks)
                {
                    newbook.AuthorName = item2.Author.AuthorName;
                }
                newbooks.Add(newbook);
            }*/
            #endregion
            return new GeneralResponse { code = 200, message = "OK", data = b2 };
        }


        [HttpPost("GetBookByIdWIthAuthorName")]
        public ActionResult<GeneralResponse> GetBookByIdWithAuthorName(int id)
        {
            var b = unit.BookRepo.GetBooksWithAuthor().Where(b => b.Id == id);
            if(!User.IsInRole("Admin"))
            {
                b= b.Where(b => b.IsDeleted == false).ToList();
            }
            var b2 = b.Select(item => new BookDTO
            {
                Title = item.Title,
                Description = item.Description,
                Category = item.Category,
                TotalCopies = item.TotalCopies,
                AvaliableCopies = item.AvaliableCopies,
                AuthorName = item.AuthorBooks.FirstOrDefault()?.Author.AuthorName
            });
            return new GeneralResponse { code = 200, data = b2, message = "OK" };
        }


        [HttpPost("GetBookByNameWIthAuthorName")]
        public ActionResult<GeneralResponse> GetBookByNameWithAuthorName(string Title)
        {
            var b = unit.BookRepo.GetBooksWithAuthor().Where(b => b.Title.ToLower() == Title.ToLower());
            if (!User.IsInRole("Admin"))
            {
                b = b.Where(b => b.IsDeleted == false).ToList();
            }
            var b2 = b.Select(item => new BookDTO
            {
                Title = item.Title,
                Description = item.Description,
                Category = item.Category,
                TotalCopies = item.TotalCopies,
                AvaliableCopies = item.AvaliableCopies,
                AuthorName = item.AuthorBooks.FirstOrDefault()?.Author.AuthorName
            });
            return new GeneralResponse { code = 200, data = b2, message = "OK" };
        }


        #region Book With Admin Only
        [Authorize(Roles = "Admin")]
        [HttpPost("AddBook")]
        public ActionResult<GeneralResponse> AddBook(int authorID, BookDTO bDTO)
        {
            var a = unit.AuthorRepo.GetByFilter(a => a.Id == authorID);
            if (a != null)
            {
                var exist = unit.BookRepo.GetByFilter(b => b.Title.ToUpper() == bDTO.Title.ToUpper());
                if (exist == null)
                {
                    var b = new Book
                    {
                        Title = bDTO.Title,
                        Description = bDTO.Description,
                        Category = bDTO.Category.ToUpper(),
                        TotalCopies = bDTO.TotalCopies,
                        AvaliableCopies = bDTO.AvaliableCopies,
                    };
                    if (ModelState.IsValid)
                    {
                        unit.BookRepo.Add(b);
                        unit.Save();
                        var ab = new AuthorBook
                        {
                            BookId = b.Id,
                            AuthorId = authorID,
                        };
                        unit.AuthorBookRepo.Add(ab);
                        unit.Save();
                        return new GeneralResponse { code = 200, message = "Added Succsessfully...", data = new { bDTO.Title, ab.AuthorId } };
                    }
                    return BadRequest(new GeneralResponse { message = "Invalid Data!" });
                }
                return BadRequest(new GeneralResponse { message = "Already Exist!" });
            }
            return BadRequest(new GeneralResponse { message = "Author not Found,It is not possible to add a book without an author" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateBookBasicInfo")]
        public ActionResult<GeneralResponse> UpdateBookBasicInfo(int id, BookDTO bDTO)
        {
            var b = unit.BookRepo.GetByFilter(b => b.Id == id);
            if (b != null)
            {
                b.Title = bDTO.Title;
                b.Description = bDTO.Description;
                b.Category = bDTO.Category.ToUpper();
                b.TotalCopies = bDTO.TotalCopies;
                b.AvaliableCopies = bDTO.AvaliableCopies;
                b.IsDeleted = b.IsDeleted;
                if (ModelState.IsValid)
                {
                    unit.BookRepo.Update(b);
                    unit.Save();
                    return new GeneralResponse { code = 200, message = "Updated Succsessfully....", data = bDTO.Title };
                }
                return BadRequest(new GeneralResponse { message = "Invalid Data!" });
            }
            return BadRequest(new GeneralResponse { message = "Book not found!!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteBook")]
        public ActionResult<GeneralResponse> DeleteBook(int id)
        {
            var b = unit.BookRepo.GetByFilter(b => b.Id == id && b.IsDeleted == false);
            if (b != null)
            {
                b.IsDeleted = true;
                unit.Save();
                return new GeneralResponse { code = 200, message = "Deleted Succsesfully" };
            }
            return BadRequest(new GeneralResponse { message = "Not Found!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("RestoreBook")]
        public ActionResult<GeneralResponse> RestoreBook(int id) 
        {
            var b = unit.BookRepo.GetByFilter(b => b.Id == id);
            if (b == null)
            {
            return BadRequest(new GeneralResponse { message = "This Book Not Found!!"});
            }
            b.IsDeleted = false;
            unit.BookRepo.Update(b);
            unit.Save();
            return new GeneralResponse { code = 200, message = "Restored Succsessfully.."};
        }
        #endregion
        #region Old Version
        //[Authorize(Roles = "Admin")]
        //[HttpGet("GetAllBookWithAuthorName")]
        //public ActionResult<GeneralResponse> GetAllBookWithAuthorName()
        //{
        //    var b = unit.BookRepo.GetBooksWithAuthor()
        //        .Select(Item => new BookDTO
        //        {
        //            Title = Item.Title,
        //            Description = Item.Description,
        //            Category = Item.Category,
        //            TotalCopies = Item.TotalCopies,
        //            AvaliableCopies = Item.AvaliableCopies,
        //            AuthorName = Item.AuthorBooks.FirstOrDefault().Author.AuthorName
        //        });
        //    #region C#STEP
        //    /*
        //    var newbooks = new List<BookDTO>();
        //    foreach (var item in b)
        //    {
        //        var newbook = new BookDTO();
        //        newbook.Title = item.Title;
        //        newbook.Description = item.Description;
        //        newbook.Category = item.Category;
        //        newbook.TotalCopies = item.TotalCopies;
        //        newbook.AvaliableCopies = item.AvaliableCopies;
        //        foreach (var item2 in item.AuthorBooks)
        //        {
        //            newbook.AuthorName = item2.Author.AuthorName;
        //        }
        //        newbooks.Add(newbook);
        //    }*/
        //    #endregion
        //    return new GeneralResponse { code = 200, message = "OK", data = b };
        //[Authorize(Roles = "Admin")]
        //[HttpGet("GetAllBook")]
        //public ActionResult<GeneralResponse> GetAllBook()
        //{
        //    var b = unit.BookRepo.GetAll();
        //    if (b != null)
        //    {
        //        return new GeneralResponse { code = 200, message = "OK", data = b };
        //    }
        //    return BadRequest(new GeneralResponse { message = "Not Found!" });
        //}

        //[Authorize(Roles = "Admin")]
        //[HttpPost("GetBookById")]
        //public ActionResult<GeneralResponse> GetBookById(int id)
        //{
        //    var b = unit.BookRepo.GetByFilter(a => a.Id == id);
        //    if (b != null)
        //    {
        //        return new GeneralResponse { code = 200, data = b };
        //    }
        //    return BadRequest(new GeneralResponse { message = "Not Found!" });
        //}

        //[Authorize(Roles = "Admin")]
        //[HttpPost("GetBookByName")]
        //public ActionResult<GeneralResponse> GetBookByName(string title) =>
        //      new GeneralResponse
        //      { code = 200, data = unit.BookRepo.GetByFilter(b => b.Title.ToUpper() == title.ToUpper()), message = "OK" };
        //}


        //[HttpPost("GetBookByName")]
        //public ActionResult<GeneralResponse> GetBookByName(string title)
        //{
        //    var b = unit.BookRepo.GetAll().Where(b => b.Title.ToUpper() == title.ToUpper());
        //    if (!User.IsInRole("Admin"))
        //    {
        //        b = b.Where(b => b.IsDeleted == false).ToList();
        //    }
        //    return new GeneralResponse
        //    { code = 200, data = b, message = "OK" };
        //}

        //[HttpPost("GetBooksByCategory")]
        //public ActionResult<GeneralResponse> GetBooksByCategory(string category)
        //{
        //    var b = unit.BookRepo.GetAll().Where(b => b.Category.ToUpper() == category.ToUpper());
        //    if (!User.IsInRole("Admin"))
        //    {
        //        b = b.Where(b => b.IsDeleted == false).ToList();
        //    }
        //    return new GeneralResponse
        //    { code = 200, data = b, message = "OK" };
        //}

        //[HttpPost("GetBookById")]
        //public ActionResult<GeneralResponse> GetBookById(int id)
        //{
        //    var b = unit.BookRepo.GetAll().Where(b => b.Id == id);
        //    if (!User.IsInRole("Admin"))
        //    {
        //        b = b.Where(b => b.IsDeleted == false).ToList();
        //    }
        //    return new GeneralResponse
        //    { code = 200, data = b, message = "OK" };
        //}

        //[HttpGet("GetAllBooks")]
        //public ActionResult<GeneralResponse> GetAllBooks()
        //{
        //    var b = unit.BookRepo.GetAll();
        //    if (!User.IsInRole("Admin"))
        //    {
        //        b = b.Where(b => b.IsDeleted == false).ToList();
        //    }
        //    return new GeneralResponse
        //    { code = 200, data = b, message = "OK" };
        //}
        #endregion
    }
}
