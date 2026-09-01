using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — LA autoridad de la capacidad geometrica de una cama. Separa DOS magnitudes que pertenecen a
    /// AUTORIDADES DISTINTAS, y esa separacion es el contrato:
    ///
    /// <list type="bullet">
    /// <item><b>RequiredBedLength</b> — lo que exige la DEMANDA de almacenamiento: cuanta longitud necesitan esos
    /// fondos con la receta normal. Se mide sobre los modulos que ALOJAN TARIMA y <b>solo</b> sobre ellos. No
    /// depende del hueco, ni de la longitud total del rack, ni de la estructura sobrante, ni del ajuste manual
    /// vigente.</item>
    /// <item><b>AvailableBedSpan</b> — lo que ofrece la ESTRUCTURA fisica efectiva: el tramo realmente utilizable
    /// por esa cama. El HUECO pertenece a la estructura, asi que suma longitud disponible.</item>
    /// </list>
    ///
    /// <para>
    /// I-42 (A3-S1) — LA VALIDEZ SE MIDE EN ALMACENAMIENTO, NO EN DISTANCIA. El hueco NO es una posicion de tarima,
    /// NO suma un fondo ficticio y NO aumenta la demanda; tampoco puede SUSTITUIR una posicion que no existe. Una
    /// cama cabe cuando hay un apoyo que recorre suficientes posiciones de ALMACENAMIENTO
    /// (<see cref="ResolveSpan"/>), y lo que el hueco aporta es longitud fisica: alarga la distancia entre el bajo
    /// y ese apoyo, no la capacidad de almacenar.
    /// </para>
    /// <para>
    /// Sumar el hueco a la demanda las volveria a acoplar; compararlo contra la demanda —que es lo que hacia la
    /// colocacion— dejaba que un hueco grande pagara fondos inexistentes. Las dos cosas son el defecto que esta
    /// autoridad existe para impedir, y por eso lleva DOS cuentas separadas.
    /// </para>
    /// <para>
    /// No se trunca la cama, no se aumentan los fondos y no se inventa estructura: una celda que no cabe se declara
    /// imposible y se dice por que. Y no se codifica ninguna suma de fondos concreta.
    /// </para>
    /// </summary>
    /// <summary>El extremo por el que una cama esta anclada, y desde el que se mide su demanda.</summary>
    public enum PushBackBedAnchor
    {
        /// <summary>El extremo EXTERIOR de su lado (el pasillo). Es el ancla de una cama de un solo lado.</summary>
        Outer = 0,

        /// <summary>El extremo ALTO de la secuencia. Es el ancla de una cama CORRIDA.</summary>
        High = 1
    }

    public static class PushBackBedSpan
    {
        /// <summary>Tolerancia de comparacion (in). Por debajo de una milesima de pulgada no hay diferencia fisica.</summary>
        public const double Tolerance = 1e-3;

        /// <summary>
        /// La longitud de las <paramref name="positions"/> PRIMERAS posiciones de una sub-estructura, medida sobre
        /// sus modulos reales. Una demanda que rebasa la secuencia se extrapola con la profundidad de tarima: es la
        /// unica forma de medir cuanto le FALTA a una celda que no cabe sin inventarle estructura.
        /// </summary>
        public static double SpanOfFirstPositions(DynamicRackSystem local, int positions)
        {
            if (local == null || positions <= 0)
            {
                return 0.0;
            }

            var modules = local.Modules.ToList();
            var span = 0.0;
            for (var index = 0; index < Math.Min(positions, modules.Count); index++)
            {
                span += modules[index].Length;
            }

            if (positions > modules.Count)
            {
                var palletDepth = Math.Max(0.0, local.Pallet?.Depth ?? 0.0);
                span += (positions - modules.Count) * palletDepth;
            }

            return span;
        }

        /// <summary>
        /// La longitud EXIGIDA por la demanda de una celda de un lado: sus fondos efectivos medidos desde el extremo
        /// exterior de ese lado, que es donde esa cama esta anclada. Delega en <see cref="DemandLength"/>: la regla
        /// de la demanda vive en UN solo sitio y solo cambia por que extremo se recorre.
        /// </summary>
        public static double Required(DynamicRackSystem local, int effectiveDeep)
            => DemandLength(local, effectiveDeep, PushBackBedAnchor.Outer);

        /// <summary>
        /// La longitud DISPONIBLE de una ranura en un lado: lo que da su estructura efectiva desde el extremo
        /// exterior. Es exactamente el tramo del frente proyectado, asi que la cama nunca se compara contra una
        /// longitud que la estructura no tenga.
        /// </summary>
        public static double Available(DynamicRackSystem local, int slotStructure)
            => SpanOfFirstPositions(local, slotStructure);

        /// <summary>
        /// LA regla de la DEMANDA: cuanta longitud exigen <paramref name="positions"/> fondos sobre una secuencia,
        /// recorrida desde el extremo en el que esa cama esta ANCLADA.
        ///
        /// <para>
        /// Suma unicamente la longitud de los modulos que ALOJAN TARIMA. El HUECO se atraviesa pero <b>no</b> se
        /// suma: pertenece a la ESTRUCTURA, no a lo que la carga necesita. Gracias a eso cambiar el hueco no mueve
        /// esta longitud, y un hueco positivo puede volver valida una cama aumentando solo lo DISPONIBLE.
        /// </para>
        /// <para>
        /// Una demanda que rebasa la secuencia se extrapola con la profundidad de tarima: es la unica forma de decir
        /// CUANTO le falta sin inventarle estructura al rack.
        /// </para>
        /// </summary>
        /// <summary>
        /// I-42 (A3-S1, contrato del dueño) — LO QUE UN MODULO APORTA A LA DEMANDA DE ALMACENAMIENTO: su longitud
        /// si aloja tarima, y CERO si no.
        ///
        /// <para>
        /// El hueco —con separador central o sin el— es ESTRUCTURA: se atraviesa, no se almacena en el. Esta es la
        /// unica funcion que lo dice, y la usan por igual la regla de la demanda
        /// (<see cref="DemandLength"/>) y la de colocacion (<see cref="ResolveSpan"/>), que antes acumulaban cosas
        /// distintas.
        /// </para>
        /// </summary>
        public static double StorageContribution(DynamicRackModule module)
            => module != null && PushBackCompositeStructure.IsStoragePosition(module) ? module.Length : 0.0;

        /// <summary>
        /// I-42 (A3-S1B, contrato del dueño) — LA CAPACIDAD DE ALMACENAMIENTO de un marco: cuantas posiciones que
        /// alojan tarima tiene, sin mas.
        ///
        /// <para>
        /// Es el PRIMERO de los dos ejes de validez de una cama, y el hueco no participa en el: por grande que sea,
        /// no anade una posicion. El segundo es el SPAN FISICO —la distancia hasta un apoyo—, en el que el hueco si
        /// participa. Tenerlos separados es lo que impide las dos confusiones: que el hueco pague fondos que no
        /// existen, y que se le niegue la longitud que si aporta.
        /// </para>
        /// </summary>
        public static int StorageCapacity(DynamicRackSystem frame)
            => frame?.Modules.Count(module => PushBackCompositeStructure.IsStoragePosition(module)) ?? 0;

        public static double DemandLength(DynamicRackSystem frame, int positions, PushBackBedAnchor anchor)
        {
            if (frame == null || positions <= 0)
            {
                return 0.0;
            }

            var modules = frame.Modules.ToList();
            var span = 0.0;
            var covered = 0;
            for (var step = 0; step < modules.Count && covered < positions; step++)
            {
                var module = modules[anchor == PushBackBedAnchor.Outer ? step : modules.Count - 1 - step];
                var contribution = StorageContribution(module);
                if (contribution <= 0.0)
                {
                    continue;   // la interfaz es ESTRUCTURA —con separador o sin el—: se atraviesa, no se exige
                }

                span += contribution;
                covered++;
            }

            if (covered < positions)
            {
                var palletDepth = Math.Max(0.0, frame.Pallet?.Depth ?? 0.0);
                span += (positions - covered) * palletDepth;
            }

            return span;
        }

        /// <summary>
        /// El SPAN FISICO resuelto de una cama: donde apoya realmente, cuanto mide y cuanto ofrece la estructura.
        /// Es la UNICA autoridad de colocacion — nadie recorta ni desplaza despues por su cuenta.
        /// </summary>
        public readonly struct PushBackResolvedSpan
        {
            public PushBackResolvedSpan(
                int demandPositions, double required, double resolved, double available, int endPosition, bool fits,
                double storageAvailable = 0.0)
            {
                DemandPositions = demandPositions;
                RequiredLength = required;
                ResolvedLength = resolved;
                AvailableLength = available;
                EndPosition = endPosition;
                Fits = fits;
                StorageAvailableLength = storageAvailable;
            }

            /// <summary>Los fondos que la cama declara. Es su DEMANDA, y no se deriva de ninguna estructura.</summary>
            public int DemandPositions { get; }

            /// <summary>Longitud MINIMA que exige esa demanda con la regla fisica vigente.</summary>
            public double RequiredLength { get; }

            /// <summary>
            /// Longitud FISICA realmente utilizada: la del primer apoyo valido, desde el ancla ALTA hacia el bajo,
            /// cuya distancia satisface la demanda. Ni una longitud flotante entre dos apoyos, ni todo el espacio
            /// disponible si con menos basta.
            /// </summary>
            public double ResolvedLength { get; }

            /// <summary>Longitud MAXIMA que la estructura efectiva pone a disposicion de esa cama.</summary>
            public double AvailableLength { get; }

            /// <summary>
            /// Posicion (1-based) del modulo donde apoya el extremo ALTO. Es un apoyo real, no un punto.
            ///
            /// <para>
            /// El extremo BAJO no necesita posicion: es SIEMPRE la primera del marco, la linea de postes exterior
            /// del lado por el que la cama se carga. Lo que se mueve al cambiar la demanda es el ALTO.
            /// </para>
            /// </summary>
            public int EndPosition { get; }

            /// <summary>
            /// I-42 (A3-S1) — la longitud de ALMACENAMIENTO que la estructura ofrece de verdad: la suma de sus
            /// posiciones que alojan tarima. El hueco no entra, asi que es la magnitud con la que se compara una
            /// demanda que no cabe: es la que le falta, no la distancia fisica.
            /// </summary>
            public double StorageAvailableLength { get; }

            /// <summary>
            /// True cuando existe un apoyo cuya distancia recorre suficientes POSICIONES DE ALMACENAMIENTO para la
            /// demanda. Un hueco grande no lo vuelve true: aumenta la distancia fisica, no lo almacenable.
            /// </summary>
            public bool Fits { get; }
        }

        /// <summary>
        /// LA autoridad de colocacion de una cama anclada en el extremo BAJO de <paramref name="frame"/>.
        ///
        /// <para>
        /// Resuelve la cadena completa en un solo sitio: la demanda da la longitud MINIMA
        /// (<see cref="DemandLength"/>); despues se recorre la estructura desde el ancla BAJA hacia la alta y se
        /// toma el PRIMER apoyo fisico cuya distancia satisface esa longitud. Los apoyos son las lineas de modulo
        /// —las mismas sobre las que el Push Back de un sentido ya coloca su larguero bajo—, asi que no se inventa
        /// ningun concepto de apoyo nuevo.
        /// </para>
        /// <para>
        /// <b>Decision fisica del dueño (validacion manual):</b> el extremo BAJO de una cama —por donde se carga y
        /// se descarga— queda SIEMPRE anclado al poste exterior de su lado. El que se mueve hacia dentro cuando la
        /// cama pide menos fondo que la estructura disponible es el ALTO. Anclar al reves dejaba el pasillo
        /// inaccesible: la cama arrancaba metida dentro del rack.
        /// </para>
        /// <para>
        /// Cuidado con no mezclar dos preguntas distintas: LONGITUDINALMENTE manda el BAJO (fija el origen), y
        /// VERTICALMENTE manda el ALTO (fija la elevacion y el troquel, I-32). Las dos siguen siendo ciertas.
        /// </para>
        /// <para>
        /// Se cumple siempre <c>Required &lt;= Resolved &lt;= Available</c>: no se trunca la cama, no se la obliga a
        /// medir exactamente el minimo si el siguiente apoyo valido queda mas lejos, y no se la estira hasta todo el
        /// espacio disponible si con menos basta.
        /// </para>
        /// <para>
        /// Cuando ni el apoyo mas lejano alcanza, la cama NO cabe: se devuelve el span completo —el maximo real, para
        /// que el dibujo siga apoyado en la estructura— con <c>Fits = false</c>, y el diagnostico dice cuanto falta.
        /// </para>
        /// </summary>
        public static PushBackResolvedSpan ResolveSpan(DynamicRackSystem frame, int demandPositions)
        {
            var modules = frame?.Modules.ToList() ?? new List<DynamicRackModule>();
            var required = DemandLength(frame, demandPositions, PushBackBedAnchor.Outer);
            if (modules.Count == 0)
            {
                return new PushBackResolvedSpan(demandPositions, required, 0.0, 0.0, 1, required <= Tolerance);
            }

            var low = modules[0].StartX;
            var available = modules[modules.Count - 1].EndX - low;
            var storageAvailable = modules.Sum(StorageContribution);

            // Se recorre desde el ancla BAJA hacia la alta llevando DOS cuentas distintas, porque son dos
            // magnitudes distintas (I-42/A3-S1):
            //
            //   - la ALMACENADA, que es la que se compara contra la demanda: solo suman los modulos que alojan
            //     tarima, con la MISMA contribucion que usa DemandLength;
            //   - la FISICA, que es la distancia real desde el ancla baja hasta ese apoyo, y que si incluye el
            //     hueco porque el hueco es estructura y la cama lo atraviesa.
            //
            // Compararlas entre si era el defecto: con un hueco grande, la distancia fisica alcanzaba la longitud
            // exigida antes de haber cruzado suficientes posiciones de almacenamiento, y el extremo ALTO se
            // resolvia un modulo —o dos— antes de tiempo. Medido con 3 x 54 + hueco 54 + 3 x 54 y demanda 6: el
            // alto caia en el modulo 6, con 258" de almacenamiento cruzado contra 312" exigidos.
            // I-42 (A3-S1B): los DOS ejes, dichos por separado.
            //
            //   CAPACIDAD  — ¿existen tantas posiciones de almacenamiento como fondos pide? El hueco no cuenta.
            //   SPAN FISICO— ¿hay un apoyo cuya distancia satisface la longitud exigida? El hueco si cuenta.
            //
            // Con la definicion vigente de la demanda —la suma de las longitudes de las N primeras posiciones de
            // ESTE marco— el segundo se cumple siempre que se cumple el primero, porque la distancia fisica hasta
            // un apoyo incluye las mismas longitudes mas, quiza, el hueco. La comprobacion no cambia por tanto
            // ningun veredicto (verificado sobre 432 configuraciones de fondos, hueco y demanda); esta escrita para
            // que las dos condiciones sean visibles y para que ninguna futura definicion de la demanda las colapse
            // en silencio.
            var capacity = demandPositions <= StorageCapacity(frame);
            var storage = 0.0;
            for (var index = 0; index < modules.Count; index++)
            {
                storage += StorageContribution(modules[index]);
                var span = modules[index].EndX - low;
                var storageMet = storage >= required - Tolerance;
                var spanMet = span >= required - Tolerance;
                if (capacity && storageMet && spanMet)
                {
                    return new PushBackResolvedSpan(
                        demandPositions, required, span, available, index + 1, true, storageAvailable);
                }
            }

            // Ni el apoyo mas lejano alcanza: la cama no cabe. Se apoya en el extremo y se declara lo que falta.
            return new PushBackResolvedSpan(
                demandPositions, required, available, available, modules.Count, false, storageAvailable);
        }

        /// <summary>True cuando la celda cabe fisicamente. Nunca corrige: solo responde.</summary>
        public static bool Fits(double required, double available) => required <= available + Tolerance;

        /// <summary>
        /// I-42 (A3-S1, contrato del dueño) — EL MOTIVO de una cama COLOCADA con <see cref="ResolveSpan"/>: no cabe
        /// cuando no hay ningun apoyo que recorra suficientes posiciones de ALMACENAMIENTO.
        ///
        /// <para>
        /// Un hueco aumenta la distancia fisica disponible y por eso puede rescatar a una cama a la que solo le
        /// faltaba longitud; lo que no puede es sustituir una posicion de almacenamiento que no existe. Comparar la
        /// demanda contra la distancia fisica —hueco incluido— dejaba pasar exactamente eso: una corrida de 7
        /// fondos sobre 6 posiciones reales se declaraba valida porque el hueco de 108" completaba los 360" que la
        /// demanda pedia. El motivo nombra por eso el almacenamiento que la estructura ofrece, que es lo que falta.
        /// </para>
        /// </summary>
        public static string DisabledReason(
            PushBackResolvedSpan span, PushBackSide side, int frontIndex, int levelNumber)
            => span.Fits
                ? null
                : string.Format(
                    System.Globalization.CultureInfo.CurrentCulture,
                    "Lado {0}, frente {1}, nivel {2}: la cama necesita {3:0.##}\" de almacenamiento y la estructura "
                    + "efectiva solo ofrece {4:0.##}\" ({5:0.##}\" fisicos, hueco incluido). Aumenta la estructura "
                    + "de ese lado o reduce el fondo de la celda.",
                    side == PushBackSide.B ? "B" : "A",
                    frontIndex + 1,
                    levelNumber,
                    span.RequiredLength,
                    span.StorageAvailableLength,
                    span.AvailableLength);

        /// <summary>
        /// El motivo, en el idioma del usuario, por el que una cama no cabe. Null cuando cabe. Declara el LADO, el
        /// frente, el nivel y las dos magnitudes reales, para que el usuario sepa exactamente que cama se queja y
        /// pueda decidir: mas estructura, mas gap o menos fondo.
        /// </summary>
        public static string DisabledReason(
            double required, double available, PushBackSide side, int frontIndex, int levelNumber)
            => Fits(required, available)
                ? null
                : string.Format(
                    System.Globalization.CultureInfo.CurrentCulture,
                    "Lado {0}, frente {1}, nivel {2}: la cama necesita {3:0.##}\" y la estructura efectiva solo "
                    + "ofrece {4:0.##}\". Aumenta la estructura de ese lado, el gap o reduce el fondo de la celda.",
                    side == PushBackSide.B ? "B" : "A",
                    frontIndex + 1,
                    levelNumber,
                    required,
                    available);
    }
}
