using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G17 — el vertical slice se cierra: vincular y desvincular desde el Selectivo.
    ///
    /// <para>
    /// Las dos operaciones son asimétricas y esa asimetría ES el contrato. <b>Vincular</b> CONGELA el literal
    /// —no lo sustituye— y deja que la variable gobierne: si escribiera el efectivo en el literal, el snapshot
    /// que permite desvincular o reparar más tarde desaparecería en el mismo gesto que lo creó.
    /// <b>Desvincular</b> hace lo contrario: MATERIALIZA el efectivo actual en el literal y quita el vínculo,
    /// porque devolver el literal viejo haría saltar la geometría — el rack se movería solo, sin que nadie lo
    /// pidiera.
    /// </para>
    /// <para>
    /// Y una referencia ROTA no se desvincula: no hay valor efectivo que materializar, y tomar el literal
    /// congelado como si lo fuera sería exactamente el fallback silencioso que el contrato prohíbe. Ese caso
    /// es la reparación explícita y avisada de la ventana central, no este botón.
    /// </para>
    /// <para>
    /// Nada de esto se decide aquí: G5 dice de qué rack hablamos, G6 qué puede hacer cada operación y G11 cómo
    /// se escribe. G17 añade una superficie y un intent dirigido por identidad.
    /// </para>
    /// </summary>
    public class SelectiveBindingIntentTests
    {
        private const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string VarX = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        private const string VarY = "11111111-2222-3333-4444-555555555555";
        private const string Token = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        private static readonly PropertyId Clearance = ProjectPropertyIds.SelectiveVerticalClearance;

        private static SelectivePalletDesign Diseno(double clearance)
        {
            var design = new SelectivePalletDesign { VerticalClearance = clearance, PalletDepth = 48.0 };
            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = "BEAM_A",
                BeamPeralte = 4.5,
            });
            design.Bays.Add(bay);
            return design;
        }

        private static SelectivePalletDesignDocument Doc(double literal = 6.0, string variableId = null)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(literal), RackA, "Rack A");

            if (variableId != null)
            {
                doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
                {
                    [Token] = SelectivePropertyValueDocument.ToProjectVariable(variableId),
                };
                doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return doc;
        }

        private static ProjectVariableScanEntry Vista(SelectivePalletDesignDocument doc, string def)
            => ProjectVariableScanEntry.Selective(def, RackA, doc, 1);

        private static ProjectVariableDocument Variable(string id, string name, double value)
            => new ProjectVariableDocument
            {
                VariableId = id,
                Name = name,
                Type = VariableType.Length.ToString(),
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
            };

        private static ProjectVariablesDocument Registro(params ProjectVariableDocument[] variables)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = variables.ToList();
            return document;
        }

        private static VariableMutationPreflightResult Correr(
            SelectiveBindingIntent intent, ProjectVariablesDocument registry, params ProjectVariableScanEntry[] entries)
            => SelectiveBindingIntentPreflight.Run(intent, registry, entries);

        private static RackMutation Vincular(
            ProjectVariablesDocument registry, params ProjectVariableScanEntry[] entries)
        {
            var result = Correr(SelectiveBindingIntent.Link(RackA, Token, VariableId.Parse(VarX)), registry, entries);

            Assert.True(result.IsSuccess);
            return result.Plan.RackMutations[0];
        }

        // ================================================================ 1-4: vincular

        [Fact]
        public void VINCULAR_CONGELA_EL_LITERAL_EN_VEZ_DE_SUSTITUIRLO()
        {
            var rack = Vincular(Registro(Variable(VarX, "Holgura", 10.0)), Vista(Doc(6.0), "D1"));

            Assert.Equal(6.0, rack.AuthoredOutput.VerticalClearance);
            Assert.Equal(10.0, rack.EffectiveOutput.VerticalClearance);
        }

        [Fact]
        public void VINCULAR_ESCRIBE_EL_VariableId_EXACTO()
        {
            var rack = Vincular(Registro(Variable(VarX, "Holgura", 10.0)), Vista(Doc(6.0), "D1"));

            Assert.True(rack.AuthoredOutput.TryGetBinding(Clearance, out var id));
            Assert.Equal(VariableId.Parse(VarX), id);
        }

        [Fact]
        public void VINCULAR_PROMUEVE_EL_ESQUEMA()
        {
            var rack = Vincular(Registro(Variable(VarX, "Holgura", 10.0)), Vista(Doc(6.0), "D1"));

            Assert.Equal(SelectivePalletDesignDocument.PromotedSchemaVersion, rack.AuthoredOutput.SchemaVersion);
        }

        /// <summary>Todas las vistas del rack son destino: una que se quede atrás es un rack divergente.</summary>
        [Fact]
        public void VINCULAR_ALCANZA_A_TODAS_LAS_VISTAS_HERMANAS()
        {
            var rack = Vincular(
                Registro(Variable(VarX, "Holgura", 10.0)),
                Vista(Doc(6.0), "D1"), Vista(Doc(6.0), "D2"), Vista(Doc(6.0), "D3"));

            Assert.Equal(3, rack.Destinations.Count);
        }

        // ================================================================ 5, 6: el alcance del rack

        /// <summary>
        /// Familia B: el usuario eligió el rack, así que la sonda no decide nada. Un rack SIN vincular responde
        /// NEGATIVO en todas sus vistas, y vincular es justo la operación que se aplica a un rack sin vincular.
        /// </summary>
        [Fact]
        public void VINCULAR_UN_RACK_SIN_NINGUN_VINCULO_ES_LO_NORMAL()
        {
            var result = Correr(
                SelectiveBindingIntent.Link(RackA, Token, VariableId.Parse(VarX)),
                Registro(Variable(VarX, "Holgura", 10.0)),
                Vista(Doc(6.0), "D1"));

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void VINCULAR_CON_VISTAS_DIVERGENTES_SE_CANCELA()
        {
            var result = Correr(
                SelectiveBindingIntent.Link(RackA, Token, VariableId.Parse(VarX)),
                Registro(Variable(VarX, "Holgura", 10.0)),
                Vista(Doc(6.0), "D1"), Vista(Doc(9.0), "D2"));

            Assert.False(result.IsSuccess);
            Assert.True(result.Plan.IsEmpty);
        }

        [Fact]
        public void VINCULAR_A_UNA_VARIABLE_QUE_NO_EXISTE_SE_CANCELA()
        {
            var result = Correr(
                SelectiveBindingIntent.Link(RackA, Token, VariableId.Parse(VarY)),
                Registro(Variable(VarX, "Holgura", 10.0)),
                Vista(Doc(6.0), "D1"));

            Assert.False(result.IsSuccess);
        }

        // ================================================================ 7-9: desvincular

        [Fact]
        public void DESVINCULAR_MATERIALIZA_EL_EFECTIVO_ACTUAL()
        {
            var result = Correr(
                SelectiveBindingIntent.Unlink(RackA, Token),
                Registro(Variable(VarX, "Holgura", 10.0)),
                Vista(Doc(6.0, VarX), "D1"));

            Assert.True(result.IsSuccess);

            var rack = result.Plan.RackMutations[0];

            // El literal pasa a 10, NO vuelve a 6: si volviera, el rack saltaría solo.
            Assert.Equal(10.0, rack.AuthoredOutput.VerticalClearance);
            Assert.Equal(10.0, rack.EffectiveOutput.VerticalClearance);
        }

        [Fact]
        public void DESVINCULAR_QUITA_EL_VINCULO()
        {
            var result = Correr(
                SelectiveBindingIntent.Unlink(RackA, Token),
                Registro(Variable(VarX, "Holgura", 10.0)),
                Vista(Doc(6.0, VarX), "D1"));

            Assert.False(result.Plan.RackMutations[0].AuthoredOutput.HasBindingEntry(Clearance));
        }

        /// <summary>El esquema NO baja: el documento estuvo vinculado y una versión anterior no debe reabrirlo a ciegas.</summary>
        [Fact]
        public void DESVINCULAR_CONSERVA_LA_LINEA_PROMOCIONADA()
        {
            var result = Correr(
                SelectiveBindingIntent.Unlink(RackA, Token),
                Registro(Variable(VarX, "Holgura", 10.0)),
                Vista(Doc(6.0, VarX), "D1"));

            Assert.Equal(
                SelectivePalletDesignDocument.PromotedSchemaVersion,
                result.Plan.RackMutations[0].AuthoredOutput.SchemaVersion);
        }

        // ================================================================ 10: el vínculo roto

        /// <summary>
        /// Sin variable no hay efectivo que materializar, y tomar el literal congelado como si lo fuera sería
        /// el fallback silencioso que el contrato prohíbe. Reparar es otra operación, avisada.
        /// </summary>
        [Fact]
        public void UN_VINCULO_ROTO_NO_SE_DESVINCULA_POR_LA_VIA_NORMAL()
        {
            var result = Correr(
                SelectiveBindingIntent.Unlink(RackA, Token),
                Registro(Variable(VarY, "Otra", 10.0)),
                Vista(Doc(6.0, VarX), "D1"));

            Assert.False(result.IsSuccess);
            Assert.True(result.Plan.IsEmpty);
        }

        [Fact]
        public void DESVINCULAR_UN_RACK_SIN_VINCULO_SE_CANCELA()
        {
            var result = Correr(
                SelectiveBindingIntent.Unlink(RackA, Token),
                Registro(Variable(VarX, "Holgura", 10.0)),
                Vista(Doc(6.0), "D1"));

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void UN_INTENT_NULO_NO_PRODUCE_PLAN()
        {
            Assert.False(SelectiveBindingIntentPreflight
                .Run(null, Registro(), new ProjectVariableScanEntry[0]).IsSuccess);
        }

        // ================================================================ 11, 12: el nombre NO es identidad

        /// <summary>
        /// Dos variables pueden llamarse igual: el nombre es texto que el usuario edita, no identidad. Lo que
        /// no puede es dejar de distinguirse — y no deja, porque nada resuelve por nombre.
        /// </summary>
        [Fact]
        public void DOS_VARIABLES_HOMONIMAS_SIGUEN_SIENDO_DISTINGUIBLES()
        {
            var registry = Registro(Variable(VarX, "Holgura", 10.0), Variable(VarY, "Holgura", 12.0));
            var options = SelectiveBindingOptions.ForLength(registry);

            Assert.Equal(2, options.Count);
            Assert.Equal(2, options.Select(option => option.Id).Distinct().Count());
            Assert.All(options, option => Assert.Equal("Holgura", option.Name));
            Assert.Contains(options, option => option.Value == 10.0);
            Assert.Contains(options, option => option.Value == 12.0);
        }

        [Fact]
        public void VINCULAR_A_UNA_HOMONIMA_LLEVA_LA_QUE_SE_ELIGIO()
        {
            var registry = Registro(Variable(VarX, "Holgura", 10.0), Variable(VarY, "Holgura", 12.0));

            var result = Correr(
                SelectiveBindingIntent.Link(RackA, Token, VariableId.Parse(VarY)), registry, Vista(Doc(6.0), "D1"));

            Assert.True(result.Plan.RackMutations[0].AuthoredOutput.TryGetBinding(Clearance, out var id));
            Assert.Equal(VariableId.Parse(VarY), id);
            Assert.Equal(12.0, result.Plan.RackMutations[0].EffectiveOutput.VerticalClearance);
        }

        /// <summary>Renombrar la variable no toca el vínculo: por eso la identidad es independiente del nombre.</summary>
        [Fact]
        public void RENOMBRAR_LA_VARIABLE_DEJA_EL_VINCULO_INTACTO()
        {
            var registry = Registro(Variable(VarX, "Holgura A", 10.0));
            var vinculado = Vincular(registry, Vista(Doc(6.0), "D1")).AuthoredOutput;

            var renombrado = Registro(Variable(VarX, "Holgura General", 10.0));

            Assert.True(vinculado.TryGetBinding(Clearance, out var id));
            Assert.Equal(VariableId.Parse(VarX), id);
            Assert.Equal(
                10.0,
                new SelectiveEffectiveDesignResolver().Resolve(vinculado, renombrado).Design.VerticalClearance);
        }

        // ================================================================ las opciones que la UI recibe

        [Fact]
        public void SOLO_SE_OFRECEN_VARIABLES_DE_TIPO_COMPATIBLE()
        {
            var registry = Registro(Variable(VarX, "Holgura", 10.0));
            registry.Variables.Add(new ProjectVariableDocument
            {
                VariableId = VarY,
                Name = "De un tipo futuro",
                Type = "Angle",
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = 30.0 },
            });

            var options = SelectiveBindingOptions.ForLength(registry);

            Assert.Single(options);
            Assert.Equal(VariableId.Parse(VarX), options[0].Id);
        }

        [Fact]
        public void SIN_REGISTRO_NO_HAY_OPCIONES()
        {
            Assert.Empty(SelectiveBindingOptions.ForLength(null));
        }

        // ================================================================ guardas de fuente

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.NotNull(dir);
            return dir;
        }

        private static string Src(params string[] relative)
            => File.ReadAllText(Path.Combine(new[] { RepoRoot().FullName, "src" }.Concat(relative).ToArray()));

        private static string Commands() => Src("RackCad.Plugin", "RackSelectivoCommands.cs");

        private static string Window()
            => Src("RackCad.UI", "Systems", "Selective", "RackSelectiveWindow.xaml.cs");

        /// <summary>El control reusable donde vive la seleccion explicita desde I-48 G4C.</summary>
        private static string Editor() => Src("RackCad.UI", "Controls", "LinkedPropertyEditor.cs");

        [Fact]
        public void GUARDA_EL_CAMINO_DE_VINCULO_PASA_POR_G6_Y_G11()
        {
            var source = Commands();

            Assert.Contains("SelectiveBindingIntentPreflight.Run", source);
            Assert.Contains("ProjectVariableMutationExecutor.Execute", source);
            Assert.Contains("ProjectVariablesRegistry.Read", source);
        }

        /// <summary>Ni el Plugin ni la ventana tocan el mapa de vínculos: escribirlo es de G6/G11.</summary>
        [Fact]
        public void GUARDA_NADIE_MUTA_PropertyValues_A_MANO()
        {
            foreach (var source in new[] { Commands(), Window() })
            {
                Assert.DoesNotContain("PropertyValues", source);
                Assert.DoesNotContain("SelectivePropertyValueDocument", source);
                Assert.DoesNotContain("ToProjectVariable(", source);
                Assert.DoesNotContain("SelectiveEffectiveDesignResolver", source);
            }
        }

        /// <summary>
        /// La regla de G17 hecha guarda: el intent sale de la IDENTIDAD de la opción elegida, nunca de su
        /// nombre. Resolver por nombre rompería el momento en que dos variables se llamen igual.
        /// </summary>
        [Fact]
        public void GUARDA_LA_VENTANA_VINCULA_POR_IDENTIDAD_Y_NO_POR_NOMBRE()
        {
            // I-48 G4C: vincular ya no es un boton de la ventana, es una seleccion explicita en el editor
            // reusable. La invariante no cambia -se vincula por IDENTIDAD- pero el sitio donde vive si.
            var editor = Editor();

            var at = editor.IndexOf("session.TrySelect(option.VariableId", StringComparison.Ordinal);
            Assert.True(at >= 0, "el editor tiene que seleccionar por VariableId");

            var call = editor.Substring(at, Math.Min(220, editor.Length - at));

            Assert.DoesNotContain("option.Name", call);

            // Y no hay ninguna via que resuelva un nombre a una identidad.
            Assert.DoesNotContain("Name ==", editor);
            Assert.DoesNotContain("FirstOrDefault(o => o.Name", editor);
        }

        /// <summary>G17 no introduce reparación desde el editor: eso vive en la ventana central.</summary>
        [Fact]
        public void GUARDA_NO_HAY_REPARACION_EN_EL_EDITOR_SELECTIVO()
        {
            var window = Window();

            Assert.DoesNotContain("RepairBroken", window);
            Assert.DoesNotContain("Reparar", window);
        }
    }
}
