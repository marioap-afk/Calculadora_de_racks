using System.Collections.Generic;
using System.Linq;
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
    /// I-48 gate G4F — el proof REAL con las DOS propiedades funcionando a la vez.
    ///
    /// <para>
    /// G4E demostro que la segunda propiedad entra por catalogo y atraviesa cada camino por separado. Lo que
    /// falta —y es lo unico que la primera propiedad no podia probar ni en principio— es lo que ocurre donde la
    /// CARDINALIDAD cambia: N propiedades por rack, N propiedades por <c>VariableId</c>, N rotas por rack.
    /// </para>
    /// <para>
    /// Cada seccion de aqui mata un defecto concreto y nombrable, y todos tienen la misma forma: «solo la
    /// primera». <c>ChangeValue</c> que actualiza solo la primera propiedad de X; <c>UnlinkAllAndDelete</c> que
    /// materializa solo la primera de P; <c>RepairBrokenRack</c> que retira solo la primera rota; un resolver
    /// que escribe el campo de una propiedad al resolver la otra. Con un catalogo de una propiedad, ninguno de
    /// esos defectos era observable.
    /// </para>
    /// <para>
    /// <b>Los oraculos leen los campos CONCRETOS</b> —<c>authored.VerticalClearance</c>,
    /// <c>design.PalletTolerance</c>— y nunca los accesores del descriptor, que es a quien se esta juzgando.
    /// </para>
    /// </summary>
    public class MultiPropertyRealProofTests
    {
        private const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string VarX = "11111111-1111-1111-1111-111111111111";
        private const string VarY = "22222222-2222-2222-2222-222222222222";
        private const string VarZ = "33333333-3333-3333-3333-333333333333";
        private const string Ausente = "99999999-8888-7777-6666-555555555555";

        private const string VcToken = ProjectPropertyIds.SelectiveVerticalClearanceToken;
        private const string PtToken = ProjectPropertyIds.SelectivePalletToleranceToken;

        private static PropertyId Vc => ProjectPropertyIds.SelectiveVerticalClearance;

        private static PropertyId Pt => ProjectPropertyIds.SelectivePalletTolerance;

        private static VariableId Id(string guid) => VariableId.Parse(guid);

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        // Ids REALES del catalogo. El downstream -geometria y BOM- solo se ejercita de verdad con piezas que
        // el catalogo conoce; con ids inventados el larguero no llega a cotizarse y la prueba pasaria sin
        // demostrar nada.
        private const string PostIdReal = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamIdReal = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        /// <summary>
        /// Un rack con DOS niveles y pieza real de catalogo. Los dos niveles importan: la holgura vertical
        /// gobierna la SEPARACION entre niveles, asi que con uno solo no habria separacion que observar.
        /// </summary>
        private static SelectivePalletDesign DisenoReal(double clearance = 6.0, double tolerance = 4.0)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostIdReal,
                PostPeralte = 3.0,
                VerticalClearance = clearance,
                PalletTolerance = tolerance,
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
                    BeamId = BeamIdReal,
                    BeamPeralte = 4.0,
                });
            }

            design.Bays.Add(bay);
            return design;
        }

        /// <summary>El mismo documento, pero sobre el rack real de catalogo.</summary>
        private static SelectivePalletDesignDocument AuthoredReal(
            double clearance = 6.0, double tolerance = 4.0, string vcTo = null, string ptTo = null)
        {
            var doc = SelectivePalletDesignDocument.From(DisenoReal(clearance, tolerance), RackA, "Rack A");
            var bindings = new Dictionary<string, SelectivePropertyValueDocument>();

            if (vcTo != null)
            {
                bindings[VcToken] = SelectivePropertyValueDocument.ToProjectVariable(vcTo);
            }

            if (ptTo != null)
            {
                bindings[PtToken] = SelectivePropertyValueDocument.ToProjectVariable(ptTo);
            }

            if (bindings.Count > 0)
            {
                doc.PropertyValues = bindings;
                doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return doc;
        }

        // ---------------------------------------------------------------- fixtures

        /// <summary>Un rack minimo: un frente de 48 con una tarima, para que el larguero sea calculable a mano.</summary>
        private static SelectivePalletDesign Diseno(double clearance = 6.0, double tolerance = 4.0)
        {
            var design = new SelectivePalletDesign
            {
                PostId = "POSTE_A",
                PostPeralte = 3.0,
                VerticalClearance = clearance,
                PalletTolerance = tolerance,
                PalletDepth = 48.0,
            };

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

        /// <summary>
        /// El documento guardado. <paramref name="vcTo"/> y <paramref name="ptTo"/> vinculan cada propiedad; un
        /// token extra permite fabricar una entrada FATAL sin tocar el catalogo.
        /// </summary>
        private static SelectivePalletDesignDocument Authored(
            double clearance = 6.0,
            double tolerance = 4.0,
            string vcTo = null,
            string ptTo = null,
            string extraToken = null,
            SelectivePropertyValueDocument extraValue = null,
            string rackId = RackA,
            string name = "Rack A")
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(clearance, tolerance), rackId, name);
            var bindings = new Dictionary<string, SelectivePropertyValueDocument>();

            if (vcTo != null)
            {
                bindings[VcToken] = SelectivePropertyValueDocument.ToProjectVariable(vcTo);
            }

            if (ptTo != null)
            {
                bindings[PtToken] = SelectivePropertyValueDocument.ToProjectVariable(ptTo);
            }

            if (extraToken != null)
            {
                bindings[extraToken] = extraValue;
            }

            if (bindings.Count > 0)
            {
                doc.PropertyValues = bindings;
                doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return doc;
        }

        private static ProjectVariableDocument Entry(string guid, string name, double value)
            => new ProjectVariableDocument
            {
                VariableId = guid,
                Name = name,
                Type = VariableType.Length.ToString(),
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
            };

        /// <summary>El registro: X = 10, Y = 9, Z = 15.</summary>
        private static ProjectVariablesDocument Registro(double x = 10.0, double y = 9.0, double z = 15.0)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                Entry(VarX, "General", x),
                Entry(VarY, "Estrecha", y),
                Entry(VarZ, "Ancha", z),
            };
            return document;
        }

        private static ProjectVariableScanEntry Vista(
            SelectivePalletDesignDocument doc, string def = "D1", string rackId = RackA)
            => ProjectVariableScanEntry.Selective(def, rackId, doc);

        private static SelectiveEffectiveResolution Resolve(
            SelectivePalletDesignDocument authored, ProjectVariablesDocument registry)
            => new SelectiveEffectiveDesignResolver().Resolve(authored, registry);

        // ================================================================ A. oracle exacto

        /// <summary>
        /// G4F (A). El catalogo productivo es EXACTAMENTE el declarado, y cada token gobierna el campo que este
        /// oracle escribe a mano. La expectativa no se construye preguntandole al descriptor.
        /// </summary>
        [Fact]
        public void EL_CATALOGO_PRODUCTIVO_ES_EXACTAMENTE_LAS_DOS_PROPIEDADES()
        {
            var declarados = new[] { VcToken, PtToken };

            var conocidos = new[]
            {
                VcToken, PtToken,
                "selective.palletDepth", "selective.floorBeamRise", "selective.postPeralte",
            }.Where(token => ProjectPropertyIds.IsKnown(PropertyId.Parse(token))).ToArray();

            Assert.Equal(
                declarados.OrderBy(t => t, System.StringComparer.Ordinal).ToArray(),
                conocidos.OrderBy(t => t, System.StringComparer.Ordinal).ToArray());
        }

        /// <summary>El cableado: cada token mueve SU campo authored y SU campo effective, y solo el suyo.</summary>
        [Theory]
        [InlineData(VcToken)]
        [InlineData(PtToken)]
        public void CADA_TOKEN_GOBIERNA_SU_CAMPO_Y_NO_EL_DEL_OTRO(string token)
        {
            var authored = Authored(
                clearance: 6.0,
                tolerance: 4.0,
                vcTo: token == VcToken ? VarX : null,
                ptTo: token == PtToken ? VarX : null);

            var design = Resolve(authored, Registro(x: 30.0)).Design;

            Assert.Equal(token == VcToken ? 30.0 : 6.0, design.VerticalClearance);
            Assert.Equal(token == PtToken ? 30.0 : 4.0, design.PalletTolerance);
        }

        // ================================================================ B. dos variables DISTINTAS

        /// <summary>
        /// G4F (B). Las dos propiedades vinculadas a la vez, a variables distintas. Es el escenario que la
        /// primera propiedad no podia expresar.
        /// </summary>
        [Fact]
        public void LAS_DOS_VINCULADAS_A_VARIABLES_DISTINTAS_RESUELVEN_CADA_UNA_LA_SUYA()
        {
            var authored = Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX, ptTo: VarY);

            var resolution = Resolve(authored, Registro(x: 10.0, y: 9.0));

            Assert.True(resolution.IsSuccess);
            Assert.Equal(10.0, resolution.Design.VerticalClearance);
            Assert.Equal(9.0, resolution.Design.PalletTolerance);

            // Y los literales congelados siguen siendo los del authored.
            Assert.Equal(6.0, authored.VerticalClearance);
            Assert.Equal(4.0, authored.PalletTolerance);

            // Cada vinculo en su clave.
            Assert.True(authored.TryGetBinding(Vc, out var boundVc));
            Assert.True(authored.TryGetBinding(Pt, out var boundPt));
            Assert.Equal(Id(VarX), boundVc);
            Assert.Equal(Id(VarY), boundPt);
        }

        [Fact]
        public void MOVER_UNA_VARIABLE_MUEVE_UN_SOLO_CAMPO_EFECTIVO()
        {
            var authored = Authored(vcTo: VarX, ptTo: VarY);

            var antes = Resolve(authored, Registro(x: 10.0, y: 9.0)).Design;
            var movidaX = Resolve(authored, Registro(x: 99.0, y: 9.0)).Design;
            var movidaY = Resolve(authored, Registro(x: 10.0, y: 77.0)).Design;

            // Mover X mueve SOLO la holgura.
            Assert.Equal(99.0, movidaX.VerticalClearance);
            Assert.Equal(antes.PalletTolerance, movidaX.PalletTolerance);

            // Mover Y mueve SOLO la tolerancia.
            Assert.Equal(77.0, movidaY.PalletTolerance);
            Assert.Equal(antes.VerticalClearance, movidaY.VerticalClearance);
        }

        // ================================================================ C. la MISMA variable gobierna las dos

        /// <summary>
        /// G4F (C). Una sola variable gobernando las DOS propiedades del mismo rack es valido, y es el caso
        /// donde <c>N→1</c> deja de ser teorico: el rack tiene que reconstruirse UNA vez, no una por propiedad.
        /// </summary>
        [Fact]
        public void UNA_MISMA_VARIABLE_PUEDE_GOBERNAR_LAS_DOS_PROPIEDADES()
        {
            var resolution = Resolve(Authored(vcTo: VarX, ptTo: VarX), Registro(x: 10.0));

            Assert.True(resolution.IsSuccess);
            Assert.Equal(10.0, resolution.Design.VerticalClearance);
            Assert.Equal(10.0, resolution.Design.PalletTolerance);
        }

        /// <summary>
        /// G4F (C). El conjunto P: TODAS las propiedades del rack que apuntan a X, en orden Ordinal. Se deriva
        /// del authored que la autoridad ya establecio, no de una vista cualquiera.
        /// </summary>
        [Fact]
        public void P_CONTIENE_LAS_DOS_PROPIEDADES_EN_ORDEN_ORDINAL()
        {
            var authored = Authored(vcTo: VarX, ptTo: VarX);

            var p = SelectiveLinkedPropertyKernel.PropertiesBoundTo(authored, Id(VarX));

            Assert.Equal(2, p.Count);

            // Ordinal: "selective.palletTolerance" < "selective.verticalClearance" ('p' < 'v').
            Assert.Equal(new[] { PtToken, VcToken }, p.Select(id => id.Value).ToArray());
        }

        [Fact]
        public void P_SE_DERIVA_DESPUES_DE_LA_AUTORIDAD_AUTHORED()
        {
            // Dos hermanas IDENTICAS: la autoridad es Single, y P sale de ese authored unico.
            var doc = Authored(vcTo: VarX, ptTo: VarX);
            var siblings = new[] { Vista(doc, "D1"), Vista(Authored(vcTo: VarX, ptTo: VarX), "D2") };

            var authority = SelectiveAuthoredAuthority.Resolve(RackA, siblings);

            Assert.Equal(AuthoredAuthorityOutcome.Single, authority.Outcome);
            Assert.Equal(2, SelectiveLinkedPropertyKernel.PropertiesBoundTo(authority.Authored, Id(VarX)).Count);
        }

        /// <summary>
        /// G4F (C). <c>ChangeValue</c> sobre la variable que gobierna las DOS: ambos efectivos se mueven, ambos
        /// congelados y ambos vinculos permanecen, y el rack produce UNA sola mutacion.
        ///
        /// <para>
        /// Este es el oraculo que mata «ChangeValue solo actualiza la primera propiedad de X»: si asi fuera,
        /// una de las dos seguiria en 10.
        /// </para>
        /// </summary>
        [Fact]
        public void CHANGEVALUE_DE_LA_VARIABLE_COMPARTIDA_MUEVE_LAS_DOS_EN_UNA_SOLA_MUTACION()
        {
            var authored = Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX, ptTo: VarX);

            var result = ProjectVariableMutationPreflight.ChangeValue(
                Registro(x: 10.0), Id(VarX), VariableDefinition.Literal(12.0), new[] { Vista(authored) });

            Assert.True(result.IsSuccess);

            // UNA mutacion para el rack, no una por propiedad.
            var rack = Assert.Single(result.Plan.RackMutations);

            // Los dos efectivos se mueven.
            Assert.Equal(12.0, rack.EffectiveOutput.VerticalClearance);
            Assert.Equal(12.0, rack.EffectiveOutput.PalletTolerance);

            // Los dos congelados y los dos vinculos permanecen.
            Assert.Equal(6.0, rack.AuthoredOutput.VerticalClearance);
            Assert.Equal(4.0, rack.AuthoredOutput.PalletTolerance);
            Assert.True(rack.AuthoredOutput.TryGetBinding(Vc, out var boundVc));
            Assert.True(rack.AuthoredOutput.TryGetBinding(Pt, out var boundPt));
            Assert.Equal(Id(VarX), boundVc);
            Assert.Equal(Id(VarX), boundPt);

            // Y el registro cambia una sola vez.
            Assert.Equal(RegistryMutationKind.ChangeValue, result.Plan.RegistryMutation.Kind);
        }

        // ================================================================ D. UnlinkAllAndDelete N→1

        /// <summary>
        /// G4F (D). Desvincular y borrar la variable que gobierna las DOS: cada propiedad materializa el
        /// efectivo que regia, los dos vinculos se retiran y el rack se reconstruye UNA vez.
        ///
        /// <para>
        /// Mata «UnlinkAllAndDelete materializa solo la primera de P»: con ese defecto, la tolerancia se
        /// quedaria en su 4 congelado mientras la holgura pasa a 10.
        /// </para>
        /// </summary>
        [Fact]
        public void UNLINKALLANDDELETE_MATERIALIZA_LAS_DOS_PROPIEDADES_EN_UNA_SOLA_MUTACION()
        {
            var authored = Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX, ptTo: VarX);

            var result = ProjectVariableMutationPreflight.UnlinkAllAndDelete(
                Registro(x: 10.0), Id(VarX), new[] { Vista(authored) });

            Assert.True(result.IsSuccess);

            var rack = Assert.Single(result.Plan.RackMutations);

            // Las DOS materializan el efectivo que regia (10), no su literal congelado.
            Assert.Equal(10.0, rack.AuthoredOutput.VerticalClearance);
            Assert.Equal(10.0, rack.AuthoredOutput.PalletTolerance);

            // Los DOS vinculos desaparecen.
            Assert.False(rack.AuthoredOutput.TryGetBinding(Vc, out _));
            Assert.False(rack.AuthoredOutput.TryGetBinding(Pt, out _));

            // El efectivo posterior es ese mismo numero por las dos vias.
            Assert.Equal(10.0, rack.EffectiveOutput.VerticalClearance);
            Assert.Equal(10.0, rack.EffectiveOutput.PalletTolerance);

            // Y la variable se borra del registro.
            Assert.Equal(RegistryMutationKind.Remove, result.Plan.RegistryMutation.Kind);
        }

        // ================================================================ E. healthy + broken

        /// <summary>
        /// G4F (E). Una sana y otra rota: el diagnostico nombra SOLO la rota, con SU literal, y la sana no
        /// aparece como reparacion.
        /// </summary>
        [Fact]
        public void UNA_SANA_Y_OTRA_ROTA_DIAGNOSTICA_SOLO_LA_ROTA_CON_SU_PROPIO_LITERAL()
        {
            var authored = Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX, ptTo: Ausente);

            var workspace = ProjectVariablesWorkspace.Build(
                ProjectVariablesReadResult.Readable(Registro()), new[] { Vista(authored) });

            var broken = Assert.Single(workspace.BrokenBindings);

            Assert.Equal(PtToken, broken.PropertyId);
            Assert.Equal(Ausente, broken.VariableId);
            Assert.Equal(4.0, broken.StoredLiteral);   // el de la TOLERANCIA, no el 6 de la holgura
            Assert.True(broken.RackCanRepair);

            // B es exactamente { PT }.
            var batch = broken.RepairBatch;
            Assert.Equal(1, batch.Count);
            Assert.Equal(PtToken, batch.Bindings[0].PropertyId);
        }

        [Fact]
        public void REPARAR_CON_UNA_SANA_RETIRA_LA_ROTA_Y_NO_TOCA_LA_SANA()
        {
            var authored = Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX, ptTo: Ausente);

            var result = ProjectVariableMutationPreflight.RepairBrokenRack(
                Registro(x: 10.0), RackA, new[] { Vista(authored) }, confirmed: true);

            Assert.True(result.IsSuccess);

            var rack = Assert.Single(result.Plan.RackMutations);

            // La rota se retira y su literal almacenado gobierna.
            Assert.False(rack.AuthoredOutput.TryGetBinding(Pt, out _));
            Assert.Equal(4.0, rack.AuthoredOutput.PalletTolerance);
            Assert.Equal(4.0, rack.EffectiveOutput.PalletTolerance);

            // La SANA no se toca: su vinculo sigue y su efectivo lo da la variable.
            Assert.True(rack.AuthoredOutput.TryGetBinding(Vc, out var bound));
            Assert.Equal(Id(VarX), bound);
            Assert.Equal(6.0, rack.AuthoredOutput.VerticalClearance);
            Assert.Equal(10.0, rack.EffectiveOutput.VerticalClearance);
        }

        // ================================================================ F. las DOS rotas

        /// <summary>
        /// G4F (F). Con las dos rotas, el diagnostico publica DOS filas del mismo rack que comparten el MISMO
        /// batch: el alcance de la reparacion es el rack y no la fila desde la que se inicia.
        /// </summary>
        [Fact]
        public void LAS_DOS_ROTAS_PUBLICAN_DOS_FILAS_QUE_COMPARTEN_UN_BATCH_COMPLETO()
        {
            var authored = Authored(clearance: 6.0, tolerance: 4.0, vcTo: Ausente, ptTo: VarZ);

            // Z tampoco existe en este registro.
            var sinZ = ProjectVariablesDocument.CreateNew();
            sinZ.Variables = new List<ProjectVariableDocument> { Entry(VarX, "General", 10.0) };

            var workspace = ProjectVariablesWorkspace.Build(
                ProjectVariablesReadResult.Readable(sinZ), new[] { Vista(authored) });

            Assert.Equal(2, workspace.BrokenBindings.Count);
            Assert.All(workspace.BrokenBindings, row => Assert.Equal(RackA, row.RackId));
            Assert.All(workspace.BrokenBindings, row => Assert.True(row.RackCanRepair));

            // Orden determinista Ordinal.
            Assert.Equal(
                new[] { PtToken, VcToken },
                workspace.BrokenBindings.Select(row => row.PropertyId).ToArray());

            // El MISMO batch, y contiene las DOS.
            var batch = workspace.BrokenBindings[0].RepairBatch;
            Assert.All(workspace.BrokenBindings, row => Assert.Same(batch, row.RepairBatch));
            Assert.Equal(2, batch.Count);
            Assert.Equal(new[] { PtToken, VcToken }, batch.Bindings.Select(b => b.PropertyId).ToArray());

            // Y cada entrada del batch lleva SU literal.
            Assert.Equal(4.0, batch.Bindings.Single(b => b.PropertyId == PtToken).StoredLiteral);
            Assert.Equal(6.0, batch.Bindings.Single(b => b.PropertyId == VcToken).StoredLiteral);
        }

        /// <summary>
        /// G4F (F). Reparar retira las DOS, conserva los DOS congelados y emite UNA mutacion. Mata
        /// «RepairBroken retira solo la primera missing».
        /// </summary>
        [Fact]
        public void REPARAR_CON_LAS_DOS_ROTAS_RETIRA_AMBAS_Y_CONSERVA_AMBOS_CONGELADOS()
        {
            var authored = Authored(clearance: 6.0, tolerance: 4.0, vcTo: Ausente, ptTo: VarZ);

            var sinZ = ProjectVariablesDocument.CreateNew();
            sinZ.Variables = new List<ProjectVariableDocument> { Entry(VarX, "General", 10.0) };

            var result = ProjectVariableMutationPreflight.RepairBrokenRack(
                sinZ, RackA, new[] { Vista(authored) }, confirmed: true);

            Assert.True(result.IsSuccess);

            var rack = Assert.Single(result.Plan.RackMutations);

            Assert.False(rack.AuthoredOutput.TryGetBinding(Vc, out _));
            Assert.False(rack.AuthoredOutput.TryGetBinding(Pt, out _));

            // Los literales almacenados se conservan y gobiernan.
            Assert.Equal(6.0, rack.AuthoredOutput.VerticalClearance);
            Assert.Equal(4.0, rack.AuthoredOutput.PalletTolerance);
            Assert.Equal(6.0, rack.EffectiveOutput.VerticalClearance);
            Assert.Equal(4.0, rack.EffectiveOutput.PalletTolerance);

            // Reparar no toca el registro.
            Assert.Equal(RegistryMutationKind.None, result.Plan.RegistryMutation.Kind);
        }

        // ================================================================ G. rota + FATAL

        /// <summary>
        /// G4F (G). Una rota reparable por si sola + un estado FATAL: el rack entero queda BLOQUEADO y no se
        /// repara nada. Una reparacion parcial no puede aplicarse porque el ejecutor necesita un efectivo
        /// COMPLETO.
        /// </summary>
        [Fact]
        public void UNA_ROTA_MAS_UN_FATAL_BLOQUEA_EL_RACK_ENTERO()
        {
            var authored = Authored(
                clearance: 6.0,
                tolerance: 4.0,
                vcTo: Ausente,
                extraToken: "selective.noExiste",
                extraValue: SelectivePropertyValueDocument.ToProjectVariable(VarX));

            var workspace = ProjectVariablesWorkspace.Build(
                ProjectVariablesReadResult.Readable(Registro()), new[] { Vista(authored) });

            // El diagnostico se ve...
            Assert.Equal(2, workspace.BrokenBindings.Count);

            // ...pero NADA es accionable, y no hay batch que ofrecer.
            Assert.All(workspace.BrokenBindings, row => Assert.False(row.RackCanRepair));
            Assert.All(workspace.BrokenBindings, row => Assert.Null(row.RepairBatch));
            Assert.All(workspace.BrokenBindings, row => Assert.NotNull(row.RackBlockingReason));

            // Y reparar deja el plan vacio.
            var result = ProjectVariableMutationPreflight.RepairBrokenRack(
                Registro(), RackA, new[] { Vista(authored) }, confirmed: true);

            Assert.False(result.IsSuccess);
            Assert.True(result.Plan.IsEmpty);
        }

        // ================================================================ T. aislamiento del diagnostico

        /// <summary>
        /// G4F (T). Una propiedad rota no contamina la CLASIFICACION de la otra: el escaneo dice HEALTHY de la
        /// sana y FATAL de la otra, aunque el veredicto del rack sea BLOCKED.
        ///
        /// <para>
        /// Es la separacion entre «que le pasa a esta propiedad» y «puede mutarse este documento». La primera
        /// es por propiedad; la segunda es del rack.
        /// </para>
        /// </summary>
        [Fact]
        public void EL_DIAGNOSTICO_ES_POR_PROPIEDAD_AUNQUE_LA_MUTABILIDAD_SEA_DEL_RACK()
        {
            var authored = Authored(
                clearance: 6.0,
                tolerance: 4.0,
                vcTo: VarX,
                ptTo: null,
                extraToken: PtToken,
                extraValue: new SelectivePropertyValueDocument { Kind = "rackProperty", VariableId = VarY });

            var read = ProjectVariablesReadResult.Readable(Registro());
            var accreditation = UsableProjectVariablesRegistry.Accredit(read);
            Assert.True(accreditation.IsUsable);

            var assessment = SelectiveLinkedPropertyKernel.Assess(
                authored, SelectiveLinkedProperties.All, accreditation.Registry);

            // El rack esta bloqueado...
            Assert.Equal(RackRepairability.Blocked, assessment.Outcome);

            // ...y aun asi cada propiedad tiene SU veredicto.
            var porToken = assessment.Inspections.ToDictionary(i => i.PropertyToken, i => i.Outcome);

            Assert.Equal(BindingInspectionOutcome.Healthy, porToken[VcToken]);
            Assert.Equal(BindingInspectionOutcome.FatalMalformedReference, porToken[PtToken]);
        }

        // ================================================================ H. autoridad multi-vista

        [Fact]
        public void DOS_HERMANAS_IDENTICAS_CON_LAS_DOS_VINCULADAS_SON_UNA_SOLA_AUTORIDAD()
        {
            var siblings = new[]
            {
                Vista(Authored(vcTo: VarX, ptTo: VarY), "D1"),
                Vista(Authored(vcTo: VarX, ptTo: VarY), "D2"),
            };

            var authority = SelectiveAuthoredAuthority.Resolve(RackA, siblings);

            Assert.Equal(AuthoredAuthorityOutcome.Single, authority.Outcome);

            // Y una operacion N→1 produce UNA mutacion con las dos vistas como destinos.
            var result = ProjectVariableMutationPreflight.ChangeValue(
                Registro(), Id(VarX), VariableDefinition.Literal(21.0), siblings);

            Assert.True(result.IsSuccess);

            var rack = Assert.Single(result.Plan.RackMutations);
            Assert.Equal(2, rack.Destinations.Count);
            Assert.Equal(21.0, rack.EffectiveOutput.VerticalClearance);
        }

        /// <summary>
        /// G4F (H). Hermanas que solo difieren en UNA de las dos propiedades vinculadas: sigue siendo
        /// divergencia, y ninguna operacion elige una hermana.
        /// </summary>
        [Fact]
        public void HERMANAS_QUE_DIFIEREN_SOLO_EN_UNA_PROPIEDAD_SON_DIVERGENTES()
        {
            var siblings = new[]
            {
                Vista(Authored(vcTo: VarX, ptTo: VarY), "D1"),
                Vista(Authored(vcTo: VarX, ptTo: VarZ), "D2"),   // PT apunta a otra
            };

            Assert.Equal(
                AuthoredAuthorityOutcome.Divergent,
                SelectiveAuthoredAuthority.Resolve(RackA, siblings).Outcome);
        }

        [Fact]
        public void UNA_OPERACION_SOBRE_UN_RACK_DIVERGENTE_ABORTA_SIN_ELEGIR_HERMANA()
        {
            var siblings = new[]
            {
                Vista(Authored(vcTo: VarX, ptTo: VarY), "D1"),
                Vista(Authored(vcTo: VarX, ptTo: VarZ), "D2"),
            };

            // target-variable: X es consumida por las dos vistas, asi que se exige autoridad y no la hay.
            var cambio = ProjectVariableMutationPreflight.ChangeValue(
                Registro(), Id(VarX), VariableDefinition.Literal(21.0), siblings);

            Assert.False(cambio.IsSuccess);
            Assert.True(cambio.Plan.IsEmpty);

            // target-rack: reparar tampoco elige hermana.
            var reparar = ProjectVariableMutationPreflight.RepairBrokenRack(
                Registro(), RackA, siblings, confirmed: true);

            Assert.False(reparar.IsSuccess);
            Assert.True(reparar.Plan.IsEmpty);

            // Y borrar-desvinculando tampoco.
            var desvincular = ProjectVariableMutationPreflight.UnlinkAllAndDelete(
                Registro(), Id(VarX), siblings);

            Assert.False(desvincular.IsSuccess);
            Assert.True(desvincular.Plan.IsEmpty);
        }

        // ================================================================ I. persistencia real de DOS vinculos

        /// <summary>
        /// G4F (I). El viaje REAL por el limite de persistencia con los dos vinculos, incluido un campo que esta
        /// version no entiende: nada se pierde y el deserializado resuelve los dos efectivos.
        /// </summary>
        [Fact]
        public void UN_ROUND_TRIP_REAL_CONSERVA_LOS_DOS_VINCULOS_Y_LOS_DOS_CONGELADOS()
        {
            var original = Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX, ptTo: VarY);
            original.ExtensionData = new Dictionary<string, System.Text.Json.JsonElement>
            {
                ["FuturoDesconocido"] = System.Text.Json.JsonDocument.Parse("\"algo\"").RootElement,
            };

            var store = new SelectivePalletDesignStore();
            var releido = store.Deserialize(store.Serialize(original));

            // Los dos literales congelados.
            Assert.Equal(6.0, releido.VerticalClearance);
            Assert.Equal(4.0, releido.PalletTolerance);

            // Los dos vinculos, con sus identidades exactas.
            Assert.True(releido.TryGetBinding(Vc, out var boundVc));
            Assert.True(releido.TryGetBinding(Pt, out var boundPt));
            Assert.Equal(Id(VarX), boundVc);
            Assert.Equal(Id(VarY), boundPt);

            // El schema promovido y lo que esta version no entiende.
            Assert.Equal(SelectivePalletDesignDocument.PromotedSchemaVersion, releido.SchemaVersion);
            Assert.True(releido.ExtensionData.ContainsKey("FuturoDesconocido"));

            // Y resolver el DESERIALIZADO da los dos efectivos.
            var design = Resolve(releido, Registro(x: 10.0, y: 9.0)).Design;

            Assert.Equal(10.0, design.VerticalClearance);
            Assert.Equal(9.0, design.PalletTolerance);
        }

        // ================================================================ J. compatibilidad hacia atras

        /// <summary>
        /// G4F (J). Un documento sin <c>PropertyValues</c> abre, resuelve y se guarda sin ganar vinculos.
        /// Registrar la segunda propiedad no migra nada.
        /// </summary>
        [Fact]
        public void UN_DOCUMENTO_SIN_VINCULOS_ABRE_RESUELVE_Y_SE_GUARDA_SIN_GANARLOS()
        {
            var legacy = Authored(clearance: 6.0, tolerance: 4.0);

            Assert.Null(legacy.PropertyValues);

            // abre
            var open = SelectiveEditorOpen.Resolve(legacy, ProjectVariablesReadResult.Readable(Registro()));
            Assert.True(open.IsOpen);
            Assert.Equal(6.0, open.Design.VerticalClearance);
            Assert.Equal(4.0, open.Design.PalletTolerance);

            // guarda sin tocar: el reconciler recibe los estados tal cual salieron de abrir
            var reconciled = LinkedPropertyReconciler.Reconcile(
                legacy, open.Design, open.LinkedPropertyStates,
                ProjectVariablesReadResult.Readable(Registro()), RackA, "Rack A");

            Assert.True(reconciled.IsSuccess);

            // NINGUNA de las dos gana vinculo.
            Assert.False(reconciled.Authored.TryGetBinding(Vc, out _));
            Assert.False(reconciled.Authored.TryGetBinding(Pt, out _));
            Assert.True(reconciled.Authored.PropertyValues == null || reconciled.Authored.PropertyValues.Count == 0);

            // Y los valores no se mueven.
            Assert.Equal(6.0, reconciled.Authored.VerticalClearance);
            Assert.Equal(4.0, reconciled.Authored.PalletTolerance);

            // El original tampoco se muto.
            Assert.Null(legacy.PropertyValues);
        }

        // ================================================================ K. EditorOpen con las dos

        /// <summary>
        /// G4F (K). Abrir con las dos vinculadas entrega DOS estados comprometidos —cada uno con SU literal
        /// authored y SU referencia— y un diseno con los dos efectivos.
        /// </summary>
        [Fact]
        public void ABRIR_CON_LAS_DOS_VINCULADAS_ENTREGA_DOS_ESTADOS_COMPLETOS()
        {
            var open = SelectiveEditorOpen.Resolve(
                Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX, ptTo: VarY),
                ProjectVariablesReadResult.Readable(Registro(x: 10.0, y: 9.0)));

            Assert.True(open.IsOpen);
            Assert.Equal(2, open.LinkedPropertyStates.Count);

            var vc = open.LinkedPropertyStates[Vc];
            var pt = open.LinkedPropertyStates[Pt];

            Assert.True(vc.IsReference);
            Assert.Equal(Id(VarX), vc.Source.VariableId);
            Assert.Equal(6.0, vc.CommittedLiteral);

            Assert.True(pt.IsReference);
            Assert.Equal(Id(VarY), pt.Source.VariableId);
            Assert.Equal(4.0, pt.CommittedLiteral);

            // Y el diseno lleva los EFECTIVOS.
            Assert.Equal(10.0, open.Design.VerticalClearance);
            Assert.Equal(9.0, open.Design.PalletTolerance);
        }

        // ================================================================ M. transiciones cruzadas

        /// <summary>
        /// G4F (M). En UNA sesion, la holgura pasa de referencia a literal y la tolerancia de literal a
        /// referencia. El reconciler aplica un ESTADO FINAL, asi que las dos transiciones opuestas conviven sin
        /// que una pise a la otra y sin persistir ningun intermedio.
        /// </summary>
        [Fact]
        public void DOS_TRANSICIONES_OPUESTAS_EN_LA_MISMA_SESION_NO_SE_PISAN()
        {
            var inicial = Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX);

            var finales = new Dictionary<PropertyId, LinkedPropertyEditState>
            {
                [Vc] = LinkedPropertyEditState.Literal(7.0),              // Reference(X) -> Literal(7)
                [Pt] = LinkedPropertyEditState.Reference(4.0, Id(VarY)),  // Literal(4)    -> Reference(Y)
            };

            var result = LinkedPropertyReconciler.Reconcile(
                inicial,
                Diseno(clearance: 99.0, tolerance: 99.0),   // el diseno editado no manda para estas dos
                finales,
                ProjectVariablesReadResult.Readable(Registro(y: 9.0)),
                RackA,
                "Rack A");

            Assert.True(result.IsSuccess);

            // VC: sin vinculo, literal 7, efectivo 7.
            Assert.False(result.Authored.TryGetBinding(Vc, out _));
            Assert.Equal(7.0, result.Authored.VerticalClearance);
            Assert.Equal(7.0, result.Effective.VerticalClearance);

            // PT: vinculada a Y, congelado 4, efectivo 9.
            Assert.True(result.Authored.TryGetBinding(Pt, out var bound));
            Assert.Equal(Id(VarY), bound);
            Assert.Equal(4.0, result.Authored.PalletTolerance);
            Assert.Equal(9.0, result.Effective.PalletTolerance);
        }

        // ================================================================ O. homonimos

        /// <summary>
        /// G4F (O). Dos variables con el MISMO nombre gobernando propiedades distintas. Nada resuelve por
        /// nombre, asi que el round-trip conserva cual es cual.
        /// </summary>
        [Fact]
        public void DOS_HOMONIMAS_GOBERNANDO_PROPIEDADES_DISTINTAS_NO_SE_CONFUNDEN()
        {
            var homonimas = ProjectVariablesDocument.CreateNew();
            homonimas.Variables = new List<ProjectVariableDocument>
            {
                Entry(VarX, "General", 10.0),
                Entry(VarY, "General", 12.0),   // mismo nombre, valor distinto
            };

            var authored = Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX, ptTo: VarY);

            var design = Resolve(authored, homonimas).Design;

            // Cada una toma el valor de SU identidad, no el del primer homonimo.
            Assert.Equal(10.0, design.VerticalClearance);
            Assert.Equal(12.0, design.PalletTolerance);

            // Y tras el round-trip real siguen apuntando a lo mismo.
            var store = new SelectivePalletDesignStore();
            var releido = store.Deserialize(store.Serialize(authored));

            Assert.True(releido.TryGetBinding(Vc, out var boundVc));
            Assert.True(releido.TryGetBinding(Pt, out var boundPt));
            Assert.Equal(Id(VarX), boundVc);
            Assert.Equal(Id(VarY), boundPt);
        }

        // ================================================================ Q. geometria downstream

        /// <summary>
        /// G4F (Q). El efectivo de la tolerancia llega a la GEOMETRIA. El largo del larguero es
        /// <c>frente*tarimas + tolerancia*(tarimas+1)</c>: con frente 48 y una tarima, 4 da 56 y 9 da 66 —
        /// numeros escritos a mano, sin preguntarle nada al descriptor.
        /// </summary>
        [Fact]
        public void EL_EFECTIVO_DE_LA_TOLERANCIA_CAMBIA_EL_LARGO_DEL_LARGUERO()
        {
            var geometry = new SelectiveGeometryResolver();

            var literal = geometry.Resolve(Resolve(Authored(tolerance: 4.0), Registro()).Design, null);
            var vinculada = geometry.Resolve(
                Resolve(Authored(tolerance: 4.0, ptTo: VarY), Registro(y: 9.0)).Design, null);

            Assert.Equal(56.0, literal.Bays[0].BeamLength);
            Assert.Equal(66.0, vinculada.Bays[0].BeamLength);
        }

        /// <summary>
        /// G4F (Q). Las DOS propiedades llegan a la geometria a la vez y cada una a LO SUYO: la tolerancia al
        /// largo del larguero, la holgura a la altura del rack por la separacion entre niveles.
        ///
        /// <para>
        /// Se mide sobre el rack real de catalogo y con dos niveles, porque la holgura gobierna la SEPARACION:
        /// con un solo nivel no hay separacion que observar y la prueba pasaria sin decir nada.
        /// </para>
        /// </summary>
        [Fact]
        public void LAS_DOS_PROPIEDADES_ALCANZAN_LA_GEOMETRIA_SIN_MEZCLARSE()
        {
            var catalog = Catalog;
            var geometry = new SelectiveGeometryResolver();

            SelectiveRackSystem Geo(double clearance, double tolerance)
                => geometry.Resolve(Resolve(AuthoredReal(clearance, tolerance), null).Design, catalog);

            var baseline = Geo(6.0, 4.0);

            // Solo la tolerancia -> cambia el larguero, NO la altura.
            var soloPt = Geo(6.0, 9.0);

            Assert.NotEqual(baseline.Bays[0].BeamLength, soloPt.Bays[0].BeamLength);
            Assert.Equal(baseline.Height, soloPt.Height);

            // Solo la holgura -> cambia la altura, NO el larguero.
            var soloVc = Geo(30.0, 4.0);

            Assert.Equal(baseline.Bays[0].BeamLength, soloVc.Bays[0].BeamLength);
            Assert.NotEqual(baseline.Height, soloVc.Height);
        }

        // ================================================================ R. BOM downstream

        /// <summary>
        /// G4F (R). La cadena EXACTA que ejecuta el handler: authored + registro -> efectivo -> geometria ->
        /// BOM. La tolerancia gobernada por una variable cambia el largo cotizado del larguero.
        /// </summary>
        [Fact]
        public void LA_TOLERANCIA_VINCULADA_COTIZA_CON_SU_EFECTIVO()
        {
            var catalog = Catalog;

            BillOfMaterials Bom(SelectivePalletDesignDocument authored, ProjectVariablesDocument registry)
            {
                var resolution = Resolve(authored, registry);
                Assert.True(resolution.IsSuccess);
                return SelectiveBomBuilder.Build(
                    new SelectiveGeometryResolver().Resolve(resolution.Design, catalog), catalog);
            }

            var literal = Bom(AuthoredReal(tolerance: 4.0), null);
            var vinculada = Bom(AuthoredReal(tolerance: 4.0, ptTo: VarY), Registro(y: 9.0));

            // El BOM del vinculado es el de un rack cuya tolerancia vale 9, no 4.
            var comoSiFuera9 = Bom(AuthoredReal(tolerance: 9.0), null);

            Assert.Equal(Firma(comoSiFuera9), Firma(vinculada));
            Assert.NotEqual(Firma(literal), Firma(vinculada));
        }

        private static string Firma(BillOfMaterials bom)
            => string.Join(
                "|",
                bom.Lines.Select(line =>
                    line.Category + ":" + line.ProfileId + ":" + line.Length.ToString("0.###") + "x" + line.Quantity));

        // ================================================================ S. payload de vista nueva

        /// <summary>
        /// G4F (S). El authored reconciliado con los DOS vinculos se serializa UNA vez y las tres vistas —
        /// frontal, lateral y planta — portan ese mismo documento. Si alguna se reconstruyese desde el efectivo,
        /// perderia los dos vinculos y congelaria los valores de las variables.
        /// </summary>
        [Fact]
        public void LAS_TRES_VISTAS_NUEVAS_PORTAN_EL_MISMO_AUTHORED_CON_LOS_DOS_VINCULOS()
        {
            var finales = new Dictionary<PropertyId, LinkedPropertyEditState>
            {
                [Vc] = LinkedPropertyEditState.Reference(6.0, Id(VarX)),
                [Pt] = LinkedPropertyEditState.Reference(4.0, Id(VarY)),
            };

            var reconciled = LinkedPropertyReconciler.Reconcile(
                Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX, ptTo: VarY),
                Diseno(clearance: 10.0, tolerance: 9.0),   // el diseno lleva los EFECTIVOS
                finales,
                ProjectVariablesReadResult.Readable(Registro(x: 10.0, y: 9.0)),
                RackA,
                "Rack A");

            Assert.True(reconciled.IsSuccess);

            var designJson = new SelectivePalletDesignStore().Serialize(reconciled.Authored);

            var frontal = RackEmbedComposer.Compose(
                null, RackEmbedDocument.KindSelective, RackA, "Rack A", RackEmbedDocument.ViewFrontal, 0, designJson);
            var lateral = RackEmbedComposer.Compose(
                null, RackEmbedDocument.KindSelective, RackA, "Rack A", RackEmbedDocument.ViewLateral, 2, designJson);
            var planta = RackEmbedComposer.Compose(
                null, RackEmbedDocument.KindSelective, RackA, "Rack A", RackEmbedDocument.ViewPlanta, -1, designJson);

            // El MISMO documento en las tres; solo el sobre cambia.
            Assert.Equal(designJson, frontal.Design);
            Assert.Equal(frontal.Design, lateral.Design);
            Assert.Equal(frontal.Design, planta.Design);

            // Y ese documento lleva los DOS vinculos y los DOS congelados.
            var releido = new SelectivePalletDesignStore().Deserialize(planta.Design);

            Assert.Equal(6.0, releido.VerticalClearance);
            Assert.Equal(4.0, releido.PalletTolerance);
            Assert.True(releido.TryGetBinding(Vc, out var boundVc));
            Assert.True(releido.TryGetBinding(Pt, out var boundPt));
            Assert.Equal(Id(VarX), boundVc);
            Assert.Equal(Id(VarY), boundPt);
        }

        /// <summary>
        /// G4F (S). El contraste explicativo: reconstruir desde el efectivo pierde LOS DOS vinculos y congela
        /// los valores de las variables, y por eso la autoridad heredada declara divergentes a las hermanas.
        /// </summary>
        [Fact]
        public void RECONSTRUIR_DESDE_EL_EFECTIVO_PERDERIA_LOS_DOS_VINCULOS()
        {
            var authored = Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX, ptTo: VarY);
            var effective = Resolve(authored, Registro(x: 10.0, y: 9.0)).Design;

            var desdeElEfectivo = SelectivePalletDesignDocument.From(effective, RackA, "Rack A");

            Assert.False(desdeElEfectivo.TryGetBinding(Vc, out _));
            Assert.False(desdeElEfectivo.TryGetBinding(Pt, out _));
            Assert.Equal(10.0, desdeElEfectivo.VerticalClearance);
            Assert.Equal(9.0, desdeElEfectivo.PalletTolerance);

            Assert.False(SelectiveAuthoredAuthority.IsSameAuthority(new[] { authored, desdeElEfectivo }));
        }

        // ================================================================ P. commit re-read con plan N→1

        /// <summary>
        /// G4F (P). La garantia de V8 no se esquiva porque el plan sea N→1: si el registro gana un
        /// <c>VariableId</c> duplicado entre el plan y el commit, la preparacion bloquea ANTES de aplicar y no
        /// hay documento que escribir.
        ///
        /// <para>
        /// No reabre G4B. Lo que anade es la cardinalidad real: el plan que se estaba por escribir gobernaba DOS
        /// propiedades del rack, no una.
        /// </para>
        /// </summary>
        [Fact]
        public void UN_PLAN_N_A_1_TAMPOCO_ESQUIVA_LA_ACREDITACION_DEL_COMMIT()
        {
            // El plan: X gobierna las dos propiedades y se le cambia el valor.
            var plan = ProjectVariableMutationPreflight.ChangeValue(
                Registro(x: 10.0),
                Id(VarX),
                VariableDefinition.Literal(12.0),
                new[] { Vista(Authored(vcTo: VarX, ptTo: VarX)) });

            Assert.True(plan.IsSuccess);
            Assert.Single(plan.Plan.RackMutations);

            // Entre el plan y el commit, el registro gana un id duplicado.
            var ambiguo = ProjectVariablesDocument.CreateNew();
            ambiguo.Variables = new List<ProjectVariableDocument>
            {
                Entry(VarX, "General", 10.0),
                Entry(VarX, "Otra copia", 20.0),
            };

            var commit = RegistryCommit.Prepare(
                plan.Plan.RegistryMutation, ProjectVariablesReadResult.Readable(ambiguo));

            Assert.True(commit.IsBlocked);
            Assert.Null(commit.Changed);   // no hay producto: ApplyTo no se alcanzo
            Assert.Contains(VarX, commit.Error);
        }
    }
}
