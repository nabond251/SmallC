// <copyright file="Parser.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc1;

using SmallC.Cc;
using SmallC.Cc2;
using System.Text;
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

    /// <summary>
    /// Open an include file.
    /// </summary>
    private async Task DoIncludeAsync()
    {
        int i;
        var str = new StringBuilder();

        // skip over to name
        await frontEnd.BlanksAsync().ConfigureAwait(false);
        if (storage.Line.ElementAtOrDefault(storage.LPtr) is '"' or '<')
        {
            storage.LPtr++;
        }

        i = 0;
        while (storage.Line.ElementAtOrDefault(storage.LPtr + i) is char c
            && c != '"'
            && c != '>'
            && c != '\n')
        {
            _ = str.Append(c);
            i++;
        }

        try
        {
            storage.Input2 = File.OpenText(str.ToString());
        }
        catch (Exception ex)
        {
            storage.Input2 = null;
            throw new InvalidOperationException(
                "open failure on include file", ex);
        }

        frontEnd.Kill();
    }

    /// <summary>
    /// Define a macro symbol.
    /// </summary>
    private async Task DoDefineAsync()
    {
        storage.MsName = await frontEnd.SymNameAsync().ConfigureAwait(false);
        if (storage.MsName is null)
        {
            ErrorUseCases.IllName();
            frontEnd.Kill();
            return;
        }

        if (!storage.Mac.ContainsKey(storage.MsName) &&
            storage.Mac.Count >= MacroPool.MacNbr)
        {
            throw new InvalidOperationException("macro name table full");
        }

        while (frontEnd.White())
        {
            _ = frontEnd.Gch();
        }

        var macQ = new StringBuilder();
        while (PutMac(macQ, frontEnd.Gch()) is not null)
        {
            // already parsed
        }

        storage.Mac[storage.MsName] = macQ.ToString();

        static char? PutMac(StringBuilder macQ, char? c)
        {
            _ = macQ.Append(c);
            return c;
        }
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
