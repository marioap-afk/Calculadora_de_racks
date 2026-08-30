using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Controls;

namespace RackCad.UI.Systems.PushBack
{
    /// <summary>
    /// I-42 (S1E, contrato del dueño) — LOS PROTECTORES DE BOTA DE UN LADO, dentro de «Elementos de seguridad».
    ///
    /// <para>
    /// Es el mismo patron por lado que el dueño ya valido para los topes y para la defensa: un titulo, la eleccion
    /// general de ESE lado y un boton que abre su rejilla POR POSTE. En un rack compuesto hay dos secciones, A y B;
    /// en uno de un solo sentido, una sola y sin etiqueta — no se inventa un lado B que no existe.
    /// </para>
    /// <para>
    /// Antes habia UNA fila global. En un compuesto eso era ambiguo hasta para el codigo: la planta leia
    /// «Entrada/Salida» como la cara cercana del rack y el corte del lado B como su propia entrada, asi que la misma
    /// eleccion producia piezas distintas segun quien la mirara. Las palabras se leen DENTRO del lado, y por eso
    /// cada lado necesita su seccion.
    /// </para>
    /// </summary>
    internal sealed class PushBackBootSection
    {
        /// <summary>Las cuatro ubicaciones, con el nombre que la familia les da (I-42, S1B).</summary>
        public static readonly IReadOnlyList<string> ModeLabels =
            new[] { "Ninguno", "Entrada/Salida", "Posterior", "Ambas" };

        public const string HeadingText = "Protectores de bota (base de poste)";

        public const string PerPostButtonText = "Por poste…";

        /// <summary>x:Name-equivalente del boton, para que una prueba lo encuentre dentro del dialogo compuesto.</summary>
        public const string PerPostButtonName = "BootPerPostButton";

        /// <summary>x:Name-equivalente del selector general de este lado.</summary>
        public const string ModeBoxName = "BootModeBox";

        public const string ModeLabelText = "Ubicación";

        /// <summary>Etiqueta del selector de tipo de ESTE lado.</summary>
        public const string PieceLabelText = "Tipo de protector";

        /// <summary>x:Name-equivalente del selector de tipo.</summary>
        public const string PieceBoxName = "BootPieceBox";

        /// <summary>El texto de la opcion «sin bota» dentro del selector de tipo.</summary>
        public const string NoneOptionText = "Ninguno";

        /// <summary>Lo que la seccion muestra cuando el lado quedo sin pieza.</summary>
        public const string NoneStatusText = "Sin protectores de bota en este lado";

        private readonly TextBlock status;
        private readonly Func<PushBackBootSection, IReadOnlyList<BootPostPlacement>> openDialog;

        /// <param name="side">El lado cuyos postes configura esta seccion.</param>
        /// <param name="sideLabel">La etiqueta del lado, o null en un rack de un solo sentido.</param>
        /// <param name="automatic">
        /// Lo que este lado hace cuando nadie ha elegido y el rack todavia no lo ha resuelto —la primera vez que se
        /// abre la ventana, cuando aun no hay ninguna seleccion de bota—. Si la casilla mostrara otra cosa, elegir
        /// justo lo que se ve no se registraria como decision y el rack dibujaria algo distinto de lo mostrado.
        /// </param>
        /// <param name="current">La configuracion de ESE lado; la seccion trabaja sobre una COPIA.</param>
        /// <param name="legacyPosts">
        /// La MATRIZ HISTORICA por poste de un documento anterior a S1B, que guardaba la decision como lado. Se
        /// muestra aqui —es del lado unico/A— para que el usuario la vea y no quede una segunda autoridad invisible:
        /// al aceptar viaja ya como decision de este lado.
        /// </param>
        public PushBackBootSection(
            PushBackSide side,
            string sideLabel,
            SelectiveBotaConfig current,
            int postCount,
            Func<PushBackBootSection, IReadOnlyList<BootPostPlacement>> openDialog,
            IEnumerable<SafetyPostSide> legacyPosts = null,
            BootPlacement automatic = BootPlacement.EntryExit,
            IReadOnlyList<SafetyElementCatalogEntry> variants = null,
            string pieceId = null)
        {
            Side = side;
            SideLabel = sideLabel;
            PostCount = Math.Max(1, postCount);
            this.openDialog = openDialog ?? throw new ArgumentNullException(nameof(openDialog));

            // Una copia de trabajo: nada de lo que el usuario haga aqui toca el rack hasta que se acepta la ventana.
            var source = current?.DeepCopy() ?? new SelectiveBotaConfig();

            // La casilla muestra lo que ese lado hace HOY —su eleccion, o el automatico si nadie eligio—, pero
            // «mostrar» no es «elegir»: mientras el usuario no la toque, el lado sigue heredando su automatico y no
            // se congela una decision que nadie tomo.
            Explicit = source.Placement.HasValue;
            Placement = source.Placement ?? source.Automatic ?? automatic;
            Posts = source.Posts
                .Where(post => post != null && post.PostIndex >= 0 && post.PostIndex < PostCount)
                .Select(post => new BootPostPlacement { PostIndex = post.PostIndex, Placement = post.Placement })
                .ToList();

            // La matriz historica solo entra donde este lado no tiene ya decision propia: lo nuevo manda, igual que
            // al resolver.
            foreach (var legacy in legacyPosts ?? Enumerable.Empty<SafetyPostSide>())
            {
                if (legacy != null
                    && legacy.PostIndex >= 0
                    && legacy.PostIndex < PostCount
                    && !Posts.Any(post => post.PostIndex == legacy.PostIndex))
                {
                    Posts.Add(new BootPostPlacement
                    {
                        PostIndex = legacy.PostIndex,
                        Placement = BootPlacements.From(legacy.Side),
                    });
                }
            }

            var heading = new TextBlock
            {
                Text = Heading(sideLabel),
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 2),
            };

