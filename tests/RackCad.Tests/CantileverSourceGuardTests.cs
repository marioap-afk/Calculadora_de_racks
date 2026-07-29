using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Source guards for the Cantilever foundation (I-37A).
    ///
    /// They pin the promises ADR-0024 makes that the compiler cannot: that the sub-assembly composes I-36's
    /// PUBLIC surface and never reaches around it. Same technique as
    /// <see cref="StructuralSectionPluginSourceGuardTests"/>, and same caveat — a text guard proves what the
    /// code does NOT say, never that what it says is right. That is what the invariant suite is for.
    ///
    /// Every assertion reads the source with its COMMENTS REMOVED. The XML-docs of these files explain at
    /// length why they never touch <c>d</c>, <c>bf</c> or a concrete dimension type, and a guard that
    /// forbade naming what it rejects would push that reasoning out of the source — the exact trade-off
    /// I-36B already resolved the same way.
    /// </summary>
    public class CantileverSourceGuardTests
    {
        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se encontro la raiz del repositorio (RackCad.sln).");
            return dir;
        }

        /// <summary>Every Cantilever source file of the product, by path, with its comments stripped.</summary>
        private static IReadOnlyList<(string Path, string Code)> Sources()
        {
            var root = RepoRoot().FullName;

            var folders = new[]
            {
                Path.Combine(root, "src", "RackCad.Domain", "Systems", "Cantilever"),
                Path.Combine(root, "src", "RackCad.Application", "Systems", "Cantilever")
            };

            var files = new List<(string, string)>();

            foreach (var folder in folders)
            {
                Assert.True(Directory.Exists(folder), "No existe la carpeta " + folder + ".");

                foreach (var file in Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories))
                {
                    files.Add((file.Substring(root.Length + 1).Replace('\\', '/'), CodeOnly(File.ReadAllText(file))));
                }
            }

            Assert.True(files.Count >= 10, "Se esperaban al menos diez archivos de Cantilever; hay " + files.Count + ".");
            return files;
        }

        private static string CodeOnly(string source)
        {
            var withoutBlocks = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            return Regex.Replace(withoutBlocks, @"//[^\n]*", string.Empty);
        }

        private static void Forbid(string pattern, string why)
        {
            var regex = new Regex(pattern, RegexOptions.Compiled);

            var offenders = Sources()
                .Where(f => regex.IsMatch(f.Code))
                .Select(f => f.Path)
                .ToList();

            Assert.True(
                offenders.Count == 0,
                why + " Lo incumplen: " + string.Join(", ", offenders) + ".");
        }

        // ---- I-36 is consumed through its public surface, never reached around --------------------------

        [Theory]
        [InlineData(@"\bWSectionDimensions\b")]
        [InlineData(@"\bSSectionDimensions\b")]
        [InlineData(@"\bChannelSectionDimensions\b")]
        [InlineData(@"\bAngleSectionDimensions\b")]
        [InlineData(@"\bHssRectangularSectionDimensions\b")]
        [InlineData(@"\bIStructuralSectionDimensions\b")]
        [InlineData(@"\.Dimensions\b")]
        public void NoCantileverCodeTouchesTheConcreteDimensionsOfASection(string pattern)
        {
            // ADR-0024 D5: every exterior dimension comes from StructuralSectionGeometry.Bounds — the
            // envelope of the contour that will be DRAWN — and not from a tabulated number. Composing
            // against one and drawing the other is how a plate stops matching its profile.
            Forbid(pattern, "Cantilever no puede leer las dimensiones concretas de una seccion.");
        }

        [Theory]
        [InlineData(@"\.d\b")]
        [InlineData(@"\.bf\b")]
        [InlineData(@"\.tw\b")]
        [InlineData(@"\.tf\b")]
        public void NoCantileverCodeReadsATabulatedDimensionByName(string pattern)
        {
            // Member access only, so a local called `d` is not a false positive and `Depth` or `Diameter`
            // never match: only the tabulated names themselves.
            Forbid(pattern, "Cantilever no puede leer d, bf, tw ni tf.");
        }

        [Theory]
        [InlineData(@"\bStructuralSectionGeometry\s*\.\s*Create\b")]
        [InlineData(@"\bnew\s+ClosedContour2D\b")]
        [InlineData(@"\bnew\s+SectionPlanCurve\b")]
        [InlineData(@"\bStructuralSectionGeometryFactory\s*\.\s*Build\b")]
        public void NoCantileverCodeBuildsAStructuralContourOfItsOwn(string pattern)
        {
            // ADR-0022 makes the section geometry of I-36 the single authority. A second generator would
            // compile, would draw, and would be wrong the first time the two disagreed.
            Forbid(pattern, "Cantilever no puede construir un contorno estructural propio.");
        }

        [Theory]
        [InlineData(@"\bWeightPerLength\b")]
        [InlineData(@"\bStructuralSectionUnits\b")]
        [InlineData(@"\.Weight\s*\(")]
        public void NoCantileverCodeComputesWeight(string pattern)
        {
            // The owner deferred weight for the whole of I-37, and its only consumer — the BOM — does not
            // exist yet. A field populated now would be a number nobody validated.
            Forbid(pattern, "El peso queda diferido y I-37A no lo calcula.");
        }

        // ---- no second catalogue, and no disk ------------------------------------------------------------

        [Theory]
        [InlineData(@"\bRackCatalog\b")]
        [InlineData(@"\bIRackCatalogProvider\b")]
        [InlineData(@"secciones\.csv")]
        [InlineData(@"blocks\.csv")]
        [InlineData(@"connection-layout\.csv")]
        [InlineData(@"blocks-library\.dwg")]
        public void NoCantileverCodeUsesTheLegacyCatalogue(string pattern)
        {
            Forbid(pattern, "Cantilever consume el catalogo neutral de I-36, no el catalogo legado.");
        }

        [Theory]
        [InlineData(@"\bFile\s*\.")]
        [InlineData(@"\bDirectory\s*\.")]
        [InlineData(@"\bStreamReader\b")]
        [InlineData(@"\bStrictCsvTable\b")]
        [InlineData(@"\bStructuralSectionCsvSerializer\b")]
        [InlineData(@"\bCsvStructuralSectionCatalogProvider\b")]
        public void NoCantileverCodeReadsAFile(string pattern)
        {
            // The only way in is IStructuralSectionCatalogProvider.Load(), which validates. A resolver that
            // opened a CSV would bypass that validation and would do it once per call.
            Forbid(pattern, "Cantilever no lee archivos: recibe el catalogo ya cargado y validado.");
        }

        // ---- the layer boundary ----------------------------------------------------------------------------

        [Theory]
        [InlineData(@"\bAutodesk\.")]
        [InlineData(@"\bSystem\.Windows\b")]
        [InlineData(@"\bSystem\.Drawing\b")]
        public void NoCantileverCodeReferencesAutoCadOrWpf(string pattern)
        {
            Forbid(pattern, "AGENTS: nada de AutoCAD fuera del Plugin y nada de WPF fuera de la UI.");
        }

        [Fact]
        public void TheDomainHalfDoesNotReferenceApplication()
        {
            // Domain declares no ProjectReference at all, so this cannot compile — the guard states the
            // intent anyway, because the day somebody adds the reference the compiler would go quiet and
            // this would not.
            var root = RepoRoot().FullName;
            var folder = Path.Combine(root, "src", "RackCad.Domain", "Systems", "Cantilever");

            foreach (var file in Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories))
            {
                var code = CodeOnly(File.ReadAllText(file));

                Assert.False(
                    Regex.IsMatch(code, @"\bRackCad\.Application\b"),
                    Path.GetFileName(file) + " referencia RackCad.Application desde Domain.");

                Assert.False(
                    Regex.IsMatch(code, @"\bStructuralSectionId\b"),
                    Path.GetFileName(file) +
                    " usa StructuralSectionId: el diseno guarda TEXTO porque Domain no puede verlo (ADR-0024 D1).");
            }
        }

        // ---- what I-37A is NOT ------------------------------------------------------------------------------

        [Theory]
        [InlineData(@"\bRackSystemKind\b")]
        [InlineData(@"\bSystemRegistry\b")]
        [InlineData(@"\bSystemDescriptor\b")]
        [InlineData(@"\bRackEmbedDocument\b")]
        [InlineData(@"\bRackProjectDocument\b")]
        [InlineData(@"\bBillOfMaterials\b")]
        [InlineData(@"\bHeaderBlockInstance\b")]
        [InlineData(@"\bHeaderRunPlan\b")]
        public void NothingHereReachesIntoALaterInitiative(string pattern)
        {
            // Registration, persistence, BOM and the drawing vocabulary are I-37B onwards. Having them
            // here would make the foundation look integrated when it is not.
            Forbid(pattern, "I-37A es fundacion pura: sin registro, sin persistencia, sin BOM y sin dibujo.");
        }

        [Theory]
        [InlineData("mensula")]
        [InlineData("bahia")]
        public void TheForbiddenVocabularyDoesNotAppear(string word)
        {
            // "Mensula" is already the beam-to-post connector, with its own catalogue and FK; "bahia" is
            // banned in new text by AGENTS. A gusset is a "cartabon" and the space between columns is a
            // "frente".
            var offenders = Sources()
                .Where(f => f.Code.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(f => f.Path)
                .ToList();

            Assert.True(
                offenders.Count == 0,
                "El termino '" + word + "' esta prohibido en Cantilever. Lo incumplen: " +
                string.Join(", ", offenders) + ".");
        }

        // ---- the punch axis has no silent default -------------------------------------------------------------

        [Fact]
        public void ThePunchDirectionFailsClosedInsteadOfDefaultingToAnAxis()
        {
            // Direction used to be `axis == AlongY ? UnitY : UnitZ`, whose else branch drilled an unknown
            // axis vertically without a word.
            //
            // It is pinned HERE, as source, and not by a runtime test, because the constructor now rejects an
            // undefined axis — so the bad state is unreachable through the public API and no test can build
            // it. That is the right defence and it is also why the throwing branch cannot be exercised: what
            // remains checkable is that the branch exists and that the ternary did not come back.
            var punch = Sources()
                .Single(f => f.Path.EndsWith("CantileverPunchPlan.cs", StringComparison.Ordinal))
                .Code;

            var direction = punch.Substring(punch.IndexOf("public Vector3D Direction", StringComparison.Ordinal));
            direction = direction.Substring(0, direction.IndexOf("public override string ToString", StringComparison.Ordinal));

            Assert.Contains("switch (Datum.Axis)", direction, StringComparison.Ordinal);
            Assert.Contains("InvalidOperationException", direction, StringComparison.Ordinal);
            Assert.DoesNotContain("?", direction, StringComparison.Ordinal);

            // And the constructor keeps the state unreachable in the first place.
            Assert.Contains("ArgumentOutOfRangeException", punch, StringComparison.Ordinal);
        }

        // ---- the frame authority is the ONLY place a frame is built -----------------------------------------

        [Theory]
        [InlineData(@"\bLocalFrame3D\s*\.\s*Create\b")]
        [InlineData(@"\bLocalFrame3D\s*\.\s*FromAxes\b")]
        public void OnlyTheFrameAuthorityBuildsAPlacementFrame(string pattern)
        {
            // The orientation registered on a variant used to be data nobody read: the resolver built fixed
            // frames regardless. Now CantileverColumnBaseFrameResolver owns them, and this stops the
            // resolver — or any future piece — from wiring one directly again, which would silently
            // reintroduce the same defect.
            var regex = new Regex(pattern, RegexOptions.Compiled);

            var authorities = new[]
            {
                "CantileverColumnBaseFrameResolver.cs",   // I-37A: base and column
                "CantileverArmFrameResolver.cs"           // I-37B: the arm
            };

            var offenders = Sources()
                .Where(f => !authorities.Any(a => f.Path.EndsWith(a, StringComparison.Ordinal)))
                .Where(f => regex.IsMatch(f.Code))
                .Select(f => f.Path)
                .ToList();

            Assert.True(
                offenders.Count == 0,
                "Solo las autoridades de marcos nombradas construyen marcos. Lo incumplen: " +
                string.Join(", ", offenders) + ".");
        }

        [Fact]
        public void TheFrameAuthorityReadsTheOrientationItWasGiven()
        {
            var authority = Sources()
                .Single(f => f.Path.EndsWith("CantileverColumnBaseFrameResolver.cs", StringComparison.Ordinal))
                .Code;

            // It must dispatch on the orientation and fail closed. A body without a switch would be the old
            // fixed frame wearing a new name.
            Assert.Contains("switch (orientation)", authority, StringComparison.Ordinal);
            Assert.Contains("ArgumentOutOfRangeException", authority, StringComparison.Ordinal);
            Assert.Contains(".Bounds", authority, StringComparison.Ordinal);
        }

        // ---- I-37B: the arm ---------------------------------------------------------------------------------

        [Fact]
        public void TheArmDoesNotDeclareItsOwnColumnPitch()
        {
            // The arm SELECTS the column's regular punches and OBSERVES their spacing. A literal 4 here would
            // be a second authority for the same grid, and it would keep working right up to the day the
            // column changed (ADR-0025, D5).
            var arm = Sources()
                .Where(f => f.Path.EndsWith("CantileverArmResolver.cs", StringComparison.Ordinal) ||
                            f.Path.EndsWith("CantileverArmColumnConnectionPattern.cs", StringComparison.Ordinal))
                .ToList();

            Assert.Equal(2, arm.Count);

            // The pattern is SELF-VERIFIED against synthetic samples FIRST.
            //
            // This is not decoration. The first version of this guard shipped with its word boundaries
            // turned into literal control characters by an escaping accident, and the surviving regex
            // matched nothing that mattered: it passed with a hard-coded 4.0 sitting in the resolver. A
            // guard that cannot fail is worse than no guard, because it reads as coverage. So the regex
            // has to prove it bites before it is trusted.
            var literalFour = new Regex(@"(^|[^\w.])4([^\w]|$)", RegexOptions.Compiled);

            Assert.Matches(literalFour, "var observedPitch = 4.0;");
            Assert.Matches(literalFour, "z += 4;");
            Assert.DoesNotMatch(literalFour, "var x = 24.5;");
            Assert.DoesNotMatch(literalFour, "Math.Round(value, 6);");

            foreach (var file in arm)
            {
                Assert.False(
                    literalFour.IsMatch(file.Code),
                    file.Path + " contiene un literal 4: el pitch se OBSERVA de la columna, no se declara.");

                Assert.DoesNotContain("RegularColumnPunchPitch", file.Code, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void TheArmPitchIsReadFromTheSelectedPunches()
        {
            // The positive half of the rule above: the pattern must actually derive the pitch from the
            // elevations it selected.
            var pattern = Sources()
                .Single(f => f.Path.EndsWith("CantileverArmColumnConnectionPattern.cs", StringComparison.Ordinal))
                .Code;

            Assert.Contains("selectedElevations[1] - selectedElevations[0]", pattern, StringComparison.Ordinal);
            Assert.Contains("ObservedPitch", pattern, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData(@"\bChannelClearGap\b")]
        [InlineData(@"\bClearGap\b")]
        [InlineData(@"\bChannelGap\b")]
        public void NoCantileverCodeCarriesAChannelGap(string pattern)
        {
            // Paired channels touch. The gap is zero because the arrangement puts both contact faces on one
            // plane, and a field whose only legal value is zero gets edited eventually (ADR-0025, D2).
            Forbid(pattern, "Los canales apareados se tocan: no existe un parametro de separacion.");
        }

        [Fact]
        public void TheArmBodyExposesAMemberCOLLECTIONAndNotASingleField()
        {
            // An arrangement can produce two profiles. A single `Member` property with an optional second one
            // would make every consumer check, and would not scale to the third.
            var properties = typeof(RackCad.Application.Systems.Cantilever.CantileverArmBodyPlan)
                .GetProperties()
                .Select(p => p.Name)
                .ToArray();

            Assert.Contains("Members", properties);
            Assert.DoesNotContain("Member", properties);
            Assert.DoesNotContain("SecondMember", properties);
            Assert.DoesNotContain("MemberB", properties);
        }

        [Fact]
        public void TheArmArrangementAuthorityDispatchesExhaustively()
        {
            var authority = Sources()
                .Single(f => f.Path.EndsWith("CantileverArmBodyArrangementResolver.cs", StringComparison.Ordinal))
                .Code;

            Assert.Contains("switch (arrangement)", authority, StringComparison.Ordinal);
            Assert.Contains("ArgumentOutOfRangeException", authority, StringComparison.Ordinal);
            // And it reads the envelope rather than a tabulated dimension.
            Assert.Contains(".Bounds", authority, StringComparison.Ordinal);
        }

        [Fact]
        public void TheArmSideAuthorityDispatchesExhaustively()
        {
            var authority = Sources()
                .Single(f => f.Path.EndsWith("CantileverArmFrameResolver.cs", StringComparison.Ordinal))
                .Code;

            Assert.Contains("switch (side)", authority, StringComparison.Ordinal);
            Assert.Contains("switch (orientation)", authority, StringComparison.Ordinal);
            Assert.Contains("ArgumentOutOfRangeException", authority, StringComparison.Ordinal);
        }

        // ---- the guard is actually looking at something ---------------------------------------------------

        [Fact]
        public void TheGuardReadsTheRealCantileverSources()
        {
            // A guard whose file set is empty passes every assertion. This is what stops that.
            var sources = Sources();

            Assert.Contains(sources, f => f.Path.EndsWith("CantileverColumnBaseResolver.cs", StringComparison.Ordinal));
            Assert.Contains(sources, f => f.Path.EndsWith("CantileverColumnBaseDesign.cs", StringComparison.Ordinal));
            Assert.Contains(sources, f => f.Code.Contains("StructuralSectionGeometry", StringComparison.Ordinal));
            Assert.Contains(sources, f => f.Code.Contains("Bounds", StringComparison.Ordinal));
        }
    }
}
