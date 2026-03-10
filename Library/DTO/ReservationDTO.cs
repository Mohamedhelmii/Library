namespace Library.DTO
{
    public class ReservationDTO
    {
        public string? title {  get; set; }
        public int BookId { get; set; }
        public string UserId { get; set; }
        public DateTime ReservationDate { get; set; }
        public bool IsActive { get; set; }
    }
}
