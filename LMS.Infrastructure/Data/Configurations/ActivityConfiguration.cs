using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Infrastructure.Data.Configurations
{
    public sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
    {
        public void Configure(EntityTypeBuilder<Activity> builder)
        {
            builder.Property(a => a.Name)
                .IsRequired();

            builder.HasOne(a => a.Module)
                .WithMany(m => m.Activities)
                .HasForeignKey(a => a.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);

			// An activity type that is in use cannot be deleted.
			builder.HasOne(a => a.ActivityType)
				.WithMany(at => at.Activities)
				.HasForeignKey(a => a.ActivityTypeId)
				.OnDelete(DeleteBehavior.Restrict);
		}
    }
}
