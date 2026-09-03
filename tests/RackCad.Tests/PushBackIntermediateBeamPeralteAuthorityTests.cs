using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-44 · Gate 2 — CARACTERIZACION de las autoridades del peralte intermedio. Ninguna de estas pruebas juzga el
    /// defecto: fijan lo que el codigo hace HOY, para que el fix de Gate 3 se mida contra ello y para cerrar dos
    /// preguntas abiertas del Gate 1.
    ///
    /// <para>
    /// 1. <b>El estado hibrido no existe.</b> Ningun writer del historial puede persistir
    /// <c>Levels[n].IntermediateBeamDepth != IntermediateBeamDepths[n]</c>: las dos listas se escriben desde LA
    /// MISMA celda en un solo bucle (asi desde el commit que introdujo <c>Levels</c>), y todo camino de lectura las
    /// re-sincroniza. Estas pruebas fijan esa invariante en los tres limites.
    /// </para>
    /// <para>
    /// 2. <b>En un rack de UN SENTIDO el defecto solo puede SUBIR el peralte.</b> El conteo consulta
    /// <c>PeralteAt(system, nivel)</c>, que es un maximo sobre los frentes que alcanzan ese nivel — y un frente que
    /// materializa intermedios en un nivel esta necesariamente en ese conjunto. Por tanto un 4.5" NO puede salir
    /// como 3.5" por este camino, y el sintoma reportado en esa direccion tiene que venir de otro sitio.
    /// </para>
    /// </summary>
    public class PushBackIntermediateBeamPeralteAuthorityTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static readonly double[] Allowed = { 3.5, 4.5, 6.0 };

        private static PushBackDesign Design(params double[][] peraltesPerFront)
        {
            var levels = peraltesPerFront[0].Length;
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = levels,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };

            foreach (var peraltes in peraltesPerFront)
            {
                var front = new DynamicRackFrontDesign
                {
                    PalletCount = 1,
                    LoadLevels = peraltes.Length,
                    PalletsDeep = 4,
                    DepthStartPosition = 1
                };
                foreach (var peralte in peraltes)
                {
                    front.Levels.Add(new DynamicRackLevelDesign
                    {
                        IntermediateBeamCatalogId = DynamicRackDefaults.IntermediateBeamCatalogId,
                        IntermediateBeamDepth = peralte
                    });
                    front.IntermediateBeamDepths.Add(peralte);
                }

                design.Structure.Fronts.Add(front);
            }

            return design;
        }

        private static void AssertFrontListsAgree(DynamicRackFront front, string where)
        {
            for (var index = 0; index < front.Levels.Count && index < front.IntermediateBeamDepths.Count; index++)
            {
                Assert.Equal(
                    front.Levels[index].IntermediateBeamDepth,
                    front.IntermediateBeamDepths[index],
                    3);
            }

            Assert.True(
                front.Levels.Count == front.IntermediateBeamDepths.Count,
                where + ": las dos listas del frente " + front.Index + " tienen longitudes distintas ("
                    + front.Levels.Count + " vs " + front.IntermediateBeamDepths.Count + ")");
        }

        private static void AssertDesignListsAgree(DynamicRackFrontDesign front, string where)
        {
            Assert.True(
                front.Levels.Count == front.IntermediateBeamDepths.Count,
                where + ": diseno con listas de longitudes distintas ("
                    + front.Levels.Count + " vs " + front.IntermediateBeamDepths.Count + ")");
            for (var index = 0; index < front.Levels.Count; index++)
            {
                Assert.Equal(
                    front.Levels[index].IntermediateBeamDepth.GetValueOrDefault(),
                    front.IntermediateBeamDepths[index],
                    3);
            }
        }

        // ---- 1. El estado hibrido no es alcanzable ------------------------------------------------------------

        [Fact]
        public void AfterResolve_TheTwoPerFrontLists_AlwaysAgree()
        {
            var system = new PushBackResolver(Catalog).Resolve(
                Design(new[] { 3.5, 4.5 }, new[] { 6.0, 3.5 }, new[] { 4.5, 6.0 }));

            foreach (var front in system.Structure.Fronts)
            {
                AssertFrontListsAgree(front, "resolve");
            }
        }

        [Fact]
        public void TheEditorWriter_AlwaysEmitsBothListsFromTheSameCell()
        {
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var state = new PushBackEditorState();
            var inputs = state.LoadNew();
            state.SetFrontCount(2);
            for (var front = 0; front < 2; front++)
            {
                state.Structure.Fronts[front].LoadLevels = 2;
                state.AdjustLevels(front, 0);
            }

            state.Structure.Fronts[0].Cells[0].IntermediateBeamDepth = 4.5;
            state.Structure.Fronts[0].Cells[1].IntermediateBeamDepth = 6.0;
            state.Structure.Fronts[1].Cells[0].IntermediateBeamDepth = 3.5;
            state.Structure.Fronts[1].Cells[1].IntermediateBeamDepth = 4.5;

            foreach (var front in state.BuildEnvelopeFrontDesigns())
            {
                AssertDesignListsAgree(front, "editor");
            }

            var design = assembler.BuildDesign(state, inputs);
            foreach (var front in design.Structure.Fronts)
            {
                AssertDesignListsAgree(front, "assembler");
            }
        }

        [Fact]
        public void APersistenceRoundTrip_CannotDesynchroniseTheTwoLists()
        {
            var catalog = Catalog;
            var design = Design(new[] { 3.5, 4.5 }, new[] { 6.0, 3.5 });
            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            var reloaded = JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();

            foreach (var front in reloaded.Structure.Fronts)
            {
                AssertDesignListsAgree(front, "documento");
            }

            foreach (var front in new PushBackResolver(catalog).Resolve(reloaded).Structure.Fronts)
            {
                AssertFrontListsAgree(front, "documento -> resolve");
            }
        }

        // ---- 2. En un solo sentido el defecto solo puede SUBIR ------------------------------------------------

        /// <summary>
        /// Barrido acotado: 2 frentes x 2 niveles x {3.5, 4.5, 6} = 81 configuraciones. Para cada celda que
        /// materializa intermedios se compara el peralte publicado contra el authored. NUNCA baja. Es la prueba de
        /// que el 4.5 -> 3.5 reportado no puede nacer en el camino de un solo sentido.
        /// </summary>
        [Fact]
        public void InASingleDirectionRack_ThePublishedPeralte_IsNeverBelowTheAuthoredOne()
        {
            var catalog = Catalog;
            var builder = new PushBackIntermediateBeamLateralBuilder();
            var resolver = new PushBackResolver(catalog);
            var configurations = 0;
            var cellsChecked = 0;

            foreach (var a0 in Allowed)
            foreach (var a1 in Allowed)
            foreach (var b0 in Allowed)
            foreach (var b1 in Allowed)
            {
                var system = resolver.Resolve(Design(new[] { a0, a1 }, new[] { b0, b1 }));
                configurations++;

                foreach (var front in system.Structure.Fronts)
                {
                    for (var level = 1; level <= DynamicFrontActivation.EffectiveLoadLevels(front); level++)
                    {
                        var authored = DynamicIntermediateBeamGeometry.PeralteAt(front, level);
                        var instances = builder.BuildFor(system, catalog, front, new[] { level });
                        foreach (var instance in instances)
                        {
                            cellsChecked++;
                            var published = instance.DynamicParameters[SelectiveRackDefaults.PeralteParam];
                            Assert.True(
                                published >= authored - 1e-6,
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Frente {0} nivel {1}: authored {2:0.##}, publicado {3:0.##} (config {4:0.##}/{5:0.##} | {6:0.##}/{7:0.##})",
                                    front.Index, level, authored, published, a0, a1, b0, b1));
                        }
                    }
                }
            }

            Assert.Equal(81, configurations);
            Assert.True(cellsChecked > 0, "el barrido no materializo ningun intermedio");
        }

        // ---- 3. Editar y restaurar NO normaliza nada ----------------------------------------------------------

        private static string IntermediateSignature(PushBackSystem system, RackCatalog catalog)
            => string.Join(
                " | ",
                PushBackBomBuilder.Build(system, catalog).Components
                    .Where(component => component.Category == SystemBomBuilder.IntermediateBeam)
                    .OrderBy(component => component.Description, StringComparer.Ordinal)
                    .ThenBy(component => component.Length)
                    .Select(component => string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}|L={1:0.####}|{2}|x{3}",
                        component.ProfileId, component.Length, component.Description, component.Quantity)));

        /// <summary>
        /// El ciclo del incidente: cargar, abrir RACKEDITAR SIN cambios, reconstruir, guardar, recargar y volver a
        /// cotizar. El BOM es identico. No hay ninguna normalizacion escondida en el round trip.
        /// </summary>
        [Fact]
        public void LoadEditWithoutChangesRebuildSaveReload_LeavesTheBomIdentical()
        {
            var catalog = Catalog;
            var resolver = new PushBackResolver(catalog);
            var assembler = new PushBackEditorDesignAssembler(catalog);

            var design = Design(new[] { 4.5, 3.5 }, new[] { 6.0, 4.5 });
            var before = IntermediateSignature(resolver.Resolve(design), catalog);

            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(design, resolver);     // RACKEDITAR
            var rebuilt = assembler.BuildDesign(state, inputs);      // Actualizar, sin tocar nada

            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(rebuilt));
            var reloaded = JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();
            var after = IntermediateSignature(resolver.Resolve(reloaded), catalog);

            Assert.Equal(before, after);
        }

        /// <summary>
        /// La otra mitad del incidente: cambiar el valor a mano y RESTAURARLO. Tampoco normaliza — el BOM vuelve a
        /// ser exactamente el mismo, defecto incluido. Por tanto la normalizacion que el dueño observo no puede
        /// explicarse por este ciclo en un rack de un solo sentido.
        /// </summary>
        [Fact]
        public void ChangingAValueAndRestoringIt_LeavesTheBomIdentical()
        {
            var catalog = Catalog;
            var resolver = new PushBackResolver(catalog);
            var assembler = new PushBackEditorDesignAssembler(catalog);

            var design = Design(new[] { 4.5, 3.5 }, new[] { 6.0, 4.5 });
            var before = IntermediateSignature(resolver.Resolve(design), catalog);

            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(design, resolver);
            var cell = state.Structure.Fronts[0].Cells[0];
            var original = cell.IntermediateBeamDepth;
            Assert.Equal(4.5, original, 3);

            cell.IntermediateBeamDepth = 6.0;                        // el usuario lo cambia
            assembler.BuildDesign(state, inputs);
            cell.IntermediateBeamDepth = original;                   // y lo restaura
            var rebuilt = assembler.BuildDesign(state, inputs);

            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(rebuilt));
            var reloaded = JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();

            Assert.Equal(before, IntermediateSignature(resolver.Resolve(reloaded), catalog));
        }
    }
}
