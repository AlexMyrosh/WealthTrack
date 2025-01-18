﻿using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.DomainModels
{
    public class Wallet
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Balance { get; set; }

        public bool IsPartOfGeneralBalance { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }

        public WalletStatus Status { get; set; }

        public WalletType Type { get; set; }

        public Guid CurrencyId { get; set; }

        public virtual Currency Currency { get; set; }

        public virtual List<Transaction> Transactions { get; set; }
    }
}
