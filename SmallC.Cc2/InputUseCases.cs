// <copyright file="InputUseCases.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc2;

using SmallC.Cc;

/// <summary>
/// Input use cases.
/// </summary>
public class InputUseCases(Storage storage)
{
    /// <summary>
    /// Preprocess.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task PreprocessAsync()
    {
        _ = storage;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Place character into parsing buffer.
    /// </summary>
    /// <param name="c">Character to keep.</param>
    public void KeepCh(char c)
    {
        if (storage.PPtr < InputLine.LineMax)
        {
            storage.PLine += c;
        }
    }

    /// <summary>
    /// Returns the current character of the input line after advancing to the
    /// next one.
    /// </summary>
    /// <returns>
    /// The current character of the input line, else null if the end of the
    /// last input file has been reached.
    /// </returns>
    public async Task<char?> InByteAsync()
    {
        while (storage.Ch is null)
        {
            if (storage.Eof)
            {
                return null;
            }

            await this.PreprocessAsync().ConfigureAwait(false);
        }

        return storage.Ch;
    }
}
