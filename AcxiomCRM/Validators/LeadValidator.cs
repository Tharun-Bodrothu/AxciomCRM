#nullable enable
using System.Text.RegularExpressions;

namespace AcxiomCRM.Validators;

public static class LeadValidator
{
    public static List<string> Validate(string? name, string? email, decimal? expectedValue)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add("Lead name is required.");

        if (!string.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            errors.Add("Invalid email format.");

        if (expectedValue.HasValue && expectedValue.Value < 0)
            errors.Add("Expected value must be non-negative.");

        return errors;
    }
}
