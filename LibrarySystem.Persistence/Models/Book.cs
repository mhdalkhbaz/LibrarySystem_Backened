namespace LibrarySystem.Persistence.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Author { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string ISBN { get; set; } = default!;
        public double Rating { get; set; }
        public string Cover { get; set; } = default!;
        public string Description { get; set; } = default!;
        public bool Available { get; set; }
        public int? UserId { get; set; }
        public bool? BorrowedByThisUser { get; set; } = false;

    }

}
