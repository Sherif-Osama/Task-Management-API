using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs.Responses.Tasks
{
    public class TaskListResponse
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public TaskItemStatus Status { get; set; }

        public TaskPriority Priority { get; set; }

        public DateTime? DueDate { get; set; }
    }
}