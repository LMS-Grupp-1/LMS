using LMS.Shared.DTOs.ActivityTypeDtos;

namespace LMS.Shared.Constants;

public static class ActivityTypes
{
    public static readonly IReadOnlyList<ActivityTypeDto> All = new List<ActivityTypeDto>
    {
        new(1, "Föreläsning"),
        new(2, "E-lärande"),
        new(3, "Övning"),
        new(4, "Inlämning"),
        new(5, "Övrigt")
    };
}