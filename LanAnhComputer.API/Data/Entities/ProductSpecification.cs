namespace LanAnhComputer.Data.Entities
{
    public class ProductSpecification
    {
        public int ProductSpecificationId { get; set; }
        public int ProductId { get; set; }
        public string GroupName { get; set; } = "General";
        public string SpecKey { get; set; } = null!;
        public string SpecValue { get; set; } = null!;
        public int SortOrder { get; set; }

        public Product Product { get; set; } = null!;
    }
}
