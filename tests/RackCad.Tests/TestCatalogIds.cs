using System.Collections.Generic;
using System.Linq;

namespace RackCad.Tests
{
    /// <summary>
    /// Independent expectations for ids shipped with the test catalogs. These literals must not
    /// reference production defaults: the guardian compares both sources and the distributed data.
    /// </summary>
    internal static class TestCatalogIds
    {
        internal enum CatalogCollection
        {
            PostProfiles,
            TrussProfiles,
            BeamProfiles,
            SpacerProfiles,
            BasePlates,
            Mensulas,
            FlowBedProfiles,
            SafetyElements,
            ConnectionPoints,
            Views,
            Templates
        }

        internal readonly record struct CatalogExpectation(CatalogCollection Collection, string Id);
        internal readonly record struct BlockExpectation(string PieceId, string View);
        internal readonly record struct ConnectionExpectation(string PieceId, string ConnectionPointId, string View);
        internal readonly record struct BeamMensulaExpectation(string BeamId, string MensulaId);

        internal static class Profiles
        {
            internal static class Posts
            {
                internal const string Standard = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";

                internal static IReadOnlyList<CatalogExpectation> Entries { get; } = new[]
                {
                    new CatalogExpectation(CatalogCollection.PostProfiles, Standard)
                };
            }

            internal static class Truss
            {
                internal const string Standard = "TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14";

                internal static IReadOnlyList<CatalogExpectation> Entries { get; } = new[]
                {
                    new CatalogExpectation(CatalogCollection.TrussProfiles, Standard)
                };
            }

            internal static class Beams
            {
                internal const string SelectiveThreeRivet = "LARGUERO_ESCALON_CAL14_3_REMACHES";
                internal const string DynamicInOut = "LARGUERO_IN_OUT_C6";
                internal const string DynamicIntermediate = "LARGUERO_ESCALON_INFINITO";

                internal static IReadOnlyList<CatalogExpectation> Entries { get; } = new[]
                {
                    new CatalogExpectation(CatalogCollection.BeamProfiles, SelectiveThreeRivet),
                    new CatalogExpectation(CatalogCollection.BeamProfiles, DynamicInOut),
                    new CatalogExpectation(CatalogCollection.BeamProfiles, DynamicIntermediate)
                };
            }

            internal static class Spacers
            {
                internal const string Header = "SEPARADOR_DE_CABECERA_FORMADA_DE_CINTA_CALIBRE_12";

                internal static IReadOnlyList<CatalogExpectation> Entries { get; } = new[]
                {
                    new CatalogExpectation(CatalogCollection.SpacerProfiles, Header)
                };
            }
        }

        internal static class BasePlates
        {
            internal const string Standard = "PLACA_BASE_DE_CABECERA_ATORNILLABLE_DE_PLACA_CALIBRE_3_16";

            internal static IReadOnlyList<CatalogExpectation> Entries { get; } = new[]
            {
                new CatalogExpectation(CatalogCollection.BasePlates, Standard)
            };
        }

        internal static class Mensulas
        {
            internal const string ThreeRivet = "MENSULA_3_REMACHES_CAL_10";
            internal const string RoundPunch = "MENSULA_TROQUEL_REDONDO_CAL_10";
            internal const string InfiniteAdjustment = "MENSULA_AJUSTE_INFINITO_CAL_10";

            internal static IReadOnlyList<CatalogExpectation> Entries { get; } = new[]
            {
                new CatalogExpectation(CatalogCollection.Mensulas, ThreeRivet),
                new CatalogExpectation(CatalogCollection.Mensulas, RoundPunch),
                new CatalogExpectation(CatalogCollection.Mensulas, InfiniteAdjustment)
            };
        }

        internal static class FlowBed
        {
            internal const string Rail = "RIEL_DE_CINTA_CALIBRE_12";
            internal const string Stop = "TOPE_DE_CAMA_DE_RODILLOS_DE_PLACA_CALIBRE_3_16";
            internal const string Brake = "FRENO_TIPO_RODILLO_DE_TUBO_DE_3_1_8";
            internal const string Roller1Point9 = "RODILLO_DE_TUBO_DE_1.9_CALIBRE_14";
            internal const string Roller2Point5 = "RODILLO_DE_TUBO_DE_2.5_CALIBRE_14";

