using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.UI.Systems.PushBack
{
    /// <summary>
    /// I-42 (ronda 7D, decision del dueño) — LA DEFENSA DE MONTACARGAS DE UN LADO, dentro de «Elementos de seguridad».
    ///
    /// <para>
    /// Es el mismo patron de organizacion que <see cref="PushBackRearTopeSection"/>, que el dueño ya valido: un
    /// titulo por lado, su estado, su copia de trabajo y un boton «Configurar…» que abre la rejilla POR POSTE de ESE
    /// lado. En un rack compuesto hay dos secciones, A y B; en uno de un solo sentido, una sola y sin etiqueta.
    /// </para>
    ///
    /// <para>
    /// La ventana principal deja de participar: abrir Seguridad con el lado activo en A o en B ofrece exactamente
    /// las mismas secciones. Antes no habia ninguna superficie para el lado B —la unica rejilla hablaba de
    /// «entrada/salida» y «posterior», el vocabulario de un rack de un solo sentido—, y por eso toda edicion
    /// terminaba aplicandose al lado A.
    /// </para>
    ///
    /// <para>
    /// La granularidad NO cambia: sigue siendo POR POSTE. La seccion solo dice a que LADO pertenecen esos postes.
    /// </para>
    /// </summary>
    internal sealed class PushBackDefenseSection
    {
        private readonly TextBlock status;
        private readonly Func<PushBackDefenseSection, IReadOnlyList<SafetyPostDefense>> openDialog;

        /// <param name="side">El lado cuyo pasillo protege esta seccion.</param>
        /// <param name="sideLabel">La etiqueta del lado, o null en un rack de un solo sentido.</param>
        /// <param name="current">Los registros POR POSTE del rack; la seccion trabaja sobre una COPIA.</param>
        /// <param name="faces">En que lineas existe la cara de ataque de este lado (aplicabilidad).</param>
        public PushBackDefenseSection(
            PushBackSide side,
            string sideLabel,
            IEnumerable<SafetyPostDefense> current,
            int postCount,
            IReadOnlyList<bool> faces,
            Func<PushBackDefenseSection, IReadOnlyList<SafetyPostDefense>> openDialog)
        {
            Side = side;
            SideLabel = sideLabel;
            PostCount = Math.Max(1, postCount);
            Faces = faces ?? Enumerable.Repeat(true, PostCount).ToList();
            this.openDialog = openDialog ?? throw new ArgumentNullException(nameof(openDialog));

            // Una copia de trabajo: nada de lo que el usuario haga aqui toca el rack hasta que se acepta la ventana.
            // Solo de las lineas que EXISTEN: una que el rack ya no tiene no deja fantasma. Las que siguen ahi
            // conservan su indice, que no se compacta nunca.
            Posts = PushBackDefenseSides.Copy(current)
                .Where(record => record.PostIndex >= 0 && record.PostIndex < PostCount)
                .ToList();

            var heading = new TextBlock
            {
                Text = Heading(sideLabel),
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 2),
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
                Name = ConfigureButtonName,
                Content = ConfigureButtonText,
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 3, 10, 3),
            };
            Button.Click += (_, __) => Configure();

            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            panel.Children.Add(heading);
            panel.Children.Add(status);
            panel.Children.Add(Button);
            View = panel;
        }

        /// <summary>The section heading shown inside the safety dialog.</summary>
        public const string HeadingText = "Defensa de montacargas";

        public const string ConfigureButtonText = "Configurar…";

        /// <summary>x:Name-equivalent of the button, so a test can find it inside the composed dialog.</summary>
        public const string ConfigureButtonName = "DefenseConfigureButton";

        public static string Heading(string sideLabel)
            => string.IsNullOrEmpty(sideLabel) ? HeadingText : HeadingText + " — lado " + sideLabel;

        /// <summary>El lado al que pertenece esta superficie. Es un input EXPLICITO, no algo que se deduzca.</summary>
        public PushBackSide Side { get; }

        /// <summary>La etiqueta del lado, o null cuando el rack tiene uno solo.</summary>
        public string SideLabel { get; }

        public int PostCount { get; }

        /// <summary>La aplicabilidad de ESTE lado, linea a linea. La decide la fisica, no la seccion.</summary>
        public IReadOnlyList<bool> Faces { get; }

        /// <summary>La copia de trabajo que el anfitrion funde al aceptar. Nunca la instancia del rack.</summary>
        public List<SafetyPostDefense> Posts { get; private set; }

        public UIElement View { get; }

        public Button Button { get; }

        /// <summary>True una vez que el usuario abrio la rejilla y acepto.</summary>
        public bool Edited { get; private set; }

        /// <summary>
        /// En un rack de UN SOLO SENTIDO esta seccion ES toda la defensa: abre la rejilla historica de los dos
        /// extremos —el bajo y el posterior, que PB-009 dejo apagado por defecto pero nunca prohibido— y su
        /// resultado sustituye la lista entera. Quitarle esa segunda columna seria perder una capacidad que el
        /// dueño valido, y esta ronda solo viene a organizar la de un COMPUESTO.
        /// </summary>
        public bool OwnsBothEnds => string.IsNullOrEmpty(SideLabel);

        /// <summary>
        /// La cara que esta seccion entrega a la rejilla: su nombre, su extremo y donde existe. NULL en un rack de
        /// un solo sentido, que abre la rejilla historica.
        /// </summary>
        public SafetyDefenseFace Face()
            => OwnsBothEnds
                ? null
                : new SafetyDefenseFace("Lado " + SideLabel, PushBackDefenseSides.IsFarEnd(Side), Faces);

        /// <summary>Abre la rejilla por poste de este lado y funde su resultado en la copia de trabajo.</summary>
        public void Configure()
        {
            var result = openDialog(this);
            if (result == null)
            {
                return;
            }

            // La fusion es POR LADO: lo que decida esta seccion no puede tocar la cara del otro. En un rack de un
            // solo sentido no hay otra cara que proteger y la rejilla historica decide las dos.
            Posts = OwnsBothEnds
                ? PushBackDefenseSides.Copy(result)
                : PushBackDefenseSides.Merge(Posts, result, Side);
            Edited = true;
            status.Text = StatusText();
        }

        /// <summary>El estado legible sin abrir la rejilla: cuantas lineas llevan defensa en ESTE lado.</summary>
        public string StatusText()
        {
            var applicable = Enumerable.Range(0, PostCount).Count(post => post >= Faces.Count || Faces[post]);
            if (applicable == 0)
            {
                return "Este lado no tiene cara de ataque en ninguna línea.";
            }

            var drawn = Enumerable.Range(0, PostCount)
                .Count(post => (post >= Faces.Count || Faces[post]) && Draws(post));
            var manual = Posts.Count(record =>
                !PushBackDefenseSides.AutoOf(record, Side) && record.PostIndex >= 0 && record.PostIndex < PostCount);
            return string.Format(
                CultureInfo.CurrentCulture,
                "{0} de {1} línea(s) con defensa · {2}",
                drawn,
                applicable,
                manual == 0 ? "todas automáticas" : manual + " decidida(s) a mano");
        }

        /// <summary>Si este lado materializa defensa en esa linea, con la misma regla que el dibujo.</summary>
        private bool Draws(int postIndex)
        {
            var applies = postIndex >= Faces.Count || Faces[postIndex];
            var setting = DynamicForkliftDefensePlan.At(
                Posts, postIndex, PostCount, lowEndOnly: true, secondLoadFace: applies);
            return PushBackDefenseSides.Resolved(setting, Side) > 0.0;
        }
    }
}
