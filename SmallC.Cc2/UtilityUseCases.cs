// <copyright file="UtilityUseCases.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc2;

using SmallC.Cc;

/// <summary>
/// Utility use cases.
/// </summary>
public class UtilityUseCases(Storage storage)
{
    /// <summary>
    /// Test if <paramref name="c"/> is alphabetic.
    /// </summary>
    /// <param name="c">Character to test.</param>
    /// <returns>
    /// A value indicating whether <paramref name="c"/> is alphabetic.
    /// </returns>
    public static bool Alpha(char? c)
    {
        return (c.HasValue && char.IsAsciiLetter(c.Value)) || c == '_';
    }

    /// <summary>
    /// Test if given character is alphanumericc.
    /// </summary>
    /// <param name="c">Character to test.</param>
    /// <returns>
    /// A value indicating whether <paramref name="c"/> is alphanumeric.
    /// </returns>
    public static bool An(char? c)
    {
        return Alpha(c) || (c.HasValue && char.IsDigit(c.Value));
    }

    /// <summary>
    /// Get integer of length <paramref name="len"/> from address
    /// <paramref name="addr"/> (byte sequence set by "putint").
    /// </summary>
    /// <param name="addr">Index into <see cref="Storage.LitQ"/>.</param>
    /// <param name="len">Length of int to get.</param>
    /// <returns>
    /// Integer of length <paramref name="len"/> from address
    /// <paramref name="addr"/>.
    /// </returns>
    public int GetInt(int addr, int len)
    {
        int i;
        i = storage.LitQ[addr + --len]; // high order sign byte extended
        while (len-- != 0)
        {
            i = (i << 8) | (storage.LitQ[addr + len] & 255);
        }

        return i;
    }
}
