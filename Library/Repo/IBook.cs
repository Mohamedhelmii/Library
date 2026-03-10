using Library.Model;

namespace Library.Repo
{
    public interface IBook
    {
        public void Update(Book book);
        public List<Book> GetBooksWithAuthor();
    }
}
