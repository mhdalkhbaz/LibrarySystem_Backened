using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LibrarySystem.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> RegisterOrGetUserAsync(string userName)
        {
            var userId = await GetUserIdByUsernameAsync(userName);
            if (userId.HasValue)
            {
                return userId.Value;
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var query = @"
                        INSERT INTO Users (UserName) 
                        VALUES (@userName); 
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        using (var command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@userName", userName);

                            var result = await command.ExecuteScalarAsync();
                            transaction.Commit();
                            return Convert.ToInt32(result);
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw new Exception("Failed to register or retrieve user");
                    }
                }
            }
        }

        private async Task<int?> GetUserIdByUsernameAsync(string userName)
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                const string query = "SELECT Id FROM Users WHERE UserName = @userName";

                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@userName", userName.Trim());

                var result = await command.ExecuteScalarAsync();

                return result is not null and not DBNull ? (int)result : null;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
