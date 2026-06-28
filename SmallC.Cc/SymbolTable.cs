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
    /// Find global with given name.
    /// </summary>
    /// <param name="sName">Symbol name of global to find.</param>
    /// <returns>Global with matching name, if any.</returns>
    public SymbolTableEntry? FindGlb(string sName)
    {
        return this.Globals.FirstOrDefault(x => x.Name == sName);
    }
}
