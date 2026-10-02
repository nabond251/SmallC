// <copyright file="GlobalParserTests.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc1.Tests;

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc3;
using SmallC.Cc4;
using System.Collections.ObjectModel;
using System.Text;
using static SmallC.Cc.Storage;

/// <summary>
/// Tests the global parser.
/// </summary>
public class GlobalParserTests
{
    /// <summary>
    /// Tests parser.
    /// </summary>
    /// <param name="inputText">Input stream text.</param>
    /// <param name="expected">Expected parsing line.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
#pragma warning disable SA1118 // Parameter should not span multiple lines
    [InlineData(
@"func1() {
  123;
  123 + 321;
  ""abc"";
  ""def"";
  }

func2() {
  'a';
  '\1\1';
  ""ghi"";
  }",
@"CODE SEGMENT PUBLIC
ASSUME CS:CODE, SS:DATA, DS:DATA
PUBLIC _FUNC1
_FUNC1:
PUSH BP
MOV BP,SP
MOV AX,123
MOV AX,444
MOV AX,OFFSET _1+0
MOV AX,OFFSET _1+4
POP BP
RET
CODE ENDS
DATA SEGMENT PUBLIC
_1 DB 97,98,99,0,100,101,102,0
DATA ENDS
CODE SEGMENT PUBLIC
ASSUME CS:CODE, SS:DATA, DS:DATA
PUBLIC _FUNC2
_FUNC2:
PUSH BP
MOV BP,SP
MOV AX,97
MOV AX,257
MOV AX,OFFSET _2+0
POP BP
RET
CODE ENDS
DATA SEGMENT PUBLIC
_2 DB 103,104,105,0
")]
    [InlineData(
@"int
 gi,
 gi2 = 123,
 gia[10] = {1, 2, 3},
 *gip;",
@"DATA SEGMENT PUBLIC
PUBLIC _GI
_GI DW 1 DUP(0)
PUBLIC _GI2
_GI2 DW 123
PUBLIC _GIA
_GIA DW 1,2,3
 DW 7 DUP(0)
PUBLIC _GIP
_GIP DW 0
")]
    [InlineData(
@"unsigned int
 ugi,
 ugi2 = 123,
 ugia[10] = {1, 2, 3},
 *ugip;",
@"DATA SEGMENT PUBLIC
PUBLIC _UGI
_UGI DW 1 DUP(0)
PUBLIC _UGI2
_UGI2 DW 123
PUBLIC _UGIA
_UGIA DW 1,2,3
 DW 7 DUP(0)
PUBLIC _UGIP
_UGIP DW 0
")]
    [InlineData(
@"char
 gc,
 gc2 = 'a',
 gca[10] = ""abc"",
 *gcp;",
@"DATA SEGMENT PUBLIC
PUBLIC _GC
_GC DB 1 DUP(0)
PUBLIC _GC2
_GC2 DB 97
PUBLIC _GCA
_GCA DB 97,98,99,0
 DB 6 DUP(0)
PUBLIC _GCP
_GCP DW 0
")]
    [InlineData(
@"char *gcp = ""def"";",
@"DATA SEGMENT PUBLIC
PUBLIC _GCP
_GCP DW $+2
 DB 100,101,102,0
")]
    [InlineData(
@"unsigned char
 guc,
 guc2 = 'a',
 guca[10] = ""abc"",
 *gucp;",
@"DATA SEGMENT PUBLIC
PUBLIC _GUC
_GUC DB 1 DUP(0)
PUBLIC _GUC2
_GUC2 DB 97
PUBLIC _GUCA
_GUCA DB 97,98,99,0
 DB 6 DUP(0)
PUBLIC _GUCP
_GUCP DW 0
")]
    [InlineData(
@"extern int
 ei,
 eia[10];",
@"DATA SEGMENT PUBLIC
EXTRN _EI:WORD
EXTRN _EIA:WORD
")]
    [InlineData(
@"extern unsigned int
 eui,
 euia[10];",
@"DATA SEGMENT PUBLIC
EXTRN _EUI:WORD
EXTRN _EUIA:WORD
")]
    [InlineData(
@"extern char
 ec,
 eca[10];",
@"DATA SEGMENT PUBLIC
EXTRN _EC:BYTE
EXTRN _ECA:BYTE
")]
    [InlineData(
@"extern unsigned char
 euc,
 euca[10];",
@"DATA SEGMENT PUBLIC
EXTRN _EUC:BYTE
EXTRN _EUCA:BYTE
")]
    [InlineData(
@"
void foo() {
  bar();
  }
void bar() {
  }",
@"CODE SEGMENT PUBLIC
ASSUME CS:CODE, SS:DATA, DS:DATA
PUBLIC _FOO
_FOO:
PUSH BP
MOV BP,SP
XOR CL,CL
CALL _BAR
POP BP
RET
PUBLIC _BAR
_BAR:
PUSH BP
MOV BP,SP
POP BP
RET
")]
    [InlineData(
@"
int gi1;
func(ai, aia, aip, ac, aca, acp) int  ai, aia[], *aip;
                                 char ac, aca[], *acp; {
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
    [InlineData(
@"#asm
_getc:  jmp     _fgetc
        public  _getc;
#endasm",
@"_getc:  jmp     _fgetc
        public  _getc;
")]
    [InlineData(
@"#include <stdio.h>
char gc = YES;",
@"DATA SEGMENT PUBLIC
PUBLIC _GC
_GC DB 1
")]
    [InlineData(
@"#include ""clib.h""
char gc = PAUSE;",
@"DATA SEGMENT PUBLIC
PUBLIC _GC
_GC DB 19
")]
#pragma warning restore SA1118 // Parameter should not span multiple lines
    public async Task CanParseAsync(
        string inputText, string? expected)
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
        var (sut, _, _) = Arrange(
            output: output, input: input, mac: mac);

        await sut.ParseAsync();
        await output.FlushAsync();
        outputStream.Position = 0;
        using var reader = new StreamReader(outputStream);
        var actual = await reader.ReadToEndAsync();

        Assert.Equal(expected, actual);
    }

    private static (GlobalParser Sut, BackEnd BackEnd, Storage Storage) Arrange(
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
        var whileQueueMgmt = new WhileQueueUseCases(utility, storage);

        var frontEnd = new FrontEnd(storage);
        var backEnd = new BackEnd(symTabMgmt, utility, storage);
        var analyzer = new Analyzer(symTabMgmt, utility, frontEnd, backEnd, storage);
        var localParser = new LocalParser(
            symTabMgmt, utility, whileQueueMgmt, frontEnd, analyzer, backEnd, storage);
        backEnd.SetCodes();

        var sut = new GlobalParser(
            symTabMgmt, utility, frontEnd, localParser, analyzer, backEnd, storage);

        return (sut, backEnd, storage);
    }
}
