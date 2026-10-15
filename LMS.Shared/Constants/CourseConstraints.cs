namespace LMS.Shared.Constants
{
	// Shared limits used by both DTO validation and the database schema
	public static class CourseConstraints
	{
		public const int NameMaxLength = 255;
		public const int DescriptionMaxLength = 1000;
		public const int CoursePeriod = 3;
    }
}
