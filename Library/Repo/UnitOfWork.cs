using Library.Model;

namespace Library.Repo
{
    public class UnitOfWork
    {
        private LibraryContext libraryContext { get; set; }

        public UnitOfWork(LibraryContext libraryContext)
        {
            this.libraryContext = libraryContext;
            BookRepo = new BookRepo(libraryContext);
            AuthorRepo = new AuthorRepo(libraryContext);
            AuthorBookRepo = new AuthorBookRepo(libraryContext);
            LoanRepo = new LoanRepo(libraryContext);
            ReservationRepo = new ReservationRepo(libraryContext);
        }

        public BookRepo BookRepo { get; set; }
        public AuthorBookRepo AuthorBookRepo { get; set; }
        public AuthorRepo AuthorRepo { get; set; }
        public LoanRepo LoanRepo { get; set; }
        public ReservationRepo ReservationRepo { get; set; }

        public void Save()
        {
            libraryContext.SaveChanges();
        }
    }
}
