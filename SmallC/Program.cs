// <copyright file="Program.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

using SmallC.Cc;
using SmallC.Cc4;
using static SmallC.Cc.SymbolTableEntry;

var storage = new Storage(
    0,
    0,
    Console.Out,
    null,
    0,
    SegmentType.None,
    false,
    new(
        [],
        [
            new(SymbolIdentity.Function, SymbolType.Int, SymbolClass.AutoExt, 2, null, "func"),
            new(SymbolIdentity.Function, SymbolType.Int, SymbolClass.Static, 2, null, "main"),
        ]),
    [],
    null);
var backend = new BackEnd(storage);
backend.SetCodes();
await backend.HeaderAsync().ConfigureAwait(true);
await backend.ToSegAsync(SegmentType.CodeSeg).ConfigureAwait(true);
await backend.GenAsync(PCode.GETb1p, 1).ConfigureAwait(true);
await backend.TrailerAsync().ConfigureAwait(true);
