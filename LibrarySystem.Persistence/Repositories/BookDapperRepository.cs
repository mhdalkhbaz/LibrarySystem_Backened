using Dapper;
using LibrarySystem.Persistence.Models;
using LibrarySystem.Persistence.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class BookDapperRepository : IBookRepository
{
    private readonly string _connectionString;

    public BookDapperRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<Book>> GetAllBooksAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        var query = @"
            SELECT 
                b.Id, b.Title, b.Author, b.Category, b.ISBN, b.Rating, b.Cover, b.Description, b.Available,
                br.UserId
            FROM Books b
            LEFT JOIN Borrowings br ON b.Id = br.BookId AND br.ReturnDate IS NULL";

        var books = await connection.QueryAsync<Book>(query);
        return books.ToList();
    }

    public async Task<Book?> GetBookByIdAsync(int bookId)
    {
        using var connection = new SqlConnection(_connectionString);
        var query = "SELECT Id, Title, Available FROM Books WHERE Id = @bookId";
        return await connection.QuerySingleOrDefaultAsync<Book>(query, new { bookId });
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        var query = "SELECT COUNT(1) FROM Users WHERE Id = @userId";
        int count = await connection.ExecuteScalarAsync<int>(query, new { userId });
        return count > 0;
    }

    public async Task<bool> IsBookBorrowedByUserAsync(int bookId, int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        var query = @"SELECT COUNT(1) FROM Borrowings 
                      WHERE BookId = @bookId AND UserId = @userId AND ReturnDate IS NULL";
        int count = await connection.ExecuteScalarAsync<int>(query, new { bookId, userId });
        return count > 0;
    }

    public async Task BorrowOrReturnBookAsync(int bookId, bool isBorrowing, int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            if (isBorrowing)
            {
                var checkQuery = "SELECT Available FROM Books WHERE Id = @bookId";
                var available = await connection.ExecuteScalarAsync<bool>(checkQuery, new { bookId }, transaction);
                if (!available)
                    throw new ApplicationException("Book is already borrowed.");

                var updateBook = "UPDATE Books SET Available = 0 WHERE Id = @bookId";
                await connection.ExecuteAsync(updateBook, new { bookId }, transaction);

                var insert = @"INSERT INTO Borrowings (UserId, BookId, BorrowDate) 
                               VALUES (@userId, @bookId, GETDATE())";
                await connection.ExecuteAsync(insert, new { userId, bookId }, transaction);
            }
            else
            {
                var updateBook = "UPDATE Books SET Available = 1 WHERE Id = @bookId";
                await connection.ExecuteAsync(updateBook, new { bookId }, transaction);

                var updateBorrow = @"
                    UPDATE TOP (1) Borrowings 
                    SET ReturnDate = GETDATE()
                    WHERE BookId = @bookId AND UserId = @userId AND ReturnDate IS NULL";
                await connection.ExecuteAsync(updateBorrow, new { bookId, userId }, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
