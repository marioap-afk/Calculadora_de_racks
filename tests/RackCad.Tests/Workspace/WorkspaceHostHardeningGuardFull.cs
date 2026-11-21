#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T3C1-HOST-HARDENING (INV-F4T3C1-01, 03, 04, 05, 07, 08, 10, 12; section 4 of the package): the source guards of the hardening of the
// host of the pilot. The Core test project cannot reference the Plugin nor load WPF, so the sources are read as text. Each guard is a function of
// a text that returns its violations; it is run on the real source and, by the body of a member, on a synthetic text that it must accept and on
// mutations of it that it must reject. The catch blocks of GetRacksAtRisk and of the two queues are unreachable today (D-T3C1-01, D-T3C1-03), so
// these guards are the whole of their coverage: no seam, no conversion and no reflection reaches them.
public class FullPanelCopyWorkspaceHostHardeningGuardRedTests
{
    private const string HostPath = Src.ApplicationWorkspace + "WorkspaceHostCore.cs";
    private const string SupportPath = Src.ApplicationWorkspace + "WorkspacePilotSupport.cs";

    private static int Count(string text, string pattern) => Regex.Matches(text, pattern).Count;

    // ------------------------------------------------------------------ H-07: the 30 literals of the port of the pilot are in Spanish

    // The table of the package (H-07, and the two rows A1-01 and A1-02 of F4-T4A1): the literal in English, its Spanish text and how many times it
    // stands in the host and the support. Since F4-T4A1 the three operations of the reconciliation reuse the texts of the others, and the count of each
    // one is the EXACT number of its occurrences now (the value before the task is in the comment of each row that changed).
    private static readonly (string English, string Spanish, int Times)[] Table =
    {
        ("A read of the pilot is in progress.", "Hay una lectura del piloto en curso.", 1),
        ("The request does not belong to the active document.", "La petición no es del documento activo.", 6),
        ("There is no draft for the rack.", "No hay borrador para el rack.", 3),
        ("The preview witness has no key.", "El testigo de la vista previa no tiene clave.", 6), // before 3
        ("The drawing is not ready to be read.", "El dibujo no está listo para leerse.", 6), // before 3
        ("The read of the edit intention failed.", "Falló la lectura de la intención de edición.", 1),
        ("The read of the edit intention carries no result.", "La lectura de la intención de edición no trajo resultado.", 1),
        ("The session was retired.", "La sesión se retiró.", 1),
        ("A blank rack has no draft.", "Un rack sin identificador no tiene borrador.", 2), // before 1
        ("The draft is not unknown after a commit that threw.", "El borrador no quedó desconocido tras una aplicación fallida.", 2),
        ("The reread failed.", "Falló la relectura.", 2), // before 1
        ("The reread carries no result.", "La relectura no trajo resultado.", 0), // before 2 (I-64 F4B PB-T4b, owner-pbt4b-list-478-decision.md)
        ("The draft does not need to be reloaded.", "El borrador no necesita recargarse.", 1),
        ("The read for the reload failed.", "Falló la lectura para la recarga.", 3), // before 1
        ("The read for the reload carries no result.", "La lectura para la recarga no trajo resultado.", 3), // before 1
        ("The draft is being applied or is no longer registered.", "El borrador se está aplicando o ya no está registrado.", 6), // before 5 (I-64 F4B PB-T4b, owner-pbt4b-list-478-decision.md)
        ("The draft was not reloaded.", "El borrador no se recargó.", 3), // before 1
        ("The state of the field could not be read.", "No se pudo leer el estado del campo.", 1),
        ("The text is required.", "El texto es obligatorio.", 1),
        ("The field cannot be edited now.", "El campo no se puede editar ahora.", 4),
        ("The boundary is not known.", "La frontera no se conoce.", 1),
        ("The rack has no annotation scale.", "El rack no tiene escala de anotación.", 0), // before 1 (I-64 F4B PB-T4b, owner-pbt4b-list-478-decision.md)
        ("No reader of the edit intention is assigned.", "No hay un lector de la intención de edición asignado.", 1),
        ("There is no active document.", "No hay documento activo.", 1),
        ("A RackCad window is open.", "Hay una ventana de RackCad abierta.", 1),
        ("A command is in progress.", "Hay un comando en curso.", 1),
        ("The value is not a finite number greater than zero.", "El valor no es un número finito mayor que cero.", 1),
        ("A notice of the panel is being delivered.", "Se está entregando un aviso del panel.", 1),
        // The two rows that F4-T4A1 adds (table T-A1 of the package), each one declared as a private constant of the host.
        ("The draft cannot be checked in its current status.", "El borrador no admite comprobación en su estado actual.", 1),
        ("Only a draft in conflict or with edits marked as possibly stale is kept.", "Solo se conserva un borrador en conflicto o con cambios marcados como posiblemente obsoletos.", 1),
    };

