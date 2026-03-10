using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Model
{
    public class AuthorBook
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int AuthorId { get; set; }

        [ForeignKey(nameof(BookId))]
        public virtual Book Book { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public virtual Author Author { get; set; }

    }
}
