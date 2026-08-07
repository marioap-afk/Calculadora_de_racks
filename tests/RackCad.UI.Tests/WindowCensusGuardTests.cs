using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RackCad.UI.Shell;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// The census of I-39A (ADR-0029 D1 and D2): every CONCRETE class deriving from <see cref="Window"/> in the
    /// product assembly, with the archetype it belongs to.
    ///
    /// <para>The unit is the TYPE, never the file and never an <c>x:Name</c>. Both alternatives were measured and
    /// both lie: <c>SafetyPerPostWindow</c> is declared inside <c>SelectiveSafetyWindow.cs</c>, so a census by file
    /// name misses it; and ten windows draw on an ordinary <c>Canvas</c> whose <c>x:Name</c> is "PreviewCanvas"
    /// while the TYPE <c>RackCad.UI.Controls.PreviewCanvas</c> has a single consumer, so a census by name would
    /// count eleven consumers where there is one. Reflection over the assembly is the only method that cannot be
    /// fooled by either — it also catches code-only windows and any depth of derivation.</para>
    ///
    /// <para>This guard is the reason the census stays alive: a window added, removed or renamed fails here until
    /// its archetype is declared, which is exactly the classification ADR-0029 D2 makes obligatory.</para>
    /// </summary>
    public class WindowCensusGuardTests
    {
        /// <summary>A — rich system editor: several editing scopes, aggregate structure, main preview, insert or
        /// update in AutoCAD, possible persistence, rack session or identity, complex recompute. The absence of a
        /// matrix does NOT move a window out of this archetype.</summary>
        private static readonly string[] RichEditors =
        {
            "RackSelectiveWindow", "RackDynamicSystemWindow", "RackPushBackSystemWindow",
            "RackCantileverWindow", "RackFlowBedWindow", "RackFrameConfiguratorWindow"
        };

        /// <summary>B — bounded editor with preview: limited functional scope, own parameters, preview, diagnostics
        /// and a bounded result or action. Deliberately NOT called "component editor": its consumers are not
        /// necessarily persistent components nor BOM members (owner decision 6).</summary>
        private static readonly string[] BoundedEditors =
        {
            "CantileverColumnBaseWindow", "CantileverArmWindow", "CantileverSeparatorWindow",
            "CantileverBraceWindow", "RackLargueroWindow", "StructuralSectionInspectorWindow"
        };

        /// <summary>C — transactional configuration dialog: edits a copy or a selection, accepts or cancels, does
        /// not run a whole rack session, may have no preview.</summary>
        private static readonly string[] ConfigurationDialogs =
        {
            "SelectiveSafetyWindow", "SafetyPerPostWindow", "SafetyTopeGridWindow", "SafetyParrillaGridWindow",
            "SafetyGuiaEntradaGridWindow", "SafetyDesviadorGridWindow", "SafetyDefensaGridWindow",
            "SelectiveSegmentsWindow", "RackWarehouseLayoutWindow", "RackWarehouseFillWindow"
        };

        /// <summary>D — utility window: navigation, consultation, help, lists or BOM, with no transactional editing
        /// contract.</summary>
        private static readonly string[] Utilities =
        {
            "RackMainMenuWindow", "RackDesignLibraryWindow", "RackBomWindow", "RackConsolidatedBomWindow",
            "RackListWindow", "RackCommandHelpWindow"
        };

        /// <summary>Infrastructure, NOT product: a shared chrome base, not a window a user opens (owner decision 7).</summary>
        private static readonly string[] Infrastructure = { "RackDialogWindow" };

        private static IReadOnlyList<Type> ConcreteWindows() =>
            typeof(RackEditorVisualShell).Assembly
                .GetTypes()
                .Where(t => typeof(Window).IsAssignableFrom(t) && !t.IsAbstract)
                .OrderBy(t => t.Name, StringComparer.Ordinal)
                .ToList();

        private static IEnumerable<string> Census() =>
            RichEditors.Concat(BoundedEditors).Concat(ConfigurationDialogs).Concat(Utilities).Concat(Infrastructure);

        [Fact]
        public void EveryConcreteWindowTypeIsCensusedWithItsArchetype()
        {
            var found = ConcreteWindows().Select(t => t.Name).ToList();
            var declared = Census().ToList();

            var missing = found.Except(declared, StringComparer.Ordinal).OrderBy(n => n, StringComparer.Ordinal).ToList();
            Assert.True(
                missing.Count == 0,
                "Ventanas sin arquetipo declarado en el censo de I-39A: " + string.Join(", ", missing));
        }

        [Fact]
        public void TheCensusDeclaresNoWindowThatDoesNotExist()
        {
            var found = ConcreteWindows().Select(t => t.Name).ToList();
            var declared = Census().ToList();

            var ghosts = declared.Except(found, StringComparer.Ordinal).OrderBy(n => n, StringComparer.Ordinal).ToList();
            Assert.True(
                ghosts.Count == 0,
                "El censo declara ventanas que ya no existen: " + string.Join(", ", ghosts));
        }

        [Fact]
        public void NoWindowIsClassifiedTwice()
        {
            var duplicated = Census()
                .GroupBy(n => n, StringComparer.Ordinal)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            Assert.True(duplicated.Count == 0, "Ventanas en dos arquetipos: " + string.Join(", ", duplicated));
        }

        [Fact]
        public void TheCensusCatchesAWindowDeclaredInsideAnotherWindowsFile()
        {
            // SafetyPerPostWindow lives inside SelectiveSafetyWindow.cs. It is the reason the unit is the type:
            // a census driven by file names would silently leave it out of every rule.
            var found = ConcreteWindows().Select(t => t.Name).ToList();

            Assert.Contains("SelectiveSafetyWindow", found);
            Assert.Contains("SafetyPerPostWindow", found);
        }

        [Fact]
        public void TheCensusCatchesCodeOnlyWindows()
        {
            // Windows built entirely in C#, with no XAML of their own, are ordinary members of the census.
            var found = ConcreteWindows().Select(t => t.Name).ToList();

            foreach (var codeOnly in new[]
                     {
                         "StructuralSectionInspectorWindow", "RackCommandHelpWindow", "RackWarehouseLayoutWindow",
                         "RackWarehouseFillWindow", "SafetyTopeGridWindow"
                     })
            {
                Assert.Contains(codeOnly, found);
            }
        }

        [Fact]
        public void InfrastructureIsNotCountedAsProduct()
        {
            // RackDialogWindow is shared chrome, not a window a user opens: it is censused, and it is not product.
            Assert.Contains("RackDialogWindow", Infrastructure);
            Assert.DoesNotContain("RackDialogWindow", RichEditors.Concat(BoundedEditors).Concat(ConfigurationDialogs).Concat(Utilities));
        }
    }
}
