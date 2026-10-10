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
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expected">Expected parsing line.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
#pragma warning disable SA1118 // Parameter should not span multiple lines
    [InlineData(
@"
int gi1;
func(ai, aia, aip, ac, aca, acp) int  ai, aia[], *aip;
                                 char ac, aca[], *acp; {
  ai;
  aia;
  aia[5];
  aip;
  *aip;
  ac;
  aca;
  aca[5];
  acp;
  *acp;
  return (gi1);
  }",
@"DATA SEGMENT PUBLIC
PUBLIC _GI1
_GI1 DW 1 DUP(0)
DATA ENDS
CODE SEGMENT PUBLIC
ASSUME CS:CODE, SS:DATA, DS:DATA
PUBLIC _FUNC
_FUNC:
PUSH BP
MOV BP,SP
MOV AX,14[BP]
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
CBW
MOV AX,_GI1
POP BP
RET
")]
    [InlineData(
@"
unsigned int gui1;
func(aui, auia, auip, auc, auca, aucp) unsigned int  aui, auia[], *auip;
                                       unsigned char auc, auca[], *aucp; {
  return (gui1);
  }",
@"DATA SEGMENT PUBLIC
PUBLIC _GUI1
_GUI1 DW 1 DUP(0)
DATA ENDS
CODE SEGMENT PUBLIC
ASSUME CS:CODE, SS:DATA, DS:DATA
PUBLIC _FUNC
_FUNC:
PUSH BP
MOV BP,SP
MOV AX,_GUI1
POP BP
RET
")]
#pragma warning restore SA1118 // Parameter should not span multiple lines
    public async Task CanOptimizeAsync(
        string inputText, string? expected)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
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
