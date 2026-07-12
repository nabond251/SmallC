// <copyright file="SymbolTableEntry.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc;

using System.Diagnostics.CodeAnalysis;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// Symbol table entry.
/// </summary>
[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Literature")]
public class SymbolTableEntry(
    SymbolIdentity ident,
    SymbolType type,
    SymbolClass @class,
    int size,
    int? offset,
    string name)
{
    public SymbolIdentity Ident { get; } = ident;
    public SymbolType Type { get; } = type;
    public SymbolClass Class { get; set; } = @class;
    public int Size { get; } = size;
    public int? Offset { get; } = offset;
    public string Name { get; } = name;

    /// <summary>
    /// Defined values for the <see cref="Ident"/> field.
    /// </summary>
    public enum SymbolIdentity
    {
        /// <summary>
        /// Declared label.
        /// </summary>
        Label,

        /// <summary>
        /// Scalar variable.
        /// </summary>
        Variable,

        /// <summary>
        /// Array of variables.
        /// </summary>
        Array,

        /// <summary>
        /// Pointer.
        /// </summary>
        Pointer,

        /// <summary>
        /// Function.
        /// </summary>
        Function,
    }

    /// <summary>
    /// Defined values for the <see cref="Type"/> field.
    /// </summary>
    public enum SymbolType
    {
        /// <summary>
        /// Not applicable.
        /// </summary>
        Label,

        /// <summary>
        /// Character data.
        /// </summary>
        Chr = 4,

        /// <summary>
        /// Integer data.
        /// </summary>
        Int = 8,

        /// <summary>
        /// Unsigned character data.
        /// </summary>
        UChr = 5,

        /// <summary>
        /// Unsigned integer data.
        /// </summary>
        UInt = 9,
    }

    /// <summary>
    /// Defined values for the <see cref="Class"/> field.
    /// </summary>
    public enum SymbolClass
    {
        /// <summary>
        /// Not applicable.
        /// </summary>
        Label,

        /// <summary>
        /// Automatic storage.
        /// </summary>
        Automatic,

        /// <summary>
        /// Static storage.
        /// </summary>
        Static,

        /// <summary>
        /// Declared external.
        /// </summary>
        External,

        /// <summary>
        /// Assumed external.
        /// </summary>
        AutoExt,
    }
}
