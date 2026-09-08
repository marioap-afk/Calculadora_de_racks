using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Agrupa las clases que sustituyen <c>EditorDiscardPrompt.confirm</c>.
    ///
    /// Existe por una razon concreta y medida (I-45, gate G0B). Ese prompt es un delegado ESTATICO DE PROCESO
    /// con un guardar-y-restaurar NO reentrante —<c>Substitute</c> captura el valor previo y <c>Restore</c> lo
    /// reasigna—, y xUnit ejecuta clases distintas en paralelo. Dos clases lo sustituyen:
    /// <see cref="EditorClosePolicyTests"/>, que lo hace seis veces desde hilos MTA del pool y tres desde el
    /// hilo STA, y <see cref="RichEditorCloseContractTests"/>, que lo hace desde el STA. Un entrelazado entre
    /// ellas puede dejar <c>confirm</c> apuntando al delegado de una prueba ya terminada o, peor, al defecto de
    /// produccion, que es un <c>MessageBox</c> MODAL REAL: a partir de ahi cualquier cierre con trabajo
    /// pendiente colgaria la suite en un dialogo que nadie va a contestar.
    ///
    /// Compartir esta coleccion las serializa, que es el mismo remedio y el mismo idioma que el repositorio ya
    /// usa para sus otros dos estaticos de proceso sustituidos por pruebas
    /// (<c>RackLogTestSupport</c> y <c>StructuralSectionPublishCollection</c>).
    ///
    /// <para>Es deliberadamente la frontera MAS ESTRECHA que cierra la carrera: dos clases de las 121 de esta
    /// suite. NO se deshabilita el paralelismo del ensamblado, y el resto de la suite conserva el suyo intacto.
    /// <c>SelectiveCabeceraHeightPrompt</c> tiene la misma forma pero NO entra aqui: hoy lo sustituye una sola
    /// clase, y xUnit ya serializa los metodos dentro de una clase, asi que no hay carrera concreta que cerrar
    /// y serializar por simetria costaria paralelismo sin comprar seguridad.</para>
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class EditorDiscardPromptCollection
    {
        public const string Name = "editor-discard-prompt";
    }
}
