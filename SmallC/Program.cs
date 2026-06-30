// <copyright file="Program.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

using SmallC.Cc;
using SmallC.Cc4;
using static SmallC.Cc.SymbolTableEntry;

var storage = new Storage(
    Console.Out,
    null,
    SegmentType.None,
    new(
        [],
        [
            new(SymbolIdentity.Function, SymbolType.Int, SymbolClass.AutoExt, 2, null, "func"),
            new(SymbolIdentity.Function, SymbolType.Int, SymbolClass.Static, 2, null, "main"),
        ]),
    null);
var backend = new BackEnd(storage);
backend.SetSeq();
await backend.HeaderAsync().ConfigureAwait(true);
await backend.TrailerAsync().ConfigureAwait(true);
