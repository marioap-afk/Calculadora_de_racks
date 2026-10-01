using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using I52Ct21d.HostFacts.Tools.Scan;
using Xunit;

namespace I52Ct21d.HostFacts.Tests;

/// <summary>
/// The forbidden-API semantic scan, with NEGATIVE CONTROLS: for every forbidden API (design 5.3) a fixture assembly that calls it
/// is built (metadata references to AcDbMgd / the BCL, no AutoCAD file) and the scan MUST fail; positive controls must stay clean.
/// The real instrument assemblies are scanned too, and the real R0 DLL is MUTATED in memory (an <c>ldc.i4.0</c> OpenMode patched to
/// <c>ldc.i4.1</c>) to prove that the CLEAN verdict on it is not vacuous.
/// </summary>
public class ForbiddenApiScanTests
{
    private const string A = "AcDbMgd";
    private const string D = "Autodesk.AutoCAD.DatabaseServices.";
    private const string Db = D + "Database";
    private const string Tr = D + "Transaction";
    private const string Oid = "vt:AcDbMgd|" + D + "ObjectId";
    private const string Om = "vt:AcDbMgd|" + D + "OpenMode";
    private const string Fs = "vt:System.Runtime|System.IO.FileShare";
    private const string SideNs = "I52Ct21d.HostFacts.R0";
    private const string SideType = "AcadSideDbReader";
    private const string SideAsm = "I52Ct21d.HostFacts.R0";
    private const string Rt = "System.Runtime";
    private const string Interop = "System.Runtime.InteropServices.";
    private const string AppCore = "Autodesk.AutoCAD.ApplicationServices.Core.Application";

    private static byte[] InType(Action<FixtureBuilder.Il> body, string ns = "Fx", string name = "Violator", string asm = "Fixture")
    {
        var fb = new FixtureBuilder(asm);
        fb.Type(ns, name).Method("M", body);
        return fb.Build();
    }

    private static string[] Rules(byte[] image, ScanRole role = ScanRole.R0, string display = "fixture.dll") =>
        ForbiddenApiScan.Scan(image, display, ScanConfig.For(role)).Select(v => v.Rule).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToArray();

    // ---- POSITIVE CONTROLS ----------------------------------------------------------------------------------------------
    [Fact]
    public void Positive_control_the_side_database_reader_pattern_is_clean()
    {
        var image = InType(il =>
        {
            il.LdcI4(0).LdcI4(1).NewObj(A, Db, "bool", "bool").Pop();                                          // new Database(false, true)
            il.CallVirt(A, Db, "ReadDwgFile", "void", "string", Fs, "bool", "string");                          // ReadDwgFile of a private copy
            il.CallVirt(A, Db, "CloseInput", "void", "bool");
            il.CallVirt(A, Db, "get_TransactionManager", "cls:AcDbMgd|" + D + "TransactionManager");
            il.CallVirt(A, D + "TransactionManager", "StartOpenCloseTransaction", "cls:AcDbMgd|" + D + "OpenCloseTransaction");
            il.LdcI4(0).CallVirt(A, Tr, "GetObject", "cls:AcDbMgd|" + D + "DBObject", Oid, Om);               // OpenMode.ForRead
            il.LdcI4(0).LdcI4(1).CallVirt(A, Tr, "GetObject", "cls:AcDbMgd|" + D + "DBObject", Oid, Om, "bool"); // ForRead, openErased
            il.CallVirt(A, D + "DBObject", "GetXDataForApplication", "cls:AcDbMgd|" + D + "ResultBuffer", "string");
            il.Call("AcCoreMgd", AppCore, "GetSystemVariable", "obj", "string");
            il.CallVirt(A, Db, "Dispose", "void");
        }, SideNs, SideType, SideAsm);
        var violations = ForbiddenApiScan.Scan(image, "clean.dll", ScanConfig.For(ScanRole.R0));
        Assert.Empty(violations);
    }

    [Fact]
    public void Positive_control_read_only_io_and_getters_are_clean_outside_the_writer()
    {
        var image = InType(il =>
        {
            il.Call("System.Runtime", "System.IO.File", "ReadAllText", "string", "string");
            il.Call("System.Runtime", "System.IO.File", "ReadAllBytes", "obj", "string");
            il.Call("System.Runtime", "System.IO.File", "Exists", "bool", "string");
            il.Call("System.Runtime", "System.IO.File", "OpenRead", "obj", "string");
            il.Call("System.Runtime", "System.IO.File", "GetAttributes", "int", "string");
            il.Call("System.Runtime", "System.IO.Directory", "EnumerateFiles", "obj", "string");
            il.NewObj("System.Runtime", "System.IO.FileInfo", "string").Pop();
            il.CallVirt("System.Runtime", "System.IO.FileInfo", "get_Length", "int");
            il.CallVirt(A, D + "Entity", "get_Layer", "string");
            il.CallVirt("AcMgd", "Autodesk.AutoCAD.EditorInput.Editor", "WriteMessage", "void", "string", "obj");
        });
        Assert.Empty(Rules(image, ScanRole.R0));
        Assert.DoesNotContain("R-FILE-WRITE", Rules(image, ScanRole.Core));
    }

    // ---- NEGATIVE CONTROLS: one fixture per forbidden API; the scan MUST report the expected rule -------------------------
    private static readonly List<(string Name, Action<FixtureBuilder.Il> Body, string Rule)> Cases = BuildCases();

    public static IEnumerable<object[]> Forbidden()
    {
        for (var i = 0; i < Cases.Count; i++) yield return new object[] { i, Cases[i].Name };
    }

