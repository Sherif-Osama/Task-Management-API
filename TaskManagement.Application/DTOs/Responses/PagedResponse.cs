namespace TaskManagement.Application.DTOs.Responses
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = [];

        public int TotalCount { get; set; }
    }
}