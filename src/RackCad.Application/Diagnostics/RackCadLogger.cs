using System;
using System.Globalization;
using System.IO;
using Serilog;

namespace RackCad.Application.Diagnostics
{
    public static class RackCadLogger
    {
        private static readonly object SyncRoot = new object();
        private static bool configured;

        public static string LogDirectory
        {
            get
            {
                var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                if (string.IsNullOrWhiteSpace(baseDirectory))
                {
                    baseDirectory = Path.GetTempPath();
                }

                return Path.Combine(baseDirectory, "RackCad", "logs");
            }
        }

        public static void Configure()
        {
            if (configured)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (configured)
                {
                    return;
                }

                Directory.CreateDirectory(LogDirectory);

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.File(
                        Path.Combine(LogDirectory, "rackcad-.log"),
                        formatProvider: CultureInfo.InvariantCulture,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 14,
                        shared: true)
                    .CreateLogger();

                configured = true;
                Log.Information("RackCad logging configured at {LogDirectory}", LogDirectory);
            }
        }

        public static void Information(string messageTemplate, params object[] propertyValues)
        {
            Configure();
            Log.Information(messageTemplate, propertyValues);
        }

        public static void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Configure();
            Log.Error(exception, messageTemplate, propertyValues);
        }

        public static void CloseAndFlush()
        {
            if (!configured)
            {
                return;
            }

            Log.CloseAndFlush();
            configured = false;
        }
    }
}
