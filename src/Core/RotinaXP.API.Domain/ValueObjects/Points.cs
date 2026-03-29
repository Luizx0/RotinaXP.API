namespace RotinaXP.API.Domain.ValueObjects;

public sealed class Points
{
    public int Value { get; }

    private Points(int value)
    {
        Value = value;
    }

    public static Points Zero => new(0);

    public static Points Create(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Points cannot be negative.");

        return new Points(value);
    }

    public Points Add(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount to add cannot be negative.");

        return new Points(Value + amount);
    }

    public Points Deduct(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount to deduct cannot be negative.");

        if (amount > Value)
            throw new InvalidOperationException("Insufficient points balance.");

        return new Points(Value - amount);
    }

    public bool CanAfford(int cost) => Value >= cost;

    public override string ToString() => Value.ToString();
    public override bool Equals(object? obj) => obj is Points other && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
