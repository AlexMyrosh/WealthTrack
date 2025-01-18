﻿using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.DomainModels
{
    public class Transaction
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public TransactionType Type { get; set; }

        public Guid? CategoryId { get; set; }

        public Category Category { get; set; }

        public Guid WalletId { get; set; }

        public Wallet Wallet { get; set; }
    }
}