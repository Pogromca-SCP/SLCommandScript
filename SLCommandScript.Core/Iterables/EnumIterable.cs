using SLCommandScript.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLCommandScript.Core.Iterables;

/// <summary>
/// Iterable wrapper for enum values.
/// </summary>
/// <typeparam name="TEnum">Type of contained enum values.</typeparam>
/// <param name="enableNone">Whether or not the None values should be included in iteration.</param>
public class EnumIterable<TEnum>(bool enableNone) : IIterable where TEnum : Enum
{
    /// <summary>
    /// Retrieves iterable object for specific enum type.
    /// </summary>
    /// <returns>Iterable object for specific enum.</returns>
    public static EnumIterable<TEnum> Get() => new(false);

    /// <summary>
    /// Retrieves iterable object for specific enum type with None values included.
    /// </summary>
    /// <returns>Iterable object for specific enum.</returns>
    public static EnumIterable<TEnum> GetWithNone() => new(true);

    /// <summary>
    /// <see langword="true" /> if last object was reached, <see langword="false" /> otherwise.
    /// </summary>
    public bool IsAtEnd
    {
        get
        {
            if (_values is null)
            {
                _values = (TEnum[]) typeof(TEnum).GetEnumValues();

                if (!_enableNone)
                {
                    _values = _values.Where(v => !v.ToString().Equals("None")).ToArray();
                }

                if (!_randomSettings.IsEmpty)
                {
                    if (_randomSettings.IsPrecise)
                    {
                        _values = _randomSettings.Amount > 0 ? IterablesUtils.Shuffle(_values, _randomSettings.Amount) : IterablesUtils.Shuffle(_values);
                    }
                    else
                    {
                        _values = _randomSettings.Percent > 0.0f ? IterablesUtils.Shuffle(_values, _randomSettings.Percent) : IterablesUtils.Shuffle(_values);
                    }
                }
                _current = 0;
            }

            return _current >= _values.Length;
        }
    }

    /// <summary>
    /// Current amount of elements.
    /// </summary>
    public int Count => _values is null ? 0 : _values.Length;

    /// <summary>
    /// Tells whether or not the None values should be included in iteration.
    /// </summary>
    private readonly bool _enableNone = enableNone;

    /// <summary>
    /// Contains wrapped array of enum values.
    /// </summary>
    private TEnum[] _values = null;

    /// <summary>
    /// Random settings used for randomization.
    /// </summary>
    private RandomSettings _randomSettings = new();

    /// <summary>
    /// Contains index of current object.
    /// </summary>
    private int _current = 0;

    /// <summary>
    /// Performs next iteration step and loads new property values into provided dictionary.
    /// </summary>
    /// <param name="targetVars">Dictionary to insert properties into.</param>
    /// <returns><see langword="true" /> if the iteration can continue, <see langword="false" /> otherwise.</returns>
    public bool LoadNext(IDictionary<string, string> targetVars)
    {
        if (IsAtEnd)
        {
            return false;
        }

        if (targetVars is not null)
        {
            targetVars["id"] = _values[_current].ToString("D");
        }

        ++_current;
        return true;
    }

    /// <summary>
    /// Randomizes contained elements.
    /// </summary>
    public void Randomize() => Randomize(new RandomSettings(-1));

    /// <summary>
    /// Randomizes contained elements and limits their amount.
    /// </summary>
    /// <param name="amount">Amount of random elements to select from iterable object, negative values disable the limit, zero disables randomization.</param>
    public void Randomize(int amount) => Randomize(new RandomSettings(amount));

    /// <summary>
    /// Randomizes contained elements and limits their amount.
    /// </summary>
    /// <param name="amount">Percentage of random elements to select from iterable object, negative values disable the limit, zero disables randomization.</param>
    public void Randomize(float amount) => Randomize(new RandomSettings(amount));

    /// <summary>
    /// Randomizes contained elements and limits their amount.
    /// </summary>
    /// <param name="settings">Settings to use for randomization, negative values disable the limit, zero disables randomization.</param>
    public void Randomize(RandomSettings settings)
    {
        _values = null;
        _randomSettings = settings;
    }

    /// <summary>
    /// Resets iteration process.
    /// </summary>
    public void Reset()
    {
        if (_values is not null)
        {
            _current = 0;
        }
    }
}
