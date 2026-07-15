// <copyright file="LocalParser.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc1;

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc3;
using SmallC.Cc4;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// 2nd level parser.
/// </summary>
public class LocalParser(
    SymbolTableUseCases symbolTable,
    UtilityUseCases utility,
    WhileQueueUseCases whileQueue,
    FrontEnd frontEnd,
    Analyzer analyzer,
    BackEnd backEnd,
    Storage storage)
{
    /// <summary>
    /// Get required array size.
    /// </summary>
    /// <returns>Required array size.</returns>
    public async Task<int> NeedSubAsync()
    {
        int val;

        if (await frontEnd.MatchAsync("]").ConfigureAwait(false))
        {
            return 0; // null size
        }

        val = analyzer.ConstExpr() ?? 1;
        if (val < 0)
        {
            throw new InvalidCastException("negative size illegal");
        }

        // force single dimension
        await frontEnd.NeedAsync("]").ConfigureAwait(false);
        return val; // and return size
    }

    /// <summary>
    /// Parse next local or argument declaration.
    /// </summary>
    /// <param name="type">Type of symbol being declared.</param>
    /// <param name="aid">Automatic identity.</param>
    /// <returns>
    /// Tuple of declared symbol name (if valid), identity, and size.
    /// </returns>
    public async Task<(string? N, SymbolIdentity Id, int Sz)>
        DeclAsync(SymbolType type, SymbolIdentity aid)
    {
        string? n;
        SymbolIdentity id;
        int sz;

        var p = await frontEnd.MatchAsync("(").ConfigureAwait(false);
        if (await frontEnd.MatchAsync("*").ConfigureAwait(false))
        {
            id = SymbolIdentity.Pointer;
            sz = Machine.Bpw;
        }
        else
        {
            id = SymbolIdentity.Variable;
            sz = (int)type >> 2;
        }

        storage.SsName = await frontEnd.SymNameAsync().ConfigureAwait(false);
        n = storage.SsName;
        if (n is null)
        {
            ErrorUseCases.IllName();
        }

        if (p && await frontEnd.MatchAsync(")").ConfigureAwait(false))
        {
            // already parsed
        }

        if (await frontEnd.MatchAsync("(").ConfigureAwait(false))
        {
            if (!p || id != SymbolIdentity.Pointer)
            {
                throw new InvalidOperationException("try (*...)()");
            }

            await frontEnd.NeedAsync(")").ConfigureAwait(false);
        }
        else if (id == SymbolIdentity.Variable
            && await frontEnd.MatchAsync("[").ConfigureAwait(false))
        {
            id = aid;
            sz *= await this.NeedSubAsync().ConfigureAwait(false);
            if (sz == 0)
            {
                if (aid == SymbolIdentity.Array)
                {
                    throw new InvalidOperationException("need array size");
                }

                sz = Machine.Bpw;
            }
        }

        return (n, id, sz);
    }

    /// <summary>
    /// Parse statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<StatementType> StatementAsync()
    {
        if (storage.Ch is null && storage.Eof)
        {
            return StatementType.None;
        }
        else if (await frontEnd.AMatchAsync("char", 4).ConfigureAwait(false))
        {
            await this.DeclLocAsync(SymbolType.Chr).ConfigureAwait(false);
            await frontEnd.NsAsync().ConfigureAwait(false);
        }
        else if (await frontEnd.AMatchAsync("int", 3).ConfigureAwait(false))
        {
            await this.DeclLocAsync(SymbolType.Int).ConfigureAwait(false);
            await frontEnd.NsAsync().ConfigureAwait(false);
        }
        else if (await frontEnd.AMatchAsync("unsigned", 8)
            .ConfigureAwait(false))
        {
            if (await frontEnd.AMatchAsync("char", 4).ConfigureAwait(false))
            {
                await this.DeclLocAsync(SymbolType.UChr)
                    .ConfigureAwait(false);
                await frontEnd.NsAsync().ConfigureAwait(false);
            }
            else
            {
                _ = await frontEnd.AMatchAsync("int", 3)
                    .ConfigureAwait(false);
                await this.DeclLocAsync(SymbolType.UInt)
                    .ConfigureAwait(false);
                await frontEnd.NsAsync().ConfigureAwait(false);
            }
        }
        else
        {
            if (storage.Declared >= 0)
            {
                if (storage.NCmp > 1)
                {
                    storage.NoGo = storage.Declared != 0; // disable goto
                }

                await backEnd.GenAsync(
                    PCode.ADDSP, storage.Csp - storage.Declared)
                    .ConfigureAwait(false);
                storage.Declared = -1;
            }

            if (await frontEnd.MatchAsync("{").ConfigureAwait(false))
            {
                await this.CompoundAsync().ConfigureAwait(false);
            }
            else if (await frontEnd.AMatchAsync("if", 2).ConfigureAwait(false))
            {
                await this.DoIfAsync().ConfigureAwait(false);
                storage.LastSt = StatementType.If;
            }
            else if (await frontEnd.AMatchAsync("while", 5)
                .ConfigureAwait(false))
            {
                await this.DoWhileAsync().ConfigureAwait(false);
                storage.LastSt = StatementType.While;
            }
            else if (await frontEnd.AMatchAsync("do", 2).ConfigureAwait(false))
            {
                await this.DoDoAsync().ConfigureAwait(false);
                storage.LastSt = StatementType.Do;
            }
            else if (await frontEnd.AMatchAsync("for", 3).ConfigureAwait(false))
            {
                await this.DoForAsync().ConfigureAwait(false);
                storage.LastSt = StatementType.For;
            }
            else if (await frontEnd.AMatchAsync("switch", 6)
                .ConfigureAwait(false))
            {
                await this.DoSwitchAsync().ConfigureAwait(false);
                storage.LastSt = StatementType.Switch;
            }
            else if (await frontEnd.AMatchAsync("case", 4)
                .ConfigureAwait(false))
            {
                await this.DoCaseAsync().ConfigureAwait(false);
                storage.LastSt = StatementType.Case;
            }
            else if (await frontEnd.AMatchAsync("default", 7)
                .ConfigureAwait(false))
            {
                await this.DoDefaultAsync().ConfigureAwait(false);
                storage.LastSt = StatementType.Def;
            }
            else if (await frontEnd.AMatchAsync("goto", 4)
                .ConfigureAwait(false))
            {
                await this.DoGotoAsync().ConfigureAwait(false);
                storage.LastSt = StatementType.Goto;
            }
            else if (await this.DoLabelAsync().ConfigureAwait(false))
            {
                storage.LastSt = StatementType.Label;
            }
            else if (await frontEnd.AMatchAsync("return", 6)
                .ConfigureAwait(false))
            {
                await this.DoReturnAsync().ConfigureAwait(false);
                await frontEnd.NsAsync().ConfigureAwait(false);
                storage.LastSt = StatementType.Return;
            }
            else if (await frontEnd.AMatchAsync("break", 5)
                .ConfigureAwait(false))
            {
                await this.DoBreakAsync().ConfigureAwait(false);
                await frontEnd.NsAsync().ConfigureAwait(false);
                storage.LastSt = StatementType.Break;
            }
            else if (await frontEnd.AMatchAsync("continue", 8)
                .ConfigureAwait(false))
            {
                await this.DoContAsync().ConfigureAwait(false);
                await frontEnd.NsAsync().ConfigureAwait(false);
                storage.LastSt = StatementType.Cont;
            }
            else if (await frontEnd.MatchAsync(";").ConfigureAwait(false))
            {
                storage.ErrFlag = false;
            }
            else if (await frontEnd.MatchAsync("#asm").ConfigureAwait(false))
            {
                await this.DoAsmAsync().ConfigureAwait(false);
                storage.LastSt = StatementType.Asm;
            }
            else
            {
                await this.DoExprAsync(false).ConfigureAwait(false);
                await frontEnd.NsAsync().ConfigureAwait(false);
                storage.LastSt = StatementType.Expr;
            }
        }

        return storage.LastSt;
    }

    /// <summary>
    /// Declare local variables.
    /// </summary>
    /// <param name="type">Type of locals to declare.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DeclLocAsync(SymbolType type)
    {
        SymbolIdentity id;
        int sz;

        if (storage.SwActive)
        {
            throw new InvalidOperationException("not allowed in switch");
        }

        if (storage.NoLoc)
        {
            throw new InvalidOperationException("not allowed with goto");
        }

        if (storage.Declared < 0)
        {
            throw new InvalidOperationException("must declare first in block");
        }

        while (true)
        {
            if (await frontEnd.EndStAsync().ConfigureAwait(false))
            {
                return;
            }

            (_, id, sz) = await this.DeclAsync(type, SymbolIdentity.Array)
                .ConfigureAwait(false);
            storage.Declared += sz;
            _ = symbolTable.AddSym(
                storage.SsName ?? throw new InvalidOperationException(),
                id,
                type,
                sz,
                storage.Csp - storage.Declared,
                storage.SymTab.Locals,
                SymbolClass.Automatic);
            if (!await frontEnd.MatchAsync(",").ConfigureAwait(false))
            {
                return;
            }
        }
    }

    /// <summary>
    /// Parse compound statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CompoundAsync()
    {
        int saveCsp;
        int? saveLoc;

        saveCsp = storage.Csp;
        saveLoc = storage.LocPtr;
        storage.Declared = 0; // may now declare local variables
        storage.NCmp++; // new level open
        while (!await frontEnd.MatchAsync("}").ConfigureAwait(false))
        {
            if (storage.Eof)
            {
                throw new InvalidOperationException("no final }");
            }
            else
            {
                _ = await this.StatementAsync().ConfigureAwait(false); // do one
            }
        }

        storage.NCmp--; // close current level
        if (storage.NCmp != 0
            && storage.LastSt != StatementType.Return
            && storage.LastSt != StatementType.Goto)
        {
            // delete local variable space
            await backEnd.GenAsync(PCode.ADDSP, saveCsp).ConfigureAwait(false);
        }

        var cptr = saveLoc; // retain labels
        while (cptr < storage.LocPtr && saveLoc.HasValue)
        {
            var cptr2 = symbolTable.NextSym(cptr.Value);
            if (storage.SymTab.Locals[cptr.Value].Ident == SymbolIdentity.Label)
            {
                storage.SymTab.Locals[saveLoc.Value] =
                    storage.SymTab.Locals[cptr.Value];
                saveLoc = cptr2;
                cptr = cptr2;
            }
            else
            {
                cptr = cptr2;
            }
        }

        while (storage.LocPtr - 1 > saveLoc)
        {
            // delete local variables
            storage.SymTab.Locals.RemoveAt(storage.LocPtr - 1);
        }

        storage.Declared = -1; // may not declare variables
    }

    /// <summary>
    /// Parse if statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DoIfAsync()
    {
        int flab1, flab2;

        flab1 = utility.GetLabel();

        // get expr, and branch false
        await analyzer.TestAsync(flab1, true).ConfigureAwait(false);

        // if true, do a statement
        _ = await this.StatementAsync().ConfigureAwait(false);

        // if...else ?
        if (!await frontEnd.AMatchAsync("else", 4).ConfigureAwait(false))
        {
            // simple "if"... print false label
            await backEnd.GenAsync(PCode.LABm, flab1).ConfigureAwait(false);
            return; // and exit
        }

        flab2 = utility.GetLabel();
        if (storage.LastSt is not StatementType.Return
            and not StatementType.Goto)
        {
            await backEnd.GenAsync(PCode.JMPm, flab2).ConfigureAwait(false);
        }

        // print false label
        await backEnd.GenAsync(PCode.LABm, flab1).ConfigureAwait(false);

        // and do "else" clause
        _ = await this.StatementAsync().ConfigureAwait(false);

        // print true label
        await backEnd.GenAsync(PCode.LABm, flab2).ConfigureAwait(false);
    }

    /// <summary>
    /// Parse while statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DoWhileAsync()
    {
        var wq = whileQueue.AddWhile(); // add entry to queue for "break"

        // loop label
        await backEnd.GenAsync(PCode.LABm, wq.LoopLabel).ConfigureAwait(false);

        // see if true
        await analyzer.TestAsync(wq.ExitLabel, true).ConfigureAwait(false);

        // if so, do a statement
        _ = await this.StatementAsync().ConfigureAwait(false);

        // loop to label
        await backEnd.GenAsync(PCode.JMPm, wq.LoopLabel).ConfigureAwait(false);

        // exit label
        await backEnd.GenAsync(PCode.LABm, wq.ExitLabel).ConfigureAwait(false);

        whileQueue.DelWhile(); // delete queue entry
    }

    /// <summary>
    /// Parse do statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DoDoAsync()
    {
        var wq = whileQueue.AddWhile();
        await backEnd.GenAsync(PCode.LABm, wq.LoopLabel).ConfigureAwait(false);
        _ = await this.StatementAsync().ConfigureAwait(false);
        await frontEnd.NeedAsync("while").ConfigureAwait(false);
        await analyzer.TestAsync(wq.ExitLabel, true).ConfigureAwait(false);
        await backEnd.GenAsync(PCode.JMPm, wq.LoopLabel).ConfigureAwait(false);
        await backEnd.GenAsync(PCode.LABm, wq.ExitLabel).ConfigureAwait(false);
        whileQueue.DelWhile();
        await frontEnd.NsAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Parse for statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task DoForAsync()
    {
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parse switch statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task DoSwitchAsync()
    {
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parse case statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task DoCaseAsync()
    {
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parse default statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task DoDefaultAsync()
    {
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parse goto statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task DoGotoAsync()
    {
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parse label statement.
    /// </summary>
    /// <returns>A value indicating whether a label was parsed.</returns>
    public async Task<bool> DoLabelAsync()
    {
        int saveLPtr;

        await frontEnd.BlanksAsync().ConfigureAwait(false);
        saveLPtr = storage.LPtr;
        storage.SsName = await frontEnd.SymNameAsync().ConfigureAwait(false);
        if (storage.SsName is not null)
        {
            if (frontEnd.Gch() == ':')
            {
                throw new NotImplementedException();
            }
            else
            {
                frontEnd.Bump(saveLPtr - storage.LPtr);
            }
        }

        return false;
    }

    /// <summary>
    /// Parse return statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DoReturnAsync()
    {
        int saveCsp;

        if (!await frontEnd.EndStAsync().ConfigureAwait(false))
        {
            await this.DoExprAsync(true).ConfigureAwait(false);
        }

        saveCsp = storage.Csp;
        await backEnd.GenAsync(PCode.RETURN, null).ConfigureAwait(false);
        storage.Csp = saveCsp;
    }

    /// <summary>
    /// Parse break statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task DoBreakAsync()
    {
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parse continue statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task DoContAsync()
    {
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parse assembly code.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DoAsmAsync()
    {
        storage.CCode = false; // mark mode as "asm"
        while (true)
        {
            await frontEnd.InLineAsync().ConfigureAwait(false);
            if (await frontEnd.MatchAsync("#endasm").ConfigureAwait(false))
            {
                break;
            }

            if (storage.Eof)
            {
                break;
            }

            await storage.Output.WriteAsync(storage.Line).ConfigureAwait(false);
        }

        frontEnd.Kill();
        storage.CCode = true;
    }

    /// <summary>
    /// Parse expression.
    /// </summary>
    /// <param name="use">
    /// A value indicating whether to use the expression.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DoExprAsync(bool use)
    {
        _ = storage;
        _ = use;

        await frontEnd.NeedAsync("(").ConfigureAwait(false);

        var level = 1;

        while (level != 0)
        {
            switch (storage.Ch)
            {
                case '(':
                    level++;
                    _ = frontEnd.Gch();
                    break;
                case ')':
                    level--;
                    _ = frontEnd.Gch();
                    break;
                case null:
                    await frontEnd.PreprocessAsync().ConfigureAwait(false);
                    break;
                default:
                    _ = frontEnd.Gch();
                    break;
            }
        }
    }
}
