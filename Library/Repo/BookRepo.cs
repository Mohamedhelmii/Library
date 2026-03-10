using Library.Model;
using Microsoft.EntityFrameworkCore;

namespace Library.Repo
{
    public class BookRepo : Repo<Book>, IBook
    {
        private readonly LibraryContext context;

        public BookRepo(LibraryContext context) : base(context)
        {
            this.context = context;
        }

        public void Update(Book book)
        {
            var oldBook = context.Books.FirstOrDefault(b => b.Id == book.Id);
            if (oldBook != null)
            {
                oldBook.Title = book.Title;
                oldBook.Description = book.Description;
                oldBook.AuthorBooks = book.AuthorBooks;
                oldBook.TotalCopies = book.TotalCopies;
                oldBook.AvaliableCopies = book.AvaliableCopies;
            }
        }
        public List<Book> GetBooksWithAuthor() 
        {
            return Context.Books.Include(b => b.AuthorBooks).ThenInclude(a=> a.Author).ToList();
        }

    }
}
