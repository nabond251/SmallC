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
    [InlineData(";", null)]
    [InlineData("test", "test")]
    [InlineData("foo()", "foo")]
    [InlineData(" bar", "bar")]
    [InlineData("  baz;", "baz")]
    [InlineData("  foo_();", "foo_")]
    [InlineData(" _bar ", "_bar")]
    [InlineData("b4z ", "b4z")]
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

    /// <summary>
    /// Tests that can match string literals.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="lit">String literal to match.</param>
    /// <param name="expectedMatch">
    /// A value indicating whether the next token matched.
    /// </param>
    /// <param name="expectedNext">Expected next input text.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("", "", false, "")]
    [InlineData(";", ";", true, "")]
    [InlineData(";", "", false, ";")]
    [InlineData("", ";", false, "")]
    [InlineData("foo", ";", false, "foo")]
    [InlineData("foo", "bar", false, "foo")]
    [InlineData("test", "test", true, "")]
    [InlineData("foo()", "foo", true, "()")]
    [InlineData(" bar", "bar", true, "")]
    [InlineData("  baz;", "baz", true, ";")]
    [InlineData("  foo_();", "foo_", true, "();")]
    [InlineData(" _bar ", "_bar", true, " ")]
    [InlineData("b4z ", "b4z", true, " ")]
    public async Task CanMatchAsync(
        string inputText, string lit, bool expectedMatch, string? expectedNext)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, storage) = Arrange(
            output, input: input, lineType: BufferLineType.Parsing);

        var actualMatch = await sut.MatchAsync(lit);

        Assert.Equal(expectedMatch, actualMatch);
        Assert.Equal(expectedNext, storage.Line[storage.LPtr..]);
    }

    /// <summary>
    /// Tests that can match alphanumeric string literals.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="lit">String literal to match.</param>
    /// <param name="expectedMatch">
    /// A value indicating whether the next token matched.
    /// </param>
    /// <param name="expectedNext">Expected next input text.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("foo", "bar", false, "foo")]
    [InlineData("test", "test", false, "test")]
    [InlineData("foo()", "foo", true, "()")]
    [InlineData(" bar", "bar", true, "")]
    [InlineData("  _az;", "_az", true, ";")]
    [InlineData("  foo_();", "foo_", false, "foo_();")]
    [InlineData(" _bar ", "_bar", false, "_bar ")]
    [InlineData("b4z ", "b4z", true, " ")]
    public async Task CanAlphanumericMatchAsync(
        string inputText, string lit, bool expectedMatch, string? expectedNext)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, storage) = Arrange(
            output, input: input, lineType: BufferLineType.Parsing);

        var actualMatch = await sut.AMatchAsync(lit, 3);

        Assert.Equal(expectedMatch, actualMatch);
        Assert.Equal(expectedNext, storage.Line[storage.LPtr..]);
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
            0,
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
