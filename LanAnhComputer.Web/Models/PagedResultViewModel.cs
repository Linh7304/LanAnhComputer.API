namespace LanAnhComputer.Web.ViewModels
{
    public class PagedResultViewModel<T>
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }

        public List<T> Items { get; set; } = new();
    }
}