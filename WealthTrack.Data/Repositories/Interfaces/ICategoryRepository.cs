﻿using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        public Task<Category> CreateAsync(Category model);

        public Task<Category?> GetByIdAsync(Guid id, string include = "");

        public Task<List<Category>> GetAllAsync(string include = "");

        public Category Update(Category model);

        public Task<Category?> HardDeleteAsync(Guid id);

        public Category HardDelete(Category model);
    }
}
