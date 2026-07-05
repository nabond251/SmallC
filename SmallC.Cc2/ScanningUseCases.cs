// <copyright file="ScanningUseCases.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc2;

using SmallC.Cc;

/// <summary>
/// Scanning use cases.
/// </summary>
public class ScanningUseCases(
    InputUseCases input,
    Storage storage)
{
    /// <summary>
    /// Advances the input past white space to the beginning of the next token
    /// or until the end of the input is reached.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task BlanksAsync()
    {
        while (true)
        {
            while (storage.Ch.HasValue)
            {
                if (this.White())
                {
                    _ = this.Gch();
                }
                else
                {
                    return;
                }
            }

            if (storage.Line == storage.MLine)
            {
                return;
            }

            await input.PreprocessAsync().ConfigureAwait(false);
            if (storage.Eof)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Returns <c>true</c> if the current input character is a space or a
    /// control character and <c>false</c> otherwise.
    /// </summary>
    /// <returns>
    /// A value indicating wether the current input character is a space or a
    /// control character.
    /// </returns>
    public bool White()
    {
        return
            storage.LPtr < storage.Line?.Length &&
            storage.Line?[storage.LPtr] <= ' ';
    }

    /// <summary>
    /// Returns the current character of the input line, advances
    /// <see cref="Storage.LPtr"/> to the next one, and places it in
    /// <see cref="Storage.Ch"/>.
    /// </summary>
    /// <returns>The current character of the input line.</returns>
    public char? Gch()
    {
        var c = storage.Ch;
        if (c.HasValue)
        {
            this.Bump(1);
        }

        return c;
    }

    /// <summary>
    /// Either advances the current position in the input line (indicated by
    /// <see cref="Storage.LPtr"/>) a specified number of positions beyond the
    /// current character, or it sets it to the beginning of the line.
    /// </summary>
    /// <param name="n">
    /// If zero, clears <see cref="Storage.LPtr"/>; else adds to it.
    /// </param>
    public void Bump(int n)
    {
        if (n != 0)
        {
            storage.LPtr += n;
        }
        else
        {
            storage.LPtr = 0;
        }

        storage.NCh = storage.Line?[storage.LPtr];
        storage.Ch = storage.NCh;
        if (storage.Ch.HasValue)
        {
            storage.NCh = storage.Line?[storage.LPtr + 1];
        }
    }
}
