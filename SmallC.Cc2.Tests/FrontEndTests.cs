// <copyright file="FrontEndTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc2.Tests;

using SmallC.Cc;
using SmallC.Cc2;
using System.Collections.ObjectModel;
using static SmallC.Cc.Storage;

/// <summary>
/// Tests the front end functions.
/// </summary>
public class FrontEndTests
{
    /// <summary>
    /// Tests that can test for legal symbol names.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CanTestSymNameAsync()
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var (sut, _) = Arrange(output);

        var actual = await sut.SymNameAsync();

        Assert.Null(actual);
    }

    private static (FrontEnd Sut, Storage Storage) Arrange(
        StreamWriter output,
        Collection<KeyValuePair<PCode, int>>? stage = null,
        SegmentType oldSeg = SegmentType.None,
        SymbolTable? symbolTable = null,
        Collection<sbyte>? litQ = null,
        string? ssName = null)
    {
        var storage = new Storage(
            0,
            Machine.Bpw,
            false,
            output,
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
            BufferLineType.None,
            0,
            ssName);
        var sut = new FrontEnd(storage);

        return (sut, storage);
    }
}
