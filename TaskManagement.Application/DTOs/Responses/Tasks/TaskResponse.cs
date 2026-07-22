using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs.Responses.Tasks
{
    public class TaskResponse
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public TaskItemStatus Status { get; set; }

        public TaskPriority Priority { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
