using LibrarySystem.Application.Interfaces;
using LibrarySystem.Persistence.Repositories;

namespace LibrarySystem.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }


        public async Task<int> RegisterUserAsync(string userName)
        {
            return await _repository.RegisterOrGetUserAsync(userName);
        }
    }
}
