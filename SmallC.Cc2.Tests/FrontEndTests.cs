// <copyright file="FrontEndTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc2.Tests;

using SmallC.Cc;
using SmallC.Cc2;
using System.Collections.ObjectModel;
using System.Text;
using static SmallC.Cc.Storage;

/// <summary>
/// Tests the front end functions.
/// </summary>
public class FrontEndTests
{
    /// <summary>
    /// Tests that can test for legal symbol names.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expected">Expected symbol match.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("", null)]
    [InlineData("test", "test")]
    public async Task CanTestSymNameAsync(string inputText, string? expected)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, _) = Arrange(
            output, input: input, lineType: BufferLineType.Parsing);

        var actual = await sut.SymNameAsync();

        Assert.Equal(expected, actual);
    }

    private static (FrontEnd Sut, Storage Storage) Arrange(
        StreamWriter output,
        StreamReader? input = null,
        Collection<KeyValuePair<PCode, int>>? stage = null,
        SegmentType oldSeg = SegmentType.None,
        SymbolTable? symbolTable = null,
        Collection<sbyte>? litQ = null,
        BufferLineType? lineType = null,
        string? ssName = null)
    {
        var storage = new Storage(
            0,
            Machine.Bpw,
            false,
            output,
            input ?? TextReader.Null,
            stage,
            null,
            null,
            StageSize,
            oldSeg,
            false,
            symbolTable ?? new([], []),
            litQ ?? [],
            string.Empty,
            string.Empty,
            lineType ?? BufferLineType.None,
            0,
            ssName);
        var sut = new FrontEnd(storage);

        return (sut, storage);
    }
}
