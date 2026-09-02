using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 (ronda 7D) — LA IDENTIDAD de una intencion de defensa: <b>LADO + LINEA FISICA</b>.
    ///
    /// <para>
    /// En un compuesto <c>A/P0</c> y <c>B/P0</c> comparten la linea y NO son la misma intencion: son dos caras de
    /// ataque distintas, con su propio pasillo. Esta clase es donde esa correspondencia esta escrita una sola vez.
    /// </para>
    ///
    /// <para><b>Por que no hace falta un almacen nuevo.</b> La ronda 6D ya establecio que la seguridad es del RACK y
    /// que un compuesto tiene DOS pasillos, uno en cada extremo de la cobertura de cada linea; el constructor coloca
    /// la defensa del cercano con <c>ExitLength</c> y la del lejano con <c>EntranceLength</c>. Es decir: el registro
    /// por poste YA distingue las dos caras, con un campo independiente para cada una. Lo que faltaba era NOMBRARLAS
    /// por lado —el lado A ataca por el extremo CERCANO, el B por el LEJANO— en vez de por el vocabulario de un rack
    /// de un solo sentido («entrada/salida» y «posterior»), que es lo que dejaba al lado B sin superficie.
    /// </para>
    ///
    /// <para>
    /// No es una codificacion: no hay desplazamientos de indice, ni signos, ni coordenadas redondeadas. Es la misma
    /// pareja de campos que el dibujo lee, con el nombre fisico que les corresponde en un rack compuesto.
    /// </para>
    /// </summary>
    public static class PushBackDefenseSides
    {
        /// <summary>El lado A ataca por el extremo CERCANO de la cobertura; el lado B por el LEJANO.</summary>
        public static bool IsFarEnd(PushBackSide side) => side == PushBackSide.B;

        /// <summary>«Ninguno»: ese lado no materializa defensa, decida lo que decida su rejilla por poste.</summary>
        public static bool IsNone(string pieceId)
            => !string.IsNullOrWhiteSpace(pieceId)
               && string.Equals(pieceId.Trim(), PushBackDefaults.NonePieceId, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// I-42 (ronda 7E) — LA CARA que un lado declara, lista para que la lea el dibujo. Tres estados y solo tres:
        /// NULL en el diseno = heredar la pieza de la seleccion (todo documento anterior a esta ronda, y todo rack
        /// que nunca haya tocado el selector); «Ninguno» = esa cara no lleva pieza; cualquier otro id = esa pieza.
        /// </summary>
        public static SafetyFacePiece FaceOf(string pieceId)
        {
            if (string.IsNullOrWhiteSpace(pieceId))
            {
                return null;   // heredado: el comportamiento historico, sin conversion automatica de nada
            }

            return IsNone(pieceId) ? SafetyFacePiece.None : SafetyFacePiece.Of(pieceId);
        }

        /// <summary>Declara en <paramref name="selection"/> el tipo que cada lado eligio. Null-safe en los dos lados.</summary>
        public static void DeclareFaces(SelectiveSafetySelection selection, string sideAPieceId, string sideBPieceId)
        {
            if (selection == null)
            {
                return;
            }

            selection.NearFace = FaceOf(sideAPieceId);
            selection.FarFace = FaceOf(sideBPieceId);
        }

        /// <summary>La longitud almacenada para la cara de <paramref name="side"/> (0 = esa cara no lleva defensa).</summary>
        public static double LengthOf(SafetyPostDefense record, PushBackSide side)
            => record == null ? 0.0 : (IsFarEnd(side) ? record.EntranceLength : record.ExitLength);

        /// <summary>Si la cara de <paramref name="side"/> sigue la regla automatica.</summary>
        public static bool AutoOf(SafetyPostDefense record, PushBackSide side)
            => record != null && (IsFarEnd(side) ? record.EntranceAuto : record.ExitAuto);

        /// <summary>Escribe la decision de UNA cara y deja la otra EXACTAMENTE como estaba.</summary>
        public static void Set(SafetyPostDefense record, PushBackSide side, double length, bool auto)
        {
            if (record == null)
            {
                return;
            }

            if (IsFarEnd(side))
            {
                record.EntranceLength = length;
                record.EntranceAuto = auto;
            }
            else
            {
                record.ExitLength = length;
                record.ExitAuto = auto;
            }
        }

        /// <summary>La longitud RESUELTA de la cara de <paramref name="side"/> — lo que el dibujo materializa.</summary>
        public static double Resolved(DynamicForkliftDefenseSetting setting, PushBackSide side)
            => IsFarEnd(side) ? setting.EntranceLength : setting.ExitLength;

        /// <summary>
        /// APPLICABILITY: si <paramref name="side"/> tiene de verdad cara de ataque en esa linea. Es la misma
        /// pregunta que hace el dibujo (<see cref="DynamicDefenseFaces"/>), no una copia.
        /// </summary>
        public static bool HasFace(DynamicRackSystem structure, int postIndex, PushBackSide side)
            => DynamicDefenseFaces.HasFace(structure, postIndex, IsFarEnd(side));

        /// <summary>
        /// La aplicabilidad de <paramref name="side"/> en cada linea, en orden — lo que una superficie de edicion
        /// necesita para ofrecer solo lo que el rack puede materializar.
        ///
        /// <para>
        /// FAIL-OPEN sobre lo que la estructura resuelta no conoce: si esa linea todavia no existe ahi —porque el
        /// modelo no se ha recalculado desde que el rack crecio— la cara se ofrece. Deshabilitar una fila por una
        /// lectura vieja le quitaria al usuario una decision que el rack SI admite, y la fisica vuelve a filtrar al
        /// dibujar. Un frente EN BLANCO no cae aqui: conserva su linea (ronda 2) y es
        /// <see cref="DynamicRackSystem.IsInteriorFace"/> quien retira SU cara, la de su lado y solo esa.
        /// </para>
        /// </summary>
        public static IReadOnlyList<bool> FacesOf(DynamicRackSystem structure, int postCount, PushBackSide side)
            => Enumerable.Range(0, Math.Max(0, postCount))
                .Select(post => structure == null
                                || !DynamicFrontActivation.BoundaryExists(structure, post)
                                || HasFace(structure, post, side))
                .ToList();

        /// <summary>Una copia profunda de los registros por poste, para que una superficie edite sin comprometer.</summary>
        public static List<SafetyPostDefense> Copy(IEnumerable<SafetyPostDefense> records)
            => (records ?? Enumerable.Empty<SafetyPostDefense>())
                .Where(record => record != null)
                .Select(record => new SafetyPostDefense
                {
                    PostIndex = record.PostIndex,
                    ExitLength = record.ExitLength,
                    EntranceLength = record.EntranceLength,
                    ExitAuto = record.ExitAuto,
                    EntranceAuto = record.EntranceAuto,
                })
                .ToList();

        /// <summary>
        /// Funde en <paramref name="target"/> lo que una superficie decidio PARA SU LADO, sin tocar el otro. Es lo
        /// que hace que editar A no pueda cambiar B ni al reves, aunque compartan la linea.
        /// </summary>
        public static List<SafetyPostDefense> Merge(
            IEnumerable<SafetyPostDefense> target,
            IEnumerable<SafetyPostDefense> edited,
            PushBackSide side)
        {
            var merged = Copy(target);
            foreach (var change in edited ?? Enumerable.Empty<SafetyPostDefense>())
            {
                if (change == null || change.PostIndex < 0)
                {
                    continue;
                }

                var record = merged.FirstOrDefault(item => item.PostIndex == change.PostIndex);
                if (record == null)
                {
                    // Sin registro previo la OTRA cara sigue siendo automatica, que es lo que «sin registro»
                    // significaba: crearlo no puede convertirla en un cero explicito.
                    record = new SafetyPostDefense
                    {
                        PostIndex = change.PostIndex,
                        ExitAuto = true,
                        EntranceAuto = true,
                    };
                    merged.Add(record);
                }

                Set(record, side, LengthOf(change, side), AutoOf(change, side));
            }

            // Una fila que volvio a ser automatica en las DOS caras no es ninguna decision: se retira, que es lo que
            // «sin registro» significa para el plan y lo que mantiene los documentos limpios.
            merged.RemoveAll(record => record.ExitAuto && record.EntranceAuto);
            return merged.OrderBy(record => record.PostIndex).ToList();
        }
    }
}
