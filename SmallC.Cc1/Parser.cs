// <copyright file="Parser.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc1;

using SmallC.Cc;
using SmallC.Cc2;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// High level parser.
/// </summary>
public class Parser(
    Storage storage,
    FrontEnd frontEnd)
{
    /// <summary>
    /// Process all input text.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// At this level, only static declarations,
    ///      defines, includes and function
    ///      definitions are legal...
    /// </remarks>
    public async Task ParseAsync()
    {
        while (!storage.Eof)
        {
            if (await frontEnd.AMatchAsync("extern", 6).ConfigureAwait(false))
            {
                _ = await this.DoDeclareAsync(SymbolClass.External)
                    .ConfigureAwait(false);
            }
            else if (await this.DoDeclareAsync(SymbolClass.Static)
                .ConfigureAwait(false))
            {
                // Already parsed
            }
            else if (await frontEnd.MatchAsync("#asm").ConfigureAwait(false))
            {
                await this.DoAsmAsync().ConfigureAwait(false);
            }
            else if (await frontEnd.MatchAsync("#include")
                .ConfigureAwait(false))
            {
                await this.DoIncludeAsync().ConfigureAwait(false);
            }
            else if (await frontEnd.MatchAsync("#define").ConfigureAwait(false))
            {
                await this.DoDefineAsync().ConfigureAwait(false);
            }
            else
            {
                await this.DoFunctionAsync().ConfigureAwait(false);
            }

            await frontEnd.BlanksAsync().ConfigureAwait(false);
        }
    }

    private Task<bool> DoDeclareAsync(SymbolClass @class)
    {
        _ = storage;
        _ = @class;
        return Task.FromResult(false);
    }

    private Task DoIncludeAsync()
    {
        _ = storage;
        return Task.CompletedTask;
    }

    private Task DoDefineAsync()
    {
        _ = storage;
        _ = storage;
        return Task.CompletedTask;
    }

    private Task DoFunctionAsync()
    {
        _ = storage;
        _ = storage;
        _ = storage;
        return Task.CompletedTask;
    }

    private Task DoAsmAsync()
    {
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        return Task.CompletedTask;
    }
}
