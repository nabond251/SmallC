// <copyright file="BackEnd.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc4;

using SmallC.Cc;
using System.ComponentModel;

/// <summary>
/// Back end.
/// </summary>
public class BackEnd(TextWriter output)
{
    private SegmentType oldSeg = SegmentType.Null;

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
        await this.ToSegAsync(SegmentType.Null).ConfigureAwait(false);
        await this.OutLineAsync("END").ConfigureAwait(false);
    }

    /// <summary>
    /// Change to a new segment.
    /// </summary>
    /// <param name="newSeg">Segment to change to.</param>
    /// <remarks>
    /// May be called with <see cref="SegmentType.Null"/>,
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

    private async Task NewLineAsync()
    {
        await output.WriteLineAsync().ConfigureAwait(false);
    }

    private async Task OutLineAsync(string ptr)
    {
        await this.OutStrAsync(ptr).ConfigureAwait(false);
        await this.NewLineAsync().ConfigureAwait(false);
    }

    private async Task OutStrAsync(string ptr)
    {
        await output.WriteAsync(ptr).ConfigureAwait(false);
    }
}
