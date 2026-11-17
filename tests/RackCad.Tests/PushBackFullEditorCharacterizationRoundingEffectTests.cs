using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T0 (CT-PB0-21) — EFECTO del redondeo de la perdida (a), para la decision OB-06.4 del paquete. La ventana
    /// escribe los controles con «0.###», asi que un valor con mas de tres decimales vuelve redondeado a tres. Aqui se
    /// mide, con el producto real y sin ventana, que cambia en las vistas, la estructura resuelta y el BOM cuando un campo
    /// entra con su valor EXACTO (E) frente a entrar con su valor redondeado a tres decimales (R).
    ///
    /// <para>«Material» es la formula mecanica del paquete (K1 no vacia, o K2, o K3, o K4 no vacia): esta prueba no juzga
    /// si el redondeo es aceptable, y la decision de OB-06.4 es del Owner. Solo se fijan claves, enteros, cadenas y
    /// booleanos; las magnitudes se comparan con los umbrales declarados 0.001 y 1e-9. Orden ordinal, cultura invariante.</para>
    /// </summary>
    public sealed class PushBackFullEditorCharacterizationRoundingEffectTests
    {
        private const double Threshold = 0.001;
        private const double Epsilon = 1e-9;

        private static readonly RackCatalog Catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string Fixture(string name)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.True(dir != null, "repo root (RackCad.sln) not found");
            var path = Path.Combine(dir.FullName, "tests", "RackCad.Tests", "Fixtures", "I64", name + ".json");
            Assert.True(File.Exists(path), "missing fixture " + path);
            return File.ReadAllText(path);
        }

        /// <summary>R3(v): el valor tal como lo devuelve un control formateado con «0.###».</summary>
        private static double R3(double value)
            => double.Parse(value.ToString("0.###", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

        /// <summary>Una fila de la tabla de campos de MB H-077: ruta en el JSON interior y valor base.</summary>
        private sealed class Field
        {
            public string Row;
            public string[] Path;
            public double Base;
            public bool KeepAsIs;
        }

        private static string[] Structure(params string[] tail) => new[] { "PushBack", "Structure" }.Concat(tail).ToArray();

        // F5 y F8 en pb-08 ya llevan mas de tres decimales: su valor exacto es el que el diseño trae (tal cual).
        private static readonly Field[] Fields =
        {
            new Field { Row = "F1", Path = Structure("PalletDepth"), Base = 48 },
            new Field { Row = "F2", Path = Structure("PostPeralte"), Base = 3 },
            new Field { Row = "F3", Path = Structure("PalletTolerance"), Base = 4 },
            new Field { Row = "F4", Path = Structure("BeamDepth"), Base = 6 },
            new Field { Row = "F5", Path = Structure("AnnotationScale"), Base = 1.23456, KeepAsIs = true },
            new Field { Row = "F6", Path = Structure("Fronts", "0", "FirstLevelHeight"), Base = 4 },
            new Field { Row = "F7", Path = Structure("Fronts", "0", "Levels", "0", "PalletFront"), Base = 42 },
            new Field { Row = "F8", Path = Structure("Fronts", "0", "Levels", "0", "PalletHeight"), Base = 61.23456, KeepAsIs = true },
            new Field { Row = "F9", Path = Structure("Fronts", "0", "Levels", "0", "PalletWeight"), Base = 1000 },
            new Field { Row = "F10", Path = Structure("Fronts", "0", "Levels", "0", "ClearHeight"), Base = 6 },
            new Field { Row = "F11", Path = Structure("Fronts", "0", "Levels", "0", "BeamLengthOverride"), Base = 100 },
        };

        private static void SetPath(JsonNode root, string[] path, double value)
        {
            var node = root;
            for (var i = 0; i < path.Length - 1; i++)
            {
                node = int.TryParse(path[i], NumberStyles.None, CultureInfo.InvariantCulture, out var index) ? node[index] : node[path[i]];
            }

            node[path[path.Length - 1]] = value;
        }

        /// <summary>El diseño de pb-08 con las ediciones dadas sobre su JSON interior, deserializado por el producto.</summary>
        private static PushBackDesign Design(IEnumerable<KeyValuePair<string[], double>> edits)
        {
            var embed = new RackEmbedStore().Deserialize(Fixture("pb-08-redondeo"));
            var inner = JsonNode.Parse(embed.Design);
            foreach (var edit in edits) SetPath(inner, edit.Key, edit.Value);
            var design = new RackProjectStore().Deserialize(inner.ToJsonString()).PushBackDesign;
            Assert.NotNull(design);
            return design;
        }

        // ---- Computo, claves de plan y firmas ---------------------------------------------------------------------

        private sealed class Result
        {
            public bool Valid;
            public string Error;
            public PushBackEditorComputation Computation;
            public Dictionary<string, List<HeaderBlockInstance>> Plans;
        }

        private static Result Compute(PushBackDesign design)
        {
            var computation = new PushBackEditorDesignAssembler(Catalog).BuildFrom(design, PushBackSide.A);
            if (!computation.IsValid) return new Result { Valid = false, Error = computation.Error };
            var plans = new Dictionary<string, List<HeaderBlockInstance>>(StringComparer.Ordinal)
            {
                ["planta"] = computation.PlantaPlan.Flatten().Instances.ToList(),
                ["frontal-es"] = computation.FrontalEntradaSalida.Flatten().Instances.ToList(),
                ["frontal-post"] = computation.FrontalPosterior.Flatten().Instances.ToList(),
                ["lateral"] = computation.LateralPlan.Flatten().Instances.ToList(),
            };
            foreach (var corte in computation.LateralCortes) plans["corte-" + corte.PostIndex.ToString(CultureInfo.InvariantCulture)] = corte.Plan.Flatten().Instances.ToList();
            return new Result { Valid = true, Computation = computation, Plans = plans };
        }

        private static string Signature(HeaderBlockInstance instance)
            => string.Join(
                "/",
                instance.Role.ToString(),
                instance.PieceId ?? "<null>",
                instance.BlockName ?? "<null>",
                instance.View ?? "<null>",
                instance.MirroredX ? "mx" : "-",
                instance.MirroredY ? "my" : "-",
                string.Join(",", instance.DynamicParameters.Keys.OrderBy(key => key, StringComparer.Ordinal)));

        private static List<string> Keys(Result e, Result r)
            => e.Plans.Keys.Union(r.Plans.Keys).OrderBy(key => key, StringComparer.Ordinal).ToList();

        private static double MaxDelta(HeaderBlockInstance a, HeaderBlockInstance b, out double geometry, out double other)
        {
            // geometry: Insertion, ConnectionAnchor y DynamicParameters (K4); other: TextHeight, DimensionOffset y RotationRadians (K5).
            geometry = Math.Max(
                Math.Max(Math.Abs(a.Insertion.X - b.Insertion.X), Math.Abs(a.Insertion.Y - b.Insertion.Y)),
                Math.Max(Math.Abs(a.ConnectionAnchor.X - b.ConnectionAnchor.X), Math.Abs(a.ConnectionAnchor.Y - b.ConnectionAnchor.Y)));
            foreach (var key in a.DynamicParameters.Keys)
            {
                geometry = Math.Max(geometry, Math.Abs(a.DynamicParameters[key] - b.DynamicParameters[key]));
            }

            other = Math.Max(
                Math.Abs(a.TextHeight - b.TextHeight),
                Math.Max(Math.Abs(a.DimensionOffset - b.DimensionOffset), Math.Abs(a.RotationRadians - b.RotationRadians)));
            return Math.Max(geometry, other);
        }

        /// <summary>Los instantes de una fila: K1, K1o, K2, K3, K4, K4', K5, «material» y el maximo |delta| medido.</summary>
        private sealed class Effect
        {
            public List<string> K1 = new List<string>();
            public List<string> K1o = new List<string>();
            public bool K2;
            public bool K3;
            public List<string> K4 = new List<string>();
            public List<string> K4Prime = new List<string>();
            public List<string> K5 = new List<string>();
            public double MaxDelta;
            public bool Material => K1.Count > 0 || K2 || K3 || K4.Count > 0;
        }

        private static Effect Measure(Result e, Result r)
        {
            var effect = new Effect();
            foreach (var key in Keys(e, r))
            {
                var left = e.Plans.TryGetValue(key, out var l) ? l : new List<HeaderBlockInstance>();
                var right = r.Plans.TryGetValue(key, out var rr) ? rr : new List<HeaderBlockInstance>();
                var leftSignatures = left.Select(Signature).ToList();
                var rightSignatures = right.Select(Signature).ToList();

                // K1: el multiconjunto de firmas difiere.
                if (!leftSignatures.OrderBy(s => s, StringComparer.Ordinal).SequenceEqual(rightSignatures.OrderBy(s => s, StringComparer.Ordinal), StringComparer.Ordinal))
                {
                    effect.K1.Add(key);
                    continue;
                }

                // K1o: el multiconjunto coincide y el orden no. Esas claves SI se emparejan, por grupo de firma y orden de aparicion.
                var sameOrder = leftSignatures.SequenceEqual(rightSignatures, StringComparer.Ordinal);
                if (!sameOrder) effect.K1o.Add(key);

                var pairs = new List<(HeaderBlockInstance A, HeaderBlockInstance B)>();
                if (sameOrder)
                {
                    for (var i = 0; i < left.Count; i++) pairs.Add((left[i], right[i]));
                }
                else
                {
                    foreach (var group in leftSignatures.Distinct(StringComparer.Ordinal))
                    {
                        var lefts = left.Where((x, i) => leftSignatures[i] == group).ToList();
                        var rights = right.Where((x, i) => rightSignatures[i] == group).ToList();
                        for (var i = 0; i < lefts.Count; i++) pairs.Add((lefts[i], rights[i]));
                    }
                }

                var geometry = 0.0;
                var other = 0.0;
                foreach (var pair in pairs)
                {
                    MaxDelta(pair.A, pair.B, out var g, out var o);
                    geometry = Math.Max(geometry, g);
                    other = Math.Max(other, o);
                }

                if (geometry > Threshold) effect.K4.Add(key);
                if (geometry > Epsilon) effect.K4Prime.Add(key);
                if (other > Epsilon) effect.K5.Add(key);
                effect.MaxDelta = Math.Max(effect.MaxDelta, Math.Max(geometry, other));
            }

            // K2: por frente del sistema resuelto, el numero de niveles, el de posiciones o la lista de PostIndex.
            effect.K2 = FrontShape(e) != FrontShape(r);

            // K3: las lineas del BOM, con todas sus propiedades publicas y en el orden producido.
            effect.K3 = BomLines(e) != BomLines(r);
            return effect;
        }

        private static string FrontShape(Result result)
            => string.Join(
                ";",
                result.Computation.System.Structure.Fronts.Select(front => front.LoadLevels.ToString(CultureInfo.InvariantCulture)
                    + "/" + front.PalletCount.ToString(CultureInfo.InvariantCulture)
                    + "/" + string.Join(",", result.Computation.LateralCortes.Select(corte => corte.PostIndex.ToString(CultureInfo.InvariantCulture)))));

        private static string BomLines(Result result)
        {
            var lines = new List<string>();
            foreach (var line in result.Computation.Bom.Lines)
            {
                var values = line.GetType().GetProperties()
                    .Where(property => property.GetIndexParameters().Length == 0)
                    .Select(property => property.Name + "=" + Convert.ToString(property.GetValue(line), CultureInfo.InvariantCulture));
                lines.Add(string.Join(";", values));
            }

            return string.Join("\n", lines);
        }

        private static string Label(Effect effect)
            => "K1=[" + string.Join(",", effect.K1) + "] K1o=[" + string.Join(",", effect.K1o) + "] K2=" + effect.K2
               + " K3=" + effect.K3 + " K4=[" + string.Join(",", effect.K4) + "] K4'=[" + string.Join(",", effect.K4Prime) + "]"
               + " K5=[" + string.Join(",", effect.K5) + "] material=" + effect.Material;

        private const string AllPlans = "corte-0,corte-1,corte-2,corte-3,frontal-es,frontal-post,lateral,planta";

        [Fact]
        public void CT_PB0_21_TheEffectOfRoundingEachFieldToThreeDecimalsOnPlansStructureAndBomIsTheOneTheFormulaGives()
        {
            var untouched = Compute(Design(Array.Empty<KeyValuePair<string[], double>>()));
            Assert.True(untouched.Valid, untouched.Error);

            // R0: pb-08 tal cual (E) frente a pb-08 con sus dos campos en R3 (R).
            var f5 = Fields.Single(field => field.Row == "F5");
            var f8 = Fields.Single(field => field.Row == "F8");
            var r0 = Compute(Design(new[]
            {
                new KeyValuePair<string[], double>(f5.Path, R3(f5.Base)),
                new KeyValuePair<string[], double>(f8.Path, R3(f8.Base)),
            }));
            Assert.True(r0.Valid, r0.Error);
            var rows = new List<string> { "R0 " + Label(Measure(untouched, r0)) };

            // R1 a R11: sobre pb-08, la fila Fn con su valor exacto (E) y con R3 (R).
            for (var n = 0; n < Fields.Length; n++)
            {
                var field = Fields[n];
                var exact = field.KeepAsIs ? field.Base : field.Base + 0.00044;
                var e = Compute(Design(new[] { new KeyValuePair<string[], double>(field.Path, exact) }));
                var r = Compute(Design(new[] { new KeyValuePair<string[], double>(field.Path, R3(exact)) }));
                Assert.True(e.Valid, e.Error);
                Assert.True(r.Valid, r.Error);
                rows.Add("R" + (n + 1) + " " + field.Row + " " + Label(Measure(e, r)));
            }

            // R5 y R8 llevan el valor de pb-08 tal cual (E) y su R3 (R): el mismo par de R0 repartido en sus dos campos.
            Assert.Equal(
                new[]
                {
                    "R0 K1=[] K1o=[] K2=False K3=False K4=[" + AllPlans + "] K4'=[" + AllPlans + "] K5=[" + AllPlans + "] material=True",
                    "R1 F1 K1=[] K1o=[] K2=False K3=False K4=[] K4'=[] K5=[] material=False",
                    "R2 F2 K1=[] K1o=[] K2=False K3=False K4=[frontal-es,frontal-post,planta] K4'=[frontal-es,frontal-post,planta] K5=[] material=True",
                    "R3 F3 K1=[] K1o=[] K2=False K3=False K4=[] K4'=[] K5=[] material=False",
                    "R4 F4 K1=[] K1o=[] K2=False K3=False K4=[] K4'=[] K5=[] material=False",
                    "R5 F5 K1=[] K1o=[] K2=False K3=False K4=[" + AllPlans + "] K4'=[" + AllPlans + "] K5=[" + AllPlans + "] material=True",
                    "R6 F6 K1=[] K1o=[] K2=False K3=False K4=[] K4'=[] K5=[] material=False",
                    "R7 F7 K1=[] K1o=[frontal-post,planta] K2=False K3=True K4=[planta] K4'=[corte-0,corte-1,frontal-es,frontal-post,planta] K5=[] material=True",
                    "R8 F8 K1=[] K1o=[] K2=False K3=False K4=[] K4'=[] K5=[] material=False",
                    "R9 F9 K1=[] K1o=[] K2=False K3=False K4=[] K4'=[] K5=[] material=False",
                    "R10 F10 K1=[] K1o=[] K2=False K3=False K4=[] K4'=[] K5=[] material=False",
                    "R11 F11 K1=[] K1o=[] K2=False K3=True K4=[] K4'=[corte-0,corte-1,frontal-es,frontal-post,planta] K5=[] material=True",
                },
                rows);
        }
    }
}
