using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T0 (CT-PB0-10) — CARACTERIZACION de la decision pura detras del borrado de vistas: que cortes
    /// laterales existen despues de cada edicion de frentes y cuales de las vistas de ANTES quedan obsoletas segun la
    /// disponibilidad de DESPUES. Es la misma decision que RACKEDITAR toma con los cortes y los hechos, calculada aqui con el
    /// producto real y sin Plugin, sin ventana y sin AutoCAD.
    ///
    /// <para>Se fija lo que HAY. El orden es ordinal y solo se fijan enteros, cadenas y booleanos.</para>
    /// </summary>
    public sealed class PushBackFullEditorCharacterizationLateralCutTests
    {
        private static readonly RackCatalog Catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string Fixture(string name)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.True(dir != null, "repo root (RackCad.sln) not found");
            var path = Path.Combine(dir.FullName, "tests", "RackCad.Tests", "Fixtures", "I64", name + ".json");
            Assert.True(File.Exists(path), "missing fixture " + path);
            return File.ReadAllText(path);
        }

        /// <summary>El estado del editor en un instante: sus cortes, y si el diseño es compuesto.</summary>
        private sealed class Moment
        {
            public string Label;
            public List<int> PostIndexes;
            public bool IsComposite;
            public PushBackViewAvailabilityFacts Facts;
        }

        private static Moment Measure(string label, PushBackPilotDraftState state)
        {
            var design = new PushBackCompositeEditorAssembler(Catalog).BuildDesign(state.Editor, state.Inputs);
            var system = new PushBackResolver(Catalog).Resolve(design);
            var cortes = new PushBackSystemLateralBuilder().Cortes(system, Catalog);
            var postIndexes = cortes.Select(corte => corte.PostIndex).ToList();
            return new Moment
            {
                Label = label,
                PostIndexes = postIndexes,
                IsComposite = design.IsComposite,
                Facts = new PushBackViewAvailabilityFacts(postIndexes, design.IsComposite),
            };
        }

        private static PushBackViewAvailabilityFacts FactsOf(Moment moment) => moment.Facts;

        /// <summary>
        /// Una linea por par consecutivo: los PostIndex de antes y de despues, y por cada poste de ANTES el estado de su
        /// vista lateral frente a los hechos de DESPUES y si RACKEDITAR la declararia obsoleta (estado distinto de
        /// Available, o sin corte con ese PostIndex).
        /// </summary>
        private static string Pair(Moment before, Moment after)
        {
            var statuses = new List<string>();
            var obsolete = new List<string>();
            foreach (var post in before.PostIndexes)
            {
                var decoded = RackViewCodec.Decode(RackEmbedDocument.KindPushBack, RackEmbedDocument.ViewLateral, post);
                var status = RackViewAvailability.Evaluate(decoded, FactsOf(after)).Status;
                statuses.Add(post + ":" + status);
                if (status != RackViewAvailabilityStatus.Available || !after.PostIndexes.Contains(post))
                {
                    obsolete.Add(post.ToString());
                }
            }

            return before.Label + " -> " + after.Label
                + " | antes=[" + string.Join(",", before.PostIndexes) + "] despues=[" + string.Join(",", after.PostIndexes) + "]"
                + " | " + string.Join(",", statuses)
                + " | obsoletas=[" + string.Join(",", obsolete) + "]";
        }

        [Fact]
        public void CT_PB0_10_TheLateralCutsAndTheObsoleteViewsAfterEachFrontEditAreTheOnesThePureDecisionGives()
        {
            var materialized = PushBackPilotMaterializer.Materialize(Fixture("pb-01-base"), Catalog);
            Assert.Equal(PushBackPilotMaterializationKind.Admitted, materialized.Kind);
            var state = materialized.State;
            var editor = state.Editor;
            var count = editor.SlotCount;
            Assert.Equal(3, count);

            var moments = new List<Moment> { Measure("base", state) };

            // De un sentido: un frente mas, uno menos y dos frentes en blanco.
            editor.SetSlotCount(count + 1);
            moments.Add(Measure("+frente", state));
            editor.SetSlotCount(count);
            moments.Add(Measure("-frente", state));
            Assert.True(editor.SetSlotPresent(PushBackSide.A, 1, false));
            Assert.True(editor.SetSlotPresent(PushBackSide.A, 2, false));
            Assert.Equal(new[] { 1, 2 }, editor.AbsentSlotsOfA());
            moments.Add(Measure("frentes 1 y 2 en blanco", state));

            // Compuesta, sobre la ultima.
            editor.SetSideBPresent(true);
            moments.Add(Measure("declarar B", state));
            Assert.True(editor.SetSlotPresent(PushBackSide.B, 1, true));
            moments.Add(Measure("B ranura 1 activa", state));
            Assert.True(editor.SetSlotPresent(PushBackSide.B, 1, false));
            moments.Add(Measure("B ranura 1 en blanco", state));
            editor.SetSideBPresent(false);
            moments.Add(Measure("retirar B", state));

            var lines = new List<string>();
            for (var i = 0; i + 1 < moments.Count; i++) lines.Add(Pair(moments[i], moments[i + 1]));

            Assert.Equal(
                new[]
                {
                    "base -> +frente | antes=[0,1,2,3] despues=[0,1,2,3,4] | 0:Available,1:Available,2:Available,3:Available | obsoletas=[]",
                    "+frente -> -frente | antes=[0,1,2,3,4] despues=[0,1,2,3] | 0:Available,1:Available,2:Available,3:Available,4:VariantNotPresent | obsoletas=[4]",
                    "-frente -> frentes 1 y 2 en blanco | antes=[0,1,2,3] despues=[0,1,3] | 0:Available,1:Available,2:VariantNotPresent,3:Available | obsoletas=[2]",
                    "frentes 1 y 2 en blanco -> declarar B | antes=[0,1,3] despues=[0,1,3] | 0:Available,1:Available,3:Available | obsoletas=[]",
                    "declarar B -> B ranura 1 activa | antes=[0,1,3] despues=[0,1,2,3] | 0:Available,1:Available,3:Available | obsoletas=[]",
                    "B ranura 1 activa -> B ranura 1 en blanco | antes=[0,1,2,3] despues=[0,1,3] | 0:Available,1:Available,2:VariantNotPresent,3:Available | obsoletas=[2]",
                    "B ranura 1 en blanco -> retirar B | antes=[0,1,3] despues=[0,1,3] | 0:Available,1:Available,3:Available | obsoletas=[]",
                },
                lines);

            // Solo la composicion con ranuras activas en B hace compuesto al diseño (ANCLA, T-6: no la mueve ningun mutante).
            Assert.Equal(
                new[] { "base=False", "+frente=False", "-frente=False", "frentes 1 y 2 en blanco=False", "declarar B=False", "B ranura 1 activa=True", "B ranura 1 en blanco=False", "retirar B=False" },
                moments.Select(m => m.Label + "=" + m.IsComposite).ToArray());
        }
    }
}
