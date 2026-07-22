using TaskManagement.Application.Enums;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs.Queries
{
    public class TaskQueryParameters
    {
        public int Page { get; set; } = 1;

        public int Limit { get; set; } = 10;

        public TaskItemStatus? Status { get; set; }

        public TaskPriority? Priority { get; set; }

        public DateTime? DueDateFrom { get; set; }

        public DateTime? DueDateTo { get; set; }

        public TaskSortBy? SortBy { get; set; }

        public SortDirection SortDirection { get; set; } = SortDirection.Asc;

        public string? Q { get; set; }
    }
}