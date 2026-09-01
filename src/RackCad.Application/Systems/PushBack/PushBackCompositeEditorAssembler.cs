using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — el ensamblador del editor COMPUESTO. Compone al de un solo sentido, no lo sustituye:
    ///
    /// <list type="number">
    /// <item>el lado A y la ESTRUCTURA FISICA compartida los arma
    /// <see cref="PushBackEditorDesignAssembler"/> tal cual — con sus cabeceras, sus modulos, su seguridad y sus
    /// anotaciones—, asi que un rack sin lado B produce EXACTAMENTE el mismo diseno que antes de la iniciativa;</item>
    /// <item>encima se anaden la configuracion funcional del lado B y la intencion de la interfaz.</item>
    /// </list>
    ///
    /// <para>
    /// Las entradas del rack (tarima, poste, peralte, seguridad, anotaciones) son UNAS: pertenecen al rack, no a un
    /// lado. Por eso hay un solo <see cref="PushBackEditorInputs"/> y el lado B no puede declarar una estructura
    /// distinta de la del rack.
    /// </para>
    /// </summary>
    public sealed class PushBackCompositeEditorAssembler
    {
        private readonly PushBackEditorDesignAssembler assembler;

        public PushBackCompositeEditorAssembler(RackCatalog catalog)
        {
            assembler = new PushBackEditorDesignAssembler(catalog ?? new RackCatalog());
        }

        /// <summary>El ensamblador de un solo sentido, para que el editor siga usandolo donde ya lo usaba.</summary>
        public PushBackEditorDesignAssembler SideAssembler => assembler;

        /// <summary>Arma el diseno canonico del rack compuesto, sin resolver.</summary>
        public PushBackDesign BuildDesign(
            PushBackCompositeEditorState state, PushBackEditorInputs inputs, bool forceRebuild = false)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            // La compositividad EFECTIVA se decide ANTES de armar: es la que dice que secuencia se entrega y sobre
            // cual informa la reconciliacion. Se calcula con la misma pregunta que hara el resolver.
            var sideB = state.BuildSideB();
            var effectivelyComposite = sideB != null && sideB.IsPresent;
            if (!effectivelyComposite)
            {
                ParkTailOf(state);
            }

            var design = assembler.BuildDesign(
                state.SideA,
                inputs,
                forceRebuild,
                effectivelyComposite,
                effectivelyComposite ? state.DormantCompositeTail.ToList() : null);
            design.SideB = sideB;
            design.Composite = state.BuildComposite();

            // Las ranuras que el usuario retiro del lado A se DECLARAN ausentes; no se borran de la lista. Borrarlas
            // desplazaria los indices de todas las siguientes —y con ellos la topologia, los topes y la
            // correspondencia con el lado B—, ademas de destruir su configuracion, que debe quedar dormante.
            foreach (var slot in state.AbsentSlotsOfA())
            {
                if (!design.Composite.AbsentSlotsA.Contains(slot))
                {
                    design.Composite.AbsentSlotsA.Add(slot);
                }
            }

            // El lado B declara las suyas por el MISMO camino: su frente viaja completo y aqui se dice cuales no
            // almacenan. Antes lo decia una entrada nula, que se llevaba por delante la declaracion fisica.
            foreach (var slot in state.AbsentSlotsOfB())
            {
                if (!design.Composite.AbsentSlotsB.Contains(slot))
                {
                    design.Composite.AbsentSlotsB.Add(slot);
                }
            }

            ApplyEffectiveCompositeness(state, design);
            return design;
        }

        /// <summary>
        /// I-42 (A4-MOD-LIFECYCLE, contrato del dueño) — LA SECUENCIA QUE SE ENTREGA ES LA DEL RACK QUE SE RESUELVE
        /// AHORA.
        ///
        /// <para>
        /// <b>Tres cosas distintas.</b> La CAPACIDAD compuesta —el editor conoce el lado B y guarda su intencion
        /// dormida—, el diseño EFECTIVAMENTE compuesto —el que ahora mismo tiene dos lados— y la SECUENCIA
        /// persistida —<c>M* + GAP + B:*</c>—. La unica pregunta que decide por que camino se resuelve un diseño es
        /// <see cref="PushBackDesign.IsComposite"/>, la misma que usa el resolver; ni el selector de lado, ni el
        /// numero de modulos, ni haber tenido lado B alguna vez.
        /// </para>
        ///
        /// <para>
        /// <b>El defecto.</b> Cuando el rack dejaba de ser efectivamente compuesto —se retira el lado B, o se queda
        /// sin ninguna ranura efectiva, que no es lo mismo— la secuencia COMPUESTA seguia viajando en el diseño y
        /// llegaba al resolver de un solo sentido. Alli la guarda «los modulos deben ser tantos como posiciones»
        /// actuaba de segunda autoridad accidental y reconstruia todo: medido, la personalizacion del lado A se
        /// perdia en ese mismo recalculo (M2 volvia de 30" a 48"), el informe declaraba «conservados» modulos que el
        /// diseño resuelto ya no tenia, y al reactivar el lado B su mitad volvia estandar.
        /// </para>
        ///
        /// <para>
        /// <b>La correccion.</b> La cola —hueco y mitad B— se APARCA en el estado compuesto y se retira de la
        /// secuencia entregada; cuando el rack vuelve a tener dos lados, se devuelve. Dormir deja de destruir: lo
        /// que cambia es lo que se resuelve, no lo que el usuario declaro.
        /// </para>
        /// </summary>
        private static void ApplyEffectiveCompositeness(PushBackCompositeEditorState state, PushBackDesign design)
        {
            var modules = design?.Structure?.Modules;
            if (modules == null)
            {
                return;
            }

            var lines = design.Structure.HeaderLineOverrides;
            if (!design.IsComposite)
            {
                // La secuencia entregada ya es la del lado A: aqui solo se retira lo que hubiera quedado y se
                // aparcan las configuraciones por linea de la cola, que viajan con ella.
                foreach (var module in modules
                    .Where(module => module != null
                        && PushBackCompositeStructure.IsCompositeTailId(module.ModuleId))
                    .ToList())
                {
                    modules.Remove(module);
                }

                var tailLines = lines
                    .Where(line => line != null && PushBackCompositeStructure.IsCompositeTailId(line.ModuleId))
                    .ToList();
                foreach (var line in tailLines)
                {
                    lines.Remove(line);
                }

                if (tailLines.Count > 0)
                {
                    state.ParkDormantTail(state.DormantCompositeTail.ToList(), tailLines);
                }

                return;
            }

            // Vuelve a haber dos lados: los modulos de la cola ya volvieron con el armado; aqui vuelven sus
            // configuraciones por LINEA, que viajan con ella y con su misma vigencia.
            foreach (var line in state.DormantTailLineOverrides)
            {
                if (!lines.Any(existing => existing != null
                    && existing.PostIndex == line.PostIndex
                    && string.Equals(existing.ModuleId, line.ModuleId, StringComparison.Ordinal)))
                {
                    lines.Add(line);
                }
            }

            // I-42 (A5-WIRE / A4V-2): armar solo MARCA el consumo. La cola se retira cuando la computacion que la
            // desperto se acepta; un recalculo que falle despues de esto la deja intacta para el siguiente intento.
            state.MarkDormantTailConsumed();
        }

        /// <summary>
        /// Aparca la COLA vigente —la del baseline del rack, que es donde vive la secuencia— antes de armar un
        /// diseño de un solo sentido. Si no hay cola que aparcar, la aparcada anterior se conserva: dormir dos
        /// veces seguidas no puede borrar lo que la primera guardo.
        /// </summary>
        private static void ParkTailOf(PushBackCompositeEditorState state)
        {
            var baseline = state.SideA?.WorkingBaseline?.Structure;
            var tail = PushBackCompositeStructure.CompositeTail(baseline);
            if (tail.Count == 0)
            {
                return;
            }

            var lines = baseline.HeaderLineOverrides
                .Where(line => line != null && PushBackCompositeStructure.IsCompositeTailId(line.ModuleId))
                .ToList();
            state.ParkDormantTail(tail.Select(PushBackCompositeStructure.ToTailDesign).ToList(), lines);
        }

        /// <summary>
        /// Arma, resuelve UNA vez y devuelve el sistema con sus diagnosticos. Un fallo no deja el estado a medias:
        /// devuelve una computacion invalida con el mensaje y la geometria nula, igual que el camino de un sentido.
        /// </summary>
        public PushBackCompositeComputation Build(
            PushBackCompositeEditorState state, PushBackEditorInputs inputs, RackCatalog catalog, bool forceRebuild = false)
        {
            // Una INTENCION invalida no se resuelve: se declara. Resolverla obligaria a interpretar el valor como
            // otro, que es justo lo que no puede pasar.
            var intent = state?.IntentDiagnostics() ?? new List<PushBackCompositeDiagnostic>();
            if (intent.Any(diagnostic => diagnostic.IsBlocking))
            {
                return new PushBackCompositeComputation(null, null, intent, intent.First().Message);
            }

            try
            {
                var design = BuildDesign(state, inputs, forceRebuild);
                var system = new PushBackResolver(catalog ?? new RackCatalog()).Resolve(design);
                return new PushBackCompositeComputation(
                    design, system, PushBackCompositeDiagnostics.Evaluate(system).Concat(intent).ToList(), null);
            }
            catch (Exception error)
            {
                return new PushBackCompositeComputation(null, null, new List<PushBackCompositeDiagnostic>(), error.Message);
            }
        }
    }

    /// <summary>El resultado de una recomputacion del editor compuesto.</summary>
    public sealed class PushBackCompositeComputation
    {
        public PushBackCompositeComputation(
            PushBackDesign design,
            PushBackSystem system,
            IReadOnlyList<PushBackCompositeDiagnostic> diagnostics,
            string error)
        {
            Design = design;
            System = system;
            Diagnostics = diagnostics ?? new List<PushBackCompositeDiagnostic>();
            Error = error;
        }

        public PushBackDesign Design { get; }
        public PushBackSystem System { get; }
        public IReadOnlyList<PushBackCompositeDiagnostic> Diagnostics { get; }

        /// <summary>El mensaje del fallo, o null si la recomputacion produjo un sistema.</summary>
        public string Error { get; }

        public bool IsValid => System != null && string.IsNullOrEmpty(Error);

        /// <summary>True cuando alguna celda no es construible: el editor lo muestra y no deja insertar a ciegas.</summary>
        public bool HasBlocking => Diagnostics.Any(diagnostic => diagnostic.IsBlocking);
    }
}
