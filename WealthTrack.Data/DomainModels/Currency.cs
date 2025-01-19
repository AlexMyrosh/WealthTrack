using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.DomainModels
{
    public class Currency
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public CurrencyStatus Status { get; set; }

        public CurrencyType Type { get; set; }

        public List<Wallet> Wallets { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            if (obj == null || GetType() != obj.GetType()) return false;

            var other = (Currency)obj;

            return Id == other.Id || Code == other.Code || Name == other.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = Id.GetHashCode();

            hashCode = (hashCode * 397) ^ (Code?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);

            return hashCode;
        }

    }
}