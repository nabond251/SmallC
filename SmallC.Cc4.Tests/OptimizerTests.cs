// <copyright file="OptimizerTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc4.Tests;

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc4;
using System.Globalization;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// Tests the peephole optimizer.
/// </summary>
public class OptimizerTests
{
    /// <summary>
    /// Tests optimizer.
    /// </summary>
    /// <param name="unoptimized">Unoptimized p-codes.</param>
    /// <param name="expected">Optimized code.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
#pragma warning disable SA1118 // Parameter should not span multiple lines
    [InlineData(
@"",
@"")]
    [InlineData(
@"ADD12,0
MOVE21,0",
@"ADD BX,AX
")]
    [InlineData(
@"ADD1n,-3",
@"ADD AX,-3
")]
    [InlineData(
@"ADD1n,-2",
@"DEC AX
DEC AX
")]
    [InlineData(
@"ADD1n,-1",
@"DEC AX
")]
    [InlineData(
@"ADD1n,0",
@"")]
    [InlineData(
@"ADD1n,1",
@"INC AX
")]
    [InlineData(
@"ADD1n,2",
@"INC AX
INC AX
")]
    [InlineData(
@"ADD1n,3",
@"ADD AX,3
")]
    [InlineData(
@"ADD2n,-3",
@"ADD BX,-3
")]
    [InlineData(
@"ADD2n,-2",
@"DEC BX
DEC BX
")]
    [InlineData(
@"ADD2n,-1",
@"DEC BX
")]
    [InlineData(
@"ADD2n,0",
@"")]
    [InlineData(
@"ADD2n,1",
@"INC BX
")]
    [InlineData(
@"ADD2n,2",
@"INC BX
INC BX
")]
    [InlineData(
@"ADD2n,3",
@"ADD BX,3
")]
    [InlineData(
@"rDEC1,1
PUTbp1,0
rINC1,1",
@"DEC BYTE PTR [BX]
")]
    [InlineData(
@"rDEC1,2
PUTbp1,0
rINC1,2",
@"SUB BYTE PTR [BX],2
")]
    [InlineData(
@"rDEC1,1
PUTwp1,0
rINC1,1",
@"DEC WORD PTR [BX]
")]
    [InlineData(
@"rDEC1,2
PUTwp1,0
rINC1,2",
@"SUB WORD PTR [BX],2
")]
    [InlineData(
@"rDEC1,1
PUTbm1,0
rINC1,1",
@"SUB _C,1
")]
    [InlineData(
@"rDEC1,2
PUTwm1,6
rINC1,2",
@"SUB _I,2
")]
    [InlineData(
@"GETw1m,6
GETw2n,12
ADD12,0
MOVE21,0
GETb1p,4",
@"MOV BX,_I
MOV AL,12[BX]
CBW
")]
    [InlineData(
@"GETw1m,6
GETw2n,12
ADD12,0
MOVE21,0
GETb1pu,4",
@"MOV BX,_I
MOV AL,12[BX]
XOR AH,AH
")]
    [InlineData(
@"GETw1m,6
GETw2n,12
ADD12,0
MOVE21,0
GETw1p,4",
@"MOV BX,_I
MOV AX,12[BX]
")]
    [InlineData(
@"GETw1m,6
GETw2m,9
SWAP12,0",
@"MOV BX,_I
MOV AX,_UI
")]
    [InlineData(
@"GETw1m,6
MOVE21,0",
@"MOV BX,_I
")]
    [InlineData(
@"GETw1m,6
PUSH1,0",
@"PUSH _I
")]
    [InlineData(
@"GETw1m,6
PUSH1,0
MOVE21,0",
@"MOV AX,_I
PUSH AX
MOV BX,AX
")]
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
GETb1p,0
POINT1s,6
MOVE21,0
GETw1p,0
POINT1s,6
MOVE21,0
GETw1p,0
GETw2n,5
ADD12,0
MOVE21,0
GETb1p,0
POINT1s,4
MOVE21,0
GETw1p,0
POINT1s,4
MOVE21,0
GETw1p,0
MOVE21,0
GETb1p,0",
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
CBW
")]
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
GETb1pu,0
POINT1s,6
MOVE21,0
GETw1p,0
POINT1s,6
MOVE21,0
GETw1p,0
GETw2n,5
ADD12,0
MOVE21,0
GETb1pu,0
POINT1s,4
MOVE21,0
GETw1p,0
POINT1s,4
MOVE21,0
GETw1p,0
MOVE21,0
GETb1pu,0",
@"MOV AX,14[BP]
MOV AX,12[BP]
MOV BX,12[BP]
ADD BX,10
MOV AX,[BX]
MOV AX,10[BP]
MOV BX,10[BP]
MOV AX,[BX]
MOV AL,8[BP]
XOR AH,AH
MOV AX,6[BP]
MOV BX,6[BP]
ADD BX,5
MOV AL,[BX]
XOR AH,AH
MOV AX,4[BP]
MOV BX,4[BP]
MOV AL,[BX]
XOR AH,AH
")]
#pragma warning restore SA1118 // Parameter should not span multiple lines
    public async Task CanOptimizeAsync(
        string unoptimized, string expected)
    {
        ArgumentNullException.ThrowIfNull(unoptimized);
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = Arrange(output, unoptimized);

        await sut.DumpStageAsync();
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        Assert.Equal(expected, actual);
    }

    private static BackEnd Arrange(
        StreamWriter output, string unoptimized)
    {
        var storage = new Storage(output: output, optimize: true);

        var symTabMgmt = new SymbolTableUseCases(storage);
        var utility = new UtilityUseCases(storage);
        _ = symTabMgmt.AddSym(
            "c",
            SymbolIdentity.Variable,
            SymbolType.Chr,
            1,
            -10,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "ca3",
            SymbolIdentity.Array,
            SymbolType.Chr,
            3,
            -8,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "cp",
            SymbolIdentity.Pointer,
            SymbolType.Chr,
            2,
            -6,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "uc",
            SymbolIdentity.Variable,
            SymbolType.UChr,
            1,
            -4,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "uca3",
            SymbolIdentity.Array,
            SymbolType.UChr,
            3,
            -2,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "ucp",
            SymbolIdentity.Pointer,
            SymbolType.UChr,
            2,
            0,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "i",
            SymbolIdentity.Variable,
            SymbolType.Int,
            2,
            2,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "ia3",
            SymbolIdentity.Array,
            SymbolType.Int,
            6,
            4,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "ip",
            SymbolIdentity.Pointer,
            SymbolType.Int,
            2,
            6,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "ui",
            SymbolIdentity.Variable,
            SymbolType.UInt,
            2,
            8,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "uia3",
            SymbolIdentity.Array,
            SymbolType.UInt,
            6,
            10,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "uip",
            SymbolIdentity.Pointer,
            SymbolType.UInt,
            2,
            12,
            storage.SymTab.Locals,
            SymbolClass.Automatic);
        _ = symTabMgmt.AddSym(
            "gc",
            SymbolIdentity.Variable,
            SymbolType.Chr,
            1,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "gca3",
            SymbolIdentity.Array,
            SymbolType.Chr,
            3,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "gcp",
            SymbolIdentity.Pointer,
            SymbolType.Chr,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "guc",
            SymbolIdentity.Variable,
            SymbolType.UChr,
            1,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "guca3",
            SymbolIdentity.Array,
            SymbolType.UChr,
            3,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "gucp",
            SymbolIdentity.Pointer,
            SymbolType.UChr,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "gi",
            SymbolIdentity.Variable,
            SymbolType.Int,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "gia3",
            SymbolIdentity.Array,
            SymbolType.Int,
            6,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "gip",
            SymbolIdentity.Pointer,
            SymbolType.Int,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "gui",
            SymbolIdentity.Variable,
            SymbolType.UInt,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "guia3",
            SymbolIdentity.Array,
            SymbolType.UInt,
            6,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "guip",
            SymbolIdentity.Pointer,
            SymbolType.UInt,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);
        _ = symTabMgmt.AddSym(
            "ec",
            SymbolIdentity.Variable,
            SymbolType.Chr,
            1,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "eca3",
            SymbolIdentity.Array,
            SymbolType.Chr,
            3,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "ecp",
            SymbolIdentity.Pointer,
            SymbolType.Chr,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "euc",
            SymbolIdentity.Variable,
            SymbolType.UChr,
            1,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "euca3",
            SymbolIdentity.Array,
            SymbolType.UChr,
            3,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "eucp",
            SymbolIdentity.Pointer,
            SymbolType.UChr,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "ei",
            SymbolIdentity.Variable,
            SymbolType.Int,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "eia3",
            SymbolIdentity.Array,
            SymbolType.Int,
            6,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "eip",
            SymbolIdentity.Pointer,
            SymbolType.Int,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "eui",
            SymbolIdentity.Variable,
            SymbolType.UInt,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "euia3",
            SymbolIdentity.Array,
            SymbolType.UInt,
            6,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "euip",
            SymbolIdentity.Pointer,
            SymbolType.UInt,
            2,
            0,
            storage.SymTab.Globals,
            SymbolClass.External);
        _ = symTabMgmt.AddSym(
            "foo",
            SymbolIdentity.Function,
            SymbolType.Int,
            0,
            0,
            storage.SymTab.Globals,
            SymbolClass.Static);

        var sut = new BackEnd(symTabMgmt, utility, storage);
        sut.SetCodes();
        _ = sut.SetStage();
        var stage = storage.Stage ?? throw new InvalidOperationException();
        ParsePCodes(unoptimized).ForEach(stage.Add);

        return sut;
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
