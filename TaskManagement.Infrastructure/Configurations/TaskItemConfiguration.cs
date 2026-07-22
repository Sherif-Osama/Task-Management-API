using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Configurations
{
    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title).IsRequired().HasMaxLength(200);

            builder.Property(t => t.Description).HasMaxLength(1000);

            builder.Property(t => t.Status).IsRequired();

            builder.Property(t => t.Priority).IsRequired();

            builder.HasOne(t => t.Project).WithMany(p => p.Tasks).HasForeignKey(t => t.ProjectId).OnDelete(DeleteBehavior.Cascade);

            builder.Property(t => t.CreatedAt).IsRequired();

            builder.Property(t => t.UpdatedAt).IsRequired();

            builder.HasIndex(t => t.Status);

            builder.HasIndex(t => t.Priority);

            builder.HasIndex(t => t.DueDate);

            builder.HasIndex(t => new { t.ProjectId, t.Status });
        }
    }
}