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
        await this.ToSegAsync(SegmentType.CodeSeg)
            .ConfigureAwait(false);
        await output.WriteAsync(@"extrn __eq: near
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
").ConfigureAwait(false);
        await this.ToSegAsync(SegmentType.DataSeg)
            .ConfigureAwait(false);
        await output.WriteLineAsync("dw 0").ConfigureAwait(false);
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
            await output.WriteLineAsync("CODE ENDS")
                .ConfigureAwait(false);
        }
        else if (this.oldSeg == SegmentType.DataSeg)
        {
            await output.WriteLineAsync("DATA ENDS")
                .ConfigureAwait(false);
        }

        if (newSeg == SegmentType.CodeSeg)
        {
            await output.WriteLineAsync("CODE SEGMENT PUBLIC")
                .ConfigureAwait(false);
            await output.WriteLineAsync("ASSUME CS:CODE, SS:DATA, DS:DATA")
                .ConfigureAwait(false);
        }
        else if (newSeg == SegmentType.DataSeg)
        {
            await output.WriteLineAsync("DATA SEGMENT PUBLIC")
                .ConfigureAwait(false);
        }

        this.oldSeg = newSeg;
    }

    /// <summary>
    /// Print any assembler stuff needed at the end.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task TrailerAsync()
    {
        await this.ToSegAsync(SegmentType.Null).ConfigureAwait(false);
        await output.WriteLineAsync("END").ConfigureAwait(false);
    }
}
