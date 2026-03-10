using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Model
{
    public class Reservation
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string UserId { get; set; }
        public DateTime ReservationDate { get; set; }
        public bool IsActive { get; set; } = false;

        [ForeignKey(nameof(BookId))]
        public virtual Book Book { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }
    }
}
