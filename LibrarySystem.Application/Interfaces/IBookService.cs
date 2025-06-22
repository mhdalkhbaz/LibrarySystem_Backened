using LibrarySystem.Persistence.Models;

namespace LibrarySystem.Application.Interfaces
{
    public interface IBookService
    {
        Task<List<Book>> GetBooksAsync(int userId, string? searchKey);
        Task BorrowOrReturnBookAsync(int bookId, bool isReturn, int userId);
    }
}
