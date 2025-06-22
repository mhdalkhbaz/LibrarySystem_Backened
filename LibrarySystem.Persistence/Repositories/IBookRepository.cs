using LibrarySystem.Persistence.Models;

namespace LibrarySystem.Persistence.Repositories
{
    public interface IBookRepository
    {
        Task<List<Book>> GetAllBooksAsync();
        Task BorrowOrReturnBookAsync(int bookId, bool isBorrowing, int userId);

        Task<Book?> GetBookByIdAsync(int bookId);
        Task<bool> UserExistsAsync(int userId);
        Task<bool> IsBookBorrowedByUserAsync(int bookId, int userId);


    }
}
