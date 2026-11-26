using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Workspace;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>The window asks AutoCAD to read the BOM of the drawing of the session (T4 bom, BM-01); the number ties the answer to this request.</summary>
    public sealed record RackBomRequest(long RequestId, SessionId Session);

    /// <summary>What the reading says of ONE rack placed in the drawing (T4 bom): quoted, or why not.</summary>
    public enum RackBomRackState
    {
        /// <summary>The rack has its BOM and enters the total.</summary>
        Quoted,

        /// <summary>A blocking diagnostic of its own authority (I-42): it cannot be quoted, and there is no total.</summary>
        Blocked,

        /// <summary>A link to a project variable that does not resolve (I-47 G13): there is no total.</summary>
        BrokenLink,

        /// <summary>A kind with no handler in this build: a total without it would look complete, so there is no total.</summary>
        UnknownKind,

        /// <summary>Its views do not describe one design (I-47 G13): it stays OUT of the total, which is said.</summary>
        NoSingleDesign,

        /// <summary>Its design cannot be read (I-42/H11): it stays OUT of the total, which is said.</summary>
        Unreadable,
    }

    /// <summary>
    /// One rack placed in the drawing as the page «BOM y lista de racks» shows it (T4 bom): the identity, the name and the system with the
    /// words of the side column (BM-07), its views, its copies (the maximum of direct references among its views, as RACKBOMTOTAL counts
    /// them), its state and, when it is quoted, its BOM of ONE copy.
    /// </summary>
    public sealed record RackBomRack(
        string RackId,
        string Name,
        string Kind,
        string KindLabel,
        string Views,
        int Copies,
        RackBomRackState State,
        string Reason,
        BillOfMaterials Bom)
    {
        public bool IsQuoted => State == RackBomRackState.Quoted && Bom != null;

        /// <summary>The rack aborts the total, as it aborts RACKBOMTOTAL (BM-05).</summary>
        public bool BlocksTheTotal => State == RackBomRackState.Blocked || State == RackBomRackState.BrokenLink || State == RackBomRackState.UnknownKind;

        /// <summary>The rack stays out of the total, which is still made (the warning of RACKBOMTOTAL, now on the page).</summary>
        public bool IsOutOfTheTotal => State == RackBomRackState.NoSingleDesign || State == RackBomRackState.Unreadable;
    }

    /// <summary>A line of a warning of the page: the rack (null for a definition that is no rack) and the reason.</summary>
    public sealed record RackBomNotice(string RackId, string Name, string Reason);

    /// <summary>
    /// What AutoCAD read for the page «BOM y lista de racks», for the request of that number (T4 bom): the racks placed in the drawing with
    /// their BOM, what aborts the total (<see cref="Refusals"/>) and the total of the drawing, or why nothing could be read
    /// (<see cref="Failure"/>). <see cref="DrawingPath"/> is the file of the drawing (null when it was never saved) and
    /// <see cref="DrawingName"/> its name without the extension, for the names of the exported files (BM-03).
    /// </summary>
    public sealed record RackBomReading(
        long RequestId,
        string DrawingPath,
        string DrawingName,
        string Failure,
        IReadOnlyList<RackBomRack> Racks,
        IReadOnlyList<RackBomNotice> Refusals,
        ConsolidatedBom Total)
    {
        public bool IsRead => Failure == null;

        /// <summary>The racks that stay out of the total, with their reason.</summary>
        public IReadOnlyList<RackBomNotice> Skipped =>
            (Racks ?? Array.Empty<RackBomRack>()).Where(r => r.IsOutOfTheTotal).Select(r => new RackBomNotice(r.RackId, r.Name, r.Reason)).ToList();

        /// <summary>A reading that could not read the drawing at all.</summary>
        public static RackBomReading Failed(long requestId, string drawingPath, string drawingName, string failure) =>
            new(requestId, drawingPath, drawingName, string.IsNullOrWhiteSpace(failure) ? "No se pudo leer el dibujo." : failure.Trim(),
                Array.Empty<RackBomRack>(), Array.Empty<RackBomNotice>(), null);
    }

    /// <summary>
    /// The door of the RackCad window to the BOM of the drawing (T4 bom, BM-01 and BM-11): the read is a command of AutoCAD with no undo
    /// marker (RACKBOMLEER), with its number and «Dejar de esperar» (<see cref="IRackCadRackDoorPort.Abandon"/>), and the same rules as the
    /// door to a rack: only without a command in progress, in the drawing of the request, the focus back to the drawing. RACKLISTA and
    /// RACKBOMTOTAL ask the window to show the page and read (<see cref="BomPageAsked"/>), instead of opening their classic windows.
    /// </summary>
    public interface IRackCadBomPort : IRackCadRackDoorPort
    {
        /// <summary>Asks AutoCAD to read the BOM of the drawing (RACKBOMLEER); the answer arrives with <see cref="BomRead"/>.</summary>
        RackCadClassicResult RequestBomRead(RackBomRequest request);

        event Action<RackBomReading> BomRead;

        /// <summary>
        /// RACKLISTA (its tab «Racks del dibujo») or RACKBOMTOTAL (its tab «BOM total») ran: the window shows «BOM y lista de racks» and reads
        /// the drawing as soon as it is free.
        /// </summary>
        event Action<RackBomTab> BomPageAsked;
    }

    /// <summary>The two tabs of the page «BOM y lista de racks».</summary>
    public enum RackBomTab
    {
        Racks,
        Total,
    }

    /// <summary>One block definition of the drawing that carries RackCad data, as the scan of AutoCAD saw it: plain values.</summary>
    public sealed record RackBomViewFact(string DefinitionId, string BlockName, RackEmbedDocument Embed, int DirectReferenceCount);

    /// <summary>What the reading needs of the handler of a kind: its label, the gate of I-42 and its BOM (the handler of the Plugin).</summary>
    public interface IRackBomKind
    {
        /// <summary>The label of the kind in the total and in the exported files («Selectivo», «Push Back»…).</summary>
        string Label { get; }

        /// <summary>Why the rack cannot be quoted (a blocking diagnostic of its own authority), or null.</summary>
        string BlockedReason(RackEmbedDocument embed);

        /// <summary>The BOM of ONE copy of the rack, typed (I-47 G13).</summary>
        BomBuildResult Build(RackEmbedDocument embed, ProjectVariablesDocument variables);
    }

    /// <summary>
    /// The reading of the page «BOM y lista de racks» (T4 bom), pure: the rules of RACKBOMTOTAL (I-42, I-47 G13) over the facts of the scan,
    /// with what the page adds. ONE read of the register for every rack; a rack is quoted from one view only once all its views say the
    /// same; a definition that is placed and cannot be classified, a blocked rack, a broken link and a kind with no handler abort the
    /// TOTAL (a total missing a rack cannot look complete) and are named with their reason; a rack whose views differ or whose design cannot
    /// be read stays out of the total, which is said. Unlike RACKBOMTOTAL, the page still shows the BOM of every rack that can be quoted when
    /// the total is refused (BM-05), and only racks with copies are listed (BM-06).
    /// </summary>
    public static class RackBomReadingRules
    {
        /// <summary>The command of AutoCAD that reads the BOM for the window, with no undo marker and no history.</summary>
        public const string CommandName = "RACKBOMLEER";

        /// <summary>The order of the systems and their words, those of the side column of the window (BM-05, BM-07).</summary>
        public static readonly IReadOnlyList<(string Kind, string Label)> KindOrder = new[]
        {
            (RackEmbedDocument.KindSelective, "Selectivo"),
            (RackEmbedDocument.KindPushBack, "Push Back"),
            (RackEmbedDocument.KindDynamic, "Dinámico"),
            (RackEmbedDocument.KindCantilever, "Cantilever"),
            (RackEmbedDocument.KindCabecera, "Cabecera"),
            (RackEmbedDocument.KindCama, "Cama"),
        };

        /// <summary>The word of the side column for a kind of the envelope; its raw name for any other.</summary>
        public static string KindLabelOf(string kind)
        {
            foreach (var (known, label) in KindOrder)
                if (string.Equals(known, kind?.Trim(), StringComparison.OrdinalIgnoreCase)) return label;
            return string.IsNullOrWhiteSpace(kind) ? "Sin tipo" : kind.Trim();
        }

        /// <summary>The position of a kind in the order of the side column; the others go after.</summary>
        public static int KindRank(string kind)
        {
            for (var i = 0; i < KindOrder.Count; i++)
                if (string.Equals(KindOrder[i].Kind, kind?.Trim(), StringComparison.OrdinalIgnoreCase)) return i;
            return KindOrder.Count;
        }

        /// <param name="views">Every definition of the drawing with RackCad data, in the order of the block table.</param>
        /// <param name="registry">The register of project variables, read ONCE for the whole reading.</param>
        /// <param name="kindOf">The handler of a kind of the envelope; null when the kind has none.</param>
        public static RackBomReading Compose(
            long requestId,
            string drawingPath,
            string drawingName,
            IReadOnlyList<RackBomViewFact> views,
            ProjectVariablesReadResult registry,
            Func<string, IRackBomKind> kindOf)
        {
            if (registry == null)
                return RackBomReading.Failed(requestId, drawingPath, drawingName, "No se pudo leer el registro de variables del dibujo.");

            // A register that EXISTS and cannot be read is not an empty one: quoting against «empty» would price every bound rack at its
            // frozen literal (RACKBOMTOTAL aborts here too).
            if (registry.Outcome != ProjectVariablesReadOutcome.Absent && registry.Outcome != ProjectVariablesReadOutcome.Readable)
                return RackBomReading.Failed(requestId, drawingPath, drawingName,
                    "No hay BOM: el registro de variables del dibujo existe y no se puede leer. " + (registry.Error ?? string.Empty).Trim());

            var refusals = new List<RackBomNotice>();
            var siblings = new Dictionary<string, List<ProjectVariableScanEntry>>(StringComparer.OrdinalIgnoreCase);
            var embeds = new Dictionary<string, RackEmbedDocument>(StringComparer.Ordinal);
            var copies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var order = new List<string>();

            foreach (var view in views ?? Array.Empty<RackBomViewFact>())
            {
                if (view == null) continue;
                var entry = ProjectVariableScanProjection.Project(view.DefinitionId, view.Embed, view.DirectReferenceCount);
                if (!entry.OuterEnvelopeInterpretable)
                {
                    // Defined but not placed: it is not in the drawing, so it cannot be missing from the total. Placed, it aborts the total:
                    // without identity there is no way to show it is not part of a rack that IS quoted. A RackId is never invented.
                    if (entry.DirectReferenceCount > 0)
                        refusals.Add(new RackBomNotice(null, DefinitionLabel(view), UnclassifiableText));
                    continue;
                }

                if (!siblings.TryGetValue(entry.RackId, out var group))
                {
                    group = new List<ProjectVariableScanEntry>();
                    siblings[entry.RackId] = group;
                    copies[entry.RackId] = 0;
                    order.Add(entry.RackId);
                }

                group.Add(entry);
                embeds[entry.DefinitionId] = view.Embed;
                if (entry.DirectReferenceCount > copies[entry.RackId]) copies[entry.RackId] = entry.DirectReferenceCount;
            }

            var racks = new List<RackBomRack>();
            var quoted = new List<ConsolidatedRackBom>();
            foreach (var rackId in order)
            {
                // Only PLACED racks (BM-06): a definition with no copy was drawn and erased, not yet purged; the side column does not list it.
                if (copies[rackId] <= 0) continue;

                var group = siblings[rackId];
                var described = RackListBuilder.Build(group.Select(s => embeds[s.DefinitionId])).FirstOrDefault();
                var name = described?.Name ?? "(sin nombre)";
                var kind = group[0].Kind;
                var views0 = described?.ViewsLabel ?? string.Empty;
                RackBomRack Rack(RackBomRackState state, string reason, BillOfMaterials bom) =>
                    new(rackId, name, kind, KindLabelOf(kind), views0, copies[rackId], state, reason, bom);

                IRackBomKind handler = null;
                try
                {
                    handler = kindOf?.Invoke(kind);
                }
                catch (Exception)
                {
                    handler = null;
                }

                if (handler == null)
                {
                    racks.Add(Rack(RackBomRackState.UnknownKind,
                        "su tipo («" + (kind ?? string.Empty) + "») no lo reconoce esta versión de RackCad: no se puede cotizar.", null));
                    continue;
                }

                var authority = BomAuthoredAuthority.Resolve(rackId, group);
                if (!authority.IsSuccess)
                {
                    racks.Add(Rack(RackBomRackState.NoSingleDesign,
                        "no tiene un diseño único entre sus vistas y queda fuera del total." + Detail(authority.Error), null));
                    continue;
                }

                var embed = embeds[authority.RepresentativeDefinitionId];
                string blocked;
                try
                {
                    blocked = handler.BlockedReason(embed);
                }
                catch (Exception ex)
                {
                    racks.Add(Rack(RackBomRackState.Unreadable, "su diseño no se pudo interpretar y queda fuera del total." + Detail(ex.Message), null));
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(blocked))
                {
                    racks.Add(Rack(RackBomRackState.Blocked, blocked.Trim(), null));
                    continue;
                }

                BomBuildResult result;
                try
                {
                    result = handler.Build(embed, registry.Document);
                }
                catch (Exception ex)
                {
                    result = BomBuildResult.UnreadablePayload(embed?.Id, embed?.Name, ex.Message);
                }

                if (result == null || result.Outcome == BomBuildOutcome.UnreadablePayload || result.Outcome == BomBuildOutcome.NoAuthoredAuthority
                    || (result.IsSuccess && result.Bom == null))
                {
                    racks.Add(Rack(RackBomRackState.Unreadable, "su diseño no se pudo interpretar y queda fuera del total." + Detail(result?.Error), null));
                    continue;
                }

                if (result.Outcome == BomBuildOutcome.BrokenProjectVariableReference)
                {
                    // NOT a skip: the design is readable and the VARIABLE is what is missing, so a total without this rack would look complete.
                    racks.Add(Rack(RackBomRackState.BrokenLink,
                        "está vinculado a una variable de proyecto que no se puede resolver (propiedad '"
                        + (string.IsNullOrWhiteSpace(result.PropertyId) ? "(desconocida)" : result.PropertyId) + "', variable '"
                        + (string.IsNullOrWhiteSpace(result.VariableId) ? "(sin id)" : result.VariableId)
                        + "'). Repara el vínculo antes de cotizar." + Detail(result.Error), null));
                    continue;
                }

                racks.Add(Rack(RackBomRackState.Quoted, null, result.Bom));
                quoted.Add(new ConsolidatedRackBom { Name = name, Kind = handler.Label, Copies = copies[rackId], Bom = result.Bom });
            }

            refusals.InsertRange(0, racks.Where(r => r.BlocksTheTotal).Select(r => new RackBomNotice(r.RackId, r.Name, r.Reason)));
            var total = refusals.Count == 0 && quoted.Count > 0 ? ConsolidatedBomBuilder.Build(quoted) : null;
            return new RackBomReading(requestId, drawingPath, drawingName, null, racks, refusals, total);
        }

        /// <summary>
        /// What RACKBOMLEER says on the command line once it answered: the page of the window says the rest (the warnings are there now,
        /// no longer only on the command line: dolor 3 of the inventory).
        /// </summary>
        public static string CommandLineText(RackBomReading reading)
        {
            if (reading == null) return "RackCad: no se leyó el BOM.";
            if (!reading.IsRead) return "RackCad: no hay BOM del dibujo (ver la ventana RackCad). " + reading.Failure;
            var listed = reading.Racks?.Count ?? 0;
            if (reading.Refusals.Count > 0)
            {
                return "RackCad: BOM leído, sin total del dibujo: " + (reading.Refusals.Count == 1
                    ? "1 rack tiene un problema que impide cotizarlo"
                    : reading.Refusals.Count.ToString(CultureInfo.InvariantCulture) + " racks tienen problemas que impiden cotizarlos")
                    + " (ver la ventana RackCad).";
            }

            var skipped = reading.Skipped.Count;
            return "RackCad: BOM leído para la ventana RackCad: " + Count(listed, "rack", "racks")
                   + (skipped == 0 ? "." : "; " + Count(skipped, "queda fuera", "quedan fuera") + " del total (ver la ventana RackCad).");
        }

        private static string Count(int count, string one, string many) =>
            count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);

        private const string UnclassifiableText =
            "está colocada en el dibujo y lleva datos de RackCad que esta versión no puede interpretar: no se sabe a qué rack pertenece, así que no se puede saber si falta en el total.";

        private static string DefinitionLabel(RackBomViewFact view) =>
            "La definición '" + (view.DefinitionId ?? "(desconocida)") + "'"
            + (string.IsNullOrWhiteSpace(view.BlockName) ? string.Empty : " (bloque «" + view.BlockName.Trim() + "»)");

        private static string Detail(string detail) => string.IsNullOrWhiteSpace(detail) ? string.Empty : " " + detail.Trim();
    }

    /// <summary>
    /// The exported files of the page (T4 bom, BM-03): saved next to the drawing with an automatic name, «&lt;dibujo&gt; - BOM.xlsx» for the
    /// drawing and «&lt;dibujo&gt; - &lt;rack&gt; - BOM.xlsx» for a rack, with «(borrador)» when it is the BOM of the draft of the editor (BM-02).
    /// A character a file name cannot carry becomes «_».
    /// </summary>
    public static class RackBomFiles
    {
        public const string Excel = ".xlsx";

        public const string Csv = ".csv";

        /// <summary>The name of the file of the BOM of the whole drawing.</summary>
        public static string DrawingFileName(string drawingName, string extension) => Clean(drawingName, "Dibujo") + " - BOM" + extension;

        /// <summary>The name of the file of the BOM of one rack, of the drawing or of the draft of its editor.</summary>
        public static string RackFileName(string drawingName, string rackName, bool draft, string extension) =>
            Clean(drawingName, "Dibujo") + " - " + Clean(rackName, "Rack") + " - BOM" + (draft ? " (borrador)" : string.Empty) + extension;

        /// <summary>The folder of the drawing: where the files go; null for a drawing that was never saved (no folder).</summary>
        public static string FolderOf(string drawingPath)
        {
            if (string.IsNullOrWhiteSpace(drawingPath)) return null;
            try
            {
                if (!Path.IsPathRooted(drawingPath)) return null;
                var folder = Path.GetDirectoryName(drawingPath);
                return string.IsNullOrWhiteSpace(folder) ? null : folder;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>The name of a drawing for its files: the file without its extension.</summary>
        public static string NameOf(string drawingPathOrName)
        {
            if (string.IsNullOrWhiteSpace(drawingPathOrName)) return null;
            try
            {
                var name = Path.GetFileNameWithoutExtension(drawingPathOrName.Trim());
                return string.IsNullOrWhiteSpace(name) ? null : name;
            }
            catch (ArgumentException)
            {
                return drawingPathOrName.Trim();
            }
        }

        private static string Clean(string part, string fallback)
        {
            var text = (part ?? string.Empty).Trim();
            if (text.Length == 0) return fallback;
            var invalid = Path.GetInvalidFileNameChars();
            var chars = text.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
            var cleaned = new string(chars).Trim().TrimEnd('.');
            return cleaned.Length == 0 ? fallback : cleaned;
        }
    }

    /// <summary>The texts of the page that name a moment: «10:42».</summary>
    public static class RackBomClock
    {
        public static string Time(DateTime at) => at.ToString("H:mm", CultureInfo.InvariantCulture);
    }
}
