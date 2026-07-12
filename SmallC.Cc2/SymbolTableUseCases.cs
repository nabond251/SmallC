// <copyright file="SymbolTableUseCases.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc2;

using SmallC.Cc;
using System.Collections.ObjectModel;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// Symbol table management use cases.
/// </summary>
public class SymbolTableUseCases(Storage storage)
{
    /// <summary>
    /// Add symbol to symbol table.
    /// </summary>
    /// <param name="sName">Symbol name.</param>
    /// <param name="id">Symbol identity.</param>
    /// <param name="type">Symbol type.</param>
    /// <param name="size">Symbol size.</param>
    /// <param name="value">Symbol value.</param>
    /// <param name="lgpp">Local or global table to add to.</param>
    /// <param name="class">Symbol class.</param>
    /// <returns>Symbol table entry for <paramref name="sName"/>.</returns>
    public SymbolTableEntry? AddSym(
        string sName,
        SymbolIdentity id,
        SymbolType type,
        int size,
        int value,
        Collection<SymbolTableEntry> lgpp,
        SymbolClass @class)
    {
        ArgumentNullException.ThrowIfNull(lgpp);

        if (lgpp == storage.SymTab.Globals)
        {
            if (this.FindGlb(sName) is SymbolTableEntry cptr2)
            {
                return cptr2;
            }

            if (lgpp.Count >= SymbolTable.NumGlbs)
            {
                throw new InvalidOperationException(
                    "global symbol table overflow");
            }
        }
        else if (lgpp.Count >= SymbolTable.NumLocs)
        {
            throw new InvalidOperationException("local symbol table overflow");
        }

        var cptr = new SymbolTableEntry(
            id,
            type,
            @class,
            size,
            value,
            sName);
        lgpp.Add(cptr);
        return cptr;
    }

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
