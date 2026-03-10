using Library.Model;

namespace Library.Repo
{
    public interface ILoan
    {
        public void Update(Loan loan);
        public Loan GetLoanWithBook(Func<Loan,bool>get);
        public List<Loan> GetAllWithBooks();
        public List<Loan> GetAllWithBooksAndUser();

    }
}
