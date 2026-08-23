using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        /// <summary>The lines the rack actually has, in the order the destination list offers them.</summary>
        public static IReadOnlyList<int> Lines(RackPushBackSystemWindow w)
        {
            var structure = w.EditorStateForTest.WorkingBaseline?.Structure;
            return structure == null
                ? Array.Empty<int>()
                : DynamicFrontActivation.PresentBoundaries(structure);
        }

        /// <summary>The header module ids, in the order the destination list offers them.</summary>
        public static IReadOnlyList<string> Headers(RackPushBackSystemWindow w)
            => w.EditorStateForTest.ModuleSession.Modules
                .Where(module => module.IsHeader)
                .Select(module => module.ModuleId)
                .ToList();

        /// <summary>The FIRST selected destination line — the one the ORIGIN instance sits on.</summary>
        public static int SelectedLine(RackPushBackSystemWindow w)
        {
            var box = (ListBox)w.FindName("HeaderLinesList");
            var lines = Lines(w);
            if (box == null || box.SelectedItems.Count == 0 || lines.Count == 0)
            {
                return lines.Count > 0 ? lines[0] : 0;
            }

            var labels = box.ItemsSource.Cast<string>().ToList();
            var index = labels.IndexOf((string)box.SelectedItems[0]);
            return index >= 0 && index < lines.Count ? lines[index] : lines[0];
        }

        /// <summary>Point the two destination axes at exactly these cabeceras and these lines (I-40, ronda 5).</summary>
        public static void Target(
            RackPushBackSystemWindow w,
            IEnumerable<string> moduleIds,
            IEnumerable<int> lines)
        {
            var headerBox = (ListBox)w.FindName("HeaderTargetsList");
            var lineBox = (ListBox)w.FindName("HeaderLinesList");
            var headerIds = Headers(w);
            var lineIndexes = Lines(w);

            var headerLabels = headerBox.ItemsSource.Cast<string>().ToList();
            headerBox.SelectedItems.Clear();
            foreach (var id in moduleIds)
            {
                var index = headerIds.ToList().IndexOf(id);
                if (index >= 0 && index < headerLabels.Count)
                {
                    headerBox.SelectedItems.Add(headerLabels[index]);
                }
            }

            var lineLabels = lineBox.ItemsSource.Cast<string>().ToList();
            lineBox.SelectedItems.Clear();
            foreach (var line in lines)
            {
                var index = lineIndexes.ToList().IndexOf(line);
                if (index >= 0 && index < lineLabels.Count)
                {
                    lineBox.SelectedItems.Add(lineLabels[index]);
                }
            }
        }

        /// <summary>«Aplicar configuracion a la seleccion»: the explicit action, with no reconfiguring.</summary>
        public static void ApplyToSelection(RackPushBackSystemWindow w)
            => ((Button)w.FindName("ApplyHeaderSelectionButton"))
                .RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

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
