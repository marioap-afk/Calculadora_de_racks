using System;

namespace I52Auth15.HostHarness
{
    /// <summary>
    /// The complete, audited list of AutoCAD system variables the harness may READ with <c>GetSystemVariable</c> (HC-6). Each name has
    /// been checked against AutoCAD 2025: an invalid name raises <c>eInvalidInput</c>, which is exactly how RUN-1 was lost
    /// (<c>PROFILENAME</c> is not a system variable; the current profile is <c>CPROFILE</c>).
    ///
    /// Pure and AutoCAD-free on purpose: the offline rig compiles this file and audits the harness source against it, so a NEW name
    /// cannot enter the harness without changing this list (and therefore being reviewed). The reads themselves go through
    /// <c>SysVar.Read</c>, the only place <c>GetSystemVariable</c> is called.
    /// </summary>
    internal static class SysVarCatalog
    {
        /// <summary>name -> what it is and what the harness uses it for.</summary>
        public static readonly string[] Audited =
        {
            "ACADVER",   // AutoCAD version string: host identity, evidence
            "CPROFILE",  // current profile name: host identity, and the profile FILEDIA belongs to
            "FILEDIA",   // integer: the run.scr / harness FILEDIA gate (live value)
        };

        /// <summary>Names that were tried and are NOT valid; kept so the incident stays visible.</summary>
        public static readonly string[] KnownInvalid =
        {
            "PROFILENAME", // RUN-1: eInvalidInput in AutoCAD 2025
        };

        public static bool IsAudited(string name) => name != null && Array.IndexOf(Audited, name) >= 0;
    }
}
