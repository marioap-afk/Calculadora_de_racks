using System;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.UI.Systems.Selective;
using Xunit;
using S = RackCad.UI.Tests.SelectiveHeaderBatchTestSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-53S, G5 — S-29 (Proposal V2 §13.3): con copias DISTRIBUIDAS por la ventana, «Actualizar» y el dibujo frontal de cada
    /// fondo, la planta y el lateral siguen siendo correctos. Y la re-medicion C2-4 que I-52 dejo pedida para cuando I-53S
    /// conectara <c>ApplyHeaderBatch</c> a la ventana (Proposal V6 de I-52, §8.5 y §15.3): el camino «estado del editor ->
    /// sistema/documento» sigue siendo equivalente al del documento guardado y reabierto, en cada vista.
    /// <para>
    /// Rack de dos fondos (3 y 2 frentes). Origen: F1, Poste 2, personalizada 24 in mas alta que el poste resuelto, para que
    /// la copia se VEA en el dibujo. Destinos: «Todos» los fondos × Postes 1 y 3.
    /// </para>
    /// </summary>
    public sealed class SelectiveHeaderBatchDrawingTests
    {
        private const string ExpectedAfter = "0/0=H 0/1=H 0/2=H 0/3=- 1/0=H 1/1=- 1/2=H 1/3=x";

        /// <summary>Opens the rack, places the source, distributes, and presses the REAL «Actualizar». <c>Before</c> is the
        /// system with the source already placed and nothing distributed yet.</summary>
        private static (RackSelectiveWindow Window, HeaderBatchOutcome<SelectiveHeaderAddress> Outcome, RackCad.Domain.Systems.Selective.SelectiveRackSystem Before, double Height) DistributeThenUpdate()
        {
            var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
            window.LoadExisting(SelectivePalletDesignDocument.From(S.Design(3, 2), "GUID-I53S-S29", "Selectivo S29"));
            S.ShowFondo(window, 1);
            var height = window.CustomizeSeedHeightForTest(1) + 24.0;
            S.PlaceCustom(window, 1, 2, height);
            var before = S.Resolve(window.BuildDesignForTest(out _), window.RackName);

            S.SelectPost(window, 2);
            S.TakeSource(window);
            SelectiveTargetsTestSupport.SetAllTargets(window);
            S.SetPostTargets(window, 1, 3);
            using (SelectiveCabeceraHeightPrompt.Substitute(_ => true))
            {
                S.Apply(window);
            }

            var outcome = window.LastHeaderBatchOutcome;
            EditorWindowTestSupport.ClickNamed(window, "UpdateButton");
            return (window, outcome, before, height);
        }

        [Fact]
        public void S29_UpdateAfterADistribution_DrawsTheCopiesInTheFrontalOfEveryFondoAndInTheLateral_AndThePayloadCorresponds()
        {
            var r = StaTestRunner.Run(() =>
            {
                var (window, outcome, before, height) = DistributeThenUpdate();
                var system = window.SystemToInsert;
                var payload = window.DesignToInsert;
                return (Outcome: outcome,
                    Requested: window.InsertRequested && window.UpdateOnly,
                    Corresponds: system != null && payload != null && S.Corresponds(payload, system),
                    Changed: system != null && S.DrawingSignature(before) != S.DrawingSignature(system),
                    Customs: system == null ? string.Empty : S.CustomMap(system, 2, 4).Replace("=" + S.R(height), "=H"),
                    FrontalsChanged: system != null
                        && S.FrontalSignature(system, 0) != S.FrontalSignature(before, 0)
                        && S.FrontalSignature(system, 1) != S.FrontalSignature(before, 1),
                    LateralChanged: system != null && S.LateralSignature(system) != S.LateralSignature(before));
            });

            var committed = Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(r.Outcome);
            Assert.Equal("0/0 0/2 1/0 1/2", S.Addresses(committed.Applied));
            Assert.True(r.Requested, "«Actualizar» debe pedir el redibujo en sitio.");
            Assert.True(r.Corresponds, "El diseño del payload debe resolver al mismo dibujo que su sistema.");
            Assert.True(r.Changed, "Las copias distribuidas tienen que llegar al dibujo.");
            Assert.Equal(ExpectedAfter, r.Customs);
            Assert.True(r.FrontalsChanged, "El frontal de CADA fondo con destinos debe dibujar sus copias.");
            Assert.True(r.LateralChanged, "Los cortes laterales deben dibujar las copias.");
        }

        [Fact]
        public void C2_4_I52_TheEditorStateAfterADistribution_EqualsTheReopenedDocument_InEveryView()
        {
            var r = StaTestRunner.Run(() =>
            {
                var (window, outcome, _, height) = DistributeThenUpdate();
                var editor = window.SystemToInsert;

                var store = new SelectivePalletDesignStore();
                var reopened = store.Deserialize(store.Serialize(
                    SelectivePalletDesignDocument.From(window.DesignToInsert, window.RackId, window.RackName)));
                var again = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                again.LoadExisting(reopened);
                var reopenedMap = S.CustomMap(again.EditorState).Replace("=" + S.R(height), "=H");
                EditorWindowTestSupport.ClickNamed(again, "UpdateButton");
                var redrawn = again.SystemToInsert;

                return (Outcome: outcome,
                    Editor: editor == null ? string.Empty : S.DrawingSignature(editor),
                    Reopened: redrawn == null ? "sin sistema" : S.DrawingSignature(redrawn),
                    EditorBom: editor == null ? string.Empty : S.BomSignature(editor),
                    ReopenedBom: redrawn == null ? "sin sistema" : S.BomSignature(redrawn),
                    ReopenedMap: reopenedMap);
            });

            Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(r.Outcome);
            Assert.True(r.Editor == r.Reopened, "Frontal por fondo, planta y lateral: el estado del editor y el documento reabierto divergen.");
            Assert.Equal(r.EditorBom, r.ReopenedBom);
            Assert.Equal(ExpectedAfter, r.ReopenedMap);
        }
    }
}
