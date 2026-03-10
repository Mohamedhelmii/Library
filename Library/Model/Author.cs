using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Model
{
    public class Author
    {
        public int Id { get; set; }
        public string AuthorName { get; set; }
        public string? BioGraphy { get; set; }
        public string? nationality { get; set; }
        public bool IsDeleted { get; set; } = true;
        public virtual ICollection<AuthorBook> AuthorBooks { get; set; } = new HashSet<AuthorBook>();   
    }
}
