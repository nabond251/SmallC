// <copyright file="AnalyzerTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc3.Tests;

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc4;
using System.Collections.ObjectModel;
using static SmallC.Cc.Storage;
using static SmallC.Cc.SymbolTableEntry;

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
    /// <param name="expectedCh">
    /// Expected current character of input line.
    /// </param>
    /// <param name="expectedNCh">Expected next character of input line.</param>
    /// <param name="expectedLPtr">
    /// Expected index to <see cref="Storage.Line"/>.
    /// </param>
    [Theory]
    [InlineData(null, null, "", 0, null, null, null, 0)]
    [InlineData('a', null, "a", 0, 'a', null, null, 1)]
    [InlineData(null, null, "a", 1, null, null, null, 1)]
    [InlineData(' ', 'a', " a", 0, ' ', 'a', null, 1)]
    [InlineData('a', null, " a", 1, 'a', null, null, 2)]
    [InlineData(null, null, " a", 2, null, null, null, 2)]
    [InlineData('\\', null, "\\", 0, '\\', null, null, 1)]
    [InlineData('\\', 'n', "\\n", 0, '\n', null, null, 2)]
    [InlineData('\\', 't', "\\t", 0, '\t', null, null, 2)]
    [InlineData('\\', 'b', "\\b", 0, '\b', null, null, 2)]
    [InlineData('\\', 'f', "\\f", 0, '\f', null, null, 2)]
    [InlineData('\\', '0', "\\0", 0, '\0', null, null, 2)]
    [InlineData('\\', '1', "\\1", 0, (char)1, null, null, 2)]
    [InlineData('\\', '1', "\\9", 0, '9', null, null, 2)]
    [InlineData('\\', '1', "\\12", 0, (char)10, null, null, 3)]
    [InlineData('\\', '1', "\\123", 0, (char)83, null, null, 4)]
    [InlineData('\\', '1', "\\1234", 0, (char)83, '4', null, 4)]
    public void GetsCharacterLiteral(
        char? ch,
        char? nCh,
        string pLine,
        int lPtr,
        char? expected,
        char? expectedCh,
        char? expectedNCh,
        int expectedLPtr)
    {
        var (sut, storage) =
            Arrange(ch: ch, nCh: nCh, pLine: pLine, lPtr: lPtr);

        var actual = sut.LitChar();

        Assert.Equal(expected, actual);
        Assert.Equal(expectedCh, storage.Ch);
        Assert.Equal(expectedNCh, storage.NCh);
        Assert.Equal(expectedLPtr, storage.LPtr);
    }

    /// <summary>
    /// Tests that can parse character constant.
    /// </summary>
    /// <param name="ch">Current character of input line.</param>
    /// <param name="nCh">Next character of input line.</param>
    /// <param name="pLine">Parsing buffer.</param>
    /// <param name="lPtr">Index to <see cref="Storage.Line"/>.</param>
    /// <param name="expectedType">Expected type of character constant.</param>
    /// <param name="expectedValue">Expected character constant.</param>
    /// <param name="expectedCh">
    /// Expected current character of input line.
    /// </param>
    /// <param name="expectedNCh">Expected next character of input line.</param>
    /// <param name="expectedLPtr">
    /// Expected index to <see cref="Storage.Line"/>.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(null, null, "''", 0, null, 0, null, null, 0)]
    [InlineData('a', null, "'a'", 0, SymbolType.Int, 'a', null, null, 3)]
    [InlineData(null, null, "'a'", 3, null, 0, null, null, 0)]
    [InlineData(' ', 'a', " 'a'", 0, SymbolType.Int, 'a', null, null, 4)]
    [InlineData('a', null, " 'a'", 1, SymbolType.Int, 'a', null, null, 4)]
    [InlineData(null, null, " 'a'", 4, null, 0, null, null, 0)]
    [InlineData('\\', '\\', "'\\\\'", 0, SymbolType.Int, '\\', null, null, 4)]
    [InlineData('\\', 'n', "'\\n'", 0, SymbolType.Int, '\n', null, null, 4)]
    [InlineData('\\', 't', "'\\t'", 0, SymbolType.Int, '\t', null, null, 4)]
    [InlineData('\\', 'b', "'\\b'", 0, SymbolType.Int, '\b', null, null, 4)]
    [InlineData('\\', 'f', "'\\f'", 0, SymbolType.Int, '\f', null, null, 4)]
    [InlineData('\\', '0', "'\\0'", 0, SymbolType.Int, '\0', null, null, 4)]
    [InlineData('\\', '1', "'\\1'", 0, SymbolType.Int, (char)1, null, null, 4)]
    [InlineData('\\', '1', "'\\9'", 0, SymbolType.Int, '9', null, null, 4)]
    [InlineData('\\', '1', "'\\12'", 0, SymbolType.Int, (char)10, null, null, 5)]
    [InlineData('\\', '1', "'\\123'", 0, SymbolType.Int, (char)83, null, null, 6)]
    [InlineData('\\', '1', "'\\1234'", 0, SymbolType.Int, ((char)83 << 8) + '4', null, null, 7)]
    public async Task ParsesCharacterConstantAsync(
        char? ch,
        char? nCh,
        string pLine,
        int lPtr,
        SymbolType? expectedType,
        int expectedValue,
        char? expectedCh,
        char? expectedNCh,
        int expectedLPtr)
    {
        using var input = StreamReader.Null;
        var (sut, storage) =
            Arrange(ch: ch, nCh: nCh, input: input, pLine: pLine, lPtr: lPtr);

        var (actualType, actualValue) = await sut.ChrConAsync(0);

        Assert.Equal(expectedType, actualType);
        Assert.Equal(expectedValue, actualValue);
        Assert.Equal(expectedCh, storage.Ch);
        Assert.Equal(expectedNCh, storage.NCh);
        Assert.Equal(expectedLPtr, storage.LPtr);
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
