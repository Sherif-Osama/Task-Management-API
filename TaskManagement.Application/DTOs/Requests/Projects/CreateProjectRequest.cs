namespace TaskManagement.Application.DTOs.Requests.Projects
{
    public class CreateProjectRequest
    {
        public required string Name { get; set; }

        public string? Description { get; set; }
    }
}