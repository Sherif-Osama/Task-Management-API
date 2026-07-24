using Moq;
using TaskManagement.Application.DTOs.Requests.Projects;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces.Repositories;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Tests.Services
{
    public class ProjectServiceTests
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly ProjectService _project;

        public ProjectServiceTests()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();

            _project = new ProjectService(_projectRepositoryMock.Object);
        }

        #region Helpers

        private static Project CreateValidProject(int id = 1, string name = "Existing Project") =>
            new()
            {
                Id = id,
                Name = name,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Tasks = new List<TaskItem>()
            };

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_NullRequest_ThrowsValidationException()
        {
            await Assert.ThrowsAsync<ValidationException>(() => _project.CreateAsync(null!));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateAsync_EmptyOrWhitespaceName_ThrowsValidationException(string name)
        {
            var request = new CreateProjectRequest { Name = name };

            await Assert.ThrowsAsync<ValidationException>(() => _project.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_NameExceeds200Characters_ThrowsValidationException()
        {
            var request = new CreateProjectRequest { Name = new string('a', 201) };

            await Assert.ThrowsAsync<ValidationException>(() => _project.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_DescriptionExceeds1000Characters_ThrowsValidationException()
        {
            var request = new CreateProjectRequest
            {
                Name = "Valid Name",
                Description = new string('a', 1001)
            };

            await Assert.ThrowsAsync<ValidationException>(() => _project.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_DuplicateName_ThrowsConflictException()
        {
            _projectRepositoryMock.Setup(r => r.ExistsByNameAsync("Existing Project")).ReturnsAsync(true);

            var request = new CreateProjectRequest { Name = "Existing Project" };

            await Assert.ThrowsAsync<ConflictException>(() => _project.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_ReturnsMappedResponse()
        {
            _projectRepositoryMock.Setup(r => r.ExistsByNameAsync(It.IsAny<string>())).ReturnsAsync(false);

            _projectRepositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Project>()))
                .ReturnsAsync((Project p) => p);

            var request = new CreateProjectRequest { Name = "New Project" };

            var result = await _project.CreateAsync(request);

            Assert.Equal("New Project", result.Name);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_InvalidId_ThrowsValidationException()
        {
            await Assert.ThrowsAsync<ValidationException>(() => _project.UpdateAsync(0, new UpdateProjectRequest { Name = "Any" }));
        }

        [Fact]
        public async Task UpdateAsync_NullRequest_ThrowsValidationException()
        {
            await Assert.ThrowsAsync<ValidationException>(() => _project.UpdateAsync(1, null!));
        }

        [Fact]
        public async Task UpdateAsync_ProjectDoesNotExist_ThrowsNotFoundException()
        {
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Project?)null);

            var request = new UpdateProjectRequest { Name = "Updated" };

            await Assert.ThrowsAsync<NotFoundException>(() => _project.UpdateAsync(999, request));
        }

        [Fact]
        public async Task UpdateAsync_NameChangedToExistingName_ThrowsConflictException()
        {
            var project = CreateValidProject();

            _projectRepositoryMock.Setup(r => r.GetByIdAsync(project.Id)).ReturnsAsync(project);
            _projectRepositoryMock.Setup(r => r.ExistsByNameAsync("Taken Name")).ReturnsAsync(true);

            var request = new UpdateProjectRequest { Name = "Taken Name" };

            await Assert.ThrowsAsync<ConflictException>(() => _project.UpdateAsync(project.Id, request));
        }

        [Fact]
        public async Task UpdateAsync_SameNameDifferentCase_DoesNotCheckUniqueness()
        {
            var project = CreateValidProject(name: "Existing Project");

            _projectRepositoryMock.Setup(r => r.GetByIdAsync(project.Id)).ReturnsAsync(project);

            var request = new UpdateProjectRequest { Name = "EXISTING PROJECT" };

            await _project.UpdateAsync(project.Id, request);

            _projectRepositoryMock.Verify(r => r.ExistsByNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ValidRequest_UpdatesFields()
        {
            var project = CreateValidProject();

            _projectRepositoryMock.Setup(r => r.GetByIdAsync(project.Id)).ReturnsAsync(project);
            _projectRepositoryMock.Setup(r => r.ExistsByNameAsync(It.IsAny<string>())).ReturnsAsync(false);

            var request = new UpdateProjectRequest { Name = "New Name", Description = "New Description" };

            var result = await _project.UpdateAsync(project.Id, request);

            Assert.Equal("New Name", result.Name);
            Assert.Equal("New Description", result.Description);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_InvalidId_ThrowsValidationException()
        {
            await Assert.ThrowsAsync<ValidationException>(() => _project.DeleteAsync(0));
        }

        [Fact]
        public async Task DeleteAsync_ProjectDoesNotExist_ThrowsNotFoundException()
        {
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Project?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _project.DeleteAsync(999));
        }

        [Fact]
        public async Task DeleteAsync_ValidId_CallsRepositoryDelete()
        {
            var project = CreateValidProject();

            _projectRepositoryMock.Setup(r => r.GetByIdAsync(project.Id)).ReturnsAsync(project);

            await _project.DeleteAsync(project.Id);

            _projectRepositoryMock.Verify(r => r.DeleteAsync(project), Times.Once);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_InvalidId_ThrowsValidationException()
        {
            await Assert.ThrowsAsync<ValidationException>(() => _project.GetByIdAsync(0));
        }

        [Fact]
        public async Task GetByIdAsync_ProjectDoesNotExist_ThrowsNotFoundException()
        {
            _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Project?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _project.GetByIdAsync(999));
        }

        [Fact]
        public async Task GetByIdAsync_ProjectExists_ReturnsMappedResponse()
        {
            var project = CreateValidProject();

            _projectRepositoryMock.Setup(r => r.GetByIdAsync(project.Id)).ReturnsAsync(project);

            var result = await _project.GetByIdAsync(project.Id);

            Assert.Equal(project.Id, result.Id);
            Assert.Equal(project.Name, result.Name);
        }

        #endregion

        #region GetAllAsync

        [Theory]
        [InlineData(0, 10)]
        [InlineData(-1, 10)]
        public async Task GetAllAsync_InvalidPage_ThrowsValidationException(int page, int limit)
        {
            await Assert.ThrowsAsync<ValidationException>(() => _project.GetAllAsync(page, limit));
        }

        [Theory]
        [InlineData(1, 101)]
        [InlineData(1, -1)]
        [InlineData(1, 0)]
        public async Task GetAllAsync_InvalidLimit_ThrowsValidationException(int page, int limit)
        {
            await Assert.ThrowsAsync<ValidationException>(() => _project.GetAllAsync(page, limit));
        }

        [Fact]
        public async Task GetAllAsync_ValidParameters_ReturnsPagedResponse()
        {
            var projects = new List<Project> { CreateValidProject() };

            _projectRepositoryMock
                .Setup(r => r.GetAllAsync(1, 10))
                .ReturnsAsync((projects, projects.Count));

            var result = await _project.GetAllAsync(1, 10);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal("Existing Project", result.Items.First().Name);
        }
        #endregion
    }
}