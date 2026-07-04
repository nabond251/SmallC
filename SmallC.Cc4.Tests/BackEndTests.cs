// <copyright file="BackEndTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc4.Tests;

using SmallC.Cc;
using SmallC.Cc4;
using System.Collections.ObjectModel;
using System.Globalization;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// Tests the back end functions.
/// </summary>
public class BackEndTests
{
    private const string BeginData = "DATA SEGMENT PUBLIC\r\n";
    private const string EndData = "DATA ENDS\r\n";
    private const string BeginCode = "CODE SEGMENT PUBLIC\r\nASSUME CS:CODE, SS:DATA, DS:DATA\r\n";
    private const string EndCode = "CODE ENDS\r\n";

    /// <summary>
    /// Tests that header is generated.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GeneratesHeaderAsync()
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = Arrange(output);

        await sut.HeaderAsync();
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        var expected = $@"{BeginCode}extrn __eq: near
extrn __ne: near
extrn __le: near
extrn __lt: near
extrn __ge: near
extrn __gt: near
extrn __ule: near
extrn __ult: near
extrn __uge: near
extrn __ugt: near
extrn __lneg: near
extrn __switch: near
dw 0
{EndCode}{BeginData}dw 0
";
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that trailer is generated.
    /// </summary>
    /// <param name="extFuncs">Comma-separated list of external funcs.</param>
    /// <param name="hasMain">A value indicating whether main exists.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("", false)]
    [InlineData("foo,bar,baz", false)]
    [InlineData("", true)]
    public async Task GeneratesTrailerAsync(
        string extFuncs,
        bool hasMain)
    {
        ArgumentNullException.ThrowIfNull(extFuncs);
        var funcs = extFuncs.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var globals = funcs.Select(f => new SymbolTableEntry(
            SymbolIdentity.Function,
            SymbolType.Int,
            SymbolClass.AutoExt,
            0,
            null,
            f));
        var globalsAndAnyMain = hasMain ? globals.Append(new(
            SymbolIdentity.Function,
            SymbolType.Int,
            SymbolClass.Static,
            0,
            null,
            "main")) : globals;
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = Arrange(
            output,
            symbolTable: new([], [.. globalsAndAnyMain]));

        await sut.TrailerAsync();
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        var funcsAndAnyMain = hasMain ? funcs.Append("_main") : funcs;
        var externs = string.Join("\r\n", funcsAndAnyMain.Select(
            f => $"EXTRN _{f.ToUpper(CultureInfo.InvariantCulture)}:NEAR"));
        var codeSeg = string.IsNullOrEmpty(externs) ? externs :
            $"{BeginCode}{externs}\r\n{EndCode}";
        var expected = $"{codeSeg}END\r\n";
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that can set stage.
    /// </summary>
    /// <param name="setStage">Whether to set stage.</param>
    /// <param name="pCode">P-code to stage.</param>
    /// <param name="value">Value to stage.</param>
    /// <param name="expectedSNext">Expected next index in stage.</param>
    /// <param name="expectedBefore">
    /// Expected new previous position in queue.
    /// </param>
    /// <param name="expectedStart">
    /// Expected new starting position in queue.
    /// </param>
    [Theory]
    [InlineData(false, null, null, 0, null, 0)]
    [InlineData(true, null, null, 0, 0, 0)]
    [InlineData(true, PCode.ADD12, 0, 1, 1, 1)]
    public void SetsStage(
        bool setStage,
        PCode? pCode,
        int? value,
        int? expectedSNext,
        int? expectedBefore,
        int? expectedStart)
    {
        // Arrange
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        Collection<KeyValuePair<PCode, int>>? stage = setStage ? [] : null;
        if (stage is not null && pCode is not null && value is not null)
        {
            stage.Add(new(pCode.Value, value.Value));
        }

        var storage = ArrangeStorage(output, stage);
        var sut = new BackEnd(storage);

        // Act
        var (actualBefore, actualStart) = sut.SetStage();

        // Assert
        Assert.Equal(expectedSNext, storage.SNext);
        Assert.Equal(expectedBefore, actualBefore);
        Assert.Equal(expectedStart, actualStart);
    }

    /// <summary>
    /// Tests that can generate code.
    /// </summary>
    /// <param name="setStage">Whether to set stage.</param>
    /// <param name="pCode">P-code to stage.</param>
    /// <param name="value">Value to stage.</param>
    /// <param name="expectedCsp">Expected compiler relative stk ptr.</param>
    /// <param name="expectedSNext">Expected next index in stage.</param>
    /// <param name="expectedFirstPCode">Expected first staged p-code.</param>
    /// <param name="expectedFirstValue">Expected first staged value.</param>
    /// <param name="expectedLastPCode">Expected last staged p-code.</param>
    /// <param name="expectedLastValue">Expected last staged value.</param>
    /// <param name="expectedGen">Expected generated code.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(false, PCode.ADD12, 0, 2, null, null, null, null, null, "ADD AX,BX\r\n")]
    [InlineData(false, PCode.ADD1n, 0, 2, null, null, null, null, null, "?ADD AX,<n>\r\n??")]
    [InlineData(false, PCode.ADD21, 0, 2, null, null, null, null, null, "ADD BX,AX\r\n")]
    [InlineData(false, PCode.ADD2n, 0, 2, null, null, null, null, null, "?ADD BX,<n>\r\n??")]
    [InlineData(false, PCode.ADDbpn, 0, 2, null, null, null, null, null, "ADD BYTE PTR [BX],<n>\r\n")]
    [InlineData(false, PCode.ADDwpn, 0, 2, null, null, null, null, null, "ADD WORD PTR [BX],<n>\r\n")]
    [InlineData(false, PCode.ADDm_, 0, 2, null, null, null, null, null, "ADD <m>")]
    [InlineData(false, PCode.ADDSP, 0, 2, null, null, null, null, null, "?ADD SP,<n>\r\n??")]
    [InlineData(false, PCode.AND12, 0, 2, null, null, null, null, null, "AND AX,BX\r\n")]
    [InlineData(false, PCode.ANEG1, 0, 2, null, null, null, null, null, "NEG AX\r\n")]
    [InlineData(false, PCode.ARGCNTn, 0, 2, null, null, null, null, null, "?MOV CL,<n>?XOR CL,CL?\r\n")]
    [InlineData(false, PCode.ASL12, 0, 2, null, null, null, null, null, "MOV CX,AX\r\nMOV AX,BX\r\nSAL AX,CL\r\n")]
    [InlineData(false, PCode.ASR12, 0, 2, null, null, null, null, null, "MOV CX,AX\r\nMOV AX,BX\r\nSAR AX,CL\r\n")]
    [InlineData(false, PCode.CALL1, 0, 2, null, null, null, null, null, "CALL AX\r\n")]
    [InlineData(false, PCode.CALLm, 0, 2, null, null, null, null, null, "CALL <m>\r\n")]
    [InlineData(false, PCode.BYTE_, 0, 2, null, null, null, null, null, " DB ")]
    [InlineData(false, PCode.BYTEn, 0, 2, null, null, null, null, null, " DB <n>\r\n")]
    [InlineData(false, PCode.BYTEr0, 0, 2, null, null, null, null, null, " DB <n> DUP(0)\r\n")]
    [InlineData(false, PCode.COM1, 0, 2, null, null, null, null, null, "NOT AX\r\n")]
    [InlineData(false, PCode.COMMAn, 0, 2, null, null, null, null, null, ",<n>\r\n")]
    [InlineData(false, PCode.DBL1, 0, 2, null, null, null, null, null, "SHL AX,1\r\n")]
    [InlineData(false, PCode.DBL2, 0, 2, null, null, null, null, null, "SHL BX,1\r\n")]
    [InlineData(false, PCode.DECbp, 0, 2, null, null, null, null, null, "DEC BYTE PTR [BX],<n>\r\n")]
    [InlineData(false, PCode.DECwp, 0, 2, null, null, null, null, null, "DEC WORD PTR [BX],<n>\r\n")]
    [InlineData(false, PCode.DIV12, 0, 2, null, null, null, null, null, "CWD\r\nIDIV BX\r\n")]
    [InlineData(false, PCode.DIV12u, 0, 2, null, null, null, null, null, "XOR DX,DX\r\nDIV BX\r\n")]
    [InlineData(false, PCode.ENTER, 0, 2, null, null, null, null, null, "PUSH BP\r\nMOV BP,SP\r\n")]
    [InlineData(false, PCode.EQ10f, 0, 2, null, null, null, null, null, "OR AX,AX\r\nJE $+5\r\nJMP _<n>\r\n")]
    [InlineData(false, PCode.EQ12, 0, 2, null, null, null, null, null, "CALL __EQ\r\n")]
    [InlineData(false, PCode.GE10f, 0, 2, null, null, null, null, null, "OR AX,AX\r\nJGE $+5\r\nJMP _<n>\r\n")]
    [InlineData(false, PCode.GE12, 0, 2, null, null, null, null, null, "CALL __GE\r\n")]
    [InlineData(false, PCode.GE12u, 0, 2, null, null, null, null, null, "CALL __UGE\r\n")]
    [InlineData(false, PCode.GETb1m, 0, 2, null, null, null, null, null, "MOV AL,<m>\r\nCBW\r\n")]
    [InlineData(false, PCode.GETb1mu, 0, 2, null, null, null, null, null, "MOV AL,<m>\r\nXOR AH,AH\r\n")]
    [InlineData(false, PCode.GETb1p, 0, 2, null, null, null, null, null, "MOV AL,?<n>??[BX]\r\nCBW\r\n")]
    [InlineData(false, PCode.GETb1pu, 0, 2, null, null, null, null, null, "MOV AL,?<n>??[BX]\r\nXOR AH,AH\r\n")]
    [InlineData(false, PCode.GETb1s, 0, 2, null, null, null, null, null, "MOV AL,<n>[BP]\r\nCBW\r\n")]
    [InlineData(false, PCode.GETb1su, 0, 2, null, null, null, null, null, "MOV AL,<n>[BP]\r\nXOR AH,AH\r\n")]
    [InlineData(false, PCode.GETw1m, 0, 2, null, null, null, null, null, "MOV AX,<m>\r\n")]
    [InlineData(false, PCode.GETw1m_, 0, 2, null, null, null, null, null, "MOV AX,<m>")]
    [InlineData(false, PCode.GETw1n, 0, 2, null, null, null, null, null, "?MOV AX,<n>?XOR AX,AX?\r\n")]
    [InlineData(false, PCode.GETw1p, 0, 2, null, null, null, null, null, "MOV AX,?<n>??[BX]\r\n")]
    [InlineData(false, PCode.GETw1s, 0, 2, null, null, null, null, null, "MOV AX,<n>[BP]\r\n")]
    [InlineData(false, PCode.GETw2m, 0, 2, null, null, null, null, null, "MOV BX,<m>\r\n")]
    [InlineData(false, PCode.GETw2n, 0, 2, null, null, null, null, null, "?MOV BX,<n>?XOR BX,BX?\r\n")]
    [InlineData(false, PCode.GETw2p, 0, 2, null, null, null, null, null, "MOV BX,?<n>??[BX]\r\n")]
    [InlineData(false, PCode.GETw2s, 0, 2, null, null, null, null, null, "MOV BX,<n>[BP]\r\n")]
    [InlineData(false, PCode.GT10f, 0, 2, null, null, null, null, null, "OR AX,AX\r\nJG $+5\r\nJMP _<n>\r\n")]
    [InlineData(false, PCode.GT12, 0, 2, null, null, null, null, null, "CALL __GT\r\n")]
    [InlineData(false, PCode.GT12u, 0, 2, null, null, null, null, null, "CALL __UGT\r\n")]
    [InlineData(false, PCode.INCbp, 0, 2, null, null, null, null, null, "INC BYTE PTR [BX]\r\n")]
    [InlineData(false, PCode.INCwp, 0, 2, null, null, null, null, null, "INC WORD PTR [BX]\r\n")]
    [InlineData(false, PCode.WORD_, 0, 2, null, null, null, null, null, " DW ")]
    [InlineData(false, PCode.WORDn, 0, 2, null, null, null, null, null, " DW <n>\r\n")]
    [InlineData(false, PCode.WORDr0, 0, 2, null, null, null, null, null, " DW <n> DUP(0)\r\n")]
    [InlineData(false, PCode.JMPm, 0, 2, null, null, null, null, null, "JMP _<n>\r\n")]
    [InlineData(false, PCode.LABm, 0, 2, null, null, null, null, null, "_<n>:\r\n")]
    [InlineData(false, PCode.LE10f, 0, 2, null, null, null, null, null, "OR AX,AX\r\nJLE $+5\r\nJMP _<n>\r\n")]
    [InlineData(false, PCode.LE12, 0, 2, null, null, null, null, null, "CALL __LE\r\n")]
    [InlineData(false, PCode.LE12u, 0, 2, null, null, null, null, null, "CALL __ULE\r\n")]
    [InlineData(false, PCode.LNEG1, 0, 2, null, null, null, null, null, "CALL __LNEG\r\n")]
    [InlineData(false, PCode.LT10f, 0, 2, null, null, null, null, null, "OR AX,AX\r\nJL $+5\r\nJMP _<n>\r\n")]
    [InlineData(false, PCode.LT12, 0, 2, null, null, null, null, null, "CALL __LT\r\n")]
    [InlineData(false, PCode.LT12u, 0, 2, null, null, null, null, null, "CALL __ULT\r\n")]
    [InlineData(false, PCode.MOD12, 0, 2, null, null, null, null, null, "CWD\r\nIDIV BX\r\nMOV AX,DX\r\n")]
    [InlineData(false, PCode.MOD12u, 0, 2, null, null, null, null, null, "XOR DX,DX\r\nDIV BX\r\nMOV AX,DX\r\n")]
    [InlineData(false, PCode.MOVE21, 0, 2, null, null, null, null, null, "MOV BX,AX\r\n")]
    [InlineData(false, PCode.MUL12, 0, 2, null, null, null, null, null, "IMUL BX\r\n")]
    [InlineData(false, PCode.MUL12u, 0, 2, null, null, null, null, null, "MUL BX\r\n")]
    [InlineData(false, PCode.NE10f, 0, 2, null, null, null, null, null, "OR AX,AX\r\nJNE $+5\r\nJMP _<n>\r\n")]
    [InlineData(false, PCode.NE12, 0, 2, null, null, null, null, null, "CALL __NE\r\n")]
    [InlineData(false, PCode.NEARm, 0, 2, null, null, null, null, null, " DW _<n>\r\n")]
    [InlineData(false, PCode.OR12, 0, 2, null, null, null, null, null, "OR AX,BX\r\n")]
    [InlineData(false, PCode.PLUSn, 0, 2, null, null, null, null, null, "?+<n>??\r\n")]
    [InlineData(false, PCode.POINT1l, 0, 2, null, null, null, null, null, "MOV AX,OFFSET _<l>+<n>\r\n")]
    [InlineData(false, PCode.POINT1m, 0, 2, null, null, null, null, null, "MOV AX,OFFSET <m>\r\n")]
    [InlineData(false, PCode.POINT1s, 0, 2, null, null, null, null, null, "LEA AX,<n>[BP]\r\n")]
    [InlineData(false, PCode.POINT2m, 0, 2, null, null, null, null, null, "MOV BX,OFFSET <m>\r\n")]
    [InlineData(false, PCode.POINT2m_, 0, 2, null, null, null, null, null, "MOV BX,OFFSET <m>")]
    [InlineData(false, PCode.POINT2s, 0, 2, null, null, null, null, null, "LEA BX,<n>[BP]\r\n")]
    [InlineData(false, PCode.POP2, 0, 2, null, null, null, null, null, "POP BX\r\n")]
    [InlineData(false, PCode.PUSH1, 0, 2, null, null, null, null, null, "PUSH AX\r\n")]
    [InlineData(false, PCode.PUSH2, 0, 2, null, null, null, null, null, "PUSH BX\r\n")]
    [InlineData(false, PCode.PUSHm, 0, 2, null, null, null, null, null, "PUSH <m>\r\n")]
    [InlineData(false, PCode.PUSHp, 0, 2, null, null, null, null, null, "PUSH ?<n>??[BX]\r\n")]
    [InlineData(false, PCode.PUSHs, 0, 2, null, null, null, null, null, "PUSH ?<n>??[BP]\r\n")]
    [InlineData(false, PCode.PUT_m_, 0, 2, null, null, null, null, null, "MOV <m>")]
    [InlineData(false, PCode.PUTbm1, 0, 2, null, null, null, null, null, "MOV <m>,AL\r\n")]
    [InlineData(false, PCode.PUTbp1, 0, 2, null, null, null, null, null, "MOV [BX],AL\r\n")]
    [InlineData(false, PCode.PUTwm1, 0, 2, null, null, null, null, null, "MOV <m>,AX\r\n")]
    [InlineData(false, PCode.PUTwp1, 0, 2, null, null, null, null, null, "MOV [BX],AX\r\n")]
    [InlineData(false, PCode.rDEC1, 0, 2, null, null, null, null, null, "#DEC AX\r\n#")]
    [InlineData(false, PCode.rDEC1, 0, 2, null, null, null, null, null, "#DEC BX\r\n#")]
    [InlineData(false, PCode.REFm, 0, 2, null, null, null, null, null, "_<n>")]
    [InlineData(false, PCode.RETURN, 0, 2, null, null, null, null, null, "?MOV SP,BP\r\n??POP BP\r\nRET\r\n")]
    [InlineData(false, PCode.rINC1, 0, 2, null, null, null, null, null, "#INC AX\r\n#")]
    [InlineData(false, PCode.rINC2, 0, 2, null, null, null, null, null, "#INC BX\r\n#")]
    [InlineData(false, PCode.SUB_m_, 0, 2, null, null, null, null, null, "SUB <m>")]
    [InlineData(false, PCode.SUB12, 0, 2, null, null, null, null, null, "SUB AX,BX\r\n")]
    [InlineData(false, PCode.SUB1n, 0, 2, null, null, null, null, null, "?SUB AX,<n>\r\n??")]
    [InlineData(false, PCode.SUBbpn, 0, 2, null, null, null, null, null, "SUB BYTE PTR [BX],<n>\r\n")]
    [InlineData(false, PCode.SUBwpn, 0, 2, null, null, null, null, null, "SUB WORD PTR [BX],<n>\r\n")]
    [InlineData(false, PCode.SWAP12, 0, 2, null, null, null, null, null, "XCHG AX,BX\r\n")]
    [InlineData(false, PCode.SWAP1s, 0, 2, null, null, null, null, null, "POP BX\r\nXCHG AX,BX\r\nPUSH BX\r\n")]
    [InlineData(false, PCode.SWITCH, 0, 2, null, null, null, null, null, "CALL __SWITCH\r\n")]
    [InlineData(false, PCode.XOR12, 0, 2, null, null, null, null, null, "XOR AX,BX\r\n")]
    [InlineData(true, PCode.ADD12, 0, 2, 1, PCode.ADD12, 0, PCode.ADD12, 0, "")]
    [InlineData(true, PCode.GETb1pu, 0, 2, 2, PCode.MOVE21, 0, PCode.GETb1pu, 0, "")]
    [InlineData(true, PCode.SUB12, 0, 2, 2, PCode.SWAP12, 0, PCode.SUB12, 0, "")]
    [InlineData(true, PCode.PUSH1, 0, 0, 1, PCode.PUSH1, 0, PCode.PUSH1, 0, "")]
    [InlineData(true, PCode.POP2, 0, 4, 1, PCode.POP2, 0, PCode.POP2, 0, "")]
    [InlineData(true, PCode.ADDSP, 4, 4, 1, PCode.ADDSP, 2, PCode.ADDSP, 2, "")]
    public async Task GensAsync(
        bool setStage,
        PCode pCode,
        int value,
        int expectedCsp,
        int? expectedSNext,
        PCode? expectedFirstPCode,
        int? expectedFirstValue,
        PCode? expectedLastPCode,
        int? expectedLastValue,
        string expectedGen)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        Collection<KeyValuePair<PCode, int>>? stage = setStage ? [] : null;
        var storage = ArrangeStorage(output, stage);
        var sut = new BackEnd(storage);
        sut.SetCodes();

        await sut.GenAsync(pCode, value);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actualGen = await reader.ReadToEndAsync();

        Assert.Equal(expectedCsp, storage.Csp);
        Assert.Equal(expectedSNext, storage.SNext);
        if (setStage)
        {
            var first = storage.Stage?.FirstOrDefault();
            Assert.Equal(expectedFirstPCode, first?.Key);
            Assert.Equal(expectedFirstValue, first?.Value);
            var last = storage.Stage?.LastOrDefault();
            Assert.Equal(expectedLastPCode, last?.Key);
            Assert.Equal(expectedLastValue, last?.Value);
        }
        else
        {
            Assert.Equal(expectedGen, actualGen);
        }
    }

