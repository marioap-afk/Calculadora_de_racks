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
    /// The single source of truth for the command/alias reference shown by RACKAYUDA and by the page «Órdenes y atajos» of the
    /// RackCad window (and mirrored in docs/guias/despliegue.md). Each command class carries its own aliases (for example
    /// RackAyudaCommands); when a command or its alias changes, update this list. An empty alias is a command without one.
    /// </summary>
    public static class RackCommandReference
    {
        public static readonly IReadOnlyList<RackCommandInfo> Commands = new[]
        {
            // I-64 A-14, fourth batch, part «ut» (UT-11): RACKCAD says what it does today; RACKMENU, RACKPUSHBACK, RACKSECCION and
            // RACKVARIABLES, registered and missing until now, are added. RACKMENU and RACKSECCION have no alias (empty).
            new RackCommandInfo("Diseñar", "RACKCAD", "RK", "Ventana RackCad (no bloquea AutoCAD): secciones y lista de racks del dibujo."),
            new RackCommandInfo("Diseñar", "RACKMENU", "", "Menú clásico (elige el tipo de rack a diseñar), mientras las secciones de la ventana se migran."),
            new RackCommandInfo("Diseñar", "RACKSELECTIVO", "RS", "Editor de rack selectivo (matriz frentes × niveles)."),
            new RackCommandInfo("Diseñar", "RACKCABECERA", "RCB", "Cabecera (marco: postes + placas + celosía). RACKCABECERA y RCB abren «Nuevo» › «Cabecera en esta ventana» en la ventana RackCad, que la edita sin bloquear AutoCAD; el configurador clásico sigue en el menú clásico (RACKMENU, «Diseñar cabecera»)."), // T7 cab
            new RackCommandInfo("Diseñar", "QUICKCABECERA", "QCB", "Cabecera por línea de comandos (pide poste/fondo/alto)."),
            new RackCommandInfo("Diseñar", "RACKSISTEMADINAMICO", "RSD", "Sistema dinámico (pallet flow)."),
            new RackCommandInfo("Diseñar", "QUICKCAMA", "QCM", "Cama de rodamiento (riel + rodillos + frenos)."),
            new RackCommandInfo("Diseñar", "RACKCANTILEVER", "RCT", "Línea Cantilever: estaciones, brazos por nivel y lado, arriostramiento; vistas frontal, lateral y planta."),
            new RackCommandInfo("Diseñar", "RACKPUSHBACK", "RPB", "Sistema Push Back. LIFO: carga y descarga por el mismo extremo (bajo); vistas lateral, frontal y planta."),
            new RackCommandInfo("Diseñar", "RACKSECCION", "", "Perfil estructural: consulta perfiles W, HSS, C, L y S del catálogo AISC, visualiza su geometría e insértala en el dibujo. RACKSECCION abre «Nuevo» › «Perfil estructural» en la ventana RackCad, sin bloquear AutoCAD; la ventana clásica sigue en el menú clásico (RACKMENU, «Generar perfil estructural»)."), // T7 per

            new RackCommandInfo("Editar y copiar", "RACKEDITAR", "RED", "Reabrir el editor de un rack dibujado; al confirmar redibuja todas sus vistas y, si lo pides, inserta una o varias vistas nuevas ligadas al mismo rack."),
            new RackCommandInfo("Editar y copiar", "RACKDUPLICAR", "RD", "Copiar un rack como INDEPENDIENTE, estilo COPY: punto base → destinos (múltiple por defecto)."),
            new RackCommandInfo("Editar y copiar", "RACKPROPIEDADES", "RPR", "Propiedades personalizadas estables (nombre y valor) de un rack o del Proyecto; lo que no se puede escribir se ve en solo lectura con su motivo, y las vistas de un rack que difieren se pueden unificar. RACKPROPIEDADES y RPR abren la pestaña «Propiedades» del rack en la ventana RackCad (con «Proyecto», «Propiedades del proyecto»); la ventana clásica sigue en el menú clásico (RACKMENU)."), // T6 rl
            // Cuarta tanda (va), VA-10, y (ut), UT-11: RACKVARIABLES (alias RVA) no estaba en la ayuda; en la ventana RackCad es la sección
            // «Variables». Las dos partes la añadieron: al integrarlas queda UNA entrada, con el texto de va.
            new RackCommandInfo("Editar y copiar", "RACKVARIABLES", "RVA", "Variables de proyecto del dibujo (longitudes con nombre, valor o fórmula) y sus referencias rotas. RACKVARIABLES y RVA abren la sección «Variables» de la ventana RackCad, que las edita sin bloquear AutoCAD; la ventana clásica sigue en el menú clásico (RACKMENU)."), // T6 rl
            new RackCommandInfo("Editar y copiar", "RACKPROYECTAR", "RPY", "Proyectar racks ya dibujados a otra clase de vista (Frontal, Lateral o Planta) con orientación Proyectada (por defecto: la referencia gira entera para seguir a su vista origen) o Predeterminada: punto base → destino. Crea vistas enlazadas del mismo rack, no copias: el rack sigue siendo el mismo y el BOM no cambia (para copiar, RACKDUPLICAR)."),

            // Sexta tanda (rl, AL-S14): los atajos abren la página «Almacén» de la ventana RackCad; las órdenes completas, los diálogos clásicos.
            new RackCommandInfo("Almacén (layout)", "RACKLAYOUT", "RLY", "Replica un rack en una rejilla: filas × columnas + pasillos + numeración; back-to-back y verificar encaje. El atajo RLY abre «Almacén» › «Rejilla» en la ventana RackCad, con el rack seleccionado como semilla; RACKLAYOUT abre el diálogo clásico."),
            new RackCommandInfo("Almacén (layout)", "RACKRELLENAR", "RR", "Auto-rellena el área con racks: contorno + columnas en la capa RACKCAD_SITIO; calcula la rejilla máxima que cabe. El atajo RR abre «Almacén» › «Rellenar un área» en la ventana RackCad, con el rack seleccionado como semilla; RACKRELLENAR abre el diálogo clásico."),

            new RackCommandInfo("Reportes", "RACKLISTA", "RL", "Tabla de todos los racks del dibujo (nombre, tipo, vistas, nº de copias) con zoom."),
            new RackCommandInfo("Reportes", "RACKBOMTOTAL", "RB", "BOM consolidado de TODO el dibujo (por rack × copias + gran total por componente)."),

            new RackCommandInfo("Ayuda", "RACKAYUDA", "RA", "Esta lista de órdenes y atajos. Hoy abre un diálogo aparte, que bloquea AutoCAD hasta la lista de la fase 9."),
        };
    }
}
