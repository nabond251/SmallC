// <copyright file="GlobalParserTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc1.Tests;

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc3;
using SmallC.Cc4;
using System.Collections.ObjectModel;
using System.Text;
using static SmallC.Cc.Storage;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// Tests the global parser.
/// </summary>
public class GlobalParserTests
{
    /// <summary>
    /// Tests parser.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expected">Expected parsing line.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("", "")]
    [InlineData(" ", " \r\n")]
    [InlineData("TEST", "TEST\r\n")]
    [InlineData(" ", "")]
    [InlineData("  ", "")]
    [InlineData("\r\n", "")]
    [InlineData("\r\n ", "")]
    [InlineData(" \r\n", "")]
    [InlineData("\r\n  ", "")]
    [InlineData(" \r\n ", "")]
    [InlineData("  \r\n", "")]
    [InlineData("\"\"", "\"\"")]
    [InlineData("\"test\"", "\"test\"")]
    [InlineData("\"test\"\r\n", "\"test\"")]
    [InlineData("''", "''")]
    [InlineData("'t'", "'t'")]
    [InlineData("/**/", "")]
    [InlineData("/* test */", "")]
    [InlineData("foo/*bar*/baz", "foobaz")]
    [InlineData("foo/*bar\r\nbaz\r\nquux", "foo")]
    [InlineData("foo/*bar*/baz\r\nquux", "foobaz")]
    [InlineData("foo/*bar\r\nbaz*/quux", "fooquux")]
    [InlineData("FOO", "BAR")]
    [InlineData("FOOBARBAZ", "QUUX")]
    public async Task CanParseAsync(
        string inputText, string? expected)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var mac = new Dictionary<string, string>
        {
            { "FOO", "BAR" },
            { "FOOBARBA", "QUUX" },
        };
        var (sut, _, storage) = Arrange(
            output: output, input: input, mac: mac);

        await sut.ParseAsync();
        var actual = storage.PLine;

        Assert.Equal(expected, actual);
    }

    private static (GlobalParser Sut, BackEnd BackEnd, Storage Storage) Arrange(
        Collection<KeyValuePair<PCode, int>>? stage = null,
        char? ch = null,
        char? nCh = null,
        StreamWriter? output = null,
        StreamReader? input = null,
        bool cCode = true,
        SegmentType oldSeg = SegmentType.None,
        SymbolTable? symTab = null,
        Collection<sbyte>? litQ = null,
        Dictionary<string, string>? mac = null,
        string? pLine = null,
        BufferLineType? lineType = null,
        int? lPtr = null,
        string? ssName = null)
    {
        var storage = new Storage(
            stage: stage,
            ch: ch,
            nCh: nCh,
            output: output,
            files: input != null,
            input: input,
            cCode: cCode,
            oldSeg: oldSeg,
            symTab: symTab ?? new([], []),
            litQ: litQ ?? [],
            mac: mac ?? [],
            pLine: pLine,
            lineType: lineType ?? BufferLineType.Parsing,
            lPtr: lPtr,
            ssName: ssName);

        var symTabMgmt = new SymbolTableUseCases(storage);
        var utility = new UtilityUseCases(storage);
        var whileQueueMgmt = new WhileQueueUseCases(utility, storage);
        _ = symTabMgmt.AddSym(
            "c",
            SymbolIdentity.Variable,
            SymbolType.Chr,
            1,
            -10,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "ca3",
            SymbolIdentity.Array,
            SymbolType.Chr,
            3,
            -8,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "cp",
            SymbolIdentity.Pointer,
            SymbolType.Chr,
            2,
            -6,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "uc",
            SymbolIdentity.Variable,
            SymbolType.UChr,
            1,
            -4,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "uca3",
            SymbolIdentity.Array,
            SymbolType.UChr,
            3,
            -2,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "ucp",
            SymbolIdentity.Pointer,
            SymbolType.UChr,
            2,
            0,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "i",
            SymbolIdentity.Variable,
            SymbolType.Int,
            2,
            2,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "ia3",
            SymbolIdentity.Array,
            SymbolType.Int,
            6,
            4,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "ip",
            SymbolIdentity.Pointer,
            SymbolType.Int,
            2,
            6,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "ui",
            SymbolIdentity.Variable,
            SymbolType.UInt,
            2,
            8,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "uia3",
            SymbolIdentity.Array,
            SymbolType.UInt,
            6,
            10,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "uip",
            SymbolIdentity.Pointer,
            SymbolType.UInt,
            2,
            12,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "gc",
            SymbolIdentity.Variable,
            SymbolType.Chr,
            1,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "gca3",
            SymbolIdentity.Array,
            SymbolType.Chr,
            3,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "gcp",
            SymbolIdentity.Pointer,
            SymbolType.Chr,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "guc",
            SymbolIdentity.Variable,
            SymbolType.UChr,
            1,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "guca3",
            SymbolIdentity.Array,
            SymbolType.UChr,
            3,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "gucp",
            SymbolIdentity.Pointer,
            SymbolType.UChr,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "gi",
            SymbolIdentity.Variable,
            SymbolType.Int,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "gia3",
            SymbolIdentity.Array,
            SymbolType.Int,
            6,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "gip",
            SymbolIdentity.Pointer,
            SymbolType.Int,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "gui",
            SymbolIdentity.Variable,
            SymbolType.UInt,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "guia3",
            SymbolIdentity.Array,
            SymbolType.UInt,
            6,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "guip",
            SymbolIdentity.Pointer,
            SymbolType.UInt,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "ec",
            SymbolIdentity.Variable,
            SymbolType.Chr,
            1,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "eca3",
            SymbolIdentity.Array,
            SymbolType.Chr,
            3,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "ecp",
            SymbolIdentity.Pointer,
            SymbolType.Chr,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "euc",
            SymbolIdentity.Variable,
            SymbolType.UChr,
            1,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "euca3",
            SymbolIdentity.Array,
            SymbolType.UChr,
            3,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "eucp",
            SymbolIdentity.Pointer,
            SymbolType.UChr,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "ei",
            SymbolIdentity.Variable,
            SymbolType.Int,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "eia3",
            SymbolIdentity.Array,
            SymbolType.Int,
            6,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "eip",
            SymbolIdentity.Pointer,
            SymbolType.Int,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "eui",
            SymbolIdentity.Variable,
            SymbolType.UInt,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "euia3",
            SymbolIdentity.Array,
            SymbolType.UInt,
            6,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "euip",
            SymbolIdentity.Pointer,
            SymbolType.UInt,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "foo",
            SymbolIdentity.Function,
            SymbolType.Int,
            0,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);

        var frontEnd = new FrontEnd(storage);
        var backEnd = new BackEnd(symTabMgmt, utility, storage);
        var analyzer = new Analyzer(symTabMgmt, utility, frontEnd, backEnd, storage);
        var localParser = new LocalParser(
            symTabMgmt, utility, whileQueueMgmt, frontEnd, analyzer, backEnd, storage);
        backEnd.SetCodes();

        var sut = new GlobalParser(
            symTabMgmt, utility, frontEnd, localParser, analyzer, backEnd, storage);

        return (sut, backEnd, storage);
    }
}
