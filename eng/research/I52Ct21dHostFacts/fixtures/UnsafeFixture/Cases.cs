using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fx;

// One class per raw-memory write shape. NONE of this is ever executed: the scan only reads the compiled bytes.

/// <summary>*p = 5 through an int* parameter.</summary>
public static unsafe class PointerWrite
{
    public static void M(int* p) { *p = 5; }
}

/// <summary>Address of a local, pointer local, write through it.</summary>
public static unsafe class PointerLocalWrite
{
    public static int M() { int x = 0; int* p = &x; *p = 5; return x; }
}

/// <summary>stackalloc into an int*, then a pointer write.</summary>
public static unsafe class StackAllocPointerWrite
{
    public static int M() { int* p = stackalloc int[4]; p[1] = 7; return p[1]; }
}

/// <summary>stackalloc into a Span: no pointer in the C# source, but localloc and a void* constructor in the IL.</summary>
public static class StackAllocSpan
{
    public static int M() { Span<int> s = stackalloc int[4]; s[1] = 7; return s[1]; }
}

/// <summary>Unsafe.Write(void*, T).</summary>
public static unsafe class UnsafeWritePointer
{
    public static void M(int* p) { Unsafe.Write(p, 5); }
}

/// <summary>Unsafe.WriteUnaligned over a managed reference: no pointer at all.</summary>
public static class UnsafeWriteManaged
{
    public static void M(ref byte b) { Unsafe.WriteUnaligned(ref b, 5); }
}

/// <summary>Marshal.Copy(byte[], int, IntPtr, int).</summary>
public static class MarshalCopyToIntPtr
{
    public static void M(byte[] src, IntPtr dst) { Marshal.Copy(src, 0, dst, src.Length); }
}

/// <summary>Buffer.MemoryCopy(void*, void*, long, long).</summary>
public static unsafe class BufferMemoryCopy
{
    public static void M(byte* s, byte* d) { Buffer.MemoryCopy(s, d, 4, 4); }
}

/// <summary>GCHandle.Alloc(object, Pinned).</summary>
public static class GcHandleAlloc
{
    public static void M(object o) { GCHandle.Alloc(o, GCHandleType.Pinned); }
}

/// <summary>A pinned buffer through fixed.</summary>
public static unsafe class FixedBuffer
{
    public static void M(byte[] a) { fixed (byte* p = a) { *p = 1; } }
}

/// <summary>A control that must stay clean of the raw-memory rules: plain managed code (it is still reported for the module's unsafe attribute).</summary>
public static class PlainManaged
{
    public static int M(int a, int b) => a + b;
}
