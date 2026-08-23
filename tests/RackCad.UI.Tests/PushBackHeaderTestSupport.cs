using System.Linq;
using System.Windows.Controls;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.RackFrames;
using RackCad.UI.Systems.PushBack;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-40 (decision final del Owner): la unidad de edicion de una cabecera es la INSTANCIA FISICA, identificada
    /// por <c>(PostIndex, ModuleId)</c>. Estos helpers leen esa instancia, que es lo que el usuario ve dibujado, en
    /// vez de la configuracion del modulo — que ya solo es el valor por defecto de las lineas sin el suyo.
    /// </summary>
    internal static class PushBackHeaderTestSupport
    {
        /// <summary>The physical LINE the window currently addresses (the one the line selector shows).</summary>
        public static int SelectedLine(RackPushBackSystemWindow w)
        {
            var box = (ComboBox)w.FindName("HeaderLineBox");
            var structure = w.EditorStateForTest.WorkingBaseline?.Structure;
            if (box == null || structure == null || box.SelectedIndex < 0)
            {
                return 0;
            }

            var lines = DynamicFrontActivation.PresentBoundaries(structure);
            return box.SelectedIndex < lines.Count ? lines[box.SelectedIndex] : 0;
        }

        /// <summary>What the session has STAGED for that physical cabecera: its own configuration when the line has
        /// one, otherwise the module's.</summary>
        public static RackFrameConfiguration Staged(RackPushBackSystemWindow w, string moduleId)
            => Staged(w, moduleId, SelectedLine(w));

        public static RackFrameConfiguration Staged(RackPushBackSystemWindow w, string moduleId, int line)
            => w.EditorStateForTest.ModuleSession.HeaderConfigurationCopy(moduleId, line);

        /// <summary>What that physical cabecera actually DRAWS, read through the single authority that geometry,
        /// BOM and preview all consume.</summary>
        public static RackFrameConfiguration Drawn(RackPushBackSystemWindow w, string moduleId)
            => Drawn(w, moduleId, SelectedLine(w));

        public static RackFrameConfiguration Drawn(RackPushBackSystemWindow w, string moduleId, int line)
        {
            var structure = w.EditorStateForTest.WorkingBaseline.Structure;
            var module = structure.Modules.First(m => m.ModuleId == moduleId);
            return DynamicFrontGeometry.HeaderConfigurationAtPost(structure, module, null, line);
        }

        /// <summary>True when that physical cabecera carries the user's own configuration — either because its line
        /// has one, or because the module itself is custom.</summary>
        public static bool IsCustom(RackPushBackSystemWindow w, string moduleId)
            => IsCustom(w, moduleId, SelectedLine(w));

        public static bool IsCustom(RackPushBackSystemWindow w, string moduleId, int line)
            => w.EditorStateForTest.ModuleSession.HasLineOverride(moduleId, line)
               || w.EditorStateForTest.ModuleSession.Modules
                   .First(m => m.ModuleId == moduleId).HasCustomHeaderConfiguration;
    }
}
