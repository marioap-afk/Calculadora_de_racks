using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RackCad.Application.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    public sealed class I59F3CorrectionTests
    {
        private const string Shared = "RackCad.Application.Systems.Shared.";

        [Theory]
        [InlineData("Ok", "NotObserved", "Unknown")]
        [InlineData("Ok", "Present", "Present")]
        [InlineData("Ok", "Missing", "BlockMissing")]
        [InlineData("FileMissing", "NotObserved", "Unknown")]
        [InlineData("Unknown", "NotObserved", "Unknown")]
        public void CR59_F3_01_EXTERNAL_LIBRARY_FACTS_REQUIRE_AN_INDEPENDENT_BLOCK_OBSERVATION(
            string availability,
            string externalObservation,
            string expectedPresence)
        {
            var result = ObserveExternal("BLOCK-A", availability, externalObservation);

            Assert.Equal(availability, Property(result, "LibraryAvailability").ToString());
            Assert.Equal(expectedPresence, Property(result, "BlockPresence").ToString());
        }

        [Theory]
        [InlineData("FileMissing", "Present")]
        [InlineData("Unknown", "Missing")]
        public void CR59_F3_01_UNAVAILABLE_LIBRARY_CANNOT_PUBLISH_EXTERNAL_BLOCK_PRESENCE(
            string availability,
            string externalObservation)
        {
            var error = Assert.Throws<TargetInvocationException>(
                () => ObserveExternal("BLOCK-A", availability, externalObservation));

            Assert.IsType<InvalidOperationException>(error.InnerException);
        }

        [Theory]
        [InlineData(LibraryBlockAvailability.Found, "FileMissing")]
        [InlineData(LibraryBlockAvailability.Found, "Unknown")]
        [InlineData(LibraryBlockAvailability.Missing, "Ok")]
        public void CR59_F3_01_ACTIVE_DRAWING_FACT_NEVER_SUBSTITUTES_EXTERNAL_LIBRARY_OBSERVATION(
            LibraryBlockAvailability drawingAvailability,
            string libraryAvailability)
        {
            var drawingFact = new LibraryBlockAvailabilityFact(
                new LibraryBlockRequirement("BLOCK-A"), drawingAvailability);

            var externalFact = ObserveExternal("BLOCK-A", libraryAvailability, "NotObserved");

            Assert.Equal(drawingAvailability, drawingFact.Availability);
            Assert.Equal("Unknown", Property(externalFact, "BlockPresence").ToString());
        }

        [Fact]
        public void CR59_F3_01_PLUGIN_OBSERVER_READS_THE_EXTERNAL_LIBRARY_NOT_THE_ACTIVE_DRAWING_QUERY()
        {
            var path = Path.Combine(
                RepositoryRoot(), "src", "RackCad.Plugin", "Drawing", "AutoCadExternalLibraryBlockQuery.cs");

            Assert.True(File.Exists(path), path);
            var source = File.ReadAllText(path);
            Assert.Contains("BlockLibraryLocator.ResolvePath()", source);
            Assert.Contains("BlockLibraryDatabaseCache.Acquire", source);
            Assert.Contains("sourceTable.Has(requirement.Key)", source);
            Assert.DoesNotContain("ILibraryBlockQuery", source);
            Assert.DoesNotContain("AutoCadLibraryBlockQuery", source);
        }

        [Fact]
        public void CR59_F3_01_IMPORTER_AND_OBSERVER_SHARE_ONE_LIBRARY_DATABASE_CACHE()
        {
            var root = RepositoryRoot();
            var importer = File.ReadAllText(Path.Combine(
                root, "src", "RackCad.Plugin", "Drawing", "BlockLibraryImporter.cs"));
            var cachePath = Path.Combine(
                root, "src", "RackCad.Plugin", "Drawing", "BlockLibraryDatabaseCache.cs");

            Assert.True(File.Exists(cachePath), cachePath);
            var cache = File.ReadAllText(cachePath);
            Assert.Contains("BlockLibraryDatabaseCache.Acquire", importer);
            Assert.DoesNotContain("ReadDwgFile", importer);
            Assert.Contains("ReadDwgFile", cache);
            Assert.Contains("cachedLibrary", cache);
        }

        [Fact]
        public void CR59_F3_01_V1_IMPORT_THEN_FINAL_DRAWING_QUERY_REMAINS_AUTHORITATIVE()
        {
            var events = new List<string>();
            var requirements = new[] { new LibraryBlockRequirement("BLOCK-A") };

            var result = LibraryBlockAvailabilityFlow.Observe(
                requirements,
                true,
                new DrawingQuery(events),
                new Importer(events));

            Assert.Equal(new[] { "import", "drawing-query" }, events);
            Assert.Equal(LibraryBlockAvailability.Missing, Assert.Single(result.Facts).Availability);
        }

        private static object ObserveExternal(string key, string availability, string observation)
        {
            var facts = RequireType(Shared + "ExternalLibraryAvailabilityFacts");
            var method = Assert.Single(
                facts.GetMethods(BindingFlags.Public | BindingFlags.Static),
                candidate => candidate.Name == "Observe" && candidate.GetParameters().Length == 3);
            return method.Invoke(null, new[]
            {
                key,
                Enum.Parse(RequireType(Shared + "LibraryAvailability"), availability),
                Enum.Parse(RequireType(Shared + "ExternalLibraryBlockObservation"), observation),
            });
        }

        private static Type RequireType(string name)
        {
            var type = typeof(LibraryPieceRequirement).Assembly.GetType(name, throwOnError: false);
            Assert.NotNull(type);
            return type;
        }

        private static object Property(object target, string name)
        {
            var property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(property);
            return property.GetValue(target);
        }

        private static string RepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "RackCad.sln")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        }

        private sealed class DrawingQuery : ILibraryBlockQuery
        {
            private readonly List<string> events;

            public DrawingQuery(List<string> events) => this.events = events;

            public IReadOnlyList<LibraryBlockAvailabilityFact> Query(
                IReadOnlyList<LibraryBlockRequirement> requirements)
            {
                events.Add("drawing-query");
                return requirements.Select(requirement => new LibraryBlockAvailabilityFact(
                    requirement, LibraryBlockAvailability.Missing)).ToArray();
            }
        }

        private sealed class Importer : ILibraryBlockImporter
        {
            private readonly List<string> events;

            public Importer(List<string> events) => this.events = events;

            public LibraryBlockImportResult Ensure(IReadOnlyList<LibraryBlockRequirement> requirements)
            {
                events.Add("import");
                return new LibraryBlockImportResult(true, 0);
            }
        }
    }
}
