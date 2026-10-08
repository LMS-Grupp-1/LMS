namespace LMS.Shared.Constants
{
	public static class UserRoles
	{
		public const string Teacher = "Teacher";
		public const string Student = "Student";

		// All roles, used when seeding the database
		public static readonly string[] All = [Teacher, Student];
	}
}
