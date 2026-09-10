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
    /// I-47 gate G15 — la biblioteca es un artefacto DERIVADO, no una copia del authored.
    ///
    /// <para>
    /// Un <c>.rackcad.json</c> vive fuera del dibujo, y las variables de proyecto son del DIBUJO. Llevarse un
    /// vínculo al archivo produciría una referencia que apunta a un registro que allí no existe: al reabrirlo
    /// en otro dibujo sería un vínculo roto desde el primer segundo, o —peor— uno que resuelve contra una
    /// variable homónima ajena. Así que al exportar el valor se MATERIALIZA: se guarda el número en vigor y
    /// el vínculo se queda donde tiene sentido.
    /// </para>
    /// <para>
    /// Y la versión vuelve a la línea literal. El 2.x del dibujo existe porque allí hay bindings que una
    /// versión anterior no entendería; un archivo de biblioteca sin bindings no tiene por qué negarse a
    /// abrirse en una versión anterior. La pegajosidad de la promoción es del dibujo, no del artefacto.
    /// </para>
    /// <para>
    /// La otra mitad es de lectura: un archivo cuyo <c>SelectiveRack</c> anidado viene de un MAJOR futuro no
    /// se abre como si tal cosa. El sobre tenía guarda y el anidado no, así que un diseño que esta versión no
    /// entiende entraba entero.
    /// </para>
    /// </summary>
    public class SelectiveLibraryExportTests
    {
        private const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        private const string Token = ProjectPropertyIds.SelectiveVerticalClearanceToken;

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

        private static SelectivePalletDesignDocument Authored(
            SelectivePropertyValueDocument binding = null, string token = Token)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(6.0), RackA, "Rack A");

            if (binding != null)
            {
                doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument> { [token] = binding };
                doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return doc;
        }

        private static SelectivePropertyValueDocument Ref(string id = VarId)
            => SelectivePropertyValueDocument.ToProjectVariable(id);

        private static ProjectVariablesDocument Registro(double value = 10.0, string id = VarId)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                new ProjectVariableDocument
                {
                    VariableId = id,
                    Name = "Holgura estándar",
                    Type = VariableType.Length.ToString(),
                    Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
                },
            };

            return document;
        }

        // ================================================================ 1, 2, 3: materializar

        [Fact]
        public void UN_EXPORT_LITERAL_SIGUE_SIENDO_LITERAL()
        {
            var export = SelectiveLibraryExport.Materialize(Authored(), null);

            Assert.True(export.IsSuccess);
            Assert.Equal(6.0, export.Document.VerticalClearance);
            Assert.Null(export.Document.PropertyValues);
        }

        [Fact]
        public void UN_EXPORT_VINCULADO_MATERIALIZA_EL_EFECTIVO()
        {
            var export = SelectiveLibraryExport.Materialize(Authored(Ref()), Registro(10.0));

            Assert.True(export.IsSuccess);
            Assert.Equal(10.0, export.Document.VerticalClearance);
        }

        /// <summary>El vínculo NO viaja: apuntaría a un registro que en el archivo no existe.</summary>
        [Fact]
        public void UN_EXPORT_VINCULADO_NO_SE_LLEVA_EL_VINCULO()
        {
            var export = SelectiveLibraryExport.Materialize(Authored(Ref()), Registro(10.0));

            Assert.Null(export.Document.PropertyValues);
            Assert.False(export.Document.HasPropertyValues);
            Assert.False(export.Document.HasBindingEntry(ProjectPropertyIds.SelectiveVerticalClearance));
        }

        [Fact]
        public void EL_EXPORT_CONSERVA_EL_RESTO_DEL_DISENO()
        {
            var export = SelectiveLibraryExport.Materialize(Authored(Ref()), Registro(10.0));

            Assert.Single(export.Document.Bays);
            Assert.Equal(48.0, export.Document.PalletDepth);
            Assert.Equal(RackA, export.Document.Id);
            Assert.Equal("Rack A", export.Document.Name);
        }

        // ================================================================ 4, 5: la versión vuelve a 1.x

        [Fact]
        public void EL_ANIDADO_VUELVE_A_LA_LINEA_LITERAL()
        {
            var export = SelectiveLibraryExport.Materialize(Authored(Ref()), Registro(10.0));

            Assert.Equal(SelectivePalletDesignDocument.CurrentSchemaVersion, export.Document.SchemaVersion);
        }

        /// <summary>La pegajosidad del 2.x es del DIBUJO. Que se filtre al archivo lo haría irreabrible sin motivo.</summary>
        [Fact]
        public void EL_2X_PEGAJOSO_DEL_DIBUJO_NO_SE_FILTRA_AL_ARCHIVO()
        {
            var authored = Authored(Ref());
            Assert.Equal(SelectivePalletDesignDocument.PromotedSchemaVersion, authored.SchemaVersion);

            var export = SelectiveLibraryExport.Materialize(authored, Registro(10.0));

            Assert.NotEqual(SelectivePalletDesignDocument.PromotedSchemaVersion, export.Document.SchemaVersion);
        }

        /// <summary>Y el archivo escrito de verdad tampoco los lleva.</summary>
        [Fact]
        public void EL_ARCHIVO_ESCRITO_NO_LLEVA_NI_VINCULO_NI_VERSION_PROMOCIONADA()
        {
            var export = SelectiveLibraryExport.Materialize(Authored(Ref()), Registro(10.0));
            var json = new RackProjectStore().Serialize(RackProject.ForSelectiveRack(export.Document));

            Assert.DoesNotContain(VarId, json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("propertyValues", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("projectVariable", json, StringComparison.OrdinalIgnoreCase);
        }

        // ================================================================ 6, 7: sin artefacto

        [Fact]
        public void UN_VINCULO_ROTO_NO_PRODUCE_ARTEFACTO()
        {
            var export = SelectiveLibraryExport.Materialize(
                Authored(Ref()), Registro(10.0, id: "11111111-2222-3333-4444-555555555555"));

            Assert.False(export.IsSuccess);
            Assert.Null(export.Document);
            Assert.Contains(VarId, export.Error);
        }

        /// <summary>Jamás exportar el literal congelado como si fuera el valor: sería un número inventado.</summary>
        [Fact]
        public void UN_VINCULO_ROTO_NO_CAE_AL_LITERAL_CONGELADO()
        {
            var export = SelectiveLibraryExport.Materialize(Authored(Ref()), null);

            Assert.False(export.IsSuccess);
            Assert.Null(export.Document);
        }

        [Fact]
        public void UNA_PROPIEDAD_DESCONOCIDA_NO_PRODUCE_ARTEFACTO()
        {
            Assert.False(SelectiveLibraryExport
                .Materialize(Authored(Ref(), token: "selective.deUnBuildFuturo"), Registro(10.0)).IsSuccess);
        }

        [Fact]
        public void UN_ID_ILEGIBLE_NO_PRODUCE_ARTEFACTO()
        {
            Assert.False(SelectiveLibraryExport.Materialize(Authored(Ref("no-es-un-guid")), Registro()).IsSuccess);
        }

        [Fact]
        public void SIN_DOCUMENTO_NO_HAY_ARTEFACTO()
        {
            Assert.False(SelectiveLibraryExport.Materialize(null, Registro()).IsSuccess);
        }

        /// <summary>El camino que usa el editor: ya trae el efectivo (G12), así que solo tiene que materializarlo.</summary>
        [Fact]
        public void DESDE_EL_EFECTIVO_SE_MATERIALIZA_LITERAL_Y_SIN_VINCULO()
        {
            var document = SelectiveLibraryExport.FromEffective(Diseno(10.0), RackA, "Rack A");

            Assert.Equal(10.0, document.VerticalClearance);
            Assert.Null(document.PropertyValues);
            Assert.Equal(SelectivePalletDesignDocument.CurrentSchemaVersion, document.SchemaVersion);
        }

        // ================================================================ 8, 9, 10: la guarda del anidado

        /// <summary>La version anidada se fija ANTES de serializar: el store del proyecto no la reescribe.</summary>
        private static string Archivo(string nestedSchemaVersion)
        {
            var document = SelectivePalletDesignDocument.From(Diseno(6.0), RackA, "Rack A");
            document.SchemaVersion = nestedSchemaVersion;

            return new RackProjectStore().Serialize(RackProject.ForSelectiveRack(document));
        }

        [Fact]
        public void UN_ANIDADO_DE_MAJOR_FUTURO_NO_SE_ABRE()
        {
            var error = Assert.Throws<InvalidOperationException>(
                () => new RackProjectStore().Deserialize(Archivo("9.0")));

            Assert.Contains("selectivo", error.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Y el motivo SEMÁNTICO llega al usuario: el <c>catch</c> genérico de la biblioteca muestra el
        /// mensaje, así que no puede degenerar en «archivo no válido» sin causa.
        /// </summary>
        [Fact]
        public void EL_DIAGNOSTICO_DEL_ANIDADO_DICE_QUE_ES_UNA_VERSION_MAS_NUEVA()
        {
            var error = Assert.Throws<InvalidOperationException>(
                () => new RackProjectStore().Deserialize(Archivo("9.0")));

            Assert.DoesNotContain("no es un JSON", error.Message, StringComparison.OrdinalIgnoreCase);
            Assert.False(string.IsNullOrWhiteSpace(error.Message));
        }

        [Fact]
        public void UN_ARCHIVO_LITERAL_HEREDADO_SIGUE_ABRIENDOSE()
        {
            var project = new RackProjectStore().Deserialize(Archivo(SelectivePalletDesignDocument.CurrentSchemaVersion));

            Assert.NotNull(project.SelectiveRack);
            Assert.Equal(6.0, project.SelectiveRack.VerticalClearance);
        }

        /// <summary>La línea promocionada sigue siendo legible: la guarda mide el MAJOR, no la procedencia.</summary>
        [Fact]
        public void UN_ARCHIVO_EN_LA_LINEA_PROMOCIONADA_SIGUE_SIENDO_LEGIBLE()
        {
            var project = new RackProjectStore().Deserialize(Archivo(SelectivePalletDesignDocument.PromotedSchemaVersion));

            Assert.NotNull(project.SelectiveRack);
        }

        /// <summary>Un archivo con MAJOR futuro anidado no se ofrece en el listado.</summary>
        [Fact]
        public void UN_ANIDADO_DE_MAJOR_FUTURO_NO_SE_OFRECE_EN_LA_BIBLIOTECA()
        {
            var folder = Path.Combine(Path.GetTempPath(), "rackcad-i47-g15-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);

            try
            {
                File.WriteAllText(Path.Combine(folder, "futuro" + RackProjectStore.FileExtension), Archivo("9.0"));
                File.WriteAllText(Path.Combine(folder, "legado" + RackProjectStore.FileExtension), Archivo("1.0"));

                var entries = RackDesignLibrary.List(folder);

                Assert.Single(entries);
                Assert.DoesNotContain(entries, entry => entry.Path.Contains("futuro", StringComparison.Ordinal));
            }
            finally
            {
                Directory.Delete(folder, recursive: true);
            }
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

        private static string Window()
            => File.ReadAllText(Path.Combine(
                RepoRoot().FullName, "src", "RackCad.UI", "Systems", "Selective", "RackSelectiveWindow.xaml.cs"));

        [Fact]
        public void GUARDA_EL_EDITOR_EXPORTA_POR_LA_CAPA_PURA()
        {
            Assert.Contains("SelectiveLibraryExport.FromEffective(", Window());
        }

        /// <summary>La biblioteca sigue siendo literal-only: nada de este slice viaja al archivo.</summary>
        [Fact]
        public void GUARDA_LA_BIBLIOTECA_SIGUE_SIENDO_LITERAL()
        {
            var window = Window();

            Assert.DoesNotContain("PropertyValues", window);
            Assert.DoesNotContain("VariableId", window);
            Assert.DoesNotContain("ProjectVariablesDocument", window);
        }

        [Fact]
        public void GUARDA_EL_ANIDADO_TIENE_SU_PROPIA_COMPROBACION()
        {
            var store = File.ReadAllText(Path.Combine(
                RepoRoot().FullName, "src", "RackCad.Application", "Persistence", "RackProjectStore.cs"));

            Assert.Contains("document.SelectiveRack.SchemaVersion", store);
        }
    }
}
