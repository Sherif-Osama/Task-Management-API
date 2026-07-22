using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs.Requests.Tasks
{
    public class CreateTaskRequest
    {
        public required string Title { get; set; }

        public string? Description { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime? DueDate { get; set; }
    }
}
