using LibrarySystem.Persistence.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LibrarySystem.Persistence.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly string _connectionString;

        public BookRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            var books = new List<Book>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
            SELECT 
                b.Id, b.Title, b.Author, b.Category, b.ISBN, b.Rating, b.Cover, b.Description, b.Available,
                br.UserId
            FROM Books b
            LEFT  JOIN Borrowings br ON b.Id = br.BookId and br.ReturnDate IS NULL";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                books.Add(new Book
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Author = reader.GetString(2),
                    Category = reader.GetString(3),
                    ISBN = reader.GetString(4),
                    Rating = reader.GetDouble(5),
                    Cover = reader.GetString(6),
                    Description = reader.GetString(7),
                    Available = reader.GetBoolean(8),
                    UserId = reader.IsDBNull(9) ? null : reader.GetInt32(9)
                });
            }

            return books;
        }



        public async Task BorrowOrReturnBookAsync(int bookId, bool isBorrowing, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            SqlTransaction transaction = connection.BeginTransaction();
            command.Transaction = transaction;

            try
            {
                if (isBorrowing)
                {
                    command.CommandText = @"
                IF EXISTS (SELECT 1 FROM Books WHERE Id = @bookId AND Available = 0)
                    THROW 50001, 'الكتاب مستعار بالفعل.', 1;

                UPDATE Books SET Available = 0 WHERE Id = @bookId;

                INSERT INTO Borrowings (UserId, BookId, BorrowDate)
                VALUES (@userId, @bookId, GETDATE());
            ";
                }
                else
                {
                    command.CommandText = @"
                UPDATE Books SET Available = 1 WHERE Id = @bookId;

                UPDATE TOP (1) Borrowings
                SET ReturnDate = GETDATE()
                WHERE BookId = @bookId AND UserId = @userId AND ReturnDate IS NULL; ";

                }

                command.Parameters.AddWithValue("@bookId", bookId);
                command.Parameters.AddWithValue("@userId", userId);

                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ApplicationException(ex.Message ?? "حدث خطأ أثناء استعارة أو إرجاع الكتاب");
            }
        }

        public async Task<Book?> GetBookByIdAsync(int bookId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT Id, Title, Available FROM Books WHERE Id = @bookId", connection);
            command.Parameters.AddWithValue("@bookId", bookId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Book
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Available = reader.GetBoolean(2)
                };
            }
            return null;
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Id = @userId", connection);
            command.Parameters.AddWithValue("@userId", userId);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> IsBookBorrowedByUserAsync(int bookId, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(
                @"SELECT COUNT(1) FROM Borrowings WHERE BookId = @bookId AND UserId = @userId AND ReturnDate IS NULL",
                connection);
            command.Parameters.AddWithValue("@bookId", bookId);
            command.Parameters.AddWithValue("@userId", userId);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }


    }
}
