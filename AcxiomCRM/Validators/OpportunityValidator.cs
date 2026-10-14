#nullable enable
using AcxiomCRM.Models.Enums;

namespace AcxiomCRM.Validators;

public static class OpportunityValidator
{
    public static List<string> Validate(decimal amount, decimal probability, DateTime? expectedCloseDate, OpportunityStage stage)
    {
        var errors = new List<string>();

        if (stage != OpportunityStage.Won && stage != OpportunityStage.Lost)
        {
            if (amount <= 0)
                errors.Add("Amount must be greater than zero for active opportunities.");

            if (expectedCloseDate.HasValue && expectedCloseDate.Value.Date < DateTime.Today)
                errors.Add("Expected close date cannot be in the past for active opportunities.");
        }

        if (probability < 0 || probability > 100)
            errors.Add("Probability must be between 0 and 100.");

        return errors;
    }
}