            internal static IReadOnlyList<CatalogExpectation> Entries { get; } = new[]
            {
                new CatalogExpectation(CatalogCollection.FlowBedProfiles, Rail),
                new CatalogExpectation(CatalogCollection.FlowBedProfiles, Stop),
                new CatalogExpectation(CatalogCollection.FlowBedProfiles, Brake),
                new CatalogExpectation(CatalogCollection.FlowBedProfiles, Roller1Point9),
                new CatalogExpectation(CatalogCollection.FlowBedProfiles, Roller2Point5)
            };
        }

        internal static class Safety
        {
            internal static class Boots
            {
                internal const string H3_16_18 = "PROTECTOR_BOTA_H_3_16_18";
                internal const string C4 = "PROTECTOR_BOTA_C_4";
                internal const string C6 = "PROTECTOR_BOTA_C_6";

                internal static IReadOnlyList<CatalogExpectation> Entries { get; } = SafetyEntries(
                    H3_16_18, C4, C6);
            }

            internal static class SideProtectors
            {
                internal const string H3_16_18 = "PROTECTOR_LATERAL_BOTA_H_3_16_18";
                internal const string C4 = "PROTECTOR_LATERAL_BOTA_C_4";
                internal const string C6 = "PROTECTOR_LATERAL_BOTA_C_6";

                internal static IReadOnlyList<CatalogExpectation> Entries { get; } = SafetyEntries(
                    H3_16_18, C4, C6);
            }

            internal static class Stops
            {
                internal const string Beam = "LARGUERO_ESCALON_TOPE_DE_3";
                internal const string Post = "POSTE_3_1_5_8_TOPE";

                internal static IReadOnlyList<CatalogExpectation> Entries { get; } = SafetyEntries(Beam, Post);
            }

            internal static class Decks
            {
                internal const string Generic = "PARRILLA_GENERICA";

                internal static IReadOnlyList<CatalogExpectation> Entries { get; } = SafetyEntries(Generic);
            }

            internal static class Deviators
            {
                internal const string A3 = "DESVIADOR_A_3";
                internal const string A4 = "DESVIADOR_A_4";
                internal const string L3 = "DESVIADOR_L_3";
                internal const string L3Point5 = "DESVIADOR_L_3_5";
                internal const string L4 = "DESVIADOR_L_4";
                internal const string L4Point5 = "DESVIADOR_L_4_5";
                internal const string L5 = "DESVIADOR_L_5";

                internal static IReadOnlyList<CatalogExpectation> Entries { get; } = SafetyEntries(
                    A3, A4, L3, L3Point5, L4, L4Point5, L5);
            }

            internal static class Dynamic
            {
                internal const string EntranceGuide = "GUIA_ENTRADA";
                internal const string ForkliftDefense = "DEFENSA_MONTACARGAS";

                internal static IReadOnlyList<CatalogExpectation> Entries { get; } = SafetyEntries(
                    EntranceGuide, ForkliftDefense);
            }

            internal static IReadOnlyList<CatalogExpectation> Entries { get; } =
                Boots.Entries
                    .Concat(SideProtectors.Entries)
                    .Concat(Stops.Entries)
                    .Concat(Decks.Entries)
                    .Concat(Deviators.Entries)
                    .Concat(Dynamic.Entries)
                    .ToArray();

            private static IReadOnlyList<CatalogExpectation> SafetyEntries(params string[] ids)
            {
                return ids.Select(id => new CatalogExpectation(CatalogCollection.SafetyElements, id)).ToArray();
            }
        }

        internal static class ConnectionPoints
        {
            internal const string PostMount = "MONTAJE_POSTE";
            internal const string TrussPunch = "TROQUEL_CELOSIA";
            internal const string TrussEnd = "CELOSIA";
            internal const string PostEnd = "FIN_POSTE";
            internal const string SpacerPunch = "TROQUEL_SEPARADOR";
            internal const string StopPunch = "TROQUEL_TOPE";
            internal const string HeaderPunch = "TROQUEL_CABECERA";
            internal const string BeamPunch = "TROQUEL_LARGUERO";
            internal const string ProfileStart = "INICIO_PERFIL";
            internal const string RailInOut = "TROQUEL_IN";
            internal const string BedMate = "TROQUEL_CAMA";
            internal const string LeftStart = "INICIO_IZQUIERDO";
            internal const string RightStart = "INICIO_DERECHO";
            internal const string PostOrigin = "ORIGEN_POSTE";

