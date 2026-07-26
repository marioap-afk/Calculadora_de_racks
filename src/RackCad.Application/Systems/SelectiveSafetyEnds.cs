using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Owner-validation round 1 (I-32) — separa los TRES ejes que <see cref="SafetySide"/> mezcla en una sola
    /// enumeración, y que confundirlos costó la validación manual:
    ///
    /// <list type="number">
    /// <item><b>Pertenencia</b> — qué postes llevan la pieza. Vive en
    /// <see cref="SelectiveSafetySelection.PostSides"/>: una entrada con <see cref="SafetySide.None"/> excluye ese
    /// poste, y un poste sin entrada hereda el <see cref="SelectiveSafetySelection.Side"/> general.</item>
    /// <item><b>Orientación</b> — el espejo de la pieza en su propio sitio. Lo consume la PLANTA, que sigue leyendo
    /// el lado literal.</item>
    /// <item><b>Extremo longitudinal</b> — en qué punta del rack se dibuja. Lo consumen el FRONTAL (qué corte) y el
    /// LATERAL (qué extremo de la línea del poste).</item>
    /// </list>
    ///
    /// Push Back solo necesita restringir el TERCERO: entrada y salida comparten el extremo bajo y el alto no lleva
    /// seguridad ordinaria. La versión anterior lo conseguía BORRANDO la matriz por poste, con lo que destruía el
    /// primero — un eje que no dice nada sobre el extremo — y el rack ignoraba los postes que el usuario había
    /// elegido. Restringir el extremo nunca exige olvidar en qué postes va la pieza.
    /// </summary>
    public static class SelectiveSafetyEnds
    {
        /// <summary>
        /// El extremo (o extremos) longitudinales donde se dibuja la pieza de <paramref name="selection"/> en el poste
        /// <paramref name="postIndex"/>.
        ///
        /// <see cref="SafetySide.None"/> significa que ese poste no lleva la pieza — la PERTENENCIA manda y nunca se
        /// reinterpreta. En cualquier otro caso, un sistema marcado
        /// <see cref="SelectiveSafetySelection.LowEndOnly"/> (Push Back) dibuja en el extremo BAJO, sea cual sea el
        /// lado almacenado; sin esa marca (Selectivo, Dinámico) el lado se lee literal, como siempre.
        /// </summary>
        public static SafetySide EndsForPost(SelectiveSafetySelection selection, int postIndex)
        {
            var side = selection?.SideForPost(postIndex) ?? SafetySide.None;
            if (side == SafetySide.None)
            {
                return SafetySide.None;
            }

            return selection.LowEndOnly ? SafetySide.Left : side;
        }
    }
}
