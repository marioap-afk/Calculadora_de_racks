using System;
using RackCad.Application.Persistence;

namespace RackCad.Application.CustomProperties
{
    /// <summary>
    /// One scanned rack block definition, flattened for Application (I-54 D-22.2 / ADR-0039 §8): its handle as text, its
    /// block name, whether it is placed, whether it depends on an external reference, and the deserialized envelope when
    /// the RackCad payload could be interpreted. No AutoCAD object crosses into Application: the edge builds these,
    /// Application decides.
    /// </summary>
    public sealed class RackCustomPropertiesDefinition
    {
        public RackCustomPropertiesDefinition(string handle, string blockName, bool isPlaced, bool isDependent, RackEmbedDocument envelope)
        {
            if (string.IsNullOrWhiteSpace(handle))
            {
                throw new ArgumentException("Una definición escaneada necesita su handle.", nameof(handle));
            }

            Handle = handle;
            BlockName = blockName;
            IsPlaced = isPlaced;
            IsDependent = isDependent;
            Envelope = envelope;
        }

        /// <summary>The definition's handle, as text: what a plan names when it says where to write.</summary>
        public string Handle { get; }

        public string BlockName { get; }

        /// <summary>True when the definition has at least one direct reference in the drawing.</summary>
        public bool IsPlaced { get; }

        /// <summary>True when the definition depends on an external reference: it belongs to another drawing (D-09.4).</summary>
        public bool IsDependent { get; }

        /// <summary>The envelope, or null when the RackCad payload could not be interpreted.</summary>
        public RackEmbedDocument Envelope { get; }

        public bool IsInterpretable => Envelope != null;
    }

    /// <summary>
    /// The definition the user picked (D-09.4 and steps 1 and 2 of D-09.8). A definition that comes from an external
    /// reference never reaches the scan, so the pick carries that fact itself.
    /// </summary>
    public sealed class RackCustomPropertiesSelection
    {
        public RackCustomPropertiesSelection(string definitionHandle, bool isFromExternalReference)
        {
            if (string.IsNullOrWhiteSpace(definitionHandle))
            {
                throw new ArgumentException("La selección necesita el handle de la definición elegida.", nameof(definitionHandle));
            }

            DefinitionHandle = definitionHandle;
            IsFromExternalReference = isFromExternalReference;
        }

        public string DefinitionHandle { get; }

        public bool IsFromExternalReference { get; }
    }
}
