using System.Collections.Generic;
using System;
using RackCad.UI.Shell;
using Xunit;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;
using static RackCad.UI.Tests.PushBackStateNetSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T1a — CT-PBT1A-14: las HUELLAS DE SALIDA de la ventana CLASICA de Push Back sobre el corpus extendido (los diecisiete
    /// diseños de <c>Fixtures/I64</c>, X-01, F-01, F-02 y F-03) por los guiones G-1 a G-6, en ese orden, para que PB-T1c se mida contra
    /// ella: por diseño y guion, si la salida es valida y bloqueada y el SHA-256 del diseño persistido que la ventana escribiria. Cada
    /// guion corre sobre una ventana NUEVA (M-13). F-03 tiene un solo guion, G-F03, que se comprueba a si mismo.
    ///
    /// <para>El metodo arma una lista de lineas y termina en UN <c>Assert.Equal</c>. Toda ventana se crea con
    /// <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/>, su carga va dentro del <c>try</c> y se cierra en el
    /// <c>finally</c>; ninguna se muestra, no se usa el teclado ni el foco real, y el metodo corre dentro de
    /// <see cref="EditorDiscardPrompt.Substitute(System.Func{string,bool})"/> con un contador de descartes que la ultima linea de cada ventana
    /// lleva, leido despues de cerrarla.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackStateNetFingerprintTests
    {
        private static readonly string[] Guiones = { "G-1", "G-2", "G-3", "G-4", "G-5", "G-6" };

        /// <summary>
        /// Un guion sobre una ventana nueva, o <c>null</c> cuando no aplica (G-4 solo corre sobre un compuesto). G-1 carga; G-2 carga y
        /// «Actualizar» (la huella es la de <c>DesignToInsert</c>; la ventana se cierra sola); G-3 carga y «Restaurar»; G-4 carga, retira
        /// el lado B y vuelve a declararlo; G-5 carga, «Restaurar estandar» y «Confirmar»; G-6 carga, escribe 61.5 en
        /// <c>CellPalletHeightBox</c> y aplica a «Todas».
        /// </summary>
        private static string Correr(Caso caso, string guion, Func<int> descartes)
        {
            var w = Ventana();
            try
            {
                caso.Cargar(w);
                var huella = default(string);
                switch (guion)
                {
                    case "G-2":
                        Click(w, "UpdateButton");
                        huella = w.InsertRequested && w.DesignToInsert != null ? Huella(Persist(w.DesignToInsert)) : null;
                        break;
                    case "G-3":
                        Click(w, "RestoreButton");
                        break;
                    case "G-4":
                        if (!w.CompositeState.SideBPresent) return null;
                        Check(w, "SideBPresentCheck").IsChecked = false;
                        Check(w, "SideBPresentCheck").IsChecked = true;
                        break;
                    case "G-5":
                        Click(w, "RestoreAllModulesButton");
                        Click(w, "ConfirmModuleButton");
                        break;
                    case "G-6":
                        SetNumberR(w, "CellPalletHeightBox", 61.5);
                        Click(w, "ApplyAllButton");
                        break;
                }

                var linea = Linea(caso.Nombre, guion, w, huella ?? HuellaDe(w));
                Cerrar(w);
                return linea + " descartes=" + descartes();
            }
            finally { Cerrar(w); }
        }

        [Fact]
        public void CT_PBT1A_14_TheOutputFingerprintsOfTheExtendedCorpusByTheScriptsG1ToG6AreFixed()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    var casos = new List<Caso>(Fixtures());
                    casos.Add(BuildX01());
                    casos.Add(BuildF01());
                    casos.Add(BuildF02());
                    foreach (var caso in casos)
                    {
                        foreach (var guion in Guiones)
                        {
                            var linea = Correr(caso, guion, () => descartes);
                            if (linea != null) log.Add(linea);
                        }
                    }

                    log.Add(LineaF03(() => descartes));
                });
            }

            Assert.Equal(
            new[]
            {
                "pb-01-base G-1 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-01-base G-2 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-01-base G-3 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-01-base G-5 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-01-base G-6 valido=True bloqueado=False sha256=2f9ac42fa15ed683b78d6b4d9f38e56c1755b01114ec593c7f73b53a9f895c76 descartes=0",
                "pb-02-modulos G-1 valido=True bloqueado=False sha256=3532921be31051a320587195d71f19dbc6e9a03f316ac8724580f564df7c00a8 descartes=0",
                "pb-02-modulos G-2 valido=True bloqueado=False sha256=3532921be31051a320587195d71f19dbc6e9a03f316ac8724580f564df7c00a8 descartes=0",
                "pb-02-modulos G-3 valido=True bloqueado=False sha256=3532921be31051a320587195d71f19dbc6e9a03f316ac8724580f564df7c00a8 descartes=0",
                "pb-02-modulos G-5 valido=True bloqueado=False sha256=54754e295d5369d082a340b4c3b85cdb9f10e607baad42f5cd7bdc4a71c35b66 descartes=0",
                "pb-02-modulos G-6 valido=True bloqueado=False sha256=e9f6956d6a4c485bf1f9b2e3bf6d061bccc36e98ced0c05e0e2a667182759361 descartes=0",
                "pb-03-fondos G-1 valido=True bloqueado=False sha256=48ac635a46045ada22e8e88818bd2a94f35924045d1ccf818c689c6567a19291 descartes=0",
                "pb-03-fondos G-2 valido=True bloqueado=False sha256=48ac635a46045ada22e8e88818bd2a94f35924045d1ccf818c689c6567a19291 descartes=0",
                "pb-03-fondos G-3 valido=True bloqueado=False sha256=48ac635a46045ada22e8e88818bd2a94f35924045d1ccf818c689c6567a19291 descartes=0",
                "pb-03-fondos G-5 valido=True bloqueado=False sha256=48ac635a46045ada22e8e88818bd2a94f35924045d1ccf818c689c6567a19291 descartes=0",
                "pb-03-fondos G-6 valido=True bloqueado=False sha256=35a64fccb02805b814959ade2c743a995cb3ca8934e4387098ac5de545019ba1 descartes=0",
                "pb-04-blanco G-1 valido=True bloqueado=False sha256=216ccd4cbf49141c95872ae85c59f920e26e0060246df86c32826eb8c7315ab0 descartes=0",
                "pb-04-blanco G-2 valido=True bloqueado=False sha256=216ccd4cbf49141c95872ae85c59f920e26e0060246df86c32826eb8c7315ab0 descartes=0",
                "pb-04-blanco G-3 valido=True bloqueado=False sha256=216ccd4cbf49141c95872ae85c59f920e26e0060246df86c32826eb8c7315ab0 descartes=0",
                "pb-04-blanco G-5 valido=True bloqueado=False sha256=216ccd4cbf49141c95872ae85c59f920e26e0060246df86c32826eb8c7315ab0 descartes=0",
                "pb-04-blanco G-6 valido=True bloqueado=False sha256=15cdbafe811621421b0a4fcaecc4872e40b11e39879ba00c7b2165e282341148 descartes=0",
                "pb-05-topes G-1 valido=True bloqueado=False sha256=8a8d35b763a15b94175bed9e6d05ae7683cfe83380819c8f4baced436e9d526a descartes=0",
                "pb-05-topes G-2 valido=True bloqueado=False sha256=8a8d35b763a15b94175bed9e6d05ae7683cfe83380819c8f4baced436e9d526a descartes=0",
                "pb-05-topes G-3 valido=True bloqueado=False sha256=8a8d35b763a15b94175bed9e6d05ae7683cfe83380819c8f4baced436e9d526a descartes=0",
                "pb-05-topes G-5 valido=True bloqueado=False sha256=8a8d35b763a15b94175bed9e6d05ae7683cfe83380819c8f4baced436e9d526a descartes=0",
                "pb-05-topes G-6 valido=True bloqueado=False sha256=1a8a0927e1c4ff1e25692c07ea84b6ba70c45780b62c94f7f2b806a6ef2fe7c2 descartes=0",
                "pb-06-seguridad G-1 valido=True bloqueado=False sha256=5311075252094499ac9eb7f8c184b5b040f6b8ffcd34ec630ea05c00bbeff2ed descartes=0",
                "pb-06-seguridad G-2 valido=True bloqueado=False sha256=5311075252094499ac9eb7f8c184b5b040f6b8ffcd34ec630ea05c00bbeff2ed descartes=0",
                "pb-06-seguridad G-3 valido=True bloqueado=False sha256=5311075252094499ac9eb7f8c184b5b040f6b8ffcd34ec630ea05c00bbeff2ed descartes=0",
                "pb-06-seguridad G-5 valido=True bloqueado=False sha256=88548281de6733e8262f2fae78f4565d863c0a3ba3bfbf3078f96a3476d3e7c9 descartes=0",
                "pb-06-seguridad G-6 valido=True bloqueado=False sha256=c2fe51fbb5b21784b8c08edf053a9b1ba959226e94f47f0cfb5530f6cf8fb274 descartes=0",
                "pb-07-combinado G-1 valido=True bloqueado=False sha256=970830b873313d0dbb0ac0a33d6e22be32d5c02dc80111d1d1b804f9be23bd23 descartes=0",
                "pb-07-combinado G-2 valido=True bloqueado=False sha256=970830b873313d0dbb0ac0a33d6e22be32d5c02dc80111d1d1b804f9be23bd23 descartes=0",
                "pb-07-combinado G-3 valido=True bloqueado=False sha256=970830b873313d0dbb0ac0a33d6e22be32d5c02dc80111d1d1b804f9be23bd23 descartes=0",
                "pb-07-combinado G-5 valido=True bloqueado=False sha256=7317c105733e3f33a2e66bbe97895f1efd089bacc8b90891da1e373f3e27a307 descartes=0",
                "pb-07-combinado G-6 valido=True bloqueado=False sha256=d57b05e3176d7d47ded23b8e6be2e7ba3ce41d3cd21f55d92aaa19e28eeb75d9 descartes=0",
                "pb-08-redondeo G-1 valido=True bloqueado=False sha256=161459f1e88f02ad384026b38421977ed598e3f48f6e99f444414c29b69103e1 descartes=0",
                "pb-08-redondeo G-2 valido=True bloqueado=False sha256=161459f1e88f02ad384026b38421977ed598e3f48f6e99f444414c29b69103e1 descartes=0",
                "pb-08-redondeo G-3 valido=True bloqueado=False sha256=161459f1e88f02ad384026b38421977ed598e3f48f6e99f444414c29b69103e1 descartes=0",
                "pb-08-redondeo G-5 valido=True bloqueado=False sha256=161459f1e88f02ad384026b38421977ed598e3f48f6e99f444414c29b69103e1 descartes=0",
                "pb-08-redondeo G-6 valido=True bloqueado=False sha256=47f735d5f6e2811a82c6b03c966bf97103c841eef16912dba2d59d07640c74a7 descartes=0",
                "pb-10-sin-datum G-1 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-10-sin-datum G-2 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-10-sin-datum G-3 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-10-sin-datum G-5 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-10-sin-datum G-6 valido=True bloqueado=False sha256=2f9ac42fa15ed683b78d6b4d9f38e56c1755b01114ec593c7f73b53a9f895c76 descartes=0",
                "pb-11-sin-i41 G-1 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-11-sin-i41 G-2 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-11-sin-i41 G-3 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-11-sin-i41 G-5 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-11-sin-i41 G-6 valido=True bloqueado=False sha256=2f9ac42fa15ed683b78d6b4d9f38e56c1755b01114ec593c7f73b53a9f895c76 descartes=0",
                "pb-12-poste-nulo G-1 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-12-poste-nulo G-2 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-12-poste-nulo G-3 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-12-poste-nulo G-5 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-12-poste-nulo G-6 valido=True bloqueado=False sha256=2f9ac42fa15ed683b78d6b4d9f38e56c1755b01114ec593c7f73b53a9f895c76 descartes=0",
                "pb-13-sideb-no-presente G-1 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-13-sideb-no-presente G-2 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-13-sideb-no-presente G-3 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-13-sideb-no-presente G-5 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-13-sideb-no-presente G-6 valido=True bloqueado=False sha256=2f9ac42fa15ed683b78d6b4d9f38e56c1755b01114ec593c7f73b53a9f895c76 descartes=0",
                "pb-14-peralte-heredado G-1 valido=True bloqueado=False sha256=a84e076b7cce139c39930b8bacff066ad3c47fc7f4f78b9c93f741e94e12c647 descartes=0",
                "pb-14-peralte-heredado G-2 valido=True bloqueado=False sha256=a84e076b7cce139c39930b8bacff066ad3c47fc7f4f78b9c93f741e94e12c647 descartes=0",
                "pb-14-peralte-heredado G-3 valido=True bloqueado=False sha256=a84e076b7cce139c39930b8bacff066ad3c47fc7f4f78b9c93f741e94e12c647 descartes=0",
                "pb-14-peralte-heredado G-5 valido=True bloqueado=False sha256=a84e076b7cce139c39930b8bacff066ad3c47fc7f4f78b9c93f741e94e12c647 descartes=0",
                "pb-14-peralte-heredado G-6 valido=True bloqueado=False sha256=3080158d71cde659e38835d3f035e024a3a0eba4026bf588a8ded1e23c6e5244 descartes=0",
                "pb-15-sin-composite G-1 valido=True bloqueado=False sha256=a518d9b853b45237448a980c5603e564024fcc7fcb17e9b8b5f69b26bd664d62 descartes=0",
                "pb-15-sin-composite G-2 valido=True bloqueado=False sha256=a518d9b853b45237448a980c5603e564024fcc7fcb17e9b8b5f69b26bd664d62 descartes=0",
                "pb-15-sin-composite G-3 valido=True bloqueado=False sha256=a518d9b853b45237448a980c5603e564024fcc7fcb17e9b8b5f69b26bd664d62 descartes=0",
                "pb-15-sin-composite G-5 valido=True bloqueado=False sha256=a518d9b853b45237448a980c5603e564024fcc7fcb17e9b8b5f69b26bd664d62 descartes=0",
                "pb-15-sin-composite G-6 valido=True bloqueado=False sha256=ee6e9b65ca9dd9a0321c9d0eabbe4706b08d1dc7ddfd6c1b0ee80cc52fafa0af descartes=0",
                "pb-20-ab G-1 valido=True bloqueado=False sha256=27db1c142c0b8d252156a7799176642f5727e9d408e48c71ab16da0fe13f4865 descartes=0",
                "pb-20-ab G-2 valido=True bloqueado=False sha256=27db1c142c0b8d252156a7799176642f5727e9d408e48c71ab16da0fe13f4865 descartes=0",
                "pb-20-ab G-3 valido=True bloqueado=False sha256=27db1c142c0b8d252156a7799176642f5727e9d408e48c71ab16da0fe13f4865 descartes=0",
                "pb-20-ab G-4 valido=True bloqueado=False sha256=c32b6ecb24165c7ec04c3d7a5a877b075a327a572559919865afe952a45bf481 descartes=0",
                "pb-20-ab G-5 valido=True bloqueado=False sha256=b83104719fbcc4cbc544c7d582c5167c9a8d6e3ab140c30c4ecf188d7a1b04fa descartes=0",
                "pb-20-ab G-6 valido=True bloqueado=False sha256=a3e223a387ae491f65d8fcfb94d0bddfdd29c3c3f07e279d747c0e9822ad53a8 descartes=0",
                "pb-21-cola G-1 valido=True bloqueado=False sha256=a448b48ad62d1718bd7a1eda70f3ffa79cbc4dddf5d19eb0d77cdb2b50889c2a descartes=0",
                "pb-21-cola G-2 valido=True bloqueado=False sha256=a448b48ad62d1718bd7a1eda70f3ffa79cbc4dddf5d19eb0d77cdb2b50889c2a descartes=0",
                "pb-21-cola G-3 valido=True bloqueado=False sha256=a448b48ad62d1718bd7a1eda70f3ffa79cbc4dddf5d19eb0d77cdb2b50889c2a descartes=0",
                "pb-21-cola G-5 valido=True bloqueado=False sha256=a448b48ad62d1718bd7a1eda70f3ffa79cbc4dddf5d19eb0d77cdb2b50889c2a descartes=0",
                "pb-21-cola G-6 valido=True bloqueado=False sha256=66c1d5e220a2dfa2411d941f485309d57b15e8aa8eaf45b2ed6f29783e0ca75d descartes=0",
                "pb-31-v2-modulos G-1 valido=True bloqueado=False sha256=3d2ba46c1757f2dc649f0830f8898342b4602d203c6313335edd704c1ca834fd descartes=0",
                "pb-31-v2-modulos G-2 valido=True bloqueado=False sha256=3d2ba46c1757f2dc649f0830f8898342b4602d203c6313335edd704c1ca834fd descartes=0",
                "pb-31-v2-modulos G-3 valido=True bloqueado=False sha256=3d2ba46c1757f2dc649f0830f8898342b4602d203c6313335edd704c1ca834fd descartes=0",
                "pb-31-v2-modulos G-5 valido=True bloqueado=False sha256=d836e08d66b9cbec76ba9e5bdb7daebeaf73d3a5fa9ba96c733b830f1c8d2c9a descartes=0",
                "pb-31-v2-modulos G-6 valido=True bloqueado=False sha256=715d9e2fdc4bd531d2d688bb625838c9426b77c3c8dd7d87777f89bd1994021e descartes=0",
                "X-01 G-1 valido=True bloqueado=False sha256=8b7918bdf0cc10bde82d3265a3bac30c78163a11d97de2a24010c58cdce44b15 descartes=0",
                "X-01 G-2 valido=True bloqueado=False sha256=8b7918bdf0cc10bde82d3265a3bac30c78163a11d97de2a24010c58cdce44b15 descartes=0",
                "X-01 G-3 valido=True bloqueado=False sha256=8b7918bdf0cc10bde82d3265a3bac30c78163a11d97de2a24010c58cdce44b15 descartes=0",
                "X-01 G-4 valido=True bloqueado=False sha256=728e1340d1c54cef36c16ec12106f5459eea0f5f951f65cce4b59e09e099c4a6 descartes=0",
                "X-01 G-5 valido=True bloqueado=False sha256=4d993fc4342612b3f30551c3e9ca93e313b073efa63d8ffceaf0d6f22576ce02 descartes=0",
                "X-01 G-6 valido=True bloqueado=False sha256=628cb6993fa11a860c6a66fe0d1b377a675ff8167f6fd8fb6fa76522d95e0da1 descartes=0",
                "F-01 G-1 valido=True bloqueado=False sha256=d04d64de10fbd240114e6b06afeb1533293c8d92984c560e5d767b0d92f355c3 descartes=0",
                "F-01 G-2 valido=True bloqueado=False sha256=d04d64de10fbd240114e6b06afeb1533293c8d92984c560e5d767b0d92f355c3 descartes=0",
                "F-01 G-3 valido=True bloqueado=False sha256=d04d64de10fbd240114e6b06afeb1533293c8d92984c560e5d767b0d92f355c3 descartes=0",
                "F-01 G-4 valido=True bloqueado=False sha256=33ef72bb0b696c321b77375564c8ca3f4ff2fa7093e07c7ef7eb26eaae5ede32 descartes=0",
                "F-01 G-5 valido=True bloqueado=False sha256=b83104719fbcc4cbc544c7d582c5167c9a8d6e3ab140c30c4ecf188d7a1b04fa descartes=0",
                "F-01 G-6 valido=True bloqueado=False sha256=4a3c6c2e7804842719e18f6bcce3f980fac018ba27e63831e88355baa511da28 descartes=0",
                "F-02 G-1 valido=True bloqueado=False sha256=27db1c142c0b8d252156a7799176642f5727e9d408e48c71ab16da0fe13f4865 descartes=0",
                "F-02 G-2 valido=True bloqueado=False sha256=27db1c142c0b8d252156a7799176642f5727e9d408e48c71ab16da0fe13f4865 descartes=0",
                "F-02 G-3 valido=True bloqueado=False sha256=27db1c142c0b8d252156a7799176642f5727e9d408e48c71ab16da0fe13f4865 descartes=0",
                "F-02 G-4 valido=True bloqueado=False sha256=c32b6ecb24165c7ec04c3d7a5a877b075a327a572559919865afe952a45bf481 descartes=0",
                "F-02 G-5 valido=True bloqueado=False sha256=b83104719fbcc4cbc544c7d582c5167c9a8d6e3ab140c30c4ecf188d7a1b04fa descartes=0",
                "F-02 G-6 valido=True bloqueado=False sha256=a3e223a387ae491f65d8fcfb94d0bddfdd29c3c3f07e279d747c0e9822ad53a8 descartes=0",
                "F-03 G-F03 valido=True bloqueado=False sha256=a49f9bd3a844eb5cdadeaddb47f856f75ccd32c64bdd6068dc4dce45c7c996b2 PalletTolerance=3.00044 pasadas-restaurar=2 frontal=A descartes=0",
            },
            log);
        }
    }
}
