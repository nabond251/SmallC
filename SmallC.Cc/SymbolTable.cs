// <copyright file="SymbolTable.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc;

using System.Collections.ObjectModel;

/// <summary>
/// Symbol table.
/// </summary>
public record class SymbolTable(
    Collection<SymbolTableEntry> Locals,
    Collection<SymbolTableEntry> Globals)
{
    /// <summary>
    /// Number of locals (max size of <see cref="Locals"/>).
    /// </summary>
    public const int NumLocs = 25;

    /// <summary>
    /// Number of globals.
    /// </summary>
    public const int NumGlbs = 200;

    /// <summary>
    /// Access symbol table by unified index.
    /// </summary>
    /// <param name="index">
    /// Index into <see cref="Locals"/> (&lt; <see cref="NumLocs"/>) or else
    /// <see cref="Globals"/> symbol table.
    /// </param>
    /// <returns>Indexed entry.</returns>
    public SymbolTableEntry this[int index] =>
        index < NumLocs ? this.Locals[index] : this.Globals[index - NumLocs];

    /// <summary>
    /// Gets index of given <see cref="SymbolTableEntry"/>.
    /// </summary>
    /// <param name="entry">Entry whose index to find.</param>
    /// <returns>Index into <see cref="SymbolTable"/>.</returns>
    public int? IndexOf(SymbolTableEntry entry)
    {
        int? index = this.Locals.IndexOf(entry);
        if (index == -1)
        {
            index = this.Globals.IndexOf(entry);
            if (index == -1)
            {
                index = null;
            }
            else
            {
                index += NumLocs;
                return index;
            }
        }

        return index;
    }
}
