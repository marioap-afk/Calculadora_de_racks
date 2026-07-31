using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-37D, corrección de columna y base — motivo 5 del rechazo: «las naturalezas físicas no se distinguen
    /// visualmente».
    ///
    /// No se distinguían: todo salía BYBLOCK en la capa 0, así que una columna, una placa, un cartabón y un
    /// agujero se leían exactamente igual. La corrección introduce un vocabulario de <b>roles visuales</b> del
    /// que Application es dueño, y dos adaptadores que lo consumen —color en la previa, capa en el dibujo— para
    /// que la imagen que el usuario aprueba y el bloque que recibe no puedan discrepar.
    ///
    /// Estas pruebas fijan que el vocabulario es TOTAL —ninguna pieza puede llegar al dibujo sin naturaleza— y
    /// que falla en cerrado, que es lo contrario de repartir un gris por defecto.
    /// </summary>
    public class CantileverVisualRoleTests
    {
        private static IEnumerable<CantileverViewPieceKind> Kinds =>
            Enum.GetValues(typeof(CantileverViewPieceKind)).Cast<CantileverViewPieceKind>();

        private static IEnumerable<CantileverVisualRole> Roles =>
            Enum.GetValues(typeof(CantileverVisualRole)).Cast<CantileverVisualRole>();

        // ---- 1. El vocabulario es total -----------------------------------------------------------------

        [Fact]
        public void TODAPiezaDeVistaTieneNaturalezaDeclarada()
        {
            foreach (var kind in Kinds)
            {
                var role = CantileverVisualRoles.Of(kind);

                Assert.True(Enum.IsDefined(typeof(CantileverVisualRole), role));
            }
        }

        [Fact]
        public void TODORolTieneCapaYColor()
        {
            foreach (var role in Roles)
            {
                Assert.False(string.IsNullOrWhiteSpace(CantileverVisualRoles.LayerNameOf(role)));
                Assert.True(CantileverVisualRoles.ColorIndexOf(role) > 0);
            }
        }

        [Fact]
        public void UnaPiezaSinClasificarFALLAEnCerrado()
        {
            // Lo contrario de un gris por defecto: una pieza nueva que nadie clasificó no puede llegar al
            // dibujo pareciendo otra cosa, porque «mal pero verosímil» es el defecto que un lector no caza.
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CantileverVisualRoles.Of((CantileverViewPieceKind)9999));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => CantileverVisualRoles.LayerNameOf((CantileverVisualRole)9999));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => CantileverVisualRoles.ColorIndexOf((CantileverVisualRole)9999));
        }

        // ---- 2. Los roles se distinguen entre sí ---------------------------------------------------------

        [Fact]
        public void CadaRolTieneSuPROPIACapa()
        {
            var names = Roles.Select(CantileverVisualRoles.LayerNameOf).ToList();

            Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        }

        [Fact]
        public void CadaCapaLlevaElPREFIJODelSistema()
        {
            // Aterrizan en el dibujo DEL USUARIO, junto a las suyas: una capa llamada «COLUMNA» chocaría con
            // la que un cliente ya tenga con ese nombre y le cambiaría el aspecto sin avisar.
            foreach (var role in Roles)
            {
                Assert.StartsWith(CantileverVisualRoles.LayerPrefix, CantileverVisualRoles.LayerNameOf(role));
            }
        }

        [Fact]
        public void LasNaturalezasQueUnLectorMasSeparaTienenColoresDISTINTOS()
        {
            // No se exige que los nueve difieran —tensor y adaptador comparten rol a propósito—, sino que las
            // seis del subensamble base–columna se lean aparte, que es lo que el rechazo pedía.
            var criticos = new[]
            {
                CantileverVisualRole.Column,
                CantileverVisualRole.Base,
                CantileverVisualRole.Arm,
                CantileverVisualRole.Plate,
                CantileverVisualRole.Gusset,
                CantileverVisualRole.Punch
            };

            var colores = criticos.Select(CantileverVisualRoles.ColorIndexOf).ToList();

            Assert.Equal(criticos.Length, colores.Distinct().Count());
        }

        [Fact]
        public void ElTENSORYSuADAPTADORCompartenNaturaleza()
        {
            // Un adaptador se fabrica PARA su varilla y no tiene vida propia; darle naturaleza propia diría lo
            // contrario en el plano.
            Assert.Equal(
                CantileverVisualRoles.Of(CantileverViewPieceKind.Brace),
                CantileverVisualRoles.Of(CantileverViewPieceKind.ColdRolledAdapter));
        }

        [Fact]
        public void LaANOTACIONEsUnRolSinPiezaTodavia()
        {
            // Declarado antes de que nada lo emita, a propósito: un consumidor escrito hoy ya tiene que
            // tratarlo, así que el día que lleguen las cotas no pueden caer en el color de lo último que hubo.
            Assert.DoesNotContain(
                Kinds.Select(CantileverVisualRoles.Of), r => r == CantileverVisualRole.Annotation);

            Assert.False(string.IsNullOrWhiteSpace(
                CantileverVisualRoles.LayerNameOf(CantileverVisualRole.Annotation)));
        }

        // ---- 3. Nadie se inventa su propia paleta ---------------------------------------------------------

        [Fact]
        public void ElPluginNoNOMBRACapasNiColoresPorSuCuenta()
        {
            // Una guarda de ARQUITECTURA, que ninguna prueba de comportamiento puede dar: el materializador
            // podría producir exactamente el mismo dibujo con la paleta escrita a mano, y entonces la previa se
            // separaría del bloque a la primera corrección de una de las dos.
            var source = CodeOnly(Path.Combine(
                SourceRoot(), "src", "RackCad.Plugin", "Drawing", "Cantilever", "CantileverViewMaterializer.cs"));

            Assert.Contains("CantileverVisualRoles.LayerNameOf", source);
            Assert.Contains("CantileverVisualRoles.ColorIndexOf", source);
            Assert.DoesNotContain("RACKCAD_CANT_", source);
        }

        [Fact]
        public void LaPreviaTampocoSeInventaLaPaleta()
        {
            var source = CodeOnly(Path.Combine(
                SourceRoot(), "src", "RackCad.UI", "Systems", "Cantilever", "CantileverPreviewRenderer.cs"));

            Assert.Contains("CantileverVisualRoles.ColorIndexOf", source);

            // Ni un solo `Color.FromRgb` de pieza: el unico que queda es el del mensaje de «no hay nada que
            // dibujar», que no es una pieza y por eso no tiene rol.
            var literales = source.Split(new[] { "Color.FromRgb" }, StringSplitOptions.None).Length - 1;

            Assert.Equal(1, literales);
        }

        [Fact]
        public void LaPALETADePantallaCubreTodosLosIndicesDeclarados()
        {
            // El adaptador de pantalla sólo mapea los índices que los roles nombran. Si alguien elige uno
            // nuevo sin añadirlo allí, la previa lo pintaría magenta chillón; esta prueba lo dice antes.
            var palette = CodeOnly(Path.Combine(
                SourceRoot(), "src", "RackCad.UI", "Systems", "Cantilever", "AciPalette.cs"));

            foreach (var role in Roles)
            {
                var index = CantileverVisualRoles.ColorIndexOf(role);

                Assert.True(
                    palette.Contains("[" + index + "]"),
                    "El indice ACI " + index + ", del rol " + role + ", no tiene color de pantalla.");
            }
        }

        private static string SourceRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "RackCad.sln")))
            {
                directory = directory.Parent;
            }

            Assert.NotNull(directory);
            return directory.FullName;
        }

        /// <summary>The file's CODE, with comments stripped: a guard must not be satisfied by prose.</summary>
        private static string CodeOnly(string path)
        {
            var text = File.ReadAllText(path);
            var lines = text.Split('\n')
                .Where(l => !l.TrimStart().StartsWith("//", StringComparison.Ordinal))
                .Where(l => !l.TrimStart().StartsWith("///", StringComparison.Ordinal));

            return string.Join("\n", lines);
        }
    }
}
