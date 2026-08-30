using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// The single, pure authority for Push Back safety selections. Push Back admits every applicable safety family EXCEPT
    /// entrance GUIDES (GUIA) and walk grids (PARRILLA), and only at the LOW (entrance/exit) end — never the rear. This one
    /// implementation is shared by <see cref="PushBackResolver"/> and <see cref="PushBackEditorDesignAssembler.AuthorizedSafety"/>,
    /// so there are never two divergent copies of the restriction: it drops the unsupported families, deep-copies each
    /// surviving selection, and normalizes it to the low end (Side = Left, per-post side overrides cleared, the rear defensa
    /// length zeroed). Because every consumer (resolver, system, plans, drawing, BOM) reads ONLY <see cref="Authorize"/>'s
    /// output, an unsupported selection persisted by an OLDER document is read back without error but never reaches a plan,
    /// a view or the BOM (PB-VAL-06): it is stripped at build, exactly as GUIA already was — no destructive migration. The
    /// input collection and its selections are never mutated.
    /// </summary>
    /// <summary>
    /// Cuantos PASILLOS de carga tiene un Push Back, que es lo que decide donde va su seguridad.
    /// </summary>
    public enum PushBackSafetyAisles
    {
        /// <summary>Uno solo, en el extremo bajo. Es todo rack de un sentido, y el comportamiento de siempre.</summary>
        NearOnly = 0,

        /// <summary>Los DOS extremos son cara de carga: todo rack compuesto.</summary>
        Both = 1
    }

    public sealed class PushBackSafetyAuthority
    {
        private readonly RackCatalog catalog;

        public PushBackSafetyAuthority(RackCatalog catalog)
        {
            this.catalog = catalog ?? new RackCatalog();
        }

        /// <summary>True when the selection's catalog element is an entrance guide (type GUIA) — never admitted by Push Back.</summary>
        public bool IsEntranceGuide(SelectiveSafetySelection selection)
            => IsFamily(selection, SelectiveSafetyDefaults.GuiaType);

        /// <summary>True when the selection's catalog element is a walk grid (type PARRILLA) — never admitted by Push Back (PB-VAL-06).</summary>
        public bool IsRearGrid(SelectiveSafetySelection selection)
            => IsFamily(selection, SelectiveSafetyDefaults.ParrillaType);

        /// <summary>True when the selection's catalog element is of the TOPE family. Owner decision (2026-07-24): in Push Back
        /// the rear stop belongs to the HIGH end and is owned by the rear-tope config (SAQUE + per-cell deactivations),
        /// so it must never travel as ordinary low-end SAFETY — that would give one physical piece two authorities.</summary>
        public bool IsRearStop(SelectiveSafetySelection selection)
            => IsFamily(selection, SelectiveSafetyDefaults.TopeType);

        /// <summary>The canonical Push Back exclusion: a GUIA (entrance guide), a PARRILLA (walk grid) or a TOPE (owned by
        /// the rear-tope config) is never admitted as safety, on either end. Every downstream consumer reads only
        /// <see cref="Authorize"/>'s output, so this one predicate is the single authority for what Push Back refuses.</summary>
        public bool IsUnsupported(SelectiveSafetySelection selection)
            => IsEntranceGuide(selection) || IsRearGrid(selection) || IsRearStop(selection);

        private bool IsFamily(SelectiveSafetySelection selection, string type)
        {
            if (selection == null || string.IsNullOrWhiteSpace(selection.ElementId))
            {
                return false;
            }

            var element = catalog?.SafetyElements?.FirstOrDefault(entry => entry != null
                && string.Equals(entry.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase));
            return element != null && SelectiveSafetyDefaults.IsType(element.Type, type);
        }

        /// <summary>
        /// The authorized, low-end-only safety set: deep copies of the GUIA/PARRILLA-free selections, each restricted to the
        /// low end. Independent of the source (the input collection and its selections are never mutated).
        /// </summary>
        public IReadOnlyList<SelectiveSafetySelection> Authorize(IEnumerable<SelectiveSafetySelection> source)
            => Authorize(source, PushBackSafetyAisles.NearOnly);

        /// <summary>
        /// La misma autorizacion, declarando CUANTOS PASILLOS tiene el rack.
        ///
        /// <para>
        /// I-42 — un Push Back compuesto tiene DOS pasillos de carga, uno por lado, y los dos son extremos BAJOS: no
        /// hay ningun extremo alto donde la seguridad estorbe. Por eso un rack de dos sentidos coloca su seguridad
        /// en los dos, exactamente como lo harian dos Push Back opuestos. Un rack de un sentido sigue teniendo un
        /// solo pasillo y su comportamiento no cambia en nada.
        /// </para>
        /// </summary>
        public IReadOnlyList<SelectiveSafetySelection> Authorize(
            IEnumerable<SelectiveSafetySelection> source, PushBackSafetyAisles aisles)
        {
            var result = new List<SelectiveSafetySelection>();
            foreach (var selection in source ?? Enumerable.Empty<SelectiveSafetySelection>())
            {
                if (selection == null || IsUnsupported(selection))
                {
                    continue;
                }

                var copy = selection.DeepCopy();
                RestrictToAisles(copy, aisles, keepChosenSide: IsFamily(selection, SelectiveSafetyDefaults.BotaType));
                result.Add(copy);
            }

            return result;
        }

        /// <summary>
        /// Restringe una seleccion a los pasillos que el rack REALMENTE tiene. Muta la COPIA, nunca el origen.
        ///
        /// <para>
        /// <see cref="PushBackSafetyAisles.NearOnly"/> es la regla de siempre —un solo pasillo, el del extremo bajo—
        /// y deja el comportamiento legacy intacto. <c>Both</c> es la de I-42: las dos caras de un rack compuesto.
        /// </para>
        /// </summary>
        /// <param name="keepChosenSide">
        /// I-42 (ronda 6E): si el LADO elegido por el usuario debe conservarse. Lo pide la familia cuyo lado es
        /// ORIENTACION —el protector de bota, cuyo selector ofrece Ninguno/Izquierda/Derecha/Ambas—, no la que lo
        /// usa como «que pasillo». Sin el, el comportamiento es exactamente el anterior.
        /// </param>
        public static void RestrictToAisles(
            SelectiveSafetySelection selection, PushBackSafetyAisles aisles, bool keepChosenSide = false)
        {
            if (selection == null)
            {
                return;
            }

            // La restriccion de extremo bajo se aplica SIEMPRE: es la regla de Push Back y es la que conserva la
            // pertenencia y la orientacion de cada familia. Lo unico que añade un rack compuesto es que su extremo
            // lejano TAMBIEN es un pasillo, y eso viaja en su propio eje.
            //
            // NO se toca Side. Escribir el lado para decir «dos pasillos» apagaba las reglas adaptativas —que solo
            // valen cuando el usuario no ha elegido lado— y el protector lateral acababa en TODOS los postes y por
            // duplicado. Pertenencia, orientacion y extremo son tres ejes y ninguno puede hablar por otro.
            RestrictToLowEnd(selection, keepChosenSide);
            selection.BothEndsAreLoadFaces = aisles == PushBackSafetyAisles.Both;
        }

        /// <summary>
        /// The safety a BRAND-NEW Push Back system opens with (PB-VAL-04: it used to open with none, so a new rack drew no
        /// safety at all). It is the SAME catalog-driven authority the dynamic editor seeds a new rack from
        /// (<see cref="DynamicSafetyDefaults.Build"/>) run through <see cref="Authorize"/> — so the GUIA the dynamic default
        /// set includes is dropped and every surviving family is restricted to the LOW end. No hard-coded family list, and
        /// the shared defaults are never mutated (<see cref="DynamicSafetyDefaults.Build"/> mints fresh selections and
        /// <see cref="Authorize"/> deep-copies them again).
        /// </summary>
        public IReadOnlyList<SelectiveSafetySelection> Defaults()
        {
            var defaults = Authorize(DynamicSafetyDefaults.Build(catalog));

            // I-42 (ronda 6E) — el DEFECTO de la bota en Push Back sigue siendo IZQUIERDA, que es exactamente lo que
            // un rack nuevo dibujaba antes de esta ronda: la autorizacion colapsaba el «Ambas» que siembra el
            // Dinamico. Ahora ese colapso ya no se aplica a esta familia —para que el selector funcione— asi que el
            // valor por defecto se declara AQUI, donde vive el arranque de un rack nuevo, en vez de imponerse a
            // cualquier eleccion del usuario. Un rack existente y uno nuevo dibujan igual que siempre; lo que cambia
            // es que elegir Derecha o Ambas ahora significa algo.
            foreach (var selection in defaults)
            {
                if (IsFamily(selection, SelectiveSafetyDefaults.BotaType) && selection.Side == SafetySide.Both)
                {
                    selection.Side = SafetySide.Left;
                }
            }

            return defaults;
        }

        /// <summary>
        /// Restrict a safety selection to the LOW (entrance/exit) end only. Mutates the passed COPY, never the source.
        ///
        /// Owner-validation round 1 (I-32): esto ya NO borra <see cref="SelectiveSafetySelection.PostSides"/>. Esa lista
        /// es la matriz POR POSTE —qué postes llevan la pieza y con qué orientación— y borrarla para imponer el extremo
        /// bajo destruía la elección del usuario: el rack dibujaba en todos los postes o en ninguno. La pertenencia y el
        /// extremo son ejes ORTOGONALES; el extremo se impone donde se decide, con
        /// <see cref="SelectiveSafetyEnds.EndsForPost"/>, que lee la marca <see cref="SelectiveSafetySelection.LowEndOnly"/>
        /// de abajo. Aquí solo se colapsa el lado GENERAL (el valor por defecto de los postes sin entrada propia).
        /// </summary>
        public static void RestrictToLowEnd(SelectiveSafetySelection selection, bool keepChosenSide = false)
        {
            if (selection == null)
            {
                return;
            }

            // I-42 (ronda 6E) — el colapso del LADO deja de aplicarse a la familia que lo usa como ORIENTACION.
            //
            // Escribir Left sobre Both y Right «para imponer el extremo bajo» borraba la eleccion del usuario: en un
            // Push Back las tres opciones del selector de BOTAS producian exactamente la misma pieza. Para esa
            // familia el extremo ya lo impone LowEndOnly, mas abajo, que SelectiveSafetyEnds lee sin tocar la
            // orientacion — pertenencia, orientacion y extremo son tres ejes.
            //
            // Las demas familias conservan el colapso: el desviador, la defensa y el protector lateral leen Side por
            // su cuenta y con otro significado, y su geometria esta validada por el dueño desde la ronda 1.
            if (!keepChosenSide && (selection.Side == SafetySide.Both || selection.Side == SafetySide.Right))
            {
                selection.Side = SafetySide.Left;
            }

            // La matriz por poste se conserva VERBATIM: es pertenencia y orientación, no extremo.

            // PB-009 (I-32): mark the selection itself, so the ADAPTIVE defaults of every family stop reaching the far
            // end. Zeroing the stored records was not enough — a brand-new rack carries NO records at all, so the
            // forklift defence fell straight through to the 12/36 default and drew a rear piece in lateral, planta and
            // the BOM, and the lateral guard's adaptive rule put a guard on the last post's far face.
            //
            // It is a DEFAULT, not a prohibition: an end the user explicitly set is honoured, which is why the stored
            // entrance lengths are no longer wiped. What IS cleared is the AUTO flag of that end — an automatic far end
            // has no meaning here and would resolve back to 12/36.
            selection.LowEndOnly = true;

            // PB-002 (I-32): the desviador grid Push Back shows has one column per POST, so its off-cells are keyed by
            // post. Marking the selection is what makes the frontal, the planta and the BOM read the same cell the
            // lateral does — before this they collapsed the last two columns with a Math.Min onto the last front.
            selection.DesviadorCellsAreByPost = true;
            foreach (var post in selection.DefensaPosts)
            {
                if (post != null)
                {
                    post.EntranceAuto = false;
                }
            }
        }
    }
}
