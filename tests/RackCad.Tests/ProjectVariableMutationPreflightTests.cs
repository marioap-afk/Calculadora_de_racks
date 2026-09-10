using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G6 — el preflight PURO: toda operacion produce un plan COMPLETO o no produce nada.
    ///
    /// <para>
    /// La regla sin excepcion es la del plan vacio. Un preflight fallido no deja un plan parcial, ni un plan
    /// con los racks que si pasaron: deja <b>nada</b>. Es la forma verificable de la promesa "cero mutacion",
    /// y lo que convierte «aborta» en algo que se puede probar en vez de solo afirmar. La atomicidad FISICA
    /// es de un gate posterior; aqui se cierra la del plan.
    /// </para>
    /// <para>
    /// El artefacto es puro a proposito: sin <c>ObjectId</c>, sin <c>Database</c>, sin <c>Transaction</c>. Por
    /// eso toda la semantica de las ocho operaciones —incluida la de aborto— se verifica sin dibujo.
    /// </para>
    /// </summary>
    public class ProjectVariableMutationPreflightTests
    {
        private const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string RackB = "5d9e2a10-77b4-4c31-8e06-2f9a4b7c1d38";
        private const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        private const string Otra = "1c7f0b52-4a88-4d0e-9a33-77b2e6c40915";
        private const string Token = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        private static VariableId Objetivo => VariableId.Parse(VarId);
        private static PropertyId Piloto => ProjectPropertyIds.SelectiveVerticalClearance;

        private static SelectivePalletDesign Diseno(double clearance = 6.0)
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

        private static SelectivePalletDesignDocument Doc(
            double clearance = 6.0,
            string rackId = RackA,
            string variableId = null,
            string schemaVersion = null)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(clearance), rackId, "Rack " + rackId.Substring(0, 4));

            if (variableId != null)
            {
                doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
                {
                    [Token] = SelectivePropertyValueDocument.ToProjectVariable(variableId),
                };
                doc.SchemaVersion = schemaVersion ?? SelectivePalletDesignDocument.PromotedSchemaVersion;
            }
            else if (schemaVersion != null)
            {
                doc.SchemaVersion = schemaVersion;
            }

            return doc;
        }

        private static ProjectVariableScanEntry Vista(SelectivePalletDesignDocument doc, string def, string rackId = RackA)
            => ProjectVariableScanEntry.Selective(def, rackId, doc);

        private static ProjectVariablesDocument Registro(params (string Id, double Valor)[] variables)
        {
            var registro = ProjectVariablesDocument.CreateNew();
            foreach (var v in variables)
            {
                registro.Variables.Add(new ProjectVariableDocument
                {
                    VariableId = v.Id,
                    Name = "Holgura",
                    Type = "Length",
                    Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = v.Valor },
                });
            }

            return registro;
        }

        private static void AssertPlanVacio(VariableMutationPreflightResult r)
        {
            Assert.False(r.IsSuccess);
            Assert.True(r.Plan.IsEmpty);
            Assert.Equal(RegistryMutationKind.None, r.Plan.RegistryMutation.Kind);
            Assert.Empty(r.Plan.RackMutations);
        }

        // ================================================================ registry-only

        [Fact]
        public void Create_ES_REGISTRY_ONLY_SinScanNiRedibujo()
        {
            var r = ProjectVariableMutationPreflight.Create(
                "Holgura estandar", VariableType.Length, VariableDefinition.Literal(6.0));

            Assert.True(r.IsSuccess);
            Assert.Equal(RegistryMutationKind.Add, r.Plan.RegistryMutation.Kind);
            Assert.Empty(r.Plan.RackMutations);
            Assert.False(r.Plan.RegistryMutation.VariableId.IsEmpty);
        }

        [Fact]
        public void Create_AcuñaUnVariableIdNUEVO()
        {
            var a = ProjectVariableMutationPreflight.Create("A", VariableType.Length, VariableDefinition.Literal(6.0));
            var b = ProjectVariableMutationPreflight.Create("A", VariableType.Length, VariableDefinition.Literal(6.0));

            Assert.NotEqual(a.Plan.RegistryMutation.VariableId, b.Plan.RegistryMutation.VariableId);
        }

        [Fact]
        public void Rename_CONSERVA_EL_VARIABLEID_Y_NO_TOCA_NINGUN_RACK()
        {
            var r = ProjectVariableMutationPreflight.Rename(Registro((VarId, 6.0)), Objetivo, "Otro nombre");

            Assert.True(r.IsSuccess);
            Assert.Equal(RegistryMutationKind.Rename, r.Plan.RegistryMutation.Kind);
            Assert.Equal(Objetivo, r.Plan.RegistryMutation.VariableId);
            Assert.Equal("Otro nombre", r.Plan.RegistryMutation.Name);
            Assert.Empty(r.Plan.RackMutations);
        }

        [Fact]
        public void Rename_NO_NECESITA_BARRIDO_DE_RACKS()
        {
            // Ni siquiera se le pasan entradas: renombrar va por VariableId y no toca a ningun consumidor.
            var r = ProjectVariableMutationPreflight.Rename(Registro((VarId, 6.0)), Objetivo, "Otro");

            Assert.True(r.IsSuccess);
        }

        [Fact]
        public void Rename_DeUnaVariableINEXISTENTE_FALLA()
            => AssertPlanVacio(ProjectVariableMutationPreflight.Rename(Registro(), Objetivo, "Otro"));

        // ================================================================ prueba 3 — Link

        [Fact]
        public void Prueba3_Link_CONSERVA_EL_LITERAL_ANADE_EL_BINDING_PROMUEVE_Y_RESUELVE()
        {
            var entries = new[] { Vista(Doc(6.0), "D1"), Vista(Doc(6.0), "D2") };

            var r = ProjectVariableMutationPreflight.Link(Registro((VarId, 11.0)), RackA, Piloto, Objetivo, entries);

            Assert.True(r.IsSuccess);
            var mutacion = Assert.Single(r.Plan.RackMutations);

            Assert.Equal(6.0, mutacion.AuthoredOutput.VerticalClearance);
            Assert.True(mutacion.AuthoredOutput.TryGetBinding(Piloto, out var id));
            Assert.Equal(Objetivo, id);
            Assert.Equal(SelectivePalletDesignDocument.PromotedSchemaVersion, mutacion.AuthoredOutput.SchemaVersion);
            Assert.Equal(11.0, mutacion.EffectiveOutput.VerticalClearance);
        }

        [Fact]
        public void Prueba31_Link_INCLUYE_COMO_DESTINO_TODAS_LAS_VISTAS()
        {
            var entries = new[] { Vista(Doc(), "D1"), Vista(Doc(), "D2"), Vista(Doc(), "D3") };

            var r = ProjectVariableMutationPreflight.Link(Registro((VarId, 11.0)), RackA, Piloto, Objetivo, entries);

            Assert.Equal(3, Assert.Single(r.Plan.RackMutations).Destinations.Count);
        }

        [Fact]
        public void Link_SOBRE_UN_RACK_TODO_NEGATIVE_SI_ENTRA()
        {
            // Familia B: sin esto Link no podria vincular nada.
            var r = ProjectVariableMutationPreflight.Link(
                Registro((VarId, 11.0)), RackA, Piloto, Objetivo, new[] { Vista(Doc(), "D1") });

            Assert.True(r.IsSuccess);
        }

        [Fact]
        public void Link_A_UNA_VARIABLE_INEXISTENTE_FALLA_SinCrearUnVinculoRoto()
            => AssertPlanVacio(ProjectVariableMutationPreflight.Link(
                Registro(), RackA, Piloto, Objetivo, new[] { Vista(Doc(), "D1") }));

        [Fact]
        public void Link_ConHermanasDIVERGENTES_ABORTA()
            => AssertPlanVacio(ProjectVariableMutationPreflight.Link(
                Registro((VarId, 11.0)), RackA, Piloto, Objetivo,
                new[] { Vista(Doc(6.0), "D1"), Vista(Doc(7.0), "D2") }));

        [Fact]
        public void Link_NO_MUTA_EL_DOCUMENTO_DE_ENTRADA()
        {
            var original = Doc(6.0);

            ProjectVariableMutationPreflight.Link(
                Registro((VarId, 11.0)), RackA, Piloto, Objetivo, new[] { Vista(original, "D1") });

            Assert.Null(original.PropertyValues);
            Assert.Equal(SelectivePalletDesignDocument.CurrentSchemaVersion, original.SchemaVersion);
        }

        // ================================================================ prueba 4 — Unlink

        [Fact]
        public void Prueba4_Unlink_MATERIALIZA_EL_EFECTIVO_QUITA_EL_BINDING_Y_NO_BAJA_EL_SCHEMA()
        {
            var entries = new[] { Vista(Doc(6.0, variableId: VarId), "D1") };

            var r = ProjectVariableMutationPreflight.Unlink(Registro((VarId, 11.0)), RackA, Piloto, entries);

            Assert.True(r.IsSuccess);
            var m = Assert.Single(r.Plan.RackMutations);

            Assert.Equal(11.0, m.AuthoredOutput.VerticalClearance);
            Assert.False(m.AuthoredOutput.HasBindingEntry(Piloto));
            Assert.Equal(SelectivePalletDesignDocument.PromotedSchemaVersion, m.AuthoredOutput.SchemaVersion);
        }

        [Fact]
        public void Prueba4_Unlink_TIENE_EFECTO_GEOMETRICO_NULO()
        {
            var entries = new[] { Vista(Doc(6.0, variableId: VarId), "D1") };
            var registro = Registro((VarId, 11.0));

            var antes = new RackCad.Application.Systems.Selective.SelectiveEffectiveDesignResolver()
                .Resolve(entries[0].Authored, registro).Design.VerticalClearance;

            var m = Assert.Single(ProjectVariableMutationPreflight.Unlink(registro, RackA, Piloto, entries).Plan.RackMutations);

            Assert.Equal(antes, m.EffectiveOutput.VerticalClearance);
        }

        [Fact]
        public void Unlink_SinBinding_FALLA()
            => AssertPlanVacio(ProjectVariableMutationPreflight.Unlink(
                Registro((VarId, 11.0)), RackA, Piloto, new[] { Vista(Doc(), "D1") }));

        [Fact]
        public void Unlink_ConReferenciaROTA_FALLA_Y_REMITE_A_LA_REPARACION()
        {
            // Unlink NO cae al literal en silencio: ese caso entra por RepairBroken.
            var r = ProjectVariableMutationPreflight.Unlink(
                Registro(), RackA, Piloto, new[] { Vista(Doc(6.0, variableId: VarId), "D1") });

            AssertPlanVacio(r);
        }

        // ================================================================ prueba 5 — RepairBroken

        [Fact]
        public void Prueba5_RepairBroken_NO_ES_ALCANZABLE_SIN_ACCION_EXPLICITA()
        {
            var r = ProjectVariableMutationPreflight.RepairBroken(
                Registro(), RackA, Piloto, new[] { Vista(Doc(6.0, variableId: VarId), "D1") }, confirmed: false);

            AssertPlanVacio(r);
            Assert.Contains("geometr", r.Error);
        }

        [Fact]
        public void Prueba5_RepairBroken_USA_EL_LITERAL_ALMACENADO_QUITA_EL_BINDING_Y_CONSERVA_EL_SCHEMA()
        {
            var r = ProjectVariableMutationPreflight.RepairBroken(
                Registro(), RackA, Piloto, new[] { Vista(Doc(6.0, variableId: VarId), "D1") }, confirmed: true);

            Assert.True(r.IsSuccess);
            var m = Assert.Single(r.Plan.RackMutations);

            Assert.Equal(6.0, m.AuthoredOutput.VerticalClearance);
            Assert.False(m.AuthoredOutput.HasBindingEntry(Piloto));
            Assert.Equal(SelectivePalletDesignDocument.PromotedSchemaVersion, m.AuthoredOutput.SchemaVersion);
        }

        /// <summary>
        /// La secuencia que el ADR fija: SOLO despues de eliminar el binding el literal vuelve a gobernar.
        /// Nunca "binding presente + literal gobernando".
        /// </summary>
        [Fact]
        public void Prueba5_SOLO_TRAS_QUITAR_EL_BINDING_EL_LITERAL_GOBIERNA()
        {
            var m = Assert.Single(ProjectVariableMutationPreflight.RepairBroken(
                Registro(), RackA, Piloto, new[] { Vista(Doc(6.0, variableId: VarId), "D1") }, confirmed: true)
                .Plan.RackMutations);

            Assert.False(m.AuthoredOutput.HasBindingEntry(Piloto));
            Assert.Equal(6.0, m.EffectiveOutput.VerticalClearance);
        }

        [Fact]
        public void RepairBroken_SOBRE_UN_BINDING_SANO_FALLA_NoEsUnaViaDeDesvinculado()
        {
            var r = ProjectVariableMutationPreflight.RepairBroken(
                Registro((VarId, 11.0)), RackA, Piloto, new[] { Vista(Doc(6.0, variableId: VarId), "D1") }, confirmed: true);

            AssertPlanVacio(r);
        }

        [Fact]
        public void RepairBroken_SinBinding_FALLA()
            => AssertPlanVacio(ProjectVariableMutationPreflight.RepairBroken(
                Registro(), RackA, Piloto, new[] { Vista(Doc(), "D1") }, confirmed: true));

        // ================================================================ prueba 32 — Delete

        [Fact]
        public void Prueba32_Delete_CON_CONSUMIDORES_ES_BLOQUEADO_SIN_MUTACION_Y_CON_RESUMENES()
        {
            var entries = new[] { Vista(Doc(6.0, variableId: VarId), "D1") };

            var r = ProjectVariableMutationPreflight.Delete(Registro((VarId, 6.0)), Objetivo, entries);

            Assert.Equal(VariableMutationOutcome.BlockedByConsumers, r.Outcome);
            AssertPlanVacio(r);

            var resumen = Assert.Single(r.BlockingConsumers);
            Assert.Equal(RackA, resumen.RackId);
            Assert.Contains(Token, resumen.PropertyIds);
        }

        [Fact]
        public void Delete_SIN_CONSUMIDORES_SI_PROCEDE()
        {
            var r = ProjectVariableMutationPreflight.Delete(
                Registro((VarId, 6.0)), Objetivo, new[] { Vista(Doc(), "D1") });

            Assert.True(r.IsSuccess);
            Assert.Equal(RegistryMutationKind.Remove, r.Plan.RegistryMutation.Kind);
            Assert.Empty(r.Plan.RackMutations);
        }

        [Fact]
        public void Delete_DeUnaVariableINEXISTENTE_FALLA()
            => AssertPlanVacio(ProjectVariableMutationPreflight.Delete(Registro(), Objetivo, new ProjectVariableScanEntry[0]));

        // ================================================================ prueba 7 y 17 — ChangeValue

        [Fact]
        public void ChangeValue_PLANIFICA_TODOS_LOS_CONSUMIDORES()
        {
            var entries = new[]
            {
                Vista(Doc(6.0, variableId: VarId), "D1"),
                Vista(Doc(6.0, variableId: VarId), "D2"),
                ProjectVariableScanEntry.Selective("D3", RackB, Doc(6.0, rackId: RackB, variableId: VarId)),
            };

            var r = ProjectVariableMutationPreflight.ChangeValue(
                Registro((VarId, 6.0)), Objetivo, VariableDefinition.Literal(11.0), entries);

            Assert.True(r.IsSuccess);
            Assert.Equal(RegistryMutationKind.ChangeValue, r.Plan.RegistryMutation.Kind);
            Assert.Equal(2, r.Plan.RackMutations.Count);
            Assert.All(r.Plan.RackMutations, m => Assert.Equal(11.0, m.EffectiveOutput.VerticalClearance));
        }

        [Fact]
        public void ChangeValue_NO_TOCA_EL_LITERAL_AUTHORED_DE_NADIE()
        {
            var r = ProjectVariableMutationPreflight.ChangeValue(
                Registro((VarId, 6.0)), Objetivo, VariableDefinition.Literal(11.0),
                new[] { Vista(Doc(6.0, variableId: VarId), "D1") });

            Assert.Equal(6.0, Assert.Single(r.Plan.RackMutations).AuthoredOutput.VerticalClearance);
        }

        [Fact]
        public void ChangeValue_IGNORA_LOS_RACKS_AJENOS()
        {
            var entries = new[]
            {
                Vista(Doc(6.0, variableId: VarId), "D1"),
                ProjectVariableScanEntry.Selective("D2", RackB, Doc(6.0, rackId: RackB, variableId: Otra)),
            };

            var r = ProjectVariableMutationPreflight.ChangeValue(
                Registro((VarId, 6.0), (Otra, 3.0)), Objetivo, VariableDefinition.Literal(11.0), entries);

            Assert.Equal(RackA, Assert.Single(r.Plan.RackMutations).RackId);
        }

        [Fact]
        public void Prueba7_Prueba17_UN_CONSUMIDOR_IRRESOLUBLE_DEJA_EL_PLAN_VACIO_Y_NO_APLICA_A_LOS_DEMAS()
        {
            var entries = new[]
            {
                Vista(Doc(6.0, variableId: VarId), "D1"),
                ProjectVariableScanEntry.Selective("D2", RackB, Doc(6.0, rackId: RackB, variableId: VarId)),
                ProjectVariableScanEntry.SelectiveUnreadableDesign("D3", RackB),
            };

            var r = ProjectVariableMutationPreflight.ChangeValue(
                Registro((VarId, 6.0)), Objetivo, VariableDefinition.Literal(11.0), entries);

            // Ni el registro, ni el rack problematico, ni NINGUN otro consumidor.
            AssertPlanVacio(r);
        }

        [Fact]
        public void Prueba27_UN_PROPERTYID_DESCONOCIDO_EN_CUALQUIER_RACK_ABORTA_Y_DEJA_EL_PLAN_VACIO()
        {
            var raro = Doc(6.0, rackId: RackB);
            raro.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                ["selective.futuro"] = SelectivePropertyValueDocument.ToProjectVariable(Otra),
            };

            var entries = new[]
            {
                Vista(Doc(6.0, variableId: VarId), "D1"),
                ProjectVariableScanEntry.Selective("D2", RackB, raro),
            };

            AssertPlanVacio(ProjectVariableMutationPreflight.ChangeValue(
                Registro((VarId, 6.0)), Objetivo, VariableDefinition.Literal(11.0), entries));
        }

        [Fact]
        public void Prueba27_UN_KIND_DESCONOCIDO_EN_CUALQUIER_RACK_ABORTA()
        {
            var raro = Doc(6.0, rackId: RackB);
            raro.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [Token] = new SelectivePropertyValueDocument { Kind = "rackProperty", VariableId = Otra },
            };

            var entries = new[]
            {
                Vista(Doc(6.0, variableId: VarId), "D1"),
                ProjectVariableScanEntry.Selective("D2", RackB, raro),
            };

            AssertPlanVacio(ProjectVariableMutationPreflight.ChangeValue(
                Registro((VarId, 6.0)), Objetivo, VariableDefinition.Literal(11.0), entries));
        }

        [Fact]
        public void Prueba29_UN_SOBRE_ININTERPRETABLE_DEJA_EL_PLAN_VACIO()
        {
            var entries = new[]
            {
                Vista(Doc(6.0, variableId: VarId), "D1"),
                ProjectVariableScanEntry.UnreadableEnvelope("DEF-X"),
            };

            AssertPlanVacio(ProjectVariableMutationPreflight.ChangeValue(
                Registro((VarId, 6.0)), Objetivo, VariableDefinition.Literal(11.0), entries));
        }

        [Fact]
        public void ChangeValue_DeUnaVariableINEXISTENTE_FALLA()
            => AssertPlanVacio(ProjectVariableMutationPreflight.ChangeValue(
                Registro(), Objetivo, VariableDefinition.Literal(11.0), new ProjectVariableScanEntry[0]));

        // ================================================================ UnlinkAllAndDelete

        [Fact]
        public void UnlinkAllAndDelete_ES_UNA_SOLA_UNIDAD_LOGICA()
        {
            var entries = new[]
            {
                Vista(Doc(6.0, variableId: VarId), "D1"),
                ProjectVariableScanEntry.Selective("D2", RackB, Doc(6.0, rackId: RackB, variableId: VarId)),
            };

            var r = ProjectVariableMutationPreflight.UnlinkAllAndDelete(Registro((VarId, 11.0)), Objetivo, entries);

            Assert.True(r.IsSuccess);
            Assert.Equal(RegistryMutationKind.Remove, r.Plan.RegistryMutation.Kind);
            Assert.Equal(2, r.Plan.RackMutations.Count);

            // Cada consumidor se desvincula MATERIALIZANDO: nadie cambia de geometria.
            Assert.All(r.Plan.RackMutations, m =>
            {
                Assert.False(m.AuthoredOutput.HasBindingEntry(Piloto));
                Assert.Equal(11.0, m.AuthoredOutput.VerticalClearance);
                Assert.Equal(11.0, m.EffectiveOutput.VerticalClearance);
            });
        }

        [Fact]
        public void UnlinkAllAndDelete_ES_TODO_O_NADA()
        {
            var entries = new[]
            {
                Vista(Doc(6.0, variableId: VarId), "D1"),
                ProjectVariableScanEntry.SelectiveUnreadableDesign("D2", RackB),
            };

            AssertPlanVacio(ProjectVariableMutationPreflight.UnlinkAllAndDelete(Registro((VarId, 11.0)), Objetivo, entries));
        }

        [Fact]
        public void UnlinkAllAndDelete_SinConsumidores_SoloBorra()
        {
            var r = ProjectVariableMutationPreflight.UnlinkAllAndDelete(
                Registro((VarId, 11.0)), Objetivo, new[] { Vista(Doc(), "D1") });

            Assert.True(r.IsSuccess);
            Assert.Equal(RegistryMutationKind.Remove, r.Plan.RegistryMutation.Kind);
            Assert.Empty(r.Plan.RackMutations);
        }

        // ================================================================ pureza y aplicacion del registro

        [Fact]
        public void ElPlanNO_LLEVA_NINGUN_TIPO_DE_AUTOCAD()
        {
            var tipos = new[] { typeof(MutationPlan), typeof(RackMutation), typeof(RegistryMutation), typeof(VariableMutationPreflightResult) };

            foreach (var tipo in tipos)
            {
                foreach (var p in tipo.GetProperties())
                {
                    var nombre = p.PropertyType.FullName ?? string.Empty;
                    Assert.DoesNotContain("Autodesk", nombre);
                    Assert.DoesNotContain("ObjectId", nombre);
                    Assert.DoesNotContain("Transaction", nombre);
                    Assert.DoesNotContain("Database", nombre);
                }
            }
        }

        [Fact]
        public void LaMutacionDelRegistroSePuedeAPLICAR_DeFormaPura()
        {
            var registro = Registro();
            var creada = ProjectVariableMutationPreflight.Create(
                "Holgura", VariableType.Length, VariableDefinition.Literal(6.0));

            var despues = creada.Plan.RegistryMutation.ApplyTo(registro);

            var variable = Assert.Single(despues.ToProjectVariables());
            Assert.Equal("Holgura", variable.Name);
            Assert.Equal(6.0, variable.Definition.LiteralValue);

            // Y el registro de entrada NO se muta.
            Assert.Empty(registro.Variables);
        }

        [Fact]
        public void ApplyTo_DeUnRemove_QuitaLaVariable()
        {
            var registro = Registro((VarId, 6.0));

            Assert.Empty(RegistryMutation.Remove(Objetivo).ApplyTo(registro).Variables);
            Assert.Single(registro.Variables);
        }

        [Fact]
        public void ApplyTo_DeUnChangeValue_CambiaSoloElValor()
        {
            var despues = RegistryMutation
                .ChangeValue(Objetivo, VariableDefinition.Literal(11.0))
                .ApplyTo(Registro((VarId, 6.0)));

            var variable = Assert.Single(despues.ToProjectVariables());
            Assert.Equal(Objetivo, variable.Id);
            Assert.Equal(11.0, variable.Definition.LiteralValue);
        }

        [Fact]
        public void ApplyTo_DeNone_NoCambiaNada()
        {
            var registro = Registro((VarId, 6.0));

            Assert.Single(RegistryMutation.None.ApplyTo(registro).Variables);
        }
    }
}
