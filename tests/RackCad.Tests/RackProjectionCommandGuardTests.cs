using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G15 — la forma de RACKPROYECTAR fijada como fuente. Ninguna suite carga el Plugin (ADR-0003) y el CI no tiene AutoCAD,
    /// asi que estas son guardas de fuente, como <see cref="RackDefinitionCreatorGuardTests"/>. El comportamiento (orden, cancelacion,
    /// un solo commit, rollback) lo prueban <c>RackProjectionCommandRunTests</c> y <c>RackProjectionMaterializationRunTests</c>
    /// sobre un AutoCAD falso; esto solo demuestra que el Plugin usa esa forma y ninguna otra.
    /// </summary>
    public class RackProjectionCommandGuardTests
    {
        private static readonly string[] PluginFiles =
        {
            "src/RackCad.Plugin/RackProyectarCommands.cs",
            "src/RackCad.Plugin/Views/RackProjectionSnapshotReader.cs",
            "src/RackCad.Plugin/Views/RackProjectionKindSessions.cs",
            "src/RackCad.Plugin/Views/RackProjectionCommandPort.cs",
            "src/RackCad.Plugin/Views/RackProjectionWriteScope.cs",
        };

        private static readonly string[] ApplicationFiles =
        {
            "src/RackCad.Application/Views/Placement/RackProjectionMaterialization.cs",
            "src/RackCad.Application/Views/Placement/RackProjectionCommandRun.cs",
            "src/RackCad.Application/Views/Placement/RackProjectionReport.cs",
            "src/RackCad.Application/Views/Placement/RackProjectionSelectionFilter.cs",
        };

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            return dir;
        }

        private static string Raw(string path)
        {
            var full = Path.Combine(RepoRoot().FullName, path.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(full), "falta el archivo de G15: " + path);
            return File.ReadAllText(full);
        }

        /// <summary>El codigo sin comentarios ni cadenas de documentacion: lo que dice lo que el archivo NO hace no cuenta.</summary>
        private static string Code(string path)
            => string.Join("\n", Raw(path).Split('\n').Select(line =>
            {
                var comment = line.IndexOf("//", StringComparison.Ordinal);
                return comment >= 0 ? line.Substring(0, comment) : line;
            }));

        private static int Count(string source, string needle)
            => Regex.Matches(source, Regex.Escape(needle)).Count;

        private static string AllPluginCode() => string.Join("\n", PluginFiles.Select(Code));

        private static string AllApplicationCode() => string.Join("\n", ApplicationFiles.Select(Code));

        // ================================================================ A, B: los comandos

        [Fact]
        public void G15_A_RACKPROYECTAR_EXISTE_UNA_SOLA_VEZ()
        {
            var command = Code("src/RackCad.Plugin/RackProyectarCommands.cs");
            Assert.Equal(1, Count(command, "[CommandMethod(\"RACKPROYECTAR\")]"));
        }

        [Fact]
        public void G15_B_EL_ALIAS_RPY_EXISTE_Y_DELEGA_EN_EL_COMANDO()
        {
            var command = Code("src/RackCad.Plugin/RackProyectarCommands.cs");
            Assert.Equal(1, Count(command, "[CommandMethod(\"RPY\")]"));
            Assert.Matches(new Regex(@"AliasRackProyectar\(\)\s*=>\s*RackProyectar\(\)"), command);
        }

        [Fact]
        public void G15_B_NO_HAY_UNA_SEGUNDA_FAMILIA_DE_COMANDOS()
        {
            var names = CustomPropertiesCommandGuardTests.PluginCommandRegistrations()
                .Where(r => r.Path.EndsWith("RackProyectarCommands.cs", StringComparison.Ordinal))
                .Select(r => r.Name)
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(new[] { "RACKPROYECTAR", "RPY" }, names);
            Assert.Empty(CustomPropertiesCommandGuardTests.PluginCommandRegistrations()
                .Where(r => r.Name.IndexOf("PROYECTAR", StringComparison.Ordinal) >= 0
                    && !r.Path.EndsWith("RackProyectarCommands.cs", StringComparison.Ordinal)));
        }

        [Fact]
        public void G15_NO_SE_CREA_NINGUN_TIPO_LLAMADO_RPY_NI_RACKPROYECTARCOMMANDS_FUERA_DEL_ARCHIVO_DEL_COMANDO()
        {
            var command = Code("src/RackCad.Plugin/RackProyectarCommands.cs");
            Assert.Equal(1, Count(command, "class RackProyectarCommands"));
        }

        [Fact]
        public void G15_EL_COMANDO_ENTREGA_LA_OPERACION_A_LA_CORRIDA_Y_NO_DECIDE_POLITICA()
        {
            var command = Code("src/RackCad.Plugin/RackProyectarCommands.cs");

            Assert.Contains("RackProjectionCommandRun.Execute(", command);
            Assert.Contains("catch (System.Exception ex)", command);
            Assert.Contains("RackCommandSupport.Report(ex);", command);
            Assert.DoesNotContain("RackGroupPlacementPlan", command);
            Assert.DoesNotContain("StartTransaction", command);
            Assert.DoesNotContain("GetPoint(", command);
        }

        // ================================================================ C, D, E: el SNAPSHOT y los bloqueos antes del punto

        [Fact]
        public void G15_C_EL_SNAPSHOT_ES_UNA_SOLA_LECTURA_DEL_DIBUJO_Y_NO_PIDE_PUNTOS()
        {
            var reader = Code("src/RackCad.Plugin/Views/RackProjectionSnapshotReader.cs");

            Assert.Equal(1, Count(reader, "RackSiblingScan.CaptureDrawing("));
            Assert.DoesNotContain("GetPoint", reader);
            Assert.DoesNotContain("StartOpenCloseTransaction", reader);
            Assert.Contains("RackSourcePlacementCaptureAdapter.Capture(", reader);
            Assert.Contains("RackSourceTransformClassifier.Classify(", reader);
            Assert.Contains("RackProjectionSelectionFilter.Build(", reader);
        }

        [Fact]
        public void G15_C_EL_SNAPSHOT_LEE_LOS_HECHOS_DE_D08F_SIN_CLASIFICARLOS()
        {
            var reader = Code("src/RackCad.Plugin/Views/RackProjectionSnapshotReader.cs");

            foreach (var fact in new[] { "IsDynamicBlock", "IsAnonymous", "IsFromExternalReference", "OwnerId", "CurrentUserCoordinateSystem" })
            {
                Assert.Contains(fact, reader);
            }
        }

        [Fact]
        public void G15_D_SOLO_LA_CORRIDA_DE_APLICACION_LLAMA_AL_PLAN_PURO_Y_EL_PLUGIN_NO_LO_REPITE()
        {
            var plugin = AllPluginCode();
            var run = Code("src/RackCad.Application/Views/Placement/RackProjectionCommandRun.cs");

            Assert.Equal(1, Count(run, "RackGroupPlacementPlan.Create("));
            Assert.DoesNotContain("RackGroupPlacementPlan.Create(", plugin);
            Assert.DoesNotContain("RackProjectionPipeline", plugin);
        }

        [Fact]
        public void G15_D_LA_CORRIDA_PIDE_LOS_PUNTOS_DESPUES_DEL_PLAN_Y_ANTES_DE_ESCRIBIR()
        {
            var run = Code("src/RackCad.Application/Views/Placement/RackProjectionCommandRun.cs");

            var capture = run.IndexOf("port.Capture()", StringComparison.Ordinal);
            var plan = run.IndexOf("RackGroupPlacementPlan.Create(", StringComparison.Ordinal);
            var basePick = run.IndexOf("port.PickBasePoint()", StringComparison.Ordinal);
            var import = run.IndexOf("port.ImportAndObserve(", StringComparison.Ordinal);
            var write = run.IndexOf("RackProjectionMaterializationRun.Execute(", StringComparison.Ordinal);

            Assert.True(0 <= capture && capture < plan && plan < basePick && basePick < import && import < write);
        }

        // ================================================================ F, G, H: puntos

        [Fact]
        public void G15_F_G_H_LOS_PUNTOS_USAN_GETPOINT_CON_ALLOWNONE_Y_MAPEAN_OK_NONE_CANCEL_Y_ERROR()
        {
            var port = Code("src/RackCad.Plugin/Views/RackProjectionCommandPort.cs");

            Assert.Equal(2, Count(port, "GetPoint("));
            // Both point prompts allow None. (G16 C16-06 adds the orientation keyword prompt BEFORE them, whose AllowNone is the
            // Enter = Proyectada default; it is counted apart in the C16-06 guard.)
            var points = port.Substring(port.IndexOf("public RackProjectionPick PickBasePoint", StringComparison.Ordinal));
            Assert.Equal(2, Count(points, "AllowNone = true"));
            Assert.Contains("PromptStatus.OK", port);
            Assert.Contains("RackProjectionPick.Ok(", port);
            Assert.Contains("PromptStatus.None", port);
            Assert.Contains("RackProjectionPick.None()", port);
            Assert.Contains("PromptStatus.Cancel", port);
            Assert.Contains("RackProjectionPick.Cancel()", port);
            Assert.Contains("RackProjectionPick.Error(", port);
            Assert.DoesNotContain("Keywords", port.Substring(port.IndexOf("GetPoint(", StringComparison.Ordinal)));
        }

        [Fact]
        public void G15_LA_PREGUNTA_DE_LA_CLASE_LLEVA_SU_LISTA_ENTRE_CORCHETES()
        {
            var port = Code("src/RackCad.Plugin/Views/RackProjectionCommandPort.cs");

            // PromptKeywordOptions(messageAndKeywords, globalKeywords): sin la lista entre corchetes en el primer argumento, AutoCAD lanza
            // «No bracketed keyword list» (hallazgo de la primera corrida del Owner).
            Assert.Contains("PromptKeywordOptions(\"\\nClase de vista a proyectar [Frontal/Lateral/Planta]\", \"Frontal Lateral Planta\")", port);
            Assert.DoesNotContain("AppendKeywordsToMessage", port);
        }

        [Fact]
        public void G15_F_LOS_PUNTOS_SE_CONVIERTEN_DEL_SCP_AL_UNIVERSAL()
        {
            var port = Code("src/RackCad.Plugin/Views/RackProjectionCommandPort.cs");

            Assert.Contains("CurrentUserCoordinateSystem", port);
            Assert.Contains("TransformBy(", port);
        }

        // ================================================================ I..R: AUTH-15 y la transaccion del llamador

        [Fact]
        public void G15_I_J_EL_PLUGIN_CONSUME_RackDefinitionCreator_CreateInTransaction()
        {
            var scope = Code("src/RackCad.Plugin/Views/RackProjectionWriteScope.cs");

            Assert.Contains("RackDefinitionCreator.CreateInTransaction(", scope);
            Assert.Contains("RackDefinitionCreationFailure.MissingLibraryBlocks", scope);
            Assert.Contains("result.IsSuccess", scope);
        }

        [Fact]
        public void G15_J_AUTH15_RECIBE_LA_TRANSACCION_QUE_ABRIO_EL_LLAMADOR()
        {
            var scope = Code("src/RackCad.Plugin/Views/RackProjectionWriteScope.cs");

            Assert.Matches(new Regex(@"CreateInTransaction\(\s*database,\s*transaction,"), scope);
            Assert.Equal(1, Count(scope, "StartTransaction()"));
        }

        [Fact]
        public void G15_K_NINGUN_ARCHIVO_DE_G15_USA_OpenCloseTransaction()
        {
            Assert.DoesNotContain("OpenCloseTransaction", AllPluginCode());
            Assert.DoesNotContain("StartOpenCloseTransaction", AllPluginCode());
        }

        [Fact]
        public void G15_L_EL_LLAMADOR_POSEE_EL_LOCKDOCUMENT_Y_LA_UNICA_TRANSACCION()
        {
            var scope = Code("src/RackCad.Plugin/Views/RackProjectionWriteScope.cs");

            Assert.Equal(1, Count(scope, "LockDocument()"));
            Assert.Equal(1, Count(scope, "StartTransaction()"));
            Assert.DoesNotContain("LockDocument", Code("src/RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs"));
        }

        [Fact]
        public void G15_N_HAY_UN_SOLO_COMMIT_EN_TODO_EL_CODIGO_DE_G15()
        {
            Assert.Equal(1, Count(Code("src/RackCad.Plugin/Views/RackProjectionWriteScope.cs"), ".Commit()"));
            Assert.Equal(1, Count(AllPluginCode(), ".Commit()"));
        }

        [Fact]
        public void G15_N_EL_PLUGIN_NO_COMPENSA_A_MANO_EL_ROLLBACK()
        {
            var scope = Code("src/RackCad.Plugin/Views/RackProjectionWriteScope.cs");

            foreach (var forbidden in new[] { ".Abort(", ".Erase(", "PurgeAfterCommit", "Purge(", "EraseUnreferencedDefinition", "TryCleanupDefinition" })
            {
                Assert.DoesNotContain(forbidden, scope);
            }
        }

        [Fact]
        public void G15_Q_R_LA_REFERENCIA_LA_CREA_G15_DESPUES_DE_LA_DEFINICION_EN_LA_MISMA_TRANSACCION()
        {
            var scope = Code("src/RackCad.Plugin/Views/RackProjectionWriteScope.cs");
            var creator = Code("src/RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs");

            Assert.Contains("new BlockReference(", scope);
            Assert.Contains("AppendEntity(", scope);
            Assert.Contains("AddNewlyCreatedDBObject(", scope);
            Assert.DoesNotContain("new BlockReference(", creator);
            Assert.True(
                scope.IndexOf("CreateInTransaction(", StringComparison.Ordinal)
                < scope.IndexOf("new BlockReference(", StringComparison.Ordinal));
        }

        [Fact]
        public void G15_Q_EL_PLUGIN_NO_DUPLICA_LA_CREACION_DE_LA_DEFINICION()
        {
            var plugin = AllPluginCode();

            foreach (var forbidden in new[] { "CreateSystemBlock(", "CreateBlockDefinitionNamed(", "RackBlockData.Write(", "SystemBlockWriter.CreatePreparedBlock" })
            {
                Assert.DoesNotContain(forbidden, plugin);
            }
        }

        [Fact]
        public void G15_Q_LOS_DOS_ORIGENES_DE_FAMILIA_USAN_LAS_SOBRECARGAS_DE_AUTH15()
        {
            var scope = Code("src/RackCad.Plugin/Views/RackProjectionWriteScope.cs");

            Assert.Equal(2, Count(scope, "RackDefinitionCreator.CreateInTransaction("));
            Assert.Contains("view.HeaderPlan", scope);
            Assert.Contains("view.CantileverPlan", scope);
            Assert.Contains("new LateralHeaderDrawer()", scope);
        }

        // ================================================================ S, T, W: identidad y definicion

        [Fact]
        public void G15_S_T_NINGUN_ARCHIVO_DE_G15_CREA_NI_REASIGNA_UNA_IDENTIDAD()
        {
            var all = AllPluginCode() + "\n" + AllApplicationCode();

            foreach (var forbidden in new[] { "Guid.NewGuid", "NewRackId", "CreateRackId", "Restamp", "RackEnvelopeRestamp", "RackDuplicationPlan", "CreateDestinationAssigner" })
            {
                Assert.DoesNotContain(forbidden, all);
            }
        }

        [Fact]
        public void G15_S_EL_SOBRE_SE_COMPONE_ANTES_DE_AUTH15_Y_ALLI_NO_SE_RECOMPONE()
        {
            var sessions = Code("src/RackCad.Plugin/Views/RackProjectionKindSessions.cs");
            var scope = Code("src/RackCad.Plugin/Views/RackProjectionWriteScope.cs");

            Assert.Contains("RackProductPreparer<", sessions);
            Assert.Contains("PrepareExisting(", sessions);
            Assert.DoesNotContain("RackViewEnvelopeComposition", scope);
            Assert.DoesNotContain("RackEmbedComposer", scope);
            Assert.Contains("view.Envelope", scope);
        }

        [Fact]
        public void G15_W_LA_DEFINICION_NUEVA_NACE_CON_ORIGIN_CERO()
        {
            var scope = Code("src/RackCad.Plugin/Views/RackProjectionWriteScope.cs");

            Assert.DoesNotContain(".Origin =", scope);
            Assert.DoesNotContain("Origin", scope.Replace("DefinitionOrigin", string.Empty).Replace("Point3d.Origin", string.Empty));
        }

        // ================================================================ AF, AG, AH, AI: una sola autoridad y sin lote parcial

        [Fact]
        public void G15_AF_AG_DESPUES_DEL_PUNTO_EL_PLUGIN_NO_RESUELVE_NI_PREPARA_NI_COMPARA()
        {
            foreach (var path in new[]
            {
                "src/RackCad.Plugin/Views/RackProjectionCommandPort.cs",
                "src/RackCad.Plugin/Views/RackProjectionWriteScope.cs",
            })
            {
                var code = Code(path);
                foreach (var forbidden in new[] { "Resolve(", "Prepare(", "PrepareExisting(", "Compare(", "RackResolvePorts", "RackAuthoredComparatorPorts", "Classify(" })
                {
                    Assert.DoesNotContain(forbidden, code);
                }
            }
        }

        [Fact]
        public void G15_AF_RESOLVE_Y_PREPARE_VIVEN_SOLO_EN_LAS_SESIONES_DEL_SNAPSHOT()
        {
            var sessions = Code("src/RackCad.Plugin/Views/RackProjectionKindSessions.cs");

            Assert.Contains("RackResolvePorts.", sessions);
            Assert.Contains("RackHeaderPieceRequirementExtractorV2.Extract", sessions);
            Assert.Contains("RackAuthoredComparatorPorts.", sessions);
        }

        [Fact]
        public void G15_AH_NINGUN_ARCHIVO_DE_G15_IMPORTA_ESTADOS_DE_LOTE_PARCIAL()
        {
            var all = AllPluginCode() + "\n" + AllApplicationCode();

            foreach (var forbidden in new[] { "PLACEMENT_CANCELLED_PARTIAL_BATCH", "PLACEMENT_FAILED_PARTIAL_BATCH", "REDRAW_ROLLED_BACK", "RackViewBatchDriver", "RackViewBatchPlan", "PlaceDefinitionWithStatus", "RackSingleViewPlacement" })
            {
                Assert.DoesNotContain(forbidden, all);
            }
        }

        [Fact]
        public void G15_AI_EL_COMANDO_NO_REGENERA_NI_PURGA()
        {
            var all = AllPluginCode() + "\n" + AllApplicationCode();

            foreach (var forbidden in new[] { "Regen(", "ApplyRegen", "PurgeAfterCommit", "UpdateScreen" })
            {
                Assert.DoesNotContain(forbidden, all);
            }
        }

        // ================================================================ politica no duplicada, biblioteca, capas

        [Fact]
        public void G15_LA_POLITICA_DE_G14_NO_SE_DUPLICA_EN_EL_PLUGIN()
        {
            var plugin = AllPluginCode();

            foreach (var forbidden in new[]
            {
                "RackRigidPlacementPolicy", "RackOrthographicPlacementPolicy", "CommonTransform2D", "SourceGroupFrame",
                "TargetGroupFrame", "Math.Atan2", "NormalizePi", "Transform2D", "RackProjectionClassMapping",
                "new RackSourcePlacementSnapshot", "RackScaleSign",
            })
            {
                Assert.DoesNotContain(forbidden, plugin);
            }
        }

        [Fact]
        public void G15_LA_BIBLIOTECA_EXTERNA_SE_OBSERVA_ANTES_Y_EL_DIBUJO_DESPUES_DE_IMPORTAR()
        {
            var sessions = Code("src/RackCad.Plugin/Views/RackProjectionKindSessions.cs");
            var port = Code("src/RackCad.Plugin/Views/RackProjectionCommandPort.cs");

            Assert.Contains("new AutoCadExternalLibraryBlockQuery()", sessions);
            Assert.Contains("LibraryPieceAvailabilityFlowV2.Observe(", sessions);
            Assert.Contains("allowImport: false", sessions);
            Assert.Contains("BlockLibraryImporter.EnsureRequirements(", port);
            Assert.Contains("new AutoCadLibraryBlockQuery(", port);
            Assert.DoesNotContain("BlockLibraryImporter", sessions);
            Assert.DoesNotContain("AutoCadExternalLibraryBlockQuery", port);
        }

        [Fact]
        public void G15_LA_IMPORTACION_OCURRE_FUERA_DE_LA_TRANSACCION_DE_ESCRITURA()
        {
            var scope = Code("src/RackCad.Plugin/Views/RackProjectionWriteScope.cs");

            Assert.DoesNotContain("BlockLibraryImporter", scope);
            Assert.DoesNotContain("LibraryBlockAvailabilityFlow", scope);
        }

        [Fact]
        public void G15_LAS_CLAVES_DE_BIBLIOTECA_NO_SE_SANEAN_EN_EL_PLUGIN()
        {
            var plugin = AllPluginCode();

            Assert.DoesNotContain("SanitizeBlockName", plugin);
            Assert.DoesNotContain("blocks-library", plugin);
        }

        [Fact]
        public void G15_LA_APLICACION_NUEVA_NO_CONOCE_AUTOCAD()
        {
            var all = AllApplicationCode();

            Assert.DoesNotContain("Autodesk", all);
            Assert.DoesNotContain("ObjectId", all);
        }

        [Fact]
        public void G15_EL_TARGET_SE_ESCRIBE_EN_EL_ESPACIO_MODELO_SIN_TOCAR_LA_PRESENTACION()
        {
            var scope = Code("src/RackCad.Plugin/Views/RackProjectionWriteScope.cs");

            Assert.Contains("SymbolUtilityServices.GetBlockModelSpaceId(", scope);
            Assert.DoesNotContain("LayerId =", scope);
            Assert.DoesNotContain("ColorIndex", scope);
        }
    }
}
