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
