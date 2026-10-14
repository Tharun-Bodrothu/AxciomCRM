#nullable enable
using System.Text.RegularExpressions;

namespace AcxiomCRM.Validators;

public static class CustomerValidator
{
    public static List<string> Validate(string? name, string? email, string? phone, string? companyName)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add("Customer name is required.");
        else if (name.Length > 200)
            errors.Add("Customer name cannot exceed 200 characters.");

        if (string.IsNullOrWhiteSpace(email))
            errors.Add("Email is required.");
        else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            errors.Add("Invalid email format.");

        if (string.IsNullOrWhiteSpace(phone))
            errors.Add("Phone is required.");
        else if (!Regex.IsMatch(phone, @"^[\+]?[(]?[0-9]{1,4}[)]?[-\s\./0-9]*$"))
            errors.Add("Invalid phone format.");

        if (companyName != null && companyName.Length > 200)
            errors.Add("Company name cannot exceed 200 characters.");

        return errors;
    }
}
