using Library.Model;
using Microsoft.EntityFrameworkCore;

namespace Library.Repo
{
    public class ReservationRepo : Repo<Reservation>, IReservation
    {
        private readonly LibraryContext context;

        public ReservationRepo(LibraryContext context) : base(context)
        {
            this.context = context;
        }

        public void Update(Reservation reservation)
        {
            var oldReservation = context.Reservations.FirstOrDefault(r => r.Id == reservation.Id);
            if (oldReservation != null) 
            {
                oldReservation.ReservationDate = reservation.ReservationDate;
                oldReservation.UserId = reservation.UserId;
                oldReservation.BookId = reservation.BookId;
                oldReservation.IsActive = reservation.IsActive;
            }
        }

        public List<Reservation> GetAllWithBooks()
        {
            var r = context.Reservations.Include(r => r.Book).ToList();
            return r;
        }

    }
}
