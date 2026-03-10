namespace Library.DTO
{
    public class AuthorWithBookDTO
    {
        public string AuthorName { get; set; }
        public string? BioGraphy { get; set; }
        public string? nationality { get; set; }
        public List<AuthorBooksDTO> Books { get; set; } = new List<AuthorBooksDTO>();
    }

    public class AuthorBooksDTO
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int TotalCopies { get; set; }
        public int AvaliableCopies { get; set; }
        //public bool isDeleted { get; set; }
    }
}
