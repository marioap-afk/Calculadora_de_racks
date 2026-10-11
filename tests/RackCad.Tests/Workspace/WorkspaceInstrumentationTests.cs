#nullable enable
using System;
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T3A-HOST-CORE: INV-F1-T3A-11 (in-memory instrumentation: switch, timings, process memory, measurement points).
public class WorkspaceInstrumentationTests
{
    private sealed class Clock
    {
        public double Now { get; set; }

        public int Reads { get; private set; }

        public double Read()
        {
            Reads++;
            return Now;
        }
    }

    private sealed class Memory
    {
        public long Bytes { get; set; } = 1_000;

        public int Reads { get; private set; }

        public long Read()
        {
            Reads++;
            return Bytes;
        }
    }

    private static (WorkspaceInstrumentation Instrumentation, Clock Clock, Memory Memory) Make()
    {
        var clock = new Clock();
        var memory = new Memory();
        return (new WorkspaceInstrumentation(clock.Read, memory.Read), clock, memory);
    }

    [Fact]
    public void ItIsOffByDefault()
    {
        Assert.False(new WorkspaceInstrumentation().Enabled);
        Assert.False(new WorkspaceInstrumentation().Report().Enabled);
    }

    [Fact]
    public void WhileOffNothingIsCollectedAndNeitherTheClockNorTheMemoryIsRead()
    {
        var (instrumentation, clock, memory) = Make();

        using (instrumentation.Begin(WorkspaceMeasurementPoints.Drain, 3)) { }
        instrumentation.Record(WorkspaceMeasurementPoints.PanelOpen, 4.5);

        Assert.Empty(instrumentation.Report().Points);
        Assert.Equal(0, clock.Reads);
        Assert.Equal(0, memory.Reads);
    }

    [Fact]
    public void TheSwitchTurnsCollectionOnAndOff()
    {
        var (instrumentation, _, _) = Make();

        instrumentation.Enabled = true;
        instrumentation.Record("one", 1);
        instrumentation.Enabled = false;
        instrumentation.Record("two", 2);
        instrumentation.Enabled = true;
        instrumentation.Record("three", 3);

        Assert.Equal(new[] { "one", "three" }, instrumentation.Report().Points.Select(p => p.Point).ToArray());
    }

    [Fact]
    public void AScopeRecordsTheElapsedMillisecondsOnDispose()
    {
        var (instrumentation, clock, _) = Make();
        instrumentation.Enabled = true;

        clock.Now = 100;
        var scope = instrumentation.Begin(WorkspaceMeasurementPoints.SelectionRead, 7);
        clock.Now = 112.5;
        scope.Dispose();

        var point = Assert.Single(instrumentation.Report().Points);
        Assert.Equal(WorkspaceMeasurementPoints.SelectionRead, point.Point);
        Assert.Equal(12.5, point.Milliseconds, 6);
        Assert.Equal(7, point.Count);
    }

    [Fact]
    public void EachPointCarriesTheProcessMemoryAtThatMoment()
    {
        var (instrumentation, _, memory) = Make();
        instrumentation.Enabled = true;

        memory.Bytes = 5_000;
        instrumentation.Record("a", 1);
        memory.Bytes = 9_000;
        instrumentation.Record("b", 1);

        Assert.Equal(new long[] { 5_000, 9_000 }, instrumentation.Report().Points.Select(p => p.ProcessMemoryBytes).ToArray());
    }

    [Fact]
    public void PointsAreOrderedBySequence()
    {
        var (instrumentation, _, _) = Make();
        instrumentation.Enabled = true;

        instrumentation.Record("a", 1);
        instrumentation.Record("b", 1);
        instrumentation.Record("c", 1);

        var sequences = instrumentation.Report().Points.Select(p => p.Sequence).ToArray();
        Assert.Equal(sequences.OrderBy(s => s).ToArray(), sequences);
        Assert.Equal(3, sequences.Distinct().Count());
    }

