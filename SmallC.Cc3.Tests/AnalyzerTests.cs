// <copyright file="AnalyzerTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc3.Tests;

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc4;
using System.Collections.ObjectModel;
using static SmallC.Cc.Storage;

/// <summary>
/// Tests the expression analyzer.
/// </summary>
public class AnalyzerTests
{
    /// <summary>
    /// Tests that can get character literal.
    /// </summary>
    /// <param name="ch">Current character of input line.</param>
    /// <param name="nCh">Next character of input line.</param>
    /// <param name="pLine">Parsing buffer.</param>
    /// <param name="lPtr">Index to <see cref="Storage.Line"/>.</param>
    /// <param name="expected">Expected character literal.</param>
    [Theory]
    [InlineData(null, null, "", 0, null)]
    [InlineData(' ', null, " ", 0, ' ')]
    [InlineData(' ', ' ', "  ", 0, ' ')]
    public void GetsCharacterLiteral(
        char? ch, char? nCh, string pLine, int lPtr, char? expected)
    {
        var (sut, _) = Arrange(ch: ch, nCh: nCh, pLine: pLine, lPtr: lPtr);

        var actual = sut.LitChar();

        Assert.Equal(expected, actual);
    }

    private static (Analyzer Sut, Storage Storage) Arrange(
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

        var frontEnd = new FrontEnd(storage);
        var backEnd = new BackEnd(symTabMgmt, utility, storage);
        var sut = new Analyzer(symTabMgmt, utility, frontEnd, backEnd, storage);

        return (sut, storage);
    }
}
