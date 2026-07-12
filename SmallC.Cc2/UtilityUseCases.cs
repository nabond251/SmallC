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
    /// Return next avail internal label number.
    /// </summary>
    /// <returns>Next avail internal label number.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "Has an observable side effect")]
    public int GetLabel()
    {
        storage.NxtLab++;
        return storage.NxtLab;
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

    /// <summary>
    /// Put integer of length <paramref name="len"/> into address
    /// <paramref name="addr"/> (low byte first).
    /// </summary>
    /// <param name="i">Int to put.</param>
    /// <param name="addr">Index into <see cref="Storage.LitQ"/>.</param>
    /// <param name="len">Length of int to put.</param>
    public void PutInt(int i, int addr, int len)
    {
        while (len-- != 0)
        {
            if (storage.LitQ.Count <= addr)
            {
                storage.LitQ.Add((sbyte)(i & 255));
            }
            else
            {
                storage.LitQ[addr] = (sbyte)(i & 255);
            }

            addr++;
            i >>= 8;
        }
    }
}
