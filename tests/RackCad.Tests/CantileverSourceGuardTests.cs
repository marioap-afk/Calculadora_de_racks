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

            Assert.True(files.Count >= 18, "Se esperaban al menos dieciocho archivos de Cantilever; hay " + files.Count + ".");
            return files;
        }

        private static string CodeOnly(string source)
        {
            var withoutBlocks = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            return Regex.Replace(withoutBlocks, @"//[^\n]*", string.Empty);
        }

        /// <summary>
        /// One product source file with its comments INTACT.
        ///
        /// <see cref="Sources"/> strips them on purpose, which is right for every guard that forbids naming a
        /// forbidden thing. The guards on the declared square-cut approximation are the opposite case: what is
        /// being pinned IS the prose, so they need the file as written.
        /// </summary>
        private static string RawSource(string fileName)
        {
            var root = RepoRoot().FullName;

            var matches = new[]
                {
                    Path.Combine(root, "src", "RackCad.Domain", "Systems", "Cantilever"),
                    Path.Combine(root, "src", "RackCad.Application", "Systems", "Cantilever")
                }
                .Where(Directory.Exists)
                .SelectMany(d => Directory.GetFiles(d, fileName, SearchOption.AllDirectories))
                .ToList();

            Assert.Single(matches);
            return File.ReadAllText(matches[0]);
        }

        /// <summary>One documentation file of the repository, verbatim.</summary>
        private static string Document(params string[] relative)
        {
            var path = Path.Combine(new[] { RepoRoot().FullName }.Concat(relative).ToArray());

            Assert.True(File.Exists(path), "No existe el documento " + path + ".");
            return File.ReadAllText(path);
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
        [InlineData(@"\bHeaderBlockInstance\b")]
        [InlineData(@"\bHeaderRunPlan\b")]
        public void NothingHereReachesIntoALaterInitiative(string pattern)
        {
            // Registration, persistence and the drawing vocabulary are still later initiatives. Having them
            // here would make the foundation look integrated when it is not.
            //
            // `BillOfMaterials` USED to be on this list, and it was right to be: for I-37A and I-37B the BOM
            // was a later initiative. I-37C IS that initiative, so the entry was REMOVED rather than
            // suppressed — a guard that forbids what the current scope requires gets weakened by whoever hits
            // it next, and that weakening is what would go unnoticed. Three narrower rules replaced it:
            // TheBomIsBuiltFromTheSTATIONAndNotFromTheDesign, TheArmBomSignatureExcludesPositionAndSide and
            // ThePunchesNeverBecomeBomLines.
            Forbid(pattern, "Cantilever no toca registro, persistencia ni vocabulario de dibujo.");
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
                "CantileverArmFrameResolver.cs",          // I-37B: the arm
                "CantileverLineFrameResolver.cs"       // I-37D: the separators, the braces and the line translation
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

        // ---- I-37B, correction round: the four defects ------------------------------------------------------

        [Fact]
        public void TheEndPlateModeIsDispatchedExhaustivelyInBOTHPlaces()
        {
            // The defect: validation with guarded cases and no default let an undeclared mode through, and the
            // builder's `if None ... else ternary` then drew it as a cap. Both sites switch now, and both close.
            var resolver = Sources()
                .Single(f => f.Path.EndsWith("CantileverArmResolver.cs", StringComparison.Ordinal))
                .Code;

            Assert.Equal(2, Regex.Matches(resolver, @"switch \(end\.Mode\)").Count);
            Assert.Contains("ArmEndPlateModeNotSupported", resolver, StringComparison.Ordinal);
            Assert.Contains("ArgumentOutOfRangeException", resolver, StringComparison.Ordinal);

            // And the shape that caused it is gone: no ternary on the mode anywhere.
            Assert.DoesNotMatch(new Regex(@"end\.Mode == CantileverArmEndPlateMode\.\w+\s*\?"), resolver);
        }

        [Fact]
        public void TheEndPlateThicknessIsNotValidatedUnconditionally()
        {
            // Under `None` there is no plate, so the thickness is dormant data. A single unconditional
            // RequirePositive on it would reject a valid arm — which is what the first version did.
            var resolver = Sources()
                .Single(f => f.Path.EndsWith("CantileverArmResolver.cs", StringComparison.Ordinal))
                .Code;

            // The method is called ValidateOwnParameters since the I-37C review moved the connection half of
            // the validation into the shared metrics authority. The RULE is unchanged: under `None` the end
            // plate thickness is dormant data, so nothing before ValidateEndPlate may read it.
            var validate = resolver.Substring(
                resolver.IndexOf("ValidateOwnParameters(", StringComparison.Ordinal));
            var untilEndPlate = validate.Substring(
                0, validate.IndexOf("ValidateEndPlate(", StringComparison.Ordinal));

            Assert.DoesNotContain("end.Thickness", untilEndPlate, StringComparison.Ordinal);
        }

        [Fact]
        public void ThePunchIndexRangeIsCheckedWithASUBTRACTION()
        {
            // `lowerColumnPunchIndex + verticalPunchCount` overflows int for a large index: it wraps negative,
            // the check passes, the selection comes out empty and the pitch read throws. The comparison has to
            // stay on the subtraction side.
            var pattern = Sources()
                .Single(f => f.Path.EndsWith("CantileverArmColumnConnectionPattern.cs", StringComparison.Ordinal))
                .Code;

            var vulnerableSum = new Regex(
                @"if\s*\(\s*lowerColumnPunchIndex\s*\+\s*verticalPunchCount", RegexOptions.Compiled);

            // Self-verified before it is trusted: a guard that cannot bite reads as coverage.
            Assert.Matches(vulnerableSum, "if (lowerColumnPunchIndex + verticalPunchCount > elevations.Count)");
            Assert.DoesNotMatch(vulnerableSum, "if (lowerColumnPunchIndex > elevations.Count - verticalPunchCount)");

            Assert.DoesNotMatch(vulnerableSum, pattern);
            Assert.Contains(
                "lowerColumnPunchIndex > elevations.Count - verticalPunchCount",
                pattern,
                StringComparison.Ordinal);
        }

        [Fact]
        public void TheSlopeGateAndTheProjectionShareONENumber()
        {
            // A gate with its own copy of the bound is a gate that eventually disagrees with what it guards.
            var frame = Sources()
                .Single(f => f.Path.EndsWith("CantileverArmFrameResolver.cs", StringComparison.Ordinal))
                .Code;

            Assert.Single(Regex.Matches(frame, @"DegenerateProjection\s*=\s*1e-8"));
            Assert.Equal(2, Regex.Matches(frame, @"<=\s*DegenerateProjection").Count);
            Assert.DoesNotMatch(new Regex(@"<=\s*1e-\d"), frame);

            // And the CONSUMER gates on the authority instead of re-deriving the condition. Since the I-37C
            // review that consumer is the shared connection-metrics authority rather than the arm resolver
            // itself: the rule did not change, the single place that applies it did.
            var metrics = Sources()
                .Single(f => f.Path.EndsWith("CantileverArmConnectionMetricsResolver.cs", StringComparison.Ordinal))
                .Code;

            Assert.Contains("IsRepresentableSlope", metrics, StringComparison.Ordinal);
            Assert.Contains("ArmSlopeFrameUndefined", metrics, StringComparison.Ordinal);

            // And the arm resolver no longer carries its own copy of the gate.
            var resolver = Sources()
                .Single(f => f.Path.EndsWith("CantileverArmResolver.cs", StringComparison.Ordinal))
                .Code;

            Assert.DoesNotContain("IsRepresentableSlope", resolver, StringComparison.Ordinal);
        }

        [Fact]
        public void NoSourceClaimsThePlateAndTheBodyDoNotOverlap()
        {
            // Read RAW, comments included: the claim that was wrong lived in an XML-doc and in a comment, and
            // stripping them is exactly how it would survive this guard.
            var plate = RawSource("CantileverPlatePlan.cs");
            var resolver = RawSource("CantileverArmResolver.cs");

            Assert.DoesNotContain("never overlap", plate, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("never share material", resolver, StringComparison.OrdinalIgnoreCase);

            Assert.Contains("ORIGIN of the cut plane", plate, StringComparison.Ordinal);
            Assert.Contains("declared visual approximation", plate, StringComparison.Ordinal);
            Assert.Contains("not a claim of no overlap", resolver, StringComparison.Ordinal);
        }

        [Fact]
        public void NoDocumentClaimsThePlateAndTheBodyDoNotOverlapEITHER()
        {
            // The ADR and the contract are where a reader goes for the rule. They said the faces did not
            // overlap; with slope and a square cut that is simply false.
            var adr = Document("docs", "adr", "0025-brazo-cantilever-cuerpo-compuesto-y-conexion.md");
            var contract = Document("docs", "initiatives", "I-37B-cantilever-brazo.md");
            var decision = Document("docs", "automation", "decisions", "I-37.md");

            foreach (var document in new[] { adr, contract, decision })
            {
                // The indicative claim. The ADR and the contract both DENY it in the subjunctive
                // ("no afirma que ... no se traslapen"), which is a different string and stays legal.
                Assert.DoesNotContain("no se traslapan", document, StringComparison.Ordinal);
            }

            // Substrings only, and short ones: the prose is hard-wrapped, so a longer phrase would be split
            // across lines and the guard would fail on formatting instead of on meaning.
            Assert.Contains("plano de corte", adr, StringComparison.Ordinal);
            Assert.Contains("cara exterior", adr, StringComparison.Ordinal);
            Assert.Contains("penetra", adr, StringComparison.Ordinal);
            Assert.Contains("holgura", adr, StringComparison.Ordinal);
            Assert.Contains("aproximaci", adr, StringComparison.Ordinal);

            Assert.Contains("NO queda a ras", contract, StringComparison.Ordinal);
            Assert.Contains("fuera de alcance", contract, StringComparison.Ordinal);

            Assert.Contains("origen", decision, StringComparison.Ordinal);
            Assert.Contains("8.13.bis", decision, StringComparison.Ordinal);
        }

        // ---- I-37C: the station -----------------------------------------------------------------------------

        [Theory]
        [InlineData(@"\bsecciones\.csv\b")]
        [InlineData(@"\bblocks\.csv\b")]
        [InlineData(@"\bconnection-layout\.csv\b")]
        [InlineData(@"\bblocks-library\b")]
        public void NoCantileverCodeNamesALegacyCatalogueFile(string pattern)
        {
            // The station composes I-36 and I-37A/B, all of which read the neutral catalogue. A file name here
            // would be a second data source, and the first one to disagree wins silently.
            Forbid(pattern, "Cantilever no lee catalogos legacy ni la biblioteca de bloques.");
        }

        [Fact]
        public void TheStationHasNoSECONDRegularGridFormula()
        {
            // The formula `LastConnectionElevation + index * pitch` exists in exactly ONE type. A copy in the
            // station or in the layout would be PB-004 again: two algorithms for one set of bolts, agreeing
            // right up to the day one is edited (ADR-0026, D5).
            var offenders = Sources()
                .Where(f => !f.Path.EndsWith("CantileverColumnRegularPunchGrid.cs", StringComparison.Ordinal))
                .Where(f => f.Code.Contains("LastConnectionElevation +", StringComparison.Ordinal) ||
                            f.Code.Contains("LastConnectionElevation+", StringComparison.Ordinal))
                .Select(f => f.Path)
                .ToList();

            Assert.True(
                offenders.Count == 0,
                "Solo la autoridad de reticula deriva la primera elevacion regular. Lo incumplen: " +
                string.Join(", ", offenders) + ".");

            // And the authority really does own it, so the guard is not passing because nobody has it.
            var grid = Sources()
                .Single(f => f.Path.EndsWith("CantileverColumnRegularPunchGrid.cs", StringComparison.Ordinal))
                .Code;

            Assert.Contains("LastConnectionElevation +", grid, StringComparison.Ordinal);
        }

        [Fact]
        public void TheStationResolverDeclaresNoPitchOfItsOwn()
        {
            // Same rule the arm follows: the pitch is the COLUMN's, observed and never declared.
            var station = Sources()
                .Where(f => f.Path.Contains("/CantileverStation", StringComparison.Ordinal))
                .ToList();

            Assert.True(station.Count >= 5, "Se esperaban al menos cinco archivos de estacion.");

            var literalPitch = new Regex(@"(^|[^\w.])(2|4)\s*\.\s*0\s*;", RegexOptions.Compiled);

            // SELF-VERIFIED before it is trusted: the first version of a guard like this one shipped with its
            // word boundaries turned into control characters and matched nothing at all.
            Assert.Matches(literalPitch, "var pitch = 4.0;");
            Assert.Matches(literalPitch, "double p = 2.0;");
            Assert.DoesNotMatch(literalPitch, "var factor = 1.0 / 3.0;");
            Assert.DoesNotMatch(literalPitch, "Math.Round(value, 4);");

            foreach (var file in station)
            {
                Assert.False(
                    literalPitch.IsMatch(file.Code),
                    file.Path + " declara un pitch literal: el espaciado lo gobierna la columna.");

                Assert.DoesNotContain("RegularColumnPunchPitch", file.Code, StringComparison.Ordinal);
            }
        }

        [Theory]
        [InlineData(@"\bProvisionalHeight\b")]
        [InlineData(@"\bTemporaryHeight\b")]
        [InlineData(@"\bProbeHeight\b")]
        [InlineData(@"\bAssumedHeight\b")]
        [InlineData(@"\bDefaultColumnHeight\b")]
        public void NoStationCodeCarriesAProvisionalHeight(string pattern)
        {
            // The circular dependency is resolved by a sequence, not by a magic number. A field named like this
            // is that number acquiring a home (ADR-0026, D5).
            Forbid(pattern, "La circularidad se resuelve con una secuencia explicita, no con una altura provisional.");
        }

        [Fact]
        public void TheGridIsObtainedWithoutReadingTheHeight()
        {
            // The positive half of the rule above: the station asks for the grid through the height-independent
            // seam, and the seam says in its own name that it does not read the height.
            var resolver = Sources()
                .Single(f => f.Path.EndsWith("CantileverStationResolver.cs", StringComparison.Ordinal))
                .Code;

            Assert.Contains("ResolveRegularPunchGrid", resolver, StringComparison.Ordinal);

            var columnBase = Sources()
                .Single(f => f.Path.EndsWith("CantileverColumnBaseResolver.cs", StringComparison.Ordinal))
                .Code;

            Assert.Contains("ResolveHeightIndependent", columnBase, StringComparison.Ordinal);
        }

        [Fact]
        public void ADoubleStationCannotHoldTwoColumnsOrTwoSubAssemblies()
        {
            // The cardinality is carried by the TYPE: one Column property, one ColumnBottomPlate property, and
            // a collection only for the sides. A second column would need a second property, and this is what
            // stops one appearing (ADR-0026, D1).
            var type = typeof(RackCad.Application.Systems.Cantilever.CantileverStationColumnBaseAssembly);
            var properties = type.GetProperties().Select(p => p.Name).ToList();

            Assert.Contains("Column", properties);
            Assert.Contains("ColumnBottomPlate", properties);
            Assert.Contains("Sides", properties);

            Assert.DoesNotContain("Columns", properties);
            Assert.DoesNotContain("SecondColumn", properties);
            Assert.DoesNotContain("ColumnBottomPlates", properties);
            Assert.DoesNotContain("PositiveAssembly", properties);
            Assert.DoesNotContain("NegativeAssembly", properties);

            // Column is a single member, and Sides is the only collection of base pieces.
            Assert.Equal(
                typeof(RackCad.Application.Systems.Cantilever.CantileverStructuralMemberPlan),
                type.GetProperty("Column").PropertyType);
        }

        [Fact]
        public void TheStationNeverResolvesASecondColumnBaseAssembly()
        {
            // A second `Resolve(` of the column–base would produce the second column this design exists to
            // avoid. There is exactly ONE call, and the mirror composes from its result.
            var resolver = Sources()
                .Single(f => f.Path.EndsWith("CantileverStationResolver.cs", StringComparison.Ordinal))
                .Code;

            Assert.Equal(1, Regex.Matches(resolver, @"_columnBase\.Resolve\(").Count);

            var side = Sources()
                .Single(f => f.Path.EndsWith("CantileverStationBaseSideResolver.cs", StringComparison.Ordinal))
                .Code;

            Assert.DoesNotContain("CantileverColumnBaseResolver", side, StringComparison.Ordinal);
            Assert.DoesNotContain(".Resolve(", side, StringComparison.Ordinal);
        }

        [Fact]
        public void TheStationRoundsNoElevationInsteadOfChoosingAnIndex()
        {
            // The answer is an INDEX of a grid that already exists. Rounding an elevation can land BELOW the
            // requested clear, and it would also invent a hole where the column has none (ADR-0026, D4).
            var layout = Sources()
                .Single(f => f.Path.EndsWith("CantileverStationLevelLayoutResolver.cs", StringComparison.Ordinal))
                .Code;

            Assert.DoesNotContain("Math.Ceiling", layout, StringComparison.Ordinal);
            Assert.DoesNotContain("Math.Floor", layout, StringComparison.Ordinal);
            Assert.DoesNotContain("Math.Round", layout, StringComparison.Ordinal);

            // And it walks candidates upwards from a floor instead.
            Assert.Contains("UpperPunchIndex + 1", layout, StringComparison.Ordinal);
        }

        [Fact]
        public void TheBomIsBuiltFromTheSTATIONAndNotFromTheDesign()
        {
            // A BOM computed from the intent counts what the user asked for, not what the model resolved — and
            // those differ exactly when something was rejected or adjusted (ADR-0026, D7).
            var bom = Sources()
                .Single(f => f.Path.EndsWith("CantileverStationBomBuilder.cs", StringComparison.Ordinal))
                .Code;

            Assert.DoesNotContain("CantileverStationDesign", bom, StringComparison.Ordinal);
            Assert.DoesNotContain("CantileverArmTemplateDesign", bom, StringComparison.Ordinal);
            Assert.DoesNotContain("CantileverStationArmMatrix", bom, StringComparison.Ordinal);
            Assert.Contains("CantileverStationAssembly station", bom, StringComparison.Ordinal);
        }

        [Fact]
        public void TheArmBomSignatureExcludesPositionAndSide()
        {
            // An identical arm on +Y and on −Y is ONE purchase line. Side, level, station index, world
            // coordinates and the owner token would all split it (ADR-0026, D8).
            var bom = Sources()
                .Single(f => f.Path.EndsWith("CantileverStationBomBuilder.cs", StringComparison.Ordinal))
                .Code;

            var signature = bom.Substring(bom.IndexOf("public static string ArmSignature", StringComparison.Ordinal));
            signature = signature.Substring(0, signature.IndexOf("private static double StopExtension", StringComparison.Ordinal));

            Assert.DoesNotContain(".Side", signature, StringComparison.Ordinal);
            Assert.DoesNotContain("Owner", signature, StringComparison.Ordinal);
            Assert.DoesNotContain("LevelIndex", signature, StringComparison.Ordinal);
            Assert.DoesNotContain("Centre", signature, StringComparison.Ordinal);
            Assert.DoesNotContain("Frame", signature, StringComparison.Ordinal);

            // And it does carry the physical recipe.
            foreach (var term in new[] { "Arrangement", "SectionId", "NominalCutLength", "SlopeRisePer12" })
            {
                Assert.Contains(term, signature, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void ThePunchesNeverBecomeBomLines()
        {
            var bom = Sources()
                .Single(f => f.Path.EndsWith("CantileverStationBomBuilder.cs", StringComparison.Ordinal))
                .Code;

            // A hole is not a PIECE, and the rule is about lines rather than about mentions. The builder does
            // read the punch collections since the I-37C review — a plate's identity includes its hole pattern,
            // which is exactly how two plates that differ only in their holes stay different parts. What must
            // not exist is a line or a category FOR a punch.
            //
            // Every BomLine is built in one of three helpers: Profile, Plate and Gusset. Counting the
            // constructions is what makes a fourth one impossible to add quietly.
            Assert.Equal(3, Regex.Matches(bom, @"new BomLine").Count);

            Assert.DoesNotMatch(new Regex(@"PunchCategory|TroquelCategory"), bom);
            Assert.DoesNotMatch(new Regex(@"private static BomLine Punch\("), bom);
            Assert.DoesNotContain("station.Punches", bom, StringComparison.Ordinal);

            // And the punches only ever reach the plate identity, never a quantity.
            Assert.DoesNotMatch(new Regex(@"Quantity\s*=\s*\w*[Pp]unch"), bom);
        }

        [Fact]
        public void TheStationDoesNotModifyTheNominalCutLengthOfAnArm()
        {
            // The length the user captured is what they order. A station that adjusted it would break the one
            // promise ADR-0025 D3 makes about that number.
            foreach (var file in Sources().Where(f => f.Path.Contains("/CantileverStation", StringComparison.Ordinal)))
            {
                Assert.DoesNotMatch(new Regex(@"NominalCutLength\s*="), file.Code);
                Assert.DoesNotMatch(new Regex(@"CutLength\s*=\s*[^;]*[-+*/]"), file.Code);
                Assert.DoesNotContain("WithLength(", file.Code, StringComparison.Ordinal);
            }
        }

        [Theory]
        [InlineData(@"\bChannelClearGap\b")]
        [InlineData(@"\bChannelSpacing\b")]
        [InlineData(@"\bArmGap\b")]
        public void TheStationCarriesNoFreeSpacingBetweenChannels(string pattern)
        {
            Forbid(pattern, "Los canales apareados se tocan: la estacion no introduce un parametro de separacion.");
        }

        [Fact]
        public void NoStationCodeMentionsARunOrItsFurniture()
        {
            // I-37C wrote this over EVERY Cantilever file, because the line did not exist yet and a property
            // named for one was how a later initiative's decisions got taken by accident. I-37D built the line,
            // so the blanket ban would now forbid the line from being about itself.
            //
            // What the rule was actually protecting is still worth protecting, and it is narrower: a STATION
            // must not know it is inside a line. So the ban now covers the three sub-assembly families — the
            // station, the column with its base, and the arm — and not the line's own files. Anything a station
            // file learns about a separator is still a defect, and this still says so.
            var subAssemblies = new[] { "CantileverStation", "CantileverColumnBase", "CantileverArm" };

            var stationFiles = Sources()
                .Where(f => subAssemblies.Any(p => FileName(f.Path).StartsWith(p, StringComparison.Ordinal)))
                .ToList();

            Assert.True(stationFiles.Count >= 12, "Se esperaban al menos doce archivos de subensamble; hay " +
                stationFiles.Count + ".");

            foreach (var word in new[]
                     {
                         "Separador", "Spacer", "Arriostr", "Brace", "StationRun", "RunIndex",
                         "LongitudinalPosition", "NeighbourStation", "IntervalIndex", "CantileverLine"
                     })
            {
                var offenders = stationFiles
                    .Where(f => f.Code.Contains(word, StringComparison.OrdinalIgnoreCase))
                    .Select(f => f.Path)
                    .ToList();

                Assert.True(
                    offenders.Count == 0,
                    "'" + word + "' pertenece a la linea y una estacion no lo conoce. Lo incumplen: " +
                    string.Join(", ", offenders) + ".");
            }
        }

        [Fact]
        public void TheLineIsTheONLYPlaceThatKnowsAboutIntervals()
        {
            // The other half of the rule above, and the reason it can be narrowed safely: the line's concepts
            // are confined to the line's own files. If "Interval" ever appears outside them, the station has
            // started to learn about its neighbours through some other door.
            var lineFiles = new[]
            {
                "CantileverLineDesign.cs", "CantileverLineResolver.cs", "CantileverLineAssembly.cs",
                "CantileverLineArmMatrix.cs", "CantileverLineBomBuilder.cs",
                "CantileverIntervalAssembly.cs", "CantileverIntervalResolver.cs",
                "CantileverBracingLayoutResolver.cs", "CantileverLineFrameResolver.cs",
                "CantileverViewPlanBuilder.cs",
                "CantileverMemberId.cs"   // shared piece tokens: the interval's owner token lives with the rest
            };

            var offenders = Sources()
                .Where(f => !lineFiles.Any(n => FileName(f.Path).Equals(n, StringComparison.Ordinal)))
                .Where(f => f.Code.Contains("Interval", StringComparison.Ordinal))
                .Select(f => f.Path)
                .ToList();

            Assert.True(
                offenders.Count == 0,
                "Solo los archivos de la linea conocen intervalos. Lo incumplen: " +
                string.Join(", ", offenders) + ".");
        }

        private static string FileName(string path)
        {
            var slash = path.LastIndexOf('/');
            return slash < 0 ? path : path.Substring(slash + 1);
        }

        [Fact]
        public void TheStationHasNoPersistenceAndNoView()
        {
            foreach (var word in new[] { "Document", "Xrecord", "RackProject", "ViewBuilder", "Preview", "RackSystemKind" })
            {
                var offenders = Sources()
                    .Where(f => f.Code.Contains(word, StringComparison.Ordinal))
                    .Select(f => f.Path)
                    .ToList();

                Assert.True(
                    offenders.Count == 0,
                    "'" + word + "' esta fuera de alcance en I-37C. Lo incumplen: " +
                    string.Join(", ", offenders) + ".");
            }
        }

        [Fact]
        public void TheMatrixHasNoOverrideLayerBeyondTheCell()
        {
            // One default plus cell overrides. A global, per-level or per-station override layer would be the
            // PB-014 failure: the same datum living in four places (ADR-0026, D3).
            var design = Sources()
                .Single(f => f.Path.EndsWith("CantileverStationDesign.cs", StringComparison.Ordinal))
                .Code;

            Assert.DoesNotContain("GlobalOverride", design, StringComparison.Ordinal);
            Assert.DoesNotContain("LevelOverride", design, StringComparison.Ordinal);
            Assert.DoesNotContain("StationOverride", design, StringComparison.Ordinal);
            Assert.Contains("PositiveYOverride", design, StringComparison.Ordinal);
            Assert.Contains("NegativeYOverride", design, StringComparison.Ordinal);

            // And the matrix writes overrides rather than storing state of its own.
            var matrix = Sources()
                .Single(f => f.Path.EndsWith("CantileverStationArmMatrix.cs", StringComparison.Ordinal))
                .Code;

            Assert.Contains("SetOverride", matrix, StringComparison.Ordinal);
            Assert.DoesNotMatch(new Regex(@"private\s+(readonly\s+)?List<"), matrix);
        }

        [Fact]
        public void NoArmOfAStationIsBuiltWithBothSides()
        {
            // There is no `Both`. A double station has TWO physical arms, and a single side per arm is what
            // makes the BOM count them (ADR-0025, D1).
            foreach (var file in Sources())
            {
                Assert.DoesNotMatch(new Regex(@"CantileverArmSide\s*\.\s*Both"), file.Code);
            }

            Assert.Equal(
                2,
                Enum.GetValues(typeof(RackCad.Domain.Systems.Cantilever.CantileverArmSide)).Length);
        }

        [Fact]
        public void TheFinalPassIsVerifiedRatherThanTrusted()
        {
            var resolver = Sources()
                .Single(f => f.Path.EndsWith("CantileverStationResolver.cs", StringComparison.Ordinal))
                .Code;

            Assert.Contains("VerifyFinalPassMatchesLayout", resolver, StringComparison.Ordinal);
            Assert.Contains("StationFinalPassDiffersFromLayout", resolver, StringComparison.Ordinal);
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
