// <copyright file="Analyzer.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc3;

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc4;

/// <summary>
/// Expression analyzer.
/// </summary>
public class Analyzer(
    UtilityUseCases utility,
    FrontEnd frontEnd,
    BackEnd backEnd,
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
    /// Analyzes a test expression.
    /// </summary>
    /// <param name="label">Label to jump to.</param>
    /// <param name="parens">Whether parens are needed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task TestAsync(int label, bool parens)
    {
        int? before, start;

        if (parens)
        {
            await frontEnd.NeedAsync("(").ConfigureAwait(false);
        }
        else
        {
            throw new NotImplementedException();
        }

        (before, start) = backEnd.SetStage();

        var level = 1;

        while (level != 0)
        {
            switch (storage.Ch)
            {
                case '(':
                    level++;
                    _ = frontEnd.Gch();
                    break;
                case ')':
                    level--;
                    _ = frontEnd.Gch();
                    break;
                case null:
                    await frontEnd.PreprocessAsync().ConfigureAwait(false);
                    break;
                default:
                    _ = frontEnd.Gch();
                    break;
            }
        }

        await backEnd.GenAsync(PCode.NE10f, label).ConfigureAwait(false);
        await backEnd.ClearStageAsync(before, start).ConfigureAwait(false);
    }

    /// <summary>
    /// Parses quoted strings.
    /// </summary>
    /// <returns>Literal table offset, if parsed.</returns>
    public async Task<int?> StringAsync()
    {
        if (!await frontEnd.MatchAsync(storage.Quote).ConfigureAwait(false))
        {
            return null;
        }

        var offset = storage.LitPtr;
        while (storage.Ch != '"')
        {
            if (!storage.Ch.HasValue)
            {
                break;
            }

            this.StowLit(this.LitChar() ?? 0, 1);
        }

        _ = frontEnd.Gch();
        storage.LitQ.Add(0);
        return offset;
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

    /// <summary>
    /// Parses character literal.
    /// </summary>
    /// <returns>Parsed literal.</returns>
    public int? LitChar()
    {
        int i, oct;

        if (storage.Ch != '\\' || !storage.NCh.HasValue)
        {
            return frontEnd.Gch();
        }

        _ = frontEnd.Gch();
        switch (storage.Ch)
        {
            case 'n':
                _ = frontEnd.Gch();
                return '\n';
            case 't':
                _ = frontEnd.Gch();
                return 9; // HT
            case 'b':
                _ = frontEnd.Gch();
                return 8; // BS
            case 'f':
                _ = frontEnd.Gch();
                return 12; // FF
            default:
                break;
        }

        i = 3;
        oct = 0;
        while (i-- > 0 && storage.Ch >= '0' && storage.Ch <= '7')
        {
            oct = (oct << 3) + (frontEnd.Gch() ?? 0) - '0';
        }

        return i == 2 ? frontEnd.Gch() : oct;
    }
}