    private static List<(string Name, Action<FixtureBuilder.Il> Body, string Rule)> BuildCases()
    {
        var cases = new List<(string Name, Action<FixtureBuilder.Il> Body, string Rule)>
        {
            ("Database.SaveAs", il => il.CallVirt(A, Db, "SaveAs", "void", "string", "vt:AcDbMgd|" + D + "DwgVersion"), "R-DB-SAVE"),
            ("Database.Save", il => il.CallVirt(A, Db, "Save", "void"), "R-DB-SAVE"),
            ("Database.Wblock", il => il.CallVirt(A, Db, "Wblock", "cls:AcDbMgd|" + Db), "R-DB-SAVE"),
            ("Database.WblockCloneObjects", il => il.CallVirt(A, Db, "WblockCloneObjects", "void"), "R-DB-SAVE"),
            ("Database.Insert", il => il.CallVirt(A, Db, "Insert", "void"), "R-DB-SAVE"),
            ("GetObject ForWrite (1)", il => il.LdcI4(1).CallVirt(A, Tr, "GetObject", "obj", Oid, Om), "R-WRITE-OPEN"),
            ("GetObject ForNotify (2)", il => il.LdcI4(2).CallVirt(A, Tr, "GetObject", "obj", Oid, Om), "R-WRITE-OPEN"),
            ("GetObject (OpenMode)5", il => il.LdcI4(5).CallVirt(A, Tr, "GetObject", "obj", Oid, Om), "R-WRITE-OPEN"),
            ("GetObject ForWrite with trailing bool", il => il.LdcI4(1).LdcI4(0).CallVirt(A, Tr, "GetObject", "obj", Oid, Om, "bool"), "R-WRITE-OPEN"),
            ("GetObject non-constant mode", il => il.LdArg0().CallVirt(A, Tr, "GetObject", "obj", Oid, Om), "R-OPENMODE-UNRESOLVABLE"),
            ("GetObject non-constant trailing argument", il => il.LdcI4(0).Call(A, "Fx.Helper", "Flag", "bool").CallVirt(A, Tr, "GetObject", "obj", Oid, Om, "bool"), "R-OPENMODE-UNRESOLVABLE"),
            ("DBObject.UpgradeOpen", il => il.CallVirt(A, D + "DBObject", "UpgradeOpen", "void"), "R-WRITE-OPEN"),
            ("Transaction.Commit", il => il.CallVirt(A, Tr, "Commit", "void"), "R-COMMIT"),
            ("OpenCloseTransaction.Commit", il => il.CallVirt(A, D + "OpenCloseTransaction", "Commit", "void"), "R-COMMIT"),
            ("StartTransaction outside the side-db reader", il => il.CallVirt(A, D + "TransactionManager", "StartTransaction", "cls:AcDbMgd|" + Tr), "R-TRANSACTION-SCOPE"),
            ("StartOpenCloseTransaction outside the reader", il => il.CallVirt(A, D + "TransactionManager", "StartOpenCloseTransaction", "obj"), "R-TRANSACTION-SCOPE"),
            ("Database.TransactionManager outside the reader", il => il.CallVirt(A, Db, "get_TransactionManager", "obj"), "R-TRANSACTION-SCOPE"),
            ("new Database(true, true)", il => il.LdcI4(1).LdcI4(1).NewObj(A, Db, "bool", "bool"), "R-DB-CTOR"),
            ("new Database(false, true) outside the reader", il => il.LdcI4(0).LdcI4(1).NewObj(A, Db, "bool", "bool"), "R-DB-CTOR"),
            ("new Database()", il => il.NewObj(A, Db), "R-DB-CTOR"),
            ("new Database(false, false)", il => il.LdcI4(0).LdcI4(0).NewObj(A, Db, "bool", "bool"), "R-DB-CTOR"),
            ("new Database(non-constant)", il => il.LdArg0().LdcI4(1).NewObj(A, Db, "bool", "bool"), "R-DB-CTOR"),
            ("ReadDwgFile outside the reader", il => il.CallVirt(A, Db, "ReadDwgFile", "void", "string", Fs, "bool", "string"), "R-READDWG-SCOPE"),
            ("SetSystemVariable (Core)", il => il.Call("AcCoreMgd", AppCore, "SetSystemVariable", "void", "string", "obj"), "R-SYSVAR-SET"),
            ("SetSystemVariable (AcMgd)", il => il.Call("AcMgd", "Autodesk.AutoCAD.ApplicationServices.Application", "SetSystemVariable", "void", "string", "obj"), "R-SYSVAR-SET"),
            ("Overrule.AddOverrule", il => il.Call(A, "Autodesk.AutoCAD.Runtime.Overrule", "AddOverrule", "void", "obj", "bool"), "R-OVERRULE"),
            ("Overrule.set_Overruling", il => il.Call(A, "Autodesk.AutoCAD.Runtime.Overrule", "set_Overruling", "void", "bool"), "R-OVERRULE"),
            ("Editor.Command", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.EditorInput.Editor", "Command", "obj", "obj"), "R-COMMAND"),
            ("Editor.CommandAsync", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.EditorInput.Editor", "CommandAsync", "obj", "obj"), "R-COMMAND"),
            ("Document.SendStringToExecute", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.ApplicationServices.Document", "SendStringToExecute", "void", "string", "bool", "bool", "bool"), "R-COMMAND"),
            ("Document.LockDocument", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.ApplicationServices.Document", "LockDocument", "obj"), "R-COMMAND"),
            ("DocumentCollection.Open", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.ApplicationServices.DocumentCollection", "Open", "obj", "string"), "R-COMMAND"),
            ("DocumentCollection.Add", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.ApplicationServices.DocumentCollection", "Add", "obj", "string"), "R-COMMAND"),
            ("MdiActiveDocument setter", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.ApplicationServices.DocumentCollection", "set_MdiActiveDocument", "void", "obj"), "R-COMMAND"),
            ("Document.CloseAndDiscard", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.ApplicationServices.Document", "CloseAndDiscard", "void"), "R-COMMAND"),
            ("Database event handler", il => il.CallVirt(A, Db, "add_ObjectAppended", "void", "obj"), "R-EVENT"),
            ("Document event handler", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.ApplicationServices.Document", "add_CommandWillStart", "void", "obj"), "R-EVENT"),
            ("event unsubscribe", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.ApplicationServices.Application", "remove_Idle", "void", "obj"), "R-EVENT"),
            ("property write Entity.Layer", il => il.CallVirt(A, D + "Entity", "set_Layer", "void", "string"), "R-AUTODESK-SETTER"),
            ("property write Database.Clayer", il => il.CallVirt(A, Db, "set_Clayer", "void", Oid), "R-AUTODESK-SETTER"),
            ("WorkingDatabase setter", il => il.Call(A, D + "HostApplicationServices", "set_WorkingDatabase", "void", "obj"), "R-PRODUCT-DATABASE"),
            ("WorkingDatabase getter", il => il.Call(A, D + "HostApplicationServices", "get_WorkingDatabase", "obj"), "R-PRODUCT-DATABASE"),
            ("Document.Database of an open document", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.ApplicationServices.Document", "get_Database", "obj"), "R-PRODUCT-DATABASE"),
            ("BlockTableRecord.AppendEntity", il => il.CallVirt(A, D + "BlockTableRecord", "AppendEntity", "obj", "obj"), "R-AUTODESK-MUTATOR-NAME"),
            ("DBDictionary.SetAt", il => il.CallVirt(A, D + "DBDictionary", "SetAt", "obj", "string", "obj"), "R-AUTODESK-MUTATOR-NAME"),
            ("DBObject.Erase", il => il.CallVirt(A, D + "DBObject", "Erase", "void"), "R-AUTODESK-MUTATOR-NAME"),
            ("Entity.TransformBy", il => il.CallVirt(A, D + "Entity", "TransformBy", "void", "obj"), "R-AUTODESK-MUTATOR-NAME"),
            ("SymbolTable.Add", il => il.CallVirt(A, D + "SymbolTable", "Add", "obj", "obj"), "R-AUTODESK-MUTATOR-NAME"),
            ("DBObject.AddXData", il => il.CallVirt(A, D + "DBObject", "AddXData", "void", "obj"), "R-AUTODESK-MUTATOR-NAME"),
            ("new Line()", il => il.NewObj(A, D + "Line"), "R-NEWOBJ-OBJECT"),
            ("new BlockReference(...)", il => il.NewObj(A, D + "BlockReference", "obj", Oid), "R-NEWOBJ-OBJECT"),
            ("new BlockTableRecord()", il => il.NewObj(A, D + "BlockTableRecord"), "R-NEWOBJ-OBJECT"),
            ("new ResultBuffer()", il => il.NewObj(A, D + "ResultBuffer"), "R-NEWOBJ-OBJECT"),
            ("new RotatedDimension()", il => il.NewObj(A, D + "RotatedDimension"), "R-NEWOBJ-OBJECT"),
            ("Process.Start", il => il.Call("System.Diagnostics.Process", "System.Diagnostics.Process", "Start", "obj", "string"), "R-PROCESS"),
            ("Process.Kill", il => il.CallVirt("System.Diagnostics.Process", "System.Diagnostics.Process", "Kill", "void"), "R-PROCESS"),
            ("ProcessStartInfo", il => il.NewObj("System.Diagnostics.Process", "System.Diagnostics.ProcessStartInfo"), "R-PROCESS"),
            ("Registry write (RegistryKey.SetValue)", il => il.CallVirt("Microsoft.Win32.Registry", "Microsoft.Win32.RegistryKey", "SetValue", "void", "string", "obj"), "R-REGISTRY"),
            ("Registry write (Registry.SetValue)", il => il.Call("Microsoft.Win32.Registry", "Microsoft.Win32.Registry", "SetValue", "void", "string", "string", "obj"), "R-REGISTRY"),
            ("Registry key creation", il => il.CallVirt("Microsoft.Win32.Registry", "Microsoft.Win32.RegistryKey", "CreateSubKey", "obj", "string"), "R-REGISTRY"),
            ("network (HttpClient)", il => il.CallVirt("System.Net.Http", "System.Net.Http.HttpClient", "GetStringAsync", "obj", "string"), "R-NETWORK"),
            ("network (Socket)", il => il.NewObj("System.Net.Sockets", "System.Net.Sockets.TcpClient"), "R-NETWORK"),
            ("Assembly.LoadFrom", il => il.Call("System.Runtime", "System.Reflection.Assembly", "LoadFrom", "obj", "string"), "R-ASSEMBLY-LOAD"),
            ("Assembly.Load", il => il.Call("System.Runtime", "System.Reflection.Assembly", "Load", "obj", "string"), "R-ASSEMBLY-LOAD"),
            ("Assembly.UnsafeLoadFrom", il => il.Call("System.Runtime", "System.Reflection.Assembly", "UnsafeLoadFrom", "obj", "string"), "R-ASSEMBLY-LOAD"),
            ("AssemblyLoadContext", il => il.CallVirt("System.Runtime.Loader", "System.Runtime.Loader.AssemblyLoadContext", "LoadFromAssemblyPath", "obj", "string"), "R-ASSEMBLY-LOAD"),
            ("MethodBase.Invoke", il => il.CallVirt("System.Runtime", "System.Reflection.MethodBase", "Invoke", "obj", "obj", "obj"), "R-REFLECTION-INVOKE"),
            ("MethodInfo.Invoke", il => il.CallVirt("System.Runtime", "System.Reflection.MethodInfo", "Invoke", "obj", "obj", "obj"), "R-REFLECTION-INVOKE"),
            ("Activator.CreateInstance", il => il.Call("System.Runtime", "System.Activator", "CreateInstance", "obj", "obj"), "R-REFLECTION-INVOKE"),
            ("PropertyInfo.SetValue", il => il.CallVirt("System.Runtime", "System.Reflection.PropertyInfo", "SetValue", "void", "obj", "obj"), "R-REFLECTION-INVOKE"),
            ("Type.InvokeMember", il => il.CallVirt("System.Runtime", "System.Type", "InvokeMember", "obj", "string"), "R-REFLECTION-INVOKE"),
            ("Delegate.DynamicInvoke", il => il.CallVirt("System.Runtime", "System.Delegate", "DynamicInvoke", "obj", "obj"), "R-REFLECTION-INVOKE"),
            ("Environment.SetEnvironmentVariable", il => il.Call("System.Runtime", "System.Environment", "SetEnvironmentVariable", "void", "string", "string"), "R-ENVIRONMENT"),
            ("Environment.Exit", il => il.Call("System.Runtime", "System.Environment", "Exit", "void", "int"), "R-ENVIRONMENT"),
            ("Marshal.WriteInt32", il => il.Call("System.Runtime", "System.Runtime.InteropServices.Marshal", "WriteInt32", "void", "obj", "int"), "R-PINVOKE"),
            ("NativeLibrary.Load", il => il.Call("System.Runtime", "System.Runtime.InteropServices.NativeLibrary", "Load", "obj", "string"), "R-PINVOKE"),
            ("DynamicMethod", il => il.NewObj("System.Runtime", "System.Reflection.Emit.DynamicMethod"), "R-DYNAMIC-CODE"),
            ("AutoCAD COM interop", il => il.Call("Autodesk.AutoCAD.Interop", "Autodesk.AutoCAD.Interop.AcadApplication", "Quit", "void"), "R-AUTODESK-INTERNAL-OR-COM"),

            // ---- round 2 (two independent reviews): the seven calls the reviewer injected into the real R0 DLL, and their families ----------
            ("Database.DxfOut", il => il.CallVirt(A, Db, "DxfOut", "void", "string", "int", "int"), "R-DB-SAVE"),
            ("Database.DxfIn", il => il.CallVirt(A, Db, "DxfIn", "void", "string", "string"), "R-DB-SAVE"),
            ("Database.AttachXref", il => il.CallVirt(A, Db, "AttachXref", "obj", "string", "string"), "R-DB-SAVE"),
            ("Database.BindXrefs", il => il.CallVirt(A, Db, "BindXrefs", "void", "obj", "bool"), "R-DB-SAVE"),
            ("Database.ResolveXrefs", il => il.CallVirt(A, Db, "ResolveXrefs", "void", "bool", "bool"), "R-DB-SAVE"),
            ("DBObject.SwapIdWith", il => il.CallVirt(A, D + "DBObject", "SwapIdWith", "void", Oid, "bool", "bool"), "R-DB-SAVE"),
            ("DynamicLinker.LoadModule", il => il.CallVirt(A, "Autodesk.AutoCAD.Runtime.DynamicLinker", "LoadModule", "bool", "string", "bool", "bool"), "R-ASSEMBLY-LOAD"),
            ("Path.GetTempFileName", il => il.Call("System.Runtime", "System.IO.Path", "GetTempFileName", "string"), "R-FILE-WRITE"),
            ("XDocument.Save(path)", il => il.CallVirt("System.Xml.Linq", "System.Xml.Linq.XDocument", "Save", "void", "string"), "R-FILE-WRITE"),
            ("XmlWriter.Create", il => il.Call("System.Xml.ReaderWriter", "System.Xml.XmlWriter", "Create", "obj", "string"), "R-FILE-WRITE"),
            ("ZipFile.ExtractToDirectory", il => il.Call("System.IO.Compression.ZipFile", "System.IO.Compression.ZipFile", "ExtractToDirectory", "void", "string", "string"), "R-FILE-WRITE"),
            ("MemoryMappedFile.CreateFromFile", il => il.Call("System.IO.MemoryMappedFiles", "System.IO.MemoryMappedFiles.MemoryMappedFile", "CreateFromFile", "obj", "string"), "R-FILE-WRITE"),
            ("FileSystemWatcher", il => il.NewObj("System.IO.FileSystem.Watcher", "System.IO.FileSystemWatcher", "string").Pop(), "R-FILE-WRITE"),
            ("CultureInfo.DefaultThreadCurrentCulture setter", il => il.Call("System.Runtime", "System.Globalization.CultureInfo", "set_DefaultThreadCurrentCulture", "void", "obj"), "R-GLOBAL-STATE"),
            ("CultureInfo.CurrentCulture setter", il => il.Call("System.Runtime", "System.Globalization.CultureInfo", "set_CurrentCulture", "void", "obj"), "R-GLOBAL-STATE"),
            ("Environment.CurrentDirectory setter", il => il.Call("System.Runtime", "System.Environment", "set_CurrentDirectory", "void", "string"), "R-GLOBAL-STATE"),
            ("AppDomain.SetData", il => il.CallVirt("System.Runtime", "System.AppDomain", "SetData", "void", "string", "obj"), "R-GLOBAL-STATE"),
            ("Console.SetOut", il => il.Call("System.Console", "System.Console", "SetOut", "void", "obj"), "R-GLOBAL-STATE"),
            ("Thread.CurrentCulture setter", il => il.CallVirt("System.Threading.Thread", "System.Threading.Thread", "set_CurrentCulture", "void", "obj"), "R-GLOBAL-STATE"),
            ("Editor.Regen", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.EditorInput.Editor", "Regen", "void"), "R-AUTODESK-MUTATOR-NAME"),
            ("DBObject.WriteDwgFile", il => il.CallVirt(A, D + "DBObject", "WriteDwgFile", "void"), "R-DB-SAVE"),
            ("Runtime WriteProperty (non-database Autodesk namespace)", il => il.CallVirt(A, "Autodesk.AutoCAD.Runtime.RXObject", "WriteProperty", "void", "string", "obj"), "R-AUTODESK-MUTATOR-NAME"),
            ("PlottingServices member", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.PlottingServices.PlotEngine", "BeginPlot", "void"), "R-AUTODESK-MUTATOR-NAME"),
            ("Process.CloseMainWindow", il => il.CallVirt("System.Diagnostics.Process", "System.Diagnostics.Process", "CloseMainWindow", "bool"), "R-PROCESS"),
            ("Process.Refresh", il => il.CallVirt("System.Diagnostics.Process", "System.Diagnostics.Process", "Refresh", "void"), "R-PROCESS"),
            ("Delegate.CreateDelegate", il => il.Call("System.Runtime", "System.Delegate", "CreateDelegate", "obj", "obj", "obj"), "R-REFLECTION-INVOKE"),
            ("MethodInfo.CreateDelegate", il => il.CallVirt("System.Runtime", "System.Reflection.MethodInfo", "CreateDelegate", "obj", "obj"), "R-REFLECTION-INVOKE"),
            ("Expression.Compile", il => il.CallVirt("System.Linq.Expressions", "System.Linq.Expressions.Expression`1", "Compile", "obj"), "R-DYNAMIC-CODE"),
            ("dynamic binder (C# dynamic)", il => il.Call("Microsoft.CSharp", "Microsoft.CSharp.RuntimeBinder.Binder", "InvokeMember", "obj", "int", "string"), "R-DYNAMIC-CODE"),
            ("dynamic call site", il => il.Call("System.Linq.Expressions", "System.Runtime.CompilerServices.CallSite", "Create", "obj", "obj"), "R-DYNAMIC-CODE"),
            ("calli", il => il.Calli(), "R-CALLI"),
            ("jmp", il => il.Jmp("System.Runtime", "Fx.Other", "Target"), "R-JMP"),
            ("store to an external static field", il => il.LdcI4(1).StsFld("System.Runtime", "System.Some.Type", "Flag"), "R-EXTERNAL-FIELD-STORE"),

            // ---- round 3 (independent audit): raw memory, unsafe APIs, document manager extensions, threads ---------------------------------
            ("Marshal.Copy to an IntPtr", il => il.Call(Rt, Interop + "Marshal", "Copy", "void", "obj", "int", "intptr", "int"), "R-PINVOKE"),
            ("Marshal.StructureToPtr", il => il.Call(Rt, Interop + "Marshal", "StructureToPtr", "void", "obj", "intptr", "bool"), "R-PINVOKE"),
            ("Marshal.GetFunctionPointerForDelegate", il => il.Call(Rt, Interop + "Marshal", "GetFunctionPointerForDelegate", "intptr", "obj"), "R-PINVOKE"),
            ("Marshal.WriteIntPtr", il => il.Call(Rt, Interop + "Marshal", "WriteIntPtr", "void", "intptr", "intptr"), "R-PINVOKE"),
            ("Marshal.AllocHGlobal", il => il.Call(Rt, Interop + "Marshal", "AllocHGlobal", "intptr", "int"), "R-PINVOKE"),
            ("Marshal.GetDelegateForFunctionPointer", il => il.Call(Rt, Interop + "Marshal", "GetDelegateForFunctionPointer", "obj", "intptr"), "R-PINVOKE"),
            ("Buffer.MemoryCopy", il => il.Call(Rt, "System.Buffer", "MemoryCopy", "void", "void*", "void*", "long", "long"), "R-UNSAFE-API"),
            ("GCHandle.Alloc", il => il.Call(Rt, Interop + "GCHandle", "Alloc", "vt:System.Runtime|" + Interop + "GCHandle", "obj", "int"), "R-UNSAFE-API"),
            ("NativeMemory.Alloc", il => il.Call(Rt, Interop + "NativeMemory", "Alloc", "void*", "intptr"), "R-UNSAFE-API"),
            ("MemoryMarshal.GetReference", il => il.Call(Rt, Interop + "MemoryMarshal", "GetReference", "obj", "obj"), "R-UNSAFE-API"),
            ("Unsafe.Write", il => il.Call(Rt, "System.Runtime.CompilerServices.Unsafe", "Write", "void", "void*", "int"), "R-UNSAFE-API"),
            ("Unsafe.WriteUnaligned over a managed reference", il => il.Call(Rt, "System.Runtime.CompilerServices.Unsafe", "WriteUnaligned", "void", "obj", "int"), "R-UNSAFE-API"),
            ("Unsafe.As", il => il.Call(Rt, "System.Runtime.CompilerServices.Unsafe", "As", "obj", "obj"), "R-UNSAFE-API"),
            ("Unsafe.Add", il => il.Call(Rt, "System.Runtime.CompilerServices.Unsafe", "Add", "obj", "obj", "int"), "R-UNSAFE-API"),
            ("DocumentCollectionExtension.Open (DocumentManager.Open(string))", il => il.Call("AcMgd", "Autodesk.AutoCAD.ApplicationServices.DocumentCollectionExtension", "Open", "obj", "obj", "string"), "R-COMMAND"),
            ("DocumentCollectionExtension.Open with options", il => il.Call("AcMgd", "Autodesk.AutoCAD.ApplicationServices.DocumentCollectionExtension", "Open", "obj", "obj", "string", "bool"), "R-COMMAND"),
            ("DocumentCollection.CloseAll", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.ApplicationServices.DocumentCollection", "CloseAll", "void"), "R-COMMAND"),
            ("DocumentCollection.RecoverDocument prefix", il => il.CallVirt("AcMgd", "Autodesk.AutoCAD.ApplicationServices.DocumentCollection", "Recover", "void", "string"), "R-COMMAND"),
            ("new Thread", il => il.NewObj("System.Threading.Thread", "System.Threading.Thread", "obj").Pop(), "R-THREAD"),
            ("Thread.Start", il => il.CallVirt("System.Threading.Thread", "System.Threading.Thread", "Start", "void"), "R-THREAD"),
            ("new Timer", il => il.NewObj("System.Threading.Timer", "System.Threading.Timer", "obj", "obj", "int", "int").Pop(), "R-THREAD"),
            ("new System.Timers.Timer", il => il.NewObj("System.ComponentModel.TypeConverter", "System.Timers.Timer", "int").Pop(), "R-THREAD"),
            ("new Task", il => il.NewObj("System.Threading.Tasks", "System.Threading.Tasks.Task", "obj").Pop(), "R-THREAD"),
            ("Task.Run", il => il.Call("System.Threading.Tasks", "System.Threading.Tasks.Task", "Run", "obj", "obj"), "R-THREAD"),
            ("Task.Factory.StartNew", il => il.CallVirt("System.Threading.Tasks", "System.Threading.Tasks.TaskFactory", "StartNew", "obj", "obj"), "R-THREAD"),
            ("ThreadPool.QueueUserWorkItem", il => il.Call("System.Threading.ThreadPool", "System.Threading.ThreadPool", "QueueUserWorkItem", "bool", "obj"), "R-THREAD"),
            ("Parallel.For", il => il.Call("System.Threading.Tasks.Parallel", "System.Threading.Tasks.Parallel", "For", "obj", "int", "int", "obj"), "R-THREAD"),
        };
        return cases;
    }

    [Theory]
    [MemberData(nameof(Forbidden))]
    public void Negative_control_the_scan_fails_on_a_fixture_that_calls_the_forbidden_api(int index, string name)
    {
        var (caseName, body, rule) = Cases[index];
        Assert.Equal(name, caseName);
        var image = InType(body);
        Assert.Contains(rule, Rules(image, ScanRole.R0));
    }

    // ---- FIRST LAYER: deny-by-default. The same fixtures, scanned with the real allowed-apis.txt, must fail on the IMPORT itself ----------
    private static ApiAllowlist RealAllowlist() => ApiAllowlist.LoadFile(AllowlistPath());

    internal static string AllowlistPath() => Path.Combine(Support.RepoRoot(), "eng", "research", "I52Ct21dHostFacts", "allowed-apis.txt");

    [Theory]
    [MemberData(nameof(Forbidden))]
    public void Negative_control_the_deny_by_default_layer_refuses_the_import_of_every_forbidden_fixture(int index, string name)
    {
        var (caseName, body, _) = Cases[index];
        Assert.Equal(name, caseName);
        var image = InType(body);
        var violations = ForbiddenApiScan.Scan(image, "fixture.dll", ScanConfig.For(ScanRole.R0, RealAllowlist()));
        Assert.Contains(violations, v => v.Rule.StartsWith("R-IMPORT-", StringComparison.Ordinal) || v.Rule is "R-CALLI" or "R-JMP" or "R-EXTERNAL-FIELD-STORE" or "R-POINTER-TYPE");
    }

    [Fact]
    public void The_second_layer_rule_fires_for_each_family_even_when_the_import_would_be_approved()
    {
        // an allowlist that approves everything the fixture imports: only the deny rules can object
        foreach (var (name, body, rule) in Cases)
        {
            var image = InType(body);
            var items = ForbiddenApiScan.ListImports(image);
            var text = string.Join("\n", items.Select(i => ApiAllowlistTestText.Line(i.Kind, i.Item)));
            var violations = ForbiddenApiScan.Scan(image, "fixture.dll", ScanConfig.For(ScanRole.R0, ApiAllowlist.Parse(text + "\n")));
            Assert.True(violations.Any(v => v.Rule == rule), name + ": expected " + rule + " but got " + string.Join(",", violations.Select(v => v.Rule)));
            Assert.DoesNotContain(violations, v => v.Rule.StartsWith("R-IMPORT-", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Every_forbidden_api_family_of_design_5_3_has_a_negative_control()
    {
        var rules = Cases.Select(c => c.Rule).ToHashSet(StringComparer.Ordinal);
        foreach (var r in new[]
        {
            "R-DB-SAVE", "R-WRITE-OPEN", "R-OPENMODE-UNRESOLVABLE", "R-COMMIT", "R-TRANSACTION-SCOPE", "R-DB-CTOR", "R-READDWG-SCOPE", "R-SYSVAR-SET",
            "R-OVERRULE", "R-COMMAND", "R-EVENT", "R-AUTODESK-SETTER", "R-PRODUCT-DATABASE", "R-AUTODESK-MUTATOR-NAME", "R-NEWOBJ-OBJECT", "R-PROCESS",
            "R-REGISTRY", "R-NETWORK", "R-ASSEMBLY-LOAD", "R-REFLECTION-INVOKE", "R-ENVIRONMENT", "R-PINVOKE", "R-DYNAMIC-CODE", "R-AUTODESK-INTERNAL-OR-COM",
            "R-FILE-WRITE", "R-GLOBAL-STATE", "R-CALLI", "R-JMP", "R-EXTERNAL-FIELD-STORE", "R-UNSAFE-API", "R-THREAD",
        })
            Assert.Contains(r, rules);
    }

    // ---- structural rules: who may write a file, who may touch a database ---------------------------------------------------
    [Fact]
    public void File_writes_are_forbidden_everywhere_except_inside_the_single_EvidenceWriter()
    {
        var writes = new (string Type, string Member, string Ret, string[] Ps, bool Ctor)[]
        {
            ("System.IO.File", "WriteAllText", "void", new[] { "string", "string" }, false),
            ("System.IO.File", "WriteAllBytes", "void", new[] { "string", "obj" }, false),
            ("System.IO.File", "AppendAllText", "void", new[] { "string", "string" }, false),
            ("System.IO.File", "Create", "obj", new[] { "string" }, false),
            ("System.IO.File", "Open", "obj", new[] { "string", "int" }, false),
            ("System.IO.File", "OpenWrite", "obj", new[] { "string" }, false),
            ("System.IO.File", "Delete", "void", new[] { "string" }, false),
            ("System.IO.File", "Copy", "void", new[] { "string", "string" }, false),
            ("System.IO.File", "Move", "void", new[] { "string", "string" }, false),
            ("System.IO.File", "Replace", "void", new[] { "string", "string", "string" }, false),
            ("System.IO.File", "SetAttributes", "void", new[] { "string", "int" }, false),
            ("System.IO.Directory", "CreateDirectory", "obj", new[] { "string" }, false),
            ("System.IO.Directory", "Delete", "void", new[] { "string" }, false),
            ("System.IO.FileStream", ".ctor", "void", new[] { "string", "int" }, true),
            ("System.IO.StreamWriter", ".ctor", "void", new[] { "string" }, true),
            ("System.IO.FileInfo", "Delete", "void", Array.Empty<string>(), false),
            ("System.IO.FileInfo", "CreateText", "obj", Array.Empty<string>(), false),
            ("System.IO.FileInfo", "MoveTo", "void", new[] { "string" }, false),
            ("System.IO.FileInfo", "set_IsReadOnly", "void", new[] { "bool" }, false),
        };
        foreach (var role in new[] { ScanRole.Core, ScanRole.Tools, ScanRole.R0 })
            foreach (var w in writes)
            {
                var image = InType(il =>
                {
                    if (w.Ctor) il.NewObj("System.Runtime", w.Type, w.Ps);
                    else il.Call("System.Runtime", w.Type, w.Member, w.Ret, w.Ps);
                });
                Assert.True(Rules(image, role).Contains("R-FILE-WRITE"), role + " " + w.Type + "::" + w.Member);
            }
    }

    [Fact]
    public void The_exact_EvidenceWriter_of_the_core_assembly_may_write_and_nobody_else_may_pretend_to_be_it()
    {
        void Body(FixtureBuilder.Il il)
        {
            il.Call("System.Runtime", "System.IO.File", "WriteAllText", "void", "string", "string");
            // the only shapes the writer may use: create-new + write + no sharing, and setting the read-only attribute (layer 3 checks the arguments)
            il.LdcI4(1).LdcI4(2).LdcI4(0).NewObj("System.Runtime", "System.IO.FileStream", "string", "vt:System.Runtime|System.IO.FileMode",
                "vt:System.Runtime|System.IO.FileAccess", "vt:System.Runtime|System.IO.FileShare").Pop();
            il.LdcI4(1).Call("System.Runtime", "System.IO.File", "SetAttributes", "void", "string", "vt:System.Runtime|System.IO.FileAttributes");
        }
        // the real privilege holder: right namespace, right name, in the core assembly
        var legit = InType(Body, "I52Ct21d.HostFacts.Core", "EvidenceWriter", "I52Ct21d.HostFacts.Core");
        Assert.Empty(Rules(legit, ScanRole.Core));
        // the same type defined in another assembly is refused as a redefinition, and its writes are not privileged either way
        var rogue = InType(Body, "I52Ct21d.HostFacts.Core", "EvidenceWriter", "Some.Other.Assembly");
        Assert.Contains("R-WRITER-REDEFINED", Rules(rogue, ScanRole.Core));
        // a look-alike in another namespace gets no privilege
        var lookalike = InType(Body, "Evil", "EvidenceWriter", "Some.Other.Assembly");
        Assert.Contains("R-FILE-WRITE", Rules(lookalike, ScanRole.Core));
        // the privilege is that of the exact top-level type (its nested closures included); a sibling type has none
        var fb = new FixtureBuilder("I52Ct21d.HostFacts.Core");
        fb.Type("I52Ct21d.HostFacts.Core", "EvidenceWriter").Method("M", Body);
        fb.Type("I52Ct21d.HostFacts.Core", "EvidenceWriterHelper").Method("M", Body);
        Assert.Contains("R-FILE-WRITE", Rules(fb.Build(), ScanRole.Core));
    }

    [Fact]
    public void The_database_privilege_belongs_only_to_the_R0_side_database_reader()
    {
        void Body(FixtureBuilder.Il il)
        {
            il.LdcI4(0).LdcI4(1).NewObj(A, Db, "bool", "bool").Pop();
            il.CallVirt(A, Db, "ReadDwgFile", "void", "string", Fs, "bool", "string");
            il.CallVirt(A, D + "TransactionManager", "StartOpenCloseTransaction", "obj");
        }
        Assert.Empty(Rules(InType(Body, SideNs, SideType, SideAsm), ScanRole.R0));
        // the same type name defined in ANOTHER assembly is a redefinition and gets no privilege either
        var spoofed = Rules(InType(Body, SideNs, SideType, "Evil.Assembly"), ScanRole.R0);
        Assert.Contains("R-SIDEDB-REDEFINED", spoofed);
        Assert.Contains("R-DB-CTOR", spoofed);
        Assert.Contains("R-READDWG-SCOPE", spoofed);
        Assert.Contains("R-TRANSACTION-SCOPE", spoofed);
        Assert.Contains("R-DB-CTOR", Rules(InType(Body, SideNs, "OtherReader", SideAsm), ScanRole.R0));
        Assert.Contains("R-READDWG-SCOPE", Rules(InType(Body, SideNs, "OtherReader", SideAsm), ScanRole.R0));
        Assert.Contains("R-TRANSACTION-SCOPE", Rules(InType(Body, SideNs, "OtherReader", SideAsm), ScanRole.R0));
        Assert.Contains("R-SIDEDB-REDEFINED", Rules(InType(Body, SideNs, SideType, SideAsm), ScanRole.Core));
        // a look-alike in another namespace gets nothing
        Assert.Contains("R-DB-CTOR", Rules(InType(Body, "Evil", SideType, SideAsm), ScanRole.R0));
    }

    [Fact]
    public void Core_and_tools_may_not_reference_AutoCAD_at_all()
    {
        var image = InType(il => il.CallVirt(A, D + "Entity", "get_Layer", "string"));
        foreach (var role in new[] { ScanRole.Core, ScanRole.Tools })
        {
            var rules = Rules(image, role);
            Assert.Contains("R-AUTODESK-IN-NONHOST", rules);
        }
        Assert.Empty(Rules(image, ScanRole.R0));
    }

    [Fact]
    public void A_type_that_derives_from_an_Overrule_is_refused_and_so_is_a_pinvoke_declaration()
    {
        var fb = new FixtureBuilder("Fixture");
        fb.Type("Fx", "MyOverrule", "AcDbMgd|Autodesk.AutoCAD.GraphicsInterface.DrawableOverrule").Method("M", il => il.Ret());
        Assert.Contains("R-OVERRULE", Rules(fb.Build()));

        var fb2 = new FixtureBuilder("Fixture");
        fb2.Type("Fx", "Native").PInvoke("GetTickCount", "kernel32.dll");
        Assert.Contains("R-PINVOKE", Rules(fb2.Build()));
    }

    [Fact]
    public void The_scan_reports_the_location_of_each_violation_and_is_deterministic()
    {
        var image = InType(il => il.CallVirt(A, Db, "Save", "void").CallVirt(A, Tr, "Commit", "void").LdcI4(1).CallVirt(A, Tr, "GetObject", "obj", Oid, Om));
        var a = ForbiddenApiScan.Scan(image, "f.dll", ScanConfig.For(ScanRole.R0));
        var b = ForbiddenApiScan.Scan(image, "f.dll", ScanConfig.For(ScanRole.R0));
        Assert.Equal(a.Select(v => v.ToString()), b.Select(v => v.ToString()));
        Assert.All(a, v => { Assert.Equal("Fx.Violator", v.Type); Assert.Equal("M", v.Method); Assert.True(v.IlOffset >= 0); });
        Assert.Equal(new[] { "R-COMMIT", "R-DB-SAVE", "R-WRITE-OPEN" }, a.Select(v => v.Rule).Distinct().Where(r => r != "R-AUTODESK-MUTATOR-NAME").OrderBy(x => x, StringComparer.Ordinal).ToArray());
    }

    // ---- THE REAL ASSEMBLIES --------------------------------------------------------------------------------------------------
    [Fact]
    public void The_real_core_assembly_is_clean_and_the_scan_really_examined_it()
    {
        var outcome = ForbiddenApiScan.ScanDetailed(File.ReadAllBytes(typeof(Core.EvidenceWriter).Assembly.Location), "core", ScanConfig.For(ScanRole.Core, RealAllowlist()));
        Assert.Empty(outcome.Violations);
        Assert.True(outcome.ImportRowsChecked > 150, "import rows examined: " + outcome.ImportRowsChecked);
        Assert.True(outcome.MethodBodies > 100, "method bodies examined: " + outcome.MethodBodies);
        Assert.True(outcome.ExternalCallSites > 500, "external call sites examined: " + outcome.ExternalCallSites);
        Assert.Equal(0, outcome.AutodeskCallSites);
    }

    [Fact]
    public void The_real_tools_assembly_is_clean_and_the_scan_really_examined_it()
    {
        var outcome = ForbiddenApiScan.ScanDetailed(File.ReadAllBytes(typeof(Tools.Program).Assembly.Location), "tools", ScanConfig.For(ScanRole.Tools, RealAllowlist()));
        Assert.Empty(outcome.Violations);
        Assert.True(outcome.ImportRowsChecked > 100, "import rows examined: " + outcome.ImportRowsChecked);
        Assert.True(outcome.MethodBodies > 30, "method bodies examined: " + outcome.MethodBodies);
        Assert.Equal(0, outcome.AutodeskCallSites);
    }

#if HAVE_R0
#if DEBUG
    private const string Cfg = "Debug";
#else
    private const string Cfg = "Release";
#endif

    private static string R0Path() => Path.Combine(Support.RepoRoot(), "eng", "research", "I52Ct21dHostFacts", "r0", "bin", Cfg, "net8.0-windows", "I52Ct21d.HostFacts.R0.dll");

    [Fact]
    public void The_real_R0_assembly_is_clean_and_the_scan_really_examined_its_AutoCAD_call_sites()
    {
        var outcome = ForbiddenApiScan.ScanDetailed(File.ReadAllBytes(R0Path()), "r0", ScanConfig.For(ScanRole.R0, RealAllowlist()));
        Assert.Empty(outcome.Violations);
        Assert.True(outcome.ImportRowsChecked > 100, "import rows examined: " + outcome.ImportRowsChecked);
        Assert.True(outcome.AutodeskCallSites >= 40, "AutoCAD call sites examined: " + outcome.AutodeskCallSites);
        Assert.True(outcome.OpenModeCallSites >= 8, "OpenMode call sites resolved: " + outcome.OpenModeCallSites);
    }

    [Fact]
    public void Mutation_control_patching_a_ForRead_constant_of_the_real_R0_to_ForWrite_makes_the_scan_fail()
    {
        var image = File.ReadAllBytes(R0Path());
        var patched = PatchBeforeCall(image, "GetObject", from: 0x16, to: 0x17); // ldc.i4.0 -> ldc.i4.1
        Assert.True(patched > 0, "no GetObject call site patched");
        var rules = Rules(image, ScanRole.R0, "mutated-r0.dll");
        Assert.Contains("R-WRITE-OPEN", rules);
    }

    [Fact]
    public void Mutation_control_patching_the_side_database_constructor_arguments_of_the_real_R0_makes_the_scan_fail()
    {
        var image = File.ReadAllBytes(R0Path());
        // new Database(false, true) is ldc.i4.0, ldc.i4.1, newobj: turn the second constant into ldc.i4.0 -> new Database(false, false)
        var patched = PatchBeforeCall(image, ".ctor", from: 0x17, to: 0x16, onlyDeclaring: "Database");
        Assert.True(patched > 0, "the Database constructor call site was not found");
        Assert.Contains("R-DB-CTOR", Rules(image, ScanRole.R0, "mutated-r0.dll"));
    }

    [Fact]
    public void Imports_layer_control_the_real_R0_fails_against_a_list_missing_one_approval_and_every_entry_is_used()
    {
        var all = ApiAllowlist.LoadFile(AllowlistPath());
        var r0 = File.ReadAllBytes(R0Path());
        // dropping the approval of Database::ReadDwgFile makes the real R0 fail on that import
        var without = ApiAllowlist.Parse(string.Join("\n", File.ReadAllLines(AllowlistPath()).Where(l => !l.StartsWith("M|Autodesk.AutoCAD.DatabaseServices.Database::ReadDwgFile|", StringComparison.Ordinal))) + "\n");
        Assert.Equal(all.Entries.Count - 1, without.Entries.Count);
        var v = ForbiddenApiScan.Scan(r0, "r0", ScanConfig.For(ScanRole.R0, without));
        Assert.Contains(v, x => x.Rule == "R-IMPORT-MEMBER" && x.Detail.Contains("ReadDwgFile"));

        // scanning the three real assemblies marks every entry of the list as used: the list is minimal
        var list = ApiAllowlist.LoadFile(AllowlistPath());
        ForbiddenApiScan.Scan(File.ReadAllBytes(typeof(Core.EvidenceWriter).Assembly.Location), "core", ScanConfig.For(ScanRole.Core, list));
        ForbiddenApiScan.Scan(File.ReadAllBytes(typeof(Tools.Program).Assembly.Location), "tools", ScanConfig.For(ScanRole.Tools, list));
        ForbiddenApiScan.Scan(r0, "r0", ScanConfig.For(ScanRole.R0, list));
        Assert.Empty(list.Unused());
    }

    /// <summary>Patches, in place, the single-byte instruction that precedes each call to <paramref name="member"/>.</summary>
    private static int PatchBeforeCall(byte[] image, string member, byte from, byte to, string? onlyDeclaring = null)
    {
        using var pe = new System.Reflection.PortableExecutable.PEReader(System.Collections.Immutable.ImmutableArray.Create(image));
        var md = pe.GetMetadataReader();
        var patched = 0;
        foreach (var th in md.TypeDefinitions)
            foreach (var mh in md.GetTypeDefinition(th).GetMethods())
            {
                var m = md.GetMethodDefinition(mh);
                if (m.RelativeVirtualAddress == 0) continue;
                var body = pe.GetMethodBody(m.RelativeVirtualAddress);
                var il = body.GetILBytes()!;
                var code = IlReader.Decode(il);
                var block = pe.GetSectionData(m.RelativeVirtualAddress).GetContent(0, 2);
                var headerSize = (block[0] & 3) == 2 ? 1 : (block[1] >> 4) * 4; // tiny header: 1 byte; fat header: size in dwords in the high nibble of byte 1
                var ilStartRva = m.RelativeVirtualAddress + headerSize;
                for (var i = 1; i < code.Count; i++)
                {
                    if (!IlReader.IsCall(code[i])) continue;
                    var handle = System.Reflection.Metadata.Ecma335.MetadataTokens.EntityHandle((int)code[i].Operand);
                    if (handle.Kind != System.Reflection.Metadata.HandleKind.MemberReference) continue;
                    var mr = md.GetMemberReference((System.Reflection.Metadata.MemberReferenceHandle)handle);
                    if (md.GetString(mr.Name) != member) continue;
                    if (onlyDeclaring is not null && !(mr.Parent.Kind == System.Reflection.Metadata.HandleKind.TypeReference
                        && md.GetString(md.GetTypeReference((System.Reflection.Metadata.TypeReferenceHandle)mr.Parent).Name) == onlyDeclaring)) continue;
                    var prev = code[i - 1];
                    if (il[prev.Offset] != from) continue;
                    var rva = ilStartRva + prev.Offset;
                    foreach (var s in pe.PEHeaders.SectionHeaders)
                        if (rva >= s.VirtualAddress && rva < s.VirtualAddress + s.VirtualSize)
                        {
                            image[rva - s.VirtualAddress + s.PointerToRawData] = to;
                            patched++;
                        }
                }
            }
        return patched;
    }
#endif
}

internal static class ApiAllowlistTestText
{
    public static string Line(AllowKind kind, string item) => AllowEntry.KindLetter(kind) + "|" + item + "||approved only for this negative control test";
}
