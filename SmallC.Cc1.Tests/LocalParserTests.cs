// <copyright file="LocalParserTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc1.Tests;

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc3;
using SmallC.Cc4;
using System.Text;

/// <summary>
/// Tests the local parser.
/// </summary>
public class LocalParserTests
{
    /// <summary>
    /// Tests parser.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expected">Expected statement type.</param>
    /// <param name="expectedOutput">Expected parsing line.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
#pragma warning disable SA1118 // Parameter should not span multiple lines
    [InlineData(
@"{
  }
",
StatementType.None,
@"")]
#pragma warning restore SA1118 // Parameter should not span multiple lines
    public async Task CanParseAsync(
        string inputText, StatementType expected, string? expectedOutput)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var sut = Arrange(output: output, input: input);

        var actual = await sut.StatementAsync();
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actualOutput = await reader.ReadToEndAsync();

        Assert.Equal(expected, actual);
        Assert.Equal(expectedOutput, actualOutput);
    }

    private static LocalParser Arrange(
        StreamWriter? output = null,
        StreamReader? input = null)
    {
        var storage = new Storage(
            output: output,
            files: input != null,
            input: input);

        var symTabMgmt = new SymbolTableUseCases(storage);
        var utility = new UtilityUseCases(storage);
        var whileQueueMgmt = new WhileQueueUseCases(utility, storage);

        var frontEnd = new FrontEnd(storage);
        var backEnd = new BackEnd(symTabMgmt, utility, storage);
        var analyzer = new Analyzer(symTabMgmt, utility, frontEnd, backEnd, storage);
        backEnd.SetCodes();

        var sut = new LocalParser(
            symTabMgmt, utility, whileQueueMgmt, frontEnd, analyzer, backEnd, storage);

        return sut;
    }
}
