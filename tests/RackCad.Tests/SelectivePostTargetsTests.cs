using System;
using System.Linq;
using System.Reflection;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;
using F = RackCad.Tests.SelectiveHeaderBatchFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 G4 — S-18: la gramatica de <see cref="SelectivePostTargets"/>, el eje de postes que el Selectivo combina con
    /// los <c>TargetFondos</c> de I-43 (Proposal V2 §6.3).
    ///
    /// <para>
    /// Es la misma gramatica que la de fondos, aplicada a los postes PRINCIPALES de la reticula maestra
    /// (<c>0..MaxFrenteCount()</c>): Actual sigue al poste actual, Todos se re-expande, un explicito se poda sin
    /// resucitar y cae al actual si queda vacio. Nada de esto se persiste ni se recuerda entre sesiones.
    /// </para>
    /// </summary>
    public class SelectivePostTargetsTests
    {
        [Fact]
        public void S18_AL_ABRIR_VALE_ACTUAL_Y_SIGUE_AL_POSTE_ACTUAL()
        {
            var state = F.State(3, 3);
            var targets = new SelectivePostTargets();

            Assert.Equal(SelectivePostTargetMode.FollowCurrent, targets.Mode);
            Assert.Equal(new[] { 2 }, targets.Resolve(state, 2));
            Assert.Equal(new[] { 0 }, targets.Resolve(state, 0));
            Assert.Empty(targets.ExplicitPosts);
        }

        [Fact]
        public void S18_TODOS_ES_LA_RETICULA_MAESTRA_Y_SE_REEXPANDE_TRAS_CRECER()
        {
            var state = F.State(3, 1);
            var targets = new SelectivePostTargets();
            targets.FollowAllPosts();

            Assert.Equal(SelectivePostTargetMode.All, targets.Mode);
            Assert.Equal(new[] { 0, 1, 2, 3 }, targets.Resolve(state, 1));

            state.ResizeBays(5); // el fondo en edicion crece: la reticula maestra pasa a 0..5
            targets.SyncPostTargets(state);

            Assert.Equal(SelectivePostTargetMode.All, targets.Mode);
            Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, targets.Resolve(state, 1));
        }

        [Fact]
        public void S18_UN_EXPLICITO_ES_UN_CONJUNTO_DISTINTO_Y_ASCENDENTE_DE_POSTES_PRINCIPALES()
        {
            var state = F.State(3);
            var targets = new SelectivePostTargets();

            targets.SetTargetPosts(new[] { 3, 1, 1, -2, 9 }, state);

            Assert.Equal(SelectivePostTargetMode.Explicit, targets.Mode);
            Assert.Equal(new[] { 1, 3 }, targets.ExplicitPosts);
            Assert.Equal(new[] { 1, 3 }, targets.Resolve(state, 0)); // no depende del poste actual
        }

        [Fact]
        public void S18_UN_EXPLICITO_SIN_NINGUN_POSTE_DEL_UNIVERSO_CAE_AL_ACTUAL()
        {
            var state = F.State(3);
            var targets = new SelectivePostTargets();

            targets.SetTargetPosts(new[] { -1, 7 }, state);
            Assert.Equal(SelectivePostTargetMode.FollowCurrent, targets.Mode);
            Assert.Equal(new[] { 2 }, targets.Resolve(state, 2));

            targets.SetTargetPosts(null, state);
            Assert.Equal(SelectivePostTargetMode.FollowCurrent, targets.Mode);
        }

        [Fact]
        public void S18_TRAS_UN_CAMBIO_ESTRUCTURAL_EL_EXPLICITO_SE_PODA_SIN_RESUCITAR_Y_VACIO_CAE_AL_ACTUAL()
        {
            var state = F.State(4);
            var targets = new SelectivePostTargets();
            targets.SetTargetPosts(new[] { 1, 4 }, state);
            Assert.Equal(new[] { 1, 4 }, targets.ExplicitPosts);

            state.ResizeBays(2); // postes 0..2: el 4 deja de existir
            targets.SyncPostTargets(state);
            Assert.Equal(new[] { 1 }, targets.ExplicitPosts);

            state.ResizeBays(4); // vuelve a haber poste 4, pero lo podado no vuelve
            targets.SyncPostTargets(state);
            Assert.Equal(new[] { 1 }, targets.ExplicitPosts);

            targets.SetTargetPosts(new[] { 3, 4 }, state);
            state.ResizeBays(1); // postes 0..1: el explicito queda vacio
            targets.SyncPostTargets(state);
            Assert.Equal(SelectivePostTargetMode.FollowCurrent, targets.Mode);
            Assert.Empty(targets.ExplicitPosts);
            Assert.Equal(new[] { 1 }, targets.Resolve(state, 1));
        }

        [Fact]
        public void S18_ACTUAL_Y_TODOS_NO_SE_TOCAN_AL_SINCRONIZAR()
        {
            var state = F.State(3);
            var actual = new SelectivePostTargets();
            var todos = new SelectivePostTargets();
            todos.FollowAllPosts();

            state.ResizeBays(1);
            actual.SyncPostTargets(state);
            todos.SyncPostTargets(state);

            Assert.Equal(SelectivePostTargetMode.FollowCurrent, actual.Mode);
            Assert.Equal(new[] { 1 }, actual.Resolve(state, 1));
            Assert.Equal(SelectivePostTargetMode.All, todos.Mode);
            Assert.Equal(new[] { 0, 1 }, todos.Resolve(state, 0));
        }

        [Fact]
        public void S18_LOS_POSTES_DE_MEDIO_FRENTE_NO_SON_DIRECCIONABLES()
        {
            var state = F.State(3);
            state.ApplySegmentsToTargets(1, new[]
            {
                new SelectiveSegment { Length = 20.0, Loaded = true },
                new SelectiveSegment { Length = 22.0, Loaded = true },
            });
            var targets = new SelectivePostTargets();

            targets.FollowAllPosts();
            Assert.Equal(new[] { 0, 1, 2, 3 }, targets.Resolve(state, 0)); // el poste intermedio del medio frente no cuenta

            targets.SetTargetPosts(new[] { 4 }, state); // no hay poste principal 4
            Assert.Equal(SelectivePostTargetMode.FollowCurrent, targets.Mode);
        }

        [Fact]
        public void S18_GUARDA_POSTTARGETS_NO_SE_PERSISTE_NI_SE_RECUERDA()
        {
            // Runtime puro: ni el diseno, ni su documento, ni el estado del editor, ni la preferencia recordada de los
            // fondos objetivo tienen donde guardarlo.
            const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var owners = new[]
            {
                typeof(SelectivePalletDesign), typeof(SelectivePalletDesignDocument), typeof(SelectiveEditorState),
                typeof(SelectiveTargetPreference),
            };

            foreach (var owner in owners)
            {
                Assert.DoesNotContain(
                    owner.GetMembers(All),
                    member => member.Name.IndexOf("PostTarget", StringComparison.OrdinalIgnoreCase) >= 0);
            }

            Assert.DoesNotContain(
                typeof(SelectivePostTargets).GetCustomAttributesData(),
                attribute => attribute.AttributeType == typeof(SerializableAttribute)
                             || (attribute.AttributeType.Namespace ?? string.Empty).StartsWith("System.Text.Json", StringComparison.Ordinal));
        }
    }
}
