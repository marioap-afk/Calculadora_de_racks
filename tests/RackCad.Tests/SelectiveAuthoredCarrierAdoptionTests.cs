using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G10 — el camino REAL de guardado deja de fabricar un documento nuevo.
    ///
    /// <para>
    /// <c>From(design, id, name)</c> construye desde el DOMINIO, y el dominio no tiene version, ni bindings,
    /// ni campos desconocidos. Usarlo para guardar un rack EXISTENTE destruye exactamente el estado que tiene
    /// que sobrevivir: el viaje <c>documento → dominio → documento</c> pierde por el camino todo lo que el
    /// diseño no sabe llevar. Guardar un rack existente ACTUALIZA su documento authored; no fabrica otro.
    /// </para>
    /// <para>
    /// Y hay un detalle que solo aparece en el camino real: el usuario puede RENOMBRAR el rack en el editor.
    /// El portador conserva la identidad, pero el nombre es del editor — asi que actualizar tiene que poder
    /// llevar el nombre nuevo sin tocar nada mas.
    /// </para>
    /// <para>
    /// La adopcion vive en el Plugin, que ninguna suite carga (ADR-0003): la conducta del portador se prueba
    /// aqui y que el camino real lo USE se fija con una guarda de fuente.
    /// </para>
    /// </summary>
    public class SelectiveAuthoredCarrierAdoptionTests
    {
        private const string RackId = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        private const string Token = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        private static SelectivePalletDesign Diseno(double clearance)
        {
            var design = new SelectivePalletDesign { VerticalClearance = clearance };
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

        private static SelectivePalletDesignDocument Guardado(
            double literal = 6.0,
            SelectivePropertyValueDocument binding = null,
            bool conEntrada = false,
            string extensionKey = null)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(literal), RackId, "Rack original");

            if (binding != null || conEntrada)
            {
                doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument> { [Token] = binding };
                doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            if (extensionKey != null)
            {
                doc.ExtensionData = new Dictionary<string, JsonElement>
                {
                    [extensionKey] = JsonDocument.Parse("7").RootElement,
                };
            }

            return doc;
        }

        private static SelectivePropertyValueDocument Ref(string id = VarId)
            => SelectivePropertyValueDocument.ToProjectVariable(id);

        // ================================================================ 1-3: lo que sobrevive a guardar

        [Fact]
        public void GuardarUnRackVINCULADO_CONSERVA_PropertyValues()
        {
            var guardado = Guardado(binding: Ref()).WithDesign(Diseno(99.0), RackId, "Rack renombrado");

            Assert.True(guardado.TryGetBinding(ProjectPropertyIds.SelectiveVerticalClearance, out var id));
            Assert.Equal(VariableId.Parse(VarId), id);
        }

        [Fact]
        public void GuardarUnRackVINCULADO_CONSERVA_ExtensionData_Y_SchemaVersion()
        {
            var guardado = Guardado(binding: Ref(), extensionKey: "DeUnBuildPosterior")
                .WithDesign(Diseno(99.0), RackId, "Rack renombrado");

            Assert.True(guardado.ExtensionData.ContainsKey("DeUnBuildPosterior"));
            Assert.Equal(SelectivePalletDesignDocument.PromotedSchemaVersion, guardado.SchemaVersion);
        }

        [Fact]
        public void GuardarUnRackVINCULADO_CONSERVA_EL_LITERAL_CONGELADO()
        {
            var guardado = Guardado(literal: 6.0, binding: Ref()).WithDesign(Diseno(99.0), RackId, "Rack renombrado");

            Assert.Equal(6.0, guardado.VerticalClearance);
        }

        // ================================================================ 4: presente pero ininterpretable

        [Fact]
        public void UnaEntradaDeBindingININTERPRETABLE_TAMPOCO_SOBRESCRIBE_EL_LITERAL()
        {
            var kindRaro = Guardado(literal: 6.0, binding: new SelectivePropertyValueDocument { Kind = "futureKind", VariableId = VarId })
                .WithDesign(Diseno(99.0), RackId, "Rack renombrado");

            var idRoto = Guardado(literal: 6.0, binding: Ref("no-es-un-guid"))
                .WithDesign(Diseno(99.0), RackId, "Rack renombrado");

            var entradaNula = Guardado(literal: 6.0, conEntrada: true)
                .WithDesign(Diseno(99.0), RackId, "Rack renombrado");

            Assert.Equal(6.0, kindRaro.VerticalClearance);
            Assert.Equal(6.0, idRoto.VerticalClearance);
            Assert.Equal(6.0, entradaNula.VerticalClearance);
        }

        // ================================================================ 5: sin binding, el literal SI cambia

        [Fact]
        public void GuardarUnRackNO_VINCULADO_ADOPTA_EL_NUEVO_LITERAL()
        {
            var guardado = Guardado(literal: 6.0).WithDesign(Diseno(99.0), RackId, "Rack renombrado");

            Assert.Equal(99.0, guardado.VerticalClearance);
        }

        // ================================================================ el renombrado del editor

        [Fact]
        public void ACTUALIZAR_LLEVA_EL_NOMBRE_NUEVO_Y_CONSERVA_LA_IDENTIDAD()
        {
            var guardado = Guardado(binding: Ref()).WithDesign(Diseno(7.0), RackId, "Rack renombrado");

            Assert.Equal(RackId, guardado.Id);
            Assert.Equal("Rack renombrado", guardado.Name);
        }

        [Fact]
        public void SIN_NOMBRE_NUEVO_SE_CONSERVA_EL_DEL_PORTADOR()
        {
            var guardado = Guardado(binding: Ref()).WithDesign(Diseno(7.0));

            Assert.Equal("Rack original", guardado.Name);
            Assert.Equal(RackId, guardado.Id);
        }

        // ================================================================ 6: un rack NUEVO sigue igual

        [Fact]
        public void UN_RACK_NUEVO_SIGUE_CONSTRUYENDOSE_DESDE_From()
        {
            var nuevo = SelectivePalletDesignDocument.From(Diseno(6.0), RackId, "Rack nuevo");

            Assert.Equal(6.0, nuevo.VerticalClearance);
            Assert.Equal(SelectivePalletDesignDocument.CurrentSchemaVersion, nuevo.SchemaVersion);
            Assert.Null(nuevo.PropertyValues);
        }

        [Fact]
        public void EL_RESTO_DEL_ESTADO_PERSISTIDO_SE_ACTUALIZA_CON_EL_DISENO()
        {
            var editado = Diseno(6.0);
            editado.Bays[0].Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 40 },
                PalletCount = 1,
                BeamId = "BEAM_A",
                BeamPeralte = 4.5,
            });

            var guardado = Guardado(binding: Ref()).WithDesign(editado, RackId, "Rack renombrado");

            Assert.Equal(2, guardado.Bays[0].Levels.Count);
        }

        // ================================================================ guarda: el camino real lo USA

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

        private static string Commands()
            => File.ReadAllText(Path.Combine(
                RepoRoot().FullName, "src", "RackCad.Plugin", "RackSelectivoCommands.cs"));

        /// <summary>
        /// La propiedad que ninguna suite puede ejecutar: al editar un rack EXISTENTE, el camino real pasa el
        /// documento authored guardado al portador, en vez de reconstruirlo desde el dominio.
        /// </summary>
        [Fact]
        public void GUARDA_ElCaminoDeEDICION_PASA_EL_AUTHORED_GUARDADO()
        {
            var source = Commands();

            Assert.Contains("SerializeSelectiveDesign(design, id, name, saved)", source);
        }

        [Fact]
        public void GUARDA_ElPortadorSE_USA_CuandoHayAuthoredExistente()
        {
            Assert.Contains("authored.WithDesign(", Commands());
        }

        /// <summary>Un rack NUEVO no tiene portador, asi que ese camino conserva <c>From(...)</c>.</summary>
        [Fact]
        public void GUARDA_LaInsercionNUEVA_CONSERVA_From()
        {
            Assert.Contains("SelectivePalletDesignDocument.From(design, id, name)", Commands());
        }
    }
}
