using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T1c — CT-PBT1C-03: the SHAPE of the classic Push Back window once it keeps its editable state in
    /// <c>PushBackEditableState</c> (decision D-PBT1-13 of the package of PB-T1). It reads the window (W) and its composite
    /// part (WC) as text, without comments, and fixes what the move to the state must leave: no field with storage for the
    /// names that became forwarders, no <c>SideAOnly</c> and no <c>LoadCompositeFromDesign</c> (the state loads), no
    /// <c>ParkDormantTail(</c> (the state parks), the output criterion delegated to <c>PushBackEditorOutputGate</c>, the
    /// carriers adopted first thing in <c>LoadFromModel</c>, the inputs of a pass built by the state and the test seam over the
    /// state. The expectation never changes to make it pass: the window does.
    /// </summary>
    public sealed class PushBackWindowOnStateSourceGuardTests
    {
        private const string WindowPath = "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs";
        private const string CompositePath = "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs";

        /// <summary>The names of D-PBT1-13 (1) and (2): each one stays as a forwarder, never as a field with storage.</summary>
        private static readonly string[] Forwarded =
        {
            "composite", "safetySelections", "generalPallet", "rackTolerance", "rackPostPeralte", "rackBeamDepth", "rackAnnotationScale",
            "advanced", "firstLevelDatum", "loadedDimensionViews", "loadedDimensionStyle", "dimensionViewsTouched",
        };

        private static string Root => PilotKit.RepoRoot().FullName;

        /// <summary>The text without comments, with the string and character literals emptied (as the source guards do).</summary>
        private static string Clean(string source)
        {
            source = Regex.Replace(source, "@\"(?:[^\"]|\"\")*\"", "\"\"");
            source = Regex.Replace(source, "\\$?\"(?:\\\\.|[^\"\\\\\\n])*\"", "\"\"");
            source = Regex.Replace(source, "'(?:\\\\.|[^'\\\\\\n])'", "''");
            source = Regex.Replace(source, @"/\*.*?\*/", " ", RegexOptions.Singleline);
            source = Regex.Replace(source, @"//[^\n]*", string.Empty);
            return source;
        }

        private static string Collapse(string text)
            => string.Join(" ", text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));

        /// <summary>The text of a member, from its signature to its closing brace.</summary>
        private static string MethodText(string text, string signature)
        {
            var start = text.IndexOf(signature, StringComparison.Ordinal);
            if (start < 0) return string.Empty;
            var open = text.IndexOf('{', start);
            if (open < 0) return string.Empty;
            var depth = 0;
            for (var index = open; index < text.Length; index++)
            {
                if (text[index] == '{') depth++;
                else if (text[index] == '}' && --depth == 0) return text.Substring(start, index - start + 1);
            }

            return string.Empty;
        }

        /// <summary>A field with storage: a modifier, a type and the name, with or without an initializer that is not an arrow.</summary>
        private static bool DeclaresStorage(string text, string name)
            => Regex.IsMatch(text, @"\b(?:private|internal|public|protected)\s+(?:static\s+)?(?:readonly\s+)?[\w<>\[\]?,.]+\s+" + Regex.Escape(name) + @"\s*(?:=(?!>)[^;]*)?;");

        [Fact]
        public void CT_PBT1C_03_TheWindowKeepsItsEditableStateInThePushBackEditableState()
        {
            var window = Collapse(Clean(File.ReadAllText(Path.Combine(Root, WindowPath))));
            var composite = Collapse(Clean(File.ReadAllText(Path.Combine(Root, CompositePath))));
            var both = window + " " + composite;

            var log = new List<string>();

            // No field with storage for any of the forwarded names.
            var storage = Forwarded.Where(name => DeclaresStorage(window, name) || DeclaresStorage(composite, name)).ToList();
            log.Add("campos-con-almacenamiento=" + storage.Count);

            // The window no longer declares SideAOnly nor LoadCompositeFromDesign, and nothing parks the tail itself.
            log.Add("SideAOnly-declarado=" + Regex.IsMatch(both, @"\b(?:private|internal|public)\s+(?:static\s+)?[\w<>\[\]?,.]+\s+SideAOnly\s*\("));
            log.Add("LoadCompositeFromDesign-presente=" + both.Contains("LoadCompositeFromDesign"));
            log.Add("ParkDormantTail-presente=" + both.Contains("ParkDormantTail("));

            // The output criterion is the one of Application.
            log.Add("CompositeHasBlocking-usa-IsBlocked=" + MethodText(composite, "bool CompositeHasBlocking(").Contains("PushBackEditorOutputGate.IsBlocked("));

            // The carriers are adopted first thing in the try of LoadFromModel.
            var loadFromModel = MethodText(window, "void LoadFromModel(");
            log.Add("LoadFromModel-AdoptCarriers-primera=" + Regex.IsMatch(loadFromModel, @"^[^{]*\{ suppressSync = true; try \{ editable\.AdoptCarriers\(inputs, catalog\);"));

            // The inputs of a pass are built by the state, and the seam is the state itself.
            log.Add("ReadInputs-usa-BuildInputs=" + window.Contains("editable.BuildInputs(assembler)"));
            log.Add("costura-es-el-estado=" + window.Contains("EditableStateForTest => editable;"));

            Assert.Equal(
                new[]
                {
                    "campos-con-almacenamiento=0",
                    "SideAOnly-declarado=False",
                    "LoadCompositeFromDesign-presente=False",
                    "ParkDormantTail-presente=False",
                    "CompositeHasBlocking-usa-IsBlocked=True",
                    "LoadFromModel-AdoptCarriers-primera=True",
                    "ReadInputs-usa-BuildInputs=True",
                    "costura-es-el-estado=True",
                },
                log);
        }
    }
}
