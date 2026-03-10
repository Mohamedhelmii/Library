using Library.DTO;
using Library.Repo;
using Library.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly UnitOfWork unit;

        public SearchController(UnitOfWork unit)
        {
            this.unit = unit;
        }

        [HttpGet("GlobalSearch")]
        public ActionResult<GeneralResponse> GlobalSearch([FromQuery] string SearchTerm)
        {
            if(string.IsNullOrWhiteSpace(SearchTerm))
            {
                return Ok(new GeneralResponse { data =  new GlobalSearchDTO()});
            }
            var UpperCaseSearchTerm = SearchTerm.Trim().ToUpper();

            var bookSearch = unit.BookRepo.GetBooksWithAuthor()
                .Where(b => (b.Title.ToUpper().Contains(UpperCaseSearchTerm)
                || b.Description.ToUpper().Contains(UpperCaseSearchTerm)
                || b.Category.ToUpper().Contains(UpperCaseSearchTerm))
                && b.IsDeleted == false)
                .Select(b => new BookSearchDTO
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.AuthorBooks.FirstOrDefault().Author.AuthorName
                })
                .ToList();

            var authorSearch = unit.AuthorRepo.GetAll()
                .Where(a => (a.AuthorName.ToUpper().Contains(UpperCaseSearchTerm)
                || a.nationality.ToUpper().Contains(UpperCaseSearchTerm))
                && a.IsDeleted == false)
                .Select(a => new AuthorSearchDTO
                {
                    Id = a.Id,
                    Name = a.AuthorName,
                    BooksTitle = a.AuthorBooks.Where(ab =>ab.Book.IsDeleted == false).Select(ab => ab.Book.Title).ToList()
                })
                .ToList();

            List<BookSearchDTO> booksToReturn = bookSearch.Any() ? bookSearch : null;
            List<AuthorSearchDTO> AuthorToReturn = authorSearch.Any() ? authorSearch : null;
            var result = new GlobalSearchDTO
            {
                Books = booksToReturn,
                Authors = AuthorToReturn,
            };
            return new GeneralResponse { code = 200, data = result };   

        }
    }
}
