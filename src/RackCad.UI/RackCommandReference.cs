using System.Collections.Generic;

namespace RackCad.UI
{
    /// <summary>One RackCad command in the in-app reference: its full name, short alias, group and a one-line summary.</summary>
    public sealed class RackCommandInfo
    {
        public RackCommandInfo(string group, string command, string alias, string summary)
        {
            Group = group;
            Command = command;
            Alias = alias;
            Summary = summary;
        }

        public string Group { get; }
        public string Command { get; }
        public string Alias { get; }
        public string Summary { get; }
    }

    /// <summary>
    /// The single source of truth for the command/alias reference shown by RACKAYUDA (and mirrored in
    /// docs/despliegue.md). When a command or its alias changes (see RackFrameCommands.Aliases.cs), update this list.
    /// </summary>
    public static class RackCommandReference
    {
        public static readonly IReadOnlyList<RackCommandInfo> Commands = new[]
        {
            new RackCommandInfo("Diseñar", "RACKCAD", "RK", "Menú principal — elige el tipo de rack a diseñar."),
            new RackCommandInfo("Diseñar", "RACKSELECTIVO", "RS", "Editor de rack selectivo (matriz frentes × niveles)."),
            new RackCommandInfo("Diseñar", "RACKCABECERA", "RCB", "Configurador de cabecera (marco: postes + placas + celosía)."),
            new RackCommandInfo("Diseñar", "QUICKCABECERA", "QCB", "Cabecera por línea de comandos (pide poste/fondo/alto)."),
            new RackCommandInfo("Diseñar", "RACKSISTEMADINAMICO", "RSD", "Sistema dinámico (pallet flow)."),
            new RackCommandInfo("Diseñar", "QUICKCAMA", "QCM", "Cama de rodamiento (riel + rodillos + frenos)."),

            new RackCommandInfo("Editar y copiar", "RACKEDITAR", "RED", "Reabrir el editor de un rack dibujado; al confirmar redibuja todas sus vistas."),
            new RackCommandInfo("Editar y copiar", "RACKDUPLICAR", "RD", "Copiar un rack como INDEPENDIENTE, estilo COPY: punto base → destinos (múltiple por defecto)."),

            new RackCommandInfo("Almacén (layout)", "RACKLAYOUT", "RLY", "Replica un rack en una rejilla: filas × columnas + pasillos + numeración; back-to-back y verificar encaje."),
            new RackCommandInfo("Almacén (layout)", "RACKRELLENAR", "RR", "Auto-rellena el área con racks: contorno + columnas en la capa RACKCAD_SITIO; calcula la rejilla máxima que cabe."),

            new RackCommandInfo("Reportes", "RACKLISTA", "RL", "Tabla de todos los racks del dibujo (nombre, tipo, vistas, nº de copias) con zoom."),
            new RackCommandInfo("Reportes", "RACKBOMTOTAL", "RB", "BOM consolidado de TODO el dibujo (por rack × copias + gran total por componente)."),

            new RackCommandInfo("Ayuda", "RACKAYUDA", "RA", "Esta ventana de comandos y atajos."),
        };
    }
}
