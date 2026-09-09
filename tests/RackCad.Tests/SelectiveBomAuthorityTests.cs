using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G13 — la autoridad del BOM para un Selectivo vinculado.
    ///
    /// <para>
    /// El BOM es el sitio donde una divergencia se vuelve dinero. Antes de I-47, <c>RACKBOMTOTAL</c> tomaba
    /// la PRIMERA vista de cada rack como representante y cotizaba: si las hermanas discrepaban nadie se
    /// enteraba, y si un vínculo estaba roto el rack se cotizaba con su literal congelado — dos números para
    /// una misma propiedad, y ninguno fallando. Aquí se cierran las tres puertas por las que eso entraba.
    /// </para>
    /// <para>
    /// La comparación es ESTRUCTURAL sobre el estado persistido, igual que en G5, y por la misma razón:
    /// el contrato exige INCLUIR POR DEFECTO. Un comparador campo a campo es excluyente por defecto, así que
    /// el día que alguien añada una propiedad y olvide el comparador, dos hermanas divergentes empezarían a
    /// leerse como iguales.
    /// </para>
    /// <para>
    /// Y las dos asimetrías deliberadas: un vínculo roto <b>aborta el total</b> (omitir solo ese rack daría un
    /// total que parece completo), mientras que una hermana genuinamente ilegible <b>omite el rack con aviso
    /// visible</b>, que es la política histórica. Un sobre indescifrable se ignora si NO está colocado y
    /// aborta si lo está: el conteo de referencias es un hecho físico, y sin identidad no se puede demostrar
    /// que esa definición no pertenezca a un rack cotizado.
    /// </para>
    /// </summary>
    public class SelectiveBomAuthorityTests
    {
        private const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        private const string Token = ProjectPropertyIds.SelectiveVerticalClearanceToken;
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectivePalletDesign Diseno(double clearance)
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

        private static SelectivePalletDesignDocument Doc(
            double literal = 6.0,
            SelectivePropertyValueDocument binding = null,
            string schemaVersion = null,
            string extensionKey = null)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(literal), RackA, "Rack A");

            if (binding != null)
            {
                doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument> { [Token] = binding };
                doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            if (schemaVersion != null)
            {
                doc.SchemaVersion = schemaVersion;
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

        private static ProjectVariableScanEntry Vista(SelectivePalletDesignDocument doc, string def)
            => ProjectVariableScanEntry.Selective(def, RackA, doc);

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

        // ================================================================ 7, 8, 9: la autoridad multi-vista

        [Fact]
        public void UNA_SOLA_VISTA_ES_SU_PROPIA_AUTORIDAD()
        {
            var authority = BomAuthoredAuthority.Resolve(RackA, new[] { Vista(Doc(), "D1") });

            Assert.True(authority.IsSuccess);
            Assert.Equal("D1", authority.RepresentativeDefinitionId);
            Assert.Equal(6.0, authority.Authored.VerticalClearance);
        }

        [Fact]
        public void VISTAS_IGUALES_APRUEBAN_LA_PRIMERA_COMO_REPRESENTANTE()
        {
            var authority = BomAuthoredAuthority.Resolve(
                RackA, new[] { Vista(Doc(), "D1"), Vista(Doc(), "D2"), Vista(Doc(), "D3") });

            Assert.True(authority.IsSuccess);
            Assert.Equal("D1", authority.RepresentativeDefinitionId);
        }

        [Fact]
        public void VISTAS_DIVERGENTES_NO_TIENEN_AUTORIDAD()
        {
            var authority = BomAuthoredAuthority.Resolve(
                RackA, new[] { Vista(Doc(6.0), "D1"), Vista(Doc(9.0), "D2") });

            Assert.False(authority.IsSuccess);
            Assert.Equal(BomAuthorityOutcome.DivergentSiblings, authority.Outcome);
            Assert.Null(authority.Authored);
        }

        /// <summary>
        /// Divergencia SOLO en campos que un build posterior escribió. Si una versión futura guardó autoridad
        /// en una vista y no en las otras, esta versión no puede saber cuál manda — y esa es exactamente la
        /// situación que no se resuelve a ciegas.
        /// </summary>
        [Fact]
        public void DIVERGENCIA_SOLO_EN_ExtensionData_NO_TIENE_AUTORIDAD()
        {
            var authority = BomAuthoredAuthority.Resolve(
                RackA, new[] { Vista(Doc(), "D1"), Vista(Doc(extensionKey: "DeUnBuildPosterior"), "D2") });

            Assert.False(authority.IsSuccess);
        }

        [Fact]
        public void DIVERGENCIA_SOLO_EN_SchemaVersion_NO_TIENE_AUTORIDAD()
        {
            var authority = BomAuthoredAuthority.Resolve(
                RackA,
                new[] { Vista(Doc(), "D1"), Vista(Doc(schemaVersion: SelectivePalletDesignDocument.PromotedSchemaVersion), "D2") });

            Assert.False(authority.IsSuccess);
        }

        [Fact]
        public void DIVERGENCIA_SOLO_EN_PropertyValues_NO_TIENE_AUTORIDAD()
        {
            var sinVinculo = Doc();
            sinVinculo.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;

            var authority = BomAuthoredAuthority.Resolve(
                RackA, new[] { Vista(sinVinculo, "D1"), Vista(Doc(binding: Ref()), "D2") });

            Assert.False(authority.IsSuccess);
        }

        [Fact]
        public void UNA_HERMANA_ILEGIBLE_NO_TIENE_AUTORIDAD()
        {
            var authority = BomAuthoredAuthority.Resolve(
                RackA,
                new[] { Vista(Doc(), "D1"), ProjectVariableScanEntry.SelectiveUnreadableDesign("D2", RackA) });

            Assert.False(authority.IsSuccess);
            Assert.Equal(BomAuthorityOutcome.UnreadableSibling, authority.Outcome);
            Assert.Contains("D2", authority.Error);
        }

        [Fact]
        public void SIN_NINGUNA_VISTA_NO_HAY_AUTORIDAD()
        {
            Assert.False(BomAuthoredAuthority.Resolve(RackA, new ProjectVariableScanEntry[0]).IsSuccess);
            Assert.False(BomAuthoredAuthority.Resolve(RackA, null).IsSuccess);
        }

        /// <summary>Un kind que no es Selectivo no tiene diseño interior que comparar en este slice.</summary>
        [Fact]
        public void UN_KIND_AJENO_APRUEBA_SU_PRIMERA_VISTA_SIN_AUTHORED()
        {
            var authority = BomAuthoredAuthority.Resolve(
                RackA,
                new[]
                {
                    ProjectVariableScanEntry.Foreign("D1", RackA, RackEmbedDocument.KindPushBack),
                    ProjectVariableScanEntry.Foreign("D2", RackA, RackEmbedDocument.KindPushBack),
                });

            Assert.True(authority.IsSuccess);
            Assert.Equal("D1", authority.RepresentativeDefinitionId);
            Assert.Null(authority.Authored);
        }

        // ================================================================ 1, 2: el BOM del rack vinculado

        /// <summary>La cadena EXACTA que ejecuta el handler Selectivo: authored + registro → efectivo → geometría → BOM.</summary>
        private static BillOfMaterials Bom(SelectivePalletDesignDocument authored, ProjectVariablesDocument registry)
        {
            var resolution = new SelectiveEffectiveDesignResolver().Resolve(authored, registry);
            Assert.True(resolution.IsSuccess);

            var catalog = Catalog;
            return SelectiveBomBuilder.Build(new SelectiveGeometryResolver().Resolve(resolution.Design, catalog), catalog);
        }

        private static string Firma(BillOfMaterials bom)
            => string.Join(
                "|",
                bom.Lines.Select(line =>
                    line.Category + ":" + line.ProfileId + ":" + line.Length.ToString("0.###") + "x" + line.Quantity));

        [Fact]
        public void UN_SELECTIVO_LITERAL_COTIZA_COMO_SIEMPRE()
        {
            Assert.Equal(Firma(Bom(Doc(6.0), null)), Firma(Bom(Doc(6.0), Registro())));
        }

        /// <summary>
        /// La holgura elegida separa DE VERDAD los dos BOM: 6 y 10 redondean al mismo pie de poste, así que
        /// habrían dado la misma firma y el test habría pasado sin demostrar nada.
        /// </summary>
        [Fact]
        public void UN_SELECTIVO_VINCULADO_COTIZA_CON_EL_EFECTIVO()
        {
            var vinculado = Bom(Doc(6.0, Ref()), Registro(30.0));

            Assert.Equal(Firma(Bom(Doc(30.0), null)), Firma(vinculado));
            Assert.NotEqual(Firma(Bom(Doc(6.0), null)), Firma(vinculado));
        }

        // ================================================================ 5: el resultado tipado

        [Fact]
        public void EL_RESULTADO_DE_UN_VINCULO_ROTO_NOMBRA_RACK_PROPIEDAD_Y_VARIABLE()
        {
            var resolution = new SelectiveEffectiveDesignResolver()
                .Resolve(Doc(6.0, Ref()), Registro(10.0, id: "11111111-2222-3333-4444-555555555555"));

            Assert.False(resolution.IsSuccess);
            Assert.Equal(VarId, resolution.VariableId);
            Assert.Equal(ProjectPropertyIds.SelectiveVerticalClearance, resolution.PropertyId);

            var result = BomBuildResult.BrokenProjectVariableReference(
                RackA, "Rack A", resolution.PropertyId.Value, resolution.VariableId, resolution.Error);

            Assert.False(result.IsSuccess);
            Assert.Equal(BomBuildOutcome.BrokenProjectVariableReference, result.Outcome);
            Assert.Equal(RackA, result.RackId);
            Assert.Equal(ProjectPropertyIds.SelectiveVerticalClearanceToken, result.PropertyId);
            Assert.Equal(VarId, result.VariableId);
        }

        /// <summary>Un id ilegible tampoco se pierde: se reporta CRUDO, que es lo que el usuario tiene que ver.</summary>
        [Fact]
        public void UN_ID_ILEGIBLE_VIAJA_CRUDO_EN_EL_RESULTADO()
        {
            var resolution = new SelectiveEffectiveDesignResolver().Resolve(Doc(6.0, Ref("no-es-un-guid")), Registro());

            Assert.False(resolution.IsSuccess);
            Assert.Equal("no-es-un-guid", resolution.VariableId);
        }

        [Fact]
        public void UN_VINCULO_ROTO_NO_ES_UN_PAYLOAD_ILEGIBLE()
        {
            var roto = BomBuildResult.BrokenProjectVariableReference(RackA, "Rack A", Token, VarId, "roto");
            var ilegible = BomBuildResult.UnreadablePayload(RackA, "Rack A", "ilegible");

            Assert.NotEqual(roto.Outcome, ilegible.Outcome);
            Assert.False(roto.IsSuccess);
            Assert.False(ilegible.IsSuccess);
        }

        [Fact]
        public void EL_RESULTADO_CON_EXITO_LLEVA_EL_BOM()
        {
            var bom = Bom(Doc(6.0), null);
            var result = BomBuildResult.Success(bom);

            Assert.True(result.IsSuccess);
            Assert.Same(bom, result.Bom);
        }

        [Fact]
        public void SIN_AUTORIDAD_ES_UN_ESTADO_PROPIO()
        {
            var result = BomBuildResult.NoAuthoredAuthority(RackA, "Rack A", "vistas divergentes");

            Assert.Equal(BomBuildOutcome.NoAuthoredAuthority, result.Outcome);
            Assert.Null(result.Bom);
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

        private static string Plugin(params string[] relative)
            => File.ReadAllText(Path.Combine(
                new[] { RepoRoot().FullName, "src", "RackCad.Plugin" }.Concat(relative).ToArray()));

        private static string BomTotal() => Plugin("RackInventarioCommands.BomTotal.cs");

        private static string SelectiveHandler() => Plugin("KindHandlers", "SelectiveKindHandler.cs");

        private static int Count(string source, string token)
        {
            var total = 0;
            var at = source.IndexOf(token, StringComparison.Ordinal);

            while (at >= 0)
            {
                total++;
                at = source.IndexOf(token, at + token.Length, StringComparison.Ordinal);
            }

            return total;
        }

        // ---------------------------------------------------------------- el contrato compartido

        [Fact]
        public void GUARDA_EL_CONTRATO_COMPARTIDO_LLEVA_EL_REGISTRO()
        {
            var contract = Plugin("KindHandlers", "IRackKindHandler.cs");

            Assert.Contains("BomBuildResult BuildBom(", contract);
            Assert.Contains("ProjectVariablesDocument projectVariables", contract);
        }

        /// <summary>Los cinco kinds ajenos reciben el registro y NO lo usan: nada de resolución innecesaria.</summary>
        [Theory]
        [InlineData("DynamicKindHandler.cs")]
        [InlineData("PushBackKindHandler.cs")]
        [InlineData("CantileverKindHandler.cs")]
        [InlineData("CabeceraKindHandler.cs")]
        [InlineData("CamaKindHandler.cs")]
        public void GUARDA_LOS_KINDS_AJENOS_IGNORAN_EL_REGISTRO(string file)
        {
            var source = Plugin("KindHandlers", file);

            Assert.Contains("ProjectVariablesDocument projectVariables", source);
            Assert.DoesNotContain("SelectiveEffectiveDesignResolver", source);
            Assert.DoesNotContain("projectVariables.", source);
        }

        // ---------------------------------------------------------------- el resolver corre UNA vez, y en el handler

        [Fact]
        public void GUARDA_EL_HANDLER_SELECTIVO_RESUELVE_EXACTAMENTE_UNA_VEZ()
        {
            // Se cuenta la CONSTRUCCION, no la palabra: la documentación del handler también la nombra.
            Assert.Equal(1, Count(SelectiveHandler(), "new SelectiveEffectiveDesignResolver()"));
        }

        [Fact]
        public void GUARDA_RACKBOMTOTAL_NO_RESUELVE_VARIABLES()
        {
            var source = BomTotal();

            Assert.DoesNotContain("SelectiveEffectiveDesignResolver", source);
            Assert.DoesNotContain("ToProjectVariables", source);
        }

        // ---------------------------------------------------------------- un snapshot por comando

        [Fact]
        public void GUARDA_EL_REGISTRO_SE_LEE_UNA_SOLA_VEZ_POR_COMANDO()
        {
            Assert.Equal(1, Count(BomTotal(), "ProjectVariablesRegistry.Read"));
        }

        [Fact]
        public void GUARDA_NINGUN_HANDLER_TOCA_AUTOCAD_PARA_LEER_EL_REGISTRO()
        {
            foreach (var file in Directory.GetFiles(
                         Path.Combine(RepoRoot().FullName, "src", "RackCad.Plugin", "KindHandlers"), "*.cs"))
            {
                Assert.DoesNotContain("ProjectVariablesRegistry", File.ReadAllText(file));
                Assert.DoesNotContain("ProjectVariablesData", File.ReadAllText(file));
            }
        }

        // ---------------------------------------------------------------- la autoridad y las dos asimetrías

        [Fact]
        public void GUARDA_RACKBOMTOTAL_PIDE_LA_AUTORIDAD_ANTES_DE_COTIZAR()
        {
            Assert.Contains("BomAuthoredAuthority.Resolve", BomTotal());
        }

        [Fact]
        public void GUARDA_UN_VINCULO_ROTO_ABORTA_EL_TOTAL()
        {
            var source = BomTotal();

            Assert.Contains("BomBuildOutcome.BrokenProjectVariableReference", source);
        }

        [Fact]
        public void GUARDA_EL_SOBRE_INDESCIFRABLE_DISTINGUE_COLOCADO_DE_NO_COLOCADO()
        {
            var source = BomTotal();

            Assert.Contains("OuterEnvelopeInterpretable", source);
            Assert.Contains("DirectReferenceCount", source);
        }

        /// <summary>Un sobre sin identidad no se asocia a nada: nombrar la definición es todo lo que se puede hacer.</summary>
        [Fact]
        public void GUARDA_NO_SE_INVENTA_IDENTIDAD_PARA_UN_SOBRE_INDESCIFRABLE()
        {
            Assert.Contains("DefinitionId", BomTotal());
        }

        // ---------------------------------------------------------------- RACKLISTA fuera de alcance

        [Fact]
        public void GUARDA_RACKLISTA_NO_CAMBIA()
        {
            var lista = Plugin("RackInventarioCommands.cs");

            Assert.DoesNotContain("ProjectVariables", lista);
            Assert.DoesNotContain("BomAuthoredAuthority", lista);
            Assert.DoesNotContain("BuildBom", lista);
        }
    }
}
