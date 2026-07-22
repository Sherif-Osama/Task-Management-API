using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs.Requests.Tasks
{
    public class UpdateTaskRequest
    {
        public required string Title { get; set; }

        public string? Description { get; set; }

        public TaskItemStatus Status { get; set; }

        public TaskPriority Priority { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
