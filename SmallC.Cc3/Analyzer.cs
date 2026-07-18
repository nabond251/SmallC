// <copyright file="Analyzer.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc3;

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc4;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// Expression analyzer.
/// </summary>
public class Analyzer(
    SymbolTableUseCases symbolTable,
    UtilityUseCases utility,
    FrontEnd frontEnd,
    BackEnd backEnd,
    Storage storage)
{
    /// <summary>
    /// Gets constant expression, if any.
    /// </summary>
    /// <returns>Constant expression from next tokens, if any.</returns>
    public async Task<int?> ConstExprAsync()
    {
        int? before;

        (before, _) = backEnd.SetStage();
        var (@const, val) = await this.ExpressionAsync().ConfigureAwait(false);

        // scratch generated code
        await backEnd.ClearStageAsync(before, null).ConfigureAwait(false);
        return !@const.HasValue ?
            throw new InvalidOperationException("must be constant expression") :
            val;
    }

    /// <summary>
    /// Analyzes expression.
    /// </summary>
    /// <returns>Constant value, if any.</returns>
    public async Task<(SymbolType? Con, int Val)> ExpressionAsync()
    {
        var @is = new ExpressionAnalysis(null, null, null, null, 0, null, null);

        if ((await this.Level1Async(@is).ConfigureAwait(false)).HasValue)
        {
            await this.FetchAsync(@is).ConfigureAwait(false);
        }

        return (@is.ConstantType, @is.ConstantValue);
    }

    /// <summary>
    /// Analyzes a test expression.
    /// </summary>
    /// <param name="label">Label to jump to.</param>
    /// <param name="parens">Whether parens are needed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task TestAsync(int label, bool parens)
    {
        var @is = new ExpressionAnalysis(null, null, null, null, 0, null, null);
        int? before, start;

        if (parens)
        {
            await frontEnd.NeedAsync("(").ConfigureAwait(false);
        }

        while (true)
        {
            (before, start) = backEnd.SetStage();

            if ((await this.Level1Async(@is).ConfigureAwait(false)).HasValue)
            {
                await this.FetchAsync(@is).ConfigureAwait(false);
            }

            if (await frontEnd.MatchAsync(",").ConfigureAwait(false))
            {
                await backEnd.ClearStageAsync(before, start)
                    .ConfigureAwait(false);
            }
            else
            {
                break;
            }
        }

        if (parens)
        {
            await frontEnd.NeedAsync(")").ConfigureAwait(false);
        }

        if (@is.ConstantType.HasValue)
        {
            // constant expression
            await backEnd.ClearStageAsync(before, null).ConfigureAwait(false);
            if (@is.ConstantValue != 0)
            {
                return;
            }

            await backEnd.GenAsync(PCode.JMPm, label).ConfigureAwait(false);
            return;
        }

        // stage index of "oper 0" code
        if (@is.StageIndex.HasValue)
        {
            // operator code
#pragma warning disable IDE0010 // Add missing cases
            switch (@is.HighestBinaryOp)
            {
                case PCode.EQ12:
                case PCode.LE12u:
                    await this.ZeroJumpAsync(PCode.EQ10f, label, @is)
                        .ConfigureAwait(false);
                    break;
                case PCode.NE12:
                case PCode.GT12u:
                    await this.ZeroJumpAsync(PCode.NE10f, label, @is)
                        .ConfigureAwait(false);
                    break;
                case PCode.GT12:
                    await this.ZeroJumpAsync(PCode.GT10f, label, @is)
                        .ConfigureAwait(false);
                    break;
                case PCode.GE12:
                    await this.ZeroJumpAsync(PCode.GE10f, label, @is)
                        .ConfigureAwait(false);
                    break;
                case PCode.GE12u:
                    await backEnd.ClearStageAsync(@is.StageIndex, null)
                        .ConfigureAwait(false);
                    break;
                case PCode.LT12:
                    await this.ZeroJumpAsync(PCode.LT10f, label, @is)
                        .ConfigureAwait(false);
                    break;
                case PCode.LT12u:
                    await this.ZeroJumpAsync(PCode.JMPm, label, @is)
                        .ConfigureAwait(false);
                    break;
                case PCode.LE12:
                    await this.ZeroJumpAsync(PCode.LE10f, label, @is)
                        .ConfigureAwait(false);
                    break;
                default:
                    await backEnd.GenAsync(PCode.NE10f, label)
                        .ConfigureAwait(false);
                    break;
            }
#pragma warning restore IDE0010 // Add missing cases
        }
        else
        {
            await backEnd.GenAsync(PCode.NE10f, label).ConfigureAwait(false);
        }

        await backEnd.ClearStageAsync(before, start).ConfigureAwait(false);
    }

    /// <summary>
    /// Test primary register against zero and jump if false.
    /// </summary>
    /// <param name="oper">Operator to use.</param>
    /// <param name="label">Label to jump to if false.</param>
    /// <param name="is">Analysis results.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ZeroJumpAsync(PCode oper, int label, ExpressionAnalysis @is)
    {
        _ = storage;
        _ = oper;
        _ = label;
        _ = @is;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Analyze level 1.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<int?> Level1Async(ExpressionAnalysis @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        int? k;
        var is2 = new ExpressionAnalysis(null, null, null, null, 0, null, null);
        var is3 = new ExpressionAnalysis(null, null, null, null, 0, null, null);
        PCode? oper, oper2;

        k = await this.Down1Async(this.Level2Async, @is).ConfigureAwait(false);
        if (@is.ConstantType.HasValue)
        {
            await backEnd.GenAsync(PCode.GETw1m, @is.ConstantValue)
                .ConfigureAwait(false);
        }

        if (await frontEnd.MatchAsync("|=").ConfigureAwait(false))
        {
            oper2 = PCode.OR12;
            oper = oper2;
        }
        else if (await frontEnd.MatchAsync("^=").ConfigureAwait(false))
        {
            oper2 = PCode.XOR12;
            oper = oper2;
        }
        else if (await frontEnd.MatchAsync("&=").ConfigureAwait(false))
        {
            oper2 = PCode.AND12;
            oper = oper2;
        }
        else if (await frontEnd.MatchAsync("+=").ConfigureAwait(false))
        {
            oper2 = PCode.ADD12;
            oper = oper2;
        }
        else if (await frontEnd.MatchAsync("-=").ConfigureAwait(false))
        {
            oper2 = PCode.SUB12;
            oper = oper2;
        }
        else if (await frontEnd.MatchAsync("*=").ConfigureAwait(false))
        {
            oper = PCode.MUL12;
            oper2 = PCode.MUL12u;
        }
        else if (await frontEnd.MatchAsync("/=").ConfigureAwait(false))
        {
            oper = PCode.DIV12;
            oper2 = PCode.DIV12u;
        }
        else if (await frontEnd.MatchAsync("%=").ConfigureAwait(false))
        {
            oper = PCode.MOD12;
            oper2 = PCode.MOD12u;
        }
        else if (await frontEnd.MatchAsync(">>=").ConfigureAwait(false))
        {
            oper2 = PCode.ASR12;
            oper = oper2;
        }
        else if (await frontEnd.MatchAsync("<<=").ConfigureAwait(false))
        {
            oper2 = PCode.ASL12;
            oper = oper2;
        }
        else if (await frontEnd.MatchAsync("=").ConfigureAwait(false))
        {
            oper2 = null;
            oper = oper2;
        }
        else
        {
            return k;
        }

        // have an assignment operator
        if (!k.HasValue)
        {
            ErrorUseCases.NeedLVal();
            return null;
        }

        is3.SymbolTableEntry = @is.SymbolTableEntry;
        is3.IndirectType = @is.IndirectType;

        // indirect target
        if (@is.IndirectType.HasValue)
        {
            // ?=
            if (oper.HasValue)
            {
                // save address
                await backEnd.GenAsync(PCode.PUSH1, null).ConfigureAwait(false);

                // fetch left side
                await this.FetchAsync(@is).ConfigureAwait(false);
            }

            // parse right side
            await this.Down2Async(oper, oper2, this.Level1Async, @is, is2)
                .ConfigureAwait(false);

            // retrieve address
            await backEnd.GenAsync(PCode.POP2, null).ConfigureAwait(false);
        }

        // direct target
        else
        {
            // ?=
            if (oper.HasValue)
            {
                // fetch left side
                await this.FetchAsync(@is).ConfigureAwait(false);

                // parse right side
                await this.Down2Async(oper, oper2, this.Level1Async, @is, is2)
                    .ConfigureAwait(false);
            }

            // =
            else
            {
                // parse right side
                if ((await this.Level1Async(@is2).ConfigureAwait(false))
                    .HasValue)
                {
                    await this.FetchAsync(is2).ConfigureAwait(false);
                }
            }
        }

        // store result
        await this.StoreAsync(is3).ConfigureAwait(false);
        return null;
    }

    /// <summary>
    /// Analyze level 2.
    /// </summary>
    /// <param name="is1">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<int?> Level2Async(ExpressionAnalysis is1)
    {
        ArgumentNullException.ThrowIfNull(is1);

        var is2 = new ExpressionAnalysis(null, null, null, null, 0, null, null);
        var is3 = new ExpressionAnalysis(null, null, null, null, 0, null, null);
        int? k;
        int flab, endLab;

        // expression 1
        k = await this.Down1Async(this.Level3Async, is1).ConfigureAwait(false);
        if (!await frontEnd.MatchAsync("?").ConfigureAwait(false))
        {
            return k;
        }

        flab = utility.GetLabel();
        await this.DropOutAsync(k, PCode.NE10f, flab, is1)
            .ConfigureAwait(false);

        // expression 2
        if ((await this.Down1Async(this.Level2Async, is2).ConfigureAwait(false))
            .HasValue)
        {
            await this.FetchAsync(is2).ConfigureAwait(false);
        }
        else if (is2.ConstantType.HasValue)
        {
            await backEnd.GenAsync(PCode.GETw1n, is2.ConstantValue)
                .ConfigureAwait(false);
        }

        await frontEnd.NeedAsync(":").ConfigureAwait(false);
        endLab = utility.GetLabel();
        await backEnd.GenAsync(PCode.JMPm, flab).ConfigureAwait(false);

        // expression 3
        if ((await this.Down1Async(this.Level2Async, is3).ConfigureAwait(false))
            .HasValue)
        {
            await this.FetchAsync(is3).ConfigureAwait(false);
        }
        else if (is3.ConstantType.HasValue)
        {
            await backEnd.GenAsync(PCode.GETw1n, is3.ConstantValue)
                .ConfigureAwait(false);
        }

        await backEnd.GenAsync(PCode.JMPm, endLab).ConfigureAwait(false);
        is1.ConstantValue = 0;
        is1.ConstantType = null;

        // expr1 ? const2 : const3
        if (is2.ConstantType.HasValue && is3.ConstantType.HasValue)
        {
            is1.StageIndex = null;
            is1.IndirectType = null;
            is1.AddressType = null;
        }

        // expr1 ? var2 : const3
        else if (is3.ConstantType.HasValue)
        {
            is1.AddressType = is2.AddressType;
            is1.IndirectType = is2.IndirectType;
            is1.StageIndex = is2.StageIndex;
        }

        // expr1 ? const2 : var3
        // expr1 ? same2 : same3
        else if (is2.ConstantType.HasValue
            || is2.AddressType == is3.AddressType)
        {
            is1.AddressType = is3.AddressType;
            is1.IndirectType = is3.IndirectType;
            is1.StageIndex = is3.StageIndex;
        }
        else
        {
            throw new InvalidOperationException("mismatched expressions");
        }

        return null;
    }

    /// <summary>
    /// Analyze level 3.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<int?> Level3Async(ExpressionAnalysis @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        return await this.SkimAsync(
            ["||"], PCode.EQ10f, 1, 0, this.Level4Async, @is)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Analyze level 4.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<int?> Level4Async(ExpressionAnalysis @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        return await this.SkimAsync(
            ["&&"], PCode.NE10f, 0, 1, this.Level5Async, @is)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Analyze level 5.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<int?> Level5Async(ExpressionAnalysis @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        return await this.DownAsync(["|"], 0, this.Level6Async, @is)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Analyze level 6.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<int?> Level6Async(ExpressionAnalysis @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        return await this.DownAsync(["^"], 1, this.Level7Async, @is)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Analyze level 7.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<int?> Level7Async(ExpressionAnalysis @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        return await this.DownAsync(["&"], 2, this.Level8Async, @is)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Analyze level 8.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<int?> Level8Async(ExpressionAnalysis @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        return await this.DownAsync(["==", "!="], 3, this.Level9Async, @is)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Analyze level 9.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<int?> Level9Async(ExpressionAnalysis @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        return await this.DownAsync(["<=", ">=", "<", ">"], 5, this.Level10Async, @is)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Analyze level 10.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<int?> Level10Async(ExpressionAnalysis @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        return await this.DownAsync([">>", "<<"], 9, this.Level11Async, @is)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Analyze level 11.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<int?> Level11Async(ExpressionAnalysis @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        return await this.DownAsync(["+", "-"], 11, this.Level12Async, @is)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Analyze level 12.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<int?> Level12Async(ExpressionAnalysis @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        return await this.DownAsync(["*", "/", "%"], 13, this.Level13Async, @is)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Analyze level 13.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "not code")]
    public async Task<int?> Level13Async(ExpressionAnalysis @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        int? k;

        // ++lval
        if (await frontEnd.MatchAsync("++").ConfigureAwait(false))
        {
            if (!(await this.Level13Async(@is).ConfigureAwait(false)).HasValue)
            {
                ErrorUseCases.NeedLVal();
                return null;
            }

            await this.StepAsync(PCode.rINC1, @is, 0).ConfigureAwait(false);
            return null;
        }

        // ++lval
        else if (await frontEnd.MatchAsync("--").ConfigureAwait(false))
        {
            if (!(await this.Level13Async(@is).ConfigureAwait(false)).HasValue)
            {
                ErrorUseCases.NeedLVal();
                return null;
            }

            await this.StepAsync(PCode.rDEC1, @is, 0).ConfigureAwait(false);
            return null;
        }

        // ~
        else if (await frontEnd.MatchAsync("~").ConfigureAwait(false))
        {
            if ((await this.Level13Async(@is).ConfigureAwait(false)).HasValue)
            {
                await this.FetchAsync(@is).ConfigureAwait(false);
            }

            await backEnd.GenAsync(PCode.COM1, null).ConfigureAwait(false);
            @is.ConstantValue = ~@is.ConstantValue;
            @is.SymbolTableEntry = null;
            return null;
        }

        // !
        else if (await frontEnd.MatchAsync("!").ConfigureAwait(false))
        {
            if ((await this.Level13Async(@is).ConfigureAwait(false)).HasValue)
            {
                await this.FetchAsync(@is).ConfigureAwait(false);
            }

            await backEnd.GenAsync(PCode.LNEG1, null).ConfigureAwait(false);
            @is.ConstantValue = @is.ConstantValue == 0 ? 1 : 0;
            @is.SymbolTableEntry = null;
            return null;
        }

        // unary -
        else if (await frontEnd.MatchAsync("-").ConfigureAwait(false))
        {
            if ((await this.Level13Async(@is).ConfigureAwait(false)).HasValue)
            {
                await this.FetchAsync(@is).ConfigureAwait(false);
            }

            await backEnd.GenAsync(PCode.ANEG1, null).ConfigureAwait(false);
            @is.ConstantValue = -@is.ConstantValue;
            @is.SymbolTableEntry = null;
            return null;
        }

        // unary *
        else if (await frontEnd.MatchAsync("*").ConfigureAwait(false))
        {
            if ((await this.Level13Async(@is).ConfigureAwait(false)).HasValue)
            {
                await this.FetchAsync(@is).ConfigureAwait(false);
            }

            @is.IndirectType = @is.SymbolTableEntry is SymbolTableEntry ptr ?
                ptr.Type : SymbolType.Int;
            @is.StageIndex = null; // no (op 0) stage index
            @is.AddressType = null; // not an address
            @is.ConstantType = null; // not a constant
            @is.ConstantValue = 1; // omit FetchAsync() on func call
            return 1;
        }

        // sizeof()
        else if (await frontEnd.AMatchAsync("sizeof", 6).ConfigureAwait(false))
        {
            int sz;
            bool p;

            p = await frontEnd.MatchAsync("(").ConfigureAwait(false);
            sz = 0;
            if (await frontEnd.AMatchAsync("unsigned", 8).ConfigureAwait(false))
            {
                sz = Machine.Bpw;
            }

            if (await frontEnd.AMatchAsync("int", 3).ConfigureAwait(false))
            {
                sz = Machine.Bpw;
            }
            else if (await frontEnd.AMatchAsync("char", 4)
                .ConfigureAwait(false))
            {
                sz = 1;
            }

            if (sz != 0)
            {
                if (await frontEnd.MatchAsync("*").ConfigureAwait(false))
                {
                    sz = Machine.Bpw;
                }
            }
            else
            {
                sz =
                    await frontEnd.SymNameAsync().ConfigureAwait(false)
                    is string sName
                    && (symbolTable.FindLoc(sName)
                    ?? symbolTable.FindGlb(sName))
                    is SymbolTableEntry ptr
                    && ptr.Ident != SymbolIdentity.Function
                    && ptr.Ident != SymbolIdentity.Label
                    ? ptr.Size
                    : throw new InvalidOperationException(
                        "must be object or type");
            }

            if (p)
            {
                await frontEnd.NeedAsync(")").ConfigureAwait(false);
            }

            @is.ConstantType = SymbolType.Int;
            @is.ConstantValue = sz;
            @is.AddressType = null;
            @is.IndirectType = null;
            @is.StageIndex = null;
            return null;
        }

        // unary &
        else if (await frontEnd.MatchAsync("&").ConfigureAwait(false))
        {
            if (!(await this.Level13Async(@is).ConfigureAwait(false)).HasValue)
            {
                throw new InvalidOperationException("illegal address");
            }

            var ptr = @is.SymbolTableEntry ??
                throw new InvalidOperationException();
            @is.AddressType = ptr.Type;
            if (@is.IndirectType.HasValue)
            {
                return null;
            }

            var index = storage.SymTab.IndexOf(ptr);
            await backEnd.GenAsync(PCode.POINT1m, index).ConfigureAwait(false);
            @is.IndirectType = ptr.Type;
            return null;
        }
        else
        {
            k = await this.Level14Async(@is).ConfigureAwait(false);

            // lval++
            if (await frontEnd.MatchAsync("++").ConfigureAwait(false))
            {
                if (!k.HasValue)
                {
                    ErrorUseCases.NeedLVal();
                    return null;
                }

                await this.StepAsync(PCode.rINC1, @is, PCode.rDEC1)
                    .ConfigureAwait(false);
                return null;
            }

            // lval--
            else if (await frontEnd.MatchAsync("--").ConfigureAwait(false))
            {
                if (!k.HasValue)
                {
                    ErrorUseCases.NeedLVal();
                    return null;
                }

                await this.StepAsync(PCode.rDEC1, @is, PCode.rINC1)
                    .ConfigureAwait(false);
                return null;
            }
            else
            {
                return k;
            }
        }
    }

    /// <summary>
    /// Analyze level 14.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<int?> Level14Async(ExpressionAnalysis @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        int? k;
        SymbolTableEntry? ptr;
        int? before, start;

        k = await this.PrimaryAsync(@is).ConfigureAwait(false);
        ptr = @is.SymbolTableEntry;
        await frontEnd.BlanksAsync().ConfigureAwait(false);
        if (storage.Ch is '[' or '(')
        {
            // allocate only if needed
            var is2 = new ExpressionAnalysis(
                null, null, null, null, 0, null, null);

            while (true)
            {
                // [subscript]
                if (await frontEnd.MatchAsync("[").ConfigureAwait(false))
                {
                    if (ptr is null)
                    {
                        throw new InvalidOperationException("can't subscript");
                    }

                    if (@is.AddressType.HasValue)
                    {
                        if (k.HasValue)
                        {
                            await this.FetchAsync(@is).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "can't subscript");
                    }

                    (before, start) = backEnd.SetStage();
                    is2.ConstantType = null;
                    await this
                        .Down2Async(null, null, this.Level1Async, is2, is2)
                        .ConfigureAwait(false);
                    await frontEnd.NeedAsync("]").ConfigureAwait(false);
                    if (is2.ConstantType.HasValue)
                    {
                        await backEnd.ClearStageAsync(before, null)
                            .ConfigureAwait(false);
                        if (is2.ConstantValue != 0)
                        {
                            // only add if non-zero
                            if ((int)ptr.Type >> 2 == Machine.Bpw)
                            {
                                await backEnd.GenAsync(
                                    PCode.GETw2n,
                                    is2.ConstantValue << Machine.Lbpw)
                                    .ConfigureAwait(false);
                            }
                            else
                            {
                                await backEnd.GenAsync(
                                    PCode.GETw2n,
                                    is2.ConstantValue).ConfigureAwait(false);
                            }

                            await backEnd.GenAsync(PCode.ADD12, null)
                                .ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        if ((int)ptr.Type >> 2 == Machine.Bpw)
                        {
                            await backEnd.GenAsync(PCode.DBL1, null)
                                .ConfigureAwait(false);
                        }

                        await backEnd.GenAsync(PCode.ADD12, null)
                            .ConfigureAwait(false);
                    }

                    @is.AddressType = null;
                    @is.IndirectType = ptr.Type;
                    k = 1;
                }

                // function(...)
                else if (await frontEnd.MatchAsync("(")
                    .ConfigureAwait(false))
                {
                    if (ptr is null)
                    {
                        await this.CallFuncAsync(null)
                            .ConfigureAwait(false);
                    }
                    else if (ptr.Ident != SymbolIdentity.Function)
                    {
                        if (k.HasValue && @is.ConstantValue == 0)
                        {
                            await this.FetchAsync(@is)
                                .ConfigureAwait(false);
                        }

                        await this.CallFuncAsync(null)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        await this.CallFuncAsync(ptr).ConfigureAwait(false);
                    }

                    k = null;
                    @is.SymbolTableEntry = null;
                    @is.ConstantType = null;
                    @is.ConstantValue = 0;
                }
                else
                {
                    return k;
                }
            }
        }

        if (ptr?.Ident == SymbolIdentity.Function)
        {
            var index = storage.SymTab.IndexOf(ptr);
            await backEnd.GenAsync(PCode.POINT1m, index).ConfigureAwait(false);
            @is.SymbolTableEntry = null;
            return null;
        }

        return k;
    }

    /// <summary>
    /// Analyze primary term.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<int?> PrimaryAsync(ExpressionAnalysis @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        string? sName;
        int? k;

        // (subexpression)
        if (await frontEnd.MatchAsync("(").ConfigureAwait(false))
        {
            do
            {
                k = await this.Level1Async(@is).ConfigureAwait(false);
            }
            while (await frontEnd.MatchAsync(",").ConfigureAwait(false));
            await frontEnd.NeedAsync(")").ConfigureAwait(false);
            return k;
        }

        @is.SymbolTableEntry = null;
        @is.IndirectType = null;
        @is.AddressType = null;
        @is.ConstantType = null;
        @is.ConstantValue = 0;
        @is.HighestBinaryOp = null;
        @is.StageIndex = null;

        sName = await frontEnd.SymNameAsync().ConfigureAwait(false);

        // is legal symbol
        if (sName is not null)
        {
            // is local
            if (symbolTable.FindLoc(sName) is SymbolTableEntry ptrLoc)
            {
                if (ptrLoc.Ident == SymbolIdentity.Label)
                {
                    await this.ExpErrAsync().ConfigureAwait(false);
                    return null;
                }

                await backEnd.GenAsync(PCode.POINT1s, ptrLoc.Offset)
                    .ConfigureAwait(false);
                @is.SymbolTableEntry = ptrLoc;
                @is.IndirectType = ptrLoc.Type;
                if (ptrLoc.Ident == SymbolIdentity.Array)
                {
                    @is.AddressType = ptrLoc.Type;
                    return null;
                }

                if (ptrLoc.Ident == SymbolIdentity.Pointer)
                {
                    @is.IndirectType = SymbolType.UInt;
                    @is.AddressType = ptrLoc.Type;
                }

                return 1;
            }

            // is global
            if (symbolTable.FindGlb(sName) is SymbolTableEntry ptrGlb)
            {
                @is.SymbolTableEntry = ptrGlb;
                if (ptrGlb.Ident != SymbolIdentity.Function)
                {
                    if (ptrGlb.Ident == SymbolIdentity.Array)
                    {
                        await backEnd.GenAsync(
                            PCode.POINT1m, storage.SymTab.IndexOf(ptrGlb))
                            .ConfigureAwait(false);
                        @is.IndirectType = ptrGlb.Type;
                        @is.AddressType = ptrGlb.Type;
                        return null;
                    }

                    if (ptrGlb.Ident == SymbolIdentity.Pointer)
                    {
                        @is.AddressType = ptrGlb.Type;
                    }

                    return 1;
                }
            }
            else
            {
                @is.SymbolTableEntry = symbolTable.AddSym(
                    sName,
                    SymbolIdentity.Function,
                    SymbolType.Int,
                    0,
                    0,
                    storage.SymTab.Globals,
                    SymbolClass.AutoExt);
            }

            return null;
        }

        if (!this.Constant(@is))
        {
            await this.ExpErrAsync().ConfigureAwait(false);
        }

        return null;
    }

    /// <summary>
    /// Outputs invalid expression error.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ExpErrAsync()
    {
        _ = storage;
        throw new InvalidOperationException("invalid expression");
    }

    /// <summary>
    /// Call function.
    /// </summary>
    /// <param name="ptr">Entry of function to call, if direct call.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "literature")]
    public async Task CallFuncAsync(SymbolTableEntry? ptr)
    {
        int nArgs;

        nArgs = 0;

        // already saw open paren
        await frontEnd.BlanksAsync().ConfigureAwait(false);
        while (storage.LPtr >= storage.Line.Length
            || FrontEnd.StrEq(storage.Line[storage.LPtr..], ")") == 0)
        {
            if (await frontEnd.EndStAsync().ConfigureAwait(false))
            {
                break;
            }

            if (ptr is not null)
            {
                _ = await this.ExpressionAsync().ConfigureAwait(false);
                await backEnd.GenAsync(PCode.PUSH1, null).ConfigureAwait(false);
            }
            else
            {
                await backEnd.GenAsync(PCode.PUSH1, null).ConfigureAwait(false);
                _ = await this.ExpressionAsync().ConfigureAwait(false);

                // don't push addr
                await backEnd.GenAsync(PCode.SWAP1s, null)
                    .ConfigureAwait(false);
            }

            nArgs += Machine.Bpw; // count args*BPW
            if (!await frontEnd.MatchAsync(",").ConfigureAwait(false))
            {
                break;
            }
        }

        await frontEnd.NeedAsync(")").ConfigureAwait(false);
        if (FrontEnd.StrEq(ptr?.Name ?? string.Empty, "CCARGC") == 0)
        {
            await backEnd.GenAsync(PCode.ARGCNTn, nArgs >> Machine.Lbpw)
                .ConfigureAwait(false);
        }

        if (ptr is not null)
        {
            await backEnd.GenAsync(PCode.CALLm, storage.SymTab.IndexOf(ptr))
                .ConfigureAwait(false);
        }
        else
        {
            await backEnd.GenAsync(PCode.CALL1, null).ConfigureAwait(false);
        }

        await backEnd.GenAsync(PCode.ADDSP, storage.Csp + nArgs)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Step.
    /// </summary>
    /// <param name="oper">Operator.</param>
    /// <param name="is">Expression analysis for result.</param>
    /// <param name="oper2">Second operator.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task StepAsync(PCode oper, ExpressionAnalysis @is, PCode oper2)
    {
        _ = storage;
        _ = oper;
        _ = @is;
        _ = oper2;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Store result.
    /// </summary>
    /// <param name="is">Expression analysis for result.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task StoreAsync(ExpressionAnalysis @is)
    {
        _ = storage;
        _ = @is;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Fetch operand.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task FetchAsync(ExpressionAnalysis @is)
    {
        _ = storage;
        _ = storage;
        _ = @is;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parses constant.
    /// </summary>
    /// <param name="is">Expression analysis.</param>
    /// <returns>A value indicating whether expression is constant.</returns>
    public bool Constant(ExpressionAnalysis @is)
    {
        _ = storage;
        _ = @is;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parses quoted strings.
    /// </summary>
    /// <returns>Literal table offset, if parsed.</returns>
    public async Task<int?> StringAsync()
    {
        if (!await frontEnd.MatchAsync(storage.Quote).ConfigureAwait(false))
        {
            return null;
        }

        var offset = storage.LitPtr;
        while (storage.Ch != '"')
        {
            if (!storage.Ch.HasValue)
            {
                break;
            }

            this.StowLit(this.LitChar() ?? 0, 1);
        }

        _ = frontEnd.Gch();
        storage.LitQ.Add(0);
        return offset;
    }

    /// <summary>
    /// Places character or integer values in the literal pool.
    /// </summary>
    /// <param name="value">Value to place.</param>
    /// <param name="size">Value size.</param>
    public void StowLit(int value, int size)
    {
        if (storage.LitPtr + size >= LiteralPool.LitMax)
        {
            throw new InvalidOperationException("literal queue overflow");
        }

        utility.PutInt(value, storage.LitPtr, size);
    }

    /// <summary>
    /// Parses character literal.
    /// </summary>
    /// <returns>Parsed literal.</returns>
    public int? LitChar()
    {
        int i, oct;

        if (storage.Ch != '\\' || !storage.NCh.HasValue)
        {
            return frontEnd.Gch();
        }

        _ = frontEnd.Gch();
        switch (storage.Ch)
        {
            case 'n':
                _ = frontEnd.Gch();
                return '\n';
            case 't':
                _ = frontEnd.Gch();
                return 9; // HT
            case 'b':
                _ = frontEnd.Gch();
                return 8; // BS
            case 'f':
                _ = frontEnd.Gch();
                return 12; // FF
            default:
                break;
        }

        i = 3;
        oct = 0;
        while (i-- > 0 && storage.Ch >= '0' && storage.Ch <= '7')
        {
            oct = (oct << 3) + (frontEnd.Gch() ?? 0) - '0';
        }

        return i == 2 ? frontEnd.Gch() : oct;
    }

    /// <summary>
    /// Skim over terms adjoining || and &amp;&amp; operators.
    /// </summary>
    private async Task<int?> SkimAsync(
        IList<string> ops,
        PCode tCode,
        int dropVal,
        int endVal,
        Func<ExpressionAnalysis, Task<int?>> levelAsync,
        ExpressionAnalysis @is)
    {
        int? k;
        int dropLab, endLab;

        dropLab = 0;
        while (true)
        {
            k = await this.Down1Async(levelAsync, @is).ConfigureAwait(false);
            if (await frontEnd.NextOpAsync(ops).ConfigureAwait(false))
            {
                frontEnd.Bump(storage.OpSize);
                if (dropLab == 0)
                {
                    dropLab = utility.GetLabel();
                    await this.DropOutAsync(k, tCode, dropLab, @is)
                        .ConfigureAwait(false);
                }
            }
            else if (dropLab != 0)
            {
                await this.DropOutAsync(k, tCode, dropLab, @is)
                    .ConfigureAwait(false);
                await backEnd.GenAsync(PCode.GETw1n, endVal)
                    .ConfigureAwait(false);
                endLab = utility.GetLabel();
                await backEnd.GenAsync(PCode.JMPm, endLab)
                    .ConfigureAwait(false);
                await backEnd.GenAsync(PCode.LABm, dropLab)
                    .ConfigureAwait(false);
                await backEnd.GenAsync(PCode.GETw1n, dropVal)
                    .ConfigureAwait(false);
                await backEnd.GenAsync(PCode.LABm, endLab)
                    .ConfigureAwait(false);
                @is.IndirectType = null;
                @is.AddressType = null;
                @is.ConstantType = null;
                @is.ConstantValue = 0;
                @is.StageIndex = null;
                return null;
            }
            else
            {
                return k;
            }
        }
    }

    /// <summary>
    /// Test for early dropout from || or &amp;&amp; sequence.
    /// </summary>
    private Task DropOutAsync(
        int? k, PCode tCode, int exit1, ExpressionAnalysis @is)
    {
        _ = storage;
        _ = k;
        _ = tCode;
        _ = exit1;
        _ = @is;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Drop to a lower level.
    /// </summary>
    private async Task<int?> DownAsync(
        IList<string> ops,
        int opOff,
        Func<ExpressionAnalysis, Task<int?>> levelAsync,
        ExpressionAnalysis @is)
    {
        int? k;

        k = await this.Down1Async(levelAsync, @is).ConfigureAwait(false);
        if (!await frontEnd.NextOpAsync(ops).ConfigureAwait(false))
        {
            return k;
        }

        if (k.HasValue)
        {
            await this.FetchAsync(@is).ConfigureAwait(false);
        }

        while (true)
        {
            if (await frontEnd.NextOpAsync(ops).ConfigureAwait(false))
            {
                // allocate only if needed
                var is2 = new ExpressionAnalysis(
                    null, null, null, null, 0, null, null);
                frontEnd.Bump(storage.OpSize);
                storage.OpIndex += opOff;
                await this.Down2Async(
                    storage.Op[storage.OpIndex],
                    storage.Op2[storage.OpIndex],
                    levelAsync,
                    @is,
                    is2).ConfigureAwait(false);
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Unary drop to a lower level.
    /// </summary>
    private async Task<int?> Down1Async(
        Func<ExpressionAnalysis, Task<int?>> levelAsync, ExpressionAnalysis @is)
    {
        int? k, before;

        (before, _) = backEnd.SetStage();
        k = await levelAsync(@is).ConfigureAwait(false);
        if (@is.ConstantType.HasValue)
        {
            // load constant later
            await backEnd.ClearStageAsync(before, null).ConfigureAwait(false);
        }

        return k;
    }

    /// <summary>
    /// Binary drop to a lower level.
    /// </summary>
    private Task Down2Async(
        PCode? oper,
        PCode? oper2,
        Func<ExpressionAnalysis, Task<int?>> levelAsync,
        ExpressionAnalysis @is,
        ExpressionAnalysis is2)
    {
        _ = storage;
        _ = oper;
        _ = oper2;
        _ = levelAsync;
        _ = @is;
        _ = is2;
        throw new NotImplementedException();
    }
}
