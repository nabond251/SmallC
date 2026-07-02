// <copyright file="BackEnd.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc4;

using SmallC.Cc;
using System.ComponentModel;
using System.Globalization;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// Back end.
/// </summary>
public class BackEnd(
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

        var cp = storage.SymTable.FindGlb("main");
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
    public async Task GenAsync(PCode pCode, int value)
    {
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
                newCsp = value;
                value -= storage.Csp;
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
            await this.OutCodeAsync(pCode, value).ConfigureAwait(false);
            return;
        }

        if (storage.SNext >= storage.SLast)
        {
            throw new InvalidOperationException("Staging buffer overflow");
        }

        storage.Stage.Add(new KeyValuePair<PCode, int>(pCode, value));
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

            var code = storage.Stage[0];
            await this.OutCodeAsync(code.Key, code.Value).ConfigureAwait(false);
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
        if (ident == SymbolIdentity.Function)
        {
            await this.ToSegAsync(SegmentType.CodeSeg).ConfigureAwait(false);
        }
        else
        {
            await this.ToSegAsync(SegmentType.DataSeg).ConfigureAwait(false);
        }

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
        if (ident == SymbolIdentity.Function)
        {
            await this.ToSegAsync(SegmentType.CodeSeg).ConfigureAwait(false);
        }
        else
        {
            await this.ToSegAsync(SegmentType.DataSeg).ConfigureAwait(false);
        }

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
        if (size == 1
            && ident != SymbolIdentity.Pointer
            && ident != SymbolIdentity.Function)
        {
            await this.OutStrAsync("BYTE").ConfigureAwait(false);
        }
        else if (ident != SymbolIdentity.Function)
        {
            await this.OutStrAsync("WORD").ConfigureAwait(false);
        }
        else
        {
            await this.OutStrAsync("NEAR").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Point to following object(s).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PointAsync()
    {
        await this.OutLineAsync(" DW $+2").ConfigureAwait(false);
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
    private Task OutCodeAsync(PCode pCode, int value)
    {
        _ = storage;
        _ = pCode;
        _ = value;
        return Task.CompletedTask;
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
        await storage.Output.WriteAsync(
            new string([.. ptr.TakeWhile(c => c >= ' ')]))
            .ConfigureAwait(false);
    }
}
