using RackCad.Application.Settings;
using RackCad.Application.VentanaRackCad;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 al (I-64, prototype «almacen», question 6): the last values of the page «Almacén», per user. An interface of its own, next to
    /// <see cref="IRackCadSettingsAccess"/>, so a settings access that does not have it (the doubles of the tests before this part) keeps the
    /// values only while the window lives, and never touches the settings of the machine.
    /// </summary>
    public interface IRackCadWarehouseSettings
    {
        /// <summary>The values saved last; null when they were never saved.</summary>
        WarehouseLayoutValues WarehouseValues { get; }

        void SetWarehouseValues(WarehouseLayoutValues values);
    }

    /// <summary>The settings of the machine for the page «Almacén», through the same store (<c>%APPDATA%\RackCad\settings.json</c>).</summary>
    public sealed partial class RackCadSettingsAccess : IRackCadWarehouseSettings
    {
        public WarehouseLayoutValues WarehouseValues => WarehouseLayoutSettingsMap.ToValues(UserSettingsStore.Load().WarehouseLayout);

        public void SetWarehouseValues(WarehouseLayoutValues values)
        {
            var settings = UserSettingsStore.Load();
            settings.WarehouseLayout = WarehouseLayoutSettingsMap.ToPreferences(values);
            UserSettingsStore.Save(settings);
        }
    }

    /// <summary>T5 al: the values of the page and what the settings keep of them. A field never saved takes the value of the classic window.</summary>
    public static class WarehouseLayoutSettingsMap
    {
        public static WarehouseLayoutValues ToValues(WarehouseLayoutPreferences saved)
        {
            if (saved == null) return null;
            var d = WarehouseLayoutValues.Defaults;
            return new WarehouseLayoutValues(
                saved.Rows ?? d.Rows,
                saved.Cols ?? d.Cols,
                saved.AisleRows ?? d.AisleRows,
                saved.AisleCols ?? d.AisleCols,
                saved.BackToBack ?? d.BackToBack,
                saved.BackGap ?? d.BackGap,
                saved.Independent ?? d.Independent,
                saved.BuildingX ?? d.BuildingX,
                saved.BuildingY ?? d.BuildingY,
                string.IsNullOrWhiteSpace(saved.Layer) ? d.Layer : saved.Layer,
                saved.WallClearance ?? d.WallClearance,
                saved.ColumnClearance ?? d.ColumnClearance,
                saved.TryRotated ?? d.TryRotated);
        }

        public static WarehouseLayoutPreferences ToPreferences(WarehouseLayoutValues values)
        {
            if (values == null) return null;
            return new WarehouseLayoutPreferences
            {
                Rows = values.Rows,
                Cols = values.Cols,
                AisleRows = values.AisleRows,
                AisleCols = values.AisleCols,
                BackToBack = values.BackToBack,
                BackGap = values.BackGap,
                Independent = values.Independent,
                BuildingX = values.BuildingX,
                BuildingY = values.BuildingY,
                Layer = values.Layer,
                WallClearance = values.WallClearance,
                ColumnClearance = values.ColumnClearance,
                TryRotated = values.TryRotated,
            };
        }
    }
}
