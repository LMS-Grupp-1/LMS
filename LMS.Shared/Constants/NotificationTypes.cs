namespace LMS.Shared.Constants;

public static class NotificationTypes
{
    public const string NewAssignment = "NewAssignment";
    public const string NewFeedback = "NewFeedback";
    public const string NewMaterial = "NewMaterial";
    public const string NewSubmission = "NewSubmission";

    public static readonly string[] All = [NewAssignment, NewFeedback, NewMaterial, NewSubmission];
}