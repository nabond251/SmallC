// <copyright file="WhileQueueEntry.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc;

/// <summary>
/// While queue entry.
/// </summary>
public record class WhileQueueEntry(
    int StackPointer,
    int LoopLabel,
    int ExitLabel);
