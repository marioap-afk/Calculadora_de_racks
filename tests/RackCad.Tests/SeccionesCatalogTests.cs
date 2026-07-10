using System;
using System.IO;
using System.Linq;
using RackCad.Application.Catalogs;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The unified secciones.csv (posts + celosía + beams in one sheet, split by "rol") must populate the SAME
    /// typed lists the legacy three files did, and the legacy files must still load when secciones is absent.
    /// </summary>
    public class SeccionesCatalogTests
    {
        [Fact]
        public void Load_UnifiedSecciones_PopulatesTheThreeLegacyLists()
        {
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();

            // Posts: the omega post with its dimensions and the Ix/Iy/norma columns still in the open bag.
            var post = catalog.PostProfiles.FindProfile("POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA");
            Assert.NotNull(post);
            Assert.Equal(3.0, post.Width, 4);
            Assert.Equal(3.0, post.Depth, 4);
            Assert.Equal("14", post.Gauge);

            // Celosía: the travesaño with its accented display name intact (UTF-8 path).
            var truss = catalog.TrussProfiles.FindProfile("TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14");
            Assert.NotNull(truss);
            Assert.Contains("Travesaño", truss.Label);

            // Beams: peraltes list and ménsula FK survive the unified read.
            var beam = catalog.BeamProfiles.FirstOrDefault(b => b.Id == "LARGUERO_ESCALON_CAL14_3_REMACHES");
            Assert.NotNull(beam);
            Assert.Equal("3;3.5;4;4.5;5;5.5;6", beam.Peraltes);
            Assert.Equal("MENSULA_3_REMACHES_CAL_10", beam.Mensula);
            Assert.Contains("escalón", beam.Label);
        }

        [Fact]
        public void Load_LegacySplitFiles_StillWorkWhenSeccionesIsAbsent()
        {
            var dir = Path.Combine(Path.GetTempPath(), "RackCadLegacyCat_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "post-profiles.csv"),
                    "id,displayName,width\nPOSTE_LEGACY,Poste legacy,3\n");
                File.WriteAllText(Path.Combine(dir, "truss-profiles.csv"),
                    "id,displayName\nCELOSIA_LEGACY,Celosia legacy\n");
                File.WriteAllText(Path.Combine(dir, "beam-profiles.csv"),
                    "id,displayName,peraltes,mensula\nBEAM_LEGACY,Larguero legacy,4;5,MENSULA_X\n");

                var catalog = new JsonRackCatalogProvider(dir).Load();

                Assert.NotNull(catalog.PostProfiles.FindProfile("POSTE_LEGACY"));
                Assert.NotNull(catalog.TrussProfiles.FindProfile("CELOSIA_LEGACY"));
                Assert.Equal("4;5", catalog.BeamProfiles.Single(b => b.Id == "BEAM_LEGACY").Peraltes);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void Load_UnifiedRow_WithUnknownRol_IsSkippedNotFatal()
        {
            var dir = Path.Combine(Path.GetTempPath(), "RackCadRolCat_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "secciones.csv"),
                    "rol,id,displayName,peraltes\nPOSTE,P1,Poste uno,\nRARO,X1,Rol desconocido,\nLARGUERO,B1,Beam uno,4;5\n");

                var catalog = new JsonRackCatalogProvider(dir).Load();

                Assert.Single(catalog.PostProfiles);
                Assert.Empty(catalog.TrussProfiles);
                Assert.Single(catalog.BeamProfiles);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
