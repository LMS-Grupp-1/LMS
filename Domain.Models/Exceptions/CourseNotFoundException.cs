namespace Domain.Models.Exceptions
{
	public sealed class CourseNotFoundException : NotFoundException
	{
		public CourseNotFoundException(int courseId)
			: base($"Course with ID {courseId} was not found.", "Course not found")
		{
		}
	}
}
