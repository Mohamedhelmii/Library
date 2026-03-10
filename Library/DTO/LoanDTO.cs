using System.ComponentModel.DataAnnotations;

namespace Library.DTO
{
    public class LoanDTO
    {
        public string? UserName { get; set; }
        public string? BookTitle {  get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }

    }
}
