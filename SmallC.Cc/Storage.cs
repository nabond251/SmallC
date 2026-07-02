// <copyright file="Storage.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc;

using System.Collections.ObjectModel;

/// <summary>
/// Miscellaneous storage.
/// </summary>
public class Storage(
    int csp,
    TextWriter output,
    Collection<KeyValuePair<PCode, int>>? stage,
    int sLast,
    SegmentType oldSeg,
    bool optimize,
    SymbolTable symTable,
    string? ssName)
{
    /// <summary>
    /// Entries in staging buffer.
    /// </summary>
    public const int StageSize = 200;

    /// <summary>
    /// Gets or sets compiler relative stk ptr.
    /// </summary>
    public int Csp { get; set; } = csp;

    /// <summary>
    /// Gets fd for output file.
    /// </summary>
    public TextWriter Output { get; } = output;

    /// <summary>
    /// Gets staging buffer.
    /// </summary>
    public Collection<KeyValuePair<PCode, int>>? Stage { get; private set; } =
        stage;

    /// <summary>
    /// Gets next index in stage.
    /// </summary>
    public int? SNext => this.Stage?.Count;

    /// <summary>
    /// Gets last index in stage.
    /// </summary>
    public int? SLast { get; } = sLast;

    /// <summary>
    /// Gets or sets current <see cref="SegmentType"/>.
    /// </summary>
    public SegmentType OldSeg { get; set; } = oldSeg;

    /// <summary>
    /// Gets or sets a value indicating whether to optimize output of staging
    /// buffer.
    /// </summary>
    public bool Optimize { get; set; } = optimize;

    /// <summary>
    /// Gets symbol table.
    /// </summary>
    public SymbolTable SymTable { get; } = symTable;

    /// <summary>
    /// Gets or sets static symbol name.
    /// </summary>
    public string? SsName { get; set; } = ssName;

    /// <summary>
    /// Sets stage if not already active.
    /// </summary>
    public void SetStage()
    {
        this.Stage ??= [];
    }

    /// <summary>
    /// Clears stage.
    /// </summary>
    public void ClearStage()
    {
        this.Stage = null;
    }
}
