using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G4 — UN solo punto de resolucion, y por que tiene que haber exactamente uno.
    ///
    /// <para>
    /// Antes de este gate el BOM re-resolvia el diseño por su cuenta desde <c>ToDomain()</c>, que es el
    /// camino AUTHORED sin resolver. Con un rack vinculado eso significa que el dibujo mostraria la variable
    /// y la <b>cotizacion el literal congelado</b>: dos numeros para la misma propiedad, sin que nada falle.
    /// La convencion del repositorio ya lo dice —cuando dibujo, BOM y UI deben coincidir en un numero, la
    /// regla vive en UNA funcion de Application— y este resolver es esa funcion.
    /// </para>
    /// <para>
    /// La propiedad que lo hace funcionar es que NADIE MAS resuelve: geometria, BOM y preview reciben un
    /// <c>SelectivePalletDesign</c> ya efectivo y no saben si nacio de un literal o de una variable. El
    /// dominio no aprende que existe un <c>VariableId</c>.
    /// </para>
    /// <para>
    /// Y una referencia que no resuelve es un ERROR TIPADO, no un cero. Devolver el literal authored seria
    /// cambiar la geometria en silencio; devolver <c>default</c> seria peor. Reparar es una accion explicita
    /// del usuario, y no ocurre aqui.
    /// </para>
    /// </summary>
    public class SelectiveEffectiveDesignResolverTests
    {
        private const string RackId = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        private const string OtroVarId = "1c7f0b52-4a88-4d0e-9a33-77b2e6c40915";

        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectiveCell Celda(double alto, double? clearOverride = null) => new SelectiveCell
        {
            Pallet = new Tarima { Frente = 48, Alto = alto },
            PalletCount = 1,
            BeamId = BeamId,
            BeamPeralte = 4.5,
            ClearOverride = clearOverride,
        };

        private static SelectivePalletDesign Diseno(double clearance, params SelectiveCell[] celdas)
        {
            var design = new SelectivePalletDesign { PostId = PostId, VerticalClearance = clearance };
            var bay = new SelectiveBayDesign();
            foreach (var celda in celdas.Length == 0 ? new[] { Celda(50) } : celdas)
            {
                bay.Levels.Add(celda);
            }

            design.Bays.Add(bay);
            return design;
        }

        private static SelectivePalletDesignDocument Authored(double literal, params SelectiveCell[] celdas)
            => SelectivePalletDesignDocument.From(Diseno(literal, celdas), RackId, "Rack 1");

        private static SelectivePalletDesignDocument Vinculado(
            double literalCongelado,
            string propertyId = ProjectPropertyIds.SelectiveVerticalClearanceToken,
            string kind = SelectivePropertyValueDocument.ProjectVariableKind,
            string variableId = VarId,
            params SelectiveCell[] celdas)
        {
            var doc = Authored(literalCongelado, celdas);
            doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [propertyId] = new SelectivePropertyValueDocument { Kind = kind, VariableId = variableId },
            };
            return doc;
        }

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

        private static SelectiveEffectiveResolution Resolver(
            SelectivePalletDesignDocument authored,
            ProjectVariablesDocument registro)
            => new SelectiveEffectiveDesignResolver().Resolve(authored, registro);

        // ================================================================ prueba 2

        [Fact]
        public void Prueba2_SinBinding_GANA_ELLITERAL_AUTHORED()
        {
            var resultado = Resolver(Authored(6.0), Registro());

            Assert.True(resultado.IsSuccess);
            Assert.Equal(6.0, resultado.Design.VerticalClearance);
        }

        [Fact]
        public void Prueba2_SinBinding_ElRegistroVacioNoEsUnProblema()
        {
            Assert.True(Resolver(Authored(6.0), ProjectVariablesDocument.CreateNew()).IsSuccess);
        }

        [Fact]
        public void Prueba2_ConBindingVALIDO_GANA_LAVARIABLE()
        {
            var resultado = Resolver(Vinculado(6.0), Registro((VarId, 11.0)));

            Assert.True(resultado.IsSuccess);
            Assert.Equal(11.0, resultado.Design.VerticalClearance);
        }

        [Fact]
        public void Prueba2_ElLITERAL_AUTHORED_NO_CAMBIA_AlResolver()
        {
            var authored = Vinculado(6.0);

            var resultado = Resolver(authored, Registro((VarId, 11.0)));

            Assert.Equal(11.0, resultado.Design.VerticalClearance);
            Assert.Equal(6.0, authored.VerticalClearance);
        }

        [Fact]
        public void Prueba2_CambiarLaVariableCambiaElEfectivo_SinTocarNadaDelRack()
        {
            var authored = Vinculado(6.0);

            Assert.Equal(11.0, Resolver(authored, Registro((VarId, 11.0))).Design.VerticalClearance);
            Assert.Equal(4.0, Resolver(authored, Registro((VarId, 4.0))).Design.VerticalClearance);
            Assert.Equal(6.0, authored.VerticalClearance);
        }

        [Fact]
        public void ElResolverNoConoceMasQueLaPropiedadPiloto_ElRestoDelDisenoViajaIntacto()
        {
            var authored = Vinculado(6.0, celdas: new[] { Celda(50), Celda(40) });

            var efectivo = Resolver(authored, Registro((VarId, 11.0))).Design;

            Assert.Equal(PostId, efectivo.PostId);
            Assert.Equal(2, efectivo.Bays[0].Levels.Count);
            Assert.Equal(40, efectivo.Bays[0].Levels[1].Pallet.Alto);
        }

        // ================================================================ referencia rota (D-08)

        [Fact]
        public void UnVariableIdINEXISTENTE_ES_ERROR_TIPADO_Y_NO_HAY_DISENO()
        {
            var resultado = Resolver(Vinculado(6.0), Registro((OtroVarId, 11.0)));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(SelectiveEffectiveOutcome.BrokenProjectVariableReference, resultado.Outcome);
            Assert.Null(resultado.Design);
        }

        [Fact]
        public void UnVariableIdINEXISTENTE_NO_CAE_AL_LITERAL_NI_A_CERO()
        {
            var resultado = Resolver(Vinculado(6.0), Registro());

            Assert.Null(resultado.Design);
            Assert.NotEqual(SelectiveEffectiveOutcome.Success, resultado.Outcome);
        }

        [Fact]
        public void ElMensajeDeUnaReferenciaRotaNOMBRA_RACK_PROPIEDAD_Y_VARIABLE()
        {
            var resultado = Resolver(Vinculado(6.0), Registro());

            Assert.Contains(RackId, resultado.Error);
            Assert.Contains(ProjectPropertyIds.SelectiveVerticalClearanceToken, resultado.Error);
            Assert.Contains(VarId, resultado.Error);
        }

        // ================================================================ pruebas 8 y 9

        [Fact]
        public void Prueba8_UnPropertyIdDESCONOCIDO_ES_ERROR_VISIBLE_JAMAS_CAIDA_AL_LITERAL()
        {
            var resultado = Resolver(
                Vinculado(6.0, propertyId: "selective.palletDepth"),
                Registro((VarId, 11.0)));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(SelectiveEffectiveOutcome.UnknownPropertyId, resultado.Outcome);
            Assert.Null(resultado.Design);
            Assert.Contains("selective.palletDepth", resultado.Error);
        }

        [Fact]
        public void Prueba8_LaComparacionDelPropertyIdEs_ORDINAL()
        {
            var resultado = Resolver(
                Vinculado(6.0, propertyId: "selective.verticalclearance"),
                Registro((VarId, 11.0)));

            Assert.Equal(SelectiveEffectiveOutcome.UnknownPropertyId, resultado.Outcome);
        }

        [Fact]
        public void Prueba9_UnKindDeReferenciaDESCONOCIDO_ES_ERROR_VISIBLE()
        {
            var resultado = Resolver(Vinculado(6.0, kind: "rackProperty"), Registro((VarId, 11.0)));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(SelectiveEffectiveOutcome.UnknownReferenceKind, resultado.Outcome);
            Assert.Null(resultado.Design);
            Assert.Contains("rackProperty", resultado.Error);
        }

        [Fact]
        public void UnVariableIdMALFORMADO_ES_ERROR_VISIBLE_NoUnaReferenciaAusente()
        {
            var resultado = Resolver(Vinculado(6.0, variableId: "no-es-un-guid"), Registro((VarId, 11.0)));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(SelectiveEffectiveOutcome.MalformedReference, resultado.Outcome);
            Assert.Null(resultado.Design);
        }

        // ================================================================ prueba 14 — ClearOverride

        /// <summary>
        /// Se mide sobre EL MISMO diseño resuelto contra dos valores distintos de la variable, no comparando
        /// dos diseños entre si: en el Selectivo el nivel 0 del diseño es la TARIMA DE PISO y el override que
        /// gobierna el primer larguero vive en el nivel 1, asi que alinear indices entre diseños distintos
        /// mediria otra cosa.
        /// </summary>
        [Fact]
        public void Prueba14_ClearOverride_CONSERVA_SU_PRECEDENCIA_SOBRE_EL_EFECTIVO()
        {
            var catalogo = Catalog;

            double PrimerLarguero(bool conOverride, double valorDeLaVariable)
            {
                var celdas = conOverride
                    ? new[] { Celda(50), Celda(50, clearOverride: 20.0) }
                    : new[] { Celda(50), Celda(50) };

                var efectivo = Resolver(Vinculado(6.0, celdas: celdas), Registro((VarId, valorDeLaVariable))).Design;
                return new SelectiveGeometryResolver().Resolve(efectivo, catalogo).Bays[0].Levels[0].Y;
            }

            // CON override: cambiar la variable NO mueve el larguero. El override manda, como siempre.
            Assert.Equal(PrimerLarguero(conOverride: true, 10.0), PrimerLarguero(conOverride: true, 40.0), 6);

            // SIN override: la variable SI gobierna. Sin esto, la prueba anterior pasaria por vacia.
            Assert.NotEqual(PrimerLarguero(conOverride: false, 10.0), PrimerLarguero(conOverride: false, 40.0), 6);
        }

        [Fact]
        public void Prueba14_ElResolverNoTOCA_LosClearOverride_DeLasCeldas()
        {
            var efectivo = Resolver(
                Vinculado(6.0, celdas: new[] { Celda(50, clearOverride: 20.0) }),
                Registro((VarId, 40.0))).Design;

            Assert.Equal(20.0, efectivo.Bays[0].Levels[0].ClearOverride);
        }

        // ================================================================ aislamiento del dominio

        [Fact]
        public void SinBindingElEfectivoEsExactamenteLoQueToDomainYaDevolvia()
        {
            var authored = Authored(6.0);

            var efectivo = Resolver(authored, Registro()).Design;
            var deToDomain = authored.ToDomain();

            Assert.Equal(deToDomain.VerticalClearance, efectivo.VerticalClearance);
            Assert.Equal(deToDomain.PostId, efectivo.PostId);
            Assert.Equal(deToDomain.Bays[0].Levels.Count, efectivo.Bays[0].Levels.Count);
        }

        [Fact]
        public void ElDominioNoAprendeQueExistenLasVariablesDeProyecto()
        {
            var propiedades = typeof(SelectivePalletDesign)
                .GetProperties()
                .Select(p => p.Name)
                .ToList();

            Assert.DoesNotContain(propiedades, n => n.Contains("Variable"));
            Assert.DoesNotContain(propiedades, n => n.Contains("PropertyValues"));
            Assert.DoesNotContain(propiedades, n => n.Contains("SchemaVersion"));
        }

        [Fact]
        public void UnDocumentoNuloSeRechazaEnVezDeProducirUnDisenoVacio()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => new SelectiveEffectiveDesignResolver().Resolve(null, Registro()));
        }

        [Fact]
        public void UnRegistroNuloSeTrataComoRegISTRO_VACIO_NoComoFallo()
        {
            // Un DWG sin variables es legado valido: un rack no vinculado se resuelve con normalidad.
            Assert.True(new SelectiveEffectiveDesignResolver().Resolve(Authored(6.0), null).IsSuccess);
        }

        [Fact]
        public void UnRackVINCULADO_ConRegistroNULO_ES_REFERENCIA_ROTA_NoUnLiteral()
        {
            var resultado = new SelectiveEffectiveDesignResolver().Resolve(Vinculado(6.0), null);

            Assert.Equal(SelectiveEffectiveOutcome.BrokenProjectVariableReference, resultado.Outcome);
        }
    }
}
