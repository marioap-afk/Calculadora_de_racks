using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Agrupa las clases que ejercitan <c>ImportOutputWriter.Publish</c>.
    ///
    /// Existe por una razon concreta: la costura de fallo del publicador es un hook ESTATICO, y xUnit ejecuta
    /// clases distintas en paralelo. Sin esta coleccion, una clase arma el fallo inyectado y otra lo recibe,
    /// con un rojo intermitente que no habla del defecto que se esta probando. Compartir la coleccion las
    /// serializa.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class StructuralSectionPublishCollection
    {
        public const string Name = "structural-sections-publish";
    }
}
