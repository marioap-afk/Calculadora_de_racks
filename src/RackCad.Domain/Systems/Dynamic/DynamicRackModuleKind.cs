namespace RackCad.Domain.Systems.Dynamic
{
    /// <summary>
    /// Type of a longitudinal module. Only headers and separators are real modules; in the UI the
    /// three header kinds are all shown as "cabecera". Intermediate posts are NOT a module type:
    /// they are derived markers drawn where two separators meet.
    /// </summary>
    public enum DynamicRackModuleKind
    {
        /// <summary>Cabecera inicial (end frame at the start). Length = pallet depth + 6".</summary>
        HeaderStart,

        /// <summary>Cabecera intermedia. Length = pallet depth.</summary>
        HeaderIntermediate,

        /// <summary>Cabecera final (end frame at the end). Length = pallet depth + 6".</summary>
        HeaderEnd,

        /// <summary>Separador / bahia de tarima. Length = pallet depth.</summary>
        Separator,

        /// <summary>
        /// I-42 — HUECO fisico entre las dos mitades de un Push Back compuesto: la separacion real entre la linea de
        /// postes terminal del lado A y la inicial del lado B. Lleva LONGITUD (la del gap) pero no es cabecera ni
        /// separador, asi que no dibuja pieza, no aporta al BOM y no genera poste derivado. Cuando el usuario pide
        /// separador central, el hueco se materializa como <see cref="Separator"/> —la MISMA pieza que ya existe— en
        /// vez de como este valor; no hay una pieza nueva.
        ///
        /// <para>
        /// Va el ULTIMO del enum a proposito: los valores anteriores conservan su ordinal, de modo que ningun
        /// documento ya escrito cambia de significado. El sistema Dinamico nunca produce este valor.
        /// </para>
        /// </summary>
        Gap
    }
}
