using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Interfaces.Services;
using TaskManagement.Application.Services;

namespace TaskManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();

        return services;
    }
}