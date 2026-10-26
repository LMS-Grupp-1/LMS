using Domain.Models.Entities;
using LMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Infrastructure.Data.Configurations;

public class ActivityTypeConfiguration : IEntityTypeConfiguration<ActivityType>
{
    public void Configure(EntityTypeBuilder<ActivityType> builder)
    {
        builder.HasKey(at => at.Id);

        builder.Property(at => at.Name)
               .IsRequired();
    }
}
