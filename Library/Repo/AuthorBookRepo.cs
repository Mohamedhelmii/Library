using Library.Model;

namespace Library.Repo
{
    public class AuthorBookRepo : Repo<AuthorBook>, IAuthorBook
    {
        private readonly LibraryContext context;

        public AuthorBookRepo(LibraryContext context) : base(context)
        {
            this.context = context;
        }

        public void Update(AuthorBook authorBook)
        {
            
        }
    }
}
