using Domain.Models.Entities;
using LMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Infrastructure.Data.Configurations
{
	public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
	{
		public void Configure(EntityTypeBuilder<ApplicationUser> builder)
		{
			builder.Property(u => u.FirstName)
				   .IsRequired()
				   .HasMaxLength(UserConstraints.FirstNameMaxLength);

			builder.Property(u => u.LastName)
				   .IsRequired()
				   .HasMaxLength(UserConstraints.LastNameMaxLength);

			// Base64 encoded random token, well below this limit
			builder.Property(u => u.RefreshToken)
				   .HasMaxLength(200);
		}
	}
}
