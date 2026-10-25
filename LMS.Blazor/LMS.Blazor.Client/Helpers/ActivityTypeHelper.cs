namespace LMS.Blazor.Client.Helpers;

public static class ActivityTypeHelper
{
    public static string GetName(int activityTypeId) => activityTypeId switch
    {
        1 => "Föreläsning",
        2 => "E-lärande",
        3 => "Övning",
        4 => "Inlämning",
        _ => "Övrigt"
    };

    public static string GetBadgeClass(int activityTypeId) => activityTypeId switch
    {
        1 => "bg-primary bg-opacity-25 text-primary",     // Föreläsning
        2 => "bg-success bg-opacity-25 text-success",     // E-lärande
        3 => "bg-warning bg-opacity-25 text-warning",     // Övning
        4 => "bg-danger bg-opacity-25 text-danger",       // Inlämning
        _ => "bg-secondary bg-opacity-25 text-secondary"  // Övrigt
    };
}
