namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Contains configuration settings for iterables randomization.
/// </summary>
public readonly struct IterableSettings
{
    /// <summary>
    /// Contains precise amount of elements to retrieve.
    /// </summary>
    public int Amount { get; }

    /// <summary>
    /// Contains the percentage of elements to retrieve.
    /// </summary>
    public float Percent { get; }

    /// <summary>
    /// <see langword="true"/> when the settings are precise, <see langword="false"/> otherwise.
    /// </summary>
    public bool IsPrecise => Percent == 0.0f;

    /// <summary>
    /// <see langword="true"/> when the settings are empty, <see langword="false"/> otherwise.
    /// </summary>
    public bool IsEmpty => IsPrecise && Amount == 0;

    /// <summary>
    /// <see langword="true"/> when the settings are valid, <see langword="false"/> otherwise.
    /// </summary>
    public bool IsValid => IsPrecise ? Amount > 0 : Percent > 0.0f;

    /// <summary>
    /// Initializes empty iterable settings.
    /// </summary>
    public IterableSettings() : this(0) {}

    /// <summary>
    /// Initializes new iterable settings.
    /// </summary>
    /// <param name="amount">Precise amount ot retrieve.</param>
    public IterableSettings(int amount)
    {
        Amount = amount;
        Percent = 0.0f;
    }

    /// <summary>
    /// Initializes new iterable settings.
    /// </summary>
    /// <param name="percent">Percentage to retrieve.</param>
    public IterableSettings(float percent)
    {
        Amount = 0;
        Percent = percent;
    }
}
