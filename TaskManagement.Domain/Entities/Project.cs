namespace TaskManagement.Domain.Entities
{
    public class Project
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public required ICollection<TaskItem> Tasks { get; set; }
    }
}
