// <copyright file="AnalyzerTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc3.Tests;

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc4;
using System.Collections.ObjectModel;
using System.Text;
using static SmallC.Cc.Storage;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// Tests the expression analyzer.
/// </summary>
public class AnalyzerTests
{
    /// <summary>
    /// Tests that can parse number.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expectedType">Expected type of character constant.</param>
    /// <param name="expectedValue">Expected number.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("", null, 0)]
    [InlineData("if", null, 0)]
    [InlineData("0", SymbolType.Int, 0)]
    [InlineData("00", SymbolType.Int, 0)]
    [InlineData("-0", SymbolType.Int, 0)]
    [InlineData("+0", SymbolType.Int, 0)]
    [InlineData("-1", SymbolType.Int, -1)]
    [InlineData("+1", SymbolType.Int, 1)]
    [InlineData("01", SymbolType.Int, 1)]
    [InlineData(" 1", SymbolType.Int, 1)]
    [InlineData("10", SymbolType.Int, 10)]
    [InlineData("-32769", SymbolType.Int, 32767)]
    [InlineData("-32768", SymbolType.Int, -32768)]
    [InlineData("32767", SymbolType.Int, 32767)]
    [InlineData("32768", SymbolType.UInt, 32768)]
    [InlineData("65535", SymbolType.UInt, 65535)]
    [InlineData("65536", SymbolType.Int, 0)]
    [InlineData("131071", SymbolType.UInt, 65535)]
    [InlineData("000", SymbolType.Int, 0)]
    [InlineData("-00", SymbolType.Int, 0)]
    [InlineData("-01", SymbolType.Int, -1)]
    [InlineData("001", SymbolType.Int, 1)]
    [InlineData("010", SymbolType.Int, 8)]
    [InlineData("018", SymbolType.Int, 1)]
    [InlineData("077", SymbolType.Int, 63)]
    [InlineData("0777", SymbolType.Int, 511)]
    [InlineData("-0100001", SymbolType.Int, 32767)]
    [InlineData("-0100000", SymbolType.Int, -32768)]
    [InlineData("077777", SymbolType.Int, 32767)]
    [InlineData("0100000", SymbolType.UInt, 32768)]
    [InlineData("0177777", SymbolType.UInt, 65535)]
    [InlineData("0200000", SymbolType.Int, 0)]
    [InlineData("0377777", SymbolType.UInt, 65535)]
    [InlineData("00x0", SymbolType.Int, 0)]
    [InlineData("-0x00", SymbolType.Int, 0)]
    [InlineData("-0x01", SymbolType.Int, -1)]
    [InlineData("0x10", SymbolType.Int, 16)]
    [InlineData("0x1G", SymbolType.Int, 1)]
    [InlineData("0xFF", SymbolType.Int, 255)]
    [InlineData("0xFG", SymbolType.Int, 15)]
    [InlineData("-0x8001", SymbolType.Int, 32767)]
    [InlineData("-0x8000", SymbolType.Int, -32768)]
    [InlineData("0x7FFF", SymbolType.Int, 32767)]
    [InlineData("0x8000", SymbolType.UInt, 32768)]
    [InlineData("0xFFFF", SymbolType.UInt, 65535)]
    [InlineData("0x10000", SymbolType.Int, 0)]
    [InlineData("0x1FFFF", SymbolType.UInt, 65535)]
    public async Task ParsesNumberAsync(
        string inputText,
        SymbolType? expectedType,
        int expectedValue)
    {
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, _, _) = Arrange(input: input);

        var (actualType, actualValue) = await sut.NumberAsync(0);

