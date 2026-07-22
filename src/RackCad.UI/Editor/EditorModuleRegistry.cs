using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems;

namespace RackCad.UI.Editor
{
    /// <summary>
    /// The single, explicit list of editor modules the menu and the design library consume (initiative I-15), built the
    /// same way as the Application <c>SystemRegistry</c> (I-08): from an ordered set of <see cref="IRackEditorModule"/>,
    /// rejecting duplicate kinds, keeping a stable order, resolving by <see cref="RackSystemKind"/> and failing explicitly
    /// for an unregistered kind. No reflection, no assembly scanning. The <see cref="Default"/> instance lists the five
    /// modules that exist today, in the menu's button order. This registry is a UI concern and is distinct from the
    /// Application <c>SystemRegistry</c> (persistence) and the Plugin <c>KindHandlerRegistry</c> (drawing/BOM/restamp).
    /// </summary>
    public sealed class EditorModuleRegistry
    {
        private readonly IReadOnlyList<IRackEditorModule> modules;
        private readonly Dictionary<RackSystemKind, IRackEditorModule> byKind;

        public EditorModuleRegistry(IEnumerable<IRackEditorModule> modules)
        {
            if (modules == null)
            {
                throw new ArgumentNullException(nameof(modules));
            }

            // Copy defensively so the registry cannot change after construction, even if the caller mutates the source.
            var ordered = new List<IRackEditorModule>();
            var map = new Dictionary<RackSystemKind, IRackEditorModule>();
            foreach (var module in modules)
            {
                if (module == null)
                {
                    throw new ArgumentException("La lista de módulos no puede contener nulos.", nameof(modules));
                }

                if (map.ContainsKey(module.Kind))
                {
                    throw new ArgumentException(
                        "Módulo de editor duplicado para el tipo '" + module.Kind + "'.", nameof(modules));
                }

                map.Add(module.Kind, module);
                ordered.Add(module);
            }

            this.modules = ordered.AsReadOnly(); // ReadOnlyCollection: no cast-to-mutable path
            this.byKind = map;
        }

        /// <summary>The registered modules in their stable registration order (the menu's button order).</summary>
        public IReadOnlyList<IRackEditorModule> Modules => modules;

        /// <summary>The module for <paramref name="kind"/>; throws if that kind is not registered.</summary>
        public IRackEditorModule Get(RackSystemKind kind)
        {
            if (byKind.TryGetValue(kind, out var module))
            {
                return module;
            }

            throw new InvalidOperationException(
                "No hay un módulo de editor registrado para el tipo '" + kind + "'.");
        }

        /// <summary>Non-throwing lookup: true and the module when <paramref name="kind"/> is registered.</summary>
        public bool TryGet(RackSystemKind kind, out IRackEditorModule module)
            => byKind.TryGetValue(kind, out module);

        /// <summary>
        /// Resolves which module opens a library <paramref name="project"/> loaded from <paramref name="entry"/>,
        /// reproducing the old <c>OpenDesignLibrary_Click</c> if/else EXACTLY: the kind-specific modules
        /// (<see cref="IRackEditorModule.IsLibraryFallback"/> false) are tried first, then the fallback module (the
        /// cabecera, matched by <c>project.Header != null</c>). Returns null when nothing matches — the caller then shows
        /// "El diseño seleccionado no se pudo interpretar.". The kind-specific matches are mutually exclusive by
        /// <see cref="RackDesignLibraryEntry.Kind"/>, so their relative order does not change the result; only the
        /// fallback being last does, which this guarantees.
        /// </summary>
        public IRackEditorModule ResolveForLibrary(RackDesignLibraryEntry entry, RackProject project)
        {
            if (project == null)
            {
                return null;
            }

            foreach (var module in modules)
            {
                if (!module.IsLibraryFallback && module.MatchesLibrary(entry, project))
                {
                    return module;
                }
            }

            foreach (var module in modules)
            {
                if (module.IsLibraryFallback && module.MatchesLibrary(entry, project))
                {
                    return module;
                }
            }

            return null;
        }

        /// <summary>
        /// The five modules that exist today, in the menu's button order: selectivo, dinámico, cabecera, cama, larguero.
        /// Explicit construction — a new system registers here, not by editing the menu window.
        /// </summary>
        public static EditorModuleRegistry Default { get; } = new EditorModuleRegistry(new IRackEditorModule[]
        {
            new SelectiveEditorModule(),
            new DynamicEditorModule(),
            new HeaderEditorModule(),
            new FlowBedEditorModule(),
            new LargueroEditorModule(),
        });
    }
}
