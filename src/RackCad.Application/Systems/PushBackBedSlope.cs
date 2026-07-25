using System;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// PB-004 (I-32) — THE Push Back bed slope, in one pure function that the axis, the drawing, the BOM and the UI
    /// all consume. Owner decision (2026-07-25): the bed rises <b>7/16" per commercial foot of rack</b>, so a 204"
    /// rack rises exactly <b>7.4375" (7 7/16")</b>.
    ///
    /// Before this rule existed, the bed slope was an EMERGENT of two unrelated mechanisms and nobody owned it: the
    /// two end elevations were snapped to the 2" troquel INDEPENDENTLY (losing the fractional part of the rise), and
    /// the axis then jumped between two different catalog datums — the low beam's <c>TROQUEL_CAMA</c> and the rear
    /// beam's <c>INICIO_DERECHO</c>, 4.9342" apart — which added a rise nobody asked for. On the Owner's rack that
    /// compounded to 11.2".
    ///
    /// The rise is measured per unit of HORIZONTAL run, which is what an AutoCAD vertical dimension over the rack
    /// reads. (The drawn bed is the same line rotated onto its own commercial length, so its rise along that length
    /// is <c>L·sin θ</c> rather than <c>L·tan θ</c> — 7.4326" instead of 7.4375" over 204", a 0.005" difference that
    /// the pin below records deliberately.)
    ///
    /// The constants are READ from <see cref="DynamicHeaderHeightCalculator"/> so the 7/16 exists once in the code
    /// base; nothing here modifies the dynamic system, whose bed keeps its own rule (its two mates are the same
    /// catalog point, so its rise IS the snapped delta).
    /// </summary>
    public static class PushBackBedSlope
    {
        /// <summary>Rise (in) per commercial foot of rack: 7/16".</summary>
        public static double RisePerFoot
            => DynamicHeaderHeightCalculator.SlopeRise / DynamicHeaderHeightCalculator.SlopeRun;

        /// <summary>Rise (in) per inch of horizontal run — the slope as a plain ratio.</summary>
        public static double Ratio
            => RisePerFoot / DynamicHeaderHeightCalculator.CommercialFoot;

        /// <summary>The bed's rise (in) over <paramref name="run"/> inches of horizontal run; 0 for a non-positive run.</summary>
        public static double Rise(double run) => Ratio * Math.Max(0.0, run);
    }
}
