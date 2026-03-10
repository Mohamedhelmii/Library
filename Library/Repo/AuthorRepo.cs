using Library.Model;
using Microsoft.EntityFrameworkCore;

namespace Library.Repo
{
    public class AuthorRepo: Repo<Author>, IAuthor
    {
        private readonly LibraryContext context;

        public AuthorRepo(LibraryContext context) : base(context)
        {
            this.context = context;
        }

        public List<Author> GetAllDetails()
        {
            var a = context.Authors
                .Include(ab=> ab.AuthorBooks)
                .ThenInclude(b => b.Book)
                .ToList();
            return a;
        }

        public void Update(Author author)
        {
            var oldAuthor = context.Authors.FirstOrDefault(a => a.Id == author.Id);
            if (oldAuthor != null)
            {
                oldAuthor.AuthorName = author.AuthorName;
                oldAuthor.BioGraphy = author.BioGraphy;
                oldAuthor.nationality = author.nationality;
            }
        }

        public List<Author> GetAllAuthorwITHDetails(bool IsDeleted)
        {
            IQueryable<Author> a = Context.Authors.Include(ab => ab.AuthorBooks).ThenInclude(b => b.Book);
            if (IsDeleted == true)
            {
                a = a.Where(a=> a.IsDeleted == false);
            }
            return a.ToList();
        }

    }
}
