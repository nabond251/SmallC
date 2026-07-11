// <copyright file="Program.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

using SmallC;
using SmallC.Cc;
using SmallC.Cc1;
using SmallC.Cc2;
using SmallC.Cc4;
using static SmallC.Cc.Storage;

await Console.Error.WriteLineAsync(Notice.Version).ConfigureAwait(true);
await Console.Error.WriteLineAsync(Notice.CRight1).ConfigureAwait(true);

var storage = new Storage(
    0,
    0,
    [],
    SwitchTable.SwTabSz - 1,
    [],
    [],
    [.. args],
    0,
    null,
    null,
    0,
    0,
    0,
    0,
    0,
    false,
    Console.Out,
    false,
    0,
    Console.In,
    null,
    true,
    StagingBuffer.StageSize,
    Console.Out,
    SegmentType.None,
    false,
    false,
    false,
    false,
    new(
        [],
        []),
    [],
    [],
    string.Empty,
    string.Empty,
    BufferLineType.None,
    0,
    null,
    null);
var misc = new MiscellaneousUseCases(storage);
var symTabMgmt = new SymbolTableUseCases(storage);
var utility = new UtilityUseCases(storage);
var frontend = new FrontEnd(storage);
var backend = new BackEnd(symTabMgmt, utility, storage);

await misc.AskAsync().ConfigureAwait(true);
await frontend.OpenFileAsync().ConfigureAwait(true);
backend.SetCodes();
await backend.HeaderAsync().ConfigureAwait(true);
await backend.ToSegAsync(SegmentType.CodeSeg).ConfigureAwait(true);
await frontend.PreprocessAsync().ConfigureAwait(true);
await backend.GenAsync(PCode.POINT1l, 1).ConfigureAwait(true);
await backend.TrailerAsync().ConfigureAwait(true);
