﻿namespace WealthTrack.API.ApiModels.Transaction
{
    public class TransferTransactionUpsertApiModel
    {
        public decimal? Amount { get; set; }

        public string? Description { get; set; }

        public DateTimeOffset? TransactionDate { get; set; }

        public Guid? SourceWalletId { get; set; }

        public Guid? TargetWalletId { get; set; }
    }
}
