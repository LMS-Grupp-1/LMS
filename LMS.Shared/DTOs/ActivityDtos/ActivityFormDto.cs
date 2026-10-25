using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.ActivityDtos;

public class ActivityFormDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Välj en giltig aktivitetstyp.")]
    public int ActivityTypeId { get; set; }

    [Required(ErrorMessage = "Namn krävs.")]
    [StringLength(100, ErrorMessage = "Namnet får inte överstiga 100 tecken.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

}