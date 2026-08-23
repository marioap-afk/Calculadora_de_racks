using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-41 (PB-015) — LA autoridad del fondo de una celda Push Back. Todo lo que dependa de "hasta donde llega este
    /// nivel" pregunta aqui: la cama, su longitud y su pendiente, las elevaciones, el larguero posterior, el tope
    /// posterior, los intermedios, las vistas y el BOM. Ninguna de esas piezas vuelve a leer
    /// <see cref="DynamicRackFront.PalletsDeep"/> para decidir donde acaba un nivel.
    ///
    /// <para>La regla de precedencia es UNA y no admite una tercera fuente:</para>
    /// <code>fondo efectivo(celda) = OverrideDeLaCelda ?? FondoPorDefectoDelFrente</code>
    /// <para>
    /// acotada despues a [2, envolvente del frente]: por debajo de 2 no hay Push Back, y por encima de la envolvente no
    /// hay estructura donde apoyarse. La ENVOLVENTE es justamente el maximo de los fondos efectivos de los niveles
    /// ACTIVOS del frente, de modo que esa cota superior nunca recorta una intencion legitima del usuario: solo
    /// defiende de un estado incoherente.
    /// </para>
    /// <para>
    /// Consecuencia deliberada: <see cref="DynamicRackFrontDesign.PalletsDeep"/> deja de ser la autoridad final de
    /// producto y pasa a ser esa envolvente DERIVADA. Es lo que hace que la estructura compartida —modulos, cabeceras,
    /// separadores y postes derivados de I-35/I-40— no se entere de que los niveles ya no son todos igual de profundos:
    /// mientras la envolvente no cambie, el recalculo reutiliza el baseline y conserva ModuleId, cabeceras por linea y
    /// alturas de poste derivado por linea.
    /// </para>
    /// El sistema Dinamico no pasa por aqui: sigue teniendo un unico fondo por frente y nada de este archivo lo toca.
    /// </summary>
    public static class PushBackCellDepth
    {
        /// <summary>El minimo fisico de un fondo Push Back (dos tarimas en el sentido del flujo).</summary>
        public const int MinimumPalletsDeep = 2;

        /// <summary>
        /// La regla de precedencia, desnuda: el override de la celda si lo hay, y si no el fondo por defecto del frente.
        /// Se expone sola para poder fijarla sin construir un sistema entero.
        /// </summary>
        public static int Effective(int? cellOverride, int frontDefault)
        {
            var value = cellOverride ?? frontDefault;
            return value < MinimumPalletsDeep ? MinimumPalletsDeep : value;
        }

        /// <summary>
        /// La ENVOLVENTE de un frente: el mayor fondo efectivo de sus niveles ACTIVOS. Un frente en blanco (I-33) no
        /// aporta ningun nivel, asi que su envolvente es su propio fondo por defecto — su estructura sigue existiendo y
        /// sigue desplazando a los frentes de atras, exactamente como antes.
        /// </summary>
        public static int Envelope(int frontDefault, IReadOnlyList<int?> overrides, int activeLevels)
        {
            var envelope = frontDefault < MinimumPalletsDeep ? MinimumPalletsDeep : frontDefault;
            if (activeLevels <= 0 || overrides == null)
            {
                return envelope;
            }

            for (var level = 0; level < activeLevels; level++)
            {
                var cell = level < overrides.Count ? overrides[level] : null;
                envelope = Math.Max(envelope, Effective(cell, frontDefault));
            }

            return envelope;
        }

        /// <summary>
        /// La ultima POSICION longitudinal (1-based, en la reticula compartida de modulos) que ocupa una celda con
        /// <paramref name="effectiveDeep"/> fondos en un frente que empieza en su <c>DepthStartPosition</c>. Se acota a
        /// la ultima posicion del frente: la celda no puede rebasar la estructura que la sostiene.
        /// </summary>
        public static int EndPosition(DynamicRackFront front, int effectiveDeep)
        {
            if (front == null)
            {
                return Math.Max(MinimumPalletsDeep, effectiveDeep);
            }

            var start = Math.Max(1, front.DepthStartPosition);
            var frontEnd = start + Math.Max(MinimumPalletsDeep, front.PalletsDeep) - 1;
            var end = start + Math.Max(MinimumPalletsDeep, effectiveDeep) - 1;
            return Math.Min(end, frontEnd);
        }

        /// <summary>
        /// El fondo EFECTIVO ya resuelto de una celda, leido del sistema. <paramref name="levelNumber"/> es 1-based
        /// (como en las colocaciones y en los ejes de cama). Un frente nulo responde por el rack entero, que es la
        /// referencia del lateral no seccionado.
        /// </summary>
        public static int Effective(PushBackSystem system, DynamicRackFront front, int levelNumber)
        {
            if (system == null)
            {
                return MinimumPalletsDeep;
            }

            if (front == null)
            {
                return Math.Max(MinimumPalletsDeep, system.Structure?.PalletsDeep ?? MinimumPalletsDeep);
            }

            return system.EffectivePalletsDeepAt(FrontIndexOf(system, front), levelNumber - 1);
        }

        /// <summary>
        /// La X de mundo donde se apoya el larguero POSTERIOR de una celda: el <c>EndX</c> del modulo que cierra su
        /// ultima posicion. Sin modulo (estructura sin resolver) cae en el <c>EndX</c> del frente, que es lo que todas
        /// las vistas usaban antes de I-41 — por eso un rack sin overrides dibuja exactamente igual.
        /// </summary>
        public static double RearX(PushBackSystem system, DynamicRackFront front, int levelNumber)
        {
            var structure = system?.Structure;
            if (structure == null || front == null)
            {
                return front?.EndX ?? structure?.TotalLength ?? 0.0;
            }

            var position = EndPosition(front, Effective(system, front, levelNumber));
            var module = structure.Modules.FirstOrDefault(m => m != null && m.Index + 1 == position);
            return module?.EndX ?? front.EndX;
        }

        /// <summary>
        /// La longitud COMERCIAL de la cama de una celda: su tramo longitudinal completo, desde el arranque del frente
        /// hasta la X de su larguero posterior. Es la misma magnitud que
        /// <see cref="PushBackFlowBedLateralBuilder.ResolveBedLength(PushBackSystem, DynamicRackFront)"/> devolvia para
        /// el frente entero —sin el descuento de 4", que es una regla del Dinamico— pero medida por celda.
        /// </summary>
        public static double BedLength(PushBackSystem system, DynamicRackFront front, int levelNumber)
        {
            if (system == null)
            {
                return 0.0;
            }

            if (front == null)
            {
                return PushBackFlowBedLateralBuilder.ResolveBedLength(system, null);
            }

            var span = RearX(system, front, levelNumber) - front.StartX;
            return span > 0.0 ? span : 0.0;
        }

        /// <summary>El indice del frente dentro de la estructura, por identidad y con respaldo en <c>Index</c>.</summary>
        internal static int FrontIndexOf(PushBackSystem system, DynamicRackFront front)
        {
            var fronts = system?.Structure?.Fronts;
            if (fronts == null || front == null)
            {
                return -1;
            }

            for (var index = 0; index < fronts.Count; index++)
            {
                if (ReferenceEquals(fronts[index], front))
                {
                    return index;
                }
            }

            // El corte lateral agrupa frentes por (StartX, EndX, LoadLevels) y puede entregar una instancia que no es la
            // de la lista; Index es el respaldo con el que el resto del codigo ya la identifica.
            return front.Index >= 0 && front.Index < fronts.Count ? front.Index : -1;
        }

        /// <summary>
        /// Los fondos efectivos DISTINTOS de un frente, en orden creciente, con los niveles que los comparten. Es lo que
        /// permite a la cama conservar el patron ARRAY (una definicion anidada compartida por varios niveles) cuando los
        /// fondos coinciden — el caso de todo rack anterior a I-41, que asi sigue produciendo UN solo grupo.
        /// </summary>
        public static IReadOnlyList<IGrouping<int, int>> LevelsByDepth(
            PushBackSystem system, DynamicRackFront front, IEnumerable<int> levelNumbers)
            => (levelNumbers ?? Enumerable.Empty<int>())
                .GroupBy(level => Effective(system, front, level))
                .OrderBy(group => group.Key)
                .ToList();
    }
}
