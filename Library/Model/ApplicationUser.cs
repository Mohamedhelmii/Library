using Microsoft.AspNetCore.Identity;

namespace Library.Model
{
    public class ApplicationUser:IdentityUser
    {
        public string? Image {  get; set; }
        public bool isDeleted { get; set; } = false;
        public virtual ICollection<Loan> Loans { get; set; } = new HashSet<Loan>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new HashSet<Reservation>();
    }
}
