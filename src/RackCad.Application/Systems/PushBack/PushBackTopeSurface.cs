using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 (ERROR 10) — la lectura NEUTRAL del tope posterior de una celda: que puede materializarse, que queda
    /// como intencion dormante y donde aterriza la pieza.
    ///
    /// <para>
    /// Vive en Application y no en la ventana a proposito: es la MISMA respuesta que consumen la UI, sus pruebas y
    /// las de nucleo. Una superficie que se calculara dentro del code-behind seria una segunda autoridad, y este
    /// error nacio precisamente de tener dos.
    /// </para>
    /// </summary>
    public readonly struct PushBackTopeSurface
    {
        public PushBackTopeSurface(
            PushBackCellTopology topology,
            PushBackRunDirection direction,
            bool appliesToA,
            bool appliesToB,
            bool atInterface)
        {
            Topology = topology;
            Direction = direction;
            AppliesToA = appliesToA;
            AppliesToB = appliesToB;
            AtInterface = atInterface;
        }

        /// <summary>La topologia que la celda tiene REALMENTE con los lados que existen en ella.</summary>
        public PushBackCellTopology Topology { get; }

        /// <summary>El sentido efectivo, que solo significa algo en una corrida.</summary>
        public PushBackRunDirection Direction { get; }

        /// <summary>Si el lado A tiene un extremo alto en esta celda, y por tanto puede llevar tope.</summary>
        public bool AppliesToA { get; }

        /// <summary>Si el lado B lo tiene.</summary>
        public bool AppliesToB { get; }

        /// <summary>Si la celda existe en algun lado. Cuando no, no hay nada que decidir.</summary>
        public bool Exists => AppliesToA || AppliesToB;

        /// <summary>
        /// Donde queda el extremo alto: en la linea INTERIOR (la del centro del rack) o en la EXTERIOR del lado
        /// alto. Es lo que permite predecir la planta: las camas de un lado y las encontradas topan en el centro;
        /// una corrida topa al final de su recorrido, en la orilla opuesta a su pasillo de carga.
        /// </summary>
        public bool AtInterface { get; }

        /// <summary>Si esta celda admite DOS topes independientes (camas encontradas).</summary>
        public bool IsIndependentPair => AppliesToA && AppliesToB;

        /// <summary>Si la intencion guardada en un lado es efectiva hoy.</summary>
        public bool AppliesTo(PushBackSide side)
            => side == PushBackSide.A ? AppliesToA : AppliesToB;
    }
}
