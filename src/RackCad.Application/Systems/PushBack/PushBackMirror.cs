using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — la REFLEXION rigida que lleva el lado B (y una corrida B-&gt;A) de su marco local al del rack.
    ///
    /// <para>
    /// Es una transformacion fisica real, no un espejo de decoracion: un Push Back del lado B es la imagen especular
    /// de uno del lado A respecto del plano medio del rack —las MISMAS piezas, la mano contraria— y por eso todo su
    /// contenido (riel, rodillos, tarima, topes de cama, largueros e intermedios) comparte UNA sola transformacion.
    /// Las elevaciones no se tocan: el eje de reflexion es vertical, asi que el extremo bajo sigue bajo y el alto,
    /// alto. Cambiar de lado no invierte la pendiente por arte grafico; la invierte porque el pasillo esta al otro
    /// extremo.
    /// </para>
    /// <para>
    /// La regla de bloques es la que impone la composicion de transformaciones: reflejar
    /// <c>T(t)·R(θ)·S(±1,1)</c> respecto de un eje vertical da <c>T(M t)·R(−θ)·S(∓1,1)</c>. Es decir: la X se refleja,
    /// la ROTACION se niega y el espejo en X se conmuta. Negar la rotacion es lo que impide que una cama inclinada
    /// aparezca inclinada al reves.
    /// </para>
    /// </summary>
    public static class PushBackMirror
    {
        /// <summary>La X reflejada respecto del eje.</summary>
        public static double X(double axis, double x) => axis - x;

        /// <summary>
        /// Una instancia reflejada respecto del eje vertical <paramref name="axis"/>.
        ///
        /// <para>
        /// Una pieza FISICA se refleja de verdad: X reflejada, rotacion NEGADA y espejo en X conmutado. Una
        /// ANOTACION o una COTA no son piezas: solo se traslada su POSICION. Reflejar un texto lo dejaria escrito
        /// del reves, y negar la rotacion de una cota horizontal la volteria sin necesidad. En una cota VERTICAL si
        /// se invierte el signo del desplazamiento, porque su linea de cota vive al otro lado tras el espejo.
        /// </para>
        /// </summary>
        public static HeaderBlockInstance Instance(HeaderBlockInstance source, double axis)
        {
            if (source == null)
            {
                return null;
            }

            var isText = source.Role == HeaderBlockRole.Annotation || source.Role == HeaderBlockRole.Dimension;
            var vertical = System.Math.Abs(source.ConnectionAnchor.X - source.Insertion.X)
                           < System.Math.Abs(source.ConnectionAnchor.Y - source.Insertion.Y);
            var clone = new HeaderBlockInstance
            {
                Role = source.Role,
                PieceId = source.PieceId,
                BlockName = source.BlockName,
                View = source.View,
                Insertion = new Point2D(X(axis, source.Insertion.X), source.Insertion.Y),
                ConnectionAnchor = new Point2D(X(axis, source.ConnectionAnchor.X), source.ConnectionAnchor.Y),
                RotationRadians = isText ? source.RotationRadians : -source.RotationRadians,
                MirroredX = isText ? source.MirroredX : !source.MirroredX,
                MirroredY = source.MirroredY,
                Text = source.Text,
                TextHeight = source.TextHeight,
                DimensionOffset = source.Role == HeaderBlockRole.Dimension && vertical
                    ? -source.DimensionOffset
                    : source.DimensionOffset,
                DimensionStyleName = source.DimensionStyleName
            };

            foreach (var pair in source.DynamicParameters)
            {
                clone.DynamicParameters[pair.Key] = pair.Value;
            }

            return clone;
        }

        /// <summary>Varias instancias reflejadas.</summary>
        public static IReadOnlyList<HeaderBlockInstance> Instances(
            IEnumerable<HeaderBlockInstance> source, double axis)
            => (source ?? Enumerable.Empty<HeaderBlockInstance>())
                .Where(instance => instance != null)
                .Select(instance => Instance(instance, axis))
                .ToList();

        /// <summary>
        /// Un grupo reflejado CONSERVANDO el patron ARRAY: se refleja la DEFINICION anidada respecto de su propio
        /// origen local y se lleva cada colocacion al otro lado del eje. Asi el lado B no paga una definicion por
        /// instancia — que es justo el cuello de botella que el patron existe para evitar.
        /// </summary>
        public static HeaderGroup Group(HeaderGroup source, double axis, string nameSuffix = null)
        {
            if (source == null)
            {
                return null;
            }

            var definition = source.Instances.Select(instance => Instance(instance, 0.0)).ToList();
            var placements = source.Placements
                .Select(placement => new HeaderPlacement(
                    X(axis, placement.InsertionX), placement.Mirrored, placement.InsertionY))
                .ToList();
            return new HeaderGroup((source.Name ?? string.Empty) + (nameSuffix ?? string.Empty), definition, placements);
        }

        /// <summary>Un plan completo reflejado (grupos y sueltas).</summary>
        public static HeaderRunPlan Plan(HeaderRunPlan source, double axis, string nameSuffix = null)
            => source == null
                ? new HeaderRunPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>())
                : new HeaderRunPlan(
                    source.Headers.Select(group => Group(group, axis, nameSuffix)).ToList(),
                    Instances(source.LooseInstances, axis).ToList());

        /// <summary>
        /// La sub-estructura REFLEJADA de un rack: los mismos modulos en orden inverso y los mismos frentes con su
        /// rango de profundidad reflejado. Sirve para resolver una corrida B-&gt;A con el codigo de siempre —el que
        /// sabe que el flujo avanza hacia +X— y devolver el resultado al mundo con <see cref="Plan"/>.
        /// <para>
        /// La retícula TRANSVERSAL no se toca: la reflexion es en el eje de la profundidad.
        /// </para>
        /// </summary>
        public static DynamicRackSystem Structure(DynamicRackSystem source) => Structure(source, null);

        /// <summary>
        /// La misma reflexion, con la opcion de re-declarar que ranuras estan ACTIVAS. Se usa para proyectar un lado
        /// sobre la retícula compartida: una ranura que no existe en ese lado viaja como frente EN BLANCO (I-33), de
        /// modo que conserva su claro —y con el la retícula transversal, que es unica— sin aportar ninguna carga.
        /// </summary>
        public static DynamicRackSystem Structure(DynamicRackSystem source, Func<int, bool> isActive)
        {
            if (source == null)
            {
                return null;
            }

            var mirrored = new DynamicRackSystem
            {
                Kind = source.Kind,
                Pallet = source.Pallet,
                PalletsDeep = source.PalletsDeep,
                BaseDepthStartPosition = source.BaseDepthStartPosition,
                BasePalletsDeep = source.BasePalletsDeep,
                PalletTolerance = source.PalletTolerance,
                InOutBeamCatalogId = source.InOutBeamCatalogId,
                PostPeralte = source.PostPeralte,
                InOutBeamDepth = source.InOutBeamDepth,
                SeparatorCountOverride = source.SeparatorCountOverride,
                SeparatorSpacingOverride = source.SeparatorSpacingOverride,
                DerivedPostReinforced = source.DerivedPostReinforced,
                DerivedPostReinforcementHeight = source.DerivedPostReinforcementHeight,
                DerivedPostHeight = source.DerivedPostHeight,
                ManualHeaderHeightOverride = source.ManualHeaderHeightOverride,
                NumberFronts = source.NumberFronts,
                NumberLevels = source.NumberLevels,
                DrawRackName = source.DrawRackName,
                AnnotationScale = source.AnnotationScale,
                Dimensions = source.Dimensions,
                DimensionStyle = source.DimensionStyle,
                Name = source.Name
            };

            foreach (var level in source.LoadBeamLevels)
            {
                mirrored.LoadBeamLevels.Add(level);
            }

            foreach (var peralte in source.IntermediateBeamDepths)
            {
                mirrored.IntermediateBeamDepths.Add(peralte);
            }

            foreach (var selection in source.SafetySelections)
            {
                mirrored.SafetySelections.Add(selection);
            }

            foreach (var line in source.HeaderLineOverrides)
            {
                mirrored.HeaderLineOverrides.Add(line);
            }

            foreach (var line in source.DerivedPostLineOverrides)
            {
                mirrored.DerivedPostLineOverrides.Add(line);
            }

            // I-42 (ronda 6D): el tramo interior tambien es de PROFUNDIDAD, asi que viaja reflejado.
            if (source.InteriorFaceStartX.HasValue && source.InteriorFaceEndX.HasValue)
            {
                mirrored.InteriorFaceStartX = source.TotalLength - source.InteriorFaceEndX.Value;
                mirrored.InteriorFaceEndX = source.TotalLength - source.InteriorFaceStartX.Value;
            }

            // I-42 (ronda 6D): las zonas de altura viajan con la reflexion, porque son TRAMOS DE PROFUNDIDAD. Sus
            // alturas por linea no se tocan: una reflexion en profundidad no reordena la retícula transversal.
            foreach (var zone in source.HeaderHeightZones)
            {
                if (zone == null)
                {
                    continue;
                }

                var copy = new DynamicHeaderHeightZone
                {
                    StartX = source.TotalLength - zone.EndX,
                    EndX = source.TotalLength - zone.StartX
                };
                foreach (var height in zone.HeightByLine)
                {
                    copy.HeightByLine.Add(height);
                }

                mirrored.HeaderHeightZones.Add(copy);
            }

            var total = source.Modules.Count;
            foreach (var module in Enumerable.Reverse(source.Modules.ToList()))
            {
                mirrored.Modules.Add(new DynamicRackModule
                {
                    ModuleId = module.ModuleId,
                    Kind = module.Kind,
                    Length = module.Length,
                    IsCalculated = module.IsCalculated,
                    IsManualOverride = module.IsManualOverride,
                    UseCalculatedHeaderConfiguration = module.UseCalculatedHeaderConfiguration,
                    AssociatedFrameConfiguration = module.AssociatedFrameConfiguration,
                    Notes = module.Notes
                });
            }

            mirrored.RecalculatePositions();

            foreach (var front in source.Fronts)
            {
                var copy = new DynamicRackFront
                {
                    Index = front.Index,
                    IsActive = isActive == null ? front.IsActive : isActive(front.Index),
                    PalletCount = front.PalletCount,
                    LoadLevels = front.LoadLevels,
                    PalletsDeep = front.PalletsDeep,
                    // El rango se refleja: la ultima posicion pasa a ser la primera.
                    DepthStartPosition = total - (front.DepthStartPosition + front.PalletsDeep - 1) + 1,
                    Bfr = front.Bfr,
                    BeamLength = front.BeamLength,
                    BeamLengthOverride = front.BeamLengthOverride,
                    FirstLevelHeight = front.FirstLevelHeight,
                    Height = front.Height
                };

                foreach (var level in front.LoadBeamLevels)
                {
                    copy.LoadBeamLevels.Add(level);
                }

                foreach (var peralte in front.IntermediateBeamDepths)
                {
                    copy.IntermediateBeamDepths.Add(peralte);
                }

                foreach (var level in front.Levels)
                {
                    copy.Levels.Add(level);
                }

                // Los TRAMOS de profundidad son geometria, asi que se reflejan como el rango: la ultima posicion
                // pasa a ser la primera. Sin esto una estructura reflejada declararia sus tramos en el marco del
                // que viene y la cobertura por linea saldria al reves.
                foreach (var segment in front.DepthSegments)
                {
                    copy.DepthSegments.Add(new DynamicDepthSegment(
                        total - segment.EndPosition + 1, segment.Positions));
                }

                mirrored.Fronts.Add(copy);
            }

            foreach (var front in mirrored.Fronts)
            {
                var first = mirrored.Modules.FirstOrDefault(module => module.Index + 1 == front.DepthStartPosition);
                var last = mirrored.Modules.FirstOrDefault(
                    module => module.Index + 1 == front.DepthStartPosition + front.PalletsDeep - 1);
                front.StartX = first?.StartX ?? 0.0;
                front.EndX = last?.EndX ?? front.StartX;
            }

            return mirrored;
        }

        /// <summary>
        /// Copia INDEPENDIENTE (sin reflejar) de una estructura, opcionalmente re-declarando que ranuras estan
        /// activas. Se apoya en la reflexion aplicada dos veces, que es la identidad, para no mantener un segundo
        /// clonador que pueda olvidarse un campo.
        /// </summary>
        public static DynamicRackSystem Clone(DynamicRackSystem source, Func<int, bool> isActive = null)
            => Structure(Structure(source), isActive);

        /// <summary>El eje de reflexion de un rack: su longitud total, de modo que 0 y la longitud se intercambian.</summary>
        public static double AxisOf(DynamicRackSystem structure) => structure?.TotalLength ?? 0.0;

        /// <summary>Comprueba que dos valores son reflejo uno del otro (usada por las pruebas y las guardas).</summary>
        public static bool AreReflected(double axis, double left, double right, double tolerance = 1e-6)
            => Math.Abs(X(axis, left) - right) <= tolerance;
    }
}
