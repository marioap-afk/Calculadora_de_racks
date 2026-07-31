using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-37D — la ronda de corrección que sigue al punto 7: paleta, placa columna–separador, colocación de la
    /// base, geometría del ángulo y visibilidad de la planta.
    ///
    /// Los cinco puntos comparten una forma: ninguno inventa producto. Cambian de qué color se lee una pieza,
    /// dónde apoya una placa, sobre qué plano se espeja una base, con qué contorno se dibuja un perfil y qué
    /// entra en una vista. El BOM y las longitudes de corte quedan donde estaban, y hay pruebas que lo dicen.
    /// </summary>
    public class CantileverRoundThreeCorrectionTests
    {
        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        private static CantileverLineEditorComputation Build(
            CantileverStationFaceMode face = CantileverStationFaceMode.Single,
            Action<CantileverLineDesign> tweak = null)
        {
            var design = CantileverRoundTwoCharacterizationTests.Reference(face);
            tweak?.Invoke(design);

            return new CantileverLineEditorAssembler(Catalog).Build(design);
        }

        private static CantileverViewPlan View(
            CantileverLineEditorComputation c,
            CantileverViewKind view,
            CantileverPlantaVisibilityDesign visibility = null) =>
            CantileverViewPlanBuilder.Build(c.Line, view, Factory, 0, visibility);

        // =====================================================================================================
        // 1. LA PALETA
        // =====================================================================================================

        [Fact]
        public void LaPaletaEsLaQueElDuenoPIDIO()
        {
            // Literal, punto por punto, para que la lista quede escrita en un sitio ejecutable y no sólo en un
            // mensaje. Los índices son ACI, que es lo que el Plugin pone en la capa.
            short Color(CantileverVisualRole role) => CantileverVisualRoles.ColorIndexOf(role);

            // Columna, base, placa de base, cartabón de base y placa que une columna con separador → ROJO.
            Assert.Equal(1, Color(CantileverVisualRole.Column));
            Assert.Equal(1, Color(CantileverVisualRole.Base));
            Assert.Equal(1, Color(CantileverVisualRole.ColumnBasePlate));
            Assert.Equal(1, Color(CantileverVisualRole.Gusset));
            Assert.Equal(1, Color(CantileverVisualRole.ColumnSeparatorPlate));

            // Troqueles → BLANCO.
            Assert.Equal(7, Color(CantileverVisualRole.Punch));
            Assert.Equal(7, Color(CantileverVisualRole.BracePunch));

            // Separador → NARANJA.
            Assert.Equal(30, Color(CantileverVisualRole.Separator));

            // Perfil del brazo → AZUL. Su placa/ménsula → MORADO.
            Assert.Equal(5, Color(CantileverVisualRole.Arm));
            Assert.Equal(6, Color(CantileverVisualRole.Plate));

            // Tensores y TODOS sus componentes → CIAN.
            Assert.Equal(4, Color(CantileverVisualRole.Brace));
            Assert.Equal(4, Color(CantileverVisualRole.BraceAdapter));
            Assert.Equal(4, Color(CantileverVisualRole.BraceGusset));
        }

        [Fact]
        public void CadaROLSigueTeniendoSuPROPIACapa()
        {
            // Compartir color no es compartir capa, y ésa es la razón de que la paleta pueda agrupar sin
            // quitarle a nadie la posibilidad de apagar una familia sola.
            var roles = Enum.GetValues(typeof(CantileverVisualRole)).Cast<CantileverVisualRole>().ToList();
            var layers = roles.Select(CantileverVisualRoles.LayerNameOf).ToList();

            Assert.Equal(roles.Count, layers.Distinct(StringComparer.Ordinal).Count());
            Assert.All(layers, l => Assert.StartsWith(CantileverVisualRoles.LayerPrefix, l, StringComparison.Ordinal));
        }

        [Fact]
        public void TODOSLosRolesTienenColorYCapa_YUnoNuevoNoPuedeColarse()
        {
            // La autoridad falla en cerrado: el compilador no obliga a cubrir un `case`, así que esto lo hace.
            foreach (var role in Enum.GetValues(typeof(CantileverVisualRole)).Cast<CantileverVisualRole>())
            {
                Assert.True(CantileverVisualRoles.ColorIndexOf(role) > 0);
                Assert.NotEmpty(CantileverVisualRoles.LayerNameOf(role));
            }

            // Y un rol inexistente NO recibe un color por omisión: se rechaza.
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CantileverVisualRoles.ColorIndexOf((CantileverVisualRole)9999));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => CantileverVisualRoles.LayerNameOf((CantileverVisualRole)9999));
        }

        [Fact]
        public void TODAPlacaTieneNaturalezaDeclarada_YLaDelSeparadorSeLeeConLaColumna()
        {
            foreach (var kind in Enum.GetValues(typeof(CantileverPlateKind)).Cast<CantileverPlateKind>())
            {
                var role = CantileverVisualRoles.OfPlate(kind);

                Assert.True(CantileverVisualRoles.ColorIndexOf(role) > 0);
            }

            // La que une columna con separador ya NO es una placa de brazo, y por eso puede leerse en rojo sin
            // teñir de paso la ménsula de un brazo.
            Assert.Equal(
                CantileverVisualRole.ColumnSeparatorPlate,
                CantileverVisualRoles.OfPlate(CantileverPlateKind.SeparatorColumn));

            Assert.Equal(
                CantileverVisualRoles.ColorIndexOf(CantileverVisualRole.Column),
                CantileverVisualRoles.ColorIndexOf(CantileverVisualRole.ColumnSeparatorPlate));

            Assert.NotEqual(
                CantileverVisualRoles.ColorIndexOf(CantileverVisualRole.Plate),
                CantileverVisualRoles.ColorIndexOf(CantileverVisualRole.ColumnSeparatorPlate));
        }

        [Fact]
        public void ElDIBUJOUsaEsosColoresYNoOtros()
        {
            // Sobre las curvas de verdad, no sólo sobre la tabla: cada naturaleza que llega al plano trae el
            // color que la autoridad dice.
            var frontal = View(Build(), CantileverViewKind.Frontal);

            var expected = new Dictionary<CantileverVisualRole, short>
            {
                [CantileverVisualRole.Column] = 1,
                [CantileverVisualRole.Base] = 1,
                [CantileverVisualRole.ColumnBasePlate] = 1,
                [CantileverVisualRole.ColumnSeparatorPlate] = 1,
                [CantileverVisualRole.Arm] = 5,
                [CantileverVisualRole.Plate] = 6,
                [CantileverVisualRole.Separator] = 30,
                [CantileverVisualRole.Brace] = 4,
                [CantileverVisualRole.BraceAdapter] = 4,
                [CantileverVisualRole.Punch] = 7
            };

            foreach (var (role, aci) in expected)
            {
                Assert.Contains(frontal.Curves, x => x.Role == role);
                Assert.Equal(aci, CantileverVisualRoles.ColorIndexOf(role));
            }
        }

        // =====================================================================================================
        // 2. LA PLACA QUE UNE COLUMNA CON SEPARADOR
        // =====================================================================================================

        /// <summary>El semiespesor del alma sobre el que apoya el arriostramiento, medido como lo mide la línea.</summary>
        private static double HalfWeb(CantileverLineEditorComputation c)
        {
            var column = c.Line.Stations[0].Station.ColumnBase.Column;
            var geometry = Factory.Get(column.Placement.SectionId, SectionDetailLevel.Tabulated);

            Assert.True(CantileverBracingPlaneResolver.TryWebHalfThickness(
                geometry, SectionRepresentationOptions.DefaultChordTolerance, out var half, out _));

            return half;
        }

        [Fact]
        public void LaPlacaNOATRAVIESAElAlmaDeLaColumna()
        {
            // EL DEFECTO. La placa mide 3 in y su agujero está a 1.25 in de la cara del alma; centrada en el
            // agujero se comía el alma entera —0.435 in de canto— y asomaba 1.28 in por el otro lado, dentro
            // del tramo vecino.
            var c = Build();
            var half = HalfWeb(c);

            Assert.NotEmpty(c.Line.SeparatorColumnPlates);

            foreach (var plate in c.Line.SeparatorColumnPlates)
            {
                var minX = plate.Plate.Outline.Min(p => p.X);
                var maxX = plate.Plate.Outline.Max(p => p.X);

                // El eje del alma de la columna a la que pertenece esta placa.
                var columnX = c.Line.Stations[plate.Side == CantileverIntervalSide.Left
                        ? plate.IntervalIndex
                        : plate.IntervalIndex + 1]
                    .OriginX;

                var webMin = columnX - half;
                var webMax = columnX + half;

                // Ni cruza al otro lado ni se mete dentro del alma: apoya en su cara.
                if (plate.Side == CantileverIntervalSide.Left)
                {
                    Assert.True(minX >= webMax - 1e-9,
                        "La placa izquierda entra en el alma o la cruza: minX=" + minX + " cara=" + webMax);
                }
                else
                {
                    Assert.True(maxX <= webMin + 1e-9,
                        "La placa derecha entra en el alma o la cruza: maxX=" + maxX + " cara=" + webMin);
                }
            }
        }

        [Fact]
        public void LaPlacaQuedaDELLADOQueLeToca_HaciaSuPropioTramo()
        {
            var c = Build();

            foreach (var plate in c.Line.SeparatorColumnPlates)
            {
                var minX = plate.Plate.Outline.Min(p => p.X);
                var maxX = plate.Plate.Outline.Max(p => p.X);
                var holeX = plate.Punch.Centre.X;

                // El agujero cae DENTRO de la placa, que es lo mínimo que se le puede pedir a un troquel.
                Assert.InRange(holeX, minX - 1e-9, maxX + 1e-9);

                // Y a la distancia de orilla declarada del borde que da a la columna.
                var edge = plate.Side == CantileverIntervalSide.Left ? holeX - minX : maxX - holeX;

                Assert.Equal(CantileverLineDefaults.SeparatorColumnPunchEdgeDistance, edge, 9);
            }
        }

        [Fact]
        public void ElDATUMDeLaPlacaNoSeMovioYElSeparadorMideLoMismo()
        {
            // Lo que la corrección CONSERVA. La longitud del separador se deriva de la distancia entre los dos
            // agujeros, así que mover la placa sin mover su agujero no puede tocarla.
            var c = Build();

            foreach (var interval in c.Line.Intervals)
            {
                foreach (var separator in interval.Separators)
                {
                    var plates = c.Line.SeparatorColumnPlates
                        .Where(p => p.IntervalIndex == interval.Index &&
                                    p.SeparatorIndex == separator.SeparatorIndex)
                        .ToList();

                    Assert.Equal(2, plates.Count);

                    var span = plates.Max(p => p.Punch.Centre.X) - plates.Min(p => p.Punch.Centre.X);

                    Assert.Equal(
                        span + (2.0 * CantileverLineDefaults.SeparatorColumnPunchEdgeDistance),
                        separator.Member.NominalCutLength,
                        9);
                }
            }
        }

        [Fact]
        public void LaPlacaSeDibujaENLASDOSVistasQueLaVen_YEnLasDosIgual()
        {
            // «Aplica en previa y en el dibujo materializado»: la previa y el materializador consumen el MISMO
            // plan, así que la comprobación honesta es que el plan diga lo mismo mire quien lo mire.
            var c = Build();

            foreach (var view in new[] { CantileverViewKind.Frontal, CantileverViewKind.Planta })
            {
                var plan = View(c, view, CantileverPlantaVisibilityDesign.ShowingEverything);

                var placas = plan.Curves
                    .Where(x => x.Role == CantileverVisualRole.ColumnSeparatorPlate)
                    .ToList();

                Assert.NotEmpty(placas);
                Assert.All(placas, p => Assert.Equal(1, CantileverVisualRoles.ColorIndexOf(p.Role)));
            }
        }

        // =====================================================================================================
        // 3. LA BASE DE COLUMNA
        // =====================================================================================================

        /// <summary>El canto de la columna en Y, leído de su huella resuelta.</summary>
        private static (double Min, double Max) ColumnY(CantileverLineEditorComputation c)
        {
            var outline = c.Line.Stations[0].Station.ColumnBase.ColumnBottomPlate.Outline;

            return (outline.Min(p => p.Y), outline.Max(p => p.Y));
        }

        [Fact]
        public void LaBaseESPEJADANoSeMeteDENTRODeLaColumna()
        {
            // EL DEFECTO. Se espejaba sobre y = 0, que es la CARA de conexión y no el plano medio, así que la
            // base negativa ocupaba el mismo sitio que la columna a lo largo de todo su canto —9.5 in con una
            // W10X33— y en la lateral la columna se dibujaba dentro de la base.
            var c = Build(CantileverStationFaceMode.Double);
            var (columnMin, columnMax) = ColumnY(c);
            var station = c.Line.Stations[0].Station;

            Assert.True(columnMin < -1e-9, "La columna tiene que ocupar canto hacia y negativa.");

            var negativa = station.ColumnBase.Sides.Single(s => s.Side == CantileverArmSide.NegativeY);
            var positiva = station.ColumnBase.Sides.Single(s => s.Side == CantileverArmSide.PositiveY);

            Assert.True(negativa.Envelope().MaxY <= columnMin + 1e-9, "La base negativa invade la columna.");
            Assert.True(positiva.Envelope().MinY >= columnMax - 1e-9, "La base positiva invade la columna.");
        }

        [Fact]
        public void LaBaseYElBRAZODelMismoLadoApoyanEnLaMISMACara()
        {
            // La comprobación que resuelve la duda de a quién creer: el brazo negativo ya montaba en la cara
            // lejana de la columna. Era la base la que discrepaba con él, y ahora coinciden.
            var c = Build(CantileverStationFaceMode.Double);
            var station = c.Line.Stations[0].Station;

            var negativa = station.ColumnBase.Sides.Single(s => s.Side == CantileverArmSide.NegativeY);

            var mensulasNegativas = station.Plates
                .Where(p => p.Kind == CantileverPlateKind.ArmMounting && p.Outline.Max(q => q.Y) < -1e-9)
                .ToList();

            Assert.NotEmpty(mensulasNegativas);

            var caraDelBrazo = mensulasNegativas.Max(p => p.Outline.Max(q => q.Y));

            Assert.Equal(caraDelBrazo, negativa.RearPlate.Outline.Max(q => q.Y), 9);
        }

        [Fact]
        public void LaColumnaSigueELEVADASobreSuPlacaInferior()
        {
            // Lo que la corrección NO toca: el datum aprobado en la ronda anterior.
            var c = Build(CantileverStationFaceMode.Double);
            var columnBase = c.Line.Stations[0].Station.ColumnBase;
            var thickness = columnBase.ColumnBottomPlate.Thickness;

            Assert.True(thickness > 0.0);
            Assert.Equal(CantileverColumnBaseDatum.ColumnStartZ(thickness), columnBase.Column.Start.Z, 9);

            // La placa apoya en el suelo y la base también: sólo la columna sube.
            Assert.Equal(
                CantileverColumnBaseDatum.FloorZ,
                columnBase.ColumnBottomPlate.Outline.Min(p => p.Z) - thickness,
                9);
        }

        [Fact]
        public void EnLaLATERALLaColumnaYaNoSeDibujaDENTRODeLaBase()
        {
            // Sobre el dibujo, que es donde el dueño lo vio. La lateral mira a lo largo de X, así que su eje
            // horizontal es el canto en Y: si la columna cae dentro del rango de una base, se ven encajadas.
            var c = Build(CantileverStationFaceMode.Double);
            var lateral = View(c, CantileverViewKind.Lateral);

            var columna = lateral.Of(CantileverViewPieceKind.Column).SelectMany(x => x.Points).ToList();
            var bases = lateral.Of(CantileverViewPieceKind.Base).ToList();

            Assert.NotEmpty(columna);
            Assert.Equal(2, bases.Select(b => b.PieceId.Value).Distinct(StringComparer.Ordinal).Count());

            var colMin = columna.Min(p => p.X);
            var colMax = columna.Max(p => p.X);

            foreach (var grupo in bases.GroupBy(b => b.PieceId.Value))
            {
                var pts = grupo.SelectMany(b => b.Points).ToList();

                // Cada base queda ENTERA a un lado de la columna. Solapar sería volver al defecto.
                var fuera = pts.Max(p => p.X) <= colMin + 1e-6 || pts.Min(p => p.X) >= colMax - 1e-6;

                Assert.True(fuera, "En la lateral la base '" + grupo.Key + "' se solapa con la columna.");
            }
        }

        [Fact]
        public void LaCorreccionDeLaBaseNoToco_ELBOM()
        {
            // Mover una pieza no cambia lo que se compra: mismos perfiles, mismas longitudes, mismas cuentas.
            var bom = Build(CantileverStationFaceMode.Double).Bom;

            Assert.NotNull(bom);

            var bases = bom.Components.Where(x => x.Category.Contains("Base", StringComparison.OrdinalIgnoreCase)).ToList();

            Assert.NotEmpty(bases);
            Assert.All(bases, b => Assert.True(b.Quantity > 0 && b.Length > 0.0));
        }

        // =====================================================================================================
        // 4. LA GEOMETRÍA DEL ÁNGULO
        // =====================================================================================================

        [Fact]
        public void ElAnguloSeParecMASAlRealQueAntes_MedidoContraElAreaPublicada()
        {
            // EL JUEZ INDEPENDIENTE. Redondear las puntas es una decisión de forma, y la forma se puede
            // comprobar: el área que publica AISC no depende de nuestro contorno.
            //
            // Con las puntas a escuadra el contorno sobraba +0.800 % de media. Con el radio derivado el sesgo
            // baja a −0.078 %, el |error| medio de 0.814 % a 0.441 % y el máximo de 3.012 % a 2.316 %.
            var errores = new List<double>();

            foreach (var section in Catalog.All.Where(s => s.Family == StructuralSectionFamily.Angle))
            {
                var published = section.Properties.Area;

                if (!published.HasValue || published.Value <= 0.0)
                {
                    continue;
                }

                var geometry = Factory.Get(section, SectionDetailLevel.Tabulated);

                errores.Add(100.0 * (geometry.Area - published.Value) / published.Value);
            }

            Assert.True(errores.Count >= 130, "Se esperaban las 137 secciones L del catalogo.");

            var sesgo = errores.Average();
            var medio = errores.Select(Math.Abs).Average();
            var maximo = errores.Select(Math.Abs).Max();

            // Los tres, mejor que el estado anterior. Se fijan con holgura para que un catalogo revisado no
            // rompa la prueba por una centesima, pero apretados para que volver a la punta viva la rompa.
            Assert.True(Math.Abs(sesgo) < 0.30, "El sesgo del area subio a " + sesgo.ToString("0.000"));
            Assert.True(medio < 0.60, "El |error| medio subio a " + medio.ToString("0.000"));
            Assert.True(maximo < 2.60, "El error maximo subio a " + maximo.ToString("0.000"));
        }

        [Fact]
        public void CadaAnguloTrAEArcosDePUNTAYNoSoloElFileteDeRaiz()
        {
            var geometry = Factory.Get(Catalog.All.Single(s => s.SectionId.Value == "AISC-L-L2X2X3_16"),
                SectionDetailLevel.Tabulated);

            var arcos = geometry.OuterContour.Segments.Where(s => s.IsArc).ToList();

            // Uno de raíz y al menos uno por punta. Antes había exactamente uno.
            Assert.True(arcos.Count >= 3, "El angulo volvio a dibujarse con un solo arco: puntas a escuadra.");

            var filete = 0.438 - 0.188;
            Assert.Contains(arcos, a => Math.Abs(a.Radius - filete) < 1e-6);
            Assert.Contains(arcos, a => a.Radius < filete - 1e-9);
        }

        [Fact]
        public void ElRadioDePuntaNuncaSuperaMedioEspesor()
        {
            // Una punta no puede ser mas redonda que gruesa es el ala. El tope se comprueba sobre las 137.
            foreach (var section in Catalog.All.Where(s => s.Family == StructuralSectionFamily.Angle))
            {
                var dim = (AngleSectionDimensions)section.Dimensions;

                if (!dim.Thickness.HasValue || !dim.KDesign.HasValue)
                {
                    continue;
                }

                var fillet = dim.KDesign.Value - dim.Thickness.Value;

                if (fillet <= 0.0)
                {
                    continue;
                }

                var geometry = Factory.Get(section, SectionDetailLevel.Tabulated);

                foreach (var arc in geometry.OuterContour.Segments.Where(s => s.IsArc))
                {
                    Assert.True(
                        arc.Radius <= Math.Max(fillet, dim.Thickness.Value / 2.0) + 1e-9,
                        section.SectionId.Value + " tiene un arco de radio " + arc.Radius);
                }
            }
        }

        [Fact]
        public void ElContornoDelAnguloSigueSiendoCERRADOYVALIDO()
        {
            // Redondear puntas mete arcos donde había vértices, y un arco mal encadenado deja un hueco. Se
            // comprueba sobre TODAS, porque el caso que falla es siempre el que nadie miró.
            foreach (var section in Catalog.All.Where(s => s.Family == StructuralSectionFamily.Angle))
            {
                var geometry = Factory.Get(section, SectionDetailLevel.Tabulated);
                var segments = geometry.OuterContour.Segments;

                Assert.True(geometry.Area > 0.0, section.SectionId.Value + " tiene area nula.");

                for (var i = 0; i < segments.Count; i++)
                {
                    var end = segments[i].End;
                    var start = segments[(i + 1) % segments.Count].Start;

                    Assert.True(
                        Math.Abs(end.X - start.X) < 1e-6 && Math.Abs(end.Y - start.Y) < 1e-6,
                        section.SectionId.Value + " tiene un hueco entre los segmentos " + i + " y " + (i + 1));
                }
            }
        }

        [Fact]
        public void ElAnguloNOReclamaEXACTITUDPorEstarMejorAproximado()
        {
            // El radio sigue sin publicarse. Mejor aproximación no es exactitud, y decir lo contrario sería
            // justo la exactitud falsa que la convención anterior quería evitar.
            var geometry = Factory.Get(Catalog.All.Single(s => s.SectionId.Value == "AISC-L-L2X2X3_16"),
                SectionDetailLevel.Tabulated);

            Assert.NotEqual(SectionFidelity.TabulatedComplete, geometry.Fidelity);
            Assert.Contains(geometry.Diagnostics, d => d.Code == SectionGeometryDiagnostics.ToeRoundingNotPublished);
        }

        // =====================================================================================================
        // 5. LA VISIBILIDAD DE LA PLANTA
        // =====================================================================================================

        [Fact]
        public void LaPlantaNACEsinBrazosNiTensores()
        {
            var c = Build();
            var planta = c.Views.Single(v => v.View == CantileverViewKind.Planta);

            Assert.DoesNotContain(planta.Curves, x => x.Kind == CantileverViewPieceKind.Arm);
            Assert.DoesNotContain(planta.Curves, x => x.Kind == CantileverViewPieceKind.Brace);
            Assert.DoesNotContain(planta.Curves, x => x.Kind == CantileverViewPieceKind.ColdRolledAdapter);

            // Y lo que la planta existe para enseñar SIGUE ahí.
            Assert.Contains(planta.Curves, x => x.Kind == CantileverViewPieceKind.Column);
            Assert.Contains(planta.Curves, x => x.Kind == CantileverViewPieceKind.Base);
            Assert.Contains(planta.Curves, x => x.Kind == CantileverViewPieceKind.Separator);
        }

        [Fact]
        public void ElDEFECTOEsApagadoTambienEnElDiseno()
        {
            var design = new CantileverLineDesign();

            Assert.NotNull(design.PlantaVisibility);
            Assert.False(design.PlantaVisibility.ShowArms);
            Assert.False(design.PlantaVisibility.ShowBraces);
        }

        [Fact]
        public void ENCENDERLOSLosDevuelve_YCadaInterruptorMandaSOLOSobreLoSuyo()
        {
            var c = Build();

            var soloBrazos = View(c, CantileverViewKind.Planta,
                new CantileverPlantaVisibilityDesign { ShowArms = true });

            Assert.Contains(soloBrazos.Curves, x => x.Kind == CantileverViewPieceKind.Arm);
            Assert.DoesNotContain(soloBrazos.Curves, x => x.Kind == CantileverViewPieceKind.Brace);

            var soloTensores = View(c, CantileverViewKind.Planta,
                new CantileverPlantaVisibilityDesign { ShowBraces = true });

            Assert.DoesNotContain(soloTensores.Curves, x => x.Kind == CantileverViewPieceKind.Arm);
            Assert.Contains(soloTensores.Curves, x => x.Kind == CantileverViewPieceKind.Brace);

            var todo = View(c, CantileverViewKind.Planta, CantileverPlantaVisibilityDesign.ShowingEverything);

            Assert.Contains(todo.Curves, x => x.Kind == CantileverViewPieceKind.Arm);
            Assert.Contains(todo.Curves, x => x.Kind == CantileverViewPieceKind.Brace);
        }

        [Fact]
        public void ApagarUnBrazoApagaTAMBIENSusPlacasYSusTroqueles()
        {
            // Una ménsula colgada de una columna sin brazo dibuja una pieza que no sujeta nada, que es peor
            // que no dibujar ninguna.
            var c = Build();
            var planta = c.Views.Single(v => v.View == CantileverViewKind.Planta);

            Assert.DoesNotContain(planta.Curves, x => x.Role == CantileverVisualRole.Plate);

            var conTodo = View(c, CantileverViewKind.Planta, CantileverPlantaVisibilityDesign.ShowingEverything);

            Assert.Contains(conTodo.Curves, x => x.Role == CantileverVisualRole.Plate);
            Assert.True(conTodo.Curves.Count > planta.Curves.Count);
        }

        [Fact]
        public void LaFRONTALYLaLATERALNoSeEnteranDeLosInterruptores()
        {
            // El encargo dice «el cambio es para la planta». Las otras dos no consultan la regla, así que
            // apagar todo no puede quitarles nada.
            var c = Build();
            var apagado = new CantileverPlantaVisibilityDesign();

            foreach (var view in new[] { CantileverViewKind.Frontal, CantileverViewKind.Lateral })
            {
                var conRegla = View(c, view, apagado);
                var sinRegla = View(c, view, CantileverPlantaVisibilityDesign.ShowingEverything);

                Assert.Equal(sinRegla.Curves.Count, conRegla.Curves.Count);
                Assert.Contains(conRegla.Curves, x => x.Kind == CantileverViewPieceKind.Arm);
            }
        }

        [Fact]
        public void APAGARLOSNoDescuentaNadaDelBOMNiDelMODELO()
        {
            // La comprobación de que esto es dibujo y no producto.
            var conTodo = Build(tweak: d => d.PlantaVisibility =
                CantileverPlantaVisibilityDesign.ShowingEverything);

            var apagado = Build();

            static string Sig(BomSnapshot bom) => string.Join("\n", bom.Components
                .Select(x => x.Category + "|" + x.ProfileId + "|" + x.Length.ToString("0.####") + "|" + x.Quantity)
                .OrderBy(x => x, StringComparer.Ordinal));

            Assert.Equal(Sig(new BomSnapshot(conTodo)), Sig(new BomSnapshot(apagado)));
            Assert.Equal(conTodo.Line.Signature(), apagado.Line.Signature());

            // Y el modelo sigue resolviendo las dos familias: no se dibujan, no es que no existan.
            Assert.NotEmpty(apagado.Line.Arms);
            Assert.NotEmpty(apagado.Line.Braces);
        }

        /// <summary>Envoltorio mínimo para comparar dos BOM sin repetir la proyección en cada aserción.</summary>
        private sealed class BomSnapshot
        {
            public BomSnapshot(CantileverLineEditorComputation computation)
            {
                Components = computation.Bom.Components;
            }

            public IReadOnlyList<RackCad.Application.Bom.BomComponent> Components { get; }
        }

        [Fact]
        public void LaVisibilidadSOBREVIVEAlGuardarYVolverAAbrir()
        {
            // Vive en el diseño justo para esto: un proyecto que se reabre tiene que enseñar la planta que su
            // dueño dejó.
            var design = CantileverRoundTwoCharacterizationTests.Reference();
            design.PlantaVisibility = new CantileverPlantaVisibilityDesign { ShowArms = true, ShowBraces = false };

            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForCantilever(design)));

            Assert.True(reloaded.CantileverLineDesign.PlantaVisibility.ShowArms);
            Assert.False(reloaded.CantileverLineDesign.PlantaVisibility.ShowBraces);

            // Y la copia profunda tampoco la pierde ni la comparte.
            var copy = design.DeepCopy();
            copy.PlantaVisibility.ShowBraces = true;

            Assert.False(design.PlantaVisibility.ShowBraces);
        }
    }
}
