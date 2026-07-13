// <copyright file="Storage.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc;

using System.Collections.ObjectModel;

/// <summary>
/// Miscellaneous storage.
/// </summary>
public class Storage(
    bool noGo,
    bool noLoc,
    int opIndex,
    int opSize,
    Dictionary<int, string> swNext,
    int swEnd,
    Collection<KeyValuePair<PCode, int>>? stage,
    Collection<WhileQueueEntry> wq,
    Collection<string> args,
    int wqPtr,
    char? ch,
    char? nCh,
    int declared,
    int ifLevel,
    int skipLevel,
    int nxtLab,
    int litLab,
    int csp,
    int argStk,
    int argTop,
    int nCmp,
    bool errFlag,
    bool eof,
    TextWriter output,
    bool files,
    int fileArg,
    TextReader? input,
    TextReader? input2,
    bool cCode,
    int sLast,
    TextWriter? listFp,
    StatementType lastSt,
    SegmentType oldSeg,
    bool optimize,
    bool alarm,
    bool monitor,
    bool pause,
    SymbolTable symTab,
    Collection<sbyte> litQ,
    Dictionary<string, string> mac,
    string pLine,
    string mLine,
    Storage.BufferLineType lineType,
    int lPtr,
    string? msName,
    string? ssName)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Storage"/> class.
    /// </summary>
    /// <param name="swNext">Switch queue.</param>
    /// <param name="swEnd">Last index in switch queue.</param>
    /// <param name="stage">Staging buffer.</param>
    /// <param name="wq">While queue.</param>
    /// <param name="args">Static args.</param>
    /// <param name="wqPtr">Index to next entry.</param>
    /// <param name="csp">Compiler relative stk ptr.</param>
    /// <param name="output">Fd for output file.</param>
    /// <param name="files">A value indicating whether file list specified on cmd line.</param>
    /// <param name="input">Fd for input file.</param>
    /// <param name="cCode">A value indicating whether parsing C code.</param>
    /// <param name="sLast">Last index in stage.</param>
    /// <param name="oldSeg">Current <see cref="SegmentType"/>.</param>
    /// <param name="symTab">Symbol table.</param>
    /// <param name="litQ">Literal pool.</param>
    /// <param name="mac">Macro name/string buffer.</param>
    /// <param name="pLine">Parsing buffer.</param>
    /// <param name="mLine">Macro buffer.</param>
    /// <param name="lineType">
    /// A value indicating whether <see cref="Line"/> points to
    /// <see cref="PLine"/> or <see cref="MLine"/>.
    /// </param>
    /// <param name="ssName">Static symbol name.</param>
    public Storage(
        Dictionary<int, string>? swNext = null,
        int? swEnd = null,
        Collection<KeyValuePair<PCode, int>>? stage = null,
        Collection<WhileQueueEntry>? wq = null,
        Collection<string>? args = null,
        int? wqPtr = null,
        int? csp = null,
        TextWriter? output = null,
        bool? files = null,
        TextReader? input = null,
        bool? cCode = null,
        int? sLast = null,
        SegmentType? oldSeg = null,
        SymbolTable? symTab = null,
        Collection<sbyte>? litQ = null,
        Dictionary<string, string>? mac = null,
        string? pLine = null,
        string? mLine = null,
        BufferLineType? lineType = null,
        string? ssName = null)
        : this(
            noGo: false,
            noLoc: false,
            opIndex: 0,
            opSize: 0,
            swNext: swNext ?? [],
            swEnd: swEnd ?? SwitchTable.SwTabSz,
            stage: stage,
            wq: wq ?? [],
            args: args ?? [],
            wqPtr: wqPtr ?? 0,
            ch: null,
            nCh: null,
            declared: 0,
            ifLevel: 0,
            skipLevel: 0,
            nxtLab: 0,
            litLab: 0,
            csp: csp ?? 0,
            argStk: 0,
            argTop: 0,
            nCmp: 0,
            errFlag: false,
            eof: false,
            output: output ?? Console.Out,
            files: files ?? false,
            fileArg: 0,
            input: input,
            input2: null,
            cCode: cCode ?? true,
            sLast: sLast ?? StagingBuffer.StageSize,
            listFp: null,
            lastSt: StatementType.None,
            oldSeg: oldSeg ?? SegmentType.None,
            optimize: false,
            alarm: false,
            monitor: false,
            pause: false,
            symTab: symTab ?? new([], []),
            litQ: litQ ?? [],
            mac: mac ?? [],
            pLine: pLine ?? string.Empty,
            mLine: mLine ?? string.Empty,
            lineType: lineType ?? BufferLineType.None,
            lPtr: 0,
            msName: null,
            ssName: ssName)
    {
    }

    /// <summary>
    /// <see cref="Line"/> type enumeration.
    /// </summary>
    public enum BufferLineType
    {
        /// <summary>
        /// No buffer selected.
        /// </summary>
        None,

        /// <summary>
        /// Parsing buffer.
        /// </summary>
        Parsing,

        /// <summary>
        /// Macro buffer.
        /// </summary>
        Macro,
    }

    /// <summary>
    /// Gets or sets a value indicating whether to disable goto statements.
    /// </summary>
    public bool NoGo { get; set; } = noGo;

    /// <summary>
    /// Gets or sets a value indicating whether to disable block locals.
    /// </summary>
    public bool NoLoc { get; set; } = noLoc;

    /// <summary>
    /// Gets or sets index to matched operator.
    /// </summary>
    public int OpIndex { get; set; } = opIndex;

    /// <summary>
    /// Gets or sets size of operator in characters.
    /// </summary>
    public int OpSize { get; set; } = opSize;

    /// <summary>
    /// Gets switch queue.
    /// </summary>
    public Dictionary<int, string> SwNext { get; } = swNext;

    /// <summary>
    /// Gets last index in switch queue.
    /// </summary>
    public int SwEnd { get; } = swEnd;

    /// <summary>
    /// Gets staging buffer.
    /// </summary>
    public Collection<KeyValuePair<PCode, int>>? Stage { get; private set; } =
        stage;

    /// <summary>
    /// Gets while queue.
    /// </summary>
    public Collection<WhileQueueEntry> Wq { get; } = wq;

    /// <summary>
    /// Gets static args.
    /// </summary>
    public Collection<string> Args { get; } = args;

    /// <summary>
    /// Gets index to next entry.
    /// </summary>
    public int WqPtr { get; } = wqPtr;

    /// <summary>
    /// Gets index to next <see cref="LitQ"/> entry.
    /// </summary>
    public int LitPtr => this.LitQ.Count;

    /// <summary>
    /// Gets index to parsing buffer.
    /// </summary>
    public int PPtr => this.PLine.Length;

    /// <summary>
    /// Gets or sets current character of input line.
    /// </summary>
    public char? Ch { get; set; } = ch;

    /// <summary>
    /// Gets or sets next character of input line.
    /// </summary>
    public char? NCh { get; set; } = nCh;

    /// <summary>
    /// Gets or sets # of local bytes to declare, -1 when declared.
    /// </summary>
    public int Declared { get; set; } = declared;

    /// <summary>
    /// Gets or sets #if... nest level.
    /// </summary>
    public int IfLevel { get; set; } = ifLevel;

    /// <summary>
    /// Gets or sets level at which #if... skipping started.
    /// </summary>
    public int SkipLevel { get; set; } = skipLevel;

    /// <summary>
    /// Gets or sets next avail label #.
    /// </summary>
    public int NxtLab { get; set; } = nxtLab;

    /// <summary>
    /// Gets or sets label # assigned to literal pool.
    /// </summary>
    public int LitLab { get; set; } = litLab;

    /// <summary>
    /// Gets or sets compiler relative stk ptr.
    /// </summary>
    public int Csp { get; set; } = csp;

    /// <summary>
    /// Gets or sets function arg sp.
    /// </summary>
    public int ArgStk { get; set; } = argStk;

    /// <summary>
    /// Gets or sets highest formal argument offset.
    /// </summary>
    public int ArgTop { get; set; } = argTop;

    /// <summary>
    /// Gets # open compound statements.
    /// </summary>
    public int NCmp { get; } = nCmp;

    /// <summary>
    /// Gets or sets a value indicating whether an error is in statement.
    /// </summary>
    public bool ErrFlag { get; set; } = errFlag;

    /// <summary>
    /// Gets or sets a value indicating whether end of input has been reached.
    /// </summary>
    public bool Eof { get; set; } = eof;

    /// <summary>
    /// Gets or sets fd for output file.
    /// </summary>
    public TextWriter Output { get; set; } = output;

    /// <summary>
    /// Gets or sets a value indicating whether file list specified on cmd line.
    /// </summary>
    public bool Files { get; set; } = files;

    /// <summary>
    /// Gets or sets cur file arg index.
    /// </summary>
    public int FileArg { get; set; } = fileArg;

    /// <summary>
    /// Gets or sets fd for input file.
    /// </summary>
    public TextReader? Input { get; set; } = input;

    /// <summary>
    /// Gets or sets fd for "#include" file.
    /// </summary>
    public TextReader? Input2 { get; set; } = input2;

    /// <summary>
    /// Gets or sets a value indicating whether parsing C code.
    /// </summary>
    public bool CCode { get; set; } = cCode;

    /// <summary>
    /// Gets next index in stage.
    /// </summary>
    public int? SNext => this.Stage?.Count;

    /// <summary>
    /// Gets last index in stage.
    /// </summary>
    public int? SLast { get; } = sLast;

    /// <summary>
    /// Gets or sets file pointer to list device.
    /// </summary>
    public TextWriter? ListFp { get; set; } = listFp;

    /// <summary>
    /// Gets or sets last parsed statement type.
    /// </summary>
    public StatementType LastSt { get; set; } = lastSt;

    /// <summary>
    /// Gets or sets current <see cref="SegmentType"/>.
    /// </summary>
    public SegmentType OldSeg { get; set; } = oldSeg;

    /// <summary>
    /// Gets or sets a value indicating whether to optimize output of staging
    /// buffer.
    /// </summary>
    public bool Optimize { get; set; } = optimize;

    /// <summary>
    /// Gets or sets a value indicating whether to emit audible alarm on errors.
    /// </summary>
    public bool Alarm { get; set; } = alarm;

    /// <summary>
    /// Gets or sets a value indicating whether to monitor function headers.
    /// </summary>
    public bool Monitor { get; set; } = monitor;

    /// <summary>
    /// Gets or sets a value indicating whether to pause for operator on errors.
    /// </summary>
    public bool Pause { get; set; } = pause;

    /// <summary>
    /// Gets symbol table.
    /// </summary>
    public SymbolTable SymTab { get; } = symTab;

    /// <summary>
    /// Gets literal pool.
    /// </summary>
    public Collection<sbyte> LitQ { get; } = litQ;

    /// <summary>
    /// Gets the macro name/string buffer.
    /// </summary>
    public Dictionary<string, string> Mac { get; } = mac;

    /// <summary>
    /// Gets or sets parsing buffer.
    /// </summary>
    public string PLine { get; set; } = pLine;

    /// <summary>
    /// Gets or sets macro buffer.
    /// </summary>
    public string MLine { get; set; } = mLine;

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="Line"/> points to
    /// <see cref="PLine"/> or <see cref="MLine"/>.
    /// </summary>
    public BufferLineType LineType { get; set; } = lineType;

    /// <summary>
    /// Gets or sets <see cref="PLine"/> or <see cref="MLine"/>, based on
    /// <see cref="LineType"/>.
    /// </summary>
    public string Line
    {
        get => this.LineType switch
        {
            BufferLineType.Parsing => this.PLine,
            BufferLineType.Macro => this.MLine,
            BufferLineType.None or _ => throw new InvalidOperationException(),
        };

        set
        {
            switch (this.LineType)
            {
                case BufferLineType.Parsing:
                    this.PLine = value;
                    break;

                case BufferLineType.Macro:
                    this.MLine = value;
                    break;

                case BufferLineType.None:
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Gets or sets index to <see cref="Line"/>.
    /// </summary>
    public int LPtr { get; set; } = lPtr;

    /// <summary>
    /// Gets literal string for '"'.
    /// </summary>
    public string Quote { get; } = "\"";

    /// <summary>
    /// Gets or sets macro symbol name.
    /// </summary>
    public string? MsName { get; set; } = msName;

    /// <summary>
    /// Gets or sets static symbol name.
    /// </summary>
    public string? SsName { get; set; } = ssName;

    /// <summary>
    /// Sets stage if not already active.
    /// </summary>
    public void SetStage()
    {
        this.Stage ??= [];
    }

    /// <summary>
    /// Clears stage.
    /// </summary>
    public void ClearStage()
    {
        this.Stage = null;
    }
}
