namespace TaskManagement.Application.DTOs.Responses
{
    public class ProjectResponse
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}