using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs.Queries;
using TaskManagement.Application.DTOs.Requests.Tasks;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.DTOs.Responses.Tasks;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces.Repositories;
using TaskManagement.Application.Interfaces.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<TaskService> _logger;

        public TaskService(ITaskRepository taskRepository, IProjectRepository projectRepository, ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _logger = logger;
        }

        #region Private Helpers

        private static void ValidateId(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("Id must be greater than zero.");
            }
        }

        private static void ValidateEnum<TEnum>(TEnum value, string fieldName)
            where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(value))
            {
                throw new ValidationException($"Invalid {fieldName}.");
            }
        }

        private static string ValidateTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ValidationException("Title is required.");
            }

            title = title.Trim();

            if (title.Length > 200)
            {
                throw new ValidationException("Title cannot exceed 200 characters.");
            }

            return title;
        }

        private static string? ValidateDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return null;
            }

            description = description.Trim();

            if (description.Length > 1000)
            {
                throw new ValidationException("Description cannot exceed 1000 characters.");
            }

            return description;
        }

        private static void ValidateDueDate(DateTime? dueDate)
        {
            if (dueDate.HasValue && dueDate.Value.Date < DateTime.Now.Date)
            {
                throw new ValidationException("Due date cannot be in the past.");
            }
        }

        private void ValidateStatusTransition(int taskId, TaskItemStatus currentStatus, TaskItemStatus newStatus)
        {
            if (currentStatus == TaskItemStatus.Done && newStatus == TaskItemStatus.Todo)
            {
                _logger.LogWarning(
                    "Unusual status transition: Task {TaskId} moved from Done back to Todo.",
                    taskId);
            }
        }

        private static void ValidatePagination(int page, int limit)
        {
            if (page <= 0)
            {
                throw new ValidationException("Page must be greater than zero.");
            }

            if (limit <= 0)
            {
                throw new ValidationException("Limit must be greater than zero.");
            }

            if (limit > 100)
            {
                throw new ValidationException("Limit cannot exceed 100.");
            }
        }

        private static void ValidateDateRange(DateTime? dueDateFrom, DateTime? dueDateTo)
        {
            if (dueDateFrom.HasValue && dueDateTo.HasValue && dueDateFrom.Value.Date > dueDateTo.Value.Date)
            {
                throw new ValidationException("DueDateFrom cannot be after DueDateTo.");
            }
        }

        private static void ValidateQueryParameters(TaskQueryParameters parameters)
        {
            if (parameters is null)
                throw new ValidationException("Query parameters cannot be null.");

            ValidatePagination(parameters.Page, parameters.Limit);
            ValidateDateRange(parameters.DueDateFrom, parameters.DueDateTo);

            if (parameters.Status.HasValue)
            {
                ValidateEnum(parameters.Status.Value, "status");
            }

            if (parameters.Priority.HasValue)
            {
                ValidateEnum(parameters.Priority.Value, "priority");
            }

            if (parameters.SortBy.HasValue)
            {
                ValidateEnum(parameters.SortBy.Value, "sortBy");
            }

            ValidateEnum(parameters.SortDirection, "sortDirection");
        }

        private static void ValidateCreateRequest(CreateTaskRequest request)
        {
            if (request is null)
            {
                throw new ValidationException("Request body cannot be null.");
            }
        }

        private static void ValidateUpdateRequest(UpdateTaskRequest request)
        {
            if (request is null)
            {
                throw new ValidationException("Request body cannot be null.");
            }
        }

        private async Task<TaskItem> EnsureTaskExistsAsync(int id)
        {
            ValidateId(id);

            var task = await _taskRepository.GetByIdAsync(id);

            if (task is null)
            {
                throw new NotFoundException($"Task with id {id} was not found.");
            }

            return task;
        }

        private async Task EnsureProjectExistsAsync(int projectId)
        {
            ValidateId(projectId);

            var project = await _projectRepository.GetByIdAsync(projectId);

            if (project is null)
            {
                throw new NotFoundException($"Project with id {projectId} was not found.");
            }
        }

        #endregion

        #region Public Methods

        public async Task<TaskResponse> CreateAsync(int projectId, CreateTaskRequest request)
        {
            ValidateCreateRequest(request);

            await EnsureProjectExistsAsync(projectId);

            var title = ValidateTitle(request.Title);
            var description = ValidateDescription(request.Description);

            ValidateDueDate(request.DueDate);
            ValidateEnum(request.Priority, "priority");

            var now = DateTime.Now;

            var task = new TaskItem
            {
                ProjectId = projectId,
                Title = title,
                Description = description,
                Priority = request.Priority,
                DueDate = request.DueDate,
                Status = TaskItemStatus.Todo,
                CreatedAt = now,
                UpdatedAt = now
            };

            var createdTask = await _taskRepository.CreateAsync(task);

            return MapToResponse(createdTask);
        }

        public async Task DeleteAsync(int id)
        {
            var task = await EnsureTaskExistsAsync(id);

            await _taskRepository.DeleteAsync(task);
        }

        public async Task<PagedResponse<TaskListResponse>> GetAllAsync(TaskQueryParameters parameters)
        {
            ValidateQueryParameters(parameters);

            var result = await _taskRepository.GetAllAsync(parameters);

            return new PagedResponse<TaskListResponse>
            {
                Items = result.Items.Select(MapToListResponse),
                TotalCount = result.TotalCount
            };
        }

        public async Task<TaskResponse> GetByIdAsync(int id)
        {
            var task = await EnsureTaskExistsAsync(id);

            return MapToResponse(task);
        }

        public async Task<PagedResponse<TaskListResponse>> GetByProjectIdAsync(int projectId, TaskQueryParameters parameters)
        {
            await EnsureProjectExistsAsync(projectId);

            ValidateQueryParameters(parameters);

            var result = await _taskRepository.GetByProjectIdAsync(projectId, parameters);

            return new PagedResponse<TaskListResponse>
            {
                Items = result.Items.Select(MapToListResponse),
                TotalCount = result.TotalCount
            };
        }

        public async Task<TaskResponse> UpdateAsync(int id, UpdateTaskRequest request)
        {
            ValidateUpdateRequest(request);

            var task = await EnsureTaskExistsAsync(id);

            var title = ValidateTitle(request.Title);
            var description = ValidateDescription(request.Description);

            if (request.DueDate != task.DueDate)
            {
                ValidateDueDate(request.DueDate);
            }

            ValidateEnum(request.Priority, "priority");
            ValidateEnum(request.Status, "status");

            ValidateStatusTransition(task.Id, task.Status, request.Status);

            task.Title = title;
            task.Description = description;
            task.Status = request.Status;
            task.Priority = request.Priority;
            task.DueDate = request.DueDate;
            task.UpdatedAt = DateTime.Now;

            await _taskRepository.UpdateAsync(task);

            return MapToResponse(task);
        }

        #endregion

        #region Mapping

        private static TaskResponse MapToResponse(TaskItem task)
        {
            return new TaskResponse
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }

        private static TaskListResponse MapToListResponse(TaskItem task)
        {
            return new TaskListResponse
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                ProjectName = task.Project.Name,
                Title = task.Title,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate
            };
        }

        #endregion
    }
}