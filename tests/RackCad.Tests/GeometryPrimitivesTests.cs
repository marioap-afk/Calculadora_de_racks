using System;
using System.Linq;
using RackCad.Application.Geometry;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Las primitivas neutrales que I-36B anadio de forma ADITIVA a <c>Application.Geometry</c>.
    ///
    /// Se prueban aparte de las secciones porque son geometria general: si un contorno de una W sale mal,
    /// estas pruebas dicen si el problema esta en la regla de la familia o en el area con signo.
    /// </summary>
    public class GeometryPrimitivesTests
    {
        private const double Tol = 1e-9;

        // ---- Vector2D -----------------------------------------------------------------------------------

        [Fact]
        public void Vector_BetweenTwoPoints_IsTheDifference()
        {
            var v = Vector2D.Between(new Point2D(1, 2), new Point2D(4, 6));

            Assert.Equal(3, v.X, 12);
            Assert.Equal(4, v.Y, 12);
            Assert.Equal(5, v.Length, 12);
        }

        [Fact]
        public void Normalizing_AZeroVector_Throws()
        {
            // Devolver cero seria peor que lanzar: viajaria en silencio hasta un contorno.
            Assert.Throws<InvalidOperationException>(() => Vector2D.Zero.Normalized());
        }

        [Fact]
        public void Perpendicular_TurnsAQuarterCounterClockwise()
        {
            var p = Vector2D.UnitX.Perpendicular();

            Assert.True(p.ApproxEquals(Vector2D.UnitY, Tol));
            Assert.Equal(0, Vector2D.UnitX.Dot(p), 12);
            Assert.Equal(1, Vector2D.UnitX.Cross(p), 12);
        }

        // ---- Bounds2D -----------------------------------------------------------------------------------

        [Fact]
        public void Bounds_FromPoints_CoversThemAll()
        {
            var b = Bounds2D.FromPoints(new[] { new Point2D(-2, 5), new Point2D(3, -1), new Point2D(0, 0) });

            Assert.Equal(-2, b.MinX, 12);
            Assert.Equal(-1, b.MinY, 12);
            Assert.Equal(3, b.MaxX, 12);
            Assert.Equal(5, b.MaxY, 12);
            Assert.Equal(5, b.Width, 12);
            Assert.Equal(6, b.Height, 12);
            Assert.True(b.HasArea);
        }

        [Fact]
        public void Bounds_RefuseAnEmptySetAndInvertedLimits()
        {
            Assert.Throws<ArgumentException>(() => Bounds2D.FromPoints(Array.Empty<Point2D>()));
            Assert.Throws<ArgumentException>(() => new Bounds2D(5, 0, 1, 1));
        }

        // ---- Transform2D --------------------------------------------------------------------------------

        [Fact]
        public void Rotation_By90Degrees_TakesXToY()
        {
            var moved = Transform2D.RotationDegrees(90).Apply(new Point2D(1, 0));

            Assert.Equal(0, moved.X, 9);
            Assert.Equal(1, moved.Y, 9);
        }

        [Fact]
        public void Mirroring_ReversesOrientation_AndPlainRotationDoesNot()
        {
            Assert.True(Transform2D.MirrorAboutY.ReversesOrientation);
            Assert.True(Transform2D.MirrorAboutX.ReversesOrientation);
            Assert.False(Transform2D.RotationDegrees(37).ReversesOrientation);
            Assert.False(Transform2D.Translation(3, -4).ReversesOrientation);
        }

        [Fact]
        public void Compose_ReadsLeftToRight()
        {
            // Rotar 90 y DESPUES trasladar: el punto (1,0) va a (0,1) y luego a (10,1).
            var combined = Transform2D.RotationDegrees(90).Then(Transform2D.Translation(10, 0));
            var moved = combined.Apply(new Point2D(1, 0));

            Assert.Equal(10, moved.X, 9);
            Assert.Equal(1, moved.Y, 9);
        }

        [Fact]
        public void ATranslationDoesNotMoveAFreeVector()
        {
            var v = Transform2D.Translation(100, -50).Apply(new Vector2D(1, 2));

            Assert.True(v.ApproxEquals(new Vector2D(1, 2), Tol));
        }

        [Fact]
        public void NonUniformScalingCannotBeBuilt()
        {
            // Escalar distinto en X y en Y convertiria un arco en una elipse, y este tipo promete arcos.
            Assert.Throws<ArgumentException>(() => Transform2D.Scale(0));
            Assert.Throws<ArgumentException>(() => Transform2D.Scale(-2));
        }

        // ---- PathSegment2D ------------------------------------------------------------------------------

        [Fact]
        public void ADegenerateSegmentIsRejected()
        {
            Assert.Throws<ArgumentException>(() => PathSegment2D.Line(new Point2D(1, 1), new Point2D(1, 1)));
            Assert.Throws<ArgumentException>(() => PathSegment2D.Arc(new Point2D(0, 0), 1, 0, 0));
            Assert.Throws<ArgumentException>(() => PathSegment2D.Arc(new Point2D(0, 0), -1, 0, 1));
        }

        [Fact]
        public void ArcEndpointsAreDerivedFromItsAngles()
        {
            var arc = PathSegment2D.QuarterArc(new Point2D(0, 0), 2, 0, counterClockwise: true);

            Assert.True(arc.Start.ApproxEquals(new Point2D(2, 0), 1e-12));
            Assert.True(arc.End.ApproxEquals(new Point2D(0, 2), 1e-12));
            Assert.Equal(Math.PI, arc.Length, 9);
        }

        [Fact]
        public void ReversingAnArcKeepsItsGeometryAndFlipsItsSweep()
        {
            var arc = PathSegment2D.QuarterArc(new Point2D(1, 1), 0.5, 0.3, counterClockwise: true);
            var back = arc.Reversed();

            Assert.True(back.Start.ApproxEquals(arc.End, 1e-12));
            Assert.True(back.End.ApproxEquals(arc.Start, 1e-12));
            Assert.Equal(-arc.SweepAngle, back.SweepAngle, 12);
            Assert.Equal(arc.Radius, back.Radius, 12);
        }

        [Fact]
        public void TransformingAnArcKeepsItCircular_AndAMirrorFlipsItsSweep()
        {
            var arc = PathSegment2D.QuarterArc(new Point2D(3, 0), 1, 0, counterClockwise: true);

            var rotated = arc.Transformed(Transform2D.RotationDegrees(90));
            Assert.Equal(arc.Radius, rotated.Radius, 9);
            Assert.Equal(arc.SweepAngle, rotated.SweepAngle, 9);
            Assert.Equal(arc.Length, rotated.Length, 9);

            var mirrored = arc.Transformed(Transform2D.MirrorAboutY);
            Assert.Equal(arc.Radius, mirrored.Radius, 9);
            Assert.Equal(-arc.SweepAngle, mirrored.SweepAngle, 9);
            Assert.True(mirrored.Start.ApproxEquals(new Point2D(-arc.Start.X, arc.Start.Y), 1e-9));
            Assert.True(mirrored.End.ApproxEquals(new Point2D(-arc.End.X, arc.End.Y), 1e-9));
        }

        // ---- Teselacion ---------------------------------------------------------------------------------

        [Fact]
        public void Flattening_KeepsEndpointsExactly()
        {
            var arc = PathSegment2D.QuarterArc(new Point2D(0, 0), 5, 0.7, counterClockwise: true);
            var points = arc.FlattenAfterStart(0.01).ToArray();

            // El ultimo punto es el extremo ALMACENADO, no uno recalculado: si se moviera, un contorno
            // cerrado dejaria de cerrar.
            Assert.Equal(arc.End.X, points[points.Length - 1].X, 15);
            Assert.Equal(arc.End.Y, points[points.Length - 1].Y, 15);
        }

        [Fact]
        public void Flattening_IsDeterministic()
        {
            var arc = PathSegment2D.QuarterArc(new Point2D(1, 2), 3, 0.4, counterClockwise: false);

            Assert.Equal(
                arc.FlattenAfterStart(0.005).Select(p => p.ToString()),
                arc.FlattenAfterStart(0.005).Select(p => p.ToString()));
        }

        [Theory]
        [InlineData(0.1, 0.01)]
        [InlineData(0.01, 0.001)]
        [InlineData(0.001, 0.0001)]
        public void ATighterToleranceNeverProducesACoarserResult(double loose, double tight)
        {
            var arc = PathSegment2D.QuarterArc(new Point2D(0, 0), 4, 0, counterClockwise: true);

            Assert.True(arc.FlattenAfterStart(tight).Count() >= arc.FlattenAfterStart(loose).Count());
        }

        [Fact]
        public void Flattening_ProducesNoZeroLengthStepAndNoDuplicate()
        {
            var arc = PathSegment2D.QuarterArc(new Point2D(0, 0), 2, 0, counterClockwise: true);
            var points = new[] { arc.Start }.Concat(arc.FlattenAfterStart(0.02)).ToArray();

            for (var i = 1; i < points.Length; i++)
            {
                Assert.False(points[i].ApproxEquals(points[i - 1], GeometryTolerance.Length));
            }
        }

        [Fact]
        public void FlatteningRefusesANonPositiveTolerance()
        {
            var arc = PathSegment2D.QuarterArc(new Point2D(0, 0), 1, 0, counterClockwise: true);

            Assert.Throws<ArgumentException>(() => arc.FlattenAfterStart(0).ToArray());
            Assert.Throws<ArgumentException>(() => arc.FlattenAfterStart(-0.1).ToArray());
        }

        // ---- ClosedContour2D ----------------------------------------------------------------------------

        [Fact]
        public void ASquare_HasTheExpectedAreaCentroidAndOrientation()
        {
            var square = ClosedContour2D.FromPolygon(new[]
            {
                new Point2D(0, 0), new Point2D(4, 0), new Point2D(4, 4), new Point2D(0, 4)
            });

            Assert.Equal(16, square.Area, 12);
            Assert.Equal(ContourOrientation.CounterClockwise, square.Orientation);
            Assert.Equal(2, square.Centroid.X, 12);
            Assert.Equal(2, square.Centroid.Y, 12);
            Assert.True(square.Bounds.ApproxEquals(new Bounds2D(0, 0, 4, 4), Tol));
        }

        [Fact]
        public void ReversingAContourFlipsItsOrientationAndKeepsItsArea()
        {
            var square = ClosedContour2D.FromPolygon(new[]
            {
                new Point2D(0, 0), new Point2D(2, 0), new Point2D(2, 2), new Point2D(0, 2)
            });
            var hole = square.Reversed();

            Assert.Equal(ContourOrientation.Clockwise, hole.Orientation);
            Assert.Equal(square.Area, hole.Area, 12);
            Assert.Equal(-square.SignedArea, hole.SignedArea, 12);
            Assert.Equal(square.Centroid.X, hole.Centroid.X, 12);
        }

        [Fact]
        public void ADiscontinuousContourIsRejectedAtConstruction()
        {
            var error = Assert.Throws<ArgumentException>(() => ClosedContour2D.Create(new[]
            {
                PathSegment2D.Line(new Point2D(0, 0), new Point2D(1, 0)),
                PathSegment2D.Line(new Point2D(5, 5), new Point2D(0, 0))
            }));

            Assert.Contains("no es continuo", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void ACircleBuiltFromFourArcs_HasTheAreaOfACircle()
        {
            // Prueba exacta del area con signo cuando TODO el contorno son arcos.
            const double r = 3.0;
            var center = new Point2D(0, 0);
            var circle = ClosedContour2D.Create(new[]
            {
                PathSegment2D.QuarterArc(center, r, 0, true),
                PathSegment2D.QuarterArc(center, r, Math.PI / 2, true),
                PathSegment2D.QuarterArc(center, r, Math.PI, true),
                PathSegment2D.QuarterArc(center, r, 3 * Math.PI / 2, true)
            });

            Assert.Equal(Math.PI * r * r, circle.Area, 9);
            Assert.Equal(0, circle.Centroid.X, 9);
            Assert.Equal(0, circle.Centroid.Y, 9);
        }

        [Fact]
        public void BoundsAccountForTheBulgeOfAnArc_NotOnlyItsEndpoints()
        {
            // El arco de 0 a 90 grados sobresale hasta x=1 e y=1, mas alla de la cuerda entre sus extremos.
            var contour = ClosedContour2D.Create(new[]
            {
                PathSegment2D.QuarterArc(new Point2D(0, 0), 1, 0, true),
                PathSegment2D.Line(new Point2D(0, 1), new Point2D(0, 0)),
                PathSegment2D.Line(new Point2D(0, 0), new Point2D(1, 0))
            });

            Assert.True(contour.Bounds.ApproxEquals(new Bounds2D(0, 0, 1, 1), 1e-9));
            Assert.Equal(Math.PI / 4.0, contour.Area, 9);
        }

        [Fact]
        public void ARoundedRectangle_HasTheExactAnalyticArea()
        {
            const double w = 6.0, h = 4.0, r = 0.5;
            var contour = RoundedRectangle(w, h, r);

            Assert.Equal((w * h) - ((4.0 - Math.PI) * r * r), contour.Area, 9);
            Assert.Equal(ContourOrientation.CounterClockwise, contour.Orientation);
            Assert.Equal(0, contour.Centroid.X, 9);
            Assert.Equal(0, contour.Centroid.Y, 9);
            Assert.True(contour.Bounds.ApproxEquals(new Bounds2D(-w / 2, -h / 2, w / 2, h / 2), 1e-9));
        }

        [Fact]
        public void TransformingAContour_PreservesAreaAndKeepsOrientationAcrossAMirror()
        {
            var contour = RoundedRectangle(6, 4, 0.5);

            var rotated = contour.Transformed(Transform2D.RotationDegrees(37));
            Assert.Equal(contour.Area, rotated.Area, 9);
            Assert.Equal(contour.Orientation, rotated.Orientation);

            // Un espejo invierte el sentido; el contorno se revierte para conservar el contrato del llamador,
            // porque si no un contorno exterior se convertiria en silencio en un hueco.
            var mirrored = contour.Transformed(Transform2D.MirrorAboutY);
            Assert.Equal(contour.Area, mirrored.Area, 9);
            Assert.Equal(contour.Orientation, mirrored.Orientation);
        }

        [Fact]
        public void FlatteningAContourClosesItWithoutDuplicatingTheFirstPoint()
        {
            var points = RoundedRectangle(6, 4, 0.5).Flatten(0.01);

            Assert.False(points[points.Count - 1].ApproxEquals(points[0], GeometryTolerance.Continuity));

            for (var i = 1; i < points.Count; i++)
            {
                Assert.False(points[i].ApproxEquals(points[i - 1], GeometryTolerance.Length));
            }
        }

        /// <summary>Un rectangulo con esquinas redondeadas centrado en el origen, CCW.</summary>
        internal static ClosedContour2D RoundedRectangle(double width, double height, double radius)
        {
            var hw = width / 2.0;
            var hh = height / 2.0;
            var ix = hw - radius;
            var iy = hh - radius;

            return ClosedContour2D.Create(new[]
            {
                PathSegment2D.Line(new Point2D(-ix, -hh), new Point2D(ix, -hh)),
                PathSegment2D.QuarterArc(new Point2D(ix, -iy), radius, -Math.PI / 2, true),
                PathSegment2D.Line(new Point2D(hw, -iy), new Point2D(hw, iy)),
                PathSegment2D.QuarterArc(new Point2D(ix, iy), radius, 0, true),
                PathSegment2D.Line(new Point2D(ix, hh), new Point2D(-ix, hh)),
                PathSegment2D.QuarterArc(new Point2D(-ix, iy), radius, Math.PI / 2, true),
                PathSegment2D.Line(new Point2D(-hw, iy), new Point2D(-hw, -iy)),
                PathSegment2D.QuarterArc(new Point2D(-ix, -iy), radius, Math.PI, true)
            });
        }

        // ---- Marcos 3D ----------------------------------------------------------------------------------

        [Fact]
        public void TheWorldFrameIsOrthonormalAndRightHanded()
        {
            var frame = LocalFrame3D.World;

            Assert.True(frame.AxisX.Cross(frame.AxisY).ApproxEquals(frame.AxisZ, 1e-12));
            Assert.Equal(0, frame.AxisX.Dot(frame.AxisY), 12);
        }

        [Fact]
        public void CreatingAFrame_ReorthogonalizesTheReference()
        {
            // La referencia no es perpendicular a Z; el marco la corrige en vez de producir un marco sesgado.
            var frame = LocalFrame3D.Create(Point3D.Origin, new Vector3D(0, 0, 2), new Vector3D(1, 0, 5));

            Assert.Equal(0, frame.AxisX.Dot(frame.AxisZ), 12);
            Assert.Equal(1, frame.AxisX.Length, 12);
            Assert.True(frame.AxisX.Cross(frame.AxisY).ApproxEquals(frame.AxisZ, 1e-9));
        }

        [Fact]
        public void AReferenceParallelToZIsRejected()
        {
            Assert.Throws<ArgumentException>(
                () => LocalFrame3D.Create(Point3D.Origin, Vector3D.UnitZ, Vector3D.UnitZ));
        }

        [Fact]
        public void ANonOrthonormalTripleIsRejected()
        {
            Assert.Throws<ArgumentException>(() => LocalFrame3D.FromAxes(
                Point3D.Origin, Vector3D.UnitX, new Vector3D(0.7, 0.7, 0), Vector3D.UnitZ));

            // Y un triple zurdo tampoco pasa.
            Assert.Throws<ArgumentException>(() => LocalFrame3D.FromAxes(
                Point3D.Origin, Vector3D.UnitX, Vector3D.UnitZ, Vector3D.UnitY));
        }

        [Fact]
        public void AFrameRoundTripsAPoint()
        {
            var frame = LocalFrame3D.Create(new Point3D(5, -2, 1), new Vector3D(1, 1, 0), new Vector3D(0, 0, 1));
            var local = new Point3D(3, 4, 5);
            var back = frame.ToLocal(frame.ToWorld(local));

            Assert.Equal(local.X, back.X, 9);
            Assert.Equal(local.Y, back.Y, 9);
            Assert.Equal(local.Z, back.Z, 9);
        }
    }
}
