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
    /// Tests that can parse primary.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expected">Expected is expression flag.</param>
    /// <param name="expectedSymbolTableEntry">
    /// Expected symbol table entry.
    /// </param>
    /// <param name="expectedIndirectType">Expected indirect type.</param>
    /// <param name="expectedAddressType">Expected address type.</param>
    /// <param name="expectedConstantType">Expected constant type.</param>
    /// <param name="expectedConstantValue">Expected constant value.</param>
    /// <param name="expectedHighestBinaryOp">
    /// Expected highest binary op.
    /// </param>
    /// <param name="expectedStageIndex">Expected stage index.</param>
    /// <param name="expectedCode">Expected generated code.</param>
    /// <param name="expectedLits">String of expected lit pool bytes.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("", false, null, null, null, null, 0, null, null, "", "")]
    [InlineData("foo", false, null, null, null, null, 0, null, null, "", "")]
    [InlineData("0", false, null, null, null, SymbolType.Int, 0, null, null, "XOR AX,AX\r\n", "")]
    [InlineData("00", false, null, null, null, SymbolType.Int, 0, null, null, "XOR AX,AX\r\n", "")]
    [InlineData("-0", false, null, null, null, SymbolType.Int, 0, null, null, "XOR AX,AX\r\n", "")]
    [InlineData("+0", false, null, null, null, SymbolType.Int, 0, null, null, "XOR AX,AX\r\n", "")]
    [InlineData("-1", false, null, null, null, SymbolType.Int, -1, null, null, "MOV AX,-1\r\n", "")]
    [InlineData("+1", false, null, null, null, SymbolType.Int, 1, null, null, "MOV AX,1\r\n", "")]
    [InlineData("01", false, null, null, null, SymbolType.Int, 1, null, null, "MOV AX,1\r\n", "")]
    [InlineData(" 1", false, null, null, null, SymbolType.Int, 1, null, null, "MOV AX,1\r\n", "")]
    [InlineData("10", false, null, null, null, SymbolType.Int, 10, null, null, "MOV AX,10\r\n", "")]
    [InlineData("-32769", false, null, null, null, SymbolType.Int, 32767, null, null, "MOV AX,32767\r\n", "")]
    [InlineData("-32768", false, null, null, null, SymbolType.Int, -32768, null, null, "MOV AX,-32768\r\n", "")]
    [InlineData("32767", false, null, null, null, SymbolType.Int, 32767, null, null, "MOV AX,32767\r\n", "")]
    [InlineData("32768", false, null, null, null, SymbolType.UInt, 32768, null, null, "MOV AX,32768\r\n", "")]
    [InlineData("65535", false, null, null, null, SymbolType.UInt, 65535, null, null, "MOV AX,65535\r\n", "")]
    [InlineData("65536", false, null, null, null, SymbolType.Int, 0, null, null, "XOR AX,AX\r\n", "")]
    [InlineData("131071", false, null, null, null, SymbolType.UInt, 65535, null, null, "MOV AX,65535\r\n", "")]
    [InlineData("000", false, null, null, null, SymbolType.Int, 0, null, null, "XOR AX,AX\r\n", "")]
    [InlineData("-00", false, null, null, null, SymbolType.Int, 0, null, null, "XOR AX,AX\r\n", "")]
    [InlineData("-01", false, null, null, null, SymbolType.Int, -1, null, null, "MOV AX,-1\r\n", "")]
    [InlineData("001", false, null, null, null, SymbolType.Int, 1, null, null, "MOV AX,1\r\n", "")]
    [InlineData("010", false, null, null, null, SymbolType.Int, 8, null, null, "MOV AX,8\r\n", "")]
    [InlineData("018", false, null, null, null, SymbolType.Int, 1, null, null, "MOV AX,1\r\n", "")]
    [InlineData("077", false, null, null, null, SymbolType.Int, 63, null, null, "MOV AX,63\r\n", "")]
    [InlineData("0777", false, null, null, null, SymbolType.Int, 511, null, null, "MOV AX,511\r\n", "")]
    [InlineData("-0100001", false, null, null, null, SymbolType.Int, 32767, null, null, "MOV AX,32767\r\n", "")]
    [InlineData("-0100000", false, null, null, null, SymbolType.Int, -32768, null, null, "MOV AX,-32768\r\n", "")]
    [InlineData("077777", false, null, null, null, SymbolType.Int, 32767, null, null, "MOV AX,32767\r\n", "")]
    [InlineData("0100000", false, null, null, null, SymbolType.UInt, 32768, null, null, "MOV AX,32768\r\n", "")]
    [InlineData("0177777", false, null, null, null, SymbolType.UInt, 65535, null, null, "MOV AX,65535\r\n", "")]
    [InlineData("0200000", false, null, null, null, SymbolType.Int, 0, null, null, "XOR AX,AX\r\n", "")]
    [InlineData("0377777", false, null, null, null, SymbolType.UInt, 65535, null, null, "MOV AX,65535\r\n", "")]
    [InlineData("00x0", false, null, null, null, SymbolType.Int, 0, null, null, "XOR AX,AX\r\n", "")]
    [InlineData("-0x00", false, null, null, null, SymbolType.Int, 0, null, null, "XOR AX,AX\r\n", "")]
    [InlineData("-0x01", false, null, null, null, SymbolType.Int, -1, null, null, "MOV AX,-1\r\n", "")]
    [InlineData("0x10", false, null, null, null, SymbolType.Int, 16, null, null, "MOV AX,16\r\n", "")]
    [InlineData("0x1G", false, null, null, null, SymbolType.Int, 1, null, null, "MOV AX,1\r\n", "")]
    [InlineData("0xFF", false, null, null, null, SymbolType.Int, 255, null, null, "MOV AX,255\r\n", "")]
    [InlineData("0xFG", false, null, null, null, SymbolType.Int, 15, null, null, "MOV AX,15\r\n", "")]
    [InlineData("-0x8001", false, null, null, null, SymbolType.Int, 32767, null, null, "MOV AX,32767\r\n", "")]
    [InlineData("-0x8000", false, null, null, null, SymbolType.Int, -32768, null, null, "MOV AX,-32768\r\n", "")]
    [InlineData("0x7FFF", false, null, null, null, SymbolType.Int, 32767, null, null, "MOV AX,32767\r\n", "")]
    [InlineData("0x8000", false, null, null, null, SymbolType.UInt, 32768, null, null, "MOV AX,32768\r\n", "")]
    [InlineData("0xFFFF", false, null, null, null, SymbolType.UInt, 65535, null, null, "MOV AX,65535\r\n", "")]
    [InlineData("0x10000", false, null, null, null, SymbolType.Int, 0, null, null, "XOR AX,AX\r\n", "")]
    [InlineData("0x1FFFF", false, null, null, null, SymbolType.UInt, 65535, null, null, "MOV AX,65535\r\n", "")]
    [InlineData("''", false, null, null, null, SymbolType.Int, 0, null, null, "XOR AX,AX\r\n", "")]
    [InlineData("'a'", false, null, null, null, SymbolType.Int, 'a', null, null, "MOV AX,97\r\n", "")]
    [InlineData(" 'a'", false, null, null, null, SymbolType.Int, 'a', null, null, "MOV AX,97\r\n", "")]
    [InlineData("'\\\\'", false, null, null, null, SymbolType.Int, '\\', null, null, "MOV AX,92\r\n", "")]
    [InlineData("'\\n'", false, null, null, null, SymbolType.Int, '\n', null, null, "MOV AX,10\r\n", "")]
    [InlineData("'\\t'", false, null, null, null, SymbolType.Int, '\t', null, null, "MOV AX,9\r\n", "")]
    [InlineData("'\\b'", false, null, null, null, SymbolType.Int, '\b', null, null, "MOV AX,8\r\n", "")]
    [InlineData("'\\f'", false, null, null, null, SymbolType.Int, '\f', null, null, "MOV AX,12\r\n", "")]
    [InlineData("'\\0'", false, null, null, null, SymbolType.Int, '\0', null, null, "XOR AX,AX\r\n", "")]
    [InlineData("'\\1'", false, null, null, null, SymbolType.Int, (char)1, null, null, "MOV AX,1\r\n", "")]
    [InlineData("'\\9'", false, null, null, null, SymbolType.Int, '9', null, null, "MOV AX,57\r\n", "")]
    [InlineData("'\\12'", false, null, null, null, SymbolType.Int, (char)10, null, null, "MOV AX,10\r\n", "")]
    [InlineData("'\\123'", false, null, null, null, SymbolType.Int, (char)83, null, null, "MOV AX,83\r\n", "")]
    [InlineData("'\\1234'", false, null, null, null, SymbolType.Int, ((char)83 << 8) + '4', null, null, "MOV AX,21300\r\n", "")]
    [InlineData("'12'", false, null, null, null, SymbolType.Int, ('1' << 8) + '2', null, null, "MOV AX,12594\r\n", "")]
    [InlineData("'123'", false, null, null, null, SymbolType.Int, ('2' << 8) + '3', null, null, "MOV AX,12851\r\n", "")]
    [InlineData("\"\"", false, null, null, null, null, 0, null, null, "MOV AX,OFFSET _0+0\r\n", "")]
    [InlineData("\"a\"", false, null, null, null, null, 0, null, null, "MOV AX,OFFSET _0+0\r\n", "a")]
    [InlineData(" \"a\"", false, null, null, null, null, 0, null, null, "MOV AX,OFFSET _0+0\r\n", "a")]
    [InlineData("\"abc\"", false, null, null, null, null, 0, null, null, "MOV AX,OFFSET _0+0\r\n", "abc")]
    public async Task ParsesPrimaryAsync(
        string inputText,
        bool expected,
        SymbolTableEntry? expectedSymbolTableEntry,
        SymbolType? expectedIndirectType,
        SymbolType? expectedAddressType,
        SymbolType? expectedConstantType,
        int expectedConstantValue,
        PCode? expectedHighestBinaryOp,
        int? expectedStageIndex,
        string expectedCode,
        string expectedLits)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, backEnd, storage) = Arrange(output: output, input: input);
        var @is = new Expression();
        var (before, start) = backEnd.SetStage();

        var actual = await sut.PrimaryAsync(@is);
        await backEnd.ClearStageAsync(before, start);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actualOutput = await reader.ReadToEndAsync();

        Assert.Equal(expected, actual);
        Assert.Equal(expectedSymbolTableEntry, @is.SymbolTableEntry);
        Assert.Equal(expectedIndirectType, @is.IndirectType);
        Assert.Equal(expectedAddressType, @is.AddressType);
        Assert.Equal(expectedConstantType, @is.ConstantType);
        Assert.Equal(expectedConstantValue, @is.ConstantValue);
        Assert.Equal(expectedHighestBinaryOp, @is.HighestBinaryOp);
        Assert.Equal(expectedStageIndex, @is.StageIndex);
        Assert.Equal(expectedCode, actualOutput);
        Assert.All(expectedLits, (lit, litPtr) =>
        {
            Assert.Equal((sbyte)lit, storage.LitQ[litPtr]);
        });
    }

    private static (Analyzer Sut, BackEnd BackEnd, Storage Storage) Arrange(
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
        backEnd.SetCodes();
        var sut = new Analyzer(symTabMgmt, utility, frontEnd, backEnd, storage);

        return (sut, backEnd, storage);
    }
}
