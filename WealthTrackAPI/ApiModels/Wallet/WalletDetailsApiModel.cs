using WealthTrack.Shared.Enums;

namespace WealthTrack.API.ApiModels.Wallet
{
    public class WalletDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Balance { get; set; }

        public bool IsPartOfGeneralBalance { get; set; }

        public EntityStatus Status { get; set; }

        public CurrencyRelatedToWalletDetailsApiModel Currency { get; set; }

        public List<TransactionRelatedToWalletDetailsApiModel> Transactions { get; set; }
        
        public List<TransactionRelatedToWalletDetailsApiModel> IncomeTransferTransactions { get; set; }

        public List<TransactionRelatedToWalletDetailsApiModel> OutgoingTransferTransactions { get; set; }
    }

    public class CurrencyRelatedToWalletDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public decimal ExchangeRate { get; set; }
        
        public CurrencyType Type { get; set; }
    }

    public class TransactionRelatedToWalletDetailsApiModel
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public DateTimeOffset TransactionDate { get; set; }

        public OperationType Type { get; set; }
        
        public EntityStatus Status { get; set; }
    }
}