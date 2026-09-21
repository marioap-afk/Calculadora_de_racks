using System.Text.Json;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: I52G3AApiCensus <repository-root> <output-json>");
    return 2;
}

var root = Path.GetFullPath(args[0]);
var output = Path.GetFullPath(args[1]);
var projectPath = Path.Combine(root, "src", "RackCad.Plugin", "RackCad.Plugin.csproj");

MSBuildLocator.RegisterDefaults();
using var workspace = MSBuildWorkspace.Create(new Dictionary<string, string>
{
    ["AutoCADInstallDir"] = @"C:\Program Files\Autodesk\AutoCAD 2025",
    ["UseAutoCADNuGetReferences"] = "false",
});

var diagnostics = new List<string>();
workspace.WorkspaceFailed += (_, e) => diagnostics.Add(e.Diagnostic.ToString());
var project = await workspace.OpenProjectAsync(projectPath);
var compilation = await project.GetCompilationAsync();
if (compilation is null)
{
    Console.Error.WriteLine("Could not compile RackCad.Plugin.");
    return 3;
}

var compilationErrors = compilation.GetDiagnostics()
    .Where(d => d.Severity == DiagnosticSeverity.Error)
    .Select(d => d.ToString())
    .ToArray();
if (compilationErrors.Length != 0)
{
    foreach (var error in compilationErrors)
    {
        Console.Error.WriteLine(error);
    }

    return 4;
}

var sites = new List<Site>();
foreach (var document in project.Documents.Where(d => d.FilePath is not null))
{
    var tree = await document.GetSyntaxTreeAsync();
    var syntax = await document.GetSyntaxRootAsync();
    if (tree is null || syntax is null)
    {
        continue;
    }

    var model = compilation.GetSemanticModel(tree);
    foreach (var node in syntax.DescendantNodes())
    {
        ISymbol? symbol = null;
        string? kind = null;

        switch (node)
        {
            case InvocationExpressionSyntax invocation:
                symbol = BestSymbol(model.GetSymbolInfo(invocation));
                kind = "METHOD";
                break;
            case ObjectCreationExpressionSyntax creation:
                symbol = BestSymbol(model.GetSymbolInfo(creation));
                kind = "CONSTRUCTOR";
                break;
            case ImplicitObjectCreationExpressionSyntax implicitCreation:
                symbol = BestSymbol(model.GetSymbolInfo(implicitCreation));
                kind = "CONSTRUCTOR";
                break;
            case MemberAccessExpressionSyntax memberAccess
                when memberAccess.Parent is not InvocationExpressionSyntax { Expression: var expression }
                     || expression != memberAccess:
                symbol = BestSymbol(model.GetSymbolInfo(memberAccess));
                kind = AccessKind(memberAccess, symbol);
                break;
            case BinaryExpressionSyntax binary:
                symbol = BestSymbol(model.GetSymbolInfo(binary));
                kind = "OPERATOR";
                break;
            case PrefixUnaryExpressionSyntax prefix:
                symbol = BestSymbol(model.GetSymbolInfo(prefix));
                kind = "OPERATOR";
                break;
            case PostfixUnaryExpressionSyntax postfix:
                symbol = BestSymbol(model.GetSymbolInfo(postfix));
                kind = "OPERATOR";
                break;
        }

        if (symbol is null || !IsAutoCad(symbol))
        {
            continue;
        }

        var span = tree.GetLineSpan(node.Span);
        var enclosing = model.GetEnclosingSymbol(node.SpanStart);
        var relative = Path.GetRelativePath(root, document.FilePath!).Replace('\\', '/');
        var classification = Classify(symbol, kind!, node);
        sites.Add(new Site(
            relative,
            span.StartLinePosition.Line + 1,
            span.StartLinePosition.Character + 1,
            kind!,
            symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
            enclosing?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) ?? "<global>",
            node.ToString(),
            classification.Status,
            classification.Reason));
    }
}

sites = sites
    .Distinct()
    .OrderBy(s => s.Path, StringComparer.Ordinal)
    .ThenBy(s => s.Line)
    .ThenBy(s => s.Column)
    .ThenBy(s => s.Symbol, StringComparer.Ordinal)
    .ToList();

