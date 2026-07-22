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

        public async Task<TaskItem?> CreateAsync(TaskItem task)
        {
            await _context.Tasks.AddAsync(task);

            await _context.SaveChangesAsync();

            return task;
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
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
            var query = _context.Tasks.AsNoTracking().Where(t => t.ProjectId == projectId);

            query = Filter(query, parameters);

            var totalCount = await query.CountAsync();

            query = Sort(query, parameters);

            query = Pagination(query, parameters.Page, parameters.Limit);

            var items = await query.ToListAsync();

            return (items, totalCount);
        }

        public async Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetAllAsync(TaskQueryParameters parameters)
        {
            var query = _context.Tasks.AsNoTracking().Include(t => t.Project).AsQueryable();

            query = Filter(query, parameters);

            query = search(query, parameters.Q);

            var totalCount = await query.CountAsync();

            query = Sort(query, parameters);

            query = Pagination(query, parameters.Page, parameters.Limit);

            var items = await query.ToListAsync();

            return (items, totalCount);
        }

        #region Filtering & Sorting &  Pagination & search Method
        private IQueryable<TaskItem> Filter(IQueryable<TaskItem> query, TaskQueryParameters parameters)
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

        private IQueryable<TaskItem> Sort(IQueryable<TaskItem> query, TaskQueryParameters parameters)
        {
            if (parameters.SortBy == TaskSortBy.DueDate)
            {
                query = parameters.SortDirection == SortDirection.Desc ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate);
            }
            else if (parameters.SortBy == TaskSortBy.Priority)
            {
                query = parameters.SortDirection == SortDirection.Desc ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority);
            }
            else if (parameters.SortBy == TaskSortBy.CreatedAt)
            {
                query = parameters.SortDirection == SortDirection.Desc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt);
            }
            else
            {
                //default sorting
                query = query.OrderByDescending(t => t.CreatedAt);
            }

            return query;
        }

        private IQueryable<TaskItem> Pagination(IQueryable<TaskItem> query, int Page, int Limit)
        {
            return query.Skip((Page - 1) * Limit).Take(Limit);
        }

        private IQueryable<TaskItem> search(IQueryable<TaskItem> query, string? searchString)
        {
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var searchQ = searchString.Trim();

                query = query.Where(t => t.Title.Contains(searchQ) || (t.Description != null && t.Description.Contains(searchQ)));
            }

            return query;
        }
        #endregion
    }
}