namespace LanAnhComputer.Dtos;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryCode { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public int? ParentCategoryId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class CategoryUpsertDto
{
    public string CategoryCode { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public int? ParentCategoryId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
