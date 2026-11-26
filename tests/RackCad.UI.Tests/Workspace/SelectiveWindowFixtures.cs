#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.VentanaRackCad;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// The Selectivo racks of the tests of the editor of the window (Selectivo E1-B): generic sample values of the classic editor, no data
    /// of a client. The racks open as the door reads them (<see cref="RackDoorRules.AnswerForSelectiveOpen"/>), so the editor starts from
    /// exactly what RACKSELECTIVOABRIR would give it.
    /// </summary>
    internal static class SelectiveWindowFixtures
    {
        public const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        public const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";
        public const string RackId = "SEL-1";
        public const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";

        public static readonly PropertyId Clearance = ProjectPropertyIds.SelectiveVerticalClearance;
        public static readonly PropertyId Tolerance = ProjectPropertyIds.SelectivePalletTolerance;
        public static readonly SelectivePalletDesignStore Store = new SelectivePalletDesignStore();

        public static SelectiveMatrixPosition P(int front, int level) => new SelectiveMatrixPosition(front, level);

        public static SelectiveCell Cell(double frente = 42.0, double alto = 60.0, int count = 2)
            => new SelectiveCell { Pallet = new Tarima { Frente = frente, Alto = alto }, PalletCount = count, BeamId = BeamId, BeamPeralte = 4.0 };

        public static SelectiveBayDesign Bay(int levels, bool floor = false, double frente = 42.0, double alto = 60.0)
        {
            var bay = new SelectiveBayDesign { FloorBeam = floor, FloorBeamRiseOverride = 4.0 };
            for (var level = 0; level < levels; level++) bay.Levels.Add(Cell(frente, alto));
            return bay;
        }

        /// <summary>One fondo: F1 with 3 levels and «Larguero a piso», F2 with 2 levels on the floor; an own peralte on P1.</summary>
        public static SelectivePalletDesign Structure()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, FloorBeamRise = 4.0,
                PalletDepth = 48.0, DepthCount = 1, DrawBasePlate = true,
            };
            design.Bays.Add(Bay(3, floor: true, frente: 40.0));
            design.Bays.Add(Bay(2, frente: 44.0, alto: 55.0));
            foreach (var peralte in new[] { 5.0, 0.0, 0.0 }) design.PostPeraltes.Add(peralte);
            design.CabeceraFondoOverrides.Add(0.0);
            return design;
        }

        /// <summary>Two fondos: fondo 1 as <see cref="Structure"/> (48 in deep); fondo 2 has three fronts of 3 levels and is 40 in deep.</summary>
        public static SelectivePalletDesign TwoFondos()
        {
            var design = Structure();
            design.DepthCount = 2;
            design.SeparatorLengths.Add(10.0);
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(3, floor: true, frente: 40.0), Bay(3, frente: 44.0, alto: 55.0), Bay(3) });
            design.ExtraFondoDepths.Add(40.0);
            design.CabeceraFondoOverrides.Add(0.0);
            design.PostPeraltes.Add(0.0);
            return design;
        }

        public static ProjectVariablesReadResult Register(double value)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                new ProjectVariableDocument
                {
                    VariableId = VarId,
                    Name = "Holgura estándar",
                    Type = VariableType.Length.ToString(),
                    Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
                },
            };
            return ProjectVariablesReadResult.Readable(document);
        }

        /// <summary>The authored document of the rack in the drawing; bound = the vertical clearance governed by the project variable.</summary>
        public static string Authored(SelectivePalletDesign design, bool bound = false)
        {
            var document = SelectivePalletDesignDocument.From(design, RackId, "Selectivo A");
            if (bound)
            {
                document.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
                {
                    [ProjectPropertyIds.SelectiveVerticalClearanceToken] = SelectivePropertyValueDocument.ToProjectVariable(VarId),
                };
                document.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return Store.Serialize(document);
        }

        /// <summary>What RACKSELECTIVOABRIR answers for that rack (the rule of the door, as the command runs it).</summary>
        public static SelectiveDesignRead Read(long requestId, string rackId, SelectivePalletDesign? design = null, bool bound = false, ProjectVariablesReadResult? register = null)
        {
            var read = RackDoorRules.AnswerForSelectiveOpen(requestId, rackId, RackEmbedDocument.KindSelective, Authored(design ?? Structure(), bound),
                register ?? (bound ? Register(10.0) : ProjectVariablesReadResult.Absent()));
            Assert.True(read.Opens, read.Reason);
            return read;
        }

        /// <summary>The editor of the window over that rack, opened as the window opens it.</summary>
        public static SelectiveEditorViewModel SelectiveEditor(SelectivePalletDesign? design = null, Action<SelectiveEditorViewModel>? update = null, bool bound = false,
            IReadOnlyList<string>? dimensionStyles = null)
        {
            var read = Read(1, RackId, design, bound);
            var draft = new SelectiveDraftEditor(UiSupport.LoadCatalogSafe());
            Assert.True(draft.LoadFrom(read.EffectiveDesign()!, read.LinkedPropertyStates, read.Name, read.LinkedVariableNames), draft.LastError);
            return new SelectiveEditorViewModel(draft, RackId, update ?? (_ => { }), read.LinkedPropertyStates, read.LinkedVariableNames, dimensionStyles);
        }

        /// <summary>A custom cabecera of the post of the rack, as the configurator gives it.</summary>
        public static RackFrameConfiguration Custom(double height)
            => new RackFrameConfigurationFactory(UiSupport.LoadCatalogSafe()).Build(RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, height, 42.0);

        public static DynamicFieldItem Field(IEnumerable<object> fields, string key) => fields.OfType<DynamicFieldItem>().Single(f => f.Key == key);

        public static SelectiveChoiceItem Choice(IEnumerable<object> fields, string key) => fields.OfType<SelectiveChoiceItem>().Single(f => f.Key == key);

        public static void Write(DynamicFieldItem field, string text)
        {
            field.Text = text;
            field.Commit();
        }

        public static string Json(SelectivePalletDesign? design) => Store.Serialize(SelectivePalletDesignDocument.From(design!, null, null));
    }
}
