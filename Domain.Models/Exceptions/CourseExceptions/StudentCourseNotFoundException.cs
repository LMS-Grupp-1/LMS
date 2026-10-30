namespace Domain.Models.Exceptions.CourseExceptions
{	
	public sealed class StudentCourseNotFoundException : NotFoundException
	{
		public StudentCourseNotFoundException()
			: base("You are not enrolled in a course.", "Course not found")
		{
		}
	}
}
