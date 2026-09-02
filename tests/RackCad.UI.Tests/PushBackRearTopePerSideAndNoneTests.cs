using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-42 (ronda 7C, defectos 1 y 3 de la Owner Validation) — los TOPES de un Push Back compuesto.
    ///
    /// <para><b>Defecto 1.</b> Un frente EN BLANCO en el lado A salia tambien en blanco en el lado B dentro de
    /// «Elementos de seguridad», aunque B existiera fisicamente ahi. La causa medida: las DOS secciones abrian su
    /// rejilla con los niveles del lado ACTIVO. Un blanco es del LADO —igual que la celda, el nivel y la corrida—, y
    /// estas pruebas lo fijan por identidad: cada seccion arma su objetivo desde SU lado.</para>
    ///
    /// <para><b>Defecto 3.</b> No habia forma de decir «este objetivo no lleva tope» de una vez ni de leerlo despues:
    /// solo se podia apagar celda por celda, y la ausencia se confundia con «todavia no lo he tocado». «Ninguno» es
    /// ahora una opcion explicita del mismo selector, acotada al lado y al objetivo, persistida y reversible.</para>
    /// </summary>
    public sealed class PushBackRearTopePerSideAndNoneTests
    {
        private static RackPushBackSystemWindow Shown()
        {
            var w = new RackPushBackSystemWindow
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
                Left = -10000,
                Top = -10000,
            };
            w.Show();
            w.UpdateLayout();
            return w;
        }

        /// <summary>Un compuesto de tres ranuras con el lado B presente en todas.</summary>
        private static RackPushBackSystemWindow Composite()
        {
            var w = Shown();
            w.CompositeState.SetSlotCount(3);
            w.CompositeState.SetSideBPresent(true);
            for (var slot = 0; slot < 3; slot++)
            {
                w.CompositeState.SetSlotPresent(PushBackSide.B, slot, true);
            }

            return w;
        }

        /// <summary>
        /// Acepta «Elementos de seguridad» SIN cambiar nada. Escribir en el estado compuesto no recalcula por si
        /// solo, asi que esto asienta el modelo antes de medir una linea base — y de paso demuestra que la ruta sin
        /// gesto no mueve el dibujo.
        /// </summary>
        private static void Settle(RackPushBackSystemWindow w)
        {
            w.SafetyDialog = selections => selections.ToList();
            EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
            w.SafetyDialog = null;
        }

        /// <summary>Los niveles que cada seccion entrega a SU rejilla, en orden A, B.</summary>
        private static List<int[]> LevelsHandedToEachGrid(RackPushBackSystemWindow w)
        {
            var seen = new List<int[]>();
            w.RearTopeDialog = (_, levels) =>
            {
                seen.Add(levels.ToArray());
                return null;
            };
            w.SafetyDialog = _ =>
            {
                w.RearTopeSectionForTest.Configure();
                w.RearTopeSectionBForTest?.Configure();
                return null;   // cancelado: solo interesa CON QUE se abrieron
            };
            EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
            return seen;
        }

        private static IReadOnlyList<string> Topes(RackPushBackSystemWindow w)
        {
            var system = w.LastComputation?.System;
            if (system == null)
            {
                return new List<string>();
            }

            return new PushBackSystemPlantaBuilder().BuildPlan(system, w.Session.Catalog).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Tope)
                .Select(instance => FormattableString.Invariant(
                    $"{instance.Insertion.X:0.###}|{instance.Insertion.Y:0.###}"))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        private static int TopesInBom(RackPushBackSystemWindow w)
        {
            var system = w.LastComputation?.System;
            if (system == null)
            {
                return 0;
            }

            return PushBackBomBuilder.Build(system, w.Session.Catalog).Components
                .Where(component => component.Category == PushBackBomBuilder.RearTope)
                .Sum(component => component.Quantity);
        }

        // ==================== DEFECTO 1: el blanco es del LADO ====================

        /// <summary>Un frente en blanco en A no pone en blanco ese frente en B: B abre con SUS niveles.</summary>
        [Fact]
        public void RearTope_BlankA_DoesNotBlankB()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.CompositeState.SetSlotPresent(PushBackSide.A, 1, false);
                    var seen = LevelsHandedToEachGrid(w);

                    Assert.Equal(2, seen.Count);
                    Assert.Equal(0, seen[0][1]);      // A: el frente 2 esta en blanco
                    Assert.NotEqual(0, seen[1][1]);   // B: no lo esta
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y al reves, que es el mismo contrato leido desde el otro lado.</summary>
        [Fact]
        public void RearTope_BlankB_DoesNotBlankA()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.CompositeState.SetSlotPresent(PushBackSide.B, 1, false);
                    var seen = LevelsHandedToEachGrid(w);

                    Assert.Equal(2, seen.Count);
                    Assert.NotEqual(0, seen[0][1]);
                    Assert.Equal(0, seen[1][1]);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Con el frente en blanco en LOS DOS lados, los dos lo declaran: el blanco no se hereda, se afirma.</summary>
        [Fact]
        public void RearTope_BlankBoth_AffectsBothExplicitly()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.CompositeState.SetSlotPresent(PushBackSide.A, 1, false);
                    w.CompositeState.SetSlotPresent(PushBackSide.B, 1, false);
                    var seen = LevelsHandedToEachGrid(w);

                    Assert.Equal(2, seen.Count);
                    Assert.Equal(0, seen[0][1]);
                    Assert.Equal(0, seen[1][1]);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>
        /// El objetivo de cada seccion es SU lado: los niveles con los que abre son exactamente los de ese lado,
        /// enteros, y no los del lado activo. Es la afirmacion de identidad de la que cuelgan las demas.
        /// </summary>
        [Fact]
        public void RearTope_TargetsAreSideScoped()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.CompositeState.SetSlotPresent(PushBackSide.A, 1, false);
                    var seen = LevelsHandedToEachGrid(w);

                    Assert.Equal(
                        PushBackRearTopeDialogAdapter.LevelsPerFrente(
                            w.CompositeState.Of(PushBackSide.A).Structure.EffectiveLevelCounts(), allowBlankFronts: true),
                        seen[0]);
                    Assert.Equal(
                        PushBackRearTopeDialogAdapter.LevelsPerFrente(
                            w.CompositeState.Of(PushBackSide.B).Structure.EffectiveLevelCounts(), allowBlankFronts: true),
                        seen[1]);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Cambiar el lado ACTIVO no cambia con que abre cada seccion: el activo es contexto, no autoridad.</summary>
        [Fact]
        public void RearTope_ActiveSide_DoesNotDecideTheGrids()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.CompositeState.SetSlotPresent(PushBackSide.A, 1, false);
                    var fromA = LevelsHandedToEachGrid(w);

                    w.CompositeState.SetActiveSide(PushBackSide.B);
                    var fromB = LevelsHandedToEachGrid(w);

                    Assert.Equal(fromA[0], fromB[0]);
                    Assert.Equal(fromA[1], fromB[1]);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Editar el tope de A no cambia el de B — el contrato StopA/StopB, ahora con un blanco de por medio.</summary>
        [Fact]
        public void RearTope_EditingA_DoesNotChangeB()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.CompositeState.SetSlotPresent(PushBackSide.A, 1, false);
                    var saqueB = w.CompositeState.Of(PushBackSide.B).RearTopeSaque;

                    w.RearTopeDialog = (_, __) => new SafetyTopeGridWindow.TopeResult { Saque = 7.0 };
                    w.SafetyDialog = selections =>
                    {
                        w.RearTopeSectionForTest.Configure();
                        return selections.ToList();
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.Equal(7.0, w.CompositeState.Of(PushBackSide.A).RearTopeSaque, 9);
                    Assert.Equal(saqueB, w.CompositeState.Of(PushBackSide.B).RearTopeSaque, 9);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y editar el de B no cambia el de A.</summary>
        [Fact]
        public void RearTope_EditingB_DoesNotChangeA()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.CompositeState.SetSlotPresent(PushBackSide.B, 1, false);
                    var saqueA = w.CompositeState.Of(PushBackSide.A).RearTopeSaque;

                    w.RearTopeDialog = (_, __) => new SafetyTopeGridWindow.TopeResult { Saque = 5.0 };
                    w.SafetyDialog = selections =>
                    {
                        w.RearTopeSectionBForTest.Configure();
                        return selections.ToList();
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.Equal(5.0, w.CompositeState.Of(PushBackSide.B).RearTopeSaque, 9);
                    Assert.Equal(saqueA, w.CompositeState.Of(PushBackSide.A).RearTopeSaque, 9);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>
        /// Un blanco NO compacta el indice del frente fisico: la rejilla del lado que si tiene ese frente conserva
        /// tantas columnas como frentes tiene el rack, con el cero en su sitio. Sin esto, el frente 3 pasaria a ser
        /// el 2 en un lado y no en el otro, y las dos rejillas dejarian de hablar del mismo rack.
        /// </summary>
        [Fact]
        public void RearTope_BlankDoesNotCompactPhysicalFrontIndex()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.CompositeState.SetSlotPresent(PushBackSide.A, 1, false);
                    var seen = LevelsHandedToEachGrid(w);

                    Assert.Equal(3, seen[0].Length);
                    Assert.Equal(3, seen[1].Length);
                    Assert.NotEqual(0, seen[0][0]);
                    Assert.Equal(0, seen[0][1]);
                    Assert.NotEqual(0, seen[0][2]);
                }
                finally { w.Close(); }
            });
        }

        // ==================== DEFECTO 3: «Ninguno» ====================

        /// <summary>«Ninguno» es una opcion del selector, no la ausencia de eleccion.</summary>
        [Fact]
        public void RearTope_None_IsExplicitOption()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.SafetyDialog = _ => null;
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    var ids = w.RearTopeSectionForTest.PieceBox.Items
                        .OfType<RackCad.UI.CatalogOption>().Select(option => option.Id).ToList();
                    Assert.Contains(PushBackRearTopeConfig.NonePieceId, ids);
                    Assert.NotEqual(PushBackRearTopeConfig.NonePieceId, w.RearTopeSectionForTest.PieceBox.SelectedId);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Elegirlo en A deja a A sin topes y no toca a B: es del LADO.</summary>
        [Fact]
        public void RearTope_None_IsSideScoped()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Settle(w);
                    var before = Topes(w);
                    w.SafetyDialog = selections =>
                    {
                        w.RearTopeSectionForTest.PieceBox.SelectedId = PushBackRearTopeConfig.NonePieceId;
                        return selections.ToList();
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.True(w.CompositeState.Of(PushBackSide.A).RearTopeConfig().IsNone);
                    Assert.False(w.CompositeState.Of(PushBackSide.B).RearTopeConfig().IsNone);
                    Assert.NotEmpty(before);
                    Assert.NotEmpty(Topes(w));            // B sigue llevando los suyos
                    Assert.True(Topes(w).Count < before.Count);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y elegirlo en B deja a A intacto.</summary>
        [Fact]
        public void RearTope_None_OnB_LeavesAUntouched()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.SafetyDialog = selections =>
                    {
                        w.RearTopeSectionBForTest.PieceBox.SelectedId = PushBackRearTopeConfig.NonePieceId;
                        return selections.ToList();
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.False(w.CompositeState.Of(PushBackSide.A).RearTopeConfig().IsNone);
                    Assert.True(w.CompositeState.Of(PushBackSide.B).RearTopeConfig().IsNone);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Quita la geometria Y el BOM del objetivo, y SOLO de el: dibujo y lista siguen de acuerdo.</summary>
        [Fact]
        public void RearTope_None_RemovesGeometryAndBomOnlyForTarget()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Settle(w);
                    var geometryBefore = Topes(w).Count;
                    var bomBefore = TopesInBom(w);

                    w.SafetyDialog = selections =>
                    {
                        w.RearTopeSectionForTest.PieceBox.SelectedId = PushBackRearTopeConfig.NonePieceId;
                        return selections.ToList();
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.True(Topes(w).Count < geometryBefore);
                    Assert.True(TopesInBom(w) < bomBefore);
                    Assert.True(TopesInBom(w) > 0);        // los de B siguen contados
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Cancelar la ventana no lo persiste: sigue siendo transaccional como el resto de la seccion.</summary>
        [Fact]
        public void RearTope_None_CancelIsTransactional()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.SafetyDialog = _ =>
                    {
                        w.RearTopeSectionForTest.PieceBox.SelectedId = PushBackRearTopeConfig.NonePieceId;
                        return null;
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.False(w.CompositeState.Of(PushBackSide.A).RearTopeConfig().IsNone);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>La seccion lo DICE: el estado que se lee sin abrir la rejilla es «sin tope», no un SAQUE.</summary>
        [Fact]
        public void RearTope_None_IsVisibleInTheSectionStatus()
        {
            var config = new PushBackRearTopeConfig { PieceId = PushBackRearTopeConfig.NonePieceId };
            Assert.Equal(PushBackRearTopeSection.NoneStatusText, PushBackRearTopeSection.StatusText(config));
        }

        /// <summary>
        /// No borra la mascara por celda: volver a elegir una pieza devuelve exactamente las celdas que habia. Los
        /// dos alcances conviven —«Ninguno» resume el objetivo, la rejilla decide celda a celda— y ninguno pisa al otro.
        /// </summary>
        [Fact]
        public void RearTope_None_IsReversible_AndKeepsThePerCellMask()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.CompositeState.Of(PushBackSide.A).Cell(1, 0).RearTopeEnabled = false;
                    Settle(w);
                    var withMask = Topes(w);

                    w.SafetyDialog = selections =>
                    {
                        w.RearTopeSectionForTest.PieceBox.SelectedId = PushBackRearTopeConfig.NonePieceId;
                        return selections.ToList();
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
                    Assert.True(w.CompositeState.Of(PushBackSide.A).RearTopeConfig().IsNone);

                    w.SafetyDialog = selections =>
                    {
                        w.RearTopeSectionForTest.PieceBox.SelectedId = PushBackRearTopeBuilder.TopePieceId;
                        return selections.ToList();
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.False(w.CompositeState.Of(PushBackSide.A).RearTopeConfig().IsNone);
                    Assert.False(w.CompositeState.RearTopeAt(PushBackSide.A, 1, 0));   // la celda apagada sigue apagada
                    Assert.Equal(withMask, Topes(w));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>
        /// Se PERSISTE y sobrevive a RACKEDITAR: guardar el diseño y volver a cargarlo devuelve «Ninguno» en el lado
        /// que lo tenia, y solo en ese. Sin esto seria un estado de pantalla, no una decision del rack.
        /// </summary>
        [Fact]
        public void RearTope_None_SurvivesRackEditar()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.SafetyDialog = selections =>
                    {
                        w.RearTopeSectionForTest.PieceBox.SelectedId = PushBackRearTopeConfig.NonePieceId;
                        return selections.ToList();
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    var design = w.LastComputation?.Design;
                    Assert.NotNull(design);
                    var document = PushBackDesignDocument.FromDomain(design);
                    var restored = document.ToDomain();

                    Assert.True(restored.RearTope.IsNone);
                    Assert.False(restored.SideB.RearTope.IsNone);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Un rack de un solo sentido tambien lo ofrece: no es una capacidad del compuesto.</summary>
        [Fact]
        public void RearTope_None_IsOfferedInASingleSidedRack()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    Settle(w);
                    var before = Topes(w);
                    w.SafetyDialog = selections =>
                    {
                        w.RearTopeSectionForTest.PieceBox.SelectedId = PushBackRearTopeConfig.NonePieceId;
                        return selections.ToList();
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.NotEmpty(before);
                    Assert.Empty(Topes(w));
                    Assert.Equal(0, TopesInBom(w));
                }
                finally { w.Close(); }
            });
        }
    }
}
