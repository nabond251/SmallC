// <copyright file="BackEndTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc4.Tests;

using SmallC.Cc;
using SmallC.Cc4;

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
        var storage = new Storage(new([], []));
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = new BackEnd(storage, output);

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
    /// <param name="oldSeg">Segment to change from.</param>
    /// <param name="expectedPrefix">Expected output prefix.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(SegmentType.DataSeg, $"{BeginData}{EndData}")]
    [InlineData(SegmentType.CodeSeg, $"{BeginCode}{EndCode}")]
    public async Task GeneratesTrailerAsync(
        SegmentType oldSeg, string expectedPrefix)
    {
        var storage = new Storage(new([], []));
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = new BackEnd(storage, output);

        await sut.ToSegAsync(oldSeg);
        await sut.TrailerAsync();
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        var expected = $"{expectedPrefix}END\r\n";
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
    [InlineData(null, null, "")]
    [InlineData(SegmentType.DataSeg, SegmentType.DataSeg, $"{BeginData}{EndData}")]
    [InlineData(SegmentType.DataSeg, SegmentType.CodeSeg, $"{BeginData}{EndData}{BeginCode}{EndCode}")]
    [InlineData(SegmentType.CodeSeg, SegmentType.CodeSeg, $"{BeginCode}{EndCode}")]
    [InlineData(SegmentType.CodeSeg, SegmentType.DataSeg, $"{BeginCode}{EndCode}{BeginData}{EndData}")]
    public async Task TransitionsSegmentAsync(
        SegmentType? oldSeg, SegmentType? newSeg, string expected)
    {
        var storage = new Storage(new([], []));
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = new BackEnd(storage, output);

        await sut.ToSegAsync(oldSeg);
        await sut.ToSegAsync(newSeg);
        await sut.ToSegAsync(null);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        Assert.Equal(expected, actual);
    }
}
