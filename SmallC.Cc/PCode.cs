// <copyright file="PCode.cs" company="Soli Deo Gloria Apps">
// Copyright (c) Soli Deo Gloria Apps. All rights reserved.
// </copyright>

namespace SmallC.Cc;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// P-code symbols.
/// </summary>
/// <remarks>
/// Legend:
/// <list type="bullet">
/// <item>1 = primary register (pr in comments)</item>
/// <item>2 = secondary register (sr in comments)</item>
/// <item>b = byte</item>
/// <item>f = jump on false condition</item>
/// <item>l = current literal pool label number</item>
/// <item>m = memory reference by label</item>
/// <item>p = indirect reference thru pointer in sr</item>
/// <item>r = repeated r times</item>
/// <item>s = stack frame reference</item>
/// <item>u = unsigned</item>
/// <item>w = word</item>
/// <item>_ (tail) = another p-code completes this one</item>
/// </list>
/// </remarks>
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Literature")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "Literature")]
public enum PCode
{
    /// <summary>
    /// No P-code.
    /// </summary>
    None,

    // Compiler-generated

    /// <summary>
    /// Add sr to pr.
    /// </summary>
    ADD12,

    /// <summary>
    /// Add to stack pointer.
    /// </summary>
    ADDSP,

    /// <summary>
    /// AND sr to pr.
    /// </summary>
    AND12,

    /// <summary>
    /// Arith negate pr.
    /// </summary>
    ANEG1,

    /// <summary>
    /// Pass arg count to function.
    /// </summary>
    ARGCNTn,

    /// <summary>
    /// Arith shift left sr by pr into pr.
    /// </summary>
    ASL12,

    /// <summary>
    /// Arith shift right sr by pr into pr.
    /// </summary>
    ASR12,

    /// <summary>
    /// Call function thru pr.
    /// </summary>
    CALL1,

    /// <summary>
    /// Call function directly.
    /// </summary>
    CALLm,

    /// <summary>
    /// Define bytes (part 1).
    /// </summary>
    BYTE_,

    /// <summary>
    /// Define byte of value n.
    /// </summary>
    BYTEn,

    /// <summary>
    /// Define r bytes of value 0.
    /// </summary>
    BYTEr0,

    /// <summary>
    /// Ones complement pr.
    /// </summary>
    COM1,

    /// <summary>
    /// Double pr.
    /// </summary>
    DBL1,

    /// <summary>
    /// Double sr.
    /// </summary>
    DBL2,

    /// <summary>
    /// Div pr by sr.
    /// </summary>
    DIV12,

    /// <summary>
    /// Div pr by sr unsignned.
    /// </summary>
    DIV12u,

    /// <summary>
    /// Set stack frame on function entry.
    /// </summary>
    ENTER,

    /// <summary>
    /// Jump if (pr == 0) is false.
    /// </summary>
    EQ10f,

    /// <summary>
    /// Set pr TRUE if (sr == pr).
    /// </summary>
    EQ12,

    /// <summary>
    /// Jump if (pr >= 0) is false.
    /// </summary>
    GE10f,

    /// <summary>
    /// Set pr TRUE if (sr >= pr).
    /// </summary>
    GE12,

    /// <summary>
    /// Set pr TRUE if (sr >= pr) unsigned.
    /// </summary>
    GE12u,

    /// <summary>
    /// Point pr to function's literal pool.
    /// </summary>
    POINT1l,

    /// <summary>
    /// Point pr to mem item thru label.
    /// </summary>
    POINT1m,

    /// <summary>
    /// Get byte into pr from mem thru label.
    /// </summary>
    GETb1m,

    /// <summary>
    /// Get unsigned byte into pr from mem thru label.
    /// </summary>
    GETb1mu,

    /// <summary>
    /// Get byte into pr from mem thru sr ptr.
    /// </summary>
    GETb1p,

    /// <summary>
    /// Get unsigned byte into pr from mem thru sr ptr.
    /// </summary>
    GETb1pu,

