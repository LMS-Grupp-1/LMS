using LMS.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.CourseDtos
{
    public class CourseCreateDto
    {
        // Parameterless constructor (required for standard object initializers & data binding)
        public CourseCreateDto() { }

        //  Parameterized constructor (supports old positional record-style usage)
        public CourseCreateDto(string name, string description, DateTime startDate, DateTime endDate)
        {
            Name = name;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Kursnamn måste anges.")]
        [StringLength(CourseConstraints.NameMaxLength, ErrorMessage = "Kursnamnet får inte överskrida {1} tecken.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(CourseConstraints.DescriptionMaxLength, ErrorMessage = "Kursbeskrivningen får inte överskrida {1} tecken.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Startdatum måste anges.")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Slutdatum måste anges.")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(CourseConstraints.CoursePeriod);
    }
}