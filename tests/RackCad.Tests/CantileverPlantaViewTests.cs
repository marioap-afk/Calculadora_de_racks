using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-37D, corrección de columna y base — motivo 2 del rechazo: «la geometría de la vista en planta es
    /// incorrecta».
    ///
    /// Lo era, y de forma medible: la base de cada estación salía como <b>una línea de 6.49 in de ancho y
    /// longitud CERO</b>, cuando vista desde arriba es una huella de 6.49 × 48 in. La causa no estaba en la
    /// planta sino una capa más abajo: se le preguntaba a la cámara si miraba a lo largo del eje Z <i>del
    /// mundo</i>, y lo que decide si una sección conserva su forma es el eje del <b>miembro</b>. Una cámara
    /// cenital conserva la forma de una columna de pie y no la de una base tumbada.
    ///
    /// Estas pruebas fijan la planta contra el MODELO —cada primitiva dentro de la huella proyectada de su
    /// pieza— y no contra números escritos a mano.
    /// </summary>
    public class CantileverPlantaViewTests
    {
        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        /// <summary>
        /// Lo que una proyección puede desplazar sin que sea un defecto.
        ///
        /// Un contorno teselado se aparta de su envolvente tabulada por la cuerda del teselado, así que la
        /// contención se comprueba con holgura. Es una milésima de pulgada: suficiente para el teselado y
        /// mucho menos que cualquier cota que un fabricante lea.
        /// </summary>
        private const double Tolerance = 1e-3;

        private static CantileverLineEditorComputation Build(
            CantileverStationFaceMode face = CantileverStationFaceMode.Single) =>
            new CantileverLineEditorAssembler(Catalog)
                .Build(CantileverRoundTwoCharacterizationTests.Reference(face));

        private static CantileverViewPlan Planta(CantileverLineEditorComputation computation) =>
            CantileverViewPlanBuilder.Build(
                computation.Line, CantileverViewKind.Planta, Factory, 0,
                CantileverPlantaVisibilityDesign.ShowingEverything);

        /// <summary>The picture-plane box of everything drawn for one piece id.</summary>
        private static Bounds2D BoxOf(CantileverViewPlan plan, string pieceId)
        {
            var points = plan.Curves
                .Where(c => c.PieceId.Value == pieceId)
                .SelectMany(c => c.Points)
                .ToList();

            Assert.True(points.Count > 0, "La planta no dibujo nada para " + pieceId + ".");
            return Bounds2D.FromPoints(points);
        }

        private static string ScopedId(
            CantileverLineEditorComputation computation, CantileverPieceId id, int stationIndex = 0) =>
            computation.Line.Stations[stationIndex].ScopedId(id).Value;

        // ---- 1. La huella de cada miembro es la que su colocación dice --------------------------------

        [Fact]
        public void LaBaseTieneSuHUELLAYNoUnaLineaSinLongitud()
        {
            // El defecto, medido. 6.49 es el ancho de patín del W12X26; 48 es la longitud de la base.
            var c = Build();
            var box = BoxOf(Planta(c), ScopedId(c, c.Line.Stations[0].Station.ColumnBase.Sides[0].Member.Id));
            var bases = c.Line.Stations[0].Station.ColumnBase.Sides[0].Member;

            Assert.Equal(bases.GeometricLength, box.Height, 3);
            Assert.True(box.Width > 1.0, "La base sigue saliendo como una linea sin ancho.");
        }

        [Fact]
        public void CadaMiembroTUMBADOMideSuLongitudEnLaPlanta()
        {
            // La afirmación general de la que la base es un caso: en una cenital, todo lo que no está de pie
            // se ve en verdadera magnitud a lo largo de su eje.
            var c = Build();
            var planta = Planta(c);
            var placement = c.Line.Stations[0];

            var tumbados = placement.Station.Members
                .Where(m => Math.Abs(m.Direction.Z) < 0.5)
                .ToList();

            Assert.NotEmpty(tumbados);

            foreach (var member in tumbados)
            {
                var box = BoxOf(planta, placement.ScopedId(member.Id).Value);

                Assert.True(
                    Math.Max(box.Width, box.Height) >= member.GeometricLength - Tolerance,
                    "El miembro " + member.Id.Value + " mide " + Math.Max(box.Width, box.Height) +
                    " en la planta y su longitud es " + member.GeometricLength + ".");
            }
        }

        [Fact]
        public void LaColumnaSeVeComoSuSECCIONYNoComoSuAltura()
        {
            // El caso contrario, y la razón de que la regla tenga que mirar al miembro: la columna SÍ está de
            // pie, así que su huella es su sección y no debe crecer con su altura.
            var c = Build();
            var planta = Planta(c);
            var columnBase = c.Line.Stations[0].Station.ColumnBase;
            var box = BoxOf(planta, ScopedId(c, columnBase.Column.Id));

            Assert.True(box.Height < columnBase.ColumnHeight / 2.0, "La columna se dibuja tumbada en la planta.");

            var bounds = Factory.Get(columnBase.Column.SectionId, SectionDetailLevel.Tabulated).Bounds;

            Assert.Equal(bounds.Height, box.Height, 2);
            Assert.Equal(bounds.Width, box.Width, 2);
        }

        [Fact]
        public void CambiarLaALTURADeLaColumnaNoCambiaLaPlanta()
        {
            // La consecuencia comprobable de lo anterior, escrita como propiedad y no como número.
            var baja = CantileverRoundTwoCharacterizationTests.Reference();
            var alta = CantileverRoundTwoCharacterizationTests.Reference();

            alta.StationTopology.ColumnHeight.Mode = CantileverStationColumnHeightMode.Manual;
            alta.StationTopology.ColumnHeight.ManualHeight = 144.0;

            var assembler = new CantileverLineEditorAssembler(Catalog);
            var planta = CantileverViewPlanBuilder.Build(
                assembler.Build(baja).Line, CantileverViewKind.Planta, Factory);
            var plantaAlta = CantileverViewPlanBuilder.Build(
                assembler.Build(alta).Line, CantileverViewKind.Planta, Factory);

            var id = ScopedId(assembler.Build(baja), assembler.Build(baja).Line.Stations[0].Station.ColumnBase.Column.Id);

            Assert.Equal(BoxOf(planta, id).Height, BoxOf(plantaAlta, id).Height, 6);
            Assert.Equal(BoxOf(planta, id).Width, BoxOf(plantaAlta, id).Width, 6);
        }

        // ---- 2. Las piezas planas tienen espesor ------------------------------------------------------

        [Fact]
        public void CadaPLACAMuestraSuEspesorEnLaPlanta()
        {
            // Una placa vista de canto no es un pelo: su silueta tiene el ancho de su espesor. Es la mitad de
            // planta del motivo 3.
            var c = Build();
            var planta = Planta(c);
            var placement = c.Line.Stations[0];

            var verticales = placement.Station.Plates
                .Where(p => Math.Abs(p.Normal.Z) < 0.5)
                .ToList();

            Assert.NotEmpty(verticales);

            foreach (var plate in verticales)
            {
                var box = BoxOf(planta, placement.ScopedId(plate.Id).Value);

                Assert.Equal(plate.Thickness, Math.Min(box.Width, box.Height), 6);
            }
        }

        [Fact]
        public void ElCARTABONMuestraSuEspesorEnLaPlanta()
        {
            var c = Build();
            var planta = Planta(c);
            var placement = c.Line.Stations[0];
            var gusset = placement.Station.Gussets[0];
            var box = BoxOf(planta, placement.ScopedId(gusset.Id).Value);

            Assert.Equal(gusset.Thickness, Math.Min(box.Width, box.Height), 6);
        }

        // ---- 3. Los troqueles, por su eje de perforación ------------------------------------------------

        [Fact]
        public void UnTroquelPerforadoHACIAABAJOSeVeRedondoEnLaPlanta()
        {
            // La placa inferior se taladra en +Z, así que en una cenital se mira por el agujero. Y los de
            // conexión se taladran en +Y, así que ahí se ven de canto. El criterio es el EJE, no la pieza.
            var c = Build();
            var planta = Planta(c);
            var placement = c.Line.Stations[0];

            var verticales = placement.Station.Punches.Where(p => Math.Abs(p.Direction.Z) > 0.9).ToList();
            var horizontales = placement.Station.Punches.Where(p => Math.Abs(p.Direction.Z) < 0.1).ToList();

            Assert.NotEmpty(verticales);
            Assert.NotEmpty(horizontales);

            foreach (var punch in verticales)
            {
                var curve = planta.Curves.Single(x => x.PieceId.Value == placement.ScopedId(punch.Id).Value);

                Assert.True(curve.IsCircle, "El troquel " + punch.Id.Value + " no se dibuja redondo en la planta.");
                Assert.Equal(punch.Diameter, curve.CircleDiameter.Value, 9);
            }

            foreach (var punch in horizontales)
            {
                var curve = planta.Curves.Single(x => x.PieceId.Value == placement.ScopedId(punch.Id).Value);

                Assert.False(curve.IsCircle, "El troquel " + punch.Id.Value + " se dibuja redondo visto de canto.");
            }
        }

        // ---- 4. La validación automática que pide el encargo --------------------------------------------

        [Fact]
        public void TodaPrimitivaCaeDENTRODeLaHuellaDeSuPiezaAnfitriona()
        {
            // «Cada primitiva física dentro de la envolvente proyectada de su pieza anfitriona, ± tolerancia».
            // Escrita contra el modelo 3D: se proyecta la envolvente que el resolutor calculó y se comprueba
            // que lo dibujado cabe dentro. Caza un troquel colocado fuera de su placa, una placa desplazada de
            // su miembro y una huella tomada de la pieza equivocada.
            foreach (var face in new[] { CantileverStationFaceMode.Single, CantileverStationFaceMode.Double })
            {
                var c = Build(face);
                var planta = Planta(c);
                var viewpoint = CantileverViewPlanBuilder.Viewpoint(CantileverViewKind.Planta);
                var placement = c.Line.Stations[0];
                var hosts = HostBoxes(placement, viewpoint);

                var comprobadas = 0;

                foreach (var curve in planta.Curves)
                {
                    if (!hosts.TryGetValue(curve.PieceId.Value, out var host))
                    {
                        continue; // de otra estación o de un intervalo: esta prueba mira una estación
                    }

                    foreach (var point in curve.Points)
                    {
                        // Un círculo se dibuja por su centro, así que su radio también tiene que caber.
                        var margin = curve.IsCircle ? curve.CircleDiameter.Value / 2.0 : 0.0;

                        Assert.True(
                            point.X - margin >= host.MinX - Tolerance &&
                            point.X + margin <= host.MaxX + Tolerance &&
                            point.Y - margin >= host.MinY - Tolerance &&
                            point.Y + margin <= host.MaxY + Tolerance,
                            "La primitiva de " + curve.PieceId.Value + " se sale de la huella de su pieza.");
                    }

                    comprobadas++;
                }

                Assert.True(comprobadas > 20, "La validacion solo alcanzo " + comprobadas + " curvas.");
            }
        }

        /// <summary>
        /// The projected footprint of every piece of a station that OWNS primitives.
        ///
        /// Built from the 3D envelope the resolver computed — never from what the view drew — because a
        /// containment check against the drawing's own extent would pass by construction and prove nothing.
        /// </summary>
        private static Dictionary<string, Bounds2D> HostBoxes(
            CantileverLineStationPlacement placement, SectionViewpoint viewpoint)
        {
            var boxes = new Dictionary<string, Bounds2D>();
            var columnBase = placement.Station.ColumnBase;

            foreach (var plate in placement.Station.Plates)
            {
                boxes[placement.ScopedId(plate.Id).Value] = Project(plate.Envelope(), placement.Offset, viewpoint);
            }

            foreach (var gusset in placement.Station.Gussets)
            {
                boxes[placement.ScopedId(gusset.Id).Value] = Project(gusset.Envelope(), placement.Offset, viewpoint);
            }

            // Every hole belongs to the flat piece or the member it pierces. The regular grid and the
            // connection rows pierce the COLUMN, whose footprint is its section — which is exactly the
            // containment a hole drilled off the flange would break.
            foreach (var punch in columnBase.ColumnConnectionPunches.Concat(columnBase.ColumnRegularPunches))
            {
                boxes[placement.ScopedId(punch.Id).Value] = ColumnBox(placement, viewpoint);
            }

            foreach (var punch in columnBase.ColumnBottomPlatePunches)
            {
                boxes[placement.ScopedId(punch.Id).Value] =
                    Project(columnBase.ColumnBottomPlate.Envelope(), placement.Offset, viewpoint);
            }

            foreach (var side in columnBase.Sides)
            {
                foreach (var punch in side.RearPlatePunches)
                {
                    boxes[placement.ScopedId(punch.Id).Value] =
                        Project(side.RearPlate.Envelope(), placement.Offset, viewpoint);
                }
            }

            return boxes;
        }

        private static Bounds2D ColumnBox(CantileverLineStationPlacement placement, SectionViewpoint viewpoint)
        {
            var column = placement.Station.ColumnBase.Column;
            var bounds = Factory.Get(column.SectionId, SectionDetailLevel.Tabulated).Bounds;

            var half = new Vector3D(bounds.Width / 2.0, bounds.Height / 2.0, 0.0);
            var centre = column.Start + placement.Offset;

            return Bounds2D.FromPoints(new[]
            {
                viewpoint.Project(new Point3D(centre.X - half.X, centre.Y - half.Y, centre.Z)),
                viewpoint.Project(new Point3D(centre.X + half.X, centre.Y + half.Y, centre.Z))
            });
        }

        private static Bounds2D Project(
            CantileverEnvelope3D envelope, Vector3D offset, SectionViewpoint viewpoint)
        {
            var corners = new List<Point2D>(8);

            foreach (var x in new[] { envelope.MinX, envelope.MaxX })
            {
                foreach (var y in new[] { envelope.MinY, envelope.MaxY })
                {
                    foreach (var z in new[] { envelope.MinZ, envelope.MaxZ })
                    {
                        corners.Add(viewpoint.Project(new Point3D(x, y, z) + offset));
                    }
                }
            }

            return Bounds2D.FromPoints(corners);
        }
    }
}
