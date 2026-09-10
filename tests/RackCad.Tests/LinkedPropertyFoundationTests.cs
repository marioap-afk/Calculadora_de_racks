using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-48 gate G4A — la FOUNDATION semantica de propiedades vinculables: catalogo, descriptor set,
    /// target snapshot, registro acreditado y la primitiva de inspeccion. Todo puro, sin AutoCAD, sin WPF
    /// y sin migrar todavia ninguna superficie.
    ///
    /// <para>
    /// Lo que estas pruebas fijan no es que los tipos existan. Son las cuatro fronteras que el resto de
    /// I-48 da por supuestas, y ninguna se lee en una firma:
    /// </para>
    /// <list type="number">
    /// <item><b>persistence-readable NO es semantic-usable.</b> El store puede aceptar un documento que la
    /// autoridad de identidad rechaza. El caso concreto es el <c>VariableId</c> duplicado: el store no lo
    /// valida —y no se le cambia para que lo haga—, asi que la unica forma de que no llegue a un lookup es
    /// que la acreditacion falle despues.</item>
    /// <item><b>El tipo incompatible tiene que ser CONSTRUIBLE.</b> <c>ProjectVariable.Create</c> lanza ante
    /// un tipo no soportado, asi que un target incompatible jamas se puede construir por ahi. Si la rama
    /// fatal no se puede alcanzar, la regla no se puede probar; por eso el snapshot es un tipo aparte y su
    /// construccion no consulta <c>IsSupported</c>.</item>
    /// <item><b>Una PropertyId desconocida se decide DENTRO de la primitiva.</b> Si recibiera un descriptor
    /// ya resuelto, el veredicto lo habria tomado quien hizo el lookup y la autoridad quedaria partida.</item>
    /// <item><b>El catalogo productivo tiene exactamente una propiedad.</b> No es un placeholder: la guarda
    /// que impide que una segunda propiedad se resuelva mal es el propio catalogo, asi que es lo ultimo
    /// que crece.</item>
    /// </list>
    /// </summary>
    public class LinkedPropertyFoundationTests
    {
        private const string ClearanceToken = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        // Un tipo que esta build NO soporta productivamente. Existe solo para probar la DESIGUALDAD; no
        // declara que RackCad admita un segundo VariableType.
        private const VariableType TipoSintetico = (VariableType)9001;

        private static VariableId Id(string guid) => VariableId.Parse(guid);

        private static readonly string GuidA = "11111111-1111-1111-1111-111111111111";
        private static readonly string GuidB = "22222222-2222-2222-2222-222222222222";

        private static VariableTargetSnapshot Target(string guid, VariableType type, double value = 6.0)
        {
            Assert.True(VariableTargetSnapshot.TryCreate(Id(guid), type, value, out var snapshot, out _));
            return snapshot;
        }

        private static LinkedPropertyDescriptorSet SetOf(params SelectiveLinkedPropertyDescriptor[] descriptors)
        {
            Assert.True(LinkedPropertyDescriptorSet.TryCreate(descriptors, out var set, out _));
            return set;
        }

        private static UsableProjectVariablesRegistry Registry(params VariableTargetSnapshot[] targets)
        {
            var accreditation = UsableProjectVariablesRegistry.FromTargets(targets);
            Assert.True(accreditation.IsUsable);
            return accreditation.Registry;
        }

        private static SelectivePropertyValueDocument Reference(string guid)
            => SelectivePropertyValueDocument.ToProjectVariable(guid);

        private static ProjectVariableDocument Entry(string guid, string type = "Length", double value = 6.0)
            => new ProjectVariableDocument
            {
                VariableId = guid,
                Name = "Holgura",
                Type = type,
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
            };

        private static ProjectVariablesDocument Document(params ProjectVariableDocument[] entries)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>(entries);
            return document;
        }

        // ================================================================ descriptor set

        [Fact]
        public void UnDescriptorSetConPropertyIdDuplicada_NoSeConstruye()
        {
            var duplicado = new[]
            {
                new SelectiveLinkedPropertyDescriptor(PropertyId.Parse(ClearanceToken), VariableType.Length),
                new SelectiveLinkedPropertyDescriptor(PropertyId.Parse(ClearanceToken), VariableType.Length),
            };

            Assert.False(LinkedPropertyDescriptorSet.TryCreate(duplicado, out var set, out var error));
            Assert.Null(set);
            Assert.Contains(ClearanceToken, error);
        }

        [Fact]
        public void LaIdentidadDeUnaPropiedadEsOrdinalYSensibleALaCaja()
        {
            // Dos cajas distintas NO son la misma propiedad, asi que conviven en el mismo set sin ser duplicado.
            var set = SetOf(
                new SelectiveLinkedPropertyDescriptor(PropertyId.Parse("selective.verticalClearance"), VariableType.Length),
                new SelectiveLinkedPropertyDescriptor(PropertyId.Parse("selective.verticalclearance"), VariableType.Length));

            Assert.Equal(2, set.Count);
            Assert.True(set.Contains(PropertyId.Parse("selective.verticalClearance")));
            Assert.True(set.Contains(PropertyId.Parse("selective.verticalclearance")));
        }

        // ================================================================ catalogo productivo

        [Fact]
        public void ElCatalogoProductivoDeclaraLaHolguraVertical()
        {
            Assert.True(SelectiveLinkedProperties.All.Contains(ProjectPropertyIds.SelectiveVerticalClearance));
            Assert.True(ProjectPropertyIds.IsKnown(ProjectPropertyIds.SelectiveVerticalClearance));
        }

        [Fact]
        public void ElCatalogoProductivoNoDeclaraTodaviaUnaSegundaPropiedadVinculable()
        {
            // G4A cierra con capacidad productiva de UNA sola propiedad. Registrar la segunda es G4E, y
            // hacerlo antes dejaria un estado donde se reconoce pero no se resuelve bien.
            Assert.Equal(1, SelectiveLinkedProperties.All.Count);
            Assert.False(SelectiveLinkedProperties.IsKnown(PropertyId.Parse("selective.palletTolerance")));
            Assert.False(ProjectPropertyIds.IsKnown(PropertyId.Parse("selective.palletTolerance")));
        }

        // ================================================================ registro productivo

        [Fact]
        public void UnRegistroAusenteSeAcreditaComoVacioUsable()
        {
            var accreditation = UsableProjectVariablesRegistry.Accredit(ProjectVariablesReadResult.Absent());

            Assert.True(accreditation.IsUsable);
            Assert.Equal(0, accreditation.Registry.Count);
        }

        [Fact]
        public void UnRegistroLegibleConIdsUnicosSeAcredita()
        {
            var read = ProjectVariablesReadResult.Readable(Document(Entry(GuidA), Entry(GuidB)));
            var accreditation = UsableProjectVariablesRegistry.Accredit(read);

            Assert.True(accreditation.IsUsable);
            Assert.Equal(2, accreditation.Registry.Count);
            Assert.True(accreditation.Registry.TryGetTarget(Id(GuidA), out _));
        }

        [Fact]
        public void UnRegistroLegibleConVariableIdDuplicado_ElStoreLoSIGUE_ACEPTANDO_PeroNoSeAcredita()
        {
            var document = Document(Entry(GuidA, value: 6.0), Entry(GuidA, value: 8.0));

            // El store NO valida unicidad y no se le cambia para que lo haga: sigue diciendo Readable.
            var read = ProjectVariablesReadResult.Readable(document);
            Assert.Equal(ProjectVariablesReadOutcome.Readable, read.Outcome);

            // La frontera semantica es la que falla, DESPUES del veredicto de persistencia.
            var accreditation = UsableProjectVariablesRegistry.Accredit(read);

            Assert.False(accreditation.IsUsable);
            Assert.Equal(ProjectVariablesAccreditationOutcome.AmbiguousIdentity, accreditation.Outcome);
            Assert.Null(accreditation.Registry);
            Assert.Contains(GuidA, accreditation.Error);
        }

        [Fact]
        public void UnRegistroPresentePeroIlegibleNoSeAcreditaYJamasEsVacio()
        {
            var accreditation = UsableProjectVariablesRegistry.Accredit(
                ProjectVariablesReadResult.Unreadable("registro corrupto"));

            Assert.False(accreditation.IsUsable);
            Assert.Equal(ProjectVariablesAccreditationOutcome.NotReadable, accreditation.Outcome);
            Assert.Null(accreditation.Registry);
        }

        [Fact]
        public void UnRegistroDeMajorIncompatibleNoSeAcredita()
        {
            var accreditation = UsableProjectVariablesRegistry.Accredit(
                ProjectVariablesReadResult.IncompatibleMajor("escrito por una version mas nueva"));

            Assert.False(accreditation.IsUsable);
            Assert.Equal(ProjectVariablesAccreditationOutcome.NotReadable, accreditation.Outcome);
            Assert.Null(accreditation.Registry);
        }

        [Fact]
        public void UnRegistroNoConsultadoNoSeAcredita()
        {
            var accreditation = UsableProjectVariablesRegistry.Accredit(null);

            Assert.False(accreditation.IsUsable);
            Assert.Equal(ProjectVariablesAccreditationOutcome.NotReadable, accreditation.Outcome);
            Assert.Null(accreditation.Registry);
        }

        // ================================================================ registro sintetico

        [Fact]
        public void UnFixtureConVariableIdDuplicadoNoSeConstruye()
        {
            var accreditation = UsableProjectVariablesRegistry.FromTargets(
                new[] { Target(GuidA, VariableType.Length), Target(GuidA, VariableType.Length, 8.0) });

            Assert.False(accreditation.IsUsable);
            Assert.Equal(ProjectVariablesAccreditationOutcome.AmbiguousIdentity, accreditation.Outcome);
        }

        [Fact]
        public void UnTargetConVariableIdVacioNoSeConstruye()
        {
            Assert.False(
                VariableTargetSnapshot.TryCreate(default, VariableType.Length, 6.0, out var snapshot, out var error));
            Assert.Null(snapshot);
            Assert.Contains("VariableId", error);
        }

        [Fact]
        public void UnTargetConValorNoFinitoNoSeConstruye()
        {
            Assert.False(
                VariableTargetSnapshot.TryCreate(Id(GuidA), VariableType.Length, double.NaN, out var conNaN, out _));
            Assert.Null(conNaN);

            Assert.False(
                VariableTargetSnapshot.TryCreate(
                    Id(GuidA), VariableType.Length, double.PositiveInfinity, out var conInfinito, out _));
            Assert.Null(conInfinito);
        }

        [Fact]
        public void UnTargetConTipoNoSoportadoSI_SeConstruye()
        {
            // Esta es la costura que hace PROBABLE la rama de incompatibilidad. Si el snapshot validara
            // IsSupported, FATAL_INCOMPATIBLE_TARGET seria inalcanzable sin anadir un segundo tipo productivo.
            Assert.False(VariableTypes.IsSupported(TipoSintetico));

            Assert.True(
                VariableTargetSnapshot.TryCreate(Id(GuidA), TipoSintetico, 6.0, out var snapshot, out _));
            Assert.Equal(TipoSintetico, snapshot.VariableType);
        }

        // ================================================================ InspectBinding

        [Fact]
        public void UnaPropertyIdDesconocidaEsFatalUnknownProperty()
        {
            var inspection = LinkedPropertyInspection.InspectBinding(
                "selective.loQueSea",
                Reference(GuidA),
                SelectiveLinkedProperties.All,
                Registry(Target(GuidA, VariableType.Length)));

            Assert.Equal(BindingInspectionOutcome.FatalUnknownProperty, inspection.Outcome);
            Assert.True(inspection.IsFatal);
        }

        [Fact]
        public void UnTokenDePropiedadIlegibleTambienEsFatalUnknownProperty()
        {
            // No se inventa una categoria distinta para un token malformado: no resuelve descriptor, que es
            // exactamente lo que UNKNOWN significa, y la disposicion es la misma.
            var inspection = LinkedPropertyInspection.InspectBinding(
                "   ",
                Reference(GuidA),
                SelectiveLinkedProperties.All,
                Registry(Target(GuidA, VariableType.Length)));

            Assert.Equal(BindingInspectionOutcome.FatalUnknownProperty, inspection.Outcome);
        }

        [Fact]
        public void UnaReferenciaDeClaseDesconocidaEsFatalMalformedReference()
        {
            var referencia = new SelectivePropertyValueDocument { Kind = "rackProperty", VariableId = GuidA };

            var inspection = LinkedPropertyInspection.InspectBinding(
                ClearanceToken, referencia, SelectiveLinkedProperties.All, Registry(Target(GuidA, VariableType.Length)));

            Assert.Equal(BindingInspectionOutcome.FatalMalformedReference, inspection.Outcome);
        }

        [Fact]
        public void UnVariableIdIlegibleEsFatalMalformedReference()
        {
            var referencia = new SelectivePropertyValueDocument
            {
                Kind = SelectivePropertyValueDocument.ProjectVariableKind,
                VariableId = "no-soy-un-guid",
            };

            var inspection = LinkedPropertyInspection.InspectBinding(
                ClearanceToken, referencia, SelectiveLinkedProperties.All, Registry(Target(GuidA, VariableType.Length)));

            Assert.Equal(BindingInspectionOutcome.FatalMalformedReference, inspection.Outcome);
            Assert.Equal("no-soy-un-guid", inspection.RawVariableId);
        }

        [Fact]
        public void UnTargetAusenteEsReparableYNoFatal()
        {
            var inspection = LinkedPropertyInspection.InspectBinding(
                ClearanceToken,
                Reference(GuidB),
                SelectiveLinkedProperties.All,
                Registry(Target(GuidA, VariableType.Length)));

            Assert.Equal(BindingInspectionOutcome.RepairableMissingTarget, inspection.Outcome);
            Assert.True(inspection.IsRepairable);
            Assert.False(inspection.IsFatal);
        }

        [Fact]
        public void UnTargetPresenteYCompatibleEsHealthyYTransportaSuSnapshot()
        {
            var target = Target(GuidA, VariableType.Length, 7.5);

            var inspection = LinkedPropertyInspection.InspectBinding(
                ClearanceToken, Reference(GuidA), SelectiveLinkedProperties.All, Registry(target));

            Assert.Equal(BindingInspectionOutcome.Healthy, inspection.Outcome);
            Assert.Same(target, inspection.Target);
            Assert.Equal(7.5, inspection.Target.LiteralValue);
        }

        // ================================================================ PROOF DE COMPOSICION

        [Fact]
        public void ElSnapshotIncompatibleVIAJA_DeVerdad_HastaInspectBinding()
        {
            // No basta con que el snapshot exista y con que la rama exista: hay que demostrar que hay CAMINO.
            // synthetic snapshot -> synthetic usable registry -> InspectBinding -> FATAL_INCOMPATIBLE_TARGET.
            var incompatible = Target(GuidA, TipoSintetico, 6.0);

            var accreditation = UsableProjectVariablesRegistry.FromTargets(new[] { incompatible });
            Assert.True(accreditation.IsUsable);

            var inspection = LinkedPropertyInspection.InspectBinding(
                ClearanceToken, Reference(GuidA), SelectiveLinkedProperties.All, accreditation.Registry);

            Assert.Equal(BindingInspectionOutcome.FatalIncompatibleTarget, inspection.Outcome);
            Assert.True(inspection.IsFatal);
            Assert.False(inspection.IsRepairable);

            // El target existia: la incompatibilidad NO se disfraza de "variable inexistente".
            Assert.Same(incompatible, inspection.Target);
            Assert.Contains(VariableType.Length.ToString(), inspection.Detail);
        }
    }
}