    [Fact]
    public void TheReportReadsTheCurrentProcessMemory()
    {
        var (instrumentation, _, memory) = Make();
        instrumentation.Enabled = true;
        memory.Bytes = 123_456;

        Assert.Equal(123_456, instrumentation.Report().CurrentProcessMemoryBytes);
    }

    [Fact]
    public void WithTheDefaultProbeTheMemoryOfTheRealProcessIsPositive()
    {
        var instrumentation = new WorkspaceInstrumentation();
        instrumentation.Enabled = true;

        Assert.True(instrumentation.Report().CurrentProcessMemoryBytes > 0);
    }

    [Fact]
    public void AReportIsASnapshotLaterPointsDoNotChange()
    {
        var (instrumentation, _, _) = Make();
        instrumentation.Enabled = true;
        instrumentation.Record("a", 1);

        var report = instrumentation.Report();
        instrumentation.Record("b", 1);

        Assert.Single(report.Points);
        Assert.Equal(2, instrumentation.Report().Points.Count);
    }

    [Fact]
    public void ClearEmptiesThePointsButKeepsTheSwitch()
    {
        var (instrumentation, _, _) = Make();
        instrumentation.Enabled = true;
        instrumentation.Record("a", 1);

        instrumentation.Clear();

        Assert.Empty(instrumentation.Report().Points);
        Assert.True(instrumentation.Enabled);
    }

    [Fact]
    public void TheCollectorKeepsOnlyTheMostRecentPointsSoMemoryIsBounded()
    {
        var (instrumentation, _, _) = Make();
        instrumentation.Enabled = true;

        for (var i = 0; i < WorkspaceInstrumentation.Capacity + 50; i++) instrumentation.Record("p" + i, i);

        var points = instrumentation.Report().Points;
        Assert.Equal(WorkspaceInstrumentation.Capacity, points.Count);
        Assert.Equal("p" + (WorkspaceInstrumentation.Capacity + 49), points[^1].Point);
        Assert.Equal("p50", points[0].Point);
    }

    [Fact]
    public void AScopeOpenedWhileOffRecordsNothingEvenIfTheSwitchTurnsOnBeforeItEnds()
    {
        var (instrumentation, clock, _) = Make();

        var scope = instrumentation.Begin("late", 1);
        instrumentation.Enabled = true;
        clock.Now = 10;
        scope.Dispose();

        Assert.Empty(instrumentation.Report().Points);
    }

    [Fact]
    public void DisposingAScopeTwiceRecordsItOnce()
    {
        var (instrumentation, _, _) = Make();
        instrumentation.Enabled = true;

        var scope = instrumentation.Begin("once", 1);
        scope.Dispose();
        scope.Dispose();

        Assert.Single(instrumentation.Report().Points);
    }

    [Fact]
    public void ANegativeElapsedTimeIsRecordedAsZero()
    {
        var (instrumentation, clock, _) = Make();
        instrumentation.Enabled = true;

        clock.Now = 50;
        var scope = instrumentation.Begin("clock-went-back");
        clock.Now = 40;
        scope.Dispose();

        Assert.Equal(0, Assert.Single(instrumentation.Report().Points).Milliseconds);
    }

    [Fact]
    public void ThePointNamesAreStableStructuredNamesWithoutSpaces()
    {
        var names = typeof(WorkspaceMeasurementPoints)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToArray();

        Assert.True(names.Length >= 6);
        Assert.All(names, n => Assert.Matches("^[a-z]+(\\.[a-z]+)*$", n));
        Assert.Equal(names.Length, names.Distinct().Count());
    }

    [Fact]
    public void ARecordedPointKeepsItsCount()
    {
        var (instrumentation, _, _) = Make();
        instrumentation.Enabled = true;

        instrumentation.Record(WorkspaceMeasurementPoints.Drain, 2.0, 41);

        Assert.Equal(41, Assert.Single(instrumentation.Report().Points).Count);
    }
}