        Assert.Equal(expectedType, actualType);
        Assert.Equal(expectedValue, actualValue);
    }

    /// <summary>
    /// Tests that can parse character constant.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expectedType">Expected type of character constant.</param>
    /// <param name="expectedValue">Expected character constant.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("", null, 0)]
    [InlineData("if", null, 0)]
    [InlineData("''", SymbolType.Int, 0)]
    [InlineData("'a'", SymbolType.Int, 'a')]
    [InlineData(" 'a'", SymbolType.Int, 'a')]
    [InlineData("'\\\\'", SymbolType.Int, '\\')]
    [InlineData("'\\n'", SymbolType.Int, '\n')]
    [InlineData("'\\t'", SymbolType.Int, '\t')]
    [InlineData("'\\b'", SymbolType.Int, '\b')]
    [InlineData("'\\f'", SymbolType.Int, '\f')]
    [InlineData("'\\0'", SymbolType.Int, '\0')]
    [InlineData("'\\1'", SymbolType.Int, (char)1)]
    [InlineData("'\\9'", SymbolType.Int, '9')]
    [InlineData("'\\12'", SymbolType.Int, (char)10)]
    [InlineData("'\\123'", SymbolType.Int, (char)83)]
    [InlineData("'\\1234'", SymbolType.Int, ((char)83 << 8) + '4')]
    [InlineData("'12'", SymbolType.Int, ('1' << 8) + '2')]
    [InlineData("'123'", SymbolType.Int, ('2' << 8) + '3')]
    public async Task ParsesCharacterConstantAsync(
        string inputText,
        SymbolType? expectedType,
        int expectedValue)
    {
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, _, _) = Arrange(input: input);

        var (actualType, actualValue) = await sut.ChrConAsync(0);

        Assert.Equal(expectedType, actualType);
        Assert.Equal(expectedValue, actualValue);
    }

    /// <summary>
    /// Tests that can parse character constant.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expected">Expected lit pool offset.</param>
    /// <param name="expectedLits">String of expected lit pool bytes.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("", null, "")]
    [InlineData("if", null, "")]
    [InlineData("\"\"", 0, "")]
    [InlineData("\"a\"", 0, "a")]
    [InlineData(" \"a\"", 0, "a")]
    [InlineData("\"abc\"", 0, "abc")]
    public async Task ParsesStringAsync(
        string inputText,
        int? expected,
        string expectedLits)
    {
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, _, storage) = Arrange(input: input);

        var actual = await sut.StringAsync();

        Assert.Equal(expected, actual);
        Assert.All(expectedLits, (lit, litPtr) =>
        {
            Assert.Equal((sbyte)lit, storage.LitQ[litPtr]);
        });
    }

    /// <summary>
    /// Tests that can parse constant primary.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expected">Expected lit pool offset.</param>
    /// <param name="expectedConstantType">Expected constant type.</param>
    /// <param name="expectedConstantValue">Expected constant value.</param>
    /// <param name="expectedPCode">Expected generated p-code.</param>
    /// <param name="expectedCodeValue">Expected generated code value.</param>
    /// <param name="expectedLits">String of expected lit pool bytes.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("", false, null, 0, PCode.None, 0, "")]
    [InlineData("if", false, null, 0, PCode.None, 0, "")]
    [InlineData("0xFFFF", true, SymbolType.UInt, 65535, PCode.GETw1n, 65535, "")]
    [InlineData("'a'", true, SymbolType.Int, 'a', PCode.GETw1n, 'a', "")]
    [InlineData("\"abc\"", true, null, 0, PCode.POINT1l, 0, "abc")]
    public async Task ParsesConstantAsync(
        string inputText,
        bool expected,
        SymbolType? expectedConstantType,
        int expectedConstantValue,
        PCode expectedPCode,
        int expectedCodeValue,
        string expectedLits)
    {
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, backEnd, storage) = Arrange(input: input);
        var @is = new Expression();
        _ = backEnd.SetStage();

        var actual = await sut.ConstantAsync(@is);

        Assert.Equal(expected, actual);
        Assert.Equal(expectedConstantType, @is.ConstantType);
        Assert.Equal(expectedConstantValue, @is.ConstantValue);
        Assert.Equal(expectedPCode, storage.Stage?.FirstOrDefault().Key);
        Assert.Equal(expectedCodeValue, storage.Stage?.FirstOrDefault().Value);
        Assert.All(expectedLits, (lit, litPtr) =>
        {
            Assert.Equal((sbyte)lit, storage.LitQ[litPtr]);
        });
    }

    private static (
        Analyzer Sut,
        BackEnd BackEnd,
        Storage Storage)
        Arrange(
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

        return (sut, backEnd, storage);
    }
}