var report = new Report(
    DateTimeOffset.UtcNow,
    projectPath,
    sites.Count,
    sites.Count(s => s.Classification == "UNCLASSIFIED"),
    sites,
    diagnostics);

Directory.CreateDirectory(Path.GetDirectoryName(output)!);
await File.WriteAllTextAsync(output, JsonSerializer.Serialize(report, new JsonSerializerOptions
{
    WriteIndented = true,
}));

Console.WriteLine($"TOTAL={report.Total}");
Console.WriteLine($"UNCLASSIFIED={report.Unclassified}");
Console.WriteLine($"OUTPUT={output}");
return 0;

static ISymbol? BestSymbol(SymbolInfo info) => info.Symbol ?? info.CandidateSymbols.FirstOrDefault();

static bool IsAutoCad(ISymbol symbol)
{
    var assembly = symbol.ContainingAssembly?.Name;
    if (assembly is not null &&
        (assembly.Equals("AcCoreMgd", StringComparison.OrdinalIgnoreCase) ||
         assembly.Equals("AcDbMgd", StringComparison.OrdinalIgnoreCase) ||
         assembly.Equals("AcMgd", StringComparison.OrdinalIgnoreCase)))
    {
        return true;
    }

    return symbol.ContainingNamespace?.ToDisplayString().StartsWith("Autodesk.AutoCAD", StringComparison.Ordinal) == true;
}

static string AccessKind(MemberAccessExpressionSyntax access, ISymbol? symbol)
{
    if (symbol is IEventSymbol)
    {
        return access.Parent is AssignmentExpressionSyntax assignment && assignment.IsKind(SyntaxKind.AddAssignmentExpression)
            ? "EVENT_ADD"
            : "EVENT_REMOVE";
    }

    if (access.Parent is AssignmentExpressionSyntax parent && parent.Left.Span.Contains(access.Span))
    {
        return "PROPERTY_OR_FIELD_SET";
    }

    if (access.Parent is PrefixUnaryExpressionSyntax or PostfixUnaryExpressionSyntax)
    {
        return "PROPERTY_OR_FIELD_SET";
    }

    return "PROPERTY_OR_FIELD_GET";
}