            internal static IReadOnlyList<CatalogExpectation> Entries { get; } = new[]
            {
                new CatalogExpectation(CatalogCollection.ConnectionPoints, PostMount),
                new CatalogExpectation(CatalogCollection.ConnectionPoints, TrussPunch),
                new CatalogExpectation(CatalogCollection.ConnectionPoints, TrussEnd),
                new CatalogExpectation(CatalogCollection.ConnectionPoints, PostEnd),
                new CatalogExpectation(CatalogCollection.ConnectionPoints, SpacerPunch),
                new CatalogExpectation(CatalogCollection.ConnectionPoints, StopPunch),
                new CatalogExpectation(CatalogCollection.ConnectionPoints, HeaderPunch),
                new CatalogExpectation(CatalogCollection.ConnectionPoints, BeamPunch),
                new CatalogExpectation(CatalogCollection.ConnectionPoints, ProfileStart),
                new CatalogExpectation(CatalogCollection.ConnectionPoints, RailInOut),
                new CatalogExpectation(CatalogCollection.ConnectionPoints, BedMate),
                new CatalogExpectation(CatalogCollection.ConnectionPoints, LeftStart),
                new CatalogExpectation(CatalogCollection.ConnectionPoints, RightStart),
                new CatalogExpectation(CatalogCollection.ConnectionPoints, PostOrigin)
            };
        }

        internal static class Views
        {
            internal const string Front = "FRONTAL";
            internal const string Lateral = "LATERAL";
            internal const string Plan = "PLANTA";

            internal static IReadOnlyList<CatalogExpectation> Entries { get; } = new[]
            {
                new CatalogExpectation(CatalogCollection.Views, Front),
                new CatalogExpectation(CatalogCollection.Views, Lateral),
                new CatalogExpectation(CatalogCollection.Views, Plan)
            };
        }

        internal static class Templates
        {
            internal const string Standard = "STD-3P";
            internal const string Compact = "COMPACT-2P";
            internal const string Tall = "TALL-4P";

            internal static IReadOnlyList<CatalogExpectation> Entries { get; } = new[]
            {
                new CatalogExpectation(CatalogCollection.Templates, Standard),
                new CatalogExpectation(CatalogCollection.Templates, Compact),
                new CatalogExpectation(CatalogCollection.Templates, Tall)
            };
        }

        internal static class BlockOnlyPieces
        {
            internal const string Pallet = "TARIMA_GENERICA";
        }

        internal static IReadOnlyList<CatalogExpectation> AllCatalogEntries { get; } =
            Profiles.Posts.Entries
                .Concat(Profiles.Truss.Entries)
                .Concat(Profiles.Beams.Entries)
                .Concat(Profiles.Spacers.Entries)
                .Concat(BasePlates.Entries)
                .Concat(Mensulas.Entries)
                .Concat(FlowBed.Entries)
                .Concat(Safety.Entries)
                .Concat(ConnectionPoints.Entries)
                .Concat(Views.Entries)
                .Concat(Templates.Entries)
                .ToArray();

        internal static IReadOnlyList<BlockExpectation> EssentialBlocks { get; } = BuildEssentialBlocks();

