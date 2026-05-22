namespace LanAnhComputer.Data.Entities
{
    public class ProductSpecification
    {
        public long ProductSpecificationId { get; set; }
        public long ProductId { get; set; }
        public string GroupName { get; set; } = "General";
        public string SpecKey { get; set; } = null!;
        public string SpecValue { get; set; } = null!;
        public int SortOrder { get; set; }

        public Product Product { get; set; } = null!;
    }
}
