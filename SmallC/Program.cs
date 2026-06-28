// <copyright file="Program.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

using SmallC.Cc;
using SmallC.Cc4;

var storage = new Storage(new(
    [],
    [
        new(SymbolTableEntry.SymbolIdentity.Function, SymbolTableEntry.SymbolType.Int, SymbolTableEntry.SymbolClass.AutoExt, 2, null, "func"),
        new(SymbolTableEntry.SymbolIdentity.Function, SymbolTableEntry.SymbolType.Int, SymbolTableEntry.SymbolClass.Static, 2, null, "main"),
    ]));
var backend = new BackEnd(storage, Console.Out);
await backend.HeaderAsync().ConfigureAwait(true);
await backend.TrailerAsync().ConfigureAwait(true);
