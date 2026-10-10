// <copyright file="OptimizerTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc4.Tests;

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc4;
using System.Globalization;

/// <summary>
/// Tests the peephole optimizer.
/// </summary>
public class OptimizerTests
{
    /// <summary>
    /// Tests optimizer.
    /// </summary>
    /// <param name="unoptimized">Unoptimized p-codes.</param>
    /// <param name="optimized">Optimized p-codes.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
#pragma warning disable SA1118 // Parameter should not span multiple lines
    [InlineData(
@"",
@"")]
    [InlineData(
@"POINT1s,14
MOVE21,0
GETw1p,0
POINT1s,12
MOVE21,0
GETw1p,0
POINT1s,12
MOVE21,0
GETw1p,0
GETw2n,10
ADD12,0
MOVE21,0
GETw1p,0
POINT1s,10
MOVE21,0
GETw1p,0
POINT1s,10
MOVE21,0
GETw1p,0
MOVE21,0
GETw1p,0
POINT1s,8
MOVE21,0
GETb1m,0
POINT1s,6
MOVE21,0
GETw1p,0
POINT1s,6
MOVE21,0
GETw1p,0
GETw2n,5
ADD12,0
MOVE21,0
GETb1m,0
POINT1s,4
MOVE21,0
GETw1p,0
POINT1s,4
MOVE21,0
GETw1p,0
MOVE21,0
GETb1m,0",
@"GETw1s,14
GETw1s,12
GETw2s,12
ADD2n,10
GETw1p,0
GETw1s,10
GETw2s,10
GETw1p,0
GETb1s,8
GETw1s,6
GETw2s,6
ADD2n,5
GETb1m,0
GETw1s,4
GETw2s,4
GETb1m,0")]
#pragma warning restore SA1118 // Parameter should not span multiple lines
    public async Task CanOptimizeAsync(
        string unoptimized, string optimized)
    {
        ArgumentNullException.ThrowIfNull(unoptimized);
        ArgumentNullException.ThrowIfNull(optimized);
        var (sut, storage) = Arrange(unoptimized);

        await sut.DumpStageAsync();
        var actual = storage.Stage;

        var expected = ParsePCodes(optimized);
        Assert.Equal(expected, actual);
    }

    private static (BackEnd Sut, Storage Storage) Arrange(string unoptimized)
    {
        var storage = new Storage(optimize: true);

        var symTabMgmt = new SymbolTableUseCases(storage);
        var utility = new UtilityUseCases(storage);

        var sut = new BackEnd(symTabMgmt, utility, storage);
        sut.SetCodes();
        _ = sut.SetStage();
        var stage = storage.Stage ?? throw new InvalidOperationException();
        ParsePCodes(unoptimized).ForEach(stage.Add);

        return (sut, storage);
    }

    private static List<KeyValuePair<PCode, int>> ParsePCodes(string pCodes)
    {
        return [.. pCodes
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Split(','))
            .Select(ts => new KeyValuePair<PCode, int>(
                Enum.Parse<PCode>(ts[0]),
                int.Parse(ts[1], CultureInfo.InvariantCulture)))];
    }
}
