namespace Library.DTO
{
    public class BookDTO
    {
        public string? AuthorName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        //public int AuthorID { get; set; }
        public string Category { get; set; }
        public int TotalCopies { get; set; }
        [LessThanOrEqualTo("TotalCopies", ErrorMessage = "Available copies cannot be more than the total copies.")]
        public int AvaliableCopies { get; set; }
    }
}
