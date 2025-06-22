using LibrarySystem.Application.Interfaces;
using LibrarySystem.Persistence.Models;
using LibrarySystem.Persistence.Repositories;

namespace LibrarySystem.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _repository;

        public BookService(IBookRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Book>> GetBooksAsync(int userId, string? searchKey)
        {
            var books = await _repository.GetAllBooksAsync();

            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.ToLower();
                books = books.Where(b =>
                    b.Title.ToLower().Contains(searchKey) ||
                    b.Author.ToLower().Contains(searchKey) ||
                    b.ISBN.ToLower().Contains(searchKey)
                ).ToList();
            }

            books.Where(b => b.UserId == userId).ToList()
                 .ForEach(b => b.BorrowedByThisUser = true);
            return books;
        }


        public async Task BorrowOrReturnBookAsync(int bookId, bool isBorrowing, int userId)
        {

            if (!await _repository.UserExistsAsync(userId))
                throw new ApplicationException("User not fount");

            var book = await _repository.GetBookByIdAsync(bookId);
            if (book is null)
                throw new ApplicationException("The book not found .");

            if (isBorrowing && !book.Available)
                throw new ApplicationException("The book is not availabel Now!.");

            if (!isBorrowing)
            {
                var isBorrowedByUser = await _repository.IsBookBorrowedByUserAsync(bookId, userId);
                if (!isBorrowedByUser)
                    throw new ApplicationException("Cannot return a book that was not borrowed by this user.");
            }

            await _repository.BorrowOrReturnBookAsync(bookId, isBorrowing, userId);

        }
    }

}
