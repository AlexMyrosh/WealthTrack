using WealthTrack.Shared.Enums;

namespace WealthTrack.Shared.Extensions;

public static class TransactionTypeExtensions
{
    public static OperationType? ToOperationType(this TransactionType? transactionType)
    {
        return transactionType switch
        {
            null => null,
            TransactionType.Income => OperationType.Income,
            TransactionType.Expense => OperationType.Expense,
            TransactionType.Transfer => throw new InvalidOperationException("Transfer cannot be converted to OperationType."),
            _ => throw new ArgumentOutOfRangeException(nameof(transactionType), transactionType, null)
        };
    }

    public static TransactionType? ToTransactionType(this OperationType? operationType)
    {
        return operationType switch
        {
            null => null,
            OperationType.Income => TransactionType.Income,
            OperationType.Expense => TransactionType.Expense,
            _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null)
        };
    }
    
    public static TransactionType ToTransactionType(this OperationType operationType)
    {
        return operationType switch
        {
            OperationType.Income => TransactionType.Income,
            OperationType.Expense => TransactionType.Expense,
            _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null)
        };
    }
}