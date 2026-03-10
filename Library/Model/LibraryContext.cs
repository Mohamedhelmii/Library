using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Library.Model
{
    public class LibraryContext:IdentityDbContext<ApplicationUser>
    {
        public LibraryContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<AuthorBook>().HasKey(ab => ab.Id);
            builder.Entity<AuthorBook>().HasOne(ab => ab.Book).WithMany(b => b.AuthorBooks).HasForeignKey(b => b.BookId);
            builder.Entity<AuthorBook>().HasOne(ab => ab.Author).WithMany(a => a.AuthorBooks).HasForeignKey(a => a.AuthorId);
        }

        public DbSet<AuthorBook> AuthorBooks { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
    }
}
