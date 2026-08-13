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
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// Tests the expression analyzer.
/// </summary>
public class AnalyzerTests
{
    /// <summary>
    /// Tests that can analyze lowest level of expression.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expected">Expected fetch flag.</param>
    /// <param name="expectedSymbolIdent">Expected symbol identity.</param>
    /// <param name="expectedSymbolType">Expected symbol type.</param>
    /// <param name="expectedSymbolClass">Expected symbol storage class.</param>
    /// <param name="expectedSymbolSize">Expected symbol size.</param>
    /// <param name="expectedSymbolOffset">Expected symbol offset.</param>
    /// <param name="expectedSymbolName">Expected symbol name.</param>
    /// <param name="expectedIndirectType">Expected indirect type.</param>
    /// <param name="expectedAddressType">Expected address type.</param>
    /// <param name="expectedConstantType">Expected constant type.</param>
    /// <param name="expectedConstantValue">Expected constant value.</param>
    /// <param name="expectedHighestBinaryOp">
    /// Expected highest binary op.
    /// </param>
    /// <param name="expectedStageIndex">Expected stage index.</param>
    /// <param name="expectedCode">Expected generated code.</param>
    /// <param name="expectedLits">String of expected lit pool bytes.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
#pragma warning disable SA1118 // Parameter should not span multiple lines
#pragma warning disable SA1117 // Parameters should be on same line or separate lines
    [InlineData("sizeof(char)", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 1, null, null,
@"MOV AX,1
", "")]
    [InlineData("c", true,
SymbolIdentity.Variable, SymbolType.Chr, SymbolClass.Automatic, 1, -10, "c", SymbolType.Chr, null, null, 0, null, null,
@"LEA AX,-10[BP]
", "")]
    [InlineData("c++", false,
SymbolIdentity.Variable, SymbolType.Chr, SymbolClass.Automatic, 1, -10, "c", SymbolType.Chr, null, null, 0, null, null,
@"LEA AX,-10[BP]
MOV BX,AX
MOV AL,[BX]
CBW
INC AX
MOV [BX],AL
DEC AX
", "")]
    [InlineData("sizeof(char*)", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 2, null, null,
@"MOV AX,2
", "")]
    [InlineData("ca3", false,
SymbolIdentity.Array, SymbolType.Chr, SymbolClass.Automatic, 3, -8, "ca3", SymbolType.Chr, SymbolType.Chr, null, 0, null, null,
@"LEA AX,-8[BP]
", "")]
    [InlineData("cp", true,
SymbolIdentity.Pointer, SymbolType.Chr, SymbolClass.Automatic, 2, -6, "cp", SymbolType.UInt, SymbolType.Chr, null, 0, null, null,
@"LEA AX,-6[BP]
", "")]
    [InlineData("*cp", true,
SymbolIdentity.Pointer, SymbolType.Chr, SymbolClass.Automatic, 2, -6, "cp", SymbolType.Chr, null, null, 1, null, null,
@"LEA AX,-6[BP]
MOV BX,AX
MOV AX,[BX]
", "")]
    [InlineData("sizeof(unsigned char)", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 1, null, null,
@"MOV AX,1
", "")]
    [InlineData("uc", true,
SymbolIdentity.Variable, SymbolType.UChr, SymbolClass.Automatic, 1, -4, "uc", SymbolType.UChr, null, null, 0, null, null,
@"LEA AX,-4[BP]
", "")]
    [InlineData("sizeof(unsigned char*)", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 2, null, null,
@"MOV AX,2
", "")]
    [InlineData("uca3", false,
SymbolIdentity.Array, SymbolType.UChr, SymbolClass.Automatic, 3, -2, "uca3", SymbolType.UChr, SymbolType.UChr, null, 0, null, null,
@"LEA AX,-2[BP]
", "")]
    [InlineData("sizeof uca3", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 3, null, null,
@"MOV AX,3
", "")]
    [InlineData("sizeof(uca3)", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 3, null, null,
@"MOV AX,3
", "")]
    [InlineData("ucp", true,
SymbolIdentity.Pointer, SymbolType.UChr, SymbolClass.Automatic, 2, 0, "ucp", SymbolType.UInt, SymbolType.UChr, null, 0, null, null,
@"LEA AX,0[BP]
", "")]
    [InlineData("ucp - &ucp", false,
SymbolIdentity.Pointer, SymbolType.UChr, SymbolClass.Automatic, 2, 0, "ucp", SymbolType.UInt, null, null, 0, PCode.SUB12, null,
@"LEA AX,0[BP]
MOV BX,AX
MOV AX,[BX]
PUSH AX
LEA AX,0[BP]
POP BX
XCHG AX,BX
SUB AX,BX
", "")]
    [InlineData("!ucp", false,
null, null, null, null, null, null, SymbolType.UInt, SymbolType.UChr, null, 1, null, null,
@"LEA AX,0[BP]
MOV BX,AX
MOV AX,[BX]
CALL __LNEG
", "")]
    [InlineData("sizeof(int)", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 2, null, null,
@"MOV AX,2
", "")]
    [InlineData("i", true,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, null, null,
@"LEA AX,2[BP]
", "")]
    [InlineData("i ? 1 : 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", null, null, null, 0, null, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
OR AX,AX
JNE $+5
JMP _1
MOV AX,1
JMP _2
_1:
MOV AX,2
_2:
", "")]
    [InlineData("i ? c : 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Chr, null, null, 0, null, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
OR AX,AX
JNE $+5
JMP _1
LEA AX,-10[BP]
MOV BX,AX
MOV AL,[BX]
CBW
JMP _2
_1:
MOV AX,2
_2:
", "")]
    [InlineData("i ? c : gc", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", null, null, null, 0, null, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
OR AX,AX
JNE $+5
JMP _1
LEA AX,-10[BP]
MOV BX,AX
MOV AL,[BX]
CBW
JMP _2
_1:
MOV AL,_GC
CBW
_2:
", "")]
    [InlineData("i || 2", false,
null, null, null, null, null, null, null, null, null, 0, null, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
OR AX,AX
JE $+5
JMP _1
MOV AX,2
OR AX,AX
JE $+5
JMP _1
XOR AX,AX
JMP _2
_1:
MOV AX,1
_2:
", "")]
    [InlineData("i && 2", false,
null, null, null, null, null, null, null, null, null, 0, null, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
OR AX,AX
JNE $+5
JMP _1
MOV AX,2
OR AX,AX
JNE $+5
JMP _1
MOV AX,1
JMP _2
_1:
XOR AX,AX
_2:
", "")]
    [InlineData("i | 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.OR12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
OR AX,BX
", "")]
    [InlineData("i ^ 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.XOR12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
XOR AX,BX
", "")]
    [InlineData("i & 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.AND12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
AND AX,BX
", "")]
    [InlineData("i == 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.EQ12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
CALL __EQ
", "")]
    [InlineData("i != 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.NE12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
CALL __NE
", "")]
    [InlineData("i <= 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.LE12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
CALL __LE
", "")]
    [InlineData("i < 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.LT12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
CALL __LT
", "")]
    [InlineData("i > 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.GT12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
CALL __GT
", "")]
    [InlineData("i >= 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.GE12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
CALL __GE
", "")]
    [InlineData("i << 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.ASL12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
MOV CX,AX
MOV AX,BX
SAL AX,CL
", "")]
    [InlineData("i >> 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.ASR12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
MOV CX,AX
MOV AX,BX
SAR AX,CL
", "")]
    [InlineData("i + 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.ADD12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,2
ADD AX,BX
", "")]
    [InlineData("2 + i", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", null, null, null, 2, PCode.ADD12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,2
ADD AX,BX
", "")]
    [InlineData("i - 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.SUB12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
XCHG AX,BX
SUB AX,BX
", "")]
    [InlineData("2 - i", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", null, null, null, 2, PCode.SUB12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,2
XCHG AX,BX
SUB AX,BX
", "")]
    [InlineData("i * 2", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.MUL12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
IMUL BX
", "")]
    [InlineData("2 / i", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", null, null, null, 2, PCode.DIV12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,2
XCHG AX,BX
CWD
IDIV BX
", "")]
    [InlineData("i % i", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, PCode.MOD12, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
PUSH AX
LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
POP BX
XCHG AX,BX
CWD
IDIV BX
MOV AX,DX
", "")]
    [InlineData("i--", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Automatic, 2, 2, "i", SymbolType.Int, null, null, 0, null, null,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
DEC AX
MOV [BX],AX
INC AX
", "")]
    [InlineData("sizeof(int*)", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 2, null, null,
@"MOV AX,2
", "")]
    [InlineData("ia3", false,
SymbolIdentity.Array, SymbolType.Int, SymbolClass.Automatic, 6, 4, "ia3", SymbolType.Int, SymbolType.Int, null, 0, null, null,
@"LEA AX,4[BP]
", "")]
    [InlineData("ip", true,
SymbolIdentity.Pointer, SymbolType.Int, SymbolClass.Automatic, 2, 6, "ip", SymbolType.UInt, SymbolType.Int, null, 0, null, null,
@"LEA AX,6[BP]
", "")]
    [InlineData("ip - &ip", false,
SymbolIdentity.Pointer, SymbolType.Int, SymbolClass.Automatic, 2, 6, "ip", SymbolType.UInt, null, null, 0, PCode.SUB12, null,
@"LEA AX,6[BP]
MOV BX,AX
MOV AX,[BX]
PUSH AX
LEA AX,6[BP]
POP BX
XCHG AX,BX
SUB AX,BX
XCHG AX,BX
MOV AX,1
MOV CX,AX
MOV AX,BX
SAR AX,CL
", "")]
    [InlineData("~ip", false,
null, null, null, null, null, null, SymbolType.UInt, SymbolType.Int, null, -1, null, null,
@"LEA AX,6[BP]
MOV BX,AX
MOV AX,[BX]
NOT AX
", "")]
    [InlineData("sizeof(unsigned)", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 2, null, null,
@"MOV AX,2
", "")]
    [InlineData("sizeof(unsigned int)", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 2, null, null,
@"MOV AX,2
", "")]
    [InlineData("ui", true,
SymbolIdentity.Variable, SymbolType.UInt, SymbolClass.Automatic, 2, 8, "ui", SymbolType.UInt, null, null, 0, null, null,
@"LEA AX,8[BP]
", "")]
    [InlineData("sizeof(unsigned*)", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 2, null, null,
@"MOV AX,2
", "")]
    [InlineData("sizeof(unsigned int*)", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 2, null, null,
@"MOV AX,2
", "")]
    [InlineData("&ui", false,
SymbolIdentity.Variable, SymbolType.UInt, SymbolClass.Automatic, 2, 8, "ui", SymbolType.UInt, SymbolType.UInt, null, 0, null, null,
@"LEA AX,8[BP]
", "")]
    [InlineData("uia3", false,
SymbolIdentity.Array, SymbolType.UInt, SymbolClass.Automatic, 6, 10, "uia3", SymbolType.UInt, SymbolType.UInt, null, 0, null, null,
@"LEA AX,10[BP]
", "")]
    [InlineData("uip", true,
SymbolIdentity.Pointer, SymbolType.UInt, SymbolClass.Automatic, 2, 12, "uip", SymbolType.UInt, SymbolType.UInt, null, 0, null, null,
@"LEA AX,12[BP]
", "")]
    [InlineData("--uip", false,
SymbolIdentity.Pointer, SymbolType.UInt, SymbolClass.Automatic, 2, 12, "uip", SymbolType.UInt, SymbolType.UInt, null, 0, null, null,
@"LEA AX,12[BP]
MOV BX,AX
MOV AX,[BX]
DEC AX
DEC AX
MOV [BX],AX
", "")]
    [InlineData("gc", true,
SymbolIdentity.Variable, SymbolType.Chr, SymbolClass.Static, 1, 0, "gc", null, null, null, 0, null, null,
"", "")]
    [InlineData("&gc", false,
SymbolIdentity.Variable, SymbolType.Chr, SymbolClass.Static, 1, 0, "gc", SymbolType.Chr, SymbolType.Chr, null, 0, null, null,
@"MOV AX,OFFSET _GC
", "")]
    [InlineData("gca3", false,
SymbolIdentity.Array, SymbolType.Chr, SymbolClass.Static, 3, 0, "gca3", SymbolType.Chr, SymbolType.Chr, null, 0, null, null,
@"MOV AX,OFFSET _GCA3
", "")]
    [InlineData("gcp", true,
SymbolIdentity.Pointer, SymbolType.Chr, SymbolClass.Static, 2, 0, "gcp", null, SymbolType.Chr, null, 0, null, null,
"", "")]
    [InlineData("1 + gcp", false,
SymbolIdentity.Pointer, SymbolType.Chr, SymbolClass.Static, 2, 0, "gcp", null, SymbolType.Chr, null, 1, PCode.ADD12, null,
@"MOV AX,_GCP
MOV BX,1
ADD AX,BX
", "")]
    [InlineData("guc", true,
SymbolIdentity.Variable, SymbolType.UChr, SymbolClass.Static, 1, 0, "guc", null, null, null, 0, null, null,
"", "")]
    [InlineData("~guc", false,
null, null, null, null, null, null, null, null, null, -1, null, null,
@"MOV AL,_GUC
XOR AH,AH
NOT AX
", "")]
    [InlineData("guca3", false,
SymbolIdentity.Array, SymbolType.UChr, SymbolClass.Static, 3, 0, "guca3", SymbolType.UChr, SymbolType.UChr, null, 0, null, null,
@"MOV AX,OFFSET _GUCA3
", "")]
    [InlineData("gucp", true,
SymbolIdentity.Pointer, SymbolType.UChr, SymbolClass.Static, 2, 0, "gucp", null, SymbolType.UChr, null, 0, null, null,
"", "")]
    [InlineData("gi", true,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.Static, 2, 0, "gi", null, null, null, 0, null, null,
"", "")]
    [InlineData("gia3", false,
SymbolIdentity.Array, SymbolType.Int, SymbolClass.Static, 6, 0, "gia3", SymbolType.Int, SymbolType.Int, null, 0, null, null,
@"MOV AX,OFFSET _GIA3
", "")]
    [InlineData("gip", true,
SymbolIdentity.Pointer, SymbolType.Int, SymbolClass.Static, 2, 0, "gip", null, SymbolType.Int, null, 0, null, null,
"", "")]
    [InlineData("*gip", true,
SymbolIdentity.Pointer, SymbolType.Int, SymbolClass.Static, 2, 0, "gip", SymbolType.Int, null, null, 1, null, null,
@"MOV AX,_GIP
", "")]
    [InlineData("-*gip", false,
null, null, null, null, null, null, SymbolType.Int, null, null, -1, null, null,
@"MOV AX,_GIP
MOV BX,AX
MOV AX,[BX]
NEG AX
", "")]
    [InlineData("gui", true,
SymbolIdentity.Variable, SymbolType.UInt, SymbolClass.Static, 2, 0, "gui", null, null, null, 0, null, null,
"", "")]
    [InlineData("!gui", false,
null, null, null, null, null, null, null, null, null, 1, null, null,
@"MOV AX,_GUI
CALL __LNEG
", "")]
    [InlineData("guia3", false,
SymbolIdentity.Array, SymbolType.UInt, SymbolClass.Static, 6, 0, "guia3", SymbolType.UInt, SymbolType.UInt, null, 0, null, null,
@"MOV AX,OFFSET _GUIA3
", "")]
    [InlineData("guip", true,
SymbolIdentity.Pointer, SymbolType.UInt, SymbolClass.Static, 2, 0, "guip", null, SymbolType.UInt, null, 0, null, null,
"", "")]
    [InlineData("1 - guip", false,
SymbolIdentity.Pointer, SymbolType.UInt, SymbolClass.Static, 2, 0, "guip", null, SymbolType.UInt, null, 1, PCode.SUB12, null,
@"MOV AX,_GUIP
MOV BX,2
XCHG AX,BX
SUB AX,BX
", "")]
    [InlineData("ec", true,
SymbolIdentity.Variable, SymbolType.Chr, SymbolClass.External, 1, 0, "ec", null, null, null, 0, null, null,
"", "")]
    [InlineData("eca3", false,
SymbolIdentity.Array, SymbolType.Chr, SymbolClass.External, 3, 0, "eca3", SymbolType.Chr, SymbolType.Chr, null, 0, null, null,
@"MOV AX,OFFSET _ECA3
", "")]
    [InlineData("ecp", true,
SymbolIdentity.Pointer, SymbolType.Chr, SymbolClass.External, 2, 0, "ecp", null, SymbolType.Chr, null, 0, null, null,
"", "")]
    [InlineData("euc", true,
SymbolIdentity.Variable, SymbolType.UChr, SymbolClass.External, 1, 0, "euc", null, null, null, 0, null, null,
"", "")]
    [InlineData("euca3", false,
SymbolIdentity.Array, SymbolType.UChr, SymbolClass.External, 3, 0, "euca3", SymbolType.UChr, SymbolType.UChr, null, 0, null, null,
@"MOV AX,OFFSET _EUCA3
", "")]
    [InlineData("eucp", true,
SymbolIdentity.Pointer, SymbolType.UChr, SymbolClass.External, 2, 0, "eucp", null, SymbolType.UChr, null, 0, null, null,
"", "")]
    [InlineData("ei", true,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.External, 2, 0, "ei", null, null, null, 0, null, null,
"", "")]
    [InlineData("++ei", false,
SymbolIdentity.Variable, SymbolType.Int, SymbolClass.External, 2, 0, "ei", null, null, null, 0, null, null,
@"MOV AX,_EI
INC AX
MOV _EI,AX
", "")]
    [InlineData("eia3", false,
SymbolIdentity.Array, SymbolType.Int, SymbolClass.External, 6, 0, "eia3", SymbolType.Int, SymbolType.Int, null, 0, null, null,
@"MOV AX,OFFSET _EIA3
", "")]
    [InlineData("eip", true,
SymbolIdentity.Pointer, SymbolType.Int, SymbolClass.External, 2, 0, "eip", null, SymbolType.Int, null, 0, null, null,
"", "")]
    [InlineData("eui", true,
SymbolIdentity.Variable, SymbolType.UInt, SymbolClass.External, 2, 0, "eui", null, null, null, 0, null, null,
"", "")]
    [InlineData("euia3", false,
SymbolIdentity.Array, SymbolType.UInt, SymbolClass.External, 6, 0, "euia3", SymbolType.UInt, SymbolType.UInt, null, 0, null, null,
@"MOV AX,OFFSET _EUIA3
", "")]
    [InlineData("euip", true,
SymbolIdentity.Pointer, SymbolType.UInt, SymbolClass.External, 2, 0, "euip", null, SymbolType.UInt, null, 0, null, null,
"", "")]
    [InlineData("foo", false,
null, null, null, null, null, null, null, null, null, 0, null, null,
@"MOV AX,OFFSET _FOO
", "")]
    [InlineData("foo()", false,
null, null, null, null, null, null, null, null, null, 0, null, null,
@"XOR CL,CL
CALL _FOO
", "")]
    [InlineData("bar", false,
null, null, null, null, null, null, null, null, null, 0, null, null,
@"MOV AX,OFFSET _BAR
", "")]
    [InlineData("bar()", false,
null, null, null, null, null, null, null, null, null, 0, null, null,
@"XOR CL,CL
CALL _BAR
", "")]
    [InlineData("0", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("!0", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 1, null, null,
@"MOV AX,1
", "")]
    [InlineData("00", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("-0", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("+0", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("-1", false,
null, null, null, null, null, null, null, null, SymbolType.Int, -1, null, null,
@"MOV AX,-1
", "")]
    [InlineData("!-1", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("+1", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 1, null, null,
@"MOV AX,1
", "")]
    [InlineData("!+1", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("01", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 1, null, null,
@"MOV AX,1
", "")]
    [InlineData(" 1", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 1, null, null,
@"MOV AX,1
", "")]
    [InlineData("10", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 10, null, null,
@"MOV AX,10
", "")]
    [InlineData("!10", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("*10", true,
null, null, null, null, null, null, SymbolType.Int, null, null, 1, null, null,
@"MOV AX,10
", "")]
    [InlineData("-32769", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 32767, null, null,
@"MOV AX,32767
", "")]
    [InlineData("-32768", false,
null, null, null, null, null, null, null, null, SymbolType.Int, -32768, null, null,
@"MOV AX,-32768
", "")]
    [InlineData("32767", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 32767, null, null,
@"MOV AX,32767
", "")]
    [InlineData("32768", false,
null, null, null, null, null, null, null, null, SymbolType.UInt, -32768, null, null,
@"MOV AX,-32768
", "")]
    [InlineData("65535", false,
null, null, null, null, null, null, null, null, SymbolType.UInt, -1, null, null,
@"MOV AX,-1
", "")]
    [InlineData("65536", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("131071", false,
null, null, null, null, null, null, null, null, SymbolType.UInt, -1, null, null,
@"MOV AX,-1
", "")]
    [InlineData("000", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("-00", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("-01", false,
null, null, null, null, null, null, null, null, SymbolType.Int, -1, null, null,
@"MOV AX,-1
", "")]
    [InlineData("001", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 1, null, null,
@"MOV AX,1
", "")]
    [InlineData("010", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 8, null, null,
@"MOV AX,8
", "")]
    [InlineData("018", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 1, null, null,
@"MOV AX,1
", "")]
    [InlineData("077", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 63, null, null,
@"MOV AX,63
", "")]
    [InlineData("0777", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 511, null, null,
@"MOV AX,511
", "")]
    [InlineData("-0100001", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 32767, null, null,
@"MOV AX,32767
", "")]
    [InlineData("-0100000", false,
null, null, null, null, null, null, null, null, SymbolType.Int, -32768, null, null,
@"MOV AX,-32768
", "")]
    [InlineData("077777", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 32767, null, null,
@"MOV AX,32767
", "")]
    [InlineData("0100000", false,
null, null, null, null, null, null, null, null, SymbolType.UInt, -32768, null, null,
@"MOV AX,-32768
", "")]
    [InlineData("0177777", false,
null, null, null, null, null, null, null, null, SymbolType.UInt, -1, null, null,
@"MOV AX,-1
", "")]
    [InlineData("0200000", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("0377777", false,
null, null, null, null, null, null, null, null, SymbolType.UInt, -1, null, null,
@"MOV AX,-1
", "")]
    [InlineData("00x0", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("-0x00", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("-0x01", false,
null, null, null, null, null, null, null, null, SymbolType.Int, -1, null, null,
@"MOV AX,-1
", "")]
    [InlineData("0x10", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 16, null, null,
@"MOV AX,16
", "")]
    [InlineData("~0x10", false,
null, null, null, null, null, null, null, null, SymbolType.Int, -17, null, null,
@"MOV AX,-17
", "")]
    [InlineData("0x1G", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 1, null, null,
@"MOV AX,1
", "")]
    [InlineData("0xFF", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 255, null, null,
@"MOV AX,255
", "")]
    [InlineData("0xFG", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 15, null, null,
@"MOV AX,15
", "")]
    [InlineData("-0x8001", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 32767, null, null,
@"MOV AX,32767
", "")]
    [InlineData("-0x8000", false,
null, null, null, null, null, null, null, null, SymbolType.Int, -32768, null, null,
@"MOV AX,-32768
", "")]
    [InlineData("0x7FFF", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 32767, null, null,
@"MOV AX,32767
", "")]
    [InlineData("0x8000", false,
null, null, null, null, null, null, null, null, SymbolType.UInt, -32768, null, null,
@"MOV AX,-32768
", "")]
    [InlineData("0xFFFF", false,
null, null, null, null, null, null, null, null, SymbolType.UInt, -1, null, null,
@"MOV AX,-1
", "")]
    [InlineData("0x10000", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("0x1FFFF", false,
null, null, null, null, null, null, null, null, SymbolType.UInt, -1, null, null,
@"MOV AX,-1
", "")]
    [InlineData("''", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 0, null, null,
@"XOR AX,AX
", "")]
    [InlineData("'a'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 'a', null, null,
@"MOV AX,97
", "")]
    [InlineData(" 'a'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, 'a', null, null,
@"MOV AX,97
", "")]
    [InlineData("'\\\\'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, '\\', null, null,
@"MOV AX,92
", "")]
    [InlineData("'\\n'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, '\n', null, null,
@"MOV AX,10
", "")]
    [InlineData("'\\t'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, '\t', null, null,
@"MOV AX,9
", "")]
    [InlineData("'\\b'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, '\b', null, null,
@"MOV AX,8
", "")]
    [InlineData("'\\f'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, '\f', null, null,
@"MOV AX,12
", "")]
    [InlineData("'\\0'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, '\0', null, null,
@"XOR AX,AX
", "")]
    [InlineData("'\\1'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, (char)1, null, null,
@"MOV AX,1
", "")]
    [InlineData("'\\9'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, '9', null, null,
@"MOV AX,57
", "")]
    [InlineData("'\\12'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, (char)10, null, null,
@"MOV AX,10
", "")]
    [InlineData("'\\123'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, (char)83, null, null,
@"MOV AX,83
", "")]
    [InlineData("'\\1234'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, ((char)83 << 8) + '4', null, null,
@"MOV AX,21300
", "")]
    [InlineData("'12'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, ('1' << 8) + '2', null, null,
@"MOV AX,12594
", "")]
    [InlineData("'123'", false,
null, null, null, null, null, null, null, null, SymbolType.Int, ('2' << 8) + '3', null, null,
@"MOV AX,12851
", "")]
    [InlineData("\"\"", false,
null, null, null, null, null, null, null, null, null, 0, null, null,
@"MOV AX,OFFSET _0+0
", "")]
    [InlineData("\"a\"", false,
null, null, null, null, null, null, null, null, null, 0, null, null,
@"MOV AX,OFFSET _0+0
", "a")]
    [InlineData(" \"a\"", false,
null, null, null, null, null, null, null, null, null, 0, null, null,
@"MOV AX,OFFSET _0+0
", "a")]
    [InlineData("\"abc\"", false,
null, null, null, null, null, null, null, null, null, 0, null, null,
@"MOV AX,OFFSET _0+0
", "abc")]
#pragma warning restore SA1117 // Parameters should be on same line or separate lines
#pragma warning restore SA1118 // Parameter should not span multiple lines
    public async Task ParsesLevel1Async(
        string inputText,
        bool expected,
        SymbolIdentity? expectedSymbolIdent,
        SymbolType? expectedSymbolType,
        SymbolClass? expectedSymbolClass,
        int? expectedSymbolSize,
        int? expectedSymbolOffset,
        string? expectedSymbolName,
        SymbolType? expectedIndirectType,
        SymbolType? expectedAddressType,
        SymbolType? expectedConstantType,
        short expectedConstantValue,
        PCode? expectedHighestBinaryOp,
        int? expectedStageIndex,
        string expectedCode,
        string expectedLits)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, backEnd, storage) = Arrange(output: output, input: input);
        var @is = new Expression();
        var (before, start) = backEnd.SetStage();

        var actual = await sut.Level1Async(@is);
        await backEnd.ClearStageAsync(before, start);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actualOutput = await reader.ReadToEndAsync();

        Assert.Equal(expected, actual);
        Assert.Equal(expectedSymbolIdent, @is.SymbolTableEntry?.Ident);
        Assert.Equal(expectedSymbolType, @is.SymbolTableEntry?.Type);
        Assert.Equal(expectedSymbolClass, @is.SymbolTableEntry?.Class);
        Assert.Equal(expectedSymbolSize, @is.SymbolTableEntry?.Size);
        Assert.Equal(expectedSymbolOffset, @is.SymbolTableEntry?.Offset);
        Assert.Equal(expectedSymbolName, @is.SymbolTableEntry?.Name);
        Assert.Equal(expectedIndirectType, @is.IndirectType);
        Assert.Equal(expectedAddressType, @is.AddressType);
        Assert.Equal(expectedConstantType, @is.ConstantType);
        Assert.Equal(expectedConstantValue, @is.ConstantValue);
        Assert.Equal(expectedHighestBinaryOp, @is.HighestBinaryOp);
        Assert.Equal(expectedStageIndex, @is.StageIndex);
        Assert.Equal(expectedCode, actualOutput);
        Assert.All(expectedLits, (lit, litPtr) =>
        {
            Assert.Equal((sbyte)lit, storage.LitQ[litPtr]);
        });
    }

    private static (Analyzer Sut, BackEnd BackEnd, Storage Storage) Arrange(
        Collection<KeyValuePair<PCode, int>>? stage = null,
        char? ch = null,
        char? nCh = null,
        StreamWriter? output = null,
        StreamReader? input = null,
        bool cCode = true,
        SegmentType oldSeg = SegmentType.None,
        SymbolTable? symTab = null,
        Collection<sbyte>? litQ = null,
        Dictionary<string, string>? mac = null,
        string? pLine = null,
        BufferLineType? lineType = null,
        int? lPtr = null,
        string? ssName = null)
    {
        var storage = new Storage(
            stage: stage,
            ch: ch,
            nCh: nCh,
            output: output,
            files: input != null,
            input: input,
            cCode: cCode,
            oldSeg: oldSeg,
            symTab: symTab ?? new([], []),
            litQ: litQ ?? [],
            mac: mac ?? [],
            pLine: pLine,
            lineType: lineType ?? BufferLineType.Parsing,
            lPtr: lPtr,
            ssName: ssName);

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

        var frontEnd = new FrontEnd(storage);
        var backEnd = new BackEnd(symTabMgmt, utility, storage);
        backEnd.SetCodes();
        var sut = new Analyzer(symTabMgmt, utility, frontEnd, backEnd, storage);

        return (sut, backEnd, storage);
    }
}
