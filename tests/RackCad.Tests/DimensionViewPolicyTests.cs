using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-01 y T-02 (G4) — la regla única de ADR-0035, sin sistemas ni vistas: qué detalle efectivo tiene un tipo
    /// de vista, y qué valor guarda el editor.
    ///
    /// <para>
    /// La tabla de verdad NO deriva lo esperado con operaciones de bits: para cada política dice explícitamente qué
    /// tipos quedan visibles. <c>null</c> es el legacy exacto (los tres), cualquier <c>int</c> presente se conserva y
    /// solo sus bits 1, 2 y 4 deciden: <c>13</c> es Frontal y Planta, <c>-1</c> los tres y <c>-8</c> ninguno.
    /// </para>
    /// </summary>
    public class DimensionViewPolicyTests
    {
        private static readonly IReadOnlyList<DimensionDetail> Details = new[]
        {
            DimensionDetail.None, DimensionDetail.Minimal, DimensionDetail.Standard, DimensionDetail.Detailed
        };

        private static readonly IReadOnlyList<DimensionViewKind> Kinds = new[]
        {
            DimensionViewKind.Frontal, DimensionViewKind.Lateral, DimensionViewKind.Planta
        };

        /// <summary>Política (como <c>int?</c>) → tipos visibles, escritos a mano: F = Frontal, L = Lateral, P = Planta.</summary>
        private static readonly IReadOnlyList<(int? Policy, string Visible)> TruthTable = new (int?, string)[]
        {
            (null, "FLP"),
            (0, ""),
            (1, "F"),
            (2, "L"),
            (3, "FL"),
            (4, "P"),
            (5, "FP"),
            (6, "LP"),
            (7, "FLP"),
            (8, ""),
            (13, "FP"),
            (-1, "FLP"),
            (-8, "")
        };

        private static DimensionViewVisibility? Policy(int? value)
            => value.HasValue ? (DimensionViewVisibility)value.Value : (DimensionViewVisibility?)null;

        private static char Letter(DimensionViewKind kind)
            => kind == DimensionViewKind.Frontal ? 'F' : kind == DimensionViewKind.Lateral ? 'L' : 'P';

        public static IEnumerable<object[]> EveryCombination()
            => from row in TruthTable
               from detail in Details
               from kind in Kinds
               select new object[] { row.Policy, detail, kind, row.Visible };

        // ---- T-01 — EffectiveDetail ------------------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(EveryCombination))]
        public void T01_EffectiveDetail_FollowsTheTruthTable(int? policy, DimensionDetail detail, DimensionViewKind kind, string visible)
        {
            var expected = detail == DimensionDetail.None || !visible.Contains(Letter(kind))
                ? DimensionDetail.None
                : detail;

            Assert.Equal(expected, DimensionViewPolicy.EffectiveDetail(detail, Policy(policy), kind));
        }

        [Fact]
        public void T01_TheTruthTable_CoversEveryDetailEveryKindAndTheRequiredPolicies()
        {
            var policies = TruthTable.Select(row => row.Policy).ToList();
            Assert.Contains(null, policies);
            foreach (var required in new int?[] { 0, 1, 2, 3, 4, 5, 6, 7, 13, -1, -8 })
            {
                Assert.Contains(required, policies);
            }

            Assert.Equal(TruthTable.Count * Details.Count * Kinds.Count, EveryCombination().Count());
        }

        [Fact]
        public void T01_Thirteen_DrawsFrontalAndPlanta_ButNotLateral()
        {
            var policy = (DimensionViewVisibility)13;

            Assert.Equal(DimensionDetail.Standard, DimensionViewPolicy.EffectiveDetail(DimensionDetail.Standard, policy, DimensionViewKind.Frontal));
            Assert.Equal(DimensionDetail.None, DimensionViewPolicy.EffectiveDetail(DimensionDetail.Standard, policy, DimensionViewKind.Lateral));
            Assert.Equal(DimensionDetail.Standard, DimensionViewPolicy.EffectiveDetail(DimensionDetail.Standard, policy, DimensionViewKind.Planta));
        }

        [Fact]
        public void T01_MinusOne_DrawsTheThree()
        {
            var policy = (DimensionViewVisibility)(-1);

            Assert.All(Kinds, kind =>
                Assert.Equal(DimensionDetail.Detailed, DimensionViewPolicy.EffectiveDetail(DimensionDetail.Detailed, policy, kind)));
        }

        [Fact]
        public void T01_MinusEight_DrawsNone()
        {
            var policy = (DimensionViewVisibility)(-8);

            Assert.All(Kinds, kind =>
                Assert.Equal(DimensionDetail.None, DimensionViewPolicy.EffectiveDetail(DimensionDetail.Detailed, policy, kind)));
        }

        [Fact]
        public void T01_NullPolicy_IsTheLegacyDetail_AndNoneAlwaysWins()
        {
            foreach (var detail in Details)
            {
                Assert.All(Kinds, kind => Assert.Equal(detail, DimensionViewPolicy.EffectiveDetail(detail, null, kind)));
            }

            foreach (var row in TruthTable)
            {
                Assert.All(Kinds, kind =>
                    Assert.Equal(DimensionDetail.None, DimensionViewPolicy.EffectiveDetail(DimensionDetail.None, Policy(row.Policy), kind)));
            }
        }

        [Theory]
        [InlineData(3)]
        [InlineData(-1)]
        [InlineData(99)]
        public void T01_AnUndefinedViewKind_Throws_WhateverTheDetailOrPolicy(int undefinedKind)
        {
            var kind = (DimensionViewKind)undefinedKind;

            Assert.Throws<ArgumentOutOfRangeException>(() => DimensionViewPolicy.EffectiveDetail(DimensionDetail.Standard, null, kind));
            Assert.Throws<ArgumentOutOfRangeException>(() => DimensionViewPolicy.EffectiveDetail(DimensionDetail.None, null, kind));
            Assert.Throws<ArgumentOutOfRangeException>(() => DimensionViewPolicy.EffectiveDetail(DimensionDetail.Detailed, (DimensionViewVisibility)7, kind));
        }

        // ---- T-02 — FromEditor -----------------------------------------------------------------------------------

        private static readonly IReadOnlyList<int?> LoadedValues = new int?[] { null, 0, 1, 7, 8, 13, -1, -8 };

        public static IEnumerable<object[]> EveryLoadedValueAndCheckboxes()
            => from loaded in LoadedValues
               from frontal in new[] { false, true }
               from lateral in new[] { false, true }
               from planta in new[] { false, true }
               select new object[] { loaded, frontal, lateral, planta };

        [Theory]
        [MemberData(nameof(EveryLoadedValueAndCheckboxes))]
        public void T02_Untouched_ReturnsTheLoadedValueExactly_WhateverTheCheckboxesShow(int? loaded, bool frontal, bool lateral, bool planta)
        {
            var result = DimensionViewPolicy.FromEditor(Policy(loaded), touched: false, frontal, lateral, planta);

            Assert.Equal(loaded, result.HasValue ? (int)result.Value : (int?)null);
        }

        [Theory]
        [MemberData(nameof(EveryLoadedValueAndCheckboxes))]
        public void T02_Touched_ReplacesOnlyTheThreeKnownBits_AndNeverReturnsNull(int? loaded, bool frontal, bool lateral, bool planta)
        {
            var result = DimensionViewPolicy.FromEditor(Policy(loaded), touched: true, frontal, lateral, planta);

            Assert.True(result.HasValue, "una elección tocada nunca vuelve a legacy");
            var value = (int)result.Value;
            var boxes = (frontal ? 1 : 0) + (lateral ? 2 : 0) + (planta ? 4 : 0);
            Assert.Equal(boxes, value & 7);                            // las casillas mandan en sus tres bits
            Assert.Equal((loaded ?? 0) & ~7, value & ~7);              // todo lo demás, signo incluido, sobrevive
        }

        [Fact]
        public void T02_NullUntouched_StaysNull()
        {
            Assert.Null(DimensionViewPolicy.FromEditor(null, touched: false, frontal: true, lateral: true, planta: true));
            Assert.Null(DimensionViewPolicy.FromEditor(null, touched: false, frontal: false, lateral: false, planta: false));
        }

        [Fact]
        public void T02_NullTouched_BecomesTheExplicitCheckboxes()
        {
            Assert.Equal(DimensionViewVisibility.Frontal | DimensionViewVisibility.Planta,
                DimensionViewPolicy.FromEditor(null, touched: true, frontal: true, lateral: false, planta: true));
            Assert.Equal((DimensionViewVisibility)7,
                DimensionViewPolicy.FromEditor(null, touched: true, frontal: true, lateral: true, planta: true));
            Assert.Equal(DimensionViewVisibility.None,
                DimensionViewPolicy.FromEditor(null, touched: true, frontal: false, lateral: false, planta: false));
        }

        [Fact]
        public void T02_Thirteen_KeepsItsUnknownBit()
        {
            var thirteen = (DimensionViewVisibility)13;

            Assert.Equal(13, (int)DimensionViewPolicy.FromEditor(thirteen, touched: false, frontal: false, lateral: true, planta: false));
            Assert.Equal(9, (int)DimensionViewPolicy.FromEditor(thirteen, touched: true, frontal: true, lateral: false, planta: false));
            Assert.Equal(15, (int)DimensionViewPolicy.FromEditor(thirteen, touched: true, frontal: true, lateral: true, planta: true));
            Assert.Equal(8, (int)DimensionViewPolicy.FromEditor(thirteen, touched: true, frontal: false, lateral: false, planta: false));
        }

        [Fact]
        public void T02_MinusOne_KeepsItsUnknownAndSignBits()
        {
            var minusOne = (DimensionViewVisibility)(-1);

            Assert.Equal(-1, (int)DimensionViewPolicy.FromEditor(minusOne, touched: false, frontal: false, lateral: false, planta: false));
            Assert.Equal(-1, (int)DimensionViewPolicy.FromEditor(minusOne, touched: true, frontal: true, lateral: true, planta: true));
            Assert.Equal(-8, (int)DimensionViewPolicy.FromEditor(minusOne, touched: true, frontal: false, lateral: false, planta: false));
            Assert.Equal(-3, (int)DimensionViewPolicy.FromEditor(minusOne, touched: true, frontal: true, lateral: false, planta: true));
        }

        [Fact]
        public void T02_MinusEight_KeepsItsUnknownAndSignBits()
        {
            var minusEight = (DimensionViewVisibility)(-8);

            Assert.Equal(-8, (int)DimensionViewPolicy.FromEditor(minusEight, touched: false, frontal: true, lateral: true, planta: true));
            Assert.Equal(-7, (int)DimensionViewPolicy.FromEditor(minusEight, touched: true, frontal: true, lateral: false, planta: false));
            Assert.Equal(-1, (int)DimensionViewPolicy.FromEditor(minusEight, touched: true, frontal: true, lateral: true, planta: true));
            Assert.Equal(-8, (int)DimensionViewPolicy.FromEditor(minusEight, touched: true, frontal: false, lateral: false, planta: false));
        }
    }
}
