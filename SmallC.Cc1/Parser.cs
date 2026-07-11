// <copyright file="Parser.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc1;

using SmallC.Cc;

/// <summary>
/// High level parser.
/// </summary>
public class Parser(Storage storage)
{
    /// <summary>
    /// Process all input text.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// At this level, only static declarations,
    ///      defines, includes and function
    ///      definitions are legal...
    /// </remarks>
    public Task ParseAsync()
    {
        _ = storage;
        return Task.CompletedTask;
    }
}
