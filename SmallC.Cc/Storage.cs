// <copyright file="Storage.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc;

/// <summary>
/// Miscellaneous storage.
/// </summary>
public class Storage(
    SymbolTable symTable)
{
    /// <summary>
    /// Gets symbol table.
    /// </summary>
    public SymbolTable SymTable { get; } = symTable;
}
