namespace TaskManagement.Application.DTOs.Requests
{
    public class UpdateProjectRequest
    {
        public required string Name { get; set; }

        public string? Description { get; set; }
    }
}
