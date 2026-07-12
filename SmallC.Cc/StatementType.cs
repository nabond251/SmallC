// <copyright file="StatementType.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc;

/// <summary>
/// Statement types enumeration.
/// </summary>
public enum StatementType
{
    /// <summary>
    /// No statement.
    /// </summary>
    None,

    /// <summary>
    /// If statement.
    /// </summary>
    If,

    /// <summary>
    /// While statement.
    /// </summary>
    While,

    /// <summary>
    /// Return statement.
    /// </summary>
    Return,

    /// <summary>
    /// Break statement.
    /// </summary>
    Break,

    /// <summary>
    /// Continue statement.
    /// </summary>
    Cont,

    /// <summary>
    /// Assembly statement.
    /// </summary>
    Asm,

    /// <summary>
    /// Expression statement.
    /// </summary>
    Expr,

    /// <summary>
    /// Do statement.
    /// </summary>
    Do,

    /// <summary>
    /// For statement.
    /// </summary>
    For,

    /// <summary>
    /// Switch statement.
    /// </summary>
    Switch,

    /// <summary>
    /// Case statement.
    /// </summary>
    Case,

    /// <summary>
    /// Definition statement.
    /// </summary>
    Def,

    /// <summary>
    /// Goto statement.
    /// </summary>
    Goto,

    /// <summary>
    /// Label statement.
    /// </summary>
    Label,
}
