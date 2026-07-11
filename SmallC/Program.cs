// <copyright file="Program.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

using SmallC;
using SmallC.Cc;
using SmallC.Cc1;
using SmallC.Cc2;
using SmallC.Cc4;

await Console.Error.WriteLineAsync(Notice.Version).ConfigureAwait(true);
await Console.Error.WriteLineAsync(Notice.CRight1).ConfigureAwait(true);

var storage = new Storage(
    swNext: [],
    swEnd: SwitchTable.SwTabSz - 1,
    stage: null,
    wq: [],
    args: [.. args],
    wqPtr: 0,
    sLast: StagingBuffer.StageSize,
    symTab: new(
        [],
        []),
    litQ: [],
    mac: [],
    pLine: string.Empty,
    mLine: string.Empty);

var misc = new MiscellaneousUseCases(storage);
var symTabMgmt = new SymbolTableUseCases(storage);
var utility = new UtilityUseCases(storage);

var frontend = new FrontEnd(storage);
var parser = new Parser(storage);
var backend = new BackEnd(symTabMgmt, utility, storage);

await misc.AskAsync().ConfigureAwait(true); // get user options
await frontend.OpenFileAsync().ConfigureAwait(true); // and initial input file
await frontend.PreprocessAsync().ConfigureAwait(true); // fetch first line
await backend.HeaderAsync().ConfigureAwait(true); // intro code
backend.SetCodes(); // initialize code pointer array
await parser.ParseAsync().ConfigureAwait(true); // process ALL input
await backend.TrailerAsync().ConfigureAwait(true); // follow-up code
storage.Output.Close(); // explicitly close output
