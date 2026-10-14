#nullable enable
using AcxiomCRM.Models.Enums;

namespace AcxiomCRM.Validators;

public static class FollowUpValidator
{
    public static List<string> Validate(DateTime followUpDate, string? subject, FollowUpStatus status)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(subject))
            errors.Add("Subject is required.");

        if ((status == FollowUpStatus.Planned) && followUpDate.Date < DateTime.Today)
            errors.Add("Follow-up date cannot be in the past for new or planned follow-ups.");

        return errors;
    }
}
