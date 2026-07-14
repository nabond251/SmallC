// <copyright file="WhileQueueUseCases.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc2;

using SmallC.Cc;

/// <summary>
/// While queue management use cases.
/// </summary>
public class WhileQueueUseCases(
    UtilityUseCases utility,
    Storage storage)
{
    /// <summary>
    /// Add while to while queue.
    /// </summary>
    /// <returns>Added while queue entry.</returns>
    public WhileQueueEntry AddWhile()
    {
        var ptr = new WhileQueueEntry(
            storage.Csp,
            utility.GetLabel(),
            utility.GetLabel());
        storage.Wq.Add(ptr);
        return ptr;
    }

    /// <summary>
    /// Remove last while.
    /// </summary>
    public void DelWhile()
    {
        if (storage.WqPtr > 0)
        {
            storage.Wq.RemoveAt(storage.WqPtr - 1);
        }
    }
}
