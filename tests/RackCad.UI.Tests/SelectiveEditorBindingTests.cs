using System.Collections.Generic;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-47 gate G12 — lo que el editor Selectivo REAL enseña cuando la holgura vertical está gobernada.
    ///
    /// <para>
    /// La mitad pura ya está probada en el Core: authored + registro → efectivo. Aquí se comprueba lo único
    /// que solo puede comprobarse con la ventana de verdad — que la caja muestra el valor efectivo, que
    /// mientras el vínculo esté activo no es una vía de edición, y que el viaje de vuelta por el portador de
    /// G10 devuelve el literal congelado.
    /// </para>
    /// <para>
    /// El control queda <b>deshabilitado</b> y no oculto: el usuario tiene que poder VER el número que gobierna
    /// su rack. Esconderlo convertiría "gobernado por una variable" en "no existe", que es peor que editable.
    /// </para>
    /// </summary>
    public sealed class SelectiveEditorBindingTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";
        private const string RackId = "GUID-SEL-G12";
        private const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";

        private static SelectivePalletDesign MinimalDesign(double clearance)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = clearance,
                FloorBeamRise = 4.0,
                PalletDepth = 48.0,
                DepthCount = 1,
                DrawBasePlate = true,
            };

            var bay = new SelectiveBayDesign { FloorBeam = true };

            for (var level = 0; level < 2; level++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42.0, Alto = 60.0 },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0,
                });
            }

            design.Bays.Add(bay);
            return design;
        }

        private static SelectivePalletDesignDocument Authored(bool bound)
        {
            var document = SelectivePalletDesignDocument.From(MinimalDesign(6.0), RackId, "Selectivo G12");

            if (bound)
            {
                document.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
                {
                    [ProjectPropertyIds.SelectiveVerticalClearanceToken] =
                        SelectivePropertyValueDocument.ToProjectVariable(VarId),
                };
                document.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return document;
        }

        private static ProjectVariablesReadResult Registro(double value)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                new ProjectVariableDocument
                {
                    VariableId = VarId,
                    Name = "Holgura estándar",
                    Type = VariableType.Length.ToString(),
                    Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
                },
            };

            return ProjectVariablesReadResult.Readable(document);
        }

        private static TextBox Clearance(RackSelectiveWindow window)
            => EditorWindowTestSupport.Find<TextBox>(window, box => box.Name == "ClearanceBox");

        // ================================================================ lo que la caja enseña

        [Fact]
        public void BOUND_LaCajaMUESTRA_EL_EFECTIVO_Y_NO_SE_EDITA()
        {
            var (text, readOnly, enabled) = StaTestRunner.Run(() =>
            {
                var authored = Authored(bound: true);
                var open = SelectiveEditorOpen.Resolve(authored, Registro(10.0));
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                window.LoadExisting(authored, open.Design, open.VerticalClearance);

                var box = Clearance(window);
                return (box.Text, box.IsReadOnly, box.IsEnabled);
            });

            Assert.Equal("10", text);
            Assert.True(readOnly);
            Assert.False(enabled);
        }

        [Fact]
        public void UNBOUND_LaCajaMUESTRA_EL_LITERAL_Y_SE_EDITA()
        {
            var (text, readOnly, enabled) = StaTestRunner.Run(() =>
            {
                var authored = Authored(bound: false);
                var open = SelectiveEditorOpen.Resolve(authored, ProjectVariablesReadResult.Absent());
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                window.LoadExisting(authored, open.Design, open.VerticalClearance);

                var box = Clearance(window);
                return (box.Text, box.IsReadOnly, box.IsEnabled);
            });

            Assert.Equal("6", text);
            Assert.False(readOnly);
            Assert.True(enabled);
        }

        /// <summary>La carga histórica de un argumento —biblioteca, tests— sigue siendo el caso sin vínculo.</summary>
        [Fact]
        public void LA_CARGA_HISTORICA_SIGUE_SIENDO_EDITABLE()
        {
            var (text, readOnly, enabled) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                window.LoadExisting(Authored(bound: false));

                var box = Clearance(window);
                return (box.Text, box.IsReadOnly, box.IsEnabled);
            });

            Assert.Equal("6", text);
            Assert.False(readOnly);
            Assert.True(enabled);
        }

        // ================================================================ el viaje completo

        /// <summary>
        /// El ciclo entero por el editor REAL: authored + registro → efectivo → la ventana → «Actualizar» →
        /// el diseño que sale lleva el efectivo (el dibujo tiene que reflejarlo) → el portador de G10 lo
        /// persiste dejando el literal congelado y el vínculo intacto.
        /// </summary>
        [Fact]
        public void BOUND_ACTUALIZAR_DEVUELVE_EL_EFECTIVO_Y_EL_PORTADOR_CONGELA_EL_LITERAL()
        {
            var authored = Authored(bound: true);

            var salida = StaTestRunner.Run(() =>
            {
                var open = SelectiveEditorOpen.Resolve(authored, Registro(10.0));
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                window.LoadExisting(authored, open.Design, open.VerticalClearance);
                EditorWindowTestSupport.ClickNamed(window, "UpdateButton");

                return window.DesignToInsert;
            });

            Assert.NotNull(salida);
            Assert.Equal(10.0, salida.VerticalClearance); // el dibujo refleja el efectivo

            var persistido = authored.WithDesign(salida, RackId, "Selectivo G12");

            Assert.Equal(6.0, persistido.VerticalClearance); // el snapshot congelado sobrevive
            Assert.True(persistido.TryGetBinding(ProjectPropertyIds.SelectiveVerticalClearance, out var id));
            Assert.Equal(VariableId.Parse(VarId), id);
        }
    }
}
