using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// F4-S2-03 text guard: RedefineSystemBlock must re-assign the position of every direct reference of the
    /// redefined definition, inside the loop and after the ForWrite open, so that U and REDO repaint the screen.
    /// Pure source-text guard over the code without comments; it never loads the Plugin nor AutoCAD.
    /// </summary>
    public sealed class RedefinedViewReferenceUndoTouchGuardTests
    {
        private const string MethodMarker = "public LateralHeaderDrawOutcome RedefineSystemBlock(";

        // The no-op assignment, whitespace tolerant: <name>.Position = <name>.Position;
        private static readonly Regex TouchStatement = new Regex(
            @"\b(\w+)\s*\.\s*Position\s*=\s*\1\s*\.\s*Position\s*;", RegexOptions.CultureInvariant);

        // The loop over the direct references of the definition, the ForWrite open of the loop variable, and the
        // touch of THAT SAME variable later in the same loop body (no closing brace in between).
        private static readonly Regex TouchInsideDirectReferenceLoop = new Regex(
            @"foreach\s*\(\s*ObjectId\s+(?<id>\w+)\s+in\s+\w+\s*\.\s*GetBlockReferenceIds\s*\(\s*directOnly\s*:\s*true[^)]*\)\s*\)\s*\{"
            + @"[^}]*?\bvar\s+(?<ref>\w+)\s*=\s*\(\s*BlockReference\s*\)\s*tr\s*\.\s*GetObject\s*\(\s*\k<id>\s*,\s*OpenMode\s*\.\s*ForWrite\s*\)\s*;"
            + @"[^}]*?\b\k<ref>\s*\.\s*Position\s*=\s*\k<ref>\s*\.\s*Position\s*;",
            RegexOptions.CultureInvariant);

        private static string RedefineBody()
        {
            var drawer = I55ProductCharacterizationTestSupport.Code("src", "RackCad.Plugin", "Drawing", "LateralHeaderDrawer.cs");
            return I55ProductCharacterizationTestSupport.Body(drawer, MethodMarker);
        }

        private static bool TouchIsInsideTheDirectReferenceLoop(string body)
        {
            return TouchInsideDirectReferenceLoop.IsMatch(body);
        }

        [Fact]
        public void LaSentenciaEstaDentroDelBucleDeReferenciasDirectasTrasLaAperturaForWrite()
        {
            var body = RedefineBody();

            Assert.True(
                TouchIsInsideTheDirectReferenceLoop(body),
                "RedefineSystemBlock must run '<reference>.Position = <reference>.Position;' inside the loop over "
                + "GetBlockReferenceIds(directOnly: true ...), after opening the same variable ForWrite (F4-S2-03).");
        }

        [Fact]
        public void LaSentenciaApareceExactamenteUnaVezEnElCuerpoDeRedefineSystemBlock()
        {
            var body = RedefineBody();

            Assert.Single(TouchStatement.Matches(body));
        }

        [Fact]
        public void AutopruebaQuitarLaSentenciaHaceQueElComprobadorLaRechace()
        {
            var body = RedefineBody();

            var mutated = TouchStatement.Replace(body, string.Empty);

            Assert.NotEqual(body, mutated);
            Assert.False(TouchIsInsideTheDirectReferenceLoop(mutated));
        }

        [Fact]
        public void AutopruebaMoverLaSentenciaFueraDelBucleHaceQueElComprobadorLaRechace()
        {
            var body = RedefineBody();

            // Take the statement out of wherever it is and put it right after the opening brace of the method,
            // which is outside every loop. In the RED (no statement yet) this only inserts it outside.
            var withoutTouch = TouchStatement.Replace(body, string.Empty);
            var mutated = withoutTouch.Insert(1, " reference.Position = reference.Position; ");

            Assert.NotEqual(body, mutated);
            Assert.False(TouchIsInsideTheDirectReferenceLoop(mutated));
        }
    }
}
