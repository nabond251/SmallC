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
    /// Find global with given name.
    /// </summary>
    /// <param name="sName">Symbol name of global to find.</param>
    /// <returns>Global with matching name, if any.</returns>
    public SymbolTableEntry? FindGlb(string sName)
    {
        return this.Globals.FirstOrDefault(x => x.Name == sName);
    }
}
