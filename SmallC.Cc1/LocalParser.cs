// <copyright file="LocalParser.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc1;

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc4;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// 2nd level parser.
/// </summary>
public class LocalParser(
    FrontEnd frontEnd,
    BackEnd backEnd,
    Storage storage)
{
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
    public Task DeclLocAsync(SymbolType type)
    {
        _ = storage;
        _ = type;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parse compound statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task CompoundAsync()
    {
        _ = storage;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parse if statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task DoIfAsync()
    {
        _ = storage;
        _ = storage;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parse while statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task DoWhileAsync()
    {
        _ = storage;
        _ = storage;
        _ = storage;
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parse do statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task DoDoAsync()
    {
        _ = storage;
        _ = storage;
        _ = storage;
        _ = storage;
        throw new NotImplementedException();
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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<bool> DoLabelAsync()
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parse return statement.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task DoReturnAsync()
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
        throw new NotImplementedException();
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
    public Task DoExprAsync(bool use)
    {
        _ = storage;
        _ = use;
        throw new NotImplementedException();
    }
}
