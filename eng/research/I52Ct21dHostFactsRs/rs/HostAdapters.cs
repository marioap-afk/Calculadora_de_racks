using System.Diagnostics;
using System.Globalization;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;
using CoreApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace I52Ct21d.HostFacts.Rs;

/// <summary>
/// System-variable reader (DBMOD, SECURELOAD, TRUSTEDPATHS, CPROFILE, DWGPREFIX, DWGNAME): <c>GetSystemVariable</c> ONLY (SetSystemVariable is a
/// forbidden symbol). The same shape as the R0 reader.
/// </summary>
internal sealed class RsSystemVariables : ISystemVariables
{
    public SysVarReading Read(string name)
    {
        try
        {
            var value = CoreApp.GetSystemVariable(name);
            if (value is null) return new SysVarReading(name, null, null, null, "NULL_VALUE");
            var type = value.GetType().FullName ?? value.GetType().Name;
            if (value is double d)
                return new SysVarReading(name, type, d.ToString("R", CultureInfo.InvariantCulture), DoubleBits.Hex(d), null);
            return new SysVarReading(name, type, Convert.ToString(value, CultureInfo.InvariantCulture), null, null);
        }
        catch (System.Exception ex)
        {
            return new SysVarReading(name, null, null, null, ex.GetType().Name);
        }
    }
}

/// <summary>Process id, process start and clock. <c>Process.GetCurrentProcess</c> is allowed; <c>Process.Start</c> is a forbidden symbol.</summary>
internal sealed class RsEnvironment : IRunEnvironment
{
    private readonly Process _process = Process.GetCurrentProcess();

    public DateTime UtcNow => DateTime.UtcNow;

    public int ProcessId => _process.Id;

    public string ProcessStartUtc => _process.StartTime.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
}

// The three adapters below only forward to SideDbWriter / SideDbReader / ScratchSaver: they hold no AutoCAD call of their own (the scan allows
// none in them). A SideDbHandle never leaves the adapter.

internal sealed class TolScaleHostAdapter : ITolScaleHost
{
    private readonly SideDbWriter _writer;
    private readonly SideDbReader _reader;
    private readonly ScratchSaver _saver;

    internal TolScaleHostAdapter(SideDbWriter writer, SideDbReader reader, ScratchSaver saver)
    {
        _writer = writer;
        _reader = reader;
        _saver = saver;
    }

    public ITolScaleDatabase CreateSideDatabase()
    {
        var side = _writer.CreateScratch("TOLSCALE_PROBE");
        try { _writer.BuildTolScaleTarget(side); }
        catch (System.Exception)
        {
            side.Dispose();
            throw;
        }
        return new Db(_writer, _reader, _saver, side);
    }

    private sealed class Db : ITolScaleDatabase
    {
        private readonly SideDbWriter _writer;
        private readonly SideDbReader _reader;
        private readonly ScratchSaver _saver;
        private readonly SideDbHandle _side;

        internal Db(SideDbWriter writer, SideDbReader reader, ScratchSaver saver, SideDbHandle side)
        {
            _writer = writer;
            _reader = reader;
            _saver = saver;
            _side = side;
        }

        public string Id => _side.Id;

        public IReadOnlyList<RowConstruction> AppendReferences(IReadOnlyList<ProbeRow> rows) => _writer.AppendScaledReferences(_side, rows);

        public IReadOnlyList<ReferenceReading> ReadReferences() => _reader.ReadScaleReferences(_side);

        public ScratchFileEntry SaveToScratch(ScratchTarget target) => _saver.SaveNew(_side, target);

        public ITolScaleDatabase Reopen(ReadableSource source) => new Db(_writer, _reader, _saver, _writer.OpenForRead(source));

        public void Dispose() => _side.Dispose();
    }
}

internal sealed class WriteBackHostAdapter : IWriteBackHost
{
    private readonly SideDbWriter _writer;
    private readonly SideDbReader _reader;
    private readonly ScratchSaver _saver;

    internal WriteBackHostAdapter(SideDbWriter writer, SideDbReader reader, ScratchSaver saver)
    {
        _writer = writer;
        _reader = reader;
        _saver = saver;
    }

    public IWriteBackDatabase CreateSideDatabase() => new Db(_writer, _reader, _saver, _writer.CreateScratch("DIMENSION_WRITEBACK_PROBE"));

    private sealed class Db : IWriteBackDatabase
    {
        private readonly SideDbWriter _writer;
        private readonly SideDbReader _reader;
        private readonly ScratchSaver _saver;
        private readonly SideDbHandle _side;

        internal Db(SideDbWriter writer, SideDbReader reader, ScratchSaver saver, SideDbHandle side)
        {
            _writer = writer;
            _reader = reader;
            _saver = saver;
            _side = side;
        }

        public string Id => _side.Id;

        public IReadOnlyList<StyleValue> ReadStyleValues() => _reader.ReadStyleValues(_side);

        public IReadOnlyList<ScenarioConstruction> AppendScenarios(IReadOnlyList<WriteBackScenario> scenarios) => _writer.AppendDimensionScenarios(_side, scenarios);

        public IReadOnlyList<DimensionReading> ReadDimensions() => _reader.ReadDimensions(_side);

        public ScratchFileEntry SaveToScratch(ScratchTarget target) => _saver.SaveNew(_side, target);

        public IWriteBackDatabase Reopen(ReadableSource source) => new Db(_writer, _reader, _saver, _writer.OpenForRead(source));

        public void Dispose() => _side.Dispose();
    }
}

internal sealed class DynCensusHostAdapter : IDynCensusHost
{
    private readonly SideDbWriter _writer;
    private readonly SideDbReader _reader;

    internal DynCensusHostAdapter(SideDbWriter writer, SideDbReader reader)
    {
        _writer = writer;
        _reader = reader;
    }

    public IDynCensusSession OpenPrivateCopy(ReadableSource source)
    {
        var privateCopy = _writer.OpenForRead(source);
        try
        {
            var scratch = _writer.CreateScratch("DYNAMIC_CENSUS_SCRATCH");
            return new Session(_writer, _reader, privateCopy, scratch);
        }
        catch (System.Exception)
        {
            privateCopy.Dispose();
            throw;
        }
    }

    private sealed class Session : IDynCensusSession
    {
        private readonly SideDbWriter _writer;
        private readonly SideDbReader _reader;
        private readonly SideDbHandle _privateCopy;
        private readonly SideDbHandle _scratch;

        internal Session(SideDbWriter writer, SideDbReader reader, SideDbHandle privateCopy, SideDbHandle scratch)
        {
            _writer = writer;
            _reader = reader;
            _privateCopy = privateCopy;
            _scratch = scratch;
        }

        public IReadOnlyList<string> ListDynamicDefinitionNames() => _reader.ListDynamicDefinitionNames(_privateCopy);

        public int CountBlockRecords() => _reader.CountBlockRecords(_privateCopy);

        public DynamicBlockProbe Probe(string blockName)
        {
            _writer.CloneDefinition(_privateCopy, _scratch, blockName);
            var handle = _writer.InsertReference(_scratch, blockName);
            return new DynamicBlockProbe(blockName, _reader.ReadDynamicProperties(_scratch, handle), null);
        }

        public void Dispose()
        {
            try { _scratch.Dispose(); }
            finally { _privateCopy.Dispose(); }
        }
    }
}
