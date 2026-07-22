using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Drawing;

namespace RackCad.Plugin
{
    /// <summary>
    /// Units guardrail at the AutoCAD boundary (initiative I-05, audit finding D4). RackCad's builders emit ALL
    /// geometry in INCHES; when the active drawing's <c>INSUNITS</c> is not inches, a freshly inserted rack lands at
    /// inch scale and looks wrong against the drawing (for example 25.4x on a millimetre plan). This helper reads
    /// <c>INSUNITS</c> and, when it is not inches, writes ONE non-blocking advisory to the command line BEFORE the
    /// first block of an insertion is drawn.
    ///
    /// It NEVER converts, rescales or reinterprets geometry and NEVER aborts a command (ADR-0005): it only reads a
    /// system variable and, at most, prints a message. It is the SINGLE place in RackCad that reads <c>INSUNITS</c>;
    /// the warn/no-warn decision is the pure <see cref="DrawingUnitsAdvisory"/> in Application (which carries no
    /// AutoCAD dependency), so the AutoCAD-specific knowledge —the <see cref="UnitsValue"/> mapping and the read— stays
    /// confined here.
    /// </summary>
    internal static class RackUnitsGuard
    {
        // Sin acentos: mensaje de linea de comandos (evita mojibake en consolas no-Unicode), como el resto del Plugin.
        private const string Advisory =
            "RackCad: aviso de unidades - el dibujo no esta en PULGADAS (INSUNITS). RackCad dibuja en pulgadas; el rack "
            + "se insertara a escala de pulgadas y puede verse desproporcionado frente al dibujo. No se convierte ni se "
            + "reescala nada; ajusta INSUNITS o el dibujo si hace falta.";

        /// <summary>
        /// Writes the units advisory ONCE when the active drawing is not in inches. Call at each authorized insertion
        /// path, before the first DWG modification; a pure in-place update (RACKEDITAR "Actualizar") must NOT call it.
        /// Inches (or a null document/database) produce no message. Non-blocking: it never throws and never stops the
        /// command.
        /// </summary>
        internal static void WarnIfNotInches(Document document)
        {
            if (document?.Database == null)
            {
                return;
            }

            if (DrawingUnitsAdvisory.RequiresInsertionAdvisory(Classify(document.Database.Insunits)))
            {
                document.Editor.WriteMessage("\n" + Advisory);
            }
        }

        /// <summary>
        /// Maps AutoCAD's <c>INSUNITS</c> value to the neutral category the pure policy consumes. Inches is RackCad's
        /// authoring unit; an undefined (unitless) drawing is deliberately treated as non-inches; every other unit is
        /// <see cref="DrawingUnits.Other"/>. This is the ONLY AutoCAD-units knowledge in the guard.
        /// </summary>
        internal static DrawingUnits Classify(UnitsValue insunits)
        {
            switch (insunits)
            {
                case UnitsValue.Inches:
                    return DrawingUnits.Inches;
                case UnitsValue.Undefined:
                    return DrawingUnits.Unitless;
                default:
                    return DrawingUnits.Other;
            }
        }
    }
}
