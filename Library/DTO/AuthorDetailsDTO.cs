using Library.Model;

namespace Library.DTO
{
    public class AuthorDetailsDTO
    {
        public string AuthorName { get; set; }
        public List<BDTO> Books { get; set; } = new List<BDTO>();
    }
    public class BDTO
    {
        public string Title { get; set; }
    }
}
