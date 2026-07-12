// <copyright file="LocalParser.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc1;

using SmallC.Cc;
using SmallC.Cc2;

/// <summary>
/// 2nd level parser.
/// </summary>
public class LocalParser(
    FrontEnd frontEnd,
    Storage storage)
{
    /// <summary>
    /// Parse assembly code.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DoAsmAsync()
    {
        storage.CCode = false; // mark mode as "asm"
        while (true)
        {
            await frontEnd.InLineAsync().ConfigureAwait(false);
            if (await frontEnd.MatchAsync("#endasm").ConfigureAwait(false))
            {
                break;
            }

            if (storage.Eof)
            {
                break;
            }

            await storage.Output.WriteAsync(storage.Line).ConfigureAwait(false);
        }

        frontEnd.Kill();
        storage.CCode = true;
    }
}
