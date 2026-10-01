using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace I52Ct21d.HostFacts.Tools.Scan;

/// <summary>One decoded IL instruction. <see cref="Operand"/> is the token, the constant or the absolute branch target.</summary>
public sealed record Instr(int Offset, OpCode Op, long Operand, IReadOnlyList<int>? SwitchTargets);

/// <summary>
/// Minimal IL decoder for the semantic scan. It reads the bytes of a method body and returns the instructions with their operands, so
/// the scan sees the CONSTANT operand of a call site (an enum literal such as <c>OpenMode.ForWrite</c> leaves no member reference, only
/// an <c>ldc.i4</c>; design 5.3 item 2).
/// </summary>
public static class IlReader
{
    private static readonly Dictionary<ushort, OpCode> Table = BuildTable();

    private static Dictionary<ushort, OpCode> BuildTable()
    {
        var table = new Dictionary<ushort, OpCode>();
        foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            if (field.GetValue(null) is OpCode op) table[(ushort)op.Value] = op;
        return table;
    }

    public static IReadOnlyList<Instr> Decode(byte[] il)
    {
        var list = new List<Instr>();
        var i = 0;
        while (i < il.Length)
        {
            var start = i;
            ushort value = il[i++];
            if (value == 0xFE) value = (ushort)(0xFE00 | il[i++]);
            if (!Table.TryGetValue(value, out var op)) throw new InvalidDataException("unknown opcode 0x" + value.ToString("X") + " at IL offset " + start);

            long operand = 0;
            List<int>? switchTargets = null;
            switch (op.OperandType)
            {
                case OperandType.InlineNone:
                    break;
                case OperandType.ShortInlineBrTarget:
                    operand = (sbyte)il[i]; i += 1; operand += i;
                    break;
                case OperandType.ShortInlineI:
                    operand = (sbyte)il[i]; i += 1;
                    break;
                case OperandType.ShortInlineVar:
                    operand = il[i]; i += 1;
                    break;
                case OperandType.InlineVar:
                    operand = BitConverter.ToUInt16(il, i); i += 2;
                    break;
                case OperandType.InlineBrTarget:
                    operand = BitConverter.ToInt32(il, i); i += 4; operand += i;
                    break;
                case OperandType.InlineField:
                case OperandType.InlineMethod:
                case OperandType.InlineSig:
                case OperandType.InlineString:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.InlineI:
                    operand = BitConverter.ToInt32(il, i); i += 4;
                    break;
                case OperandType.ShortInlineR:
                    i += 4;
                    break;
                case OperandType.InlineI8:
                case OperandType.InlineR:
                    i += 8;
                    break;
                case OperandType.InlineSwitch:
                    var n = BitConverter.ToInt32(il, i); i += 4;
                    var baseOffset = i + 4 * n;
                    switchTargets = new List<int>();
                    for (var k = 0; k < n; k++) { switchTargets.Add(baseOffset + BitConverter.ToInt32(il, i)); i += 4; }
                    break;
                default:
                    throw new InvalidDataException("unsupported operand type " + op.OperandType);
            }
            list.Add(new Instr(start, op, operand, switchTargets));
        }
        return list;
    }

    /// <summary>The int32 constant pushed by an <c>ldc.i4*</c> instruction, or null when the instruction is not one.</summary>
    public static int? ConstantI4(Instr ins)
    {
        var v = (ushort)ins.Op.Value;
        if (v == (ushort)OpCodes.Ldc_I4_M1.Value) return -1;
        for (var k = 0; k <= 8; k++)
            if (v == (ushort)(OpCodes.Ldc_I4_0.Value + k)) return k;
        if (v == (ushort)OpCodes.Ldc_I4_S.Value || v == (ushort)OpCodes.Ldc_I4.Value) return (int)ins.Operand;
        return null;
    }

    /// <summary>A load that pushes exactly one value and consumes none (safe to step over when walking back to an argument).</summary>
    public static bool IsSimpleLoad(Instr ins)
    {
        if (ConstantI4(ins) is not null) return true;
        var op = ins.Op;
        return op == OpCodes.Ldnull || op == OpCodes.Ldstr || op == OpCodes.Ldc_I8 || op == OpCodes.Ldc_R4 || op == OpCodes.Ldc_R8
               || op == OpCodes.Ldloc || op == OpCodes.Ldloc_S || op == OpCodes.Ldloc_0 || op == OpCodes.Ldloc_1 || op == OpCodes.Ldloc_2
               || op == OpCodes.Ldloc_3 || op == OpCodes.Ldarg || op == OpCodes.Ldarg_S || op == OpCodes.Ldarg_0 || op == OpCodes.Ldarg_1
               || op == OpCodes.Ldarg_2 || op == OpCodes.Ldarg_3 || op == OpCodes.Ldloca || op == OpCodes.Ldloca_S || op == OpCodes.Ldarga
               || op == OpCodes.Ldarga_S;
    }

    public static HashSet<int> BranchTargets(IReadOnlyList<Instr> code)
    {
        var targets = new HashSet<int>();
        foreach (var ins in code)
        {
            if (ins.Op.OperandType is OperandType.ShortInlineBrTarget or OperandType.InlineBrTarget) targets.Add((int)ins.Operand);
            if (ins.SwitchTargets is not null) foreach (var t in ins.SwitchTargets) targets.Add(t);
        }
        return targets;
    }

    public static bool IsCall(Instr ins) =>
        ins.Op == OpCodes.Call || ins.Op == OpCodes.Callvirt || ins.Op == OpCodes.Newobj || ins.Op == OpCodes.Ldftn
        || ins.Op == OpCodes.Ldvirtftn || ins.Op == OpCodes.Jmp || ins.Op == OpCodes.Calli;
}
