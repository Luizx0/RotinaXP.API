using System.ComponentModel.DataAnnotations;

namespace RotinaXP.API.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class NotFutureDateAttribute : ValidationAttribute
{
    public NotFutureDateAttribute() : base("The date cannot be in the future.") { }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is DateTime date) return date <= DateTime.UtcNow;
        if (value is DateOnly dateOnly) return dateOnly <= DateOnly.FromDateTime(DateTime.UtcNow);
        return false;
    }
}