        internal static IReadOnlyList<ConnectionExpectation> EssentialConnections { get; } = new[]
        {
            new ConnectionExpectation(BasePlates.Standard, ConnectionPoints.PostMount, Views.Front),
            new ConnectionExpectation(BasePlates.Standard, ConnectionPoints.PostMount, Views.Lateral),
            new ConnectionExpectation(BasePlates.Standard, ConnectionPoints.PostMount, Views.Plan),
            new ConnectionExpectation(Profiles.Truss.Standard, ConnectionPoints.TrussEnd, Views.Lateral),
            new ConnectionExpectation(Profiles.Truss.Standard, ConnectionPoints.TrussEnd, Views.Plan),
            new ConnectionExpectation(Profiles.Posts.Standard, ConnectionPoints.TrussPunch, Views.Lateral),
            new ConnectionExpectation(Profiles.Posts.Standard, ConnectionPoints.TrussPunch, Views.Plan),
            new ConnectionExpectation(Profiles.Posts.Standard, ConnectionPoints.PostEnd, Views.Lateral),
            new ConnectionExpectation(Profiles.Posts.Standard, ConnectionPoints.StopPunch, Views.Front),
            new ConnectionExpectation(Profiles.Posts.Standard, ConnectionPoints.StopPunch, Views.Lateral),
            new ConnectionExpectation(Profiles.Posts.Standard, ConnectionPoints.StopPunch, Views.Plan),
            new ConnectionExpectation(Profiles.Posts.Standard, ConnectionPoints.SpacerPunch, Views.Lateral),
            new ConnectionExpectation(Profiles.Posts.Standard, ConnectionPoints.SpacerPunch, Views.Plan),
            new ConnectionExpectation(Profiles.Posts.Standard, ConnectionPoints.BeamPunch, Views.Front),
            new ConnectionExpectation(Profiles.Posts.Standard, ConnectionPoints.BeamPunch, Views.Plan),
            new ConnectionExpectation(Profiles.Spacers.Header, ConnectionPoints.HeaderPunch, Views.Front),
            new ConnectionExpectation(Profiles.Spacers.Header, ConnectionPoints.HeaderPunch, Views.Plan),
            new ConnectionExpectation(FlowBed.Rail, ConnectionPoints.StopPunch, Views.Lateral),
            new ConnectionExpectation(FlowBed.Rail, ConnectionPoints.RailInOut, Views.Lateral),
            new ConnectionExpectation(Profiles.Beams.SelectiveThreeRivet, ConnectionPoints.ProfileStart, Views.Front),
            new ConnectionExpectation(Profiles.Beams.SelectiveThreeRivet, ConnectionPoints.ProfileStart, Views.Plan),
            new ConnectionExpectation(Profiles.Beams.DynamicIntermediate, ConnectionPoints.ProfileStart, Views.Front),
            new ConnectionExpectation(Profiles.Beams.DynamicIntermediate, ConnectionPoints.ProfileStart, Views.Plan),
            new ConnectionExpectation(Profiles.Beams.DynamicIntermediate, ConnectionPoints.LeftStart, Views.Lateral),
            new ConnectionExpectation(Profiles.Beams.DynamicIntermediate, ConnectionPoints.RightStart, Views.Lateral),
            new ConnectionExpectation(Profiles.Beams.DynamicInOut, ConnectionPoints.ProfileStart, Views.Front),
            new ConnectionExpectation(Profiles.Beams.DynamicInOut, ConnectionPoints.ProfileStart, Views.Plan),
            new ConnectionExpectation(Profiles.Beams.DynamicInOut, ConnectionPoints.BedMate, Views.Lateral),
            new ConnectionExpectation(Safety.Dynamic.ForkliftDefense, ConnectionPoints.PostOrigin, Views.Front),
            new ConnectionExpectation(Safety.Dynamic.ForkliftDefense, ConnectionPoints.PostOrigin, Views.Lateral),
            new ConnectionExpectation(Safety.Dynamic.ForkliftDefense, ConnectionPoints.PostOrigin, Views.Plan)
        };

        internal static IReadOnlyList<BeamMensulaExpectation> EssentialBeamMensulas { get; } = new[]
        {
            new BeamMensulaExpectation(Profiles.Beams.SelectiveThreeRivet, Mensulas.ThreeRivet),
            new BeamMensulaExpectation(Profiles.Beams.DynamicInOut, Mensulas.RoundPunch),
            new BeamMensulaExpectation(Profiles.Beams.DynamicIntermediate, Mensulas.InfiniteAdjustment)
        };

        private static IReadOnlyList<BlockExpectation> BuildEssentialBlocks()
        {
            var threeViewPieces = Profiles.Posts.Entries
                .Concat(Profiles.Truss.Entries)
                .Concat(Profiles.Beams.Entries)
                .Concat(Profiles.Spacers.Entries)
                .Concat(BasePlates.Entries)
                .Concat(Mensulas.Entries)
                .Concat(Safety.Entries)
                .Select(entry => entry.Id);

            var blocks = threeViewPieces.SelectMany(ThreeViewBlocks).ToList();
            blocks.AddRange(FlowBed.Entries.Select(entry => new BlockExpectation(entry.Id, Views.Lateral)));
            blocks.Add(new BlockExpectation(BlockOnlyPieces.Pallet, Views.Front));
            blocks.Add(new BlockExpectation(BlockOnlyPieces.Pallet, Views.Lateral));
            return blocks;
        }

        private static IEnumerable<BlockExpectation> ThreeViewBlocks(string pieceId)
        {
            yield return new BlockExpectation(pieceId, Views.Front);
            yield return new BlockExpectation(pieceId, Views.Lateral);
            yield return new BlockExpectation(pieceId, Views.Plan);
        }
    }
}
