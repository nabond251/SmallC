// <copyright file="Analyzer.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc3;

using SmallC.Cc;
using SmallC.Cc2;

/// <summary>
/// Expression analyzer.
/// </summary>
public class Analyzer(
    UtilityUseCases utility,
    FrontEnd frontEnd,
    Storage storage)
{
    /// <summary>
    /// Gets constant expression, if any.
    /// </summary>
    /// <returns>Constant expression from next tokens, if any.</returns>
    public int? ConstExpr()
    {
        int? e = null;
        if (storage.Ch is char c && char.IsDigit(c))
        {
            e = 0;
            while (storage.Ch is char d && char.IsDigit(d))
            {
                e *= 10;
                e += d - '0';
                _ = frontEnd.Gch();
            }
        }

        return e;
    }

    /// <summary>
    /// Places character or integer values in the literal pool.
    /// </summary>
    /// <param name="value">Value to place.</param>
    /// <param name="size">Value size.</param>
    public void StowLit(int value, int size)
    {
        if (storage.LitPtr + size >= LiteralPool.LitMax)
        {
            throw new InvalidOperationException("literal queue overflow");
        }

        utility.PutInt(value, storage.LitPtr, size);
    }
}
