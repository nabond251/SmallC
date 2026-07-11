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
    /// Tests preprocessor.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="cCode">Whether parsing C code.</param>
    /// <param name="expected">Expected parsing line.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("", false, "")]
    [InlineData(" ", false, " \r\n")]
    [InlineData("TEST", false, "TEST\r\n")]
    [InlineData("", true, "")]
    [InlineData(" ", true, "")]
    [InlineData("  ", true, "")]
    [InlineData("\r\n", true, "")]
    [InlineData("\r\n ", true, "")]
    [InlineData(" \r\n", true, "")]
    [InlineData("\r\n  ", true, "")]
    [InlineData(" \r\n ", true, "")]
    [InlineData("  \r\n", true, "")]
    [InlineData("\"\"", true, "\"\"")]
    [InlineData("\"test\"", true, "\"test\"")]
    [InlineData("\"test\"\r\n", true, "\"test\"")]
    [InlineData("''", true, "''")]
    [InlineData("'t'", true, "'t'")]
    [InlineData("/**/", true, "")]
    [InlineData("/* test */", true, "")]
    [InlineData("foo/*bar*/baz", true, "foobaz")]
    [InlineData("foo/*bar\r\nbaz\r\nquux", true, "foo")]
    [InlineData("foo/*bar*/baz\r\nquux", true, "foobaz")]
    [InlineData("foo/*bar\r\nbaz*/quux", true, "fooquux")]
    [InlineData("FOO", true, "BAR")]
    [InlineData("FOOBARBAZ", true, "QUUX")]
    public async Task CanPreprocessAsync(
        string inputText, bool cCode, string? expected)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var mac = new Dictionary<string, string>
        {
            { "FOO", "BAR" },
            { "FOOBARBA", "QUUX" },
        };
        var (sut, storage) = Arrange(
            output, input: input, cCode: cCode, mac: mac);

        await sut.PreprocessAsync();
        var actual = storage.PLine;

        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests failing preprocessor.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expected">Expected error.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("\"", "no quote")]
    [InlineData("\"test", "no quote")]
    [InlineData("\'", "no apostrophe")]
    [InlineData("\'t", "no apostrophe")]
    [InlineData("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789", "line too long")]
    public async Task CanFailPreprocessingAsync(string inputText, string? expected)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, _) = Arrange(output, input: input);

        var actual = (await Assert.ThrowsAsync<InvalidOperationException>(
            sut.PreprocessAsync)).Message;

        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests #if... directives.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expected">Expected parsing line.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("", "")]
    [InlineData(
        @"
#ifdef FOO
    foo();
#endif",
        "foo();")]
    [InlineData(
        @"
#ifdef FOO
#ifdef FOOBARBAZ
    foobarbaz();
#else
    foo();
#endif
#else
    bar();
#endif",
        "foobarbaz();")]
    [InlineData(
        @"
#ifndef BAR
    bar();
#endif",
        "bar();")]
    [InlineData(
        @"
#ifndef BAR
    bar();
#else
    foo();
#endif",
        "bar();")]
    [InlineData(
        @"
#ifndef FOO
    foo();
#endif
    bar();",
        "bar();")]
    [InlineData(
        @"
#ifndef FOO
    foo();
#else
    bar();
#endif",
        "bar();")]
    [InlineData(
        @"
#ifndef FOO
    foo();
#else
#ifndef FOOBARBAZ
    foobarbaz();
#else
    bar();
#endif
#endif",
        "bar();")]
    [InlineData(
        @"
#ifndef FOO
    foo();
#else
#ifndef FOOBARBAZ
#endif
#endif
    bar();",
        "bar();")]
    [InlineData(
        @"
#ifdef BAR
    bar();
#endif
    foo();",
        "foo();")]
    [InlineData(
        @"
#ifdef BAR
    bar();
#else
    foo();
#endif",
        "foo();")]
    [InlineData(
        @"
#ifdef FOO
#ifndef FOOBARBAZ
    foobarbaz();
#endif
#endif
    bar();",
        "bar();")]
    public async Task CanIfLineAsync(
        string inputText, string? expected)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var mac = new Dictionary<string, string>
        {
            { "FOO", "BAR" },
            { "FOOBARBA", "QUUX" },
        };
        var (sut, storage) = Arrange(
            output, input: input, mac: mac);

        await sut.IfLineAsync();
        var actual = storage.PLine;

        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests failing #if... directives.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expected">Expected error.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("#else", "no matching #if...")]
    [InlineData("#endif", "no matching #if...")]
    public async Task CanFailIfLineAsync(string inputText, string? expected)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, _) = Arrange(output, input: input);

        var actual = (await Assert.ThrowsAsync<InvalidOperationException>(
            sut.IfLineAsync)).Message;

        Assert.Equal(expected, actual);
    }

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

    /// <summary>
    /// Tests that can match next operator.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="list">List of operators to match.</param>
    /// <param name="expectedMatch">
    /// A value indicating whether the next token matched.
    /// </param>
    /// <param name="expectedNext">Expected next input text.</param>
    /// <param name="expectedOpIndex">Expected operator match index.</param>
    /// <param name="expectedOpSize">Expected operator size.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("", "", false, "", 0, 0)]
    [InlineData(" = ", "", false, "= ", 0, 0)]
    [InlineData(" = ", "=", true, "= ", 0, 1)]
    [InlineData(" = ", "==", false, "= ", 1, 0)]
    [InlineData(" = ", "== =", true, "= ", 1, 1)]
    [InlineData("< ", "<= =", false, "< ", 2, 0)]
    [InlineData("<= ", "= <=", true, "<= ", 1, 2)]
    public async Task CanMatchNextOpAsync(
        string inputText,
        string list,
        bool expectedMatch,
        string? expectedNext,
        int expectedOpIndex,
        int expectedOpSize)
    {
        ArgumentNullException.ThrowIfNull(list);
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, storage) = Arrange(
            output, input: input, lineType: BufferLineType.Parsing);

        var actualMatch = await sut.NextOpAsync(list.Split(
            ' ', StringSplitOptions.RemoveEmptyEntries));

        Assert.Equal(expectedMatch, actualMatch);
        Assert.Equal(expectedNext, storage.Line[storage.LPtr..]);
        Assert.Equal(expectedOpIndex, storage.OpIndex);
        Assert.Equal(expectedOpSize, storage.OpSize);
    }

    private static (FrontEnd Sut, Storage Storage) Arrange(
        StreamWriter output,
        StreamReader? input = null,
        bool cCode = true,
        Collection<KeyValuePair<PCode, int>>? stage = null,
        SegmentType oldSeg = SegmentType.None,
        SymbolTable? symbolTable = null,
        Collection<sbyte>? litQ = null,
        Dictionary<string, string>? mac = null,
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
            cCode,
            stage,
            null,
            null,
            0,
            0,
            StageSize,
            oldSeg,
            false,
            symbolTable ?? new([], []),
            litQ ?? [],
            mac ?? [],
            string.Empty,
            string.Empty,
            lineType ?? BufferLineType.Parsing,
            0,
            null,
            ssName);
        var sut = new FrontEnd(storage);

        return (sut, storage);
    }
}
