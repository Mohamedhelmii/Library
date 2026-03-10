using Library.Model;

namespace Library.Repo
{
    public interface IReservation
    {
        public void Update(Reservation reservation);
        public List<Reservation> GetAllWithBooks();
    }
}
