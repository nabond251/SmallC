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
    /// Tests that can analyze constant.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expected">Expected constant value.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("sizeof(char)", 1)]
    [InlineData("0100000 > 0", 1)]
    [InlineData("sizeof(char*)", 2)]
    [InlineData("sizeof(unsigned char)", 1)]
    [InlineData("sizeof(unsigned char*)", 2)]
    [InlineData("sizeof uca3", 3)]
    [InlineData("sizeof(uca3)", 3)]
    [InlineData("1 - 1", 0)]
    [InlineData("!2", 0)]
    [InlineData("sizeof(int)", 2)]
    [InlineData("3 ? 5 : 8", 5)]
    [InlineData("13 || 2", 1)]
    [InlineData("21 && 34", 1)]
    [InlineData("55 | 2", 55)]
    [InlineData("89 ^ 2", 91)]
    [InlineData("144 & 2", 2)]
    [InlineData("233 == 2", 0)]
    [InlineData("377 != 2", 1)]
    [InlineData("610 <= 2", 0)]
    [InlineData("987 < 2", 0)]
    [InlineData("1597 > 2", 1)]
    [InlineData("2584 >= 2", 1)]
    [InlineData("4181 << 2", 8362)]
    [InlineData("6765 >> 2", 3832)]
    [InlineData("10946 + 2", 10948)]
    [InlineData("17711 - 2", 17709)]
    [InlineData("28657 * 2", -8222)]
    [InlineData("2 / -19168", 0)]
    [InlineData("32768 <= 9489", 0)]
    [InlineData("sizeof(int*)", 2)]
    [InlineData("~-9679", 9678)]
    [InlineData("0x8000 * -190", 0)]
    [InlineData("sizeof(unsigned)", 2)]
    [InlineData("sizeof(unsigned int)", 2)]
    [InlineData("sizeof(unsigned*)", 2)]
    [InlineData("sizeof(unsigned int*)", 2)]
    [InlineData("0177777 < -9869", 0)]
    [InlineData("65535 >= -10059", 1)]
    [InlineData("0xFFFF / -19928", 1)]
    [InlineData("0377777 > -29987", 1)]
    [InlineData("131071 <= 15621", 0)]
    [InlineData("0x1FFFF % -14366", 28731)]
    [InlineData("0", 0)]
    [InlineData("!0", 1)]
    [InlineData("00", 0)]
    [InlineData("-0", 0)]
    [InlineData("+0", 0)]
    [InlineData("-1", -1)]
    [InlineData("!-1", 0)]
    [InlineData("+1", 1)]
    [InlineData("!+1", 0)]
    [InlineData("01", 1)]
    [InlineData(" 1", 1)]
    [InlineData("10", 10)]
    [InlineData("!10", 0)]
    [InlineData("-32769", 32767)]
    [InlineData("-32768", -32768)]
    [InlineData("32767", 32767)]
    [InlineData("32768", -32768)]
    [InlineData("65535", -1)]
    [InlineData("65536", 0)]
    [InlineData("131071", -1)]
    [InlineData("000", 0)]
    [InlineData("-00", 0)]
    [InlineData("-01", -1)]
    [InlineData("001", 1)]
    [InlineData("010", 8)]
    [InlineData("018", 1)]
    [InlineData("077", 63)]
    [InlineData("0777", 511)]
    [InlineData("-0100001", 32767)]
    [InlineData("-0100000", -32768)]
    [InlineData("077777", 32767)]
    [InlineData("0100000", -32768)]
    [InlineData("0177777", -1)]
    [InlineData("0200000", 0)]
    [InlineData("0377777", -1)]
    [InlineData("00x0", 0)]
    [InlineData("-0x00", 0)]
    [InlineData("-0x01", -1)]
    [InlineData("0x10", 16)]
    [InlineData("~0x10", -17)]
    [InlineData("0x1G", 1)]
    [InlineData("0xFF", 255)]
    [InlineData("0xFG", 15)]
    [InlineData("-0x8001", 32767)]
    [InlineData("-0x8000", -32768)]
    [InlineData("0x7FFF", 32767)]
    [InlineData("0x8000", -32768)]
    [InlineData("0xFFFF", -1)]
    [InlineData("0x10000", 0)]
    [InlineData("0x1FFFF", -1)]
    [InlineData("''", 0)]
    [InlineData("'a'", 'a')]
    [InlineData(" 'a'", 'a')]
    [InlineData("'\\\\'", '\\')]
    [InlineData("'\\n'", '\n')]
    [InlineData("'\\t'", '\t')]
    [InlineData("'\\b'", '\b')]
    [InlineData("'\\f'", '\f')]
    [InlineData("'\\0'", '\0')]
    [InlineData("'\\1'", (char)1)]
    [InlineData("'\\9'", '9')]
    [InlineData("'\\12'", (char)10)]
    [InlineData("'\\123'", (char)83)]
    [InlineData("'\\1234'", ((char)83 << 8) + '4')]
    [InlineData("'12'", ('1' << 8) + '2')]
    [InlineData("'123'", ('2' << 8) + '3')]
    public async Task ParsesConstantAsync(
        string inputText,
        short expected)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, backEnd, storage) = Arrange(output: output, input: input);
        var (before, start) = backEnd.SetStage();

        var actual = await sut.ConstExprAsync();
        await backEnd.ClearStageAsync(before, start);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actualOutput = await reader.ReadToEndAsync();

        Assert.Equal(expected, actual);
        Assert.Empty(actualOutput);
        Assert.Empty(storage.LitQ);
    }

    /// <summary>
    /// Tests that can analyze expression.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expectedConstant">Expected constant type.</param>
    /// <param name="expectedConstantValue">Expected constant value.</param>
    /// <param name="expectedCode">Expected generated code.</param>
    /// <param name="expectedLits">String of expected lit pool bytes.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
