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
    int offset,
    string name)
{
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
    [Flags]
    public enum SymbolType
    {
        /// <summary>
        /// Label - Not applicable.
        /// </summary>
        None,

        /// <summary>
        /// Character data.
        /// </summary>
        Chr = 1 << 2,

        /// <summary>
        /// Integer data.
        /// </summary>
        Int = Machine.Bpw << 2,

        /// <summary>
        /// Unsigned character data.
        /// </summary>
        UChr = (1 << 2) + 1,

        /// <summary>
        /// Unsigned integer data.
        /// </summary>
        UInt = (Machine.Bpw << 2) + 1,

        /// <summary>
        /// Unsigned flag.
        /// </summary>
        Unsigned = 1,
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

    /// <summary>
    /// Gets or sets what the declared entity is.
    /// </summary>
    public SymbolIdentity Ident { get; set; } = ident;

    /// <summary>
    /// Gets or sets the data type.
    /// </summary>
    public SymbolType Type { get; set; } = type;

    /// <summary>
    /// Gets or sets the storage class.
    /// </summary>
    public SymbolClass Class { get; set; } = @class;

    /// <summary>
    /// Gets or sets the number of bytes occupied.
    /// </summary>
    public int Size { get; set; } = size;

    /// <summary>
    /// Gets or sets the numeric value (if applicable).
    /// </summary>
    /// <remarks>
    /// Primarily the stack frame offset for local objects.
    /// Compiler-assigned label number for labels.
    /// </remarks>
    public int Offset { get; set; } = offset;

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; } = name;
}
