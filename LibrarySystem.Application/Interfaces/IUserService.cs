namespace LibrarySystem.Application.Interfaces
{
    public interface IUserService
    {
        Task<int> RegisterUserAsync(string userName);
    }
}
