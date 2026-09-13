using System;
using RackCad.Application.Persistence;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.Systems.Shared
{
    /// <summary>Why a header configuration could not be captured (I-53, contract §4). The set is CLOSED.</summary>
    public enum HeaderCaptureFailure
    {
        /// <summary>There was no configuration to capture.</summary>
        NullSource = 1,

        /// <summary>The configuration is not a usable header (<see cref="RackDesignValidation.IsUsableHeader"/>).</summary>
        UnusableHeader = 2,
    }

    /// <summary>
    /// The result of <see cref="HeaderConfigurationSnapshot.TryCapture"/>: <see cref="Captured"/> or <see cref="Failed"/>,
    /// and nothing else — the base constructor is private, so no third form can exist.
    /// </summary>
    public abstract class HeaderConfigurationCapture
    {
        private HeaderConfigurationCapture()
        {
        }

        /// <summary>The source was a usable header and its private canonical copy now lives in <see cref="Snapshot"/>.</summary>
        public sealed class Captured : HeaderConfigurationCapture
        {
            internal Captured(HeaderConfigurationSnapshot snapshot)
            {
                Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            }

            public HeaderConfigurationSnapshot Snapshot { get; }
        }

        /// <summary>Nothing was copied: the source was refused BEFORE any copy was attempted.</summary>
        public sealed class Failed : HeaderConfigurationCapture
        {
            internal Failed(HeaderCaptureFailure reason)
            {
                if (!Enum.IsDefined(reason))
                {
                    throw new ArgumentOutOfRangeException(nameof(reason), reason, "Motivo de captura fuera del conjunto cerrado.");
                }

                Reason = reason;
            }

            public HeaderCaptureFailure Reason { get; }
        }
    }

    /// <summary>
    /// I-53 (ID6 REUSE + ID7 BATCH DISTRIBUTION) — the private, transient copy of the header configuration that a reuse
    /// or a batch distribution hands out (contract §4, AM-1; ADR-0037).
    /// <para>
    /// A configuration handed to several destinations must reach each of them as an INDEPENDENT instance: the discovery
    /// of I-53 found configuration instances shared by reference, and at least one real defect caused by it. So the
    /// snapshot is taken inside PREPARE from the source's CURRENT value, and every destination receives its own
    /// <see cref="Materialize"/> — never the source, never the private copy, never another destination's copy.
    /// </para>
    /// <para>
    /// Both copies are the single canonical clone, <see cref="RackFrameProjectStore.DeepCopy"/> (I-17): the persisted
    /// model by round trip, the derived <see cref="RackFrameConfiguration.Members"/> rebuilt and the runtime-only
    /// <see cref="RackFrameConfiguration.Exceptions"/> re-attached, exactly as that method does. There is no other copy
    /// path, serialization or DTO here.
    /// </para>
    /// <para>
    /// It is not a framework and carries no provenance: it knows no system, address or destination, it is never persisted
    /// and it is not kept between gestures. Nothing of the private copy is exposed — no configuration, collection,
    /// authored object or setter; <see cref="Materialize"/> is the only way out.
    /// </para>
    /// </summary>
    public sealed class HeaderConfigurationSnapshot
    {
        private readonly RackFrameConfiguration privateCopy;

        private HeaderConfigurationSnapshot(RackFrameConfiguration privateCopy)
        {
            this.privateCopy = privateCopy;
        }

        /// <summary>
        /// Validates the source and, only when it is a usable header, takes the private canonical copy.
        /// <para>
        /// A null source fails with <see cref="HeaderCaptureFailure.NullSource"/> and an unusable one with
        /// <see cref="HeaderCaptureFailure.UnusableHeader"/>: neither throws, copies or touches the source. There is
        /// deliberately no catch-all around the copy — once the source passed the explicit validation, an exception is a
        /// defect and propagates (I-03).
        /// </para>
        /// </summary>
        public static HeaderConfigurationCapture TryCapture(RackFrameConfiguration source)
        {
            if (source == null)
            {
                return new HeaderConfigurationCapture.Failed(HeaderCaptureFailure.NullSource);
            }

            // Validate BEFORE copying: an invalid source never reaches DeepCopy.
            if (!RackDesignValidation.IsUsableHeader(source))
            {
                return new HeaderConfigurationCapture.Failed(HeaderCaptureFailure.UnusableHeader);
            }

            return new HeaderConfigurationCapture.Captured(
                new HeaderConfigurationSnapshot(new RackFrameProjectStore().DeepCopy(source)));
        }

        /// <summary>
        /// A NEW configuration on every call: a canonical deep copy of the private copy, sharing no object or collection
        /// with it, with the source or with any other materialized copy.
        /// </summary>
        public RackFrameConfiguration Materialize()
        {
            return new RackFrameProjectStore().DeepCopy(privateCopy);
        }
    }
}