    /// <summary>
    /// Tests that can clear stage.
    /// </summary>
    /// <param name="setStage">Whether to set stage.</param>
    /// <param name="before">
    /// New previous position in queue.
    /// </param>
    /// <param name="start">
    /// New starting position in queue.
    /// </param>
    /// <param name="expectedSNext">Expected next index in stage.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(false, null, null, null)]
    [InlineData(false, null, 0, null)]
    [InlineData(false, 0, null, null)]
    [InlineData(false, 0, 0, null)]
    [InlineData(true, null, null, null)]
    [InlineData(true, null, 0, null)]
    [InlineData(true, 0, null, 0)]
    [InlineData(true, 0, 0, 0)]
    public async Task ClearsStageAsync(
        bool setStage,
        int? before,
        int? start,
        int? expectedSNext)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        Collection<KeyValuePair<PCode, int>>? stage = setStage ? [] : null;
        stage?.Add(new(PCode.ADD12, 0));
        var storage = ArrangeStorage(output, stage);
        var sut = new BackEnd(storage);
        sut.SetCodes();

        await sut.ClearStageAsync(before, start);

        Assert.Equal(expectedSNext, storage.SNext);
    }

    /// <summary>
    /// Tests that can dump literals.
    /// </summary>
    /// <param name="size">Literal size.</param>
    /// <param name="lits">Literals to dump.</param>
    /// <param name="expectedPCode">Expected p-code.</param>
    /// <param name="expectedDump">Expected dumped literals.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(1, "", null, "")]
    [InlineData(1, "-1", PCode.BYTE_, "-1\r\n")]
    [InlineData(1, "0", PCode.BYTE_, "0\r\n")]
    [InlineData(1, "1", PCode.BYTE_, "1\r\n")]
    [InlineData(1, "0,1,2,3,4,5,6,7,8,9,10,11", PCode.BYTE_, "0,1,2,3,4,5,6,7,8,9\r\n10,11\r\n")]
    [InlineData(2, "", null, "")]
    [InlineData(2, "-1,-1", PCode.WORD_, "-1\r\n")]
    [InlineData(2, "0,0", PCode.WORD_, "0\r\n")]
    [InlineData(2, "1,0", PCode.WORD_, "1\r\n")]
    [InlineData(2, "0,1", PCode.WORD_, "256\r\n")]
    [InlineData(2, "0,0,1,0,2,0,3,0,4,0,5,0,6,0,7,0,8,0,9,0,10,0,11,0", PCode.WORD_, "0,1,2,3,4,5,6,7,8,9\r\n10,11\r\n")]
    [InlineData(2, "0,1,1,1,2,1,3,1,4,1,5,1,6,1,7,1,8,1,9,1,10,1,11,1", PCode.WORD_, "256,257,258,259,260,261,262,263,264,265\r\n266,267\r\n")]
    public async Task DumpsLiteralsAsync(
        int size,
        string lits,
        PCode? expectedPCode,
        string expectedDump)
    {
        ArgumentNullException.ThrowIfNull(lits);
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        Collection<KeyValuePair<PCode, int>>? stage = [];
        var litQ = lits
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(sbyte.Parse);
        var storage = ArrangeStorage(output, stage, litQ: [.. litQ]);
        var sut = new BackEnd(storage);

        await sut.DumpLitsAsync(size);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actualDump = await reader.ReadToEndAsync();

        Assert.Equal(expectedPCode, stage.Count > 0 ? stage[0].Key : null);
        Assert.Equal(expectedDump, actualDump);
    }

    /// <summary>
    /// Tests that can dump zero.
    /// </summary>
    /// <param name="size">Size of zero to dump.</param>
    /// <param name="count">Number of zeroes to dump.</param>
    /// <param name="expectedPCode">Expected p-code.</param>
    /// <param name="expectedValue">Expected value.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(1, 0, null, null)]
    [InlineData(1, 1, PCode.BYTEr0, 1)]
    [InlineData(1, 2, PCode.BYTEr0, 2)]
    [InlineData(2, 0, null, null)]
    [InlineData(2, 1, PCode.WORDr0, 1)]
    [InlineData(2, 2, PCode.WORDr0, 2)]
    public async Task DumpsZeroAsync(
        int size,
        int count,
        PCode? expectedPCode,
        int? expectedValue)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        Collection<KeyValuePair<PCode, int>>? stage = [];
        var storage = ArrangeStorage(output, stage);
        var sut = new BackEnd(storage);

        await sut.DumpZeroAsync(size, count);

        Assert.Equal(expectedPCode, stage.Count > 0 ? stage[0].Key : null);
        Assert.Equal(expectedValue, stage.Count > 0 ? stage[0].Value : null);
    }

    /// <summary>
    /// Tests that can transition between segments.
    /// </summary>
    /// <param name="oldSeg">Segment to change from.</param>
    /// <param name="newSeg">Segment to change to.</param>
    /// <param name="expected">Expected output.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(SegmentType.None, SegmentType.None, "")]
    [InlineData(SegmentType.None, SegmentType.DataSeg, BeginData)]
    [InlineData(SegmentType.None, SegmentType.CodeSeg, BeginCode)]
    [InlineData(SegmentType.DataSeg, SegmentType.None, EndData)]
    [InlineData(SegmentType.DataSeg, SegmentType.DataSeg, "")]
    [InlineData(SegmentType.DataSeg, SegmentType.CodeSeg, $"{EndData}{BeginCode}")]
    [InlineData(SegmentType.CodeSeg, SegmentType.None, EndCode)]
    [InlineData(SegmentType.CodeSeg, SegmentType.DataSeg, $"{EndCode}{BeginData}")]
    [InlineData(SegmentType.CodeSeg, SegmentType.CodeSeg, "")]
    public async Task TransitionsSegmentAsync(
        SegmentType oldSeg, SegmentType newSeg, string expected)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = Arrange(output, oldSeg: oldSeg);

        await sut.ToSegAsync(newSeg);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that can declare entry point.
    /// </summary>
    /// <param name="ident">Identity code of object being defined.</param>
    /// <param name="ssName">Static symbol name.</param>
    /// <param name="expectedSeg">Expected segment beginning.</param>
    /// <param name="expectedSuffix">Expected declaration suffix.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(SymbolIdentity.Variable, "a", BeginData, "")]
    [InlineData(SymbolIdentity.Function, "foo", BeginCode, ":\r\n")]
    public async Task DeclaresEntryAsync(
        SymbolIdentity ident,
        string ssName,
        string expectedSeg,
        string expectedSuffix)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = Arrange(output, ssName: ssName);

        await sut.PublicAsync(ident);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        var outName = $"_{ssName?.ToUpperInvariant()}";
        var expected = @$"{expectedSeg}PUBLIC {outName}
{outName}{expectedSuffix}";
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that can declare external.
    /// </summary>
    /// <param name="name">Extern name.</param>
    /// <param name="size">Extern size.</param>
    /// <param name="ident">Identity code of object being defined.</param>
    /// <param name="expectedSeg">Expected segment beginning.</param>
    /// <param name="expectedSuffix">Expected declaration suffix.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("b", 1, SymbolIdentity.Variable, BeginData, "BYTE")]
    [InlineData("w", 2, SymbolIdentity.Variable, BeginData, "WORD")]
    [InlineData("foo", 0, SymbolIdentity.Function, BeginCode, "NEAR")]
    public async Task DeclaresExternAsync(
        string name,
        int size,
        SymbolIdentity ident,
        string expectedSeg,
        string expectedSuffix)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = Arrange(output);

        await sut.ExternalAsync(name, size, ident);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        var outName = $"_{name?.ToUpperInvariant()}";
        var expected = $"{expectedSeg}EXTRN {outName}:{expectedSuffix}\r\n";
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that can point to following object(s).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task PointsAsync()
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var sut = Arrange(output);

        await sut.PointAsync();
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        var expected = " DW $+2\r\n";
        Assert.Equal(expected, actual);
    }

    private static BackEnd Arrange(
        StreamWriter output,
        Collection<KeyValuePair<PCode, int>>? stage = null,
        SegmentType oldSeg = SegmentType.None,
        SymbolTable? symbolTable = null,
        Collection<sbyte>? litQ = null,
        string? ssName = null)
    {
        var storage = ArrangeStorage(
            output,
            stage,
            oldSeg,
            symbolTable ?? new([], []),
            litQ,
            ssName);

        return new BackEnd(storage);
    }

    private static Storage ArrangeStorage(
        StreamWriter output,
        Collection<KeyValuePair<PCode, int>>? stage = null,
        SegmentType oldSeg = SegmentType.None,
        SymbolTable? symbolTable = null,
        Collection<sbyte>? litQ = null,
        string? ssName = null)
    {
        return new Storage(
            0,
            Machine.Bpw,
            output,
            stage,
            Storage.StageSize,
            oldSeg,
            false,
            symbolTable ?? new([], []),
            litQ ?? [],
            ssName);
    }
}
