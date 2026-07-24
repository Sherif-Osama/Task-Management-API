using System.Net;
using System.Net.Http.Json;
using TaskManagement.Application.DTOs.Requests.Projects;
using TaskManagement.Application.DTOs.Requests.Tasks;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Application.DTOs.Responses.Projects;
using TaskManagement.Application.DTOs.Responses.Tasks;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Tests.Integration
{
    public class CriticalFlowsIntegrationTests
    {
        private readonly HttpClient _client;

        public CriticalFlowsIntegrationTests()
        {
            var factory = new CustomWebApplicationFactory();

            _client = factory.CreateClient();
        }

        //Create project → Add task → Mark task as done → Delete project
        [Fact]
        public async Task CreateProject_AddTask_MarkDone_DeleteProject()
        {
            var project = await CreateProjectAsync();

            var task = await CreateTaskAsync(project.Id, "Test Task");

            await MarkTaskAsDoneAsync(task);

            await DeleteProjectAsync(project.Id);

            await AssertTaskDoesNotExistAsync(task.Id);
        }

        [Fact]
        public async Task FilterTasks_ByStatusAndPriority_ReturnsOnlyMatchingTasks()
        {
            var project = await CreateProjectAsync();

            var doneHighTask = await CreateTaskAsync(project.Id, "Done High Task", TaskPriority.High);

            doneHighTask = await MarkTaskStatusAsync(doneHighTask, TaskItemStatus.Done);

            var todoLowTask = await CreateTaskAsync(project.Id, "Todo Low Task", TaskPriority.Low);

            var response = await _client.GetAsync($"/api/tasks?status={TaskItemStatus.Done}&priority={TaskPriority.High}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<PagedResponse<TaskListResponse>>();

            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Contains(result.Items, t => t.Id == doneHighTask.Id);
            Assert.DoesNotContain(result.Items, t => t.Id == todoLowTask.Id);
            Assert.All(result.Items, t =>
            {
                Assert.Equal(TaskItemStatus.Done, t.Status);
                Assert.Equal(TaskPriority.High, t.Priority);
            });
        }

        [Fact]
        public async Task SearchTasks_WithPagination_ReturnsMatchingResultsAndRespectsPageSize()
        {
            var project = await CreateProjectAsync();

            var matchingTitle = $"UniqueSearchTerm_{Guid.NewGuid():N}";

            for (var i = 1; i <= 3; i++)
            {
                await CreateTaskAsync(project.Id, $"{matchingTitle} #{i}");
            }

            await CreateTaskAsync(project.Id, "Unrelated");

            var firstPageResponse = await _client.GetAsync($"/api/tasks?q={matchingTitle}&page=1&limit=2");

            Assert.Equal(HttpStatusCode.OK, firstPageResponse.StatusCode);

            var firstPage = await firstPageResponse.Content.ReadFromJsonAsync<PagedResponse<TaskListResponse>>();

            Assert.NotNull(firstPage);
            Assert.Equal(3, firstPage.TotalCount);
            Assert.Equal(2, firstPage.Items.Count());
            Assert.All(firstPage.Items, t => Assert.Contains(matchingTitle, t.Title));

            var secondPageResponse = await _client.GetAsync($"/api/tasks?q={matchingTitle}&page=2&limit=2");

            Assert.Equal(HttpStatusCode.OK, secondPageResponse.StatusCode);

            var secondPage = await secondPageResponse.Content.ReadFromJsonAsync<PagedResponse<TaskListResponse>>();

            Assert.NotNull(secondPage);
            Assert.Single(secondPage.Items);

            var allReturnedIds = firstPage.Items.Select(t => t.Id).Concat(secondPage.Items.Select(t => t.Id)).ToList();

            Assert.Equal(3, allReturnedIds.Distinct().Count());
        }

        #region Helpers

        private async Task<ProjectResponse> CreateProjectAsync()
        {
            var request = new CreateProjectRequest
            {
                Name = $"Test Project {Guid.NewGuid():N}",
                Description = "Created from integration test"
            };

            var response = await _client.PostAsJsonAsync("/api/projects", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var project = await response.Content.ReadFromJsonAsync<ProjectResponse>();

            Assert.NotNull(project);

            return project;
        }

        private async Task<TaskResponse> CreateTaskAsync(int projectId, string title, TaskPriority priority = TaskPriority.Medium)
        {
            var request = new CreateTaskRequest
            {
                Title = title,
                Description = "Created from integration test",
                Priority = priority
            };

            var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/tasks", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var task = await response.Content.ReadFromJsonAsync<TaskResponse>();

            Assert.NotNull(task);

            return task;
        }

        private async Task<TaskResponse> MarkTaskStatusAsync(TaskResponse task, TaskItemStatus status)
        {
            var request = new UpdateTaskRequest
            {
                Title = task.Title,
                Description = task.Description,
                Status = status,
                Priority = task.Priority,
                DueDate = task.DueDate
            };

            var response = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}", request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var updatedTask = await response.Content.ReadFromJsonAsync<TaskResponse>();

            Assert.NotNull(updatedTask);
            Assert.Equal(status, updatedTask.Status);

            return updatedTask;
        }

        private Task MarkTaskAsDoneAsync(TaskResponse task) => MarkTaskStatusAsync(task, TaskItemStatus.Done);

        private async Task DeleteProjectAsync(int projectId)
        {
            var response = await _client.DeleteAsync($"/api/projects/{projectId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await _client.GetAsync($"/api/projects/{projectId}");

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        private async Task AssertTaskDoesNotExistAsync(int taskId)
        {
            var response = await _client.GetAsync($"/api/tasks/{taskId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion
    }
}