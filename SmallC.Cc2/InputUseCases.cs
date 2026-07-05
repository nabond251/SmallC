// <copyright file="InputUseCases.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc2;

using SmallC.Cc;

/// <summary>
/// Input use cases.
/// </summary>
public class InputUseCases(Storage storage)
{
    /// <summary>
    /// Place character into parsing buffer.
    /// </summary>
    /// <param name="c">Character to keep.</param>
    public void KeepCh(char c)
    {
        if (storage.PPtr < InputLine.LineMax)
        {
            storage.PLine += c;
        }
    }
}
