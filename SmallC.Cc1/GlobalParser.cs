// <copyright file="GlobalParser.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc1;

using SmallC.Cc;
using SmallC.Cc2;
using SmallC.Cc3;
using SmallC.Cc4;
using System.Text;
using static SmallC.Cc.SymbolTableEntry;

/// <summary>
/// High level parser.
/// </summary>
public class GlobalParser(
    SymbolTableUseCases symbolTable,
    UtilityUseCases utility,
    FrontEnd frontEnd,
    LocalParser localParser,
    Analyzer analyzer,
    BackEnd backEnd,
    Storage storage)
{
    /// <summary>
    /// Process all input text.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// At this level, only static declarations,
    ///      defines, includes and function
    ///      definitions are legal...
    /// </remarks>
    public async Task ParseAsync()
    {
        while (!storage.Eof)
        {
            if (await frontEnd.AMatchAsync("extern", 6).ConfigureAwait(false))
            {
                _ = await this.DoDeclareAsync(SymbolClass.External)
                    .ConfigureAwait(false);
            }
            else if (await this.DoDeclareAsync(SymbolClass.Static)
                .ConfigureAwait(false))
            {
                // Already parsed
            }
            else if (await frontEnd.MatchAsync("#asm").ConfigureAwait(false))
            {
                await localParser.DoAsmAsync().ConfigureAwait(false);
            }
            else if (await frontEnd.MatchAsync("#include")
                .ConfigureAwait(false))
            {
                await this.DoIncludeAsync().ConfigureAwait(false);
            }
            else if (await frontEnd.MatchAsync("#define").ConfigureAwait(false))
            {
                await this.DoDefineAsync().ConfigureAwait(false);
            }
            else
            {
                await this.DoFunctionAsync().ConfigureAwait(false);
            }

            // force eof if pending
            await frontEnd.BlanksAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Test for global declarations.
    /// </summary>
    private async Task<bool> DoDeclareAsync(SymbolClass @class)
    {
        if (await frontEnd.AMatchAsync("char", 4).ConfigureAwait(false))
        {
            await this.DeclGlbAsync(SymbolType.Chr, @class)
                .ConfigureAwait(false);
        }
        else if (await frontEnd.AMatchAsync("unsigned", 8)
            .ConfigureAwait(false))
        {
            if (await frontEnd.AMatchAsync("char", 4).ConfigureAwait(false))
            {
                await this.DeclGlbAsync(SymbolType.UChr, @class)
                    .ConfigureAwait(false);
            }
            else
            {
                _ = await frontEnd.AMatchAsync("int", 3).ConfigureAwait(false);
                await this.DeclGlbAsync(SymbolType.UInt, @class)
                    .ConfigureAwait(false);
            }
        }
        else if (await frontEnd.AMatchAsync("int", 3).ConfigureAwait(false)
            || @class == SymbolClass.External)
        {
            await this.DeclGlbAsync(SymbolType.Int, @class)
                .ConfigureAwait(false);
        }
        else
        {
            return false;
        }

        await frontEnd.NsAsync().ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Declare a static variable.
    /// </summary>
    private async Task DeclGlbAsync(SymbolType type, SymbolClass @class)
    {
        SymbolIdentity id;
        int dim;

        while (true)
        {
            if (await frontEnd.EndStAsync().ConfigureAwait(false))
            {
                return; // do line
            }

            if (await frontEnd.MatchAsync("*").ConfigureAwait(false))
            {
                id = SymbolIdentity.Pointer;
                dim = 0;
            }
            else
            {
                id = SymbolIdentity.Variable;
                dim = 1;
            }

            storage.SsName = await frontEnd.SymNameAsync()
                .ConfigureAwait(false);
            if (storage.SsName is null)
            {
                ErrorUseCases.IllName();
                throw new InvalidOperationException();
            }

            if (symbolTable.FindGlb(storage.SsName) is not null)
            {
                ErrorUseCases.MultiDef(storage.SsName);
            }

            if (id == SymbolIdentity.Variable)
            {
                if (await frontEnd.MatchAsync("(").ConfigureAwait(false))
                {
                    id = SymbolIdentity.Function;
                    await frontEnd.NeedAsync(")").ConfigureAwait(false);
                }
                else if (await frontEnd.MatchAsync("[").ConfigureAwait(false))
                {
                    id = SymbolIdentity.Array;
                    dim = await localParser.NeedSubAsync()
                        .ConfigureAwait(false);
                }
            }

            if (@class == SymbolClass.External)
            {
                await backEnd.ExternalAsync(storage.SsName, (int)type >> 2, id)
                    .ConfigureAwait(false);
            }
            else if (id != SymbolIdentity.Function)
            {
                await this.InitialsAsync((int)type >> 2, id, dim)
                    .ConfigureAwait(false);
            }

            _ = id == SymbolIdentity.Pointer
                ? symbolTable.AddSym(
                    storage.SsName,
                    id,
                    type,
                    Machine.Bpw,
                    0,
                    storage.SymTab.Globals,
                    @class)
                : symbolTable.AddSym(
                    storage.SsName,
                    id,
                    type,
                    dim * ((int)type >> 2),
                    0,
                    storage.SymTab.Globals,
                    @class);

            if (!await frontEnd.MatchAsync(",").ConfigureAwait(false))
            {
                return;
            }
        }
    }

    /// <summary>
    /// Initialize global objects.
    /// </summary>
    private async Task InitialsAsync(int size, SymbolIdentity ident, int dim)
    {
        int savedDim;

        storage.LitQ.Clear();
        if (dim == 0)
        {
            dim = -1; // *... or ...[]
        }

        savedDim = dim;
        await backEnd.PublicAsync(ident).ConfigureAwait(false);
        if (await frontEnd.MatchAsync("=").ConfigureAwait(false))
        {
            if (await frontEnd.MatchAsync("{").ConfigureAwait(false))
            {
                while (dim != 0)
                {
                    dim = await this.InitAsync(size, ident, dim)
                        .ConfigureAwait(false);
                    if (!await frontEnd.MatchAsync(",").ConfigureAwait(false))
                    {
                        break;
                    }
                }

                await frontEnd.NeedAsync("}").ConfigureAwait(false);
            }
            else
            {
                dim = await this.InitAsync(size, ident, dim)
                    .ConfigureAwait(false);
            }
        }

        if (savedDim == -1 && dim == -1)
        {
            if (ident == SymbolIdentity.Array)
            {
                throw new InvalidOperationException("need array size");
            }

            size = Machine.Bpw;
            analyzer.StowLit(0, size);
        }

        await backEnd.DumpLitsAsync(size).ConfigureAwait(false);

        // only if dim > 0
        await backEnd.DumpZeroAsync(size, dim).ConfigureAwait(false);
    }

    /// <summary>
    /// Evaluate one initializer.
    /// </summary>
    private async Task<int> InitAsync(int size, SymbolIdentity ident, int dim)
    {
        if (await analyzer.StringAsync().ConfigureAwait(false) is int offset)
        {
            if (ident == SymbolIdentity.Variable || size != 1)
            {
                throw new InvalidOperationException(
                    "must assign to char pointer or char array");
            }

            dim -= storage.LitPtr - offset;
            if (ident == SymbolIdentity.Pointer)
            {
                await backEnd.PointAsync().ConfigureAwait(false);
            }
        }
        else if (await analyzer.ConstExprAsync().ConfigureAwait(false)
            is int value)
        {
            if (ident == SymbolIdentity.Pointer)
            {
                throw new InvalidOperationException("cannot assign to pointer");
            }

            analyzer.StowLit(value, size);
            dim -= 1;
        }

        return dim;
    }

    /// <summary>
    /// Open an include file.
    /// </summary>
    private async Task DoIncludeAsync()
    {
        int i;
        var str = new StringBuilder();

        // skip over to name
        await frontEnd.BlanksAsync().ConfigureAwait(false);
        if (storage.Line.ElementAtOrDefault(storage.LPtr) is '"' or '<')
        {
            storage.LPtr++;
        }

        i = 0;
        while (storage.Line.ElementAtOrDefault(storage.LPtr + i) is char c
            && c != '"'
            && c != '>'
            && c != '\n')
        {
            _ = str.Append(c);
            i++;
        }

        try
        {
            storage.Input2 = File.OpenText(str.ToString());
        }
        catch (Exception ex)
        {
            storage.Input2 = null;
            throw new InvalidOperationException(
                "open failure on include file", ex);
        }

        frontEnd.Kill();
    }

    /// <summary>
    /// Define a macro symbol.
    /// </summary>
    private async Task DoDefineAsync()
    {
        storage.MsName = await frontEnd.SymNameAsync().ConfigureAwait(false);
        if (storage.MsName is null)
        {
            ErrorUseCases.IllName();
            frontEnd.Kill();
            return;
        }

        if (!storage.Mac.ContainsKey(storage.MsName) &&
            storage.Mac.Count >= MacroPool.MacNbr)
        {
            throw new InvalidOperationException("macro name table full");
        }

        while (frontEnd.White())
        {
            _ = frontEnd.Gch();
        }

        var macQ = new StringBuilder();
        while (PutMac(macQ, frontEnd.Gch()) is not null)
        {
            // already parsed
        }

        storage.Mac[storage.MsName] = macQ.ToString();

        static char? PutMac(StringBuilder macQ, char? c)
        {
            _ = macQ.Append(c);
            return c;
        }
    }

    /// <summary>
    /// Begin a function.
    /// </summary>
    /// <remarks>
    /// Called from <see cref="ParseAsync"/> and tries to make a function
    /// out of the following text.
    /// </remarks>
    private async Task DoFunctionAsync()
    {
        SymbolTableEntry? ptr;

        storage.LitQ.Clear(); // clear lit pool
        storage.LastSt = StatementType.None; // no statement yet
        storage.NoLoc = false; // enable block-level declarations
        storage.NoGo = false; // enable goto statements
        storage.LitLab = utility.GetLabel(); // label next lit pool
        storage.SymTab.Locals.Clear(); // clear local variables
        if (await frontEnd.MatchAsync("void").ConfigureAwait(false))
        {
            // skip "void" & locate header
            await frontEnd.BlanksAsync().ConfigureAwait(false);
        }

        if (storage.Monitor)
        {
            await Console.Error.WriteLineAsync(storage.Line).ConfigureAwait(false);
        }

        storage.SsName = await frontEnd.SymNameAsync().ConfigureAwait(false) ??
            throw new InvalidOperationException(
                "illegal function or declaration");

        // already in symbol table?
        ptr = symbolTable.FindGlb(storage.SsName);
        if (ptr is not null)
        {
            if (ptr.Class == SymbolClass.AutoExt)
            {
                ptr.Class = SymbolClass.Static;
            }
            else
            {
                ErrorUseCases.MultiDef(storage.SsName);
            }
        }
        else
        {
            _ = symbolTable.AddSym(
                storage.SsName,
                SymbolIdentity.Function,
                SymbolType.Int,
                0,
                0,
                storage.SymTab.Globals,
                SymbolClass.Static);
        }

        await backEnd.PublicAsync(SymbolIdentity.Function)
            .ConfigureAwait(false);
        storage.ArgStk = 0; // init arg count
        if (!await frontEnd.MatchAsync("(").ConfigureAwait(false))
        {
            throw new InvalidOperationException("no open paren");
        }

        // then count args
        while (!await frontEnd.MatchAsync(")").ConfigureAwait(false))
        {
            storage.SsName = await frontEnd.SymNameAsync()
                .ConfigureAwait(false);
            if (storage.SsName is not null)
            {
                if (symbolTable.FindLoc(storage.SsName) is not null)
                {
                    ErrorUseCases.MultiDef(storage.SsName);
                }
                else
                {
                    _ = symbolTable.AddSym(
                        storage.SsName,
                        0,
                        0,
                        0,
                        storage.ArgStk,
                        storage.SymTab.Locals,
                        SymbolClass.Automatic);
                    storage.ArgStk += Machine.Bpw;
                }
            }
            else
            {
                throw new InvalidOperationException("illegal argument name");
            }

            await frontEnd.BlanksAsync().ConfigureAwait(false);
            if ((storage.LPtr >= storage.Line.Length
                || FrontEnd.StrEq(storage.Line[storage.LPtr..], ")") == 0)
                && (await frontEnd.MatchAsync(",").ConfigureAwait(false)))
            {
                throw new InvalidOperationException("no comma");
            }

            if (await frontEnd.EndStAsync().ConfigureAwait(false))
            {
                break;
            }
        }

        storage.Csp = 0; // preset stack ptr

        // account for the pushed BP
        storage.ArgTop = storage.ArgStk + Machine.Bpw;
        while (storage.ArgStk != 0)
        {
            if (await frontEnd.AMatchAsync("char", 4).ConfigureAwait(false))
            {
                await this.DoArgsAsync(SymbolType.Chr).ConfigureAwait(false);
                await frontEnd.NsAsync().ConfigureAwait(false);
            }
            else if (await frontEnd.AMatchAsync("int", 3).ConfigureAwait(false))
            {
                await this.DoArgsAsync(SymbolType.Int).ConfigureAwait(false);
                await frontEnd.NsAsync().ConfigureAwait(false);
            }
            else if (await frontEnd.AMatchAsync("unsigned", 8)
                .ConfigureAwait(false))
            {
                if (await frontEnd.AMatchAsync("char", 4).ConfigureAwait(false))
                {
                    await this.DoArgsAsync(SymbolType.UChr)
                        .ConfigureAwait(false);
                    await frontEnd.NsAsync().ConfigureAwait(false);
                }
                else
                {
                    _ = await frontEnd.AMatchAsync("int", 3)
                        .ConfigureAwait(false);
                    await this.DoArgsAsync(SymbolType.UInt)
                        .ConfigureAwait(false);
                    await frontEnd.NsAsync().ConfigureAwait(false);
                }
            }
            else
            {
                throw new InvalidOperationException(
                    "wrong number of arguments");
            }
        }

        await backEnd.GenAsync(PCode.ENTER, null).ConfigureAwait(false);
        _ = await localParser.StatementAsync().ConfigureAwait(false);
        if (storage.LastSt is not StatementType.Return
            and not StatementType.Goto)
        {
            await backEnd.GenAsync(PCode.RETURN, null).ConfigureAwait(false);
        }

        if (storage.LitPtr != 0)
        {
            await backEnd.ToSegAsync(SegmentType.DataSeg).ConfigureAwait(false);
            await backEnd.GenAsync(PCode.REFm, storage.LitLab)
                .ConfigureAwait(false);

            // dump literals
            await backEnd.DumpLitsAsync(1).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Declare argument types.
    /// </summary>
    private async Task DoArgsAsync(SymbolType type)
    {
        SymbolIdentity id;
        int sz;

        while (true)
        {
            if (storage.ArgStk == 0)
            {
                return; // no arguments
            }

            (var n, id, sz) = await localParser.DeclAsync(
                type, SymbolIdentity.Pointer)
                .ConfigureAwait(false);
            if (n is not null)
            {
                if (symbolTable.FindLoc(n) is SymbolTableEntry ptr)
                {
                    ptr.Ident = id;
                    ptr.Type = type;
                    ptr.Size = sz;
                    ptr.Offset = storage.ArgTop - ptr.Offset;
                }
                else
                {
                    throw new InvalidOperationException("not an argument");
                }
            }

            storage.ArgStk -= Machine.Bpw; // cnt down
            if (await frontEnd.EndStAsync().ConfigureAwait(false))
            {
                return;
            }

            if (!await frontEnd.MatchAsync(",").ConfigureAwait(false))
            {
                throw new InvalidOperationException("no comma");
            }
        }
    }
}
