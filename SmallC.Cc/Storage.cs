// <copyright file="Storage.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc;

/// <summary>
/// Miscellaneous storage.
/// </summary>
public class Storage(
    TextWriter output,
    SegmentType oldSeg,
    SymbolTable symTable)
{
    /// <summary>
    /// Gets fd for output file.
    /// </summary>
    public TextWriter Output { get; } = output;

    /// <summary>
    /// Gets or sets current <see cref="SegmentType"/>.
    /// </summary>
    public SegmentType OldSeg { get; set; } = oldSeg;

    /// <summary>
    /// Gets symbol table.
    /// </summary>
    public SymbolTable SymTable { get; } = symTable;
}
