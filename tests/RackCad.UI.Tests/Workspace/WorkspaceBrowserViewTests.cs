#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F3-T2-BROWSER-UI (INV-F3T2-07, 10, 11, 15): the browser section of the root panel. The tab strip wraps and works from a
    /// narrow bar to a wide window, there is exactly ONE surface element that every tab reuses, the section goes before the navigation,
    /// no control has a fixed size, and a failing click never escapes the control.
    /// </summary>
    public class WorkspaceBrowserViewTests
    {
        private static (WorkspacePanelView View, WorkspacePortDouble Host, WorkspaceNavigationPortDouble Navigation, WorkspaceBrowserSummaryPortDouble Summary) Build(
            WorkspaceHostSnapshot? snapshot = null, bool withSummaryPort = true)
        {
            var host = new WorkspacePortDouble { Snapshot = snapshot ?? WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-1")) };
            var navigation = new WorkspaceNavigationPortDouble();
            var summary = new WorkspaceBrowserSummaryPortDouble();
            var view = new WorkspacePanelView(new WorkspacePanelViewModel(host, _ => { }, navigation, withSummaryPort ? summary : null));
            view.ViewModel.Refresh();
            Lay(view, 400);
            return (view, host, navigation, summary);
        }

        private static T Named<T>(WorkspacePanelView view, string name)
            where T : class
        {
            var found = view.FindName(name);
            Assert.NotNull(found);
            return Assert.IsAssignableFrom<T>(found);
        }

        private static void Lay(WorkspacePanelView view, double width)
        {
            view.Measure(new Size(width, 6000));
            view.Arrange(new Rect(0, 0, width, 6000));
            view.UpdateLayout();
        }

        private static IEnumerable<T> Descendants<T>(DependencyObject root)
            where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match) yield return match;
                foreach (var nested in Descendants<T>(child)) yield return nested;
            }
        }

        private static Button TabButton(WorkspacePanelView view, string rack, string automationName) =>
            Descendants<Button>(Named<ItemsControl>(view, "BrowserTabStrip"))
                .Single(b => b.DataContext is WorkspaceBrowserTabItem item
                             && item.RackId == rack
                             && System.Windows.Automation.AutomationProperties.GetName(b) == automationName);

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        private static Rect Bounds(FrameworkElement element, FrameworkElement root) =>
            element.TransformToAncestor(root).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

        private static int Rows(WorkspacePanelView view)
        {
            var panel = Descendants<WrapPanel>(Named<ItemsControl>(view, "BrowserTabStrip")).First();
            return panel.Children.OfType<FrameworkElement>()
                .Select(c => Math.Round(LayoutInformation.GetLayoutSlot(c).Top))
                .Distinct()
                .Count();
        }

        private static void PinTabs(WorkspacePanelView view, WorkspacePortDouble host, int count)
        {
            for (var i = 1; i <= count; i++)
            {
                host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-" + i));
                view.ViewModel.Refresh();
                view.ViewModel.Browser.PinTab("R-" + i);
            }
        }

        // ------------------------------------------------------------------ INV-F3T2-07: one surface element

        [Fact]
        public void TheViewHasExactlyOneSurfaceElementAndEveryTabChangeReusesIt()
        {
            StaTestRunner.Run(() =>
            {
                var (view, host, _, _) = Build();
                PinTabs(view, host, 3);
                Lay(view, 400);

                var surface = Named<Border>(view, "BrowserSurface");
                Assert.Single(Descendants<Border>(view).Where(b => b.Name == "BrowserSurface"));
                Assert.Single(Descendants<TextBlock>(view).Where(t => t.Name == "BrowserSurfaceTitleText"));
                Assert.Equal(Visibility.Visible, surface.Visibility);
                Assert.Equal("Nombre R-3", Named<TextBlock>(view, "BrowserSurfaceTitleText").Text);

                foreach (var rack in new[] { "R-1", "R-2", "R-3", "R-1" })
                {
                    Click(TabButton(view, rack, "Activar pestaña"));
                    Lay(view, 400);

                    Assert.Same(surface, Named<Border>(view, "BrowserSurface"));
                    Assert.Single(Descendants<Border>(view).Where(b => b.Name == "BrowserSurface"));
                    Assert.Equal("Nombre " + rack, Named<TextBlock>(view, "BrowserSurfaceTitleText").Text);
                    Assert.Equal(1, view.ViewModel.Browser.LiveSurfaceCount);
                }

                // The surface is a sibling of the strip, not an item of it: tabs never own a surface.
                Assert.DoesNotContain(Descendants<Border>(Named<ItemsControl>(view, "BrowserTabStrip")), b => b.Name == "BrowserSurface");
            });
        }

        [Fact]
        public void WithoutTabsThereIsNoSurfaceAndTheEmptyReasonIsShown()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, _, summary) = Build(WorkspacePanelFixtures.NoDocument());

                Assert.NotEqual(Visibility.Visible, Named<Border>(view, "BrowserSurface").Visibility);
                Assert.Equal(0, view.ViewModel.Browser.LiveSurfaceCount);
                Assert.Empty(Descendants<Button>(Named<ItemsControl>(view, "BrowserTabStrip")));
                var empty = Named<TextBlock>(view, "BrowserEmptyText");
                Assert.Equal(Visibility.Visible, empty.Visibility);
                Assert.Contains("documento", empty.Text, StringComparison.Ordinal);
                Assert.Empty(summary.Received);
            });
        }

        [Fact]
        public void WithoutASummaryPortTheSectionIsInertAndSaysSo()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, navigation, _) = Build(withSummaryPort: false);

                Assert.NotEqual(Visibility.Visible, Named<Border>(view, "BrowserSurface").Visibility);
                Assert.Empty(Descendants<Button>(Named<ItemsControl>(view, "BrowserTabStrip")));
                Assert.Contains("no está disponible", Named<TextBlock>(view, "BrowserEmptyText").Text, StringComparison.Ordinal);
                Assert.Empty(navigation.Calls);
            });
        }

        // ------------------------------------------------------------------ the clicks are explicit, memory only, and contained

        [Fact]
        public void TheTabClicksActivatePinAndCloseInMemoryAndNeverReachTheNavigationOrTheHost()
        {
            StaTestRunner.Run(() =>
            {
                var (view, host, navigation, _) = Build();
                view.ViewModel.Browser.PinTab("R-1");
                host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-2"));
                view.ViewModel.Refresh();
                Lay(view, 400);
                var snapshots = host.SnapshotCalls;
                Assert.Equal(new[] { "R-1", "R-2" }, view.ViewModel.Browser.Tabs.Select(t => t.RackId).ToArray());
                Assert.Single(view.ViewModel.Browser.Tabs.Where(t => t.IsPreview));

                Click(TabButton(view, "R-2", "Fijar pestaña"));
                Lay(view, 400);
                Assert.DoesNotContain(view.ViewModel.Browser.Tabs, t => t.IsPreview);
                Assert.Empty(Descendants<Button>(Named<ItemsControl>(view, "BrowserTabStrip")).Where(b => System.Windows.Automation.AutomationProperties.GetName(b) == "Fijar pestaña" && b.IsVisible));

                Click(TabButton(view, "R-1", "Activar pestaña"));
                Lay(view, 400);
                Assert.Equal("R-1", view.ViewModel.Browser.ActiveRackId);
                Click(TabButton(view, "R-1", "Cerrar pestaña"));
                Lay(view, 400);

                Assert.Equal(new[] { "R-2" }, view.ViewModel.Browser.Tabs.Select(t => t.RackId).ToArray());
                Assert.Equal("R-2", view.ViewModel.Browser.ActiveRackId);
                Assert.Empty(navigation.Calls);
                Assert.Equal(snapshots, host.SnapshotCalls);
                Assert.Equal(0, host.AuthorizeCalls);
                Assert.Equal(0, host.SelectionCalls);
            });
        }

        // ------------------------------------------------------------------ INV-F3T2-22 (T2-F1): the active tab is visibly distinct

        // The Border that the template of the strip generates for each tab (the buttons' own template borders have a content presenter).
        private static List<Border> TabBorders(WorkspacePanelView view) =>
            Descendants<Border>(Named<ItemsControl>(view, "BrowserTabStrip"))
                .Where(b => b.DataContext is WorkspaceBrowserTabItem && b.Child is WrapPanel)
                .ToList();

        [Fact]
        public void TheActiveTabIsDistinguishedFromTheOthersOnTheGeneratedBorder()
        {
            StaTestRunner.Run(() =>
            {
                var (view, host, _, _) = Build();
                PinTabs(view, host, 2);
                Lay(view, 400);
                var primary = (Brush)view.FindResource("ShellSelectionPrimaryBrush");
                var plain = (Brush)view.FindResource("ShellBorderBrush");

                var borders = TabBorders(view);
                Assert.Equal(2, borders.Count);
                var active = borders.Single(b => ((WorkspaceBrowserTabItem)b.DataContext).IsActive);
                var other = borders.Single(b => !((WorkspaceBrowserTabItem)b.DataContext).IsActive);
                Assert.Equal("R-2", ((WorkspaceBrowserTabItem)active.DataContext).RackId);
                Assert.Equal(new Thickness(2), active.BorderThickness);
                Assert.Same(primary, active.BorderBrush);
                Assert.Equal(new Thickness(1), other.BorderThickness);
                Assert.Same(plain, other.BorderBrush);

                // The distinction follows the active tab when it changes.
                Click(TabButton(view, "R-1", "Activar pestaña"));
                Lay(view, 400);
                borders = TabBorders(view);
                var nowActive = borders.Single(b => ((WorkspaceBrowserTabItem)b.DataContext).IsActive);
                Assert.Equal("R-1", ((WorkspaceBrowserTabItem)nowActive.DataContext).RackId);
                Assert.Equal(new Thickness(2), nowActive.BorderThickness);
                Assert.Same(primary, nowActive.BorderBrush);
                var nowOther = borders.Single(b => !((WorkspaceBrowserTabItem)b.DataContext).IsActive);
                Assert.Equal(new Thickness(1), nowOther.BorderThickness);
                Assert.Same(plain, nowOther.BorderBrush);
            });
        }

        // ------------------------------------------------------------------ INV-F3T2-24 (T2-F4): the reload button of the one surface

        [Fact]
        public void TheSurfaceHasOneReloadButtonThatAsksAgainForTheSummaryOfTheActiveTabAsAUserAction()
        {
            StaTestRunner.Run(() =>
            {
                var (view, host, navigation, summary) = Build();
                PinTabs(view, host, 3);
                Lay(view, 400);
                var surface = Named<Border>(view, "BrowserSurface");
                var reload = Named<Button>(view, "BrowserSurfaceReloadButton");

                Assert.True(reload.IsDescendantOf(surface));
                Assert.Single(Descendants<Button>(view).Where(b => b.Name == "BrowserSurfaceReloadButton"));
                Assert.Equal("Volver a leer", Assert.IsType<string>(reload.Content));
                Assert.DoesNotContain(Descendants<Button>(Named<ItemsControl>(view, "BrowserTabStrip")), b => b.Name == "BrowserSurfaceReloadButton");

                var reads = summary.Received.Count;
                summary.Responder = _ => WorkspaceBrowserSummaryPortDouble.Ready("Releído", "Selective", "planta");
                Click(reload);
                Lay(view, 400);

                Assert.Equal(reads + 1, summary.Received.Count);
                var call = summary.Received.Last();
                Assert.Equal(("R-3", true, new SessionId(7)), (call.RackId, call.UserRequested, call.Origin.Origin));
                Assert.Equal("Releído", Named<TextBlock>(view, "BrowserSurfaceTitleText").Text);
                Assert.Equal("Vistas: planta", Named<TextBlock>(view, "BrowserSurfaceViewsText").Text);
                Assert.Same(surface, Named<Border>(view, "BrowserSurface"));
                Assert.Empty(navigation.Calls);
            });
        }

        [Fact]
        public void TheReloadButtonRecoversASurfaceThatFailedAndContainsAFailingReload()
        {
            StaTestRunner.Run(() =>
            {
                var (view, _, _, summary) = Build();
                summary.ThrowOnRead = new InvalidOperationException("boom-primero");
                view.ViewModel.Refresh();
                view.ViewModel.Browser.ReloadSummary();
                Lay(view, 400);
                Assert.Contains("boom-primero", Named<TextBlock>(view, "BrowserSurfaceStatusText").Text, StringComparison.Ordinal);

                summary.ThrowOnRead = null;
                Click(Named<Button>(view, "BrowserSurfaceReloadButton"));
                Lay(view, 400);
                Assert.Equal("Resumen cargado.", Named<TextBlock>(view, "BrowserSurfaceStatusText").Text);

                summary.ThrowOnRead = new InvalidOperationException("boom-segundo");
                var thrown = Record.Exception(() => Click(Named<Button>(view, "BrowserSurfaceReloadButton")));
                Lay(view, 400);
                Assert.Null(thrown);
                Assert.Contains("boom-segundo", Named<TextBlock>(view, "BrowserSurfaceStatusText").Text, StringComparison.Ordinal);
                Assert.NotEqual(WorkspacePanelState.Error, view.ViewModel.State);
            });
        }

        // ------------------------------------------------------------------ INV-F3T2-26 (T2-F6): the containers of the strip survive a presentation

        [Fact]
        public void APresentationWithNoChangeDoesNotRegenerateTheContainersOfTheStrip()
        {
            StaTestRunner.Run(() =>
            {
                var (view, host, _, _) = Build();
                PinTabs(view, host, 3);
                Lay(view, 400);
                var buttons = Descendants<Button>(Named<ItemsControl>(view, "BrowserTabStrip")).ToList();
                var borders = TabBorders(view);
                Assert.True(buttons.Count >= 6 && borders.Count == 3, "the strip did not render its three tabs");

                view.ViewModel.Refresh();
                view.ViewModel.Refresh();
                Lay(view, 400);

                Assert.Equal(buttons.Count, Descendants<Button>(Named<ItemsControl>(view, "BrowserTabStrip")).Count());
                Assert.All(Descendants<Button>(Named<ItemsControl>(view, "BrowserTabStrip")), b => Assert.Contains(b, buttons));
                Assert.All(TabBorders(view), b => Assert.Contains(b, borders));

                // A real change does regenerate them.
                view.ViewModel.Browser.ActivateTab("R-1");
                Lay(view, 400);
                Assert.Contains(TabBorders(view), b => !borders.Contains(b));
            });
        }

        [Fact]
        public void TheNavigationButtonsFollowTheTabThatWasActivatedByClick()
        {
            StaTestRunner.Run(() =>
            {
                var (view, host, navigation, _) = Build();
                view.ViewModel.Browser.PinTab("R-1");
                host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-2"));
                view.ViewModel.Refresh();
                Lay(view, 400);

                Click(TabButton(view, "R-1", "Activar pestaña"));
                Named<Button>(view, "NavigationSelectButton").RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

                var call = Assert.Single(navigation.Calls);
                Assert.Equal("R-1", call.RackId);
                Assert.Equal(new SessionId(7), call.Origin.Origin);
            });
        }

        [Fact]
        public void AFailingSummaryPortNeverEscapesAClickAndIsShownOnTheSurface()
        {
            StaTestRunner.Run(() =>
            {
                var (view, host, _, summary) = Build();
                view.ViewModel.Browser.PinTab("R-1");
                host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One("R-2"));
                view.ViewModel.Refresh();
                summary.ThrowOnRead = new InvalidOperationException("boom-superficie");
                Lay(view, 400);

                var thrown = Record.Exception(() => Click(TabButton(view, "R-1", "Activar pestaña")));
                Lay(view, 400);

                Assert.Null(thrown);
                Assert.Contains("boom-superficie", Named<TextBlock>(view, "BrowserSurfaceStatusText").Text, StringComparison.Ordinal);
                Assert.NotEqual(WorkspacePanelState.Error, view.ViewModel.State);
            });
        }

        // ------------------------------------------------------------------ INV-F3T2-10: responsive, no fixed sizes, before the navigation

        [Fact]
        public void NoControlOfTheBrowserSectionHasAFixedWidthHeightMinimumOrMaximum()
        {
            StaTestRunner.Run(() =>
            {
                var (view, host, _, _) = Build();
                PinTabs(view, host, 4);
                Lay(view, 400);

                var controls = new List<FrameworkElement>
                {
                    Named<FrameworkElement>(view, "BrowserTabStrip"), Named<FrameworkElement>(view, "BrowserSurface"),
                    Named<FrameworkElement>(view, "BrowserSurfaceTitleText"), Named<FrameworkElement>(view, "BrowserSurfaceKindText"),
                    Named<FrameworkElement>(view, "BrowserSurfaceViewsText"), Named<FrameworkElement>(view, "BrowserSurfaceStatusText"),
                    Named<FrameworkElement>(view, "BrowserEmptyText"),
                };

                // The elements the section declares; the inner parts of the button template are the shared style's, not the section's.
                controls.AddRange(Descendants<FrameworkElement>(Named<ItemsControl>(view, "BrowserTabStrip")).Where(e => e.TemplatedParent is not Button));
                Assert.True(controls.Count > 10);

                foreach (var element in controls)
                {
                    foreach (var property in new[]
                             {
                                 FrameworkElement.WidthProperty, FrameworkElement.MinWidthProperty, FrameworkElement.MaxWidthProperty,
                                 FrameworkElement.HeightProperty, FrameworkElement.MinHeightProperty, FrameworkElement.MaxHeightProperty,
                             })
                        Assert.True(element.ReadLocalValue(property) == DependencyProperty.UnsetValue, element.GetType().Name + " sets " + property.Name);
                    Assert.True(double.IsNaN(element.Width) && double.IsNaN(element.Height), element.GetType().Name + " has a fixed size");
                }

                Assert.IsType<WrapPanel>(Descendants<Panel>(Named<ItemsControl>(view, "BrowserTabStrip")).First());
            });
        }

        [Fact]
        public void TenTabsWrapInRowsAtNarrowWidthAndFitFewerRowsWhenWideWithoutOverflowAtEither()
        {
            StaTestRunner.Run(() =>
            {
                var (view, host, _, _) = Build();
                PinTabs(view, host, 10);

                var rowsByWidth = new Dictionary<double, int>();
                foreach (var width in new[] { 240d, 1600d })
                {
                    Lay(view, width);
                    rowsByWidth[width] = Rows(view);
                    foreach (var name in new[] { "BrowserTabStrip", "BrowserSurface", "BrowserSurfaceTitleText", "BrowserSurfaceStatusText" })
                    {
                        var element = Named<FrameworkElement>(view, name);
                        Assert.True(Bounds(element, view).Right <= width + 0.5, name + " overflows at " + width.ToString(CultureInfo.InvariantCulture));
                    }

                    foreach (var button in Descendants<Button>(Named<ItemsControl>(view, "BrowserTabStrip")))
                        Assert.True(Bounds(button, view).Right <= width + 0.5, "a tab button overflows at " + width.ToString(CultureInfo.InvariantCulture));
                }

                Assert.True(rowsByWidth[240] > rowsByWidth[1600], "the strip does not reflow: " + rowsByWidth[240] + " vs " + rowsByWidth[1600]);
                Assert.True(rowsByWidth[240] >= 3);
            });
        }

        [Fact]
        public void TheBrowserSectionComesBeforeTheNavigationAndIsPartOfTheOneRootControlWithNoWindow()
        {
            StaTestRunner.Run(() =>
            {
                var (view, host, _, _) = Build();
                PinTabs(view, host, 2);
                Lay(view, 400);

                var strip = Named<FrameworkElement>(view, "BrowserTabStrip");
                var surface = Named<FrameworkElement>(view, "BrowserSurface");
                var navigation = Named<FrameworkElement>(view, "NavigationActionsPanel");

                Assert.True(strip.IsDescendantOf(view) && surface.IsDescendantOf(view));
                Assert.True(Bounds(strip, view).Top < Bounds(surface, view).Top);
                Assert.True(Bounds(surface, view).Bottom <= Bounds(navigation, view).Top);
                Assert.Empty(Descendants<Window>(view));
                Assert.Null(Window.GetWindow(view));
            });
        }
    }
}
