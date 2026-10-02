using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>
    /// El contrato de entrada sintetico de la poblacion (I-63 D-25, fase Phi0): la lista de definiciones capturadas
    /// <c>(DefinitionKey, envelopeJson, DirectReferenceCount)</c>, la lectura del registro y como termino la carga
    /// del catalogo. En V1 lo construyen las pruebas; ningun componente lo produce desde AutoCAD.
    /// </summary>
    public sealed class RackMetricPopulationInput
    {
        public RackMetricPopulationInput(
            IReadOnlyList<RackDefinitionCapture> definitions,
            ProjectVariablesReadResult registry,
            RackCatalogInput catalog)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            if (definitions.Any(definition => definition == null))
            {
                throw new ArgumentException("Una captura de definicion no puede ser nula.", nameof(definitions));
            }

            Definitions = definitions;
            Registry = registry;
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        /// <summary>Las definiciones capturadas, en el orden en que llegaron (la poblacion las ordena de forma canonica).</summary>
        public IReadOnlyList<RackDefinitionCapture> Definitions { get; }

        /// <summary>La lectura del registro de variables. La pertenencia nunca la consulta (I-63 D-10, P-03).</summary>
        public ProjectVariablesReadResult Registry { get; }

        /// <summary>Como termino la carga del catalogo: <c>Loaded</c> o <c>LoadFailed</c> (D-27).</summary>
        public RackCatalogInput Catalog { get; }
    }
}
