using System.Text.Json.Serialization;

namespace Library.DTO
{
    public class GlobalSearchDTO
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<BookSearchDTO> Books { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<AuthorSearchDTO> Authors { get; set; }
    }
    public class BookSearchDTO
    { 
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
    }
    public class AuthorSearchDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> BooksTitle { get; set; }
    }
}
