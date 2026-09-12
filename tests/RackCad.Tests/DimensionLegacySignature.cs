using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.Drawing;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, G4 Paso 1 — la firma de la CAPA DE ANOTACIÓN de una vista tal como hoy la dibuja el Plugin: las
    /// instancias <see cref="HeaderBlockRole.Dimension"/> y las etiquetas <see cref="HeaderBlockRole.Annotation"/>
    /// (números de frente y de nivel, nombre del rack, rótulos A/B). Es exactamente lo que emiten los dos emisores de
    /// cotas y lo que desplaza su alcance; la geometría de las piezas ya la fijan las golden existentes.
    ///
    /// <para>
    /// Cada pin se lee sin abrir nada: <c>D</c> cuántas cotas, <c>L</c> cuántas etiquetas y, entre corchetes, la caja
    /// de las etiquetas (X mínima, Y mínima; X máxima, Y máxima), que es donde se ve el alcance —una cota activa empuja
    /// los números hacia fuera—. El SHA-256 fija todo lo demás, fila a fila: puntos medidos, desfase de la línea de
    /// cota, altura de texto, estilo de cota, texto y posición de cada etiqueta.
    /// </para>
    /// </summary>
    internal static class DimensionLegacySignature
    {
        /// <summary>Los cuatro niveles. <c>None</c> fija dónde quedan hoy las etiquetas sin cotas: la base del alcance.</summary>
        internal static readonly IReadOnlyList<DimensionDetail> Levels = new[]
        {
            DimensionDetail.None,
            DimensionDetail.Minimal,
            DimensionDetail.Standard,
            DimensionDetail.Detailed
        };

        internal static string Of(IEnumerable<HeaderBlockInstance> instances)
        {
            var all = (instances ?? Enumerable.Empty<HeaderBlockInstance>()).ToList();
            var dimensions = all.Where(instance => instance.Role == HeaderBlockRole.Dimension).ToList();
            var labels = all.Where(instance => instance.Role == HeaderBlockRole.Annotation).ToList();
            var rows = dimensions.Concat(labels).Select(Row).OrderBy(row => row, StringComparer.Ordinal);
            var sha = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", rows))));
            return FormattableString.Invariant($"D{dimensions.Count} L{labels.Count} {Box(labels)} {sha}");
        }

        /// <summary>Solo las etiquetas, fila a fila: sirve para comparar su posición entre niveles de detalle.</summary>
        internal static string LabelsOnly(IEnumerable<HeaderBlockInstance> instances)
            => string.Join("\n", (instances ?? Enumerable.Empty<HeaderBlockInstance>())
                .Where(instance => instance.Role == HeaderBlockRole.Annotation)
                .Select(Row)
                .OrderBy(row => row, StringComparer.Ordinal));

        internal static int DimensionCount(IEnumerable<HeaderBlockInstance> instances)
            => (instances ?? Enumerable.Empty<HeaderBlockInstance>()).Count(instance => instance.Role == HeaderBlockRole.Dimension);

        internal static int LabelCount(IEnumerable<HeaderBlockInstance> instances)
            => (instances ?? Enumerable.Empty<HeaderBlockInstance>()).Count(instance => instance.Role == HeaderBlockRole.Annotation);

        /// <summary>
        /// Las diferencias entre los pines y lo calculado, listas para pegar como inicializador. Vacío = todo coincide.
        /// Una clave que falta en uno de los dos lados también es diferencia: ni una vista nueva ni una que desaparece
        /// pasan en silencio.
        /// </summary>
        internal static string Drift(IReadOnlyDictionary<string, string> pins, IReadOnlyDictionary<string, string> actual)
        {
            var lines = new List<string>();
            foreach (var key in actual.Keys.Union(pins.Keys).OrderBy(key => key, StringComparer.Ordinal))
            {
                actual.TryGetValue(key, out var got);
                pins.TryGetValue(key, out var pinned);
                if (string.Equals(got, pinned, StringComparison.Ordinal))
                {
                    continue;
                }

                lines.Add(got == null
                    ? $"// sobra el pin \"{key}\": la vista ya no se calcula"
                    : $"[\"{key}\"] = \"{got}\",   // pin: {pinned ?? "(falta)"}");
            }

            return string.Join("\n", lines);
        }

        /// <summary>El subconjunto de pines de una familia de vistas.</summary>
        internal static IReadOnlyDictionary<string, string> Where(
            IReadOnlyDictionary<string, string> pins, Func<string, bool> belongs)
            => pins.Where(pin => belongs(pin.Key)).ToDictionary(pin => pin.Key, pin => pin.Value, StringComparer.Ordinal);

        private static string Row(HeaderBlockInstance i)
            => FormattableString.Invariant(
                $"{i.View}|{i.Role}|{i.PieceId}|{i.BlockName}|{i.Text}|{i.Insertion.X:0.####}|{i.Insertion.Y:0.####}|{i.ConnectionAnchor.X:0.####}|{i.ConnectionAnchor.Y:0.####}|{i.RotationRadians:0.######}|{(i.MirroredX ? 1 : 0)}|{(i.MirroredY ? 1 : 0)}|{i.DimensionOffset:0.####}|{i.TextHeight:0.####}|{i.DimensionStyleName}|{Params(i)}");

        private static string Params(HeaderBlockInstance i)
            => string.Join(",", i.DynamicParameters.OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => FormattableString.Invariant($"{p.Key}={p.Value:0.####}")));

        private static string Box(IReadOnlyCollection<HeaderBlockInstance> labels)
        {
            if (labels.Count == 0)
            {
                return "[]";
            }

            var minX = labels.Min(label => label.Insertion.X);
            var minY = labels.Min(label => label.Insertion.Y);
            var maxX = labels.Max(label => label.Insertion.X);
            var maxY = labels.Max(label => label.Insertion.Y);
            return FormattableString.Invariant($"[{minX:0.####},{minY:0.####};{maxX:0.####},{maxY:0.####}]");
        }
    }
}
