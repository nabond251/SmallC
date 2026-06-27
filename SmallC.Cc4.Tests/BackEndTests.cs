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
    private const string NullToData = @"DATA SEGMENT PUBLIC";

    private const string NullToCode = @"CODE SEGMENT PUBLIC
ASSUME CS:CODE, SS:DATA, DS:DATA";

    /// <summary>
    /// Tests that header is generated.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GeneratesHeaderAsync()
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = new BackEnd(output);

        await sut.HeaderAsync();
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        var expected = @"CODE SEGMENT PUBLIC
ASSUME CS:CODE, SS:DATA, DS:DATA
extrn __eq: near
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
CODE ENDS
DATA SEGMENT PUBLIC
dw 0";
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that can transition between segments.
    /// </summary>
    /// <param name="newSeg">Segment to change to.</param>
    /// <param name="expected">Expected output.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(SegmentType.Null, "")]
    [InlineData(SegmentType.DataSeg, NullToData)]
    [InlineData(SegmentType.CodeSeg, NullToCode)]
    public async Task TransitionsSegmentAsync(
        SegmentType newSeg, string expected)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = new BackEnd(output);

        await sut.ToSegAsync(newSeg);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        Assert.Equal(expected, actual);
    }
}