    // The four identifiers that stay (two held by the guards of the port, two by decision) with the Spanish text each one carries.
    private static readonly (string Pattern, string Name)[] Identifiers =
    {
        (@"private\s+const\s+string\s+PilotReadInProgressReason\s*=\s*""Hay una lectura del piloto en curso\.""\s*;", "PilotReadInProgressReason"),
        (@"private\s+const\s+string\s+NotThisDocumentReason\s*=\s*""La petición no es del documento activo\.""\s*;", "NotThisDocumentReason"),
        (@"private\s+const\s+string\s+NoDraftReason\s*=\s*""No hay borrador para el rack\.""\s*;", "NoDraftReason"),
        (@"public\s+const\s+string\s+InsideNoticeReason\s*=\s*""Se está entregando un aviso del panel\.""\s*;", "InsideNoticeReason"),
    };

    // The substrings that the guards of the port forbid in these files (G-04), compared without case: the Spanish texts carry none.
    private static readonly Regex ForbiddenSubstrings = new(
        @"Transaction|Database|Commit\(|RackLog|Provider|Metric|LockDocument|Aggregat|\bBom|TotalRacks|ProjectSummary|ComputedParameter|proveedor|metrica|métrica",
        RegexOptions.IgnoreCase);

    private static IEnumerable<string> LiteralViolations(string host, string support)
    {
        var all = host + "\n" + support;
        if (Table.Length != 30) yield return "the table does not have its 30 literals";

        foreach (var (english, spanish, times) in Table)
        {
            if (all.Contains("\"" + english + "\"", StringComparison.Ordinal)) yield return "the literal in English stays: " + english;
            var found = Regex.Matches(all, Regex.Escape("\"" + spanish + "\"")).Count;
            if (found != times) yield return "the Spanish text of '" + english + "' stands " + found + " times; it must stand " + times;
            if (ForbiddenSubstrings.IsMatch(spanish)) yield return "the Spanish text carries a forbidden substring: " + spanish;
        }

        foreach (var (pattern, name) in Identifiers)
            if (Count(all, pattern) != 1) yield return "the identifier " + name + " is not preserved with its Spanish text";

        // They stay in English by decision: the text of the record and the one of the differences.
        if (!Regex.IsMatch(host, @"DescribeLoss[\s\S]*?Uncertain racks")) yield return "DescribeLoss changed";
        if (!host.Contains("(variable registry)", StringComparison.Ordinal)) yield return "DescribeDifferences changed";
    }

    [Fact]
    public void NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept()
    {
        Assert.Equal(30, Table.Length);
        Assert.Equal(30, Table.Select(t => t.English).Distinct().Count());
        Assert.Equal(30, Table.Select(t => t.Spanish).Distinct().Count());

        Assert.Empty(LiteralViolations(Src.Code(HostPath), Src.Code(SupportPath)));

        // I-64 F4B PB-T4b (CA-T4b I-01; owner-pbt4b-list-478-decision.md): the self-test of the guard (WHHG:501-526), over the table of this
        // copy. In the GREEN the original self-test passes vacuously: its table (2, 5 and 1) no longer matches the host (0, 6 and 0), so
        // LiteralViolations already answers three violations without any mutation. Here the host is clean for this table (the Assert.Empty
        // above), so each of the 13 mutations has to make the guard trip.
        var host = Src.Code(HostPath);
        var support = Src.Code(SupportPath);

        // An English literal that comes back, in the host and in the support.
        Assert.NotEmpty(LiteralViolations(host + " \"The reread failed.\"", support));
        Assert.NotEmpty(LiteralViolations(host, support + " \"A notice of the panel is being delivered.\""));
        // A Spanish text that goes away (here, replaced by another), and one that is repeated.
        Assert.NotEmpty(LiteralViolations(host.Replace("\"Falló la relectura.\"", "\"Otra cosa.\"", StringComparison.Ordinal), support));
        Assert.NotEmpty(LiteralViolations(host + " \"Falló la relectura.\"", support));
        // A lost identifier.
        Assert.NotEmpty(LiteralViolations(host.Replace("NoDraftReason", "NoBorradorReason", StringComparison.Ordinal), support));
        Assert.NotEmpty(LiteralViolations(host, support.Replace("InsideNoticeReason", "DentroDelAvisoReason", StringComparison.Ordinal)));
        // The two new texts: an English original that comes back, and a Spanish text that goes away.
        Assert.NotEmpty(LiteralViolations(host + " \"The draft cannot be checked in its current status.\"", support));
        Assert.NotEmpty(LiteralViolations(host + " \"Only a draft in conflict or with edits marked as possibly stale is kept.\"", support));
        Assert.NotEmpty(LiteralViolations(host.Replace("El borrador no admite comprobación en su estado actual.", "Otra cosa.", StringComparison.Ordinal), support));
        Assert.NotEmpty(LiteralViolations(host.Replace("Solo se conserva un borrador en conflicto", "Se conserva un borrador en conflicto", StringComparison.Ordinal), support));
        // A reused text that one operation of the reconciliation stops using changes its exact count.
        Assert.NotEmpty(LiteralViolations(host.Replace("\"El testigo de la vista previa no tiene clave.\"", "\"Otro.\"", StringComparison.Ordinal), support));
        // The text of the record or of the differences changes.
        Assert.NotEmpty(LiteralViolations(host.Replace("(variable registry)", "(registro de variables)", StringComparison.Ordinal), support));
        Assert.NotEmpty(LiteralViolations(host.Replace("Uncertain racks", "Racks inciertos", StringComparison.Ordinal), support));
    }
}
