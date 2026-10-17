using Abp.Zero.EntityFrameworkCore;
using Toeic.Authorization.Roles;
using Toeic.Authorization.Users;
using Toeic.MultiTenancy;
using Toeic.ReadingTest;
using Microsoft.EntityFrameworkCore;

namespace Toeic.EntityFrameworkCore;

public class ToeicDbContext : AbpZeroDbContext<Tenant, Role, User, ToeicDbContext>
{
    /* Reading test tables */
    public DbSet<Exam> Exams { get; set; }
    public DbSet<Passage> Passages { get; set; }
    public DbSet<PassageDocument> PassageDocuments { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<Attempt> Attempts { get; set; }
    public DbSet<AttemptAnswer> AttemptAnswers { get; set; }

    public ToeicDbContext(DbContextOptions<ToeicDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Exam>(b =>
        {
            b.HasIndex(e => e.Status);
            b.HasMany(e => e.Passages).WithOne(p => p.Exam).HasForeignKey(p => p.ExamId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasMany(e => e.Questions).WithOne(q => q.Exam).HasForeignKey(q => q.ExamId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Passage>(b =>
        {
            b.HasIndex(p => new { p.ExamId, p.OrderIndex });
            b.HasMany(p => p.Documents).WithOne(d => d.Passage).HasForeignKey(d => d.PassageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(b =>
        {
            b.HasIndex(q => new { q.ExamId, q.Number }).IsUnique();
            b.HasMany(q => q.Options).WithOne(o => o.Question).HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionOption>(b =>
        {
            b.HasIndex(o => new { o.QuestionId, o.OptionKey }).IsUnique();
        });

        modelBuilder.Entity<Attempt>(b =>
        {
            b.HasIndex(a => new { a.ExamId, a.UserId });
            b.HasMany(a => a.Answers).WithOne(x => x.Attempt).HasForeignKey(x => x.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Attempt>(b =>
        {
            b.HasOne(a => a.Exam).WithMany().HasForeignKey(a => a.ExamId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AttemptAnswer>(b =>
        {
            b.HasIndex(a => new { a.AttemptId, a.QuestionId }).IsUnique();
        });
    }
}
