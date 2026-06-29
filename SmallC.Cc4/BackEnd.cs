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
    Storage storage,
    TextWriter output)
{
    private SegmentType oldSeg = SegmentType.None;

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
        var globals = storage.SymbolTable.Globals;
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

        var cp = storage.SymbolTable.FindGlb("main");
        if (cp?.Class == SymbolClass.Static)
        {
            await this.ExternalAsync(
                "_main", 0, SymbolIdentity.Function)
                .ConfigureAwait(false);
        }

        await this.ToSegAsync(SegmentType.None).ConfigureAwait(false);
        await this.OutLineAsync("END").ConfigureAwait(false);
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

        if (this.oldSeg == newSeg)
        {
            return;
        }

        if (this.oldSeg == SegmentType.CodeSeg)
        {
            await this.OutLineAsync("CODE ENDS").ConfigureAwait(false);
        }
        else if (this.oldSeg == SegmentType.DataSeg)
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

        this.oldSeg = newSeg;
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
        await output.WriteAsync(':').ConfigureAwait(false);
    }

    private async Task NewLineAsync()
    {
        await output.WriteLineAsync().ConfigureAwait(false);
    }

    private async Task OutLineAsync(string ptr)
    {
        await this.OutStrAsync(ptr).ConfigureAwait(false);
        await this.NewLineAsync().ConfigureAwait(false);
    }

    private async Task OutNameAsync(string ptr)
    {
        await this.OutStrAsync("_").ConfigureAwait(false);
        await output.WriteAsync(
            new string([.. ptr.TakeWhile(c => c >= ' ')])
            .ToUpper(CultureInfo.InvariantCulture))
            .ConfigureAwait(false);
    }

    private async Task OutStrAsync(string ptr)
    {
        await output.WriteAsync(
            new string([.. ptr.TakeWhile(c => c >= ' ')]))
            .ConfigureAwait(false);
    }
}
