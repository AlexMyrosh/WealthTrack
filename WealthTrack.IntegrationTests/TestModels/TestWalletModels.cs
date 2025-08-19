using WealthTrack.API.ApiModels.Wallet;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Helpers
{
    public static class TestWalletModels
    {
        public static readonly Guid FirstWalletId = new("d8f37d0f-12f3-4998-bf1a-7b563a6baa3a");
        public static readonly Guid SecondWalletId = new("bee8548b-e126-40a9-a13e-dbb7f311e968");

        public static WalletUpsertApiModel UpsertApiModel
        {
            get
            {
                return new WalletUpsertApiModel
                {
                    Name = Guid.NewGuid().ToString(),
                    Balance = 0,
                    IsPartOfGeneralBalance = true,
                    Type = WalletType.CreditCard
                };
            }
        }

        public static Wallet FirstDomainModelWithoutDetails
        {
            get
            {
                return new Wallet
                {
                    Id = FirstWalletId,
                    Name = "Test wallet name 1",
                    Balance = 0,
                    IsPartOfGeneralBalance = true,
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    ModifiedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    Status = WalletStatus.Active,
                    Type = WalletType.Cash,
                };
            }
        }

        public static Wallet SecondDomainModelWithoutDetails
        {
            get
            {
                return new Wallet
                {
                    Id = SecondWalletId,
                    Name = "Test wallet name 2",
                    Balance = 0,
                    IsPartOfGeneralBalance = true,
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    ModifiedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    Status = WalletStatus.Active,
                    Type = WalletType.CreditCard,
                };
            }
        }
    }
}
