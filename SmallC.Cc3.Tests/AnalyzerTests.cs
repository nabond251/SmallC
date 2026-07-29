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

/// <summary>
/// Tests the expression analyzer.
/// </summary>
public class AnalyzerTests
{
    /// <summary>
    /// Tests that can get character literal.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expected">Expected character literal.</param>
    [Theory]
    [InlineData("", null)]
    public void GetsCharacterLiteral(string inputText, char? expected)
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
        var (sut, _) = Arrange(
            output: output, input: input, mac: mac);

        var actual = sut.LitChar();

        Assert.Equal(expected, actual);
    }

    private static (Analyzer Sut, Storage Storage) Arrange(
        Collection<KeyValuePair<PCode, int>>? stage = null,
        StreamWriter? output = null,
        StreamReader? input = null,
        bool cCode = true,
        SegmentType oldSeg = SegmentType.None,
        SymbolTable? symTab = null,
        Collection<sbyte>? litQ = null,
        Dictionary<string, string>? mac = null,
        BufferLineType? lineType = null,
        string? ssName = null)
    {
        var storage = new Storage(
            stage: stage,
            output: output,
            files: input != null,
            input: input,
            cCode: cCode,
            oldSeg: oldSeg,
            symTab: symTab ?? new([], []),
            litQ: litQ ?? [],
            mac: mac ?? [],
            lineType: lineType ?? BufferLineType.Parsing,
            ssName: ssName);

        var symTabMgmt = new SymbolTableUseCases(storage);
        var utility = new UtilityUseCases(storage);

        var frontEnd = new FrontEnd(storage);
        var backEnd = new BackEnd(symTabMgmt, utility, storage);
        var sut = new Analyzer(symTabMgmt, utility, frontEnd, backEnd, storage);

        return (sut, storage);
    }
}
