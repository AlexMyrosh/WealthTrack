using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.DomainModels
{
    public class Category
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? IconName { get; set; }

        public OperationType? Type { get; set; }
        
        public bool IsSystem { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }

        public CategoryStatus Status { get; set; }

        public Guid? ParentCategoryId { get; set; }

        public Category ParentCategory { get; set; }

        public List<Category> ChildCategories { get; set; }

        public List<Transaction> Transactions { get; set; }

        public List<Goal> Goals { get; set; }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            if (obj == null || GetType() != obj.GetType()) return false;

            var other = (Category)obj;

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            var hashCode = Id.GetHashCode();
            return hashCode;
        }
    }
}