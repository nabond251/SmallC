// <copyright file="ExpressionAnalysis.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc3;

using SmallC.Cc;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// Results of expression analysis.
/// </summary>
public class ExpressionAnalysis(
    SymbolTableEntry? symbolTableEntry,
    SymbolType? indirectType,
    SymbolType? addressType,
    SymbolType? constantType,
    int constantValue,
    PCode? highestBinaryOp,
    int? stageIndex)
{
    /// <summary>
    /// Gets or sets symbol table entry, if any.
    /// </summary>
    public SymbolTableEntry? SymbolTableEntry { get; set; } = symbolTableEntry;

    /// <summary>
    /// Gets or sets data type of indirectly referenced object, if any.
    /// </summary>
    public SymbolType? IndirectType { get; set; } = indirectType;

    /// <summary>
    /// Gets or sets data type of address, if any.
    /// </summary>
    public SymbolType? AddressType { get; set; } = addressType;

    /// <summary>
    /// Gets or sets type of constant, if any.
    /// </summary>
    public SymbolType? ConstantType { get; set; } = constantType;

    /// <summary>
    /// Gets or sets constant value.
    /// </summary>
    public int ConstantValue { get; set; } = constantValue;

    /// <summary>
    /// Gets p-code of highest binary operator.
    /// </summary>
    public PCode? HighestBinaryOp { get; } = highestBinaryOp;

    /// <summary>
    /// Gets or sets stage index of "oper 0" code, if any.
    /// </summary>
    public int? StageIndex { get; set; } = stageIndex;
}
