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

            var design = assembler.BuildDesign(state.SideA, inputs, forceRebuild);
            design.SideB = state.BuildSideB();
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

            return design;
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
