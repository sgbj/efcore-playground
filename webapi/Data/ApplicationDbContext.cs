using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using webapi.Models;

namespace webapi.Data;

public class ApplicationDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : this(options, null)
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        IHttpContextAccessor? httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.IgnoreAny<List<IEvent>>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Default delete behavior to restrict
            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            if (entityType.ClrType.IsAssignableTo(typeof(EntityBase)))
            {
                // Concurrency stamp
                entityType.FindProperty(nameof(EntityBase.ConcurrencyStamp))!.IsConcurrencyToken = true;

                // Temporal
                entityType.SetIsTemporal(true);

                // Soft delete
                var parameter = Expression.Parameter(entityType.ClrType);
                var lambda =
                    Expression.Lambda(
                        Expression.Equal(Expression.Property(parameter, nameof(EntityBase.DeletedOn)),
                            Expression.Constant(null, typeof(DateTimeOffset?))), parameter);
                entityType.SetQueryFilter(lambda);
            }
        }

        modelBuilder.Entity<Blog>().HasData(
            new Blog { Id = "47B12A9A-03A0-4FF5-91A8-6EF0093BD6AD", Name = "Blog 1" },
            new Blog { Id = "F1AD8BA9-F590-4C7D-90E0-85C0AA138093", Name = "Blog 2" },
            new Blog { Id = "8C0CDEF3-B8A8-4C79-AF0A-6663ADDA1FA2", Name = "Blog 3" });

        modelBuilder.Entity<Post>().HasData(
            new Post
            {
                Id = "D3BB153E-8856-4A3B-9E03-0D5B9B295D96", Title = "Post 1", Content = "Post 1 content",
                BlogId = "47B12A9A-03A0-4FF5-91A8-6EF0093BD6AD"
            },
            new Post
            {
                Id = "1217FD74-183E-47D7-8C00-69CCC2F384EF", Title = "Post 2", Content = "Post 2 content",
                BlogId = "47B12A9A-03A0-4FF5-91A8-6EF0093BD6AD"
            },
            new Post
            {
                Id = "C51D4C18-D178-432A-A8FC-AA6893DAE5C7", Title = "Post 3", Content = "Post 3 content",
                BlogId = "47B12A9A-03A0-4FF5-91A8-6EF0093BD6AD"
            },
            new Post
            {
                Id = "1FC6D571-4A93-4CE8-AF2A-03290127BF82", Title = "Post 4", Content = "Post 4 content",
                BlogId = "F1AD8BA9-F590-4C7D-90E0-85C0AA138093"
            });

        modelBuilder.Entity<Comment>().HasData(
            new Comment
            {
                Id = "F1107A63-6F7D-4C00-898E-D550BAF73A26", Content = "Comment 1",
                PostId = "D3BB153E-8856-4A3B-9E03-0D5B9B295D96"
            },
            new Comment
            {
                Id = "B1433D5D-70B7-48F9-808E-81CC42BEB05C", Content = "Comment 2",
                PostId = "D3BB153E-8856-4A3B-9E03-0D5B9B295D96"
            },
            new Comment
            {
                Id = "6CC5A0E6-3E09-4454-ABF5-534EEB5E27C8", Content = "Comment 3",
                PostId = "1217FD74-183E-47D7-8C00-69CCC2F384EF"
            });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var dateTime = DateTimeOffset.UtcNow;
        var userName = _httpContextAccessor?.HttpContext?.User.Identity?.Name;

        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedOn = dateTime;
                entity.CreatedBy = userName;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedOn = dateTime;
                entity.UpdatedBy = userName;
            }
            else if (entry.State == EntityState.Deleted)
            {
                entity.DeletedOn = dateTime;
                entity.DeletedBy = userName;
                entry.State = EntityState.Modified;
            }

            // TODO publish domain events
            entity.Events.Clear();
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}