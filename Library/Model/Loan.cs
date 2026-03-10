using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Model
{
    public class Loan
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string UserId { get; set; }
        public DateTime LoanDate { get; set; }= DateTime.Now;
        public DateTime DueDate {  get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsReturned { get; set; } = false;

        [ForeignKey(nameof(BookId))]
        public virtual Book Book { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }
    }
}
