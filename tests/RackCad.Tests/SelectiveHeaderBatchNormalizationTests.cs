using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using Xunit;
using F = RackCad.Tests.SelectiveHeaderBatchFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 G4 — la normalizacion de cada copia preparada del Selectivo: altura, profundidad y peralte
    /// (Proposal V2 §6.5-§6.7; ADR-0032 D9/D10).
    ///
    /// <para>
    /// La ALTURA viaja con la receta y se revisa en cada destino con la revision que ya existe. La PROFUNDIDAD la impone
    /// el fondo destino sobre la copia, por la autoridad existente, que tambien refresca su derivado. El PERALTE lo
    /// impone el poste destino por la regla unica de <see cref="SelectivePostGeometry"/>. Cubre S-14, S-15, S-16 y la
    /// guarda de equivalencia de la sobrecarga de peralte.
    /// </para>
    /// </summary>
    public class SelectiveHeaderBatchNormalizationTests
    {
        private static HeaderWarningSeverity SeverityOf(SelectiveCabeceraHeightIssue issue)
            => issue == SelectiveCabeceraHeightIssue.Severe ? HeaderWarningSeverity.Severe : HeaderWarningSeverity.Informative;

        // ===== S-14 — altura =============================================================================================

        [Fact]
        public void S14_LA_ALTURA_VIAJA_CON_LA_RECETA_Y_SE_REVISA_EN_CADA_DESTINO_CON_LA_REVISION_EXISTENTE()
        {
            var editor = new F.Editor(F.State(F.Fondo(3, levels: 4), F.Fondo(3)));
            F.StoreCustom(editor.State, 0, 0, F.Custom(100.0));
            editor.Recompute();
            editor.State.FollowAllFondos();

            var preparation = editor.Prepare(F.Distribute(F.At(0, 0), 1, 2, 3));
            var plan = F.Prepared(preparation);

            // La altura es la de la receta en todas las copias, nunca la del destino.
            Assert.All(F.Copies(preparation), copy => Assert.Equal(100.0, copy.Height, 6));

            // El oraculo es la revision historica, poste a poste, sin tocar: los avisos son sus hallazgos por (fondo, poste).
            var expected = plan.Targets
                .SelectMany(target => SelectiveCabeceraHeightReview.Of(editor.System, new[] { target.FondoIndex }, target.PostIndex, 100.0)
                    .Findings.Select(finding => (Address: target, Severity: SeverityOf(finding.Issue))))
                .ToList();
            Assert.Equal(expected, plan.Warnings.Select(warning => (warning.Address, warning.Severity)).ToList());
            Assert.Contains(plan.Warnings, warning => warning.Severity == HeaderWarningSeverity.Severe);
            Assert.Contains(plan.Warnings, warning => warning.Severity == HeaderWarningSeverity.Informative);
            Assert.All(plan.Warnings, warning => Assert.False(string.IsNullOrWhiteSpace(warning.Message)));
            Assert.True(plan.RequiresConfirmation);

            // La API multidestino de la revision es la misma revision, en el mismo orden.
            var findings = SelectiveCabeceraHeightReview.OfDestinations(editor.System, plan.Targets, 100.0);
            Assert.Equal(expected, findings.Select(finding => (finding.Address, SeverityOf(finding.Issue))).ToList());
            Assert.Equal(plan.Warnings.Select(warning => warning.Message).ToList(), findings.Select(finding => finding.Describe()).ToList());
        }

        [Fact]
        public void S14_NO_CONFIRMAR_UN_AVISO_SEVERO_TERMINA_EN_CANCELLED_SIN_MUTACION()
        {
            var editor = new F.Editor(F.State(F.Fondo(3, levels: 4), F.Fondo(3)));
            F.StoreCustom(editor.State, 0, 0, F.Custom(100.0));
            editor.Recompute();
            editor.State.FollowAllFondos();
            var before = F.StateFingerprint(editor.State);

            var preparation = editor.Prepare(F.Distribute(F.At(0, 0), 1, 2, 3));
            Assert.True(F.Prepared(preparation).RequiresConfirmation);

            var outcome = preparation.Cancel();

            Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Cancelled>(outcome);
            Assert.Equal(before, F.StateFingerprint(editor.State));
            Assert.Throws<InvalidOperationException>(() => editor.Mutate(preparation)); // un gesto cancelado no se aplica despues
            Assert.Equal(before, F.StateFingerprint(editor.State));
        }

        [Fact]
        public void S14_SIN_AVISO_SEVERO_NO_SE_PIDE_CONFIRMACION_Y_NO_HAY_A_QUIEN_CANCELAR()
        {
            var editor = new F.Editor(F.State(3, 3));
            F.StoreCustom(editor.State, 0, 0, F.Custom(500.0)); // muy por encima de todo: solo informativos
            editor.Recompute();
            editor.State.FollowAllFondos();

            var preparation = editor.Prepare(F.Distribute(F.At(0, 0), 1, 2));
            var plan = F.Prepared(preparation);

            Assert.DoesNotContain(plan.Warnings, warning => warning.Severity == HeaderWarningSeverity.Severe);
            Assert.False(plan.RequiresConfirmation);
            Assert.Throws<InvalidOperationException>(() => preparation.Cancel());
        }

        // ===== S-15 — profundidad ======================================================================================

        [Fact]
        public void S15_CADA_COPIA_RECIBE_LA_PROFUNDIDAD_DE_SU_FONDO_NUNCA_LA_DEL_ORIGEN_Y_SU_DERIVADO_LA_SIGUE()
        {
            var editor = new F.Editor(F.State(F.Fondo(3, depth: 48.0), F.Fondo(3, depth: 60.0), F.Fondo(3, depth: 72.0)));
            F.StoreCustom(editor.State, 1, 0, F.Custom(180.0, 54.0)); // autorada en el fondo intermedio
            editor.Recompute();
            editor.State.FollowAllFondos();

            var preparation = editor.Prepare(F.Distribute(F.At(1, 0), 0, 1));
            var plan = F.Prepared(preparation);
            var copies = F.Copies(preparation);

            Assert.Equal(new[] { 42.0, 54.0 }, new[] { editor.State.CabeceraDepthOfFondo(0), editor.State.CabeceraDepthOfFondo(1) });
            for (var i = 0; i < plan.Targets.Count; i++)
            {
                var fondo = plan.Targets[i].FondoIndex;
                Assert.Equal(editor.State.CabeceraDepthOfFondo(fondo), copies[i].Depth, 6);

                // El derivado de la copia es el de su profundidad: identico a reconstruirlo por la ruta canonica.
                Assert.Equal(
                    HeaderConfigurationFixtures.MembersFingerprint(new RackFrameProjectStore().DeepCopy(copies[i])),
                    HeaderConfigurationFixtures.MembersFingerprint(copies[i]));
            }

            var committed = F.Committed(editor.Mutate(preparation));
            Assert.Contains(F.At(2, 0), committed.Applied);
            Assert.Equal(66.0, editor.State.CabeceraAt(2, 0).Depth, 6);
            Assert.Equal(42.0, editor.State.CabeceraAt(0, 0).Depth, 6);
            Assert.Equal(54.0, editor.State.CabeceraAt(1, 0).Depth, 6); // el origen no se movio

            // Non-vacuo: el fondo mas profundo tiene un derivado distinto del del origen.
            Assert.NotEqual(
                HeaderConfigurationFixtures.MembersFingerprint(editor.State.CabeceraAt(1, 0)),
                HeaderConfigurationFixtures.MembersFingerprint(editor.State.CabeceraAt(2, 0)));
        }

        // ===== S-16 — peralte =========================================================================================

        [Fact]
        public void S16_CADA_COPIA_PERSISTE_EL_PERALTE_EFECTIVO_DE_SU_POSTE_DESTINO_NUNCA_EL_DEL_ORIGEN()
        {
            var editor = new F.Editor(F.State(3, 3));
            editor.State.PostPeraltes[1] = 5.5; // override propio del poste origen
            var origen = F.Custom(180.0);
            origen.PostPeralte = 5.5; // como lo siembra «Personalizar»
            F.StoreCustom(editor.State, 0, 1, origen);
            editor.Recompute();
            editor.State.FollowAllFondos();

            var preparation = editor.Prepare(F.Distribute(F.At(0, 1), 1, 2));
            var plan = F.Prepared(preparation);
            var copies = F.Copies(preparation);

            Assert.Equal(new[] { F.At(0, 2), F.At(1, 1), F.At(1, 2) }, plan.Targets);
            for (var i = 0; i < plan.Targets.Count; i++)
            {
                Assert.Equal(SelectivePostGeometry.PostPeralteAt(editor.System, plan.Targets[i].PostIndex), copies[i].PostPeralte, 6);
            }

            Assert.Equal(new[] { F.RunPeralte, 5.5, F.RunPeralte }, copies.Select(copy => copy.PostPeralte));
            F.Committed(editor.Mutate(preparation));
            Assert.Equal(F.RunPeralte, editor.State.CabeceraAt(0, 2).PostPeralte, 6);
        }

        [Fact]
        public void S16_CON_EL_PERALTE_DEL_TRAMO_SIN_FIJAR_LA_PLANTA_DEL_DESTINO_NO_DIBUJA_EL_PERALTE_DEL_ORIGEN()
        {
            var editor = new F.Editor(F.State(3)) { RunPeralte = 0.0 }; // peralte del tramo sin fijar
            editor.State.PostPeraltes[1] = 5.5;
            var origen = F.Custom(180.0);
            origen.PostPeralte = 5.5;
            F.StoreCustom(editor.State, 0, 1, origen);
            editor.Recompute();
            Assert.Equal(0.0, SelectivePostGeometry.PostPeralteAt(editor.System, 2)); // premisa: la fuga seria visible

            F.Committed(editor.Mutate(editor.Prepare(F.Distribute(F.At(0, 1), 2))));
            editor.Recompute();

            var destino = editor.State.CabeceraAt(0, 2);
            Assert.Equal(0.0, destino.PostPeralte); // coherencia en escritura: el peralte efectivo de SU poste

            var builder = new PlantaHeaderLayoutBuilder();
            var dibujada = F.PlantaSignature(builder.Build(destino, F.Catalog, default, SelectivePostGeometry.PostPeralteAt(editor.System, 2)));

            var conFuga = F.DeepCopy(destino);
            conFuga.PostPeralte = 5.5;
            var fuga = F.PlantaSignature(builder.Build(conFuga, F.Catalog, default, SelectivePostGeometry.PostPeralteAt(editor.System, 2)));

            Assert.NotEqual(fuga, dibujada); // sin la normalizacion, la planta del destino mostraria el peralte del origen
        }

        [Fact]
        public void G4_POSTPERALTEFOR_ES_LA_MISMA_REGLA_QUE_POSTPERALTEAT_SOBRE_UN_VALOR_POR_POSTE()
        {
            foreach (var tramo in new[] { F.RunPeralte, 0.0 })
            {
                var editor = new F.Editor(F.State(5)) { RunPeralte = tramo };
                var valores = new[] { 0.0, 5.5, -1.0, double.NaN, 7.0, 2.25 };
                for (var post = 0; post < valores.Length; post++)
                {
                    editor.State.PostPeraltes[post] = valores[post];
                }

                editor.Recompute();

                for (var post = 0; post < valores.Length; post++)
                {
                    Assert.Equal(
                        SelectivePostGeometry.PostPeralteAt(editor.System, post),
                        SelectivePostGeometry.PostPeralteFor(editor.System, valores[post]));
                }
            }

            Assert.Equal(SelectivePostGeometry.PostPeralteAt(null, 0), SelectivePostGeometry.PostPeralteFor(null, 0.0));
            Assert.Equal(4.0, SelectivePostGeometry.PostPeralteFor(null, 4.0));
        }

        [Fact]
        public void G4_EDIT_NORMALIZA_LA_COPIA_AL_PERALTE_EFECTIVO_DEL_ESCALAR_PRECOMPUTADO()
        {
            var editor = new F.Editor(F.State(3, 3));
            editor.Recompute();
            editor.State.FollowAllFondos();

            foreach (var (editado, esperado) in new[] { (6.25, 6.25), (F.RunPeralte, F.RunPeralte), (0.0, F.RunPeralte) })
            {
                var result = F.Custom(190.0);
                result.PostPeralte = editado;
                var preparation = editor.Prepare(SelectiveHeaderBatchRequest.Edit(result, 1));

                Assert.All(F.Copies(preparation), copy => Assert.Equal(esperado, copy.PostPeralte, 6));
                Assert.Throws<InvalidOperationException>(() => preparation.Cancel()); // sin avisos severos: no hay CONFIRM
            }
        }
    }
}
