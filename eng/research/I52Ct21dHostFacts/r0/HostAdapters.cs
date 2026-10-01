using System.Diagnostics;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using I52Ct21d.HostFacts.Core;
using CoreApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace I52Ct21d.HostFacts.R0;

/// <summary>
/// I-4 / DBMOD reader: <c>GetSystemVariable</c> ONLY (SetSystemVariable is a forbidden symbol). The value is returned exactly as the
/// host returns it; the runtime type is the fact (design HF-G3). HOST-TO-CONFIRM: the runtime types on the exact build.
/// </summary>
internal sealed class AcadSystemVariables : ISystemVariables
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
internal sealed class HostEnvironment : IRunEnvironment
{
    private readonly Process _process = Process.GetCurrentProcess();

    public DateTime UtcNow => DateTime.UtcNow;

    public int ProcessId => _process.Id;

    public string ProcessStartUtc => _process.StartTime.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
}