#pragma warning disable SA1118 // Parameter should not span multiple lines
#pragma warning disable SA1117 // Parameters should be on same line or separate lines
    [InlineData("sizeof(char)", true, 1,
@"MOV AX,1
", "")]
    [InlineData("c", false, 0,
@"LEA AX,-10[BP]
MOV BX,AX
MOV AL,[BX]
CBW
", "")]
    [InlineData("c = 0", false, 0,
@"LEA AX,-10[BP]
MOV BX,AX
XOR AX,AX
MOV [BX],AL
", "")]
    [InlineData("c = *gcp", false, 0,
@"LEA AX,-10[BP]
PUSH AX
MOV AX,_GCP
MOV BX,AX
MOV AL,[BX]
CBW
POP BX
MOV [BX],AL
", "")]
    [InlineData("c++", false, 0,
@"LEA AX,-10[BP]
MOV BX,AX
MOV AL,[BX]
CBW
INC AX
MOV [BX],AL
DEC AX
", "")]
    [InlineData("0100000 > c", false, -32768,
@"LEA AX,-10[BP]
MOV BX,AX
MOV AL,[BX]
CBW
MOV BX,-32768
CALL __UGT
", "")]
    [InlineData("sizeof(char*)", true, 2,
@"MOV AX,2
", "")]
    [InlineData("ca3", false, 0,
@"LEA AX,-8[BP]
", "")]
    [InlineData("cp", false, 0,
@"LEA AX,-6[BP]
MOV BX,AX
MOV AX,[BX]
", "")]
    [InlineData("*cp", false, 1,
@"LEA AX,-6[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AL,[BX]
CBW
", "")]
    [InlineData("sizeof(unsigned char)", true, 1,
@"MOV AX,1
", "")]
    [InlineData("uc", false, 0,
@"LEA AX,-4[BP]
MOV BX,AX
MOV AL,[BX]
XOR AH,AH
", "")]
    [InlineData("sizeof(unsigned char*)", true, 2,
@"MOV AX,2
", "")]
    [InlineData("uca3", false, 0,
@"LEA AX,-2[BP]
", "")]
    [InlineData("sizeof uca3", true, 3,
@"MOV AX,3
", "")]
    [InlineData("sizeof(uca3)", true, 3,
@"MOV AX,3
", "")]
    [InlineData("ucp", false, 0,
@"LEA AX,0[BP]
MOV BX,AX
MOV AX,[BX]
", "")]
    [InlineData("ucp - &ucp", false, 0,
@"LEA AX,0[BP]
MOV BX,AX
MOV AX,[BX]
PUSH AX
LEA AX,0[BP]
POP BX
XCHG AX,BX
SUB AX,BX
", "")]
    [InlineData("!ucp", false, 1,
@"LEA AX,0[BP]
MOV BX,AX
MOV AX,[BX]
CALL __LNEG
", "")]
    [InlineData("sizeof(int)", true, 2,
@"MOV AX,2
", "")]
    [InlineData("i", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
", "")]
    [InlineData("i |= 2", false, 0,
@"LEA AX,2[BP]
PUSH AX
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
OR AX,BX
POP BX
MOV [BX],AX
", "")]
    [InlineData("i ^= 2", false, 0,
@"LEA AX,2[BP]
PUSH AX
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
XOR AX,BX
POP BX
MOV [BX],AX
", "")]
    [InlineData("i &= 2", false, 0,
@"LEA AX,2[BP]
PUSH AX
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
AND AX,BX
POP BX
MOV [BX],AX
", "")]
    [InlineData("i += 2", false, 0,
@"LEA AX,2[BP]
PUSH AX
MOV BX,AX
MOV AX,[BX]
MOV BX,2
ADD AX,BX
POP BX
MOV [BX],AX
", "")]
    [InlineData("i -= 2", false, 0,
@"LEA AX,2[BP]
PUSH AX
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
XCHG AX,BX
SUB AX,BX
POP BX
MOV [BX],AX
", "")]
    [InlineData("i *= 2", false, 0,
@"LEA AX,2[BP]
PUSH AX
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
IMUL BX
POP BX
MOV [BX],AX
", "")]
    [InlineData("i /= 2", false, 0,
@"LEA AX,2[BP]
PUSH AX
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
XCHG AX,BX
CWD
IDIV BX
POP BX
MOV [BX],AX
", "")]
    [InlineData("i %= 2", false, 0,
@"LEA AX,2[BP]
PUSH AX
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
XCHG AX,BX
CWD
IDIV BX
MOV AX,DX
POP BX
MOV [BX],AX
", "")]
    [InlineData("i ? 1 : 2", false, 0,
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
    [InlineData("i ? c : 2", false, 0,
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
    [InlineData("i ? c : gc", false, 0,
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
    [InlineData("i || 2", false, 0,
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
    [InlineData("i && 2", false, 0,
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
    [InlineData("i | 2", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
OR AX,BX
", "")]
    [InlineData("i ^ 2", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
XOR AX,BX
", "")]
    [InlineData("i & 2", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
AND AX,BX
", "")]
    [InlineData("i == 2", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
CALL __EQ
", "")]
    [InlineData("i != 2", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
CALL __NE
", "")]
    [InlineData("i <= 2", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
CALL __LE
", "")]
    [InlineData("i < 2", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
CALL __LT
", "")]
    [InlineData("i > 2", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
CALL __GT
", "")]
    [InlineData("i >= 2", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
CALL __GE
", "")]
    [InlineData("i << 2", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
MOV CX,AX
MOV AX,BX
SAL AX,CL
", "")]
    [InlineData("i >> 2", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
MOV CX,AX
MOV AX,BX
SAR AX,CL
", "")]
    [InlineData("i + 2", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,2
ADD AX,BX
", "")]
    [InlineData("2 + i", false, 2,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,2
ADD AX,BX
", "")]
    [InlineData("i - 2", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
XCHG AX,BX
SUB AX,BX
", "")]
    [InlineData("2 - i", false, 2,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,2
XCHG AX,BX
SUB AX,BX
", "")]
    [InlineData("i * 2", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,2
IMUL BX
", "")]
    [InlineData("2 / i", false, 2,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,2
XCHG AX,BX
CWD
IDIV BX
", "")]
    [InlineData("i % i", false, 0,
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
    [InlineData("i--", false, 0,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
DEC AX
MOV [BX],AX
INC AX
", "")]
    [InlineData("32768 <= i", false, -32768,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,-32768
CALL __ULE
", "")]
    [InlineData("sizeof(int*)", true, 2,
@"MOV AX,2
", "")]
    [InlineData("ia3", false, 0,
@"LEA AX,4[BP]
", "")]
    [InlineData("ip", false, 0,
@"LEA AX,6[BP]
MOV BX,AX
MOV AX,[BX]
", "")]
    [InlineData("ip - &ip", false, 0,
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
    [InlineData("~ip", false, -1,
@"LEA AX,6[BP]
MOV BX,AX
MOV AX,[BX]
NOT AX
", "")]
    [InlineData("0x8000 * *ip", false, -32768,
@"LEA AX,6[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,[BX]
MOV BX,-32768
MUL BX
", "")]
    [InlineData("sizeof(unsigned)", true, 2,
@"MOV AX,2
", "")]
    [InlineData("sizeof(unsigned int)", true, 2,
@"MOV AX,2
", "")]
    [InlineData("ui", false, 0,
@"LEA AX,8[BP]
MOV BX,AX
MOV AX,[BX]
", "")]
    [InlineData("sizeof(unsigned*)", true, 2,
@"MOV AX,2
", "")]
    [InlineData("sizeof(unsigned int*)", true, 2,
@"MOV AX,2
", "")]
    [InlineData("&ui", false, 0,
@"LEA AX,8[BP]
", "")]
    [InlineData("uia3", false, 0,
@"LEA AX,10[BP]
", "")]
    [InlineData("uip", false, 0,
@"LEA AX,12[BP]
MOV BX,AX
MOV AX,[BX]
", "")]
    [InlineData("--uip", false, 0,
@"LEA AX,12[BP]
MOV BX,AX
MOV AX,[BX]
DEC AX
DEC AX
MOV [BX],AX
", "")]
    [InlineData("gc", false, 0,
@"MOV AL,_GC
CBW
", "")]
    [InlineData("gc = 0", false, 0,
@"XOR AX,AX
MOV _GC,AL
", "")]
    [InlineData("gc = *cp", false, 0,
@"LEA AX,-6[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AL,[BX]
CBW
MOV _GC,AL
", "")]
    [InlineData("&gc", false, 0,
@"MOV AX,OFFSET _GC
", "")]
    [InlineData("0177777 < gc", false, -1,
@"MOV AL,_GC
CBW
MOV BX,-1
CALL __ULT
", "")]
    [InlineData("gca3", false, 0,
@"MOV AX,OFFSET _GCA3
", "")]
    [InlineData("gcp", false, 0,
@"MOV AX,_GCP
", "")]
    [InlineData("1 + gcp", false, 1,
@"MOV AX,_GCP
MOV BX,1
ADD AX,BX
", "")]
    [InlineData("guc", false, 0,
@"MOV AL,_GUC
XOR AH,AH
", "")]
    [InlineData("~guc", false, -1,
@"MOV AL,_GUC
XOR AH,AH
NOT AX
", "")]
    [InlineData("guca3", false, 0,
@"MOV AX,OFFSET _GUCA3
", "")]
    [InlineData("gucp", false, 0,
@"MOV AX,_GUCP
", "")]
    [InlineData("gi", false, 0,
@"MOV AX,_GI
", "")]
    [InlineData("gi >>= 1", false, 0,
@"MOV AX,_GI
MOV BX,AX
MOV AX,1
MOV CX,AX
MOV AX,BX
SAR AX,CL
MOV _GI,AX
", "")]
    [InlineData("gi <<= 1", false, 0,
@"MOV AX,_GI
MOV BX,AX
MOV AX,1
MOV CX,AX
MOV AX,BX
SAL AX,CL
MOV _GI,AX
", "")]
    [InlineData("65535 >= gi", false, -1,
@"MOV AX,_GI
MOV BX,-1
CALL __UGE
", "")]
    [InlineData("gia3", false, 0,
@"MOV AX,OFFSET _GIA3
", "")]
    [InlineData("gip", false, 0,
@"MOV AX,_GIP
", "")]
    [InlineData("*gip", false, 1,
@"MOV AX,_GIP
MOV BX,AX
MOV AX,[BX]
", "")]
    [InlineData("-*gip", false, -1,
@"MOV AX,_GIP
MOV BX,AX
MOV AX,[BX]
NEG AX
", "")]
    [InlineData("0xFFFF / *gip", false, -1,
@"MOV AX,_GIP
MOV BX,AX
MOV AX,[BX]
MOV BX,-1
XCHG AX,BX
XOR DX,DX
DIV BX
", "")]
    [InlineData("gui", false, 0,
@"MOV AX,_GUI
", "")]
    [InlineData("!gui", false, 1,
@"MOV AX,_GUI
CALL __LNEG
", "")]
    [InlineData("guia3", false, 0,
@"MOV AX,OFFSET _GUIA3
", "")]
    [InlineData("guip", false, 0,
@"MOV AX,_GUIP
", "")]
    [InlineData("1 - guip", false, 1,
@"MOV AX,_GUIP
MOV BX,2
XCHG AX,BX
SUB AX,BX
", "")]
    [InlineData("ec", false, 0,
@"MOV AL,_EC
CBW
", "")]
    [InlineData("0377777 > ec", false, -1,
@"MOV AL,_EC
CBW
MOV BX,-1
CALL __UGT
", "")]
    [InlineData("eca3", false, 0,
@"MOV AX,OFFSET _ECA3
", "")]
    [InlineData("ecp", false, 0,
@"MOV AX,_ECP
", "")]
    [InlineData("euc", false, 0,
@"MOV AL,_EUC
XOR AH,AH
", "")]
    [InlineData("euca3", false, 0,
@"MOV AX,OFFSET _EUCA3
", "")]
    [InlineData("eucp", false, 0,
@"MOV AX,_EUCP
", "")]
    [InlineData("ei", false, 0,
@"MOV AX,_EI
", "")]
    [InlineData("++ei", false, 0,
@"MOV AX,_EI
INC AX
MOV _EI,AX
", "")]
    [InlineData("131071 <= ei", false, -1,
@"MOV AX,_EI
MOV BX,-1
CALL __ULE
", "")]
    [InlineData("eia3", false, 0,
@"MOV AX,OFFSET _EIA3
", "")]
    [InlineData("eip", false, 0,
@"MOV AX,_EIP
", "")]
    [InlineData("0x1FFFF % *eip", false, -1,
@"MOV AX,_EIP
MOV BX,AX
MOV AX,[BX]
MOV BX,-1
XCHG AX,BX
XOR DX,DX
DIV BX
MOV AX,DX
", "")]
    [InlineData("eui", false, 0,
@"MOV AX,_EUI
", "")]
    [InlineData("euia3", false, 0,
@"MOV AX,OFFSET _EUIA3
", "")]
    [InlineData("euip", false, 0,
@"MOV AX,_EUIP
", "")]
    [InlineData("foo", false, 0,
@"MOV AX,OFFSET _FOO
", "")]
    [InlineData("foo()", false, 0,
@"XOR CL,CL
CALL _FOO
", "")]
    [InlineData("bar", false, 0,
@"MOV AX,OFFSET _BAR
", "")]
    [InlineData("bar()", false, 0,
@"XOR CL,CL
CALL _BAR
", "")]
    [InlineData("0", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("!0", true, 1,
@"MOV AX,1
", "")]
    [InlineData("00", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("-0", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("+0", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("-1", true, -1,
@"MOV AX,-1
", "")]
    [InlineData("!-1", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("+1", true, 1,
@"MOV AX,1
", "")]
    [InlineData("!+1", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("01", true, 1,
@"MOV AX,1
", "")]
    [InlineData(" 1", true, 1,
@"MOV AX,1
", "")]
    [InlineData("10", true, 10,
@"MOV AX,10
", "")]
    [InlineData("!10", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("-32769", true, 32767,
@"MOV AX,32767
", "")]
    [InlineData("-32768", true, -32768,
@"MOV AX,-32768
", "")]
    [InlineData("32767", true, 32767,
@"MOV AX,32767
", "")]
    [InlineData("32768", true, -32768,
@"MOV AX,-32768
", "")]
    [InlineData("65535", true, -1,
@"MOV AX,-1
", "")]
    [InlineData("65536", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("131071", true, -1,
@"MOV AX,-1
", "")]
    [InlineData("000", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("-00", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("-01", true, -1,
@"MOV AX,-1
", "")]
    [InlineData("001", true, 1,
@"MOV AX,1
", "")]
    [InlineData("010", true, 8,
@"MOV AX,8
", "")]
    [InlineData("018", true, 1,
@"MOV AX,1
", "")]
    [InlineData("077", true, 63,
@"MOV AX,63
", "")]
    [InlineData("0777", true, 511,
@"MOV AX,511
", "")]
    [InlineData("-0100001", true, 32767,
@"MOV AX,32767
", "")]
    [InlineData("-0100000", true, -32768,
@"MOV AX,-32768
", "")]
    [InlineData("077777", true, 32767,
@"MOV AX,32767
", "")]
    [InlineData("0100000", true, -32768,
@"MOV AX,-32768
", "")]
    [InlineData("0177777", true, -1,
@"MOV AX,-1
", "")]
    [InlineData("0200000", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("0377777", true, -1,
@"MOV AX,-1
", "")]
    [InlineData("00x0", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("-0x00", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("-0x01", true, -1,
@"MOV AX,-1
", "")]
    [InlineData("0x10", true, 16,
@"MOV AX,16
", "")]
    [InlineData("~0x10", true, -17,
@"MOV AX,-17
", "")]
    [InlineData("0x1G", true, 1,
@"MOV AX,1
", "")]
    [InlineData("0xFF", true, 255,
@"MOV AX,255
", "")]
    [InlineData("0xFG", true, 15,
@"MOV AX,15
", "")]
    [InlineData("-0x8001", true, 32767,
@"MOV AX,32767
", "")]
    [InlineData("-0x8000", true, -32768,
@"MOV AX,-32768
", "")]
    [InlineData("0x7FFF", true, 32767,
@"MOV AX,32767
", "")]
    [InlineData("0x8000", true, -32768,
@"MOV AX,-32768
", "")]
    [InlineData("0xFFFF", true, -1,
@"MOV AX,-1
", "")]
    [InlineData("0x10000", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("0x1FFFF", true, -1,
@"MOV AX,-1
", "")]
    [InlineData("''", true, 0,
@"XOR AX,AX
", "")]
    [InlineData("'a'", true, 'a',
@"MOV AX,97
", "")]
    [InlineData(" 'a'", true, 'a',
@"MOV AX,97
", "")]
    [InlineData("'\\\\'", true, '\\',
@"MOV AX,92
", "")]
    [InlineData("'\\n'", true, '\n',
@"MOV AX,10
", "")]
    [InlineData("'\\t'", true, '\t',
@"MOV AX,9
", "")]
    [InlineData("'\\b'", true, '\b',
@"MOV AX,8
", "")]
    [InlineData("'\\f'", true, '\f',
@"MOV AX,12
", "")]
    [InlineData("'\\0'", true, '\0',
@"XOR AX,AX
", "")]
    [InlineData("'\\1'", true, (char)1,
@"MOV AX,1
", "")]
    [InlineData("'\\9'", true, '9',
@"MOV AX,57
", "")]
    [InlineData("'\\12'", true, (char)10,
@"MOV AX,10
", "")]
    [InlineData("'\\123'", true, (char)83,
@"MOV AX,83
", "")]
    [InlineData("'\\1234'", true, ((char)83 << 8) + '4',
@"MOV AX,21300
", "")]
    [InlineData("'12'", true, ('1' << 8) + '2',
@"MOV AX,12594
", "")]
    [InlineData("'123'", true, ('2' << 8) + '3',
@"MOV AX,12851
", "")]
    [InlineData("\"\"", false, 0,
@"MOV AX,OFFSET _0+0
", "")]
    [InlineData("\"a\"", false, 0,
@"MOV AX,OFFSET _0+0
", "a")]
    [InlineData(" \"a\"", false, 0,
@"MOV AX,OFFSET _0+0
", "a")]
    [InlineData("\"abc\"", false, 0,
@"MOV AX,OFFSET _0+0
", "abc")]
#pragma warning restore SA1117 // Parameters should be on same line or separate lines
#pragma warning restore SA1118 // Parameter should not span multiple lines
    public async Task ParsesExpressionAsync(
        string inputText,
        bool expectedConstant,
        short expectedConstantValue,
        string expectedCode,
        string expectedLits)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, backEnd, storage) = Arrange(output: output, input: input);
        var (before, start) = backEnd.SetStage();

        var (actualConstant, actualConstantValue) =
            await sut.ExpressionAsync();
        await backEnd.ClearStageAsync(before, start);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actualOutput = await reader.ReadToEndAsync();

        Assert.Equal(expectedConstant, actualConstant);
        Assert.Equal(expectedConstantValue, actualConstantValue);
        Assert.Equal(expectedCode, actualOutput);
        Assert.All(expectedLits, (lit, litPtr) =>
        {
            Assert.Equal((sbyte)lit, storage.LitQ[litPtr]);
        });
    }

    /// <summary>
    /// Tests that can analyze test expression.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="parens">Whether parens are needed.</param>
    /// <param name="expected">Expected generated code.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
#pragma warning disable SA1118 // Parameter should not span multiple lines
#pragma warning disable SA1117 // Parameters should be on same line or separate lines
    [InlineData("i, 0", false,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
JMP _0
")]
    [InlineData("(1)", true,
@"")]
    [InlineData("1", false,
@"")]
    [InlineData("0", false,
@"JMP _0
")]
    [InlineData("i == 0", false,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
OR AX,AX
JE $+5
JMP _0
")]
    [InlineData("ui <= 0", false,
@"LEA AX,8[BP]
MOV BX,AX
MOV AX,[BX]
OR AX,AX
JE $+5
JMP _0
")]
    [InlineData("i != 0", false,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
OR AX,AX
JNE $+5
JMP _0
")]
    [InlineData("ui > 0", false,
@"LEA AX,8[BP]
MOV BX,AX
MOV AX,[BX]
OR AX,AX
JNE $+5
JMP _0
")]
    [InlineData("i > 0", false,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
OR AX,AX
JG $+5
JMP _0
")]
    [InlineData("i >= 0", false,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
OR AX,AX
JGE $+5
JMP _0
")]
    [InlineData("ui >= 0", false,
@"LEA AX,8[BP]
MOV BX,AX
MOV AX,[BX]
")]
    [InlineData("i < 0", false,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
OR AX,AX
JL $+5
JMP _0
")]
    [InlineData("ui < 0", false,
@"LEA AX,8[BP]
MOV BX,AX
MOV AX,[BX]
JMP _0
")]
    [InlineData("i <= 0", false,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
OR AX,AX
JLE $+5
JMP _0
")]
    [InlineData("i <= 1", false,
@"LEA AX,2[BP]
MOV BX,AX
MOV AX,[BX]
MOV BX,AX
MOV AX,1
CALL __LE
OR AX,AX
JNE $+5
JMP _0
")]
#pragma warning restore SA1117 // Parameters should be on same line or separate lines
#pragma warning restore SA1118 // Parameter should not span multiple lines
    public async Task ParsesTestAsync(
        string inputText,
        bool parens,
        string expected)
    {
        using var outputStream = new MemoryStream();
        using var output = new StreamWriter(outputStream);
        var byteArray = Encoding.ASCII.GetBytes(inputText);
        var inputStream = new MemoryStream(byteArray);
        using var input = new StreamReader(inputStream);
        var (sut, _, _) = Arrange(output: output, input: input);

        await sut.TestAsync(0, parens);
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        Assert.Equal(expected, actual);
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
