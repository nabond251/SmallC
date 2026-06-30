// <copyright file="Storage.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc;

/// <summary>
/// Miscellaneous storage.
/// </summary>
public class Storage(
    TextWriter output,
    int? sNext,
    SegmentType oldSeg,
    SymbolTable symTable,
    string? ssName)
{
    /// <summary>
    /// Gets fd for output file.
    /// </summary>
    public TextWriter Output { get; } = output;

    /// <summary>
    /// Gets or sets next index in stage.
    /// </summary>
    public int? SNext { get; set; } = sNext;

    /// <summary>
    /// Gets or sets current <see cref="SegmentType"/>.
    /// </summary>
    public SegmentType OldSeg { get; set; } = oldSeg;

    /// <summary>
    /// Gets symbol table.
    /// </summary>
    public SymbolTable SymTable { get; } = symTable;

    /// <summary>
    /// Gets or sets static symbol name.
    /// </summary>
    public string? SsName { get; set; } = ssName;
}
