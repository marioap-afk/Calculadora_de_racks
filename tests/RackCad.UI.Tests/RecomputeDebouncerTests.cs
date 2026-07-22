using System;
using System.Collections.Generic;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The async recompute coalescer (the configurator's model, centralized in I-15), driven by a manual
    /// scheduler so the coalescing is verified without a WPF message loop.</summary>
    public sealed class RecomputeDebouncerTests
    {
        /// <summary>Captures the scheduled flushes and runs them on demand (the test's "idle").</summary>
        private sealed class ManualScheduler : IRecomputeScheduler
        {
            private readonly List<Action> pending = new List<Action>();

            public int ScheduleCount { get; private set; }

            public void Schedule(Action flush)
            {
                ScheduleCount++;
                pending.Add(flush);
            }

            public void RunPending()
            {
                var toRun = pending.ToArray();
                pending.Clear();
                foreach (var flush in toRun)
                {
                    flush();
                }
            }
        }

        [Fact]
        public void Burst_BeforeIdle_CollapsesToOneRedraw()
        {
            var redraws = 0;
            var scheduler = new ManualScheduler();
            var debouncer = new RecomputeDebouncer(() => redraws++, scheduler);

            debouncer.Schedule();
            debouncer.Schedule();
            debouncer.Schedule();

            Assert.True(debouncer.IsQueued);
            Assert.Equal(1, scheduler.ScheduleCount); // only ONE flush queued for the whole burst
            Assert.Equal(0, redraws);                 // nothing drawn until idle

            scheduler.RunPending();

            Assert.Equal(1, redraws);                 // the burst drew exactly once
            Assert.False(debouncer.IsQueued);
        }

        [Fact]
        public void AfterFlush_ANewScheduleQueuesAgain()
        {
            var redraws = 0;
            var scheduler = new ManualScheduler();
            var debouncer = new RecomputeDebouncer(() => redraws++, scheduler);

            debouncer.Schedule();
            scheduler.RunPending();
            debouncer.Schedule();
            scheduler.RunPending();

            Assert.Equal(2, redraws);
            Assert.Equal(2, scheduler.ScheduleCount);
        }
    }
}