    /// <summary>
    /// Get word into pr from mem thru label.
    /// </summary>
    GETw1m,

    /// <summary>
    /// Get word of value n into pr.
    /// </summary>
    GETw1n,

    /// <summary>
    /// Get word into pr from mem thru sr ptr.
    /// </summary>
    GETw1p,

    /// <summary>
    /// Get word of value n into sr.
    /// </summary>
    GETw2n,

    /// <summary>
    /// Jump if (pr > 0) is false.
    /// </summary>
    GT10f,

    /// <summary>
    /// Set pr TRUE if (sr > pr).
    /// </summary>
    GT12,

    /// <summary>
    /// Set pr TRUE if (sr > pr) unsigned.
    /// </summary>
    GT12u,

    /// <summary>
    /// Define word (part 1).
    /// </summary>
    WORD_,

    /// <summary>
    /// Define word of value n.
    /// </summary>
    WORDn,

    /// <summary>
    /// Define r words of value 0.
    /// </summary>
    WORDr0,

    /// <summary>
    /// Jump to label.
    /// </summary>
    JMPm,

    /// <summary>
    /// Define label m.
    /// </summary>
    LABm,

    /// <summary>
    /// Jump if (pr &lt;= 0) is false.
    /// </summary>
    LE10f,

    /// <summary>
    /// Set pr TRUE if (sr &lt;= pr).
    /// </summary>
    LE12,

    /// <summary>
    /// Set pr TRUE if (sr &lt;= pr) unsigned.
    /// </summary>
    LE12u,

    /// <summary>
    /// Logical negate pr.
    /// </summary>
    LNEG1,

    /// <summary>
    /// Jump if (pr &lt; 0) is false.
    /// </summary>
    LT10f,

    /// <summary>
    /// Set pr TRUE if (sr &lt; pr).
    /// </summary>
    LT12,

    /// <summary>
    /// Set pr TRUE if (sr &lt; pr) unsigned.
    /// </summary>
    LT12u,

    /// <summary>
    /// Modulo pr by sr.
    /// </summary>
    MOD12,

    /// <summary>
    /// Modulo pr by sr unsigned.
    /// </summary>
    MOD12u,

    /// <summary>
    /// Move pr to sr.
    /// </summary>
    MOVE21,

    /// <summary>
    /// Multiply pr by sr.
    /// </summary>
    MUL12,

    /// <summary>
    /// Multiply pr by sr unsigned.
    /// </summary>
    MUL12u,

    /// <summary>
    /// Jump if (pr != 0) is false.
    /// </summary>
    NE10f,

    /// <summary>
    /// Set pr TRUE if (sr != pr).
    /// </summary>
    NE12,

    /// <summary>
    /// Define near pointer thru label.
    /// </summary>
    NEARm,

    /// <summary>
    /// OR sr onto pr.
    /// </summary>
    OR12,

    /// <summary>
    /// Point pr to stack item.
    /// </summary>
    POINT1s,

    /// <summary>
    /// Pop stack into sr.
    /// </summary>
    POP2,

    /// <summary>
    /// Push pr onto stack.
    /// </summary>
    PUSH1,

    /// <summary>
    /// Put pr byte in mem thru label.
    /// </summary>
    PUTbm1,

    /// <summary>
    /// Put pr byte in mem thru sr ptr.
    /// </summary>
    PUTbp1,

    /// <summary>
    /// Put pr word in mem thru label.
    /// </summary>
    PUTwm1,

    /// <summary>
    /// Put pr word in mem thru sr ptr.
    /// </summary>
    PUTwp1,

    /// <summary>
    /// Dec pr (may repeat).
    /// </summary>
    rDEC1,

    /// <summary>
    /// Finish instruction with label.
    /// </summary>
    REFm,

    /// <summary>
    /// Restore stack and return.
    /// </summary>
    RETURN,

    /// <summary>
    /// Inc pr (may repeat).
    /// </summary>
    rINC1,

