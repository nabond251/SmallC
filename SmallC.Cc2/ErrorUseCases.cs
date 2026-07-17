// <copyright file="ErrorUseCases.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc2;

/// <summary>
/// Error use cases.
/// </summary>
public static class ErrorUseCases
{
    /// <summary>
    /// Output illegal symbol error.
    /// </summary>
    public static void IllName()
    {
        throw new InvalidOperationException("illegal symbol");
    }

    /// <summary>
    /// Output multiple defined error.
    /// </summary>
    /// <param name="sName">Symbol with multiple definitions.</param>
    public static void MultiDef(string sName)
    {
        throw new InvalidOperationException("already defined");
    }

    /// <summary>
    /// Output missing l-value error.
    /// </summary>
    public static void NeedLVal()
    {
        throw new InvalidOperationException("must be lvalue");
    }

    /// <summary>
    /// Output no #if... error.
    /// </summary>
    public static void NoIfError()
    {
        throw new InvalidOperationException("no matching #if...");
    }
}