static Classification Classify(ISymbol symbol, string kind, SyntaxNode node)
{
    if (kind == "PROPERTY_OR_FIELD_SET" || kind is "EVENT_ADD" or "EVENT_REMOVE")
    {
        return new("DECLARED_MUTATOR", "Explicit AutoCAD property/field mutation or event subscription boundary.");
    }

    if (kind == "PROPERTY_OR_FIELD_GET" || kind == "OPERATOR")
    {
        if (symbol.Name == "ForWrite")
        {
            return new("DECLARED_MUTATOR", "Explicit write-open marker.");
        }

        return new("READ_ONLY", "Value observation, enum/constant access, or pure operator.");
    }

    if (kind == "CONSTRUCTOR" && symbol is IMethodSymbol constructor)
    {
        if (InheritsFrom(constructor.ContainingType, "Autodesk.AutoCAD.DatabaseServices.DBObject") ||
            constructor.ContainingType.ToDisplayString() == "Autodesk.AutoCAD.DatabaseServices.Database")
        {
            return new("DECLARED_MUTATOR", "Construction of a database-resident object or database container.");
        }

        return new("EXCLUDED_WITH_REASON", "Construction of a value, prompt, mapping, or transient collection object.");
    }

    if (symbol is not IMethodSymbol method)
    {
        return new("UNCLASSIFIED", "No classification rule for this symbol kind.");
    }

    if (method.Name == "GetObject")
    {
        return node.ToString().Contains("ForWrite", StringComparison.Ordinal)
            ? new("DECLARED_MUTATOR", "Object is opened ForWrite at this call site.")
            : new("READ_ONLY", "Object is opened ForRead at this call site.");
    }

    var declaringType = method.ContainingType.ToDisplayString();
    if (declaringType.StartsWith("Autodesk.AutoCAD.Geometry.", StringComparison.Ordinal))
    {
        return new("READ_ONLY", "Pure geometry calculation.");
    }

    if (declaringType.StartsWith("Autodesk.AutoCAD.Colors.", StringComparison.Ordinal))
    {
        return new("READ_ONLY", "Pure color value construction.");
    }

    if (declaringType.StartsWith("Autodesk.AutoCAD.EditorInput.", StringComparison.Ordinal))
    {
        if (method.Name is "Regen" or "SetCurrentView")
        {
            return new("DECLARED_MUTATOR", "Editor presentation state mutation.");
        }

        return new("EXCLUDED_WITH_REASON", "Prompt, selection, jig, or command-line UX outside database materialization.");
    }

    if (declaringType.StartsWith("Autodesk.AutoCAD.ApplicationServices.", StringComparison.Ordinal))
    {
        if (method.Name is "LockDocument")
        {
            return new("DECLARED_MUTATOR", "Document write/serialization boundary.");
        }

        return new("EXCLUDED_WITH_REASON", "Application/document lookup or UI operation outside database materialization.");
    }

    if (method.Name is "AppendEntity" or "AddNewlyCreatedDBObject" or "Commit" or "Add" or "SetAt" or
        "Erase" or "UpgradeOpen" or "CreateExtensionDictionary" or "AdjustAlignment" or
        "RecomputeDimensionBlock" or "RecordGraphicsModified" or "DeepCloneObjects" or
        "WblockCloneObjects" or "ReadDwgFile" or "StartTransaction" or "StartOpenCloseTransaction" or
        "AddVertexAt")
    {
        return new("DECLARED_MUTATOR", "Known creation, write-open, clone, transaction, or graphics mutation boundary.");
    }

    var signature = method.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    if (IsKnownReadOnlyQuery(signature))
    {
        return new("READ_ONLY", "Exact reviewed query/observation with no drawing mutation at this call site; no name-prefix inference.");
    }

    if (method.Name == "Dispose")
    {
        return new("EXCLUDED_WITH_REASON", "Deterministic release of a managed AutoCAD wrapper.");
    }

    return new("UNCLASSIFIED", "No semantic rule matched this AutoCAD API invocation.");
}

static bool IsKnownReadOnlyQuery(string signature) => signature is
    "Autodesk.AutoCAD.DatabaseServices.BlockTableRecord.GetBlockReferenceIds(bool, bool)" or
    "Autodesk.AutoCAD.DatabaseServices.Curve.GetPointAtParameter(double)" or
    "Autodesk.AutoCAD.DatabaseServices.Database.Purge(Autodesk.AutoCAD.DatabaseServices.ObjectIdCollection)" or
    "Autodesk.AutoCAD.DatabaseServices.DBDictionary.Contains(string)" or
    "Autodesk.AutoCAD.DatabaseServices.DBDictionary.GetAt(string)" or
    "Autodesk.AutoCAD.DatabaseServices.Handle.ToString()" or
    "Autodesk.AutoCAD.DatabaseServices.Polyline.GetBulgeAt(int)" or
    "Autodesk.AutoCAD.DatabaseServices.Polyline.GetPoint2dAt(int)" or
    "Autodesk.AutoCAD.DatabaseServices.SymbolTable.Has(string)" or
    "Autodesk.AutoCAD.DatabaseServices.SymbolUtilityServices.GetBlockModelSpaceId(Autodesk.AutoCAD.DatabaseServices.Database)";

static bool InheritsFrom(ITypeSymbol? type, string fullName)
{
    for (var current = type; current is not null; current = current.BaseType)
    {
        if (current.ToDisplayString() == fullName)
        {
            return true;
        }
    }

    return false;
}

internal sealed record Site(
    string Path,
    int Line,
    int Column,
    string Kind,
    string Symbol,
    string EnclosingMember,
    string Expression,
    string Classification,
    string Reason);

internal sealed record Classification(string Status, string Reason);

internal sealed record Report(
    DateTimeOffset GeneratedAtUtc,
    string Project,
    int Total,
    int Unclassified,
    IReadOnlyList<Site> Sites,
    IReadOnlyList<string> WorkspaceDiagnostics);
