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
            builder.HasOne(activity => activity.Module)
                .WithMany()
                .HasForeignKey(activity => activity.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);

			// An activity type that is in use cannot be deleted.
			builder.HasOne(activity => activity.ActivityType)
				.WithMany()
				.HasForeignKey(activity => activity.ActivityTypeId)
				.OnDelete(DeleteBehavior.Restrict);
		}
    }
}
