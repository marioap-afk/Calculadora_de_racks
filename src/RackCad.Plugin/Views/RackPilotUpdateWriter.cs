#nullable enable
using System;
using System.Collections.Generic;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Views.Redraw;

namespace RackCad.Plugin.Views
{
    /// <summary>
    /// I-64 F4-T4B (D-T4B-03) — the writer of the update of the bounded Push Back pilot. Skeleton of the RED step: the three classes exist and
    /// every body is inert. This class is the port: it keeps nothing, and every <see cref="Begin"/> hands back a NEW scope.
    /// </summary>
    internal sealed class RackPilotUpdateWriter : IPushBackPilotUpdatePort
    {
        public IPushBackPilotUpdateScope Begin(PushBackPilotUpdateRequest request)
        {
            return new RackPilotUpdateScope();
        }
    }

    /// <summary>The scope of one run: PREPARE, the start of MUTATE and POST. Inert.</summary>
    internal sealed class RackPilotUpdateScope : IPushBackPilotUpdateScope
    {
        public PushBackPilotScanValues ReadForPrepare()
        {
            return new PushBackPilotScanValues(false, Array.Empty<RackSiblingScanFact>(), new Dictionary<string, string>());
        }

        public PushBackPilotImportResult PrepareUnits(PushBackPilotUpdatePlan plan)
        {
            return PushBackPilotImportResult.Failed("inert");
        }

        public PushBackPilotMutationStart BeginMutation()
        {
            return PushBackPilotMutationStart.Rejected("inert");
        }

        public PushBackPilotPostValues Post(PushBackPilotPostRequest request)
        {
            return new PushBackPilotPostValues(false, null, null, null);
        }

        public void Dispose()
        {
        }
    }

    /// <summary>The ONE owned scope of MUTATE. Inert.</summary>
    internal sealed class RackPilotMutationScope : IPushBackPilotMutationScope
    {
        public PushBackPilotScanValues Reread()
        {
            return new PushBackPilotScanValues(false, Array.Empty<RackSiblingScanFact>(), new Dictionary<string, string>());
        }

        public PushBackPilotUnitResult WriteUnit(string memberKey, string envelopeText)
        {
            return new PushBackPilotUnitResult(Array.Empty<string>());
        }

        public void Seal()
        {
        }

        public void Dispose()
        {
        }
    }
}
