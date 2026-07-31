using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>Lo que una acción del editor hizo, o por qué no lo hizo.</summary>
    public readonly struct CantileverPanelEditResult
    {
        private CantileverPanelEditResult(bool applied, string reason, bool replacesManualWork)
        {
            Applied = applied;
            Reason = reason;
            ReplacesManualWork = replacesManualWork;
        }

        public bool Applied { get; }

        /// <summary>Por qué no se aplicó, o qué hay que saber de lo que se aplicó. Nunca vacío al rechazar.</summary>
        public string Reason { get; }

        /// <summary>
        /// La acción DESCARTA trabajo manual y hay que avisar antes.
        ///
        /// Sólo lo pone <see cref="CantileverPanelLayoutEditorState.RestoreAutomatic"/>. Existe como bandera, y
        /// no como un cuadro de diálogo dentro de esta capa, porque esta capa no sabe de ventanas: quien pinta
        /// decide cómo preguntar, pero no puede decidir si hay algo que preguntar.
        /// </summary>
        public bool ReplacesManualWork { get; }

        public static CantileverPanelEditResult Ok() => new CantileverPanelEditResult(true, null, false);

        public static CantileverPanelEditResult Warned(string reason) =>
            new CantileverPanelEditResult(true, reason, true);

        public static CantileverPanelEditResult Rejected(string reason) =>
            new CantileverPanelEditResult(false, reason, false);
    }

    /// <summary>
    /// THE authority sobre las ACCIONES del editor avanzado de paneles.
    ///
    /// <para>Vive en Application y no en la ventana, por la misma razón que el estado de los editores
    /// selectivo y dinámico (I-20, I-21): una acción que sólo existe dentro de un <c>Window</c> no se puede
    /// probar sin abrir WPF, y la primera vez que alguien necesita la misma acción desde otro sitio acaba
    /// copiada. La ventana COORDINA — pinta la tabla, recoge el clic — y esto decide.</para>
    ///
    /// <para><b>Nada de esto valida la lista.</b> Validar es de
    /// <see cref="CantileverPanelLayoutResolver.Validate"/>, y se hace al resolver, sobre la lista completa y
    /// contra la altura de columna. Aquí se rechaza sólo lo que la acción misma no puede hacer —unir dos
    /// tramos que no se tocan, mover el primero hacia arriba— para que el editor pueda dejar una lista
    /// temporalmente incompleta mientras el usuario la construye, y verla en rojo, en vez de impedirle
    /// escribirla.</para>
    /// </summary>
    public sealed class CantileverPanelLayoutEditorState
    {
        private readonly List<CantileverPanelSegmentDesign> segments;

        public CantileverPanelLayoutEditorState(
            CantileverPanelLayoutMode mode, IEnumerable<CantileverPanelSegmentDesign> initial)
        {
            Mode = mode;
            segments = (initial ?? Enumerable.Empty<CantileverPanelSegmentDesign>())
                .Where(s => s != null)
                .Select(s => s.DeepCopy())
                .ToList();
        }

        public CantileverPanelLayoutMode Mode { get; private set; }

        /// <summary>Los tramos, de abajo arriba. Copias: nadie edita la lista por detrás.</summary>
        public IReadOnlyList<CantileverPanelSegmentDesign> Segments => segments;

        public bool IsAdvanced => Mode == CantileverPanelLayoutMode.Advanced;

        /// <summary>
        /// Pasa a AVANZADO materializando la secuencia que la regla produce ahora mismo.
        ///
        /// El usuario empieza a editar la lista que ya estaba viendo, no una en blanco: cambiar de modo no es
        /// una petición de rehacer el trabajo, es una petición de poder tocarlo.
        /// </summary>
        public CantileverPanelEditResult MaterializeAutomatic(
            IReadOnlyList<CantileverPanelSegmentDesign> automatic)
        {
            if (automatic == null || automatic.Count == 0)
            {
                return CantileverPanelEditResult.Rejected(
                    "La secuencia automatica no se pudo resolver, asi que no hay nada que materializar. " +
                    "Corrige primero lo que la bloquea.");
            }

            segments.Clear();
            segments.AddRange(automatic.Select(s => s.DeepCopy()));
            Mode = CantileverPanelLayoutMode.Advanced;

            return CantileverPanelEditResult.Ok();
        }

        /// <summary>
        /// Vuelve a AUTOMÁTICO, avisando de que la lista manual deja de mandar.
        ///
        /// La lista NO se borra: se conserva como dato dormido, para que volver a avanzado no cueste rehacerla.
        /// Lo que se devuelve es la autoridad, no el contenido.
        /// </summary>
        public CantileverPanelEditResult RestoreAutomatic()
        {
            if (Mode == CantileverPanelLayoutMode.Automatic)
            {
                return CantileverPanelEditResult.Rejected("La secuencia ya la gobierna la regla estandar.");
            }

            Mode = CantileverPanelLayoutMode.Automatic;

            return CantileverPanelEditResult.Warned(
                "La secuencia vuelve a la regla estandar y los " + segments.Count +
                " tramos editados dejan de mandar. Se conservan por si vuelves a avanzado, pero el dibujo y el " +
                "BOM pasan a salir de la regla.");
        }

        /// <summary>
        /// Añade un tramo ENCIMA del último, con la altura del panel declarado.
        ///
        /// Encima y no al final de una lista cualquiera: la secuencia sube, y un tramo nuevo que no continuara
        /// al anterior nacería con un hueco implícito que la validación rechazaría acto seguido.
        /// </summary>
        public CantileverPanelEditResult Add(double height, CantileverPanelBracingMode mode)
        {
            if (!(height > 0.0) || double.IsInfinity(height))
            {
                return CantileverPanelEditResult.Rejected(
                    "Un tramo necesita una altura positiva; se pidio " + Format(height) + " in.");
            }

            var start = segments.Count == 0 ? 0.0 : segments[segments.Count - 1].EndElevation;

            segments.Add(new CantileverPanelSegmentDesign
            {
                StartElevation = start,
                EndElevation = start + height,
                BracingMode = mode
            });

            return CantileverPanelEditResult.Ok();
        }

        /// <summary>
        /// Quita un tramo y CIERRA el vacío bajando todo lo que había encima.
        ///
        /// Cerrar es parte de la acción, no un arreglo posterior. Quitar sin cerrar dejaría un hueco implícito
        /// justo donde el usuario acaba de decir que no quiere nada, y le tocaría a él corregir cotas que no ha
        /// pedido tocar.
        /// </summary>
        public CantileverPanelEditResult Remove(int index)
        {
            if (!InRange(index))
            {
                return CantileverPanelEditResult.Rejected(OutOfRange(index));
            }

            if (segments.Count == 1)
            {
                return CantileverPanelEditResult.Rejected(
                    "Es el unico tramo. Una secuencia sin tramos no es un arriostramiento: si lo que quieres " +
                    "es no arriostrar, apaga sus tensores.");
            }

            var height = segments[index].Height;
            segments.RemoveAt(index);

            for (var i = index; i < segments.Count; i++)
            {
                segments[i].StartElevation -= height;
                segments[i].EndElevation -= height;
            }

            return CantileverPanelEditResult.Ok();
        }

        /// <summary>
        /// Intercambia un tramo con su vecino CONSERVANDO las cotas de la secuencia.
        ///
        /// Mover no mueve cotas: mueve CONTENIDO. Los dos tramos se quedan donde estaban y lo que viaja son sus
        /// alturas y su arriostramiento, así que la secuencia sigue siendo contigua por construcción y no hace
        /// falta recolocar nada después.
        /// </summary>
        public CantileverPanelEditResult Move(int index, int delta)
        {
            if (!InRange(index))
            {
                return CantileverPanelEditResult.Rejected(OutOfRange(index));
            }

            var target = index + delta;

            if (delta != 1 && delta != -1)
            {
                return CantileverPanelEditResult.Rejected("Un tramo se mueve de uno en uno.");
            }

            if (!InRange(target))
            {
                return CantileverPanelEditResult.Rejected(
                    delta < 0
                        ? "El tramo " + (index + 1) + " ya es el de mas abajo."
                        : "El tramo " + (index + 1) + " ya es el de mas arriba.");
            }

            var a = segments[Math.Min(index, target)];
            var b = segments[Math.Max(index, target)];

            var bottom = a.StartElevation;
            var heightOfB = b.Height;

            var modeA = a.BracingMode;
            var modeB = b.BracingMode;

            // B pasa a ocupar la parte de abajo y A la de arriba; el techo del par no se mueve.
            a.StartElevation = bottom;
            a.EndElevation = bottom + heightOfB;
            a.BracingMode = modeB;

            b.StartElevation = a.EndElevation;
            b.BracingMode = modeA;

            return CantileverPanelEditResult.Ok();
        }

        /// <summary>
        /// Parte un tramo en dos por su mitad, heredando los dos el arriostramiento del original.
        /// </summary>
        public CantileverPanelEditResult Split(int index)
        {
            if (!InRange(index))
            {
                return CantileverPanelEditResult.Rejected(OutOfRange(index));
            }

            var segment = segments[index];
            var middle = segment.StartElevation + (segment.Height / 2.0);

            if (!(segment.Height > 0.0))
            {
                return CantileverPanelEditResult.Rejected(
                    "El tramo " + (index + 1) + " no tiene altura que partir.");
            }

            var upper = new CantileverPanelSegmentDesign
            {
                StartElevation = middle,
                EndElevation = segment.EndElevation,
                BracingMode = segment.BracingMode
            };

            segment.EndElevation = middle;
            segments.Insert(index + 1, upper);

            return CantileverPanelEditResult.Ok();
        }

        /// <summary>
        /// Une un tramo con el de encima, SI se tocan y SI llevan lo mismo dentro.
        ///
        /// Lo segundo es la parte que importa: unir un tramo arriostrado con un hueco tendría que decidir qué
        /// queda, y esa decisión es del usuario. Que la tome apagando o encendiendo los tensores primero.
        /// </summary>
        public CantileverPanelEditResult MergeWithNext(int index)
        {
            if (!InRange(index))
            {
                return CantileverPanelEditResult.Rejected(OutOfRange(index));
            }

            if (index + 1 >= segments.Count)
            {
                return CantileverPanelEditResult.Rejected(
                    "El tramo " + (index + 1) + " es el de mas arriba: no tiene con quien unirse.");
            }

            var lower = segments[index];
            var upper = segments[index + 1];

            if (Math.Abs(upper.StartElevation - lower.EndElevation) >
                CantileverPanelLayoutResolver.JoinTolerance)
            {
                return CantileverPanelEditResult.Rejected(
                    "Los tramos " + (index + 1) + " y " + (index + 2) + " no se tocan, asi que unirlos " +
                    "inventaria una cota. Corrige antes la continuidad.");
            }

            if (lower.BracingMode != upper.BracingMode)
            {
                return CantileverPanelEditResult.Rejected(
                    "El tramo " + (index + 1) + " y el " + (index + 2) + " no llevan lo mismo dentro: uno " +
                    "tiene tensores y el otro no. Decide primero cual de los dos gana.");
            }

            lower.EndElevation = upper.EndElevation;
            segments.RemoveAt(index + 1);

            return CantileverPanelEditResult.Ok();
        }

        /// <summary>Enciende o apaga los tensores de un tramo. Un hueco es un tramo con ellos apagados.</summary>
        public CantileverPanelEditResult ToggleBracing(int index)
        {
            if (!InRange(index))
            {
                return CantileverPanelEditResult.Rejected(OutOfRange(index));
            }

            segments[index].BracingMode =
                segments[index].BracingMode == CantileverPanelBracingMode.CrossBraced
                    ? CantileverPanelBracingMode.None
                    : CantileverPanelBracingMode.CrossBraced;

            return CantileverPanelEditResult.Ok();
        }

        /// <summary>Escribe modo y tramos en el diseño. Copias, para que el editor siga siendo dueño de los suyos.</summary>
        public void ApplyTo(CantileverBracingDesign bracing)
        {
            if (bracing == null)
            {
                throw new ArgumentNullException(nameof(bracing));
            }

            bracing.PanelLayoutMode = Mode;
            bracing.AdvancedPanelSegments = segments.Select(s => s.DeepCopy()).ToList();
        }

        private bool InRange(int index) => index >= 0 && index < segments.Count;

        private string OutOfRange(int index) =>
            "No hay tramo numero " + (index + 1) + ": la secuencia tiene " + segments.Count + ".";

        private static string Format(double value) =>
            value.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
