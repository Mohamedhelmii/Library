using Library.Model;

namespace Library.Repo
{
    public interface IAuthor
    {
        public void Update (Author author);
        public List<Author> GetAllDetails ();
        public List<Author> GetAllAuthorwITHDetails(bool IsDeleted = false);
    }
}
