using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace RackCad.UI
{
    internal sealed class RackFrameConfiguratorLayoutSettings
    {
        public double LeftWidth { get; set; }
        public double PreviewWidth { get; set; }
        public double LeftModelHeight { get; set; }
        public double LeftValidationHeight { get; set; }
        public double LeftExceptionsHeight { get; set; }
        public double CenterPropertiesHeight { get; set; }
        public double CenterQuickHeight { get; set; }
        public double CenterBulkHeight { get; set; }
        public double CenterTablesHeight { get; set; }
        public double HorizontalTableHeight { get; set; }
        public double PanelTableHeight { get; set; }
    }

    internal static class RackFrameConfiguratorLayoutStore
    {
        private static readonly string LayoutDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RackCad");

        private static readonly string LayoutPath = Path.Combine(LayoutDirectory, "RackFrameConfigurator.layout");

        public static RackFrameConfiguratorLayoutSettings Load()
        {
            if (!File.Exists(LayoutPath))
            {
                return null;
            }

            var values = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in File.ReadAllLines(LayoutPath))
            {
                var separatorIndex = line.IndexOf('=');

                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = line.Substring(0, separatorIndex).Trim();
                var value = line.Substring(separatorIndex + 1).Trim();

                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue))
                {
                    values[key] = parsedValue;
                }
            }

            return new RackFrameConfiguratorLayoutSettings
            {
                LeftWidth = GetValue(values, nameof(RackFrameConfiguratorLayoutSettings.LeftWidth)),
                PreviewWidth = GetValue(values, nameof(RackFrameConfiguratorLayoutSettings.PreviewWidth)),
                LeftModelHeight = GetValue(values, nameof(RackFrameConfiguratorLayoutSettings.LeftModelHeight)),
                LeftValidationHeight = GetValue(values, nameof(RackFrameConfiguratorLayoutSettings.LeftValidationHeight)),
                LeftExceptionsHeight = GetValue(values, nameof(RackFrameConfiguratorLayoutSettings.LeftExceptionsHeight)),
                CenterPropertiesHeight = GetValue(values, nameof(RackFrameConfiguratorLayoutSettings.CenterPropertiesHeight)),
                CenterQuickHeight = GetValue(values, nameof(RackFrameConfiguratorLayoutSettings.CenterQuickHeight)),
                CenterBulkHeight = GetValue(values, nameof(RackFrameConfiguratorLayoutSettings.CenterBulkHeight)),
                CenterTablesHeight = GetValue(values, nameof(RackFrameConfiguratorLayoutSettings.CenterTablesHeight)),
                HorizontalTableHeight = GetValue(values, nameof(RackFrameConfiguratorLayoutSettings.HorizontalTableHeight)),
                PanelTableHeight = GetValue(values, nameof(RackFrameConfiguratorLayoutSettings.PanelTableHeight))
            };
        }

        public static void Save(RackFrameConfiguratorLayoutSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            Directory.CreateDirectory(LayoutDirectory);

            var lines = new[]
            {
                Format(nameof(settings.LeftWidth), settings.LeftWidth),
                Format(nameof(settings.PreviewWidth), settings.PreviewWidth),
                Format(nameof(settings.LeftModelHeight), settings.LeftModelHeight),
                Format(nameof(settings.LeftValidationHeight), settings.LeftValidationHeight),
                Format(nameof(settings.LeftExceptionsHeight), settings.LeftExceptionsHeight),
                Format(nameof(settings.CenterPropertiesHeight), settings.CenterPropertiesHeight),
                Format(nameof(settings.CenterQuickHeight), settings.CenterQuickHeight),
                Format(nameof(settings.CenterBulkHeight), settings.CenterBulkHeight),
                Format(nameof(settings.CenterTablesHeight), settings.CenterTablesHeight),
                Format(nameof(settings.HorizontalTableHeight), settings.HorizontalTableHeight),
                Format(nameof(settings.PanelTableHeight), settings.PanelTableHeight)
            };

            File.WriteAllLines(LayoutPath, lines);
        }

        public static void Delete()
        {
            if (File.Exists(LayoutPath))
            {
                File.Delete(LayoutPath);
            }
        }

        private static double GetValue(IDictionary<string, double> values, string key)
        {
            return values.TryGetValue(key, out var value) ? value : 0.0;
        }

        private static string Format(string key, double value)
        {
            return key + "=" + value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
