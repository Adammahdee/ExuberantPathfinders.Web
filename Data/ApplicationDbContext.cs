using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ExuberantPathfinders.Web.Models;

namespace ExuberantPathfinders.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Application> Applications { get; set; }
        public DbSet<ThematicArea> ThematicAreas { get; set; }
        public DbSet<GrantProgram> Programs { get; set; }
        public DbSet<ApplicationStatusHistory> ApplicationStatusHistories { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename Identity tables
            builder.Entity<ApplicationUser>().ToTable("AspNetUsers");

            // Application configurations
            builder.Entity<Application>()
                .HasOne(a => a.Applicant)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.ApplicantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Application>()
                .HasOne(a => a.Program)
                .WithMany(p => p.Applications)
                .HasForeignKey(a => a.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Application>()
                .Property(a => a.Status)
                .HasConversion<int>();

            // Program configurations
            builder.Entity<GrantProgram>()
                .HasOne(p => p.ThematicArea)
                .WithMany(t => t.Programs)
                .HasForeignKey(p => p.ThematicAreaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GrantProgram>()
                .HasOne(p => p.ProgramOfficer)
                .WithMany()
                .HasForeignKey(p => p.ProgramOfficerId)
                .OnDelete(DeleteBehavior.SetNull);

            // ApplicationStatusHistory
            builder.Entity<ApplicationStatusHistory>()
                .Property(a => a.PreviousStatus)
                .HasConversion<int>();

            builder.Entity<ApplicationStatusHistory>()
                .Property(a => a.NewStatus)
                .HasConversion<int>();

            // Campaign configurations
            builder.Entity<Campaign>()
                .HasOne(c => c.Program)
                .WithMany(p => p.Campaigns)
                .HasForeignKey(c => c.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            // Donation configurations
            builder.Entity<Donation>()
                .HasOne(d => d.Donor)
                .WithMany(u => u.Donations)
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Donation>()
                .HasOne(d => d.Campaign)
                .WithMany(c => c.Donations)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Donation>()
                .Property(d => d.Status)
                .HasConversion<int>();

            builder.Entity<Donation>()
                .Property(d => d.Gateway)
                .HasConversion<int>();

            // AuditLog configurations
            builder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<AuditLog>()
                .Property(a => a.Action)
                .HasConversion<int>();

            // Indexes
            builder.Entity<Application>().HasIndex(a => a.SubmissionReference).IsUnique();
            builder.Entity<Donation>().HasIndex(d => d.PaystackReference);
            builder.Entity<Donation>().HasIndex(d => d.TransactionId);
            builder.Entity<AuditLog>().HasIndex(a => a.CreatedAt);
        }
    }
}
