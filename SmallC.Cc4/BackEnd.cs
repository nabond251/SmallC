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

    private async Task ColonAsync()
    {
        await storage.Output.WriteAsync(':').ConfigureAwait(false);
    }

    private async Task NewLineAsync()
    {
        await storage.Output.WriteLineAsync().ConfigureAwait(false);
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
