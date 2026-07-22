using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Queries;
using TaskManagement.Application.Enums;
using TaskManagement.Application.Interfaces.Repositories;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            await _context.Tasks.AddAsync(task);

            await _context.SaveChangesAsync();

            return task;
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.Tasks.AsNoTracking().Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateAsync(TaskItem task)
        {
            _context.Tasks.Update(task);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TaskItem task)
        {
            _context.Tasks.Remove(task);

            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetByProjectIdAsync(int projectId, TaskQueryParameters parameters)
        {
            var query = _context.Tasks.AsNoTracking().Include(t => t.Project).Where(t => t.ProjectId == projectId);

            query = Filter(query, parameters);

            var totalCount = await query.CountAsync();

            query = Sort(query, parameters);

            query = Paginate(query, parameters.Page, parameters.Limit);

            var items = await query.ToListAsync();

            return (items, totalCount);
        }

        public async Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetAllAsync(TaskQueryParameters parameters)
        {
            var query = _context.Tasks.AsNoTracking().Include(t => t.Project).AsQueryable();

            query = Filter(query, parameters);

            query = Search(query, parameters.Q);

            var totalCount = await query.CountAsync();

            query = Sort(query, parameters);

            query = Paginate(query, parameters.Page, parameters.Limit);

            var items = await query.ToListAsync();

            return (items, totalCount);
        }

        #region Filtering, Sorting, Pagination & Search

        private static IQueryable<TaskItem> Filter(IQueryable<TaskItem> query, TaskQueryParameters parameters)
        {
            if (parameters.Status.HasValue)
            {
                query = query.Where(t => t.Status == parameters.Status.Value);
            }

            if (parameters.Priority.HasValue)
            {
                query = query.Where(t => t.Priority == parameters.Priority.Value);
            }

            if (parameters.DueDateFrom.HasValue)
            {
                query = query.Where(t => t.DueDate >= parameters.DueDateFrom.Value);
            }

            if (parameters.DueDateTo.HasValue)
            {
                var nextDay = parameters.DueDateTo.Value.Date.AddDays(1);

                query = query.Where(t => t.DueDate < nextDay);
            }

            return query;
        }

        private static IQueryable<TaskItem> Sort(IQueryable<TaskItem> query, TaskQueryParameters parameters)
        {
            return parameters.SortBy switch
            {
                TaskSortBy.DueDate => parameters.SortDirection == SortDirection.Desc
                    ? query.OrderByDescending(t => t.DueDate)
                    : query.OrderBy(t => t.DueDate),

                TaskSortBy.Priority => parameters.SortDirection == SortDirection.Desc
                    ? query.OrderByDescending(t => t.Priority)
                    : query.OrderBy(t => t.Priority),

                TaskSortBy.CreatedAt => parameters.SortDirection == SortDirection.Desc
                    ? query.OrderByDescending(t => t.CreatedAt)
                    : query.OrderBy(t => t.CreatedAt),

                _ => query.OrderByDescending(t => t.CreatedAt)
            };
        }

        private static IQueryable<TaskItem> Paginate(IQueryable<TaskItem> query, int page, int limit)
        {
            return query.Skip((page - 1) * limit).Take(limit);
        }

        private static IQueryable<TaskItem> Search(IQueryable<TaskItem> query, string? searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return query;
            }

            var trimmedSearch = searchString.Trim();

            return query.Where(t => t.Title.Contains(trimmedSearch) || (t.Description != null && t.Description.Contains(trimmedSearch)));
        }

        #endregion
    }
}