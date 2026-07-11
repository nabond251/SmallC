// <copyright file="MiscellaneousUseCases.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc1;

using SmallC.Cc;
using System;
using static SmallC.Cc.Storage;

/// <summary>
/// Miscellaneous use cases.
/// </summary>
public class MiscellaneousUseCases(Storage storage)
{
    /// <summary>
    /// Get run options.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AskAsync()
    {
        storage.NxtLab = 0;
        storage.ListFp = null;
        storage.Output = Console.Out;
        storage.Optimize = true;
        storage.Pause = false;
        storage.Monitor = storage.Pause;
        storage.Alarm = storage.Monitor;
        storage.LineType = BufferLineType.Macro;
        foreach (var line in storage.Args)
        {
            if (line.ElementAtOrDefault(0) != '-')
            {
                continue;
            }

            if (char.ToUpperInvariant(line.ElementAtOrDefault(1)) == 'L'
                && char.IsDigit(line.ElementAtOrDefault(2))
                && line.Length == 3)
            {
                storage.ListFp = (line[2] - '0') switch
                {
                    0 => throw new InvalidOperationException(),
                    1 => Console.Out,
                    2 => Console.Error,
                    3 => throw new NotSupportedException(),
                    4 => throw new NotSupportedException(),
                    _ => throw new InvalidOperationException(),
                };

                continue;
            }

            if (char.ToUpperInvariant(line.ElementAtOrDefault(1)) == 'N'
                && char.ToUpperInvariant(line.ElementAtOrDefault(2)) == 'O'
                && line.Length == 3)
            {
                storage.Optimize = false;
                continue;
            }

            if (line.Length == 2)
            {
                if (char.ToUpperInvariant(line[1]) == 'A')
                {
                    storage.Alarm = true;
                    continue;
                }

                if (char.ToUpperInvariant(line[1]) == 'M')
                {
                    storage.Monitor = true;
                    continue;
                }

                if (char.ToUpperInvariant(line[1]) == 'P')
                {
                    storage.Pause = true;
                    continue;
                }
            }

            await Console.Error.WriteLineAsync(
                "usage: cc [file]... [-m] [-a] [-p] [-l#] [-no]")
                .ConfigureAwait(false);
            throw new InvalidOperationException();
        }
    }
}
