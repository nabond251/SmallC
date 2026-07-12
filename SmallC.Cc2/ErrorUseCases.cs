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
    /// Output no #if... error.
    /// </summary>
    public static void NoIfError()
    {
        throw new InvalidOperationException("no matching #if...");
    }
}
