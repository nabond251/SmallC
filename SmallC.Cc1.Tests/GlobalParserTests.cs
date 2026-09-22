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
#pragma warning disable SA1118 // Parameter should not span multiple lines
    [InlineData(
@"int
 gi;",
@"DATA SEGMENT PUBLIC
PUBLIC _GI
_GI DW 1 DUP(0)
")]
#pragma warning restore SA1118 // Parameter should not span multiple lines
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
        var (sut, _, _) = Arrange(
            output: output, input: input, mac: mac);

        await sut.ParseAsync();
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

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
