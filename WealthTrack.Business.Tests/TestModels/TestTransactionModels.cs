﻿using WealthTrack.Business.BusinessModels.Transaction;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Tests.TestModels
{
    public static class TestTransactionModels
    {
        public static Transaction TransactionDomainModel
        {
            get
            {
                var model = DomainModelWithoutDetails;
                model.Category = TestCategoryModels.DomainModelWithoutDetails;
                model.CategoryId = model.Category.Id;
                model.Wallet = TestWalletModels.DomainModelWithoutDetails;
                model.WalletId = model.Wallet.Id;
                return model;
            }
        }

        public static Transaction TransferTransactionDomainModel
        {
            get
            {
                var model = DomainModelWithoutDetails;
                model.Type = TransactionType.Transfer;
                model.Category = TestCategoryModels.DomainModelWithoutDetails;
                model.CategoryId = model.Category.Id;
                model.SourceWallet = TestWalletModels.DomainModelWithoutDetails;
                model.SourceWalletId = model.SourceWallet.Id;
                model.TargetWallet = TestWalletModels.DomainModelWithoutDetails;
                model.TargetWalletId = model.TargetWallet.Id;
                return model;
            }
        }

        public static TransactionUpsertBusinessModel UpsertTransactionBusinessModel
        {
            get
            {
                var model = new TransactionUpsertBusinessModel
                {
                    Amount = TransactionDomainModel.Amount,
                    Description = TransactionDomainModel.Description,
                    TransactionDate = TransactionDomainModel.TransactionDate,
                    Type = TransactionDomainModel.Type,
                    CategoryId = TransactionDomainModel.CategoryId,
                    WalletId = TransactionDomainModel.WalletId
                };

                return model;
            }
        }

        public static TransactionDetailsBusinessModel DetailsBusinessModel
        {
            get
            {
                var model = new TransactionDetailsBusinessModel
                {
                    Id = TransferTransactionDomainModel.Id,
                    Amount = TransferTransactionDomainModel.Amount,
                    Description = TransferTransactionDomainModel.Description,
                    TransactionDate = TransferTransactionDomainModel.TransactionDate,
                    Type = TransferTransactionDomainModel.Type,
                    Category = new CategoryRelatedToTransactionDetailsBusinessModel
                    {
                        Id = TestCategoryModels.DomainModel.Id,
                        Name = TestCategoryModels.DomainModel.Name,
                        IconName = TestCategoryModels.DomainModel.IconName,
                    },
                    SourceWallet = new WalletRelatedToTransactionDetailsBusinessModel
                    {
                        Id = TestWalletModels.DomainModelWithoutDetails.Id,
                        Name = TestWalletModels.DomainModel.Name
                    },
                    TargetWallet = new WalletRelatedToTransactionDetailsBusinessModel
                    {
                        Id = TestWalletModels.DomainModelWithoutDetails.Id,
                        Name = TestWalletModels.DomainModel.Name
                    }
                };

                return model;
            }
        }

        public static TransferTransactionUpsertBusinessModel UpsertTransferTransactionBusinessModel
        {
            get
            {
                var model = new TransferTransactionUpsertBusinessModel
                {
                    Amount = TransferTransactionDomainModel.Amount,
                    Description = TransferTransactionDomainModel.Description,
                    TransactionDate = TransferTransactionDomainModel.TransactionDate,
                    SourceWalletId = TransferTransactionDomainModel.SourceWalletId,
                    TargetWalletId = TransferTransactionDomainModel.TargetWalletId
                };

                return model;
            }
        }

        public static Transaction DomainModelWithoutDetails
        {
            get
            {
                return new Transaction
                {
                    Id = Guid.NewGuid(),
                    Amount = 50.123M,
                    Description = "Test transaction",
                    TransactionDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero),
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    Type = TransactionType.Income
                };
            }
        }
    }
}
