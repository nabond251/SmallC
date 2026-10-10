// <copyright file="OptimizerTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc4.Tests;

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc4;
using System.Text;

/// <summary>
/// Tests the peephole optimizer.
/// </summary>
public class OptimizerTests
{
    /// <summary>
    /// Tests optimizer.
    /// </summary>
    /// <param name="unoptimized">Input stream text.</param>
    /// <param name="expected">Expected parsing line.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
#pragma warning disable SA1118 // Parameter should not span multiple lines
    [InlineData(
@"",
@"")]
    [InlineData(
@"LEA AX,14[BP]
MOV BX,AX
MOV AX,[BX]
LEA AX,12[BP]
MOV BX,AX
MOV AX,[BX]
LEA AX,12[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,10
ADD AX,BX
MOV BX,AX
MOV AX,[BX]
LEA AX,10[BP]
MOV BX,AX
MOV AX,[BX]
LEA AX,10[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,[BX]
LEA AX,8[BP]
MOV BX,AX
MOV AL,[BX]
CBW
LEA AX,6[BP]
MOV BX,AX
MOV AX,[BX]
LEA AX,6[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,5
ADD AX,BX
MOV BX,AX
MOV AL,[BX]
CBW
LEA AX,4[BP]
MOV BX,AX
MOV AX,[BX]
LEA AX,4[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AL,[BX]
CBW",
@"MOV AX,14[BP]
MOV AX,12[BP]
MOV BX,12[BP]
ADD BX,10
MOV AX,[BX]
MOV AX,10[BP]
MOV BX,10[BP]
MOV AX,[BX]
MOV AL,8[BP]
CBW
MOV AX,6[BP]
MOV BX,6[BP]
ADD BX,5
MOV AL,[BX]
CBW
MOV AX,4[BP]
MOV BX,4[BP]
MOV AL,[BX]
CBW")]
#pragma warning restore SA1118 // Parameter should not span multiple lines
    public async Task CanOptimizeAsync(
        string unoptimized, string? expected)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(unoptimized);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var sut = Arrange(output: output, input: input);

        await sut.DumpStageAsync();
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        Assert.Equal(expected, actual);
    }

    private static BackEnd Arrange(
        StreamWriter? output = null,
        StreamReader? input = null)
    {
        var storage = new Storage(
            output: output,
            files: input != null,
            input: input,
            optimize: true);

        var symTabMgmt = new SymbolTableUseCases(storage);
        var utility = new UtilityUseCases(storage);

        var sut = new BackEnd(symTabMgmt, utility, storage);
        sut.SetCodes();

        return sut;
    }
}
