// <copyright file="BackEndTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc4.Tests;

using SmallC.Cc;
using SmallC.Cc4;
using System.Globalization;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// Tests the back end functions.
/// </summary>
public class BackEndTests
{
    private const string BeginData = "DATA SEGMENT PUBLIC\r\n";
    private const string EndData = "DATA ENDS\r\n";
    private const string BeginCode = "CODE SEGMENT PUBLIC\r\nASSUME CS:CODE, SS:DATA, DS:DATA\r\n";
    private const string EndCode = "CODE ENDS\r\n";

    /// <summary>
    /// Tests that header is generated.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GeneratesHeaderAsync()
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var storage = new Storage(output, SegmentType.None, new([], []));
        var sut = new BackEnd(storage);

        await sut.HeaderAsync();
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        var expected = $@"{BeginCode}extrn __eq: near
extrn __ne: near
extrn __le: near
extrn __lt: near
extrn __ge: near
extrn __gt: near
extrn __ule: near
extrn __ult: near
extrn __uge: near
extrn __ugt: near
extrn __lneg: near
extrn __switch: near
dw 0
{EndCode}{BeginData}dw 0
";
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that trailer is generated.
    /// </summary>
    /// <param name="extFuncs">Comma-separated list of external funcs.</param>
    /// <param name="hasMain">A value indicating whether main exists.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("", false)]
    [InlineData("foo,bar,baz", false)]
    [InlineData("", true)]
    public async Task GeneratesTrailerAsync(
        string extFuncs,
        bool hasMain)
    {
        ArgumentNullException.ThrowIfNull(extFuncs);
        var funcs = extFuncs.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var globals = funcs.Select(f => new SymbolTableEntry(
            SymbolIdentity.Function,
            SymbolType.Int,
            SymbolClass.AutoExt,
            0,
            null,
            f));
        var globalsAndAnyMain = hasMain ? globals.Append(new(
            SymbolIdentity.Function,
            SymbolType.Int,
            SymbolClass.Static,
            0,
            null,
            "main")) : globals;
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var storage = new Storage(
            output, SegmentType.None, new([], [.. globalsAndAnyMain]));
        var sut = new BackEnd(storage);

        await sut.TrailerAsync();
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        var funcsAndAnyMain = hasMain ? funcs.Append("_main") : funcs;
        var externs = string.Join("\r\n", funcsAndAnyMain.Select(
            f => $"EXTRN _{f.ToUpper(CultureInfo.InvariantCulture)}:NEAR"));
        var codeSeg = string.IsNullOrEmpty(externs) ? externs :
            $"{BeginCode}{externs}\r\n{EndCode}";
        var expected = $"{codeSeg}END\r\n";
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that can transition between segments.
    /// </summary>
    /// <param name="oldSeg">Segment to change from.</param>
    /// <param name="newSeg">Segment to change to.</param>
    /// <param name="expected">Expected output.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(SegmentType.None, SegmentType.None, "")]
    [InlineData(SegmentType.DataSeg, SegmentType.DataSeg, $"{BeginData}{EndData}")]
    [InlineData(SegmentType.DataSeg, SegmentType.CodeSeg, $"{BeginData}{EndData}{BeginCode}{EndCode}")]
    [InlineData(SegmentType.CodeSeg, SegmentType.CodeSeg, $"{BeginCode}{EndCode}")]
    [InlineData(SegmentType.CodeSeg, SegmentType.DataSeg, $"{BeginCode}{EndCode}{BeginData}{EndData}")]
    public async Task TransitionsSegmentAsync(
        SegmentType oldSeg, SegmentType newSeg, string expected)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var storage = new Storage(output, SegmentType.None, new([], []));
        var sut = new BackEnd(storage);

        await sut.ToSegAsync(oldSeg);
        await sut.ToSegAsync(newSeg);
        await sut.ToSegAsync(SegmentType.None);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        Assert.Equal(expected, actual);
    }
}
