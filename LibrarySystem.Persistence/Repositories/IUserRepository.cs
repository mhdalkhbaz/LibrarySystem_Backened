namespace LibrarySystem.Persistence.Repositories
{
    public interface IUserRepository
    {
        Task<int> RegisterOrGetUserAsync(string userName);
    }
}
