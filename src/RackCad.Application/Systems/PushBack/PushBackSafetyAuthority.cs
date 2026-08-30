using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
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
                RestrictToAisles(copy, aisles);

                // I-42 (S1B) — el AUTOMATICO de la bota lo decide el rack: un solo pasillo protege su frente
                // operativo; dos pasillos protegen los dos. Es el DEFECTO, no una imposicion: una eleccion
                // explicita del usuario manda siempre y puede pedir la posterior aunque no se cargue por ella.
                if (IsBoot(copy))
                {
                    copy.AutomaticBootPlacement = aisles == PushBackSafetyAisles.Both
                        ? BootPlacement.Both
                        : BootPlacement.EntryExit;
                }

                result.Add(copy);
            }

            return result;
        }

        /// <summary>
        /// I-42 (S1C, contrato del dueño) — declara en que LINEAS el automatico de la bota no coloca nada porque el
        /// rack esta EN BLANCO ahi.
        ///
        /// <para>
        /// Es la MISMA condicion fisica que la ronda 6D declaro para la defensa y la 6F para la bota: la cobertura
        /// de esa linea se acorta hasta la INTERFAZ entre los dos lados, que no es una cara de carga sino el otro
        /// lado. La correccion de esta ronda es que eso apaga el automatico de la linea ENTERA: retirar solo la
        /// cara que falta dejaba la otra en pie, y un blanco acababa eligiendo «posterior» sin que nadie lo pidiera.
        /// </para>
        /// <para>
        /// Se declara AQUI, en la unica autoridad de seguridad del sistema, para que la planta, los dos frontales y
        /// el BOM lean todos la misma resolucion. No decide nada sobre los postes que el usuario configuro: esos se
        /// resuelven antes, en <see cref="SelectiveSafetySelection.BootPlacementAt"/>. Es DERIVADA y se vuelve a
        /// imponer entera en cada resolucion, asi que al quitar el blanco la linea recupera sola lo que herede.
        /// </para>
        /// </summary>
        public void DeclareBlankLines(
            IReadOnlyCollection<int> blankLines, IEnumerable<SelectiveSafetySelection> selections)
        {
            foreach (var selection in selections ?? Enumerable.Empty<SelectiveSafetySelection>())
            {
                if (selection == null || !IsBoot(selection))
                {
                    continue;
                }

                selection.PostsWithoutAutomaticBoot.Clear();
                foreach (var line in blankLines ?? (IReadOnlyCollection<int>)Array.Empty<int>())
                {
                    selection.PostsWithoutAutomaticBoot.Add(line);
                }
            }
        }

        /// <summary>
        /// Las LINEAS que un frente en blanco dejo sin pasillo propio, medidas UNA vez sobre la estructura completa.
        /// Es la unica medicion: los cortes de cada lado la reciben ya hecha, porque desde el lado que sigue lleno
        /// el blanco del contrario no se ve —y es justo esa linea la que hay que apagar—.
        /// </summary>
        public IReadOnlyCollection<int> BlankLines(DynamicRackSystem structure)
        {
            var lines = new List<int>();
            var layout = structure == null ? null : DynamicFrontGeometry.Compute(structure, catalog);
            if (layout?.PostPositions == null)
            {
                return lines;
            }

            for (var postIndex = 0; postIndex < layout.PostPositions.Count; postIndex++)
            {
                if (IsBlankLine(structure, postIndex))
                {
                    lines.Add(postIndex);
                }
            }

            return lines;
        }

        /// <summary>
        /// True cuando la cobertura de esa linea llega a la INTERFAZ entre los dos lados, que es lo que deja un
        /// frente en blanco. Se mide sobre los MISMOS extremos de profundidad que usa la planta para colocar las
        /// dos copias, asi que la declaracion y el dibujo no pueden discrepar.
        /// </summary>
        private static bool IsBlankLine(DynamicRackSystem structure, int postIndex)
        {
            if (!DynamicFrontActivation.BoundaryExists(structure, postIndex))
            {
                return false;   // la frontera no existe: no hay nada que declarar, ya no se coloca nada (I-33)
            }

            var depthRange = DynamicDepthGeometry.AtPost(structure, postIndex);
            var rangeStart = structure.Modules
                .FirstOrDefault(module => module.Index + 1 == depthRange.StartPosition)?.StartX ?? 0.0;
            var rangeEnd = structure.Modules
                .FirstOrDefault(module => module.Index + 1 == depthRange.EndPosition)?.EndX ?? structure.TotalLength;
            return structure.IsInteriorFace(rangeStart) || structure.IsInteriorFace(rangeEnd);
        }

        /// <summary>Si la seleccion es de la familia BOTA, segun el catalogo de este rack.</summary>
        private bool IsBoot(SelectiveSafetySelection selection)
        {
            var element = catalog?.SafetyElements?.FirstOrDefault(entry =>
                entry != null && string.Equals(entry.Id, selection?.ElementId, StringComparison.OrdinalIgnoreCase));
            return element != null && SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.BotaType);
        }

        /// <summary>
        /// Restringe una seleccion a los pasillos que el rack REALMENTE tiene. Muta la COPIA, nunca el origen.
        ///
        /// <para>
        /// <see cref="PushBackSafetyAisles.NearOnly"/> es la regla de siempre —un solo pasillo, el del extremo bajo—
        /// y deja el comportamiento legacy intacto. <c>Both</c> es la de I-42: las dos caras de un rack compuesto.
        /// </para>
        /// </summary>
        public static void RestrictToAisles(SelectiveSafetySelection selection, PushBackSafetyAisles aisles)
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
            RestrictToLowEnd(selection);
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
        public IReadOnlyList<SelectiveSafetySelection> Defaults() => Authorize(DynamicSafetyDefaults.Build(catalog));

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
        public static void RestrictToLowEnd(SelectiveSafetySelection selection)
        {
            if (selection == null)
            {
                return;
            }

            // I-42 (S1) — la eleccion del usuario se CONSERVA antes de colapsar el lado general. El colapso
            // sigue siendo necesario para las familias que leen el lado como orientacion o como extremo —el
            // protector lateral y el desviador, los dos con contrato validado—, pero destruia la unica informacion
            // que la BOTA necesita: QUE CARA DE ATAQUE proteger. Con el lado colapsado, sus tres opciones daban
            // exactamente la misma bota.
            selection.AuthoredSide = selection.AuthoredSide ?? selection.Side;
            if (selection.Side == SafetySide.Both || selection.Side == SafetySide.Right)
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
            // entrance lengths are no longer wiped.
            selection.LowEndOnly = true;

            // PB-002 (I-32): the desviador grid Push Back shows has one column per POST, so its off-cells are keyed by
            // post. Marking the selection is what makes the frontal, the planta and the BOM read the same cell the
            // lateral does — before this they collapsed the last two columns with a Math.Min onto the last front.
            selection.DesviadorCellsAreByPost = true;

            // I-42 (ronda 7C) — el extremo LEJANO conserva su marca de automatico. Aqui se le borraba, porque
            // «un extremo lejano automatico no significa nada y volveria a 12/36»: eso era cierto antes de que
            // PB-009 llegara al plan, pero hoy <see cref="DynamicForkliftDefensePlan"/> ya resuelve el automatico
            // lejano a CERO en cuanto la seleccion lleva LowEndOnly, asi que borrarlo no defiende de nada — y en un
            // rack COMPUESTO, donde ese extremo es un pasillo de verdad (6D), lo convertia en un cero explicito.
            // El efecto se veia al apagar un poste y volver a encenderlo: la cara lejana no volvia.
        }
    }
}
