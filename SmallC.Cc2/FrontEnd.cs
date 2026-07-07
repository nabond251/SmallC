// <copyright file="FrontEnd.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc2;

using SmallC.Cc;
using System.Text;

/// <summary>
/// Front end.
/// </summary>
public class FrontEnd(Storage storage)
{
    /// <summary>
    /// Indicates whether or not the current substring in the source line
    /// (<paramref name="str1"/>) matches a literal string
    /// (<paramref name="str2"/>).
    /// </summary>
    /// <param name="str1">Source line substring.</param>
    /// <param name="str2">Literal to match.</param>
    /// <returns>If match, length of <paramref name="str2"/>; else 0.</returns>
    public static int StrEq(string str1, string str2)
    {
        ArgumentNullException.ThrowIfNull(str1);
        ArgumentNullException.ThrowIfNull(str2);

        return str1.StartsWith(str2, StringComparison.InvariantCulture) ?
            str2.Length : 0;
    }

    /// <summary>
    /// Indicates whether or not two alphanumeric strings or substrings match.
    /// </summary>
    /// <param name="str1">First string to match.</param>
    /// <param name="str2">Second string to match.</param>
    /// <param name="len">Max match length.</param>
    /// <returns>
    /// Length of match if first <paramref name="len"/> alphanumeric characters
    /// of <paramref name="str1"/> and <paramref name="str2"/> match; else 0.
    /// </returns>
    public static int AStrEq(string str1, string str2, int len)
    {
        ArgumentNullException.ThrowIfNull(str1);
        ArgumentNullException.ThrowIfNull(str2);

        var k = 0;
        while (k < len)
        {
            if (k == str1.Length)
            {
                break;
            }

            if (k == str2.Length)
            {
                break;
            }

            if (str1[k] != str2[k])
            {
                break;
            }

            k++;
        }

        return (k < str1.Length && UtilityUseCases.An(str1[k])) ||
            (k < str2.Length && UtilityUseCases.An(str2[k]))
            ? 0 : k;
    }

    /// <summary>
    /// Preprocess.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PreprocessAsync()
    {
        await this.IfLineAsync().ConfigureAwait(false);
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
    /// Handles all matters pertaining to conditional compilation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task IfLineAsync()
    {
        await this.InLineAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Fetches the next line of code from a source file and optionally lists
    /// it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InLineAsync()
    {
        if (await storage.Input.ReadLineAsync()
            .ConfigureAwait(false) is string line)
        {
            storage.Line = line;
        }
        else
        {
            storage.Input.Close();
            storage.Input.Dispose();
        }

        this.Bump(0);
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

        return this.Gch();
    }

    /// <summary>
    /// Test if next input string is legal symbol name.
    /// </summary>
    /// <returns>
    /// Legal symbol name, if any; else null.
    /// </returns>
    public async Task<string?> SymNameAsync()
    {
        var sName = new StringBuilder();
        await this.BlanksAsync().ConfigureAwait(false);
        if (!UtilityUseCases.Alpha(storage.Ch))
        {
            return null;
        }

        while (UtilityUseCases.An(storage.Ch))
        {
            if (sName.Length < SymbolName.NameMax)
            {
                _ = sName.Append(this.Gch());
            }
        }

        return sName.ToString();
    }

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

            await this.PreprocessAsync().ConfigureAwait(false);
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
            storage.LPtr < storage.Line.Length &&
            storage.Line[storage.LPtr] <= ' ';
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

        storage.NCh = storage.Line[storage.LPtr];
        storage.Ch = storage.NCh;
        if (storage.Ch.HasValue)
        {
            storage.NCh = storage.Line[storage.LPtr + 1];
        }
    }
}