    /// <summary>
    /// Sub sr from pr.
    /// </summary>
    SUB12,

    /// <summary>
    /// Swap pr and sr.
    /// </summary>
    SWAP12,

    /// <summary>
    /// Swap pr and top of stack.
    /// </summary>
    SWAP1s,

    /// <summary>
    /// Find switch case.
    /// </summary>
    SWITCH,

    /// <summary>
    /// XOR pr with sr.
    /// </summary>
    XOR12,

    // Optimizer-generated

    /// <summary>
    /// Add n to pr.
    /// </summary>
    ADD1n,

    /// <summary>
    /// Add pr to sr.
    /// </summary>
    ADD21,

    /// <summary>
    /// Add immediate to sr.
    /// </summary>
    ADD2n,

    /// <summary>
    /// Add n to mem byte thru sr ptr.
    /// </summary>
    ADDbpn,

    /// <summary>
    /// Add n to mem word thru sr ptr.
    /// </summary>
    ADDwpn,

    /// <summary>
    /// Add n to mem byte/word thru label (part 1).
    /// </summary>
    ADDm_,

    /// <summary>
    /// Finish instruction with ,n.
    /// </summary>
    COMMAn,

    /// <summary>
    /// Dec mem byte thru sr ptr.
    /// </summary>
    DECbp,

    /// <summary>
    /// Dec mem word thru sr ptr.
    /// </summary>
    DECwp,

    /// <summary>
    /// Point sr to mem thru label.
    /// </summary>
    POINT2m,

    /// <summary>
    /// Point sr to mem thru label (part 1).
    /// </summary>
    POINT2m_,

    /// <summary>
    /// Get byte into pr from stack.
    /// </summary>
    GETb1s,

    /// <summary>
    /// Get unsigned byte into pr from stack.
    /// </summary>
    GETb1su,

    /// <summary>
    /// Get word into pr from mem thru label (part 1).
    /// </summary>
    GETw1m_,

    /// <summary>
    /// Get word into pr from stack.
    /// </summary>
    GETw1s,

    /// <summary>
    /// Get word into sr from mem (label).
    /// </summary>
    GETw2m,

    /// <summary>
    /// Get word into sr thru sr ptr.
    /// </summary>
    GETw2p,

    /// <summary>
    /// Get word into sr from stack.
    /// </summary>
    GETw2s,

    /// <summary>
    /// Inc byte in mem thru sr ptr.
    /// </summary>
    INCbp,

    /// <summary>
    /// Inc word in mem thru sr ptr.
    /// </summary>
    INCwp,

    /// <summary>
    /// Finish instruction with +n.
    /// </summary>
    PLUSn,

    /// <summary>
    /// Point sr to stack.
    /// </summary>
    POINT2s,

    /// <summary>
    /// Push sr to stack.
    /// </summary>
    PUSH2,

    /// <summary>
    /// Push word from mem thru label.
    /// </summary>
    PUSHm,

    /// <summary>
    /// Push word from mem thru sr ptr.
    /// </summary>
    PUSHp,

    /// <summary>
    /// Push word from stack.
    /// </summary>
    PUSHs,

    /// <summary>
    /// Put byte/word into mem thru label (part 1).
    /// </summary>
    PUT_m_,

    /// <summary>
    /// Dec sr (may repeat).
    /// </summary>
    rDEC2,

    /// <summary>
    /// Inc sr (may repeat).
    /// </summary>
    rINC2,

    /// <summary>
    /// Sub from mem byte/word thru label (part 1).
    /// </summary>
    SUB_m_,

    /// <summary>
    /// Sub n from pr.
    /// </summary>
    SUB1n,

    /// <summary>
    /// Sub n from mem byte thru sr ptr.
    /// </summary>
    SUBbpn,

    /// <summary>
    /// Sub n from mem word thru sr ptr.
    /// </summary>
    SUBwpn,

    /// <summary>
    /// Size of code[].
    /// </summary>
    PCODES,
}
