// <copyright file="Storage.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc;

using System.Collections.ObjectModel;

/// <summary>
/// Miscellaneous storage.
/// </summary>
public class Storage(
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
    int ifLevel,
    int skipLevel,
    int nxtLab,
    int litLab,
    int csp,
    bool eof,
    TextWriter output,
    bool files,
    int fileArg,
    TextReader? input,
    TextReader? input2,
    bool cCode,
    int sLast,
    TextWriter? listFp,
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
    /// <param name="stage">Staging buffer.</param>
    /// <param name="output">Fd for output file.</param>
    /// <param name="files">
    /// A value indicating whether file list specified on cmd line.
    /// </param>
    /// <param name="input">Fd for input file.</param>
    /// <param name="cCode">A value indicating whether parsing C code.</param>
    /// <param name="oldSeg">Current <see cref="SegmentType"/>.</param>
    /// <param name="symTab">Symbol table.</param>
    /// <param name="litQ">Literal pool.</param>
    /// <param name="mac">Macro buffer.</param>
    /// <param name="lineType">
    /// A value indicating whether <see cref="Line"/> points to
    /// <see cref="PLine"/> or <see cref="MLine"/>.
    /// </param>
    /// <param name="ssName">Static symbol name.</param>
    public Storage(
        Collection<KeyValuePair<PCode, int>>? stage = null,
        StreamWriter? output = null,
        bool files = false,
        StreamReader? input = null,
        bool cCode = true,
        SegmentType oldSeg = SegmentType.None,
        SymbolTable? symTab = null,
        Collection<sbyte>? litQ = null,
        Dictionary<string, string>? mac = null,
        BufferLineType? lineType = null,
        string? ssName = null)
        : this(
            0,
            0,
            [],
            0,
            stage,
            [],
            [],
            0,
            null,
            null,
            0,
            0,
            0,
            0,
            Machine.Bpw,
            false,
            output ?? Console.Out,
            files,
            0,
            input,
            null,
            cCode,
            StagingBuffer.StageSize,
            null,
            oldSeg,
            false,
            false,
            false,
            false,
            symTab ?? new([], []),
            litQ ?? [],
            mac ?? [],
            string.Empty,
            string.Empty,
            lineType ?? BufferLineType.Parsing,
            0,
            null,
            ssName)
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
    /// Gets the macro buffer.
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
