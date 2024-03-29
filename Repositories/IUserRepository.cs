﻿using OnlineBingoAPI.Models;
using System.Threading.Tasks;

namespace OnlineBingoAPI.Repositories
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        public Task<User> GetByName(string username);
        public Task<User> Get(string username, string password);
    }
}
