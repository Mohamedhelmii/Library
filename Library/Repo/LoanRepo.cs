using Library.Model;
using Microsoft.EntityFrameworkCore;

namespace Library.Repo
{
    public class LoanRepo : Repo<Loan>, ILoan
    {
        private readonly LibraryContext context;

        public LoanRepo(LibraryContext context) : base(context)
        {
            this.context = context;
        }

        public void Update(Loan loan)
        {
            var oldLoan = context.Loans.FirstOrDefault(l => l.Id == loan.Id);
            if (oldLoan != null) 
            {
                oldLoan.LoanDate = loan.LoanDate;
                oldLoan.DueDate = loan.DueDate;
                oldLoan.ReturnDate = loan.ReturnDate;
                oldLoan.IsReturned = loan.IsReturned;
                oldLoan.UserId = loan.UserId;
                oldLoan.BookId = loan.BookId;
            }
        }

        public Loan GetLoanWithBook(Func<Loan, bool> get)
        {
            var l = context.Loans.Include(l => l.Book).Where(get).FirstOrDefault();
            return l;
        }

        public List<Loan> GetAllWithBooks()
        {
            var LB = context.Loans.Include(l => l.Book);
            return LB.ToList();
        }

        public List<Loan> GetAllWithBooksAndUser()
        {
            var LB = context.Loans.Include(l => l.User).Include(l => l.Book);
            return LB.ToList();
        }
    }
}
