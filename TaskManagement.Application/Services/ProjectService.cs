using TaskManagement.Application.DTOs.Requests.Projects;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.DTOs.Responses.Projects;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces.Repositories;
using TaskManagement.Application.Interfaces.Services;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Services
{
    public class ProjectService : IProjectService
    {
        private const int MaxNameLength = 200;
        private const int MaxDescriptionLength = 1000;

        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        #region Private Helpers
        private static void ValidateProjectId(int projectId)
        {
            if (projectId <= 0)
                throw new ValidationException("Project ID must be greater than zero.");
        }

        private static string ValidateAndNormalizeName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Project name is required.");

            var trimmedName = name.Trim();

            if (trimmedName.Length > MaxNameLength)
                throw new ValidationException($"Project name cannot exceed {MaxNameLength} characters.");

            return trimmedName;
        }

        private static string? ValidateAndNormalizeDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return null;

            var trimmedDescription = description.Trim();

            if (trimmedDescription.Length > MaxDescriptionLength)
                throw new ValidationException($"Description cannot exceed {MaxDescriptionLength} characters.");

            return trimmedDescription;
        }

        private static void ValidateCreateRequest(CreateProjectRequest request)
        {
            if (request is null)
                throw new ValidationException("Request cannot be null.");
        }

        private static void ValidateUpdateRequest(UpdateProjectRequest request)
        {
            if (request is null)
                throw new ValidationException("Request cannot be null.");
        }

        private async Task<Project> EnsureProjectExistsAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);

            if (project is null)
                throw new NotFoundException($"Project with ID {projectId} does not exist.");

            return project;
        }

        private async Task EnsureNameIsUniqueAsync(string name)
        {
            if (await _projectRepository.ExistsByNameAsync(name))
                throw new ConflictException($"A project with the name '{name}' already exists.");
        }

        private static void ValidatePagination(int page, int limit)
        {
            if (page <= 0)
                throw new ValidationException("Page must be greater than zero.");

            if (limit <= 0)
                throw new ValidationException("Limit must be greater than zero.");

            if (limit > 100)
                throw new ValidationException("Limit cannot exceed 100.");
        }
        #endregion

        #region Public Methods
        public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request)
        {
            ValidateCreateRequest(request);

            var name = ValidateAndNormalizeName(request.Name);
            var description = ValidateAndNormalizeDescription(request.Description);

            await EnsureNameIsUniqueAsync(name);

            var project = new Project
            {
                Name = name,
                Description = description,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Tasks = new List<TaskItem>()
            };

            var created = await _projectRepository.CreateAsync(project);

            return MapToResponse(created);
        }

        public async Task<ProjectResponse> UpdateAsync(int id, UpdateProjectRequest request)
        {
            ValidateProjectId(id);
            ValidateUpdateRequest(request);

            var existingProject = await EnsureProjectExistsAsync(id);

            var name = ValidateAndNormalizeName(request.Name);
            var description = ValidateAndNormalizeDescription(request.Description);

            if (!string.Equals(name, existingProject.Name, StringComparison.OrdinalIgnoreCase))
            {
                await EnsureNameIsUniqueAsync(name);
                existingProject.Name = name;
            }

            existingProject.Description = description;
            existingProject.UpdatedAt = DateTime.Now;

            await _projectRepository.UpdateAsync(existingProject);

            return MapToResponse(existingProject);
        }

        public async Task DeleteAsync(int id)
        {
            ValidateProjectId(id);

            var existingProject = await EnsureProjectExistsAsync(id);

            await _projectRepository.DeleteAsync(existingProject);
        }

        public async Task<ProjectResponse> GetByIdAsync(int id)
        {
            ValidateProjectId(id);

            var project = await EnsureProjectExistsAsync(id);

            return MapToResponse(project);
        }

        public async Task<PagedResponse<ProjectResponse>> GetAllAsync(int page, int limit)
        {
            ValidatePagination(page, limit);

            var result = await _projectRepository.GetAllAsync(page, limit);

            return new PagedResponse<ProjectResponse>
            {
                Items = result.Items.Select(MapToResponse),
                TotalCount = result.TotalCount
            };
        }

        #endregion

        //Just for mapping to Project Response DTO
        #region Mapping
        private static ProjectResponse MapToResponse(Project project)
        {
            return new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }
        #endregion
    }
}