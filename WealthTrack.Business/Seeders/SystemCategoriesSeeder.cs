using Microsoft.Extensions.Configuration;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Seeders
{
    public class SystemCategoriesSeeder(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        private readonly string _balanceCorrectionId = configuration["SystemCategories:BalanceCorrectionId"] ?? throw new InvalidOperationException("Unable to get balance correction category id from configuration");

        public async Task SeedAsync()
        {
            var existedCategories = await unitOfWork.CategoryRepository.GetAllSystemOwnedAsync();
            var predefinedCurrencies = new List<Category>
            {
                new()
                {
                    Id = new Guid(_balanceCorrectionId),
                    Name = "Balance correction",
                    IconName = "BalanceCorrection",
                    IsSystem = true,
                    CreatedDate = DateTimeOffset.Now,
                    ModifiedDate = DateTimeOffset.Now,
                }
            };

            foreach (var category in predefinedCurrencies)
            {
                if (!existedCategories.Any(c => c.Equals(category)))
                {
                    await unitOfWork.CategoryRepository.CreateAsync(category);
                }
            }

            await unitOfWork.SaveAsync();
        }
    }
}
