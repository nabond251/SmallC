// <copyright file="Program.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc4;
using static SmallC.Cc.Storage;
using static SmallC.Cc.SymbolTableEntry;

var storage = new Storage(
    0,
    0,
    0,
    0,
    false,
    Console.Out,
    Console.In,
    true,
    null,
    null,
    null,
    0,
    0,
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
    [],
    string.Empty,
    string.Empty,
    BufferLineType.None,
    0,
    null,
    null);
var symTabMgmt = new SymbolTableUseCases(storage);
var utility = new UtilityUseCases(storage);
var backend = new BackEnd(symTabMgmt, utility, storage);
backend.SetCodes();
await backend.HeaderAsync().ConfigureAwait(true);
await backend.ToSegAsync(SegmentType.CodeSeg).ConfigureAwait(true);
await backend.GenAsync(PCode.POINT1l, 1).ConfigureAwait(true);
await backend.TrailerAsync().ConfigureAwait(true);
