namespace Library.Model
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TotalCopies { get; set; }
        public int AvaliableCopies { get; set; }
        public bool IsDeleted { get; set; } = true;
        public string Category { get; set; }
        //public int AuthorId { get; set; }
        public virtual ICollection<Loan> Loans { get; set; } = new HashSet<Loan>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new HashSet<Reservation>();
        public virtual ICollection<AuthorBook> AuthorBooks { get; set; } = new HashSet<AuthorBook>();

    }
}