            ModeBox = new ComboBox
            {
                Name = ModeBoxName,
                Margin = new Thickness(0, 0, 0, 6),
                HorizontalAlignment = HorizontalAlignment.Left,
                MinWidth = 180,
                ToolTip = "Dónde va la bota EN ESTE LADO: «Entrada/Salida» es su pasillo de carga y «Posterior»"
                          + " la cara opuesta, que puede necesitar protección aunque no se cargue por ella."
                          + " Se puede afinar por poste.",
            };
            foreach (var label in ModeLabels)
            {
                ModeBox.Items.Add(label);
            }

            ModeBox.SelectedIndex = (int)Placement;
            ModeBox.SelectionChanged += (_, __) =>
            {
                Placement = (BootPlacement)Math.Max(0, ModeBox.SelectedIndex);
                Explicit = true;
                status.Text = StatusText();
            };

            status = new TextBlock
            {
                Text = StatusText(),
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(0x61, 0x70, 0x80)),
                Margin = new Thickness(0, 0, 0, 4),
            };

            Button = new Button
            {
                Name = PerPostButtonName,
                Content = PerPostButtonText,
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 3, 10, 3),
            };
            Button.Click += (_, __) => Configure();

            // I-42 (S1G, decision del dueño) — EL TIPO VIVE AQUI, con su «Ninguno», igual que en la seccion de
            // defensa. Antes se elegia en una fila global al fondo de la ventana mientras la ubicacion y los postes
            // se decidian arriba: una tercera autoridad que gobernaba los dos lados desde abajo.
            PieceId = pieceId;
            PieceBox = new CatalogCombo
            {
                Name = PieceBoxName,
                Margin = new Thickness(0, 0, 0, 6),
                HorizontalAlignment = HorizontalAlignment.Left,
                MinWidth = 220,
                ToolTip = "Tipo de protector de bota de este lado. «Ninguno» significa que este lado no lleva"
                          + " ninguno; la ubicación y los postes que ya tengas se conservan y vuelven al elegir"
                          + " otra vez una pieza.",
            };
            PieceBox.SetCatalogEntries(
                variants ?? new List<SafetyElementCatalogEntry>(),
                IsNone ? PushBackDefaults.NonePieceId : PieceId,
                placeholder: new CatalogOption(PushBackDefaults.NonePieceId, NoneOptionText));
            PieceId = PieceBox.SelectedId;
            PieceBox.SelectionChanged += (_, __) =>
            {
                PieceId = PieceBox.SelectedId;
                status.Text = StatusText();
            };

            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            panel.Children.Add(heading);
            panel.Children.Add(new TextBlock
            {
                Text = PieceLabelText,
                FontSize = 11,
                Margin = new Thickness(0, 2, 0, 2),
            });
            panel.Children.Add(PieceBox);
            panel.Children.Add(new TextBlock
            {
                Text = ModeLabelText,
                FontSize = 11,
                Margin = new Thickness(0, 2, 0, 2),
            });
            panel.Children.Add(ModeBox);
            panel.Children.Add(status);
            panel.Children.Add(Button);
            View = panel;
        }

        public static string Heading(string sideLabel)
            => string.IsNullOrEmpty(sideLabel) ? HeadingText : HeadingText + " — lado " + sideLabel;

        /// <summary>El lado al que pertenece esta superficie. Es un input EXPLICITO, no algo que se deduzca.</summary>
        public PushBackSide Side { get; }

        /// <summary>La etiqueta del lado, o null cuando el rack tiene uno solo.</summary>
        public string SideLabel { get; }

        public int PostCount { get; }

        /// <summary>El selector de tipo de este lado.</summary>
        public CatalogCombo PieceBox { get; }

        /// <summary>
        /// El TIPO elegido para este lado: un id de catalogo, <see cref="PushBackDefaults.NonePieceId"/> o NULL —el
        /// comportamiento historico, que hereda el tipo del documento—.
        /// </summary>
        public string PieceId { get; private set; }

        /// <summary>True cuando este lado quedo explicitamente sin pieza.</summary>
        public bool IsNone => !string.IsNullOrWhiteSpace(PieceId)
                              && string.Equals(
                                  PieceId.Trim(), PushBackDefaults.NonePieceId, StringComparison.OrdinalIgnoreCase);

        /// <summary>La eleccion GENERAL de este lado: el defecto de los postes que heredan.</summary>
        public BootPlacement Placement { get; private set; }

        /// <summary>True cuando esa general la eligio ALGUIEN. False = el lado sigue heredando su automatico.</summary>
        public bool Explicit { get; private set; }

        /// <summary>Los postes con decision propia de este lado. Copia de trabajo.</summary>
        public List<BootPostPlacement> Posts { get; private set; }

        public UIElement View { get; }

        public ComboBox ModeBox { get; }

        public Button Button { get; }

        /// <summary>True una vez que el usuario abrio la rejilla por poste y acepto.</summary>
        public bool Edited { get; private set; }

        /// <summary>La configuracion que el anfitrion aplica al aceptar. Siempre una instancia nueva.</summary>
        public SelectiveBotaConfig ToConfig()
        {
            var config = new SelectiveBotaConfig
            {
                PieceId = PieceId,
                Placement = Explicit ? Placement : (BootPlacement?)null,
            };
            foreach (var post in Posts)
            {
                if (post != null && post.PostIndex >= 0)
                {
                    config.Posts.Add(new BootPostPlacement { PostIndex = post.PostIndex, Placement = post.Placement });
                }
            }

            return config;
        }

        /// <summary>Abre la rejilla por poste de este lado y sustituye la copia de trabajo con su resultado.</summary>
        public void Configure()
        {
            var result = openDialog(this);
            if (result == null)
            {
                return;
            }

            Posts = result
                .Where(post => post != null && post.PostIndex >= 0 && post.PostIndex < PostCount)
                .Select(post => new BootPostPlacement { PostIndex = post.PostIndex, Placement = post.Placement })
                .ToList();
            Edited = true;
            status.Text = StatusText();
        }

        /// <summary>El estado legible sin abrir la rejilla: que hace este lado y cuantos postes deciden por su cuenta.</summary>
        public string StatusText()
        {
            if (IsNone)
            {
                // La intencion NO se borra: se dice que hoy no materializa y se conserva para cuando vuelva a haber
                // pieza. Es el mismo trato que la seccion de defensa da a su «Ninguno».
                return Posts.Count == 0
                    ? NoneStatusText
                    : NoneStatusText + " · " + Posts.Count + " poste(s) configurado(s), en espera";
            }

            var general = Placement == BootPlacement.None
                ? "Sin botas por defecto en este lado"
                : "Por defecto: " + ModeLabels[(int)Placement];
            return Posts.Count == 0
                ? general + " · todos los postes heredan"
                : string.Format(
                    CultureInfo.CurrentCulture, "{0} · {1} poste(s) con decisión propia", general, Posts.Count);
        }
    }
}
