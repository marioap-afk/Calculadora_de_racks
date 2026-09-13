using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 G4 — fixtures del consumidor Selectivo del nucleo compartido.
    ///
    /// <para>
    /// Todo pasa por las rutas reales: el estado del editor (<see cref="SelectiveEditorState"/>), su diseno
    /// (<c>BuildDesign</c>) y el resolver de geometria. <see cref="Editor"/> hace de "recompute" con una generacion
    /// monotona: es la mitad de Application de lo que la ventana aporta en G5 con <c>lastSystem</c> y su contador.
    /// </para>
    /// <para>
    /// Las huellas del estado son de SOLO LECTURA —ninguna llama a <c>BuildDesign</c>, <c>SyncPostCabeceras</c> ni a
    /// ningun normalizador—, porque sirven justamente para demostrar que PREPARE no muta nada.
    /// </para>
    /// </summary>
    internal static class SelectiveHeaderBatchFixtures
    {
        internal const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        internal const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;
        internal const double RunPeralte = 3.0;

        internal static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        internal readonly struct FondoSpec
        {
            internal FondoSpec(int frentes, int levels, double depth)
            {
                Frentes = frentes;
                Levels = levels;
                Depth = depth;
            }

            internal int Frentes { get; }

            internal int Levels { get; }

            internal double Depth { get; }
        }

        internal static FondoSpec Fondo(int frentes, int levels = 2, double depth = 48.0) => new FondoSpec(frentes, levels, depth);

        /// <summary>Un estado de varios fondos, cada uno con sus frentes, niveles y fondo de tarima, sincronizado como lo
        /// deja el editor: fondo 0 en edicion y los objetivos siguiendo al fondo actual (el comportamiento historico).</summary>
        internal static SelectiveEditorState State(params FondoSpec[] fondos)
        {
            var state = new SelectiveEditorState { DefaultBeamId = BeamId };
            foreach (var fondo in fondos)
            {
                state.InitMatrix(fondo.Frentes, fondo.Levels);
                state.FondoMatrices.Add(state.SnapshotWorking(fondo.Depth, 0.0));
            }

            state.SelectedFondo = 0;
            state.LoadFondo(0);
            state.FollowCurrentFondo();
            state.SyncPostCabeceras();
            return state;
        }

        internal static SelectiveEditorState State(params int[] frentesPorFondo)
            => State(frentesPorFondo.Select(frentes => Fondo(frentes)).ToArray());

        internal static SelectiveDesignInputs Inputs(SelectiveEditorState state, double runPeralte = RunPeralte)
            => new SelectiveDesignInputs
            {
                PostId = PostId,
                PostPeralte = runPeralte,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                Fondo = state.FondoMatrices[0].Depth,
                DepthCount = state.FondoMatrices.Count,
                WorkingDepth = state.FondoMatrices[state.SelectedFondo].Depth,
                WorkingCabeceraOverride = state.FondoMatrices[state.SelectedFondo].CabeceraOverride,
                Separators = new List<double>()
            };

        /// <summary>Una cabecera personalizada reconocible por su altura, de la fabrica real.</summary>
        internal static RackFrameConfiguration Custom(double height, double depth = 42.0)
            => new RackFrameConfigurationFactory(Catalog).Build(
                RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, height, depth);

        /// <summary>Guarda una personalizada en <c>(fondo, poste)</c> directamente en las filas del estado, como quedaria
        /// tras cargar un documento; no normaliza nada.</summary>
        internal static void StoreCustom(SelectiveEditorState state, int fondo, int post, RackFrameConfiguration configuration)
        {
            List<RackFrameConfiguration> row;
            if (fondo == 0)
            {
                row = state.PostCabeceras;
            }
            else
            {
                while (state.ExtraFondoPostCabeceras.Count < fondo) state.ExtraFondoPostCabeceras.Add(new List<RackFrameConfiguration>());
                row = state.ExtraFondoPostCabeceras[fondo - 1];
            }

            while (row.Count <= post) row.Add(null);
            row[post] = configuration;
        }

        internal static SelectiveHeaderAddress At(int fondo, int post) => new SelectiveHeaderAddress(fondo, post);

        internal static RackFrameConfiguration DeepCopy(RackFrameConfiguration configuration)
            => new RackFrameProjectStore().DeepCopy(configuration);

        /// <summary>
        /// El editor: un estado y su recompute (<c>BuildDesign</c> + resolver) con una generacion que crece en cada
        /// recompute. <see cref="Current"/> es la resolucion vigente tal como G5 la construira.
        /// </summary>
        internal sealed class Editor
        {
            internal Editor(SelectiveEditorState state)
            {
                State = state;
            }

            internal SelectiveEditorState State { get; }

            internal double RunPeralte { get; set; } = SelectiveHeaderBatchFixtures.RunPeralte;

            internal SelectiveRackSystem System { get; private set; }

            internal long Generation { get; private set; }

            internal SelectiveHeaderResolution Current => SelectiveHeaderResolution.Of(System, Generation, recomputePendingOrDeferred: false);

            internal SelectiveHeaderResolution Recompute()
            {
                var design = State.BuildDesign(Inputs(State, RunPeralte));
                Assert.NotNull(design);
                System = new SelectiveGeometryResolver().Resolve(design, Catalog);
                Generation++;
                return Current;
            }

            internal SelectivePalletDesign Design() => State.BuildDesign(Inputs(State, RunPeralte));

            internal SelectiveHeaderBatchPreparation Prepare(SelectiveHeaderBatchRequest request)
                => SelectiveHeaderBatchPlanner.Prepare(State, Current, request);

            internal HeaderBatchOutcome<SelectiveHeaderAddress> Mutate(SelectiveHeaderBatchPreparation preparation)
                => State.ApplyHeaderBatch(preparation, Current);
        }

        internal static SelectiveHeaderBatchRequest Distribute(SelectiveHeaderAddress source, params int[] posts)
            => SelectiveHeaderBatchRequest.Distribute(source, posts);

        internal static HeaderBatchPlan<SelectiveHeaderAddress>.Prepared Prepared(SelectiveHeaderBatchPreparation preparation)
        {
            Assert.NotNull(preparation);
            Assert.Null(preparation.PreconditionFailure);
            return Assert.IsType<HeaderBatchPlan<SelectiveHeaderAddress>.Prepared>(preparation.Plan);
        }

        internal static HeaderRejectionCode RejectedCode(SelectiveHeaderBatchPreparation preparation)
        {
            Assert.NotNull(preparation);
            Assert.Null(preparation.PreconditionFailure);
            return Assert.IsType<HeaderBatchPlan<SelectiveHeaderAddress>.Rejected>(preparation.Plan).Code;
        }

        internal static HeaderBatchOutcome<SelectiveHeaderAddress>.Committed Committed(HeaderBatchOutcome<SelectiveHeaderAddress> outcome)
            => Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(outcome);

        internal static HeaderRejectionCode RejectedCode(HeaderBatchOutcome<SelectiveHeaderAddress> outcome)
            => Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Rejected>(outcome).Code;

        /// <summary>Las copias preparadas, con la premisa de que existen y estan alineadas 1:1 con los Targets.</summary>
        internal static IReadOnlyList<RackFrameConfiguration> Copies(SelectiveHeaderBatchPreparation preparation)
        {
            var plan = Prepared(preparation);
            Assert.NotNull(preparation.PreparedCopies);
            Assert.Equal(plan.Targets.Count, preparation.PreparedCopies.Count);
            return preparation.PreparedCopies;
        }

        // ---- Huellas de solo lectura -------------------------------------------------------------------------------

        private static string R(double value) => value.ToString("R", CultureInfo.InvariantCulture);

        private static string R(double? value) => value.HasValue ? R(value.Value) : "null";

        /// <summary>
        /// El estado observable completo del editor: seleccion, objetivos, matriz viva, cada slot, cada cabecera (su
        /// proyeccion persistida, su derivado y su runtime) y los peraltes por poste. No llama a nada que mute.
        /// </summary>
        internal static string StateFingerprint(SelectiveEditorState state)
        {
            var text = new StringBuilder();
            text.Append("sel=").Append(state.SelectedFondo).Append('/').Append(state.SelBay).Append('/').Append(state.SelLevel)
                .Append(";objetivos=").Append(state.TargetMode).Append(':').Append(string.Join(",", state.TargetFondos.Fondos))
                .Append(";posiciones=").Append(string.Join(",", state.SelectedPositions()));
            Matrix(text, "W", state.Bays, state.FloorBeams, state.BayHeights, state.FloorBeamRiseOverrides, state.BaySegments);
            for (var k = 0; k < state.FondoMatrices.Count; k++)
            {
                var matrix = state.FondoMatrices[k];
                text.Append(";F").Append(k).Append(":depth=").Append(R(matrix.Depth)).Append(",override=").Append(R(matrix.CabeceraOverride));
                Matrix(text, "F" + k, matrix.Bays, matrix.FloorBeams, matrix.BayHeights, matrix.FloorBeamRiseOverrides, matrix.BaySegments);
            }

            text.Append(";cab0=").Append(Row(state.PostCabeceras));
            for (var k = 0; k < state.ExtraFondoPostCabeceras.Count; k++)
            {
                text.Append(";cab").Append(k + 1).Append('=').Append(Row(state.ExtraFondoPostCabeceras[k]));
            }

            text.Append(";peraltes=").Append(string.Join(",", state.PostPeraltes.Select(R)));
            return text.ToString();
        }

        private static void Matrix(
            StringBuilder text,
            string label,
            List<List<SelectiveEditorCell>> bays,
            List<bool> floorBeams,
            List<double?> heights,
            List<double?> rises,
            List<List<SelectiveSegment>> segments)
        {
            text.Append(';').Append(label).Append(":bays=");
            text.Append(string.Join("#", bays.Select(column => string.Join("|", column.Select(cell =>
                string.Join("/", R(cell.Frente), R(cell.Alto), cell.PalletCount, cell.BeamId, R(cell.BeamPeralte), R(cell.BeamLength), R(cell.Clear)))))));
            text.Append(",piso=").Append(string.Join(",", floorBeams));
            text.Append(",altos=").Append(string.Join(",", heights.Select(R)));
            text.Append(",elev=").Append(string.Join(",", rises.Select(R)));
            text.Append(",tramos=").Append(string.Join("#", segments.Select(row => string.Join("|", row.Select(s => R(s.Length) + ":" + s.Loaded)))));
        }

        private static string Row(IEnumerable<RackFrameConfiguration> row)
            => row == null ? "null" : "[" + string.Join("|", row.Select(Configuration)) + "]";

        /// <summary>Una cabecera por su proyeccion persistida, su derivado y su runtime; "std" para la estandar.</summary>
        internal static string Configuration(RackFrameConfiguration configuration)
            => configuration == null
                ? "std"
                : HeaderConfigurationFixtures.Wire(configuration)
                  + "#M:" + string.Join("/", HeaderConfigurationFixtures.MembersFingerprint(configuration))
                  + "#E:" + string.Join("/", HeaderConfigurationFixtures.ExceptionsFingerprint(configuration));

        /// <summary>Cada cabecera guardada, por referencia y en orden de fila (fondo 0 y despues los demas).</summary>
        internal static IReadOnlyList<RackFrameConfiguration> StoredInstances(SelectiveEditorState state)
            => state.PostCabeceras
                .Concat(state.ExtraFondoPostCabeceras.SelectMany(row => (IEnumerable<RackFrameConfiguration>)row ?? Array.Empty<RackFrameConfiguration>()))
                .ToList();

        internal static void AssertSameInstances(IReadOnlyList<RackFrameConfiguration> expected, IReadOnlyList<RackFrameConfiguration> actual)
        {
            Assert.Equal(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Same(expected[i], actual[i]);
            }
        }

        /// <summary>El sistema resuelto en lo que PREPARE podria tocar: cabeceras (identidad y contenido), peraltes y forma.
        /// Deliberadamente sin BOM ni dibujo, que normalizan en sitio al leer.</summary>
        internal static string SystemFingerprint(SelectiveRackSystem system)
            => "cab0=" + Row(system.PostCabeceras)
               + ";extra=" + string.Join("|", system.ExtraFondoPostCabeceras.Select(Row))
               + ";peraltes=" + string.Join(",", system.PostPeraltes.Select(R))
               + ";tramo=" + R(system.PostPeralte)
               + ";alto=" + R(system.Height)
               + ";fondos=" + system.DepthCount
               + ";overrides=" + string.Join(",", system.FondoCabeceraOverrides.Select(R))
               + ";bahias=" + string.Join("|", Enumerable.Range(0, system.DepthCount).Select(k =>
                   string.Join(",", (SelectiveDepthLayout.BaysOfFondo(system, k) ?? new List<SelectiveBay>()).Select(bay => R(bay.Height) + "x" + bay.Levels.Count))));

        internal static string BomSignature(SelectiveRackSystem system)
            => string.Join("|", SelectiveBomBuilder.Build(system, Catalog).Lines
                .Select(line => line.Category + ":" + line.ProfileId + ":" + line.Description + ":"
                                + line.Length.ToString("0.###", CultureInfo.InvariantCulture) + ":" + line.Quantity)
                .OrderBy(text => text, StringComparer.Ordinal));

        internal static string PlantaSignature(IEnumerable<HeaderBlockInstance> instances)
            => string.Join("|", instances.Select(instance =>
                string.Join("/", instance.Role, instance.PieceId, instance.BlockName,
                    instance.Insertion.X.ToString("F6", CultureInfo.InvariantCulture),
                    instance.Insertion.Y.ToString("F6", CultureInfo.InvariantCulture),
                    instance.RotationRadians.ToString("F6", CultureInfo.InvariantCulture),
                    instance.MirroredX, instance.MirroredY,
                    string.Join(",", instance.DynamicParameters.OrderBy(p => p.Key, StringComparer.Ordinal)
                        .Select(p => p.Key + "=" + p.Value.ToString("F6", CultureInfo.InvariantCulture))))));
    }
}
