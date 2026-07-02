// <copyright file="BackEndTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc4.Tests;

using SmallC.Cc;
using SmallC.Cc4;
using System.Collections.ObjectModel;
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
        var sut = Arrange(output);

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
        var sut = Arrange(
            output,
            symbolTable: new([], [.. globalsAndAnyMain]));

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
    /// Tests that can set stage.
    /// </summary>
    /// <param name="setStage">Whether to set stage.</param>
    /// <param name="pCode">P-code to stage.</param>
    /// <param name="value">Value to stage.</param>
    /// <param name="expectedSNext">Expected next index in stage.</param>
    /// <param name="expectedBefore">
    /// Expected new previous position in queue.
    /// </param>
    /// <param name="expectedStart">
    /// Expected new starting position in queue.
    /// </param>
    [Theory]
    [InlineData(false, null, null, 0, null, 0)]
    [InlineData(true, null, null, 0, 0, 0)]
    [InlineData(true, PCode.ADD12, 0, 1, 1, 1)]
    public void SetsStage(
        bool setStage,
        PCode? pCode,
        int? value,
        int? expectedSNext,
        int? expectedBefore,
        int? expectedStart)
    {
        // Arrange
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        Collection<KeyValuePair<PCode, int>>? stage = setStage ? [] : null;
        if (stage is not null && pCode is not null && value is not null)
        {
            stage.Add(new(pCode.Value, value.Value));
        }

        var storage = ArrangeStorage(output, stage);
        var sut = new BackEnd(storage);

        // Act
        var (actualBefore, actualStart) = sut.SetStage();

        // Assert
        Assert.Equal(expectedSNext, storage.SNext);
        Assert.Equal(expectedBefore, actualBefore);
        Assert.Equal(expectedStart, actualStart);
    }

    /// <summary>
    /// Tests that can generate code.
    /// </summary>
    /// <param name="setStage">Whether to set stage.</param>
    /// <param name="pCode">P-code to stage.</param>
    /// <param name="value">Value to stage.</param>
    /// <param name="expectedCsp">Expected compiler relative stk ptr.</param>
    /// <param name="expectedSNext">Expected next index in stage.</param>
    /// <param name="expectedFirstPCode">Expected first staged p-code.</param>
    /// <param name="expectedFirstValue">Expected first staged value.</param>
    /// <param name="expectedLastPCode">Expected last staged p-code.</param>
    /// <param name="expectedLastValue">Expected last staged value.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(false, PCode.ADD12, 0, 2, null, null, null, null, null)]
    [InlineData(true, PCode.ADD12, 0, 2, 1, PCode.ADD12, 0, PCode.ADD12, 0)]
    [InlineData(true, PCode.GETb1pu, 0, 2, 2, PCode.MOVE21, 0, PCode.GETb1pu, 0)]
    [InlineData(true, PCode.SUB12, 0, 2, 2, PCode.SWAP12, 0, PCode.SUB12, 0)]
    [InlineData(true, PCode.PUSH1, 0, 0, 1, PCode.PUSH1, 0, PCode.PUSH1, 0)]
    [InlineData(true, PCode.POP2, 0, 4, 1, PCode.POP2, 0, PCode.POP2, 0)]
    [InlineData(true, PCode.ADDSP, 4, 4, 1, PCode.ADDSP, 2, PCode.ADDSP, 2)]
    public async Task GensAsync(
        bool setStage,
        PCode pCode,
        int value,
        int expectedCsp,
        int? expectedSNext,
        PCode? expectedFirstPCode,
        int? expectedFirstValue,
        PCode? expectedLastPCode,
        int? expectedLastValue)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        Collection<KeyValuePair<PCode, int>>? stage = setStage ? [] : null;
        var storage = ArrangeStorage(output, stage);
        var sut = new BackEnd(storage);

        await sut.GenAsync(pCode, value);

        Assert.Equal(expectedCsp, storage.Csp);
        Assert.Equal(expectedSNext, storage.SNext);
        if (setStage)
        {
            var first = storage.Stage?.FirstOrDefault();
            Assert.Equal(expectedFirstPCode, first?.Key);
            Assert.Equal(expectedFirstValue, first?.Value);
            var last = storage.Stage?.LastOrDefault();
            Assert.Equal(expectedLastPCode, last?.Key);
            Assert.Equal(expectedLastValue, last?.Value);
        }
    }

    /// <summary>
    /// Tests that can clear stage.
    /// </summary>
    /// <param name="setStage">Whether to set stage.</param>
    /// <param name="before">
    /// New previous position in queue.
    /// </param>
    /// <param name="start">
    /// New starting position in queue.
    /// </param>
    /// <param name="expectedSNext">Expected next index in stage.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(false, null, null, null)]
    [InlineData(false, null, 0, null)]
    [InlineData(false, 0, null, null)]
    [InlineData(false, 0, 0, null)]
    [InlineData(true, null, null, null)]
    [InlineData(true, null, 0, null)]
    [InlineData(true, 0, null, 0)]
    [InlineData(true, 0, 0, 0)]
    public async Task ClearsStageAsync(
        bool setStage,
        int? before,
        int? start,
        int? expectedSNext)
    {
        // Arrange
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        Collection<KeyValuePair<PCode, int>>? stage = setStage ? [] : null;
        stage?.Add(new(PCode.ADD12, 0));

        var storage = ArrangeStorage(output, stage);
        var sut = new BackEnd(storage);

        // Act
        await sut.ClearStageAsync(before, start);

        // Assert
        Assert.Equal(expectedSNext, storage.SNext);
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
    [InlineData(SegmentType.None, SegmentType.DataSeg, BeginData)]
    [InlineData(SegmentType.None, SegmentType.CodeSeg, BeginCode)]
    [InlineData(SegmentType.DataSeg, SegmentType.None, EndData)]
    [InlineData(SegmentType.DataSeg, SegmentType.DataSeg, "")]
    [InlineData(SegmentType.DataSeg, SegmentType.CodeSeg, $"{EndData}{BeginCode}")]
    [InlineData(SegmentType.CodeSeg, SegmentType.None, EndCode)]
    [InlineData(SegmentType.CodeSeg, SegmentType.DataSeg, $"{EndCode}{BeginData}")]
    [InlineData(SegmentType.CodeSeg, SegmentType.CodeSeg, "")]
    public async Task TransitionsSegmentAsync(
        SegmentType oldSeg, SegmentType newSeg, string expected)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = Arrange(output, oldSeg: oldSeg);

        await sut.ToSegAsync(newSeg);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that can declare entry point.
    /// </summary>
    /// <param name="ident">Identity code of object being defined.</param>
    /// <param name="ssName">Static symbol name.</param>
    /// <param name="expectedSeg">Expected segment beginning.</param>
    /// <param name="expectedSuffix">Expected declaration suffix.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(SymbolIdentity.Variable, "a", BeginData, "")]
    [InlineData(SymbolIdentity.Function, "foo", BeginCode, ":\r\n")]
    public async Task DeclaresEntryAsync(
        SymbolIdentity ident,
        string ssName,
        string expectedSeg,
        string expectedSuffix)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = Arrange(output, ssName: ssName);

        await sut.PublicAsync(ident);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        var outName = $"_{ssName?.ToUpperInvariant()}";
        var expected = @$"{expectedSeg}PUBLIC {outName}
{outName}{expectedSuffix}";
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that can declare external.
    /// </summary>
    /// <param name="name">Extern name.</param>
    /// <param name="size">Extern size.</param>
    /// <param name="ident">Identity code of object being defined.</param>
    /// <param name="expectedSeg">Expected segment beginning.</param>
    /// <param name="expectedSuffix">Expected declaration suffix.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("b", 1, SymbolIdentity.Variable, BeginData, "BYTE")]
    [InlineData("w", 2, SymbolIdentity.Variable, BeginData, "WORD")]
    [InlineData("foo", 0, SymbolIdentity.Function, BeginCode, "NEAR")]
    public async Task DeclaresExternAsync(
        string name,
        int size,
        SymbolIdentity ident,
        string expectedSeg,
        string expectedSuffix)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = Arrange(output);

        await sut.ExternalAsync(name, size, ident);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        var outName = $"_{name?.ToUpperInvariant()}";
        var expected = $"{expectedSeg}EXTRN {outName}:{expectedSuffix}\r\n";
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that can point to following object(s).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task PointsAsync()
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = Arrange(output);

        await sut.PointAsync();
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        var expected = " DW $+2\r\n";
        Assert.Equal(expected, actual);
    }

    private static BackEnd Arrange(
        StreamWriter output,
        Collection<KeyValuePair<PCode, int>>? stage = null,
        SegmentType oldSeg = SegmentType.None,
        SymbolTable? symbolTable = null,
        string? ssName = null)
    {
        var storage = ArrangeStorage(
            output,
            stage,
            oldSeg,
            symbolTable ?? new([], []),
            ssName);

        return new BackEnd(storage);
    }

    private static Storage ArrangeStorage(
        StreamWriter output,
        Collection<KeyValuePair<PCode, int>>? stage = null,
        SegmentType oldSeg = SegmentType.None,
        SymbolTable? symbolTable = null,
        string? ssName = null)
    {
        return new Storage(
            Machine.Bpw,
            output,
            stage,
            Storage.StageSize,
            oldSeg,
            symbolTable ?? new([], []),
            ssName);
    }
}
