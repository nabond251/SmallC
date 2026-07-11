// <copyright file="BackEnd.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc4;

using SmallC.Cc;
using SmallC.Cc2;
using System.ComponentModel;
using System.Globalization;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// Back end.
/// </summary>
public class BackEnd(
    SymbolTableUseCases symTabMgmt,
    UtilityUseCases utility,
    Storage storage)
{
    // Optimizer command definitions

    /*                                --     p-codes must not overlap these */

    /*                                --     these digits are reserved for n */
    private const int Go = /*..*/ 0x0100; // go n entries
    private const int IfE = /*.*/ 0x0600; // if value == n do commands to next 0
    private const int IfL = /*.*/ 0x0700; // if value <  n do commands to next 0
    private const int Neg = /*.*/ 0x0500; // negate the value

    private const int P1 = /*..*/ 0x0001; // plus 1
    private const int P2 = /*..*/ 0x0001; // plus 2
    private const int P3 = /*..*/ 0x0003; // plus 3
    private const int M2 = /*..*/ 0x00FE; // minus 2

    private const int HighSeq = 49;

    /// <summary>
    /// ADD21.
    /// </summary>
    private static readonly int[] Seq00 = [
        0, (int)PCode.ADD12, (int)PCode.MOVE21, 0,
        Go | P1, (int)PCode.ADD21, 0];

    /// <summary>
    /// rINC1 or rDEC1 ? .
    /// </summary>
    private static readonly int[] Seq01 = [
        0, (int)PCode.ADD1n, 0,
        IfL | M2, 0, IfL | 0, (int)PCode.rDEC1, Neg, 0, IfL | P3, (int)PCode.rINC1, 0, 0];

    /// <summary>
    /// rINC2 or rDEC2 ? .
    /// </summary>
    private static readonly int[] Seq02 = [
        0, (int)PCode.ADD2n, 0,
        IfL | M2, 0, IfL | 0, (int)PCode.rDEC2, Neg, 0, IfL | P3, (int)PCode.rINC2, 0, 0];

    /// <summary>
    /// SUBbpn or DECbp.
    /// </summary>
    private static readonly int[] Seq03 = [
        0, (int)PCode.rDEC1, (int)PCode.PUTbp1, (int)PCode.rINC1, 0,
        Go | P2, IfE | P1, (int)PCode.DECbp, 0, (int)PCode.SUBbpn, 0];

    /// <summary>
    /// SUBwpn or DECwp.
    /// </summary>
    private static readonly int[] Seq04 = [
        0, (int)PCode.rDEC1, (int)PCode.PUTwp1, (int)PCode.rINC1, 0,
        Go | P2, IfE | P1, (int)PCode.DECwp, 0, (int)PCode.SUBwpn, 0];

    private readonly int[][] seq = new int[HighSeq + 1][];

    // Assembly-code strings
    private readonly Dictionary<PCode, Code> code = [];

    /// <summary>
    /// Set optimizer command lists.
    /// </summary>
    public void SetSeq()
    {
        this.seq[0] = Seq00;
        this.seq[1] = Seq01;
        this.seq[2] = Seq02;
        this.seq[3] = Seq03;
        this.seq[4] = Seq04;
    }

    /// <summary>
    /// Set assembly-code strings.
    /// </summary>
    public void SetCodes()
    {
        this.SetSeq();
        this.code[PCode.ADD12] = Code.Build("211", "ADD AX,BX\r\n");
        this.code[PCode.ADD1n] = Code.Build("010", "?ADD AX,<n>\r\n??");
        this.code[PCode.ADD21] = Code.Build("211", "ADD BX,AX\r\n");
        this.code[PCode.ADD2n] = Code.Build("010", "?ADD BX,<n>\r\n??");
        this.code[PCode.ADDbpn] = Code.Build("001", "ADD BYTE PTR [BX],<n>\r\n");
        this.code[PCode.ADDwpn] = Code.Build("001", "ADD WORD PTR [BX],<n>\r\n");
        this.code[PCode.ADDm_] = Code.Build("000", "ADD <m>");
        this.code[PCode.ADDSP] = Code.Build("000", "?ADD SP,<n>\r\n??");
        this.code[PCode.AND12] = Code.Build("211", "AND AX,BX\r\n");
        this.code[PCode.ANEG1] = Code.Build("010", "NEG AX\r\n");
        this.code[PCode.ARGCNTn] = Code.Build("000", "?MOV CL,<n>?XOR CL,CL?\r\n");
        this.code[PCode.ASL12] = Code.Build("011", "MOV CX,AX\r\nMOV AX,BX\r\nSAL AX,CL\r\n");
        this.code[PCode.ASR12] = Code.Build("011", "MOV CX,AX\r\nMOV AX,BX\r\nSAR AX,CL\r\n");
        this.code[PCode.CALL1] = Code.Build("010", "CALL AX\r\n");
        this.code[PCode.CALLm] = Code.Build("020", "CALL <m>\r\n");
        this.code[PCode.BYTE_] = Code.Build("000", " DB ");
        this.code[PCode.BYTEn] = Code.Build("000", " DB <n>\r\n");
        this.code[PCode.BYTEr0] = Code.Build("000", " DB <n> DUP(0)\r\n");
        this.code[PCode.COM1] = Code.Build("010", "NOT AX\r\n");
        this.code[PCode.COMMAn] = Code.Build("000", ",<n>\r\n");
        this.code[PCode.DBL1] = Code.Build("010", "SHL AX,1\r\n");
        this.code[PCode.DBL2] = Code.Build("001", "SHL BX,1\r\n");
        this.code[PCode.DECbp] = Code.Build("001", "DEC BYTE PTR [BX]\r\n");
        this.code[PCode.DECwp] = Code.Build("001", "DEC WORD PTR [BX]\r\n");
        this.code[PCode.DIV12] = Code.Build("011", "CWD\r\nIDIV BX\r\n"); // see GenAsync()
        this.code[PCode.DIV12u] = Code.Build("011", "XOR DX,DX\r\nDIV BX\r\n"); // see GenAsync()
        this.code[PCode.ENTER] = Code.Build("100", "PUSH BP\r\nMOV BP,SP\r\n");
        this.code[PCode.EQ10f] = Code.Build("010", "OR AX,AX\r\nJE $+5\r\nJMP _<n>\r\n");
        this.code[PCode.EQ12] = Code.Build("211", "CALL __EQ\r\n");
        this.code[PCode.GE10f] = Code.Build("010", "OR AX,AX\r\nJGE $+5\r\nJMP _<n>\r\n");
        this.code[PCode.GE12] = Code.Build("011", "CALL __GE\r\n");
        this.code[PCode.GE12u] = Code.Build("011", "CALL __UGE\r\n");
        this.code[PCode.GETb1m] = Code.Build("020", "MOV AL,<m>\r\nCBW\r\n");
        this.code[PCode.GETb1mu] = Code.Build("020", "MOV AL,<m>\r\nXOR AH,AH\r\n");
        this.code[PCode.GETb1p] = Code.Build("021", "MOV AL,?<n>??[BX]\r\nCBW\r\n"); // see GenAsync()
        this.code[PCode.GETb1pu] = Code.Build("021", "MOV AL,?<n>??[BX]\r\nXOR AH,AH\r\n"); // see GenAsync()
        this.code[PCode.GETb1s] = Code.Build("020", "MOV AL,<n>[BP]\r\nCBW\r\n");
        this.code[PCode.GETb1su] = Code.Build("020", "MOV AL,<n>[BP]\r\nXOR AH,AH\r\n");
        this.code[PCode.GETw1m] = Code.Build("020", "MOV AX,<m>\r\n");
        this.code[PCode.GETw1m_] = Code.Build("020", "MOV AX,<m>");
        this.code[PCode.GETw1n] = Code.Build("020", "?MOV AX,<n>?XOR AX,AX?\r\n");
        this.code[PCode.GETw1p] = Code.Build("021", "MOV AX,?<n>??[BX]\r\n"); // see GenAsync()
        this.code[PCode.GETw1s] = Code.Build("020", "MOV AX,<n>[BP]\r\n");
        this.code[PCode.GETw2m] = Code.Build("020", "MOV BX,<m>\r\n");
        this.code[PCode.GETw2n] = Code.Build("002", "?MOV BX,<n>?XOR BX,BX?\r\n");
        this.code[PCode.GETw2p] = Code.Build("021", "MOV BX,?<n>??[BX]\r\n");
        this.code[PCode.GETw2s] = Code.Build("002", "MOV BX,<n>[BP]\r\n");
        this.code[PCode.GT10f] = Code.Build("010", "OR AX,AX\r\nJG $+5\r\nJMP _<n>\r\n");
        this.code[PCode.GT12] = Code.Build("010", "CALL __GT\r\n");
        this.code[PCode.GT12u] = Code.Build("011", "CALL __UGT\r\n");
        this.code[PCode.INCbp] = Code.Build("001", "INC BYTE PTR [BX]\r\n");
        this.code[PCode.INCwp] = Code.Build("001", "INC WORD PTR [BX]\r\n");
        this.code[PCode.WORD_] = Code.Build("000", " DW ");
        this.code[PCode.WORDn] = Code.Build("000", " DW <n>\r\n");
        this.code[PCode.WORDr0] = Code.Build("000", " DW <n> DUP(0)\r\n");
        this.code[PCode.JMPm] = Code.Build("000", "JMP _<n>\r\n");
        this.code[PCode.LABm] = Code.Build("000", "_<n>:\r\n");
        this.code[PCode.LE10f] = Code.Build("010", "OR AX,AX\r\nJLE $+5\r\nJMP _<n>\r\n");
        this.code[PCode.LE12] = Code.Build("011", "CALL __LE\r\n");
        this.code[PCode.LE12u] = Code.Build("011", "CALL __ULE\r\n");
        this.code[PCode.LNEG1] = Code.Build("010", "CALL __LNEG\r\n");
        this.code[PCode.LT10f] = Code.Build("010", "OR AX,AX\r\nJL $+5\r\nJMP _<n>\r\n");
        this.code[PCode.LT12] = Code.Build("011", "CALL __LT\r\n");
        this.code[PCode.LT12u] = Code.Build("011", "CALL __ULT\r\n");
        this.code[PCode.MOD12] = Code.Build("011", "CWD\r\nIDIV BX\r\nMOV AX,DX\r\n"); // see GenAsync()
        this.code[PCode.MOD12u] = Code.Build("011", "XOR DX,DX\r\nDIV BX\r\nMOV AX,DX\r\n"); // see GenAsync()
        this.code[PCode.MOVE21] = Code.Build("012", "MOV BX,AX\r\n");
        this.code[PCode.MUL12] = Code.Build("211", "IMUL BX\r\n");
        this.code[PCode.MUL12u] = Code.Build("211", "MUL BX\r\n");
        this.code[PCode.NE10f] = Code.Build("010", "OR AX,AX\r\nJNE $+5\r\nJMP _<n>\r\n");
        this.code[PCode.NE12] = Code.Build("211", "CALL __NE\r\n");
        this.code[PCode.NEARm] = Code.Build("000", " DW _<n>\r\n");
        this.code[PCode.OR12] = Code.Build("211", "OR AX,BX\r\n");
        this.code[PCode.PLUSn] = Code.Build("000", "?+<n>??\r\n");
        this.code[PCode.POINT1l] = Code.Build("020", "MOV AX,OFFSET _<l>+<n>\r\n");
        this.code[PCode.POINT1m] = Code.Build("020", "MOV AX,OFFSET <m>\r\n");
        this.code[PCode.POINT1s] = Code.Build("020", "LEA AX,<n>[BP]\r\n");
        this.code[PCode.POINT2m] = Code.Build("002", "MOV BX,OFFSET <m>\r\n");
        this.code[PCode.POINT2m_] = Code.Build("002", "MOV BX,OFFSET <m>");
        this.code[PCode.POINT2s] = Code.Build("002", "LEA BX,<n>[BP]\r\n");
        this.code[PCode.POP2] = Code.Build("002", "POP BX\r\n");
        this.code[PCode.PUSH1] = Code.Build("110", "PUSH AX\r\n");
        this.code[PCode.PUSH2] = Code.Build("101", "PUSH BX\r\n");
        this.code[PCode.PUSHm] = Code.Build("100", "PUSH <m>\r\n");
        this.code[PCode.PUSHp] = Code.Build("100", "PUSH ?<n>??[BX]\r\n");
        this.code[PCode.PUSHs] = Code.Build("100", "PUSH ?<n>??[BP]\r\n");
        this.code[PCode.PUT_m_] = Code.Build("000", "MOV <m>");
        this.code[PCode.PUTbm1] = Code.Build("010", "MOV <m>,AL\r\n");
        this.code[PCode.PUTbp1] = Code.Build("011", "MOV [BX],AL\r\n");
        this.code[PCode.PUTwm1] = Code.Build("010", "MOV <m>,AX\r\n");
        this.code[PCode.PUTwp1] = Code.Build("011", "MOV [BX],AX\r\n");
        this.code[PCode.rDEC1] = Code.Build("010", "#DEC AX\r\n#");
        this.code[PCode.rDEC2] = Code.Build("010", "#DEC BX\r\n#");
        this.code[PCode.REFm] = Code.Build("000", "_<n>");
        this.code[PCode.RETURN] = Code.Build("000", "?MOV SP,BP\r\n??POP BP\r\nRET\r\n");
        this.code[PCode.rINC1] = Code.Build("010", "#INC AX\r\n#");
        this.code[PCode.rINC2] = Code.Build("010", "#INC BX\r\n#");
        this.code[PCode.SUB_m_] = Code.Build("000", "SUB <m>");
        this.code[PCode.SUB12] = Code.Build("011", "SUB AX,BX\r\n"); // see GenAsync()
        this.code[PCode.SUB1n] = Code.Build("010", "?SUB AX,<n>\r\n??");
        this.code[PCode.SUBbpn] = Code.Build("001", "SUB BYTE PTR [BX],<n>\r\n");
        this.code[PCode.SUBwpn] = Code.Build("001", "SUB WORD PTR [BX],<n>\r\n");
        this.code[PCode.SWAP12] = Code.Build("011", "XCHG AX,BX\r\n");
        this.code[PCode.SWAP1s] = Code.Build("012", "POP BX\r\nXCHG AX,BX\r\nPUSH BX\r\n");
        this.code[PCode.SWITCH] = Code.Build("012", "CALL __SWITCH\r\n");
        this.code[PCode.XOR12] = Code.Build("211", "XOR AX,BX\r\n");
    }

    /// <summary>
    /// Print all assembler info before any code is generated
    /// and ensure that the segments appear in the correct order.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HeaderAsync()
    {
        await this.ToSegAsync(SegmentType.CodeSeg).ConfigureAwait(false);
        await this.OutLineAsync("extrn __eq: near").ConfigureAwait(false);
        await this.OutLineAsync("extrn __ne: near").ConfigureAwait(false);
        await this.OutLineAsync("extrn __le: near").ConfigureAwait(false);
        await this.OutLineAsync("extrn __lt: near").ConfigureAwait(false);
        await this.OutLineAsync("extrn __ge: near").ConfigureAwait(false);
        await this.OutLineAsync("extrn __gt: near").ConfigureAwait(false);
        await this.OutLineAsync("extrn __ule: near").ConfigureAwait(false);
        await this.OutLineAsync("extrn __ult: near").ConfigureAwait(false);
        await this.OutLineAsync("extrn __uge: near").ConfigureAwait(false);
        await this.OutLineAsync("extrn __ugt: near").ConfigureAwait(false);
        await this.OutLineAsync("extrn __lneg: near").ConfigureAwait(false);
        await this.OutLineAsync("extrn __switch: near").ConfigureAwait(false);

        // Force non-zero code pointers, word alignment
        await this.OutLineAsync("dw 0").ConfigureAwait(false);
        await this.ToSegAsync(SegmentType.DataSeg).ConfigureAwait(false);

        // Force non-zero data pointers, word alignment
        await this.OutLineAsync("dw 0").ConfigureAwait(false);
    }

    /// <summary>
    /// Print any assembler stuff needed at the end.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task TrailerAsync()
    {
        var globals = storage.SymTable.Globals;
        foreach (var cptr in globals)
        {
            if (cptr.Ident == SymbolIdentity.Function
                && cptr.Class == SymbolClass.AutoExt)
            {
                await this.ExternalAsync(
                    cptr.Name, 0, SymbolIdentity.Function)
                    .ConfigureAwait(false);
            }
        }

        var cp = symTabMgmt.FindGlb("main");
        if (cp?.Class == SymbolClass.Static)
        {
            await this.ExternalAsync(
                "_main", 0, SymbolIdentity.Function)
                .ConfigureAwait(false);
        }

        await this.ToSegAsync(SegmentType.None).ConfigureAwait(false);
        await this.OutLineAsync("END").ConfigureAwait(false);

#if DISOPT
        await Console.Out.WriteLineAsync(";opt   count").ConfigureAwait(false);
        for (var i = -1; ++i <= HighSeq;)
        {
            var count = this.seq[i];
            await Console.Out.WriteLineAsync(
                $"; {i,2}   {count[0],5}").ConfigureAwait(false);
        }
#endif
    }

    /// <summary>
    /// Remember where we are in the queue in case we have to back up.
    /// </summary>
    /// <returns>
    /// Tuple of previous position in queue, starting position in queue.
    /// </returns>
    public (int? Before, int? Start) SetStage()
    {
        var before = storage.SNext;
        storage.SetStage();
        var start = storage.SNext;

        return (before, start);
    }

    /// <summary>
    /// Generate code in staging buffer.
    /// </summary>
    /// <param name="pCode">P-code to generate.</param>
    /// <param name="value">P-code value.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// If staging buffer overflows.
    /// </exception>
    public async Task GenAsync(PCode pCode, int? value)
    {
        var valueActual = value ?? 0;
        int newCsp;
        switch (pCode)
        {
            case PCode.GETb1pu:
            case PCode.GETb1p:
            case PCode.GETw1p:
                await this.GenAsync(PCode.MOVE21, 0).ConfigureAwait(false);
                break;
            case PCode.SUB12:
            case PCode.MOD12:
            case PCode.MOD12u:
            case PCode.DIV12:
            case PCode.DIV12u:
                await this.GenAsync(PCode.SWAP12, 0).ConfigureAwait(false);
                break;
            case PCode.PUSH1:
                storage.Csp -= Machine.Bpw;
                break;
            case PCode.POP2:
                storage.Csp += Machine.Bpw;
                break;
            case PCode.ADDSP:
            case PCode.RETURN:
                newCsp = valueActual;
                valueActual -= storage.Csp;
                storage.Csp = newCsp;
                break;
            case PCode.None:
            case PCode.ADD12:
            case PCode.AND12:
            case PCode.ANEG1:
            case PCode.ARGCNTn:
            case PCode.ASL12:
            case PCode.ASR12:
            case PCode.CALL1:
            case PCode.CALLm:
            case PCode.BYTE_:
            case PCode.BYTEn:
            case PCode.BYTEr0:
            case PCode.COM1:
            case PCode.DBL1:
            case PCode.DBL2:
            case PCode.ENTER:
            case PCode.EQ10f:
            case PCode.EQ12:
            case PCode.GE10f:
            case PCode.GE12:
            case PCode.GE12u:
            case PCode.POINT1l:
            case PCode.POINT1m:
            case PCode.GETb1m:
            case PCode.GETb1mu:
            case PCode.GETw1m:
            case PCode.GETw1n:
            case PCode.GETw2n:
            case PCode.GT10f:
            case PCode.GT12:
            case PCode.GT12u:
            case PCode.WORD_:
            case PCode.WORDn:
            case PCode.WORDr0:
            case PCode.JMPm:
            case PCode.LABm:
            case PCode.LE10f:
            case PCode.LE12:
            case PCode.LE12u:
            case PCode.LNEG1:
            case PCode.LT10f:
            case PCode.LT12:
            case PCode.LT12u:
            case PCode.MOVE21:
            case PCode.MUL12:
            case PCode.MUL12u:
            case PCode.NE10f:
            case PCode.NE12:
            case PCode.NEARm:
            case PCode.OR12:
            case PCode.POINT1s:
            case PCode.PUTbm1:
            case PCode.PUTbp1:
            case PCode.PUTwm1:
            case PCode.PUTwp1:
            case PCode.rDEC1:
            case PCode.REFm:
            case PCode.rINC1:
            case PCode.SWAP12:
            case PCode.SWAP1s:
            case PCode.SWITCH:
            case PCode.XOR12:
            case PCode.ADD1n:
            case PCode.ADD21:
            case PCode.ADD2n:
            case PCode.ADDbpn:
            case PCode.ADDwpn:
            case PCode.ADDm_:
            case PCode.COMMAn:
            case PCode.DECbp:
            case PCode.DECwp:
            case PCode.POINT2m:
            case PCode.POINT2m_:
            case PCode.GETb1s:
            case PCode.GETb1su:
            case PCode.GETw1m_:
            case PCode.GETw1s:
            case PCode.GETw2m:
            case PCode.GETw2p:
            case PCode.GETw2s:
            case PCode.INCbp:
            case PCode.INCwp:
            case PCode.PLUSn:
            case PCode.POINT2s:
            case PCode.PUSH2:
            case PCode.PUSHm:
            case PCode.PUSHp:
            case PCode.PUSHs:
            case PCode.PUT_m_:
            case PCode.rDEC2:
            case PCode.rINC2:
            case PCode.SUB_m_:
            case PCode.SUB1n:
            case PCode.SUBbpn:
            case PCode.SUBwpn:
            case PCode.PCODES:
            default:
                break;
        }

        if (storage.Stage is null)
        {
            await this.OutCodeAsync(pCode, valueActual).ConfigureAwait(false);
            return;
        }

        if (storage.SNext >= storage.SLast)
        {
            throw new InvalidOperationException("Staging buffer overflow");
        }

        storage.Stage.Add(new KeyValuePair<PCode, int>(pCode, valueActual));
    }

    /// <summary>
    /// Dump the contents of the queue.
    /// </summary>
    /// <param name="before">If before != null, don't dump queue yet.</param>
    /// <param name="start">If start = null, throw away contents.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ClearStageAsync(int? before, int? start)
    {
        if (before is not null)
        {
            while (storage.SNext > before)
            {
                storage.Stage?.RemoveAt(before.Value);
            }

            return;
        }

        if (start is not null)
        {
            await this.DumpStageAsync().ConfigureAwait(false);
        }

        storage.ClearStage();
    }

    /// <summary>
    /// Dump the staging buffer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DumpStageAsync()
    {
        int i;
        while (storage.Stage?.Count > 0)
        {
            if (storage.Optimize)
            {
            restart:
                i = -1;
                while (++i <= HighSeq)
                {
                    if (this.Peep(this.seq[i]))
                    {
#if DISOPT
                        var isATty =
                            storage.Output == Console.Out ||
                            storage.Output == Console.Error;
                        if (isATty)
                        {
                            await Console.Error.WriteLineAsync(
                                $"                   optimized {i,2}")
                                .ConfigureAwait(false);
                        }
#endif
                    }
                }

#pragma warning disable S907 // "goto" statement should not be used
                goto restart;
#pragma warning restore S907 // "goto" statement should not be used
            }

            var staged = storage.Stage[0];
            await this.OutCodeAsync(staged.Key, staged.Value)
                .ConfigureAwait(false);
            storage.Stage.RemoveAt(0);
        }
    }

    /// <summary>
    /// Change to a new segment.
    /// </summary>
    /// <param name="newSeg">Segment to change to.</param>
    /// <remarks>
    /// May be called with <see cref="SegmentType.None"/>,
    /// <see cref="SegmentType.CodeSeg"/>, or <see cref="SegmentType.DataSeg"/>.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ToSegAsync(SegmentType newSeg)
    {
        if (!Enum.IsDefined(newSeg))
        {
            throw new InvalidEnumArgumentException(
                nameof(newSeg), (int)newSeg, typeof(SegmentType));
        }

        if (storage.OldSeg == newSeg)
        {
            return;
        }

        if (storage.OldSeg == SegmentType.CodeSeg)
        {
            await this.OutLineAsync("CODE ENDS").ConfigureAwait(false);
        }
        else if (storage.OldSeg == SegmentType.DataSeg)
        {
            await this.OutLineAsync("DATA ENDS").ConfigureAwait(false);
        }

        if (newSeg == SegmentType.CodeSeg)
        {
            await this.OutLineAsync("CODE SEGMENT PUBLIC")
                .ConfigureAwait(false);
            await this.OutLineAsync("ASSUME CS:CODE, SS:DATA, DS:DATA")
                .ConfigureAwait(false);
        }
        else if (newSeg == SegmentType.DataSeg)
        {
            await this.OutLineAsync("DATA SEGMENT PUBLIC")
                .ConfigureAwait(false);
        }

        storage.OldSeg = newSeg;
    }

    /// <summary>
    /// Declare entry point.
    /// </summary>
    /// <param name="ident">Identity code of object being defined.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PublicAsync(SymbolIdentity ident)
    {
        var newSeg = ident == SymbolIdentity.Function ?
            SegmentType.CodeSeg : SegmentType.DataSeg;

        await this.ToSegAsync(newSeg).ConfigureAwait(false);
        await this.OutStrAsync("PUBLIC ").ConfigureAwait(false);
        await this.OutNameAsync(storage.SsName).ConfigureAwait(false);
        await this.NewLineAsync().ConfigureAwait(false);
        await this.OutNameAsync(storage.SsName).ConfigureAwait(false);
        if (ident == SymbolIdentity.Function)
        {
            await this.ColonAsync().ConfigureAwait(false);
            await this.NewLineAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Declare external reference.
    /// </summary>
    /// <param name="name">External name.</param>
    /// <param name="size">External size.</param>
    /// <param name="ident">External identity.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExternalAsync(
        string name, int size, SymbolIdentity ident)
    {
        var newSeg = ident == SymbolIdentity.Function ?
            SegmentType.CodeSeg : SegmentType.DataSeg;

        await this.ToSegAsync(newSeg).ConfigureAwait(false);
        await this.OutStrAsync("EXTRN ").ConfigureAwait(false);
        await this.OutNameAsync(name).ConfigureAwait(false);
        await this.ColonAsync().ConfigureAwait(false);
        await this.OutSizeAsync(size, ident).ConfigureAwait(false);
        await this.NewLineAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Output the size of the object pointed to.
    /// </summary>
    /// <param name="size">Object size.</param>
    /// <param name="ident">Object identity.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OutSizeAsync(
        int size, SymbolIdentity ident)
    {
        var str = ident switch
        {
            SymbolIdentity.Pointer => "WORD",
            SymbolIdentity.Function => "NEAR",
            SymbolIdentity.Label or
            SymbolIdentity.Variable or
            SymbolIdentity.Array or
            _ => size == 1 ? "BYTE" : "WORD",
        };

        await this.OutStrAsync(str).ConfigureAwait(false);
    }

    /// <summary>
    /// Point to following object(s).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PointAsync()
    {
        await this.OutLineAsync(" DW $+2").ConfigureAwait(false);
    }

    /// <summary>
    /// Dump the literal pool.
    /// </summary>
    /// <param name="size">Size of literals to dump.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DumpLitsAsync(int size)
    {
        var lits = new List<int>();
        for (var k = 0; k < storage.LitPtr; k += size)
        {
            lits.Add(utility.GetInt(k, size));
        }

        var litLists = Enumerable
            .Range(0, (int)Math.Ceiling(lits.Count / 10d))
            .Select(i => lits.Skip(i * 10).Take(10)
            .ToList());

        foreach (var list in litLists)
        {
            var pCode = size == 1 ? PCode.BYTE_ : PCode.WORD_;
            await this.GenAsync(pCode, null).ConfigureAwait(false);

            foreach (var lit in list.Take(list.Count - 1))
            {
                await this.OutDecAsync(lit).ConfigureAwait(false);
                await storage.Output.WriteAsync(',').ConfigureAwait(false);
            }

            await this.OutDecAsync(list[^1]).ConfigureAwait(false);
            await this.NewLineAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Dump zeroes for default initial values.
    /// </summary>
    /// <param name="size">Size of zero to dump.</param>
    /// <param name="count">Number of zeroes to dump.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DumpZeroAsync(int size, int count)
    {
        if (count > 0)
        {
            var pCode = size == 1 ? PCode.BYTEr0 : PCode.WORDr0;
            await this.GenAsync(pCode, count).ConfigureAwait(false);
        }
    }

    private static (int Part, bool Skip, int Cp) HandleBranch(
        int value, int part, bool skip, int cp)
    {
        part++;
        switch (part)
        {
            case 1:
                if (value == 0)
                {
                    skip = true;
                }

                break;

            case 2:
                skip = !skip;
                break;

            case 3:
                part = 0;
                skip = false;
                break;

            default:
                break;
        }

        cp++; // skip past ?
        return (part, skip, cp);
    }

    private static (int Count, int Cp, int? Back) HandleRepeat(
        int value, int count, int cp, int? back, string text)
    {
        cp++;
        if (back is null)
        {
            count = value;
            if (count < 1)
            {
                cp = text.IndexOf('#', cp) + 1;
            }
            else
            {
                back = cp;
            }
        }
        else
        {
            count--;
            if (count > 0)
            {
                cp = back.Value;
            }
            else
            {
                back = null;
            }
        }

        return (count, cp, back);
    }

    private bool Peep(int[] seq)
    {
        _ = storage;
        _ = seq;
        return false;
    }

    private async Task ColonAsync()
    {
        await storage.Output.WriteAsync(':').ConfigureAwait(false);
    }

    private async Task NewLineAsync()
    {
        await storage.Output.WriteLineAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Output assembly code.
    /// </summary>
    private async Task OutCodeAsync(PCode pCode, int value)
    {
        var part = 0;
        var skip = false;
        var count = 0;
        var cp = 0;
        int? back = null;
        var text = this.code[pCode].Text;
        while (cp < text.Length)
        {
            switch (text[cp])
            {
                case '<':
                    cp = await this.OutPlaceholderAsync(value, skip, cp, text)
                        .ConfigureAwait(false);
                    break;

                // ?..if value...?...if not value...?
                case '?':
                    (part, skip, cp) = HandleBranch(value, part, skip, cp);
                    break;

                // repeat #...# value times
                case '#':
                    (count, cp, back) = HandleRepeat(
                        value, count, cp, back, text);

                    break;

                default:
                    cp = await this.OutCharAsync(skip, cp, text)
                        .ConfigureAwait(false);
                    break;
            }
        }
    }

    private async Task<int> OutPlaceholderAsync(
        int value, bool skip, int cp, string text)
    {
        cp++; // skip to action code
        if (!skip)
        {
            switch (text[cp])
            {
                // mem ref by label
                case 'm':
                    await this.OutNameAsync(storage.SymTable[value].Name)
                        .ConfigureAwait(false);
                    break;

                // numeric constant
                case 'n':
                    await this.OutDecAsync(value).ConfigureAwait(false);
                    break;

                // current literal label
                case 'l':
                    await this.OutDecAsync(storage.LitLab)
                        .ConfigureAwait(false);
                    break;

                default:
                    break;
            }
        }

        cp += 2; // skip past >
        return cp;
    }

    private async Task<int> OutCharAsync(bool skip, int cp, string text)
    {
        if (!skip)
        {
            await storage.Output.WriteAsync(text[cp])
                .ConfigureAwait(false);
        }

        cp++;
        return cp;
    }

    private async Task OutDecAsync(int number)
    {
        await storage.Output.WriteAsync(
            number.ToString(CultureInfo.InvariantCulture))
            .ConfigureAwait(false);
    }

    private async Task OutLineAsync(string ptr)
    {
        await this.OutStrAsync(ptr).ConfigureAwait(false);
        await this.NewLineAsync().ConfigureAwait(false);
    }

    private async Task OutNameAsync(string? ptr)
    {
        await this.OutStrAsync("_").ConfigureAwait(false);
        await storage.Output.WriteAsync(
            ptr?.ToUpper(CultureInfo.InvariantCulture)).ConfigureAwait(false);
    }

    private async Task OutStrAsync(string ptr)
    {
        await storage.Output.WriteAsync(ptr).ConfigureAwait(false);
    }
}
