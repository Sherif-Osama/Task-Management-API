using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Application.DTOs.Queries;
using TaskManagement.Application.DTOs.Requests.Tasks;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces.Repositories;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Tests.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<ILogger<TaskService>> _loggerMock;
        private readonly TaskService _task;

        public TaskServiceTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _loggerMock = new Mock<ILogger<TaskService>>();

            _task = new TaskService(_taskRepositoryMock.Object, _projectRepositoryMock.Object, _loggerMock.Object);
        }

        #region Helpers
        private static Project CreateValidProject(int id = 1) =>
            new()
            {
                Id = id,
                Name = $"Project Name",
                Tasks = new List<TaskItem>()
            };

        private static TaskItem CreateValidTask(int id = 1, int projectId = 1, TaskItemStatus status = TaskItemStatus.Todo, DateTime? dueDate = null)
        {
            return new TaskItem
            {
                Id = id,
                ProjectId = projectId,
                Title = "Existing Task",
                Status = status,
                Priority = TaskPriority.Medium,
                DueDate = dueDate,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Project = CreateValidProject(projectId)
            };
        }

        #endregion

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_NullRequest_ThrowsValidationException()
        {
            await Assert.ThrowsAsync<ValidationException>(() => _task.CreateAsync(1, null!));
        }

        [Fact]
        public async Task CreateAsync_ProjectDoesNotExist_ThrowsNotFoundException()
        {
            //It.IsAny<int>() any project Id
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Project?)null);

            var request = new CreateTaskRequest { Title = "New Task" };

            await Assert.ThrowsAsync<NotFoundException>(() => _task.CreateAsync(1, request));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateAsync_EmptyOrWhitespaceTitle_ThrowsValidationException(string title)
        {
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CreateValidProject());

            var request = new CreateTaskRequest { Title = title };

            await Assert.ThrowsAsync<ValidationException>(() => _task.CreateAsync(1, request));
        }

        [Fact]
        public async Task CreateAsync_TitleExceeds200Characters_ThrowsValidationException()
        {
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CreateValidProject());

            var request = new CreateTaskRequest { Title = new string('a', 201) };

            await Assert.ThrowsAsync<ValidationException>(() => _task.CreateAsync(1, request));
        }

        [Fact]
        public async Task CreateAsync_DescriptionExceeds1000Characters_ThrowsValidationException()
        {
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CreateValidProject());

            var request = new CreateTaskRequest
            {
                Title = "Valid title",
                Description = new string('a', 1001)
            };

            await Assert.ThrowsAsync<ValidationException>(() => _task.CreateAsync(1, request));
        }

        [Fact]
        public async Task CreateAsync_DueDateInThePast_ThrowsValidationException()
        {
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CreateValidProject());

            var request = new CreateTaskRequest
            {
                Title = "Task with past due date",
                DueDate = DateTime.Now.AddDays(-1)
            };

            await Assert.ThrowsAsync<ValidationException>(() => _task.CreateAsync(1, request));
        }

        [Fact]
        public async Task CreateAsync_DueDateToday_DoesNotThrow()
        {
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CreateValidProject());

            _taskRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<TaskItem>())).ReturnsAsync((TaskItem t) => t);

            var request = new CreateTaskRequest
            {
                Title = "Task due today",
                DueDate = DateTime.Now.Date
            };

            var result = await _task.CreateAsync(1, request);

            Assert.Equal("Task due today", result.Title);
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_DefaultsStatusToTodo()
        {
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CreateValidProject());

            _taskRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<TaskItem>())).ReturnsAsync((TaskItem t) => t);

            var request = new CreateTaskRequest { Title = "new task" };

            var result = await _task.CreateAsync(1, request);

            Assert.Equal(TaskItemStatus.Todo, result.Status);
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_TrimsTitleAndDescription()
        {
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CreateValidProject());

            _taskRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<TaskItem>())).ReturnsAsync((TaskItem t) => t);

            var request = new CreateTaskRequest
            {
                Title = "  Task with spaces  ",
                Description = "  Description with spaces  "
            };

            var result = await _task.CreateAsync(1, request);

            Assert.Equal("Task with spaces", result.Title);
            Assert.Equal("Description with spaces", result.Description);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_InvalidId_ThrowsValidationException()
        {
            await Assert.ThrowsAsync<ValidationException>(() => _task.DeleteAsync(0));
        }

        [Fact]
        public async Task DeleteAsync_TaskDoesNotExist_ThrowsNotFoundException()
        {
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((TaskItem?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _task.DeleteAsync(1));
        }

        [Fact]
        public async Task DeleteAsync_ValidId_CallsRepositoryDelete()
        {
            var task = CreateValidTask();

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

            await _task.DeleteAsync(task.Id);

            _taskRepositoryMock.Verify(r => r.DeleteAsync(task), Times.Once);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_TaskDoesNotExist_ThrowsNotFoundException()
        {
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((TaskItem?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _task.GetByIdAsync(1));
        }

        [Fact]
        public async Task GetByIdAsync_TaskExists_ReturnsMappedResponse()
        {
            var task = CreateValidTask();

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

            var result = await _task.GetByIdAsync(task.Id);

            Assert.Equal(task.Id, result.Id);
            Assert.Equal(task.Title, result.Title);
            Assert.Equal(task.Status, result.Status);
        }

        #endregion

        #region GetAllAsync
        [Fact]
        public async Task GetAllAsync_NullParameters_ThrowsValidationException()
        {
            await Assert.ThrowsAsync<ValidationException>(() => _task.GetAllAsync(null!));
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(-1, 10)]
        public async Task GetAllAsync_InvalidPage_ThrowsValidationException(int page, int limit)
        {
            var parameters = new TaskQueryParameters { Page = page, Limit = limit };

            await Assert.ThrowsAsync<ValidationException>(() => _task.GetAllAsync(parameters));
        }

        [Theory]
        [InlineData(1, 101)]
        [InlineData(1, -1)]
        [InlineData(1, 0)]
        public async Task GetAllAsync_InvalidLimit_ThrowsValidationException(int page, int limit)
        {
            var parameters = new TaskQueryParameters { Page = page, Limit = limit };

            await Assert.ThrowsAsync<ValidationException>(() => _task.GetAllAsync(parameters));
        }

        [Fact]
        public async Task GetAllAsync_DueDateFromAfterDueDateTo_ThrowsValidationException()
        {
            var parameters = new TaskQueryParameters
            {
                DueDateFrom = DateTime.Now.AddDays(1),
                DueDateTo = DateTime.Now
            };

            await Assert.ThrowsAsync<ValidationException>(() => _task.GetAllAsync(parameters));
        }

        [Fact]
        public async Task GetAllAsync_ValidParameters_ReturnsPagedResponseWithProjectName()
        {
            var tasks = new List<TaskItem> { CreateValidTask() };

            _taskRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<TaskQueryParameters>())).ReturnsAsync((tasks, tasks.Count));

            var parameters = new TaskQueryParameters();

            var result = await _task.GetAllAsync(parameters);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal("Project Name", result.Items.First().ProjectName);
        }

        #endregion

        #region GetByProjectIdAsync

        [Fact]
        public async Task GetByProjectIdAsync_ProjectDoesNotExist_ThrowsNotFoundException()
        {
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Project?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _task.GetByProjectIdAsync(1, new TaskQueryParameters()));
        }

        [Fact]
        public async Task GetByProjectIdAsync_ValidProject_ReturnsFilteredTasks()
        {
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CreateValidProject());

            var tasks = new List<TaskItem> { CreateValidTask() };

            _taskRepositoryMock.Setup(r => r.GetByProjectIdAsync(1, It.IsAny<TaskQueryParameters>())).ReturnsAsync((tasks, tasks.Count));

            var result = await _task.GetByProjectIdAsync(1, new TaskQueryParameters());

            Assert.Equal(1, result.TotalCount);
        }

        #endregion

        #region UpdateAsync
        [Fact]
        public async Task UpdateAsync_NullRequest_ThrowsValidationException()
        {
            await Assert.ThrowsAsync<ValidationException>(() => _task.UpdateAsync(1, null!));
        }

        [Fact]
        public async Task UpdateAsync_TaskDoesNotExist_ThrowsNotFoundException()
        {
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((TaskItem?)null);

            var request = new UpdateTaskRequest { Title = "Updated" };

            await Assert.ThrowsAsync<NotFoundException>(() => _task.UpdateAsync(999, request));
        }

        [Fact]
        public async Task UpdateAsync_DueDateChangedToPast_ThrowsValidationException()
        {
            var task = CreateValidTask(dueDate: DateTime.Now.AddDays(5));

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

            var request = new UpdateTaskRequest
            {
                Title = task.Title,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = DateTime.Now.AddDays(-1)
            };

            await Assert.ThrowsAsync<ValidationException>(() => _task.UpdateAsync(task.Id, request));
        }

        [Fact]
        public async Task UpdateAsync_DueDateUnchangedEvenIfInPast_DoesNotThrow()
        {
            var pastDueDate = DateTime.Now.AddDays(-1);
            var task = CreateValidTask(dueDate: pastDueDate);

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

            var request = new UpdateTaskRequest
            {
                Title = task.Title,
                Status = TaskItemStatus.Done,
                Priority = task.Priority,
                DueDate = pastDueDate
            };

            var result = await _task.UpdateAsync(task.Id, request);

            Assert.Equal(TaskItemStatus.Done, result.Status);
        }

        [Fact]
        public async Task UpdateAsync_StatusTransitionFromDoneToTodo_DoesNotThrow()
        {
            var task = CreateValidTask(status: TaskItemStatus.Done);

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

            var request = new UpdateTaskRequest
            {
                Title = task.Title,
                Status = TaskItemStatus.Todo,
                Priority = task.Priority
            };

            var result = await _task.UpdateAsync(task.Id, request);

            Assert.Equal(TaskItemStatus.Todo, result.Status);
        }

        [Fact]
        public async Task UpdateAsync_InvalidStatusEnum_ThrowsValidationException()
        {
            var task = CreateValidTask();

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

            var request = new UpdateTaskRequest
            {
                Title = task.Title,
                Status = (TaskItemStatus)999,
                Priority = task.Priority
            };

            await Assert.ThrowsAsync<ValidationException>(() => _task.UpdateAsync(task.Id, request));
        }

        [Fact]
        public async Task UpdateAsync_ValidRequest_UpdatesUpdatedAtTimestamp()
        {
            var task = CreateValidTask();
            var originalUpdatedAt = task.UpdatedAt;

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

            var request = new UpdateTaskRequest
            {
                Title = "Updated title",
                Status = task.Status,
                Priority = task.Priority
            };

            await Task.Delay(10);
            var result = await _task.UpdateAsync(task.Id, request);

            Assert.True(result.UpdatedAt > originalUpdatedAt); _taskRepositoryMock.Verify(r => r.UpdateAsync(task), Times.Once);
        }

        #endregion
    }
}