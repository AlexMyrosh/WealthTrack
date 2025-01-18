﻿using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.DomainModels
{
    public class Category
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string IconName { get; set; }

        public CategoryType Type { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }

        public CategoryStatus Status { get; set; }

        public Guid? ParentCategoryId { get; set; }

        public virtual Category ParentCategory { get; set; }

        public virtual List<Category> ChildCategories { get; set; }

        public virtual List<Transaction> Transactions { get; set; }
    }
}