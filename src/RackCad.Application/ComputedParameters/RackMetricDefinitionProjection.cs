using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Views.Insertion;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>Una definicion de bloque capturada: clave, sobre en crudo y recuento de referencias directas (I-63 D-25).</summary>
    public sealed class RackDefinitionCapture
    {
        public RackDefinitionCapture(string definitionKey, string envelopeJson, int directReferenceCount)
        {
            DefinitionKey = definitionKey;
            EnvelopeJson = envelopeJson;
            DirectReferenceCount = directReferenceCount;
        }

        public string DefinitionKey { get; }

        public string EnvelopeJson { get; }

        public int DirectReferenceCount { get; }
    }

    /// <summary>Clasificacion de una definicion (I-63 D-10a).</summary>
    public enum RackDefinitionClass
    {
        /// <summary>El sobre no deserializa.</summary>
        EnvelopeUnreadable = 1,

        /// <summary>Sobre legible con Id vacio o en blanco.</summary>
        IdAbsent = 2,

        /// <summary>Sobre legible con Id y Kind vacio o en blanco.</summary>
        KindAbsent = 3,

        /// <summary>Kind no vacio fuera de los seis tokens.</summary>
        KindUnknown = 4,

        /// <summary>Uno de los seis tokens.</summary>
        Known = 5,
    }

    /// <summary>La proyeccion de una definicion. Un RackId nunca se inventa: solo existe con <see cref="HasIdentity"/>.</summary>
    public sealed class RackMetricDefinitionProjection
    {
        private static readonly string[] KnownKinds =
        {
            RackEmbedDocument.KindSelective,
            RackEmbedDocument.KindDynamic,
            RackEmbedDocument.KindPushBack,
            RackEmbedDocument.KindCantilever,
            RackEmbedDocument.KindCabecera,
            RackEmbedDocument.KindCama,
        };

        private RackMetricDefinitionProjection(
            RackDefinitionCapture capture,
            RackDefinitionClass classification,
            string rackId,
            string kindToken,
            RackEmbedDocument envelope,
            string probeId)
        {
            Capture = capture;
            Classification = classification;
            RackId = rackId;
            KindToken = kindToken;
            Envelope = envelope;
            ProbeId = probeId;
        }

        public RackDefinitionCapture Capture { get; }

        public string DefinitionKey => Capture.DefinitionKey;

        public int DirectReferenceCount => Capture.DirectReferenceCount;

        public RackDefinitionClass Classification { get; }

        /// <summary>El Id del sobre. Nulo en <see cref="RackDefinitionClass.EnvelopeUnreadable"/> y <see cref="RackDefinitionClass.IdAbsent"/>.</summary>
        public string RackId { get; }

        /// <summary>El token de Kind tal como esta en el sobre. Nulo sin sobre legible o con Kind en blanco.</summary>
        public string KindToken { get; }

        /// <summary>El sobre. Nulo cuando no deserializa.</summary>
        public RackEmbedDocument Envelope { get; }

        /// <summary>Tanteo de Id de un sobre ilegible: solo diagnostico, nunca identifica ni fusiona.</summary>
        public string ProbeId { get; }

        /// <summary>True cuando la definicion es atribuible a un RackId (D-11: sin identidad no hay grupo).</summary>
        public bool HasIdentity
            => Classification == RackDefinitionClass.KindAbsent
               || Classification == RackDefinitionClass.KindUnknown
               || Classification == RackDefinitionClass.Known;

        /// <summary>Funcion pura de D-10a. Comparador de kind: Ordinal contra los seis tokens.</summary>
        public static RackMetricDefinitionProjection Project(RackDefinitionCapture capture)
        {
            if (capture == null)
            {
                throw new ArgumentNullException(nameof(capture));
            }

            var envelope = new RackEmbedStore().Deserialize(capture.EnvelopeJson);
            if (envelope == null)
            {
                return new RackMetricDefinitionProjection(
                    capture, RackDefinitionClass.EnvelopeUnreadable, null, null, null,
                    RackEnvelopeIdProbe.Probe(capture.EnvelopeJson));
            }

            if (string.IsNullOrWhiteSpace(envelope.Id))
            {
                return new RackMetricDefinitionProjection(
                    capture, RackDefinitionClass.IdAbsent, null, envelope.Kind, envelope, null);
            }

            if (string.IsNullOrWhiteSpace(envelope.Kind))
            {
                return new RackMetricDefinitionProjection(
                    capture, RackDefinitionClass.KindAbsent, envelope.Id, null, envelope, null);
            }

            var known = KnownKinds.Any(token => string.Equals(token, envelope.Kind, StringComparison.Ordinal));
            return new RackMetricDefinitionProjection(
                capture,
                known ? RackDefinitionClass.Known : RackDefinitionClass.KindUnknown,
                envelope.Id,
                envelope.Kind,
                envelope,
                null);
        }

        /// <summary>
        /// Orden canonico de D-11: por <c>DefinitionKey</c> con <see cref="StringComparison.Ordinal"/>, ANTES de
        /// clasificar el kind, leer el diseno o pedir la autoridad.
        /// </summary>
        public static IReadOnlyList<RackDefinitionCapture> CanonicalOrder(IEnumerable<RackDefinitionCapture> captures)
        {
            if (captures == null)
            {
                throw new ArgumentNullException(nameof(captures));
            }

            return captures
                .OrderBy(capture => capture?.DefinitionKey ?? string.Empty, StringComparer.Ordinal)
                .ToList();
        }
    }
}
