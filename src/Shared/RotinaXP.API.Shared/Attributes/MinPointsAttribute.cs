using System.ComponentModel.DataAnnotations;

namespace RotinaXP.API.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class MinPointsAttribute : ValidationAttribute
{
    private readonly int _minimum;

    public MinPointsAttribute(int minimum = 1)
        : base($"Value must be at least {minimum} point(s).")
    {
        _minimum = minimum;
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is int points) return points >= _minimum;
        return false;
    }
}
