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
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "literature")]
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
        return !@const ?
            throw new InvalidOperationException("must be constant expression") :
            val;
    }

    /// <summary>
    /// Analyzes expression.
    /// </summary>
    /// <returns>Constant value, if any.</returns>
    public async Task<(bool Con, int Val)> ExpressionAsync()
    {
        var @is = new Expression(null, null, null, null, 0, null, null);

        if (await this.Level1Async(@is).ConfigureAwait(false))
        {
            await this.FetchAsync(@is).ConfigureAwait(false);
        }

        return (@is.ConstantType.HasValue, @is.ConstantValue);
    }

    /// <summary>
    /// Analyzes a test expression.
    /// </summary>
    /// <param name="label">Label to jump to.</param>
    /// <param name="parens">Whether parens are needed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task TestAsync(int label, bool parens)
    {
        var @is = new Expression(null, null, null, null, 0, null, null);
        int? before, start;

        if (parens)
        {
            await frontEnd.NeedAsync("(").ConfigureAwait(false);
        }

        while (true)
        {
            (before, start) = backEnd.SetStage();

            if (await this.Level1Async(@is).ConfigureAwait(false))
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
    public async Task ZeroJumpAsync(
        PCode oper, int label, Expression @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        // purge conventional code
        await backEnd.ClearStageAsync(@is.StageIndex, null)
            .ConfigureAwait(false);
        await backEnd.GenAsync(oper, label).ConfigureAwait(false);
    }

    /// <summary>
    /// Analyze level 1.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<bool> Level1Async(Expression @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        bool k;
        var is2 = new Expression(null, null, null, null, 0, null, null);
        var is3 = new Expression(null, null, null, null, 0, null, null);
        PCode? oper, oper2;

        k = await this.Down1Async(this.Level2Async, @is).ConfigureAwait(false);
        if (@is.ConstantType.HasValue)
        {
            await backEnd.GenAsync(PCode.GETw1n, @is.ConstantValue)
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
        if (!k)
        {
            ErrorUseCases.NeedLVal();
            return false;
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

            if (oper.HasValue)
            {
                // retrieve address
                await backEnd.GenAsync(PCode.POP2, null).ConfigureAwait(false);
            }
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
                if (await this.Level1Async(@is2).ConfigureAwait(false))
                {
                    await this.FetchAsync(is2).ConfigureAwait(false);
                }
            }
        }

        // store result
        await this.StoreAsync(is3).ConfigureAwait(false);
        return false;
    }

    /// <summary>
    /// Analyze level 2.
    /// </summary>
    /// <param name="is1">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<bool> Level2Async(Expression is1)
    {
        ArgumentNullException.ThrowIfNull(is1);

        var is2 = new Expression(null, null, null, null, 0, null, null);
        var is3 = new Expression(null, null, null, null, 0, null, null);
        bool k;
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
        if (await this.Down1Async(this.Level2Async, is2).ConfigureAwait(false))
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
        if (await this.Down1Async(this.Level2Async, is3).ConfigureAwait(false))
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

        return false;
    }

    /// <summary>
    /// Analyze level 3.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<bool> Level3Async(Expression @is)
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
    public async Task<bool> Level4Async(Expression @is)
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
    public async Task<bool> Level5Async(Expression @is)
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
    public async Task<bool> Level6Async(Expression @is)
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
    public async Task<bool> Level7Async(Expression @is)
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
    public async Task<bool> Level8Async(Expression @is)
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
    public async Task<bool> Level9Async(Expression @is)
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
    public async Task<bool> Level10Async(Expression @is)
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
    public async Task<bool> Level11Async(Expression @is)
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
    public async Task<bool> Level12Async(Expression @is)
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
    public async Task<bool> Level13Async(Expression @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        bool k;

        // ++lval
        if (await frontEnd.MatchAsync("++").ConfigureAwait(false))
        {
            if (!await this.Level13Async(@is).ConfigureAwait(false))
            {
                ErrorUseCases.NeedLVal();
                return false;
            }

            await this.StepAsync(PCode.rINC1, @is, null).ConfigureAwait(false);
            return false;
        }

        // ++lval
        else if (await frontEnd.MatchAsync("--").ConfigureAwait(false))
        {
            if (!await this.Level13Async(@is).ConfigureAwait(false))
            {
                ErrorUseCases.NeedLVal();
                return false;
            }

            await this.StepAsync(PCode.rDEC1, @is, null).ConfigureAwait(false);
            return false;
        }

        // ~
        else if (await frontEnd.MatchAsync("~").ConfigureAwait(false))
        {
            if (await this.Level13Async(@is).ConfigureAwait(false))
            {
                await this.FetchAsync(@is).ConfigureAwait(false);
            }

            await backEnd.GenAsync(PCode.COM1, null).ConfigureAwait(false);
            @is.ConstantValue = ~@is.ConstantValue;
            @is.SymbolTableEntry = null;
            return false;
        }

        // !
        else if (await frontEnd.MatchAsync("!").ConfigureAwait(false))
        {
            if (await this.Level13Async(@is).ConfigureAwait(false))
            {
                await this.FetchAsync(@is).ConfigureAwait(false);
            }

            await backEnd.GenAsync(PCode.LNEG1, null).ConfigureAwait(false);
            @is.ConstantValue = @is.ConstantValue == 0 ? 1 : 0;
            @is.SymbolTableEntry = null;
            return false;
        }

        // unary -
        else if (await frontEnd.MatchAsync("-").ConfigureAwait(false))
        {
            if (await this.Level13Async(@is).ConfigureAwait(false))
            {
                await this.FetchAsync(@is).ConfigureAwait(false);
            }

            await backEnd.GenAsync(PCode.ANEG1, null).ConfigureAwait(false);
            @is.ConstantValue = -@is.ConstantValue;
            @is.SymbolTableEntry = null;
            return false;
        }

        // unary *
        else if (await frontEnd.MatchAsync("*").ConfigureAwait(false))
        {
            if (await this.Level13Async(@is).ConfigureAwait(false))
            {
                await this.FetchAsync(@is).ConfigureAwait(false);
            }

            @is.IndirectType = @is.SymbolTableEntry is SymbolTableEntry ptr ?
                ptr.Type : SymbolType.Int;
            @is.StageIndex = null; // no (op 0) stage index
            @is.AddressType = null; // not an address
            @is.ConstantType = null; // not a constant
            @is.ConstantValue = 1; // omit FetchAsync() on func call
            return true;
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
            return false;
        }

        // unary &
        else if (await frontEnd.MatchAsync("&").ConfigureAwait(false))
        {
            if (!await this.Level13Async(@is).ConfigureAwait(false))
            {
                throw new InvalidOperationException("illegal address");
            }

            var ptr = @is.SymbolTableEntry ??
                throw new InvalidOperationException();
            @is.AddressType = ptr.Type;
            if (@is.IndirectType.HasValue)
            {
                return false;
            }

            var index = storage.SymTab.IndexOf(ptr);
            await backEnd.GenAsync(PCode.POINT1m, index).ConfigureAwait(false);
            @is.IndirectType = ptr.Type;
            return false;
        }
        else
        {
            k = await this.Level14Async(@is).ConfigureAwait(false);

            // lval++
            if (await frontEnd.MatchAsync("++").ConfigureAwait(false))
            {
                if (!k)
                {
                    ErrorUseCases.NeedLVal();
                    return false;
                }

                await this.StepAsync(PCode.rINC1, @is, PCode.rDEC1)
                    .ConfigureAwait(false);
                return false;
            }

            // lval--
            else if (await frontEnd.MatchAsync("--").ConfigureAwait(false))
            {
                if (!k)
                {
                    ErrorUseCases.NeedLVal();
                    return false;
                }

                await this.StepAsync(PCode.rDEC1, @is, PCode.rINC1)
                    .ConfigureAwait(false);
                return false;
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
    public async Task<bool> Level14Async(Expression @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        bool k;
        SymbolTableEntry? ptr;
        int? before, start;

        k = await this.PrimaryAsync(@is).ConfigureAwait(false);
        ptr = @is.SymbolTableEntry;
        await frontEnd.BlanksAsync().ConfigureAwait(false);
        if (storage.Ch is '[' or '(')
        {
            // allocate only if needed
            var is2 = new Expression(
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
                        if (k)
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
                    k = true;
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
                        if (k && @is.ConstantValue == 0)
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

                    k = false;
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
            return false;
        }

        return k;
    }

    /// <summary>
    /// Analyze primary term.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>Expression operand.</returns>
    public async Task<bool> PrimaryAsync(Expression @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        string? sName;
        bool k;

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
                    return false;
                }

                await backEnd.GenAsync(PCode.POINT1s, ptrLoc.Offset)
                    .ConfigureAwait(false);
                @is.SymbolTableEntry = ptrLoc;
                @is.IndirectType = ptrLoc.Type;
                if (ptrLoc.Ident == SymbolIdentity.Array)
                {
                    @is.AddressType = ptrLoc.Type;
                    return false;
                }

                if (ptrLoc.Ident == SymbolIdentity.Pointer)
                {
                    @is.IndirectType = SymbolType.UInt;
                    @is.AddressType = ptrLoc.Type;
                }

                return true;
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
                        return false;
                    }

                    if (ptrGlb.Ident == SymbolIdentity.Pointer)
                    {
                        @is.AddressType = ptrGlb.Type;
                    }

                    return true;
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

            return false;
        }

        if (!await this.ConstantAsync(@is).ConfigureAwait(false))
        {
            await this.ExpErrAsync().ConfigureAwait(false);
        }

        return false;
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
    /// <param name="oper2">Second operator, if any.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StepAsync(
        PCode oper, Expression @is, PCode? oper2)
    {
        await this.FetchAsync(@is).ConfigureAwait(false);
        await backEnd.GenAsync(
            oper,
            @is.AddressType.HasValue ? ((int)@is.AddressType.Value >> 2) : 1)
            .ConfigureAwait(false);
        await this.StoreAsync(@is).ConfigureAwait(false);
        if (oper2.HasValue)
        {
            var value = @is.AddressType.HasValue ?
                ((int)@is.AddressType.Value >> 2) : 1;
            await backEnd.GenAsync(oper2.Value, value).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Store result.
    /// </summary>
    /// <param name="is">Expression analysis for result.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StoreAsync(Expression @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        // putstk
        if (@is.IndirectType.HasValue)
        {
            if ((int)@is.IndirectType.Value >> 2 == 1)
            {
                await backEnd.GenAsync(PCode.PUTbp1, null)
                    .ConfigureAwait(false);
            }
            else
            {
                await backEnd.GenAsync(PCode.PUTwp1, null)
                    .ConfigureAwait(false);
            }
        }

        // putmem
        else
        {
            var ptr = @is.SymbolTableEntry ??
                throw new InvalidOperationException();
            if (ptr.Ident != SymbolIdentity.Pointer
                && (int)ptr.Type >> 2 == 1)
            {
                await backEnd.GenAsync(
                    PCode.PUTbm1, storage.SymTab.IndexOf(ptr))
                    .ConfigureAwait(false);
            }
            else
            {
                await backEnd.GenAsync(
                    PCode.PUTwm1, storage.SymTab.IndexOf(ptr))
                    .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Fetch operand.
    /// </summary>
    /// <param name="is">Analysis results.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task FetchAsync(Expression @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        SymbolTableEntry? ptr;

        ptr = @is.SymbolTableEntry ?? throw new InvalidOperationException();

        // indirect
        if (@is.IndirectType.HasValue)
        {
            if ((int)@is.IndirectType.Value >> 2 == Machine.Bpw)
            {
                await backEnd.GenAsync(PCode.GETw1p, null)
                    .ConfigureAwait(false);
            }
            else
            {
                if ((ptr.Type & SymbolType.Unsigned) != 0)
                {
                    await backEnd.GenAsync(PCode.GETb1pu, null)
                        .ConfigureAwait(false);
                }
                else
                {
                    await backEnd.GenAsync(PCode.GETb1p, null)
                        .ConfigureAwait(false);
                }
            }
        }

        // direct
        else
        {
            if (ptr.Ident == SymbolIdentity.Pointer
                || (int)ptr.Type >> 2 == Machine.Bpw)
            {
                await backEnd.GenAsync(
                    PCode.GETw1m, storage.SymTab.IndexOf(ptr))
                    .ConfigureAwait(false);
            }
            else
            {
                if ((ptr.Type & SymbolType.Unsigned) != 0)
                {
                    await backEnd.GenAsync(
                        PCode.GETb1mu, storage.SymTab.IndexOf(ptr))
                        .ConfigureAwait(false);
                }
                else
                {
                    await backEnd.GenAsync(
                        PCode.GETb1m, storage.SymTab.IndexOf(ptr))
                        .ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Parses constant.
    /// </summary>
    /// <param name="is">Expression analysis.</param>
    /// <returns>A value indicating whether expression is constant.</returns>
    public async Task<bool> ConstantAsync(Expression @is)
    {
        ArgumentNullException.ThrowIfNull(@is);

        int? offset;

        (@is.ConstantType, @is.ConstantValue) = await this.NumberAsync(
            @is.ConstantValue).ConfigureAwait(false);
        if (@is.ConstantType.HasValue)
        {
            await backEnd.GenAsync(PCode.GETw1n, @is.ConstantValue)
                .ConfigureAwait(false);
        }
        else
        {
            (@is.ConstantType, @is.ConstantValue) = await this.ChrConAsync(
                @is.ConstantValue).ConfigureAwait(false);
            if (@is.ConstantType.HasValue)
            {
                await backEnd.GenAsync(PCode.GETw1n, @is.ConstantValue)
                    .ConfigureAwait(false);
            }
            else
            {
                offset = await this.StringAsync().ConfigureAwait(false);
                if (offset.HasValue)
                {
                    await backEnd.GenAsync(PCode.POINT1l, offset)
                        .ConfigureAwait(false);
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Parses number constant.
    /// </summary>
    /// <param name="value">Current constant value.</param>
    /// <returns>Tuple of constant type, if any, and constant.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code", Justification = "not quite dead")]
    public async Task<(SymbolType? Type, int Value)> NumberAsync(int value)
    {
        ushort k;
        bool minus;

        k = 0;
        minus = false;
        while (true)
        {
            if (await frontEnd.MatchAsync("+").ConfigureAwait(false))
            {
                // already parsed
            }
            else if (await frontEnd.MatchAsync("-").ConfigureAwait(false))
            {
                minus = true;
            }
            else
            {
                break;
            }
        }

        if (!storage.Ch.HasValue || !char.IsDigit(storage.Ch.Value))
        {
            return (null, value);
        }

        if (storage.Ch == '0')
        {
            while (storage.Ch == '0')
            {
                _ = await frontEnd.InByteAsync().ConfigureAwait(false);
            }

            if (storage.Ch is char ch && char.ToUpperInvariant(ch) == 'X')
            {
                _ = await frontEnd.InByteAsync().ConfigureAwait(false);
                while (storage.Ch is char chex && char.IsAsciiHexDigit(chex))
                {
                    if (char.IsDigit(chex) && await frontEnd.InByteAsync()
                            .ConfigureAwait(false) is char chexIn)
                    {
                        k = (ushort)((k * 16) + chexIn - '0');
                    }
                    else
                    {
                        var chx = await frontEnd.InByteAsync()
                            .ConfigureAwait(false) ??
                            throw new InvalidOperationException();
                        k = (ushort)((k * 16) + 10 + (char
                            .ToUpperInvariant(chx) - 'A'));
                    }
                }
            }
            else
            {
                while (storage.Ch is >= '0' and <= '7' &&
                    await frontEnd.InByteAsync().ConfigureAwait(false)
                    is char chin)
                {
                    k = (ushort)((k * 8) + chin - '0');
                }
            }
        }
        else
        {
            while (storage.Ch is char ch && char.IsDigit(ch) &&
                await frontEnd.InByteAsync().ConfigureAwait(false)
                is char chin)
            {
                k = (ushort)((k * 10) + chin - '0');
            }
        }

        if (minus)
        {
            value = (short)-k;
            return (SymbolType.Int, value);
        }

        value = k;
        if (value > 0x7FFF)
        {
            return (SymbolType.UInt, value);
        }
        else
        {
            return (SymbolType.Int, value);
        }
    }

    /// <summary>
    /// Parses character constant.
    /// </summary>
    /// <param name="value">Current constant value.</param>
    /// <returns>Tuple of constant type, if any, and constant.</returns>
    public async Task<(SymbolType? Type, int Value)> ChrConAsync(int value)
    {
        short k;

        k = 0;
        if (!await frontEnd.MatchAsync("'").ConfigureAwait(false))
        {
            return (null, value);
        }

        while (storage.Ch != '\'')
        {
            k = (short)((k << 8) + ((this.LitChar() ?? 0) & 255));
        }

        _ = frontEnd.Gch();
        value = k;
        return (SymbolType.Int, value);
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
    public short? LitChar()
    {
        short i, oct;

        if (storage.Ch != '\\' || !storage.NCh.HasValue)
        {
            return (short?)frontEnd.Gch();
        }

        _ = frontEnd.Gch();
        switch (storage.Ch)
        {
            case 'n':
                _ = frontEnd.Gch();
                return (short)'\n';
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
            oct = (short)((oct << 3) + (frontEnd.Gch() ?? 0) - '0');
        }

        return i == 2 ? (short?)frontEnd.Gch() : oct;
    }

    /// <summary>
    /// True if <paramref name="is2"/>'s operand should be doubled.
    /// </summary>
    private static int Double(
        PCode? oper, Expression is1, Expression is2)
    {
        ArgumentNullException.ThrowIfNull(is1);
        ArgumentNullException.ThrowIfNull(is2);

        return ((oper == PCode.ADD12 || oper == PCode.SUB12)
            && is1.AddressType.HasValue
            && (int)is1.AddressType >> 2 == Machine.Bpw
            && !is2.AddressType.HasValue) ? 1 : 0;
    }

    /// <summary>
    /// Unsigned operand?.
    /// </summary>
    private static bool NoSign(Expression @is)
    {
        return @is.AddressType.HasValue
            || @is.ConstantType == SymbolType.UInt
            || ((@is.SymbolTableEntry is SymbolTableEntry ptr)
            && (ptr.Type & SymbolType.Unsigned) != 0);
    }

    /// <summary>
    /// Calculate signed constant result.
    /// </summary>
    private static int Calc(int left, PCode? oper, int right)
    {
#pragma warning disable IDE0010 // Add missing cases
        switch (oper)
        {
            case PCode.ADD12:
                return left + right;
            case PCode.SUB12:
                return left - right;
            case PCode.MUL12:
                return left * right;
            case PCode.DIV12:
                return left / right;
            case PCode.MOD12:
                return left % right;
            case PCode.EQ12:
                return left == right ? 1 : 0;
            case PCode.NE12:
                return left != right ? 1 : 0;
            case PCode.LE12:
                return left <= right ? 1 : 0;
            case PCode.GE12:
                return left >= right ? 1 : 0;
            case PCode.LT12:
                return left < right ? 1 : 0;
            case PCode.GT12:
                return left > right ? 1 : 0;
            case PCode.AND12:
                return left & right;
            case PCode.OR12:
                return left | right;
            case PCode.XOR12:
                return left ^ right;
            case PCode.ASR12:
                return left >> right;
            case PCode.ASL12:
                return left << right;
        }
#pragma warning restore IDE0010 // Add missing cases

        return Calc2((uint)left, oper, (uint)right);
    }

    /// <summary>
    /// Calculate unsigned constant result.
    /// </summary>
    private static int Calc2(uint left, PCode? oper, uint right)
    {
#pragma warning disable IDE0010 // Add missing cases
        switch (oper)
        {
            case PCode.MUL12u:
                return (int)(left * right);
            case PCode.DIV12u:
                return (int)(left / right);
            case PCode.MOD12u:
                return (int)(left % right);
            case PCode.LE12:
                return left <= right ? 1 : 0;
            case PCode.GE12:
                return left >= right ? 1 : 0;
            case PCode.LT12:
                return left < right ? 1 : 0;
            case PCode.GT12:
                return left > right ? 1 : 0;
        }
#pragma warning restore IDE0010 // Add missing cases

        return 0;
    }

    /// <summary>
    /// Skim over terms adjoining || and &amp;&amp; operators.
    /// </summary>
    private async Task<bool> SkimAsync(
        IList<string> ops,
        PCode tCode,
        int dropVal,
        int endVal,
        Func<Expression, Task<bool>> levelAsync,
        Expression @is)
    {
        bool k;
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
                return false;
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
    private async Task DropOutAsync(
        bool k, PCode tCode, int exitL, Expression @is)
    {
        if (k)
        {
            await this.FetchAsync(@is).ConfigureAwait(false);
        }
        else if (@is.ConstantType.HasValue)
        {
            await backEnd.GenAsync(PCode.GETw1n, @is.ConstantValue)
                .ConfigureAwait(false);
        }

        // jumps on false
        await backEnd.GenAsync(tCode, exitL).ConfigureAwait(false);
    }

    /// <summary>
    /// Drop to a lower level.
    /// </summary>
    private async Task<bool> DownAsync(
        IList<string> ops,
        int opOff,
        Func<Expression, Task<bool>> levelAsync,
        Expression @is)
    {
        bool k;

        k = await this.Down1Async(levelAsync, @is).ConfigureAwait(false);
        if (!await frontEnd.NextOpAsync(ops).ConfigureAwait(false))
        {
            return k;
        }

        if (k)
        {
            await this.FetchAsync(@is).ConfigureAwait(false);
        }

        while (true)
        {
            if (await frontEnd.NextOpAsync(ops).ConfigureAwait(false))
            {
                // allocate only if needed
                var is2 = new Expression(
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
                return false;
            }
        }
    }

    /// <summary>
    /// Unary drop to a lower level.
    /// </summary>
    private async Task<bool> Down1Async(
        Func<Expression, Task<bool>> levelAsync, Expression @is)
    {
        bool k;
        int? before;

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
    private async Task Down2Async(
        PCode? oper,
        PCode? oper2,
        Func<Expression, Task<bool>> levelAsync,
        Expression @is,
        Expression is2)
    {
        int? before, start;

        (before, start) = backEnd.SetStage();
        @is.StageIndex = null; // not "... op 0" syntax

        // constant op unknown
        if (@is.ConstantType.HasValue)
        {
            if (await this.Down1Async(levelAsync, is2).ConfigureAwait(false))
            {
                await this.FetchAsync(is2).ConfigureAwait(false);
            }

            if (@is.ConstantValue == 0)
            {
                @is.StageIndex = storage.SNext;
            }

            await backEnd.GenAsync(
                PCode.GETw2n,
                @is.ConstantValue << Double(oper, is2, @is))
                .ConfigureAwait(false);
        }

        // variable op unknown
        else
        {
            // at start of the buffer
            await backEnd.GenAsync(PCode.PUSH1, null).ConfigureAwait(false);
            if (await this.Down1Async(levelAsync, is2).ConfigureAwait(false))
            {
                await this.FetchAsync(is2).ConfigureAwait(false);
            }

            // variable op constant
            if (is2.ConstantType.HasValue)
            {
                if (is2.ConstantValue == 0)
                {
                    @is.StageIndex = start;
                }

                storage.Csp += Machine.Bpw; // adjust stack and

                // discard the PUSH
                await backEnd.ClearStageAsync(before, null)
                    .ConfigureAwait(false);

                // commutative
                if (oper == PCode.ADD12)
                {
                    await backEnd.GenAsync(
                        PCode.GETw2n,
                        is2.ConstantValue << Double(oper, @is, is2))
                        .ConfigureAwait(false);
                }

                // non-commutative
                else
                {
                    await backEnd.GenAsync(PCode.MOVE21, null)
                        .ConfigureAwait(false);
                    await backEnd.GenAsync(
                        PCode.GETw1n,
                        is2.ConstantValue << Double(oper, @is, is2))
                        .ConfigureAwait(false);
                }
            }

            // variable op variable
            else
            {
                await backEnd.GenAsync(PCode.POP2, null).ConfigureAwait(false);
                if (Double(oper, @is, is2) != 0)
                {
                    await backEnd.GenAsync(PCode.DBL1, null)
                        .ConfigureAwait(false);
                }

                if (Double(oper, is2, @is) != 0)
                {
                    await backEnd.GenAsync(PCode.DBL2, null)
                        .ConfigureAwait(false);
                }
            }
        }

        if (oper.HasValue)
        {
            if (NoSign(@is) || NoSign(is2))
            {
                oper = oper2;
            }

            @is.ConstantType &= is2.ConstantType;

            // constant result
            if (@is.ConstantType.HasValue)
            {
                @is.ConstantValue = Calc(
                    @is.ConstantValue, oper, is2.ConstantValue);
                await backEnd.ClearStageAsync(before, null)
                    .ConfigureAwait(false);
                if (is2.ConstantType == SymbolType.UInt)
                {
                    @is.ConstantType = SymbolType.UInt;
                }
            }

            // variable result
            else
            {
                await backEnd.GenAsync(
                    oper ?? throw new InvalidOperationException(), null)
                    .ConfigureAwait(false);

                // difference of two word addresses
                if (oper == PCode.SUB12
                    && @is.AddressType.HasValue
                    && (int)@is.AddressType.Value >> 2 == Machine.Bpw
                    && is2.AddressType.HasValue
                    && (int)is2.AddressType.Value >> 2 == Machine.Bpw)
                {
                    await backEnd.GenAsync(PCode.SWAP12, null)
                        .ConfigureAwait(false);
                    await backEnd.GenAsync(PCode.GETw1n, 1)
                        .ConfigureAwait(false);
                    await backEnd.GenAsync(PCode.ASR12, null) // div by 2
                        .ConfigureAwait(false);
                }

                @is.HighestBinaryOp = oper; // identify the operator
            }

            if (oper is PCode.SUB12 or PCode.ADD12)
            {
                // addr +/- addr
                if (@is.AddressType.HasValue && is2.AddressType.HasValue)
                {
                    @is.AddressType = null;
                }

                // value +/- addr
                else if (is2.AddressType.HasValue)
                {
                    @is.SymbolTableEntry = is2.SymbolTableEntry;
                    @is.IndirectType = is2.IndirectType;
                    @is.AddressType = is2.AddressType;
                }
            }

            if (@is.SymbolTableEntry is null
                || ((is2.SymbolTableEntry is SymbolTableEntry ptr)
                && (ptr.Type & SymbolType.Unsigned) != 0))
            {
                @is.SymbolTableEntry = is2.SymbolTableEntry;
            }
        }
    }
}
