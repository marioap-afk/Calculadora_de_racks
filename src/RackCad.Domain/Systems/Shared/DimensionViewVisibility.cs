using System;

namespace RackCad.Domain.Systems.Shared
{
    /// <summary>
    /// I-50 (ADR-0035) — en qué TIPOS de vista del rack se dibujan las cotas. Es autoridad del rack por tipo de vista,
    /// no de la instancia dibujada: todas las frontales, todos los cortes laterales y todas las copias de un rack
    /// comparten su tipo. El nivel (<see cref="DimensionDetail"/>) y el estilo de cota siguen siendo globales.
    ///
    /// <para>
    /// Los valores de los bits son contrato de persistencia y NO se renumeran. No existe un miembro «todas»: la
    /// ausencia de la política (<c>null</c>) es el legacy exacto, y una elección explícita de las tres vistas es
    /// <c>Frontal | Lateral | Planta</c>. Un valor presente con bits que este tipo no nombra —incluido el de signo— se
    /// conserva tal cual: la regla solo observa los tres bits conocidos.
    /// </para>
    /// </summary>
    [Flags]
    public enum DimensionViewVisibility
    {
        /// <summary>Ninguna vista dibuja cotas.</summary>
        None = 0,

        /// <summary>Todas las frontales: la de cada fondo, la de salida y la de entrada, y los cortes frontales de Push Back.</summary>
        Frontal = 1,

        /// <summary>El lateral entero y todos sus cortes.</summary>
        Lateral = 2,

        /// <summary>La planta.</summary>
        Planta = 4
    }
}
