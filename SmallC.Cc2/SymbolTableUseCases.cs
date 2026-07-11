// <copyright file="SymbolTableUseCases.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc2;

using SmallC.Cc;

/// <summary>
/// Symbol table management use cases.
/// </summary>
public class SymbolTableUseCases(Storage storage)
{
    /// <summary>
    /// Find global with given name.
    /// </summary>
    /// <param name="sName">Symbol name of global to find.</param>
    /// <returns>Global with matching name, if any.</returns>
    public SymbolTableEntry? FindGlb(string sName)
    {
        return storage.SymTab.Globals.FirstOrDefault(x => x.Name == sName);
    }
}
