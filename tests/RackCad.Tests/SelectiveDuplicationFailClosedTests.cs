using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G14 — duplicar es indivisible, o no se duplica.
    ///
    /// <para>
    /// El re-estampado podía fallar y devolver el JSON ORIGINAL: la copia salía con un RackId nuevo en el
    /// sobre y la identidad vieja dentro. Dos racks con identidad semántica mezclada, y el defecto solo se ve
    /// la primera vez que alguien abre la copia y guarda —momento en el que ya no hay forma de saber cuál era
    /// cuál—. Con vínculos de proyecto encima, esa copia además arrastra un binding cuyo dueño ya no es quien
    /// dice ser.
    /// </para>
    /// <para>
    /// Así que la transformación que puede fallar se decide ANTES de materializar nada: o sale entera —sobre
    /// nuevo, identidad interior re-estampada, vínculo y literal congelado y versión y campos desconocidos
    /// intactos— o no sale ninguna copia y el usuario lo ve.
    /// </para>
    /// <para>
    /// Y la asimetría que hay que decir en voz alta: <b>la identidad del RACK cambia, la de la VARIABLE no</b>.
    /// Una copia es otro rack; la variable de proyecto que gobierna su holgura sigue siendo la misma variable
    /// del mismo dibujo.
    /// </para>
    /// </summary>
    public class SelectiveDuplicationFailClosedTests
    {
        private const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string CopyId = "9d5e2a10-7b44-4c81-a0f3-51c6e7b29a88";
        private const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        private const string Token = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        private static SelectivePalletDesign Diseno(double clearance)
        {
            var design = new SelectivePalletDesign { VerticalClearance = clearance, PalletDepth = 48.0 };
            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = "BEAM_A",
                BeamPeralte = 4.5,
            });
            design.Bays.Add(bay);
            return design;
        }

        private static string Json(bool bound, string extensionKey = null)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(6.0), RackA, "Rack original");

            if (bound)
            {
                doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
                {
                    [Token] = SelectivePropertyValueDocument.ToProjectVariable(VarId),
                };
                doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            if (extensionKey != null)
            {
                doc.ExtensionData = new Dictionary<string, JsonElement>
                {
                    [extensionKey] = JsonDocument.Parse("7").RootElement,
                };
            }

            return new SelectivePalletDesignStore().Serialize(doc);
        }

        private static SelectivePalletDesignDocument Copia(bool bound, string extensionKey = null)
        {
            var result = SelectiveAuthoredRestamp.Restamp(Json(bound, extensionKey), CopyId, "Rack copia");

            Assert.True(result.IsSuccess);
            return new SelectivePalletDesignStore().Deserialize(result.DesignJson);
        }

        // ================================================================ 1-3: la identidad que SÍ cambia

        [Fact]
        public void UNA_COPIA_SIN_VINCULO_ESTRENA_IDENTIDAD()
        {
            var copia = Copia(bound: false);

            Assert.Equal(CopyId, copia.Id);
            Assert.Equal("Rack copia", copia.Name);
        }

        [Fact]
        public void UNA_COPIA_VINCULADA_ESTRENA_IDENTIDAD_DE_RACK()
        {
            var copia = Copia(bound: true);

            Assert.Equal(CopyId, copia.Id);
            Assert.NotEqual(RackA, copia.Id);
            Assert.Equal("Rack copia", copia.Name);
        }

        /// <summary>La identidad del RACK cambia; la de la VARIABLE no. Son dos cosas distintas.</summary>
        [Fact]
        public void UNA_COPIA_VINCULADA_CONSERVA_LA_MISMA_VARIABLE()
        {
            var copia = Copia(bound: true);

            Assert.True(copia.TryGetBinding(ProjectPropertyIds.SelectiveVerticalClearance, out var id));
            Assert.Equal(VariableId.Parse(VarId), id);
        }

        // ================================================================ 4-7: lo que sobrevive intacto

        [Fact]
        public void LA_COPIA_CONSERVA_EL_LITERAL_CONGELADO()
        {
            Assert.Equal(6.0, Copia(bound: true).VerticalClearance);
        }

        [Fact]
        public void LA_COPIA_CONSERVA_LA_VERSION_PROMOCIONADA()
        {
            Assert.Equal(SelectivePalletDesignDocument.PromotedSchemaVersion, Copia(bound: true).SchemaVersion);
        }

        [Fact]
        public void LA_COPIA_CONSERVA_LOS_CAMPOS_DESCONOCIDOS()
        {
            var copia = Copia(bound: true, extensionKey: "DeUnBuildPosterior");

            Assert.NotNull(copia.ExtensionData);
            Assert.True(copia.ExtensionData.ContainsKey("DeUnBuildPosterior"));
        }

        [Fact]
        public void LA_COPIA_CONSERVA_EL_RESTO_DEL_DISENO()
        {
            var copia = Copia(bound: false);

            Assert.Single(copia.Bays);
            Assert.Equal(48.0, copia.PalletDepth);
        }

        // ================================================================ 8, 9: fallar sin copiar

        /// <summary>
        /// Lo que NUNCA puede volver: un diseño ilegible que devuelve el JSON original. Esa copia llevaría el
        /// RackId nuevo por fuera y el viejo por dentro.
        /// </summary>
        [Fact]
        public void UN_DISENO_ILEGIBLE_NO_DEVUELVE_EL_ORIGINAL()
        {
            var result = SelectiveAuthoredRestamp.Restamp("{ esto no es json", CopyId, "Rack copia");

            Assert.False(result.IsSuccess);
            Assert.Null(result.DesignJson);
            Assert.False(string.IsNullOrWhiteSpace(result.Error));
        }

        [Fact]
        public void UN_DISENO_AUSENTE_TAMPOCO_PRODUCE_COPIA()
        {
            Assert.False(SelectiveAuthoredRestamp.Restamp(null, CopyId, "Rack copia").IsSuccess);
            Assert.False(SelectiveAuthoredRestamp.Restamp("   ", CopyId, "Rack copia").IsSuccess);
        }

        /// <summary>Un MAJOR del futuro es ilegible para este build, y no se copia a ciegas.</summary>
        [Fact]
        public void UN_MAJOR_DEL_FUTURO_NO_PRODUCE_COPIA()
        {
            // Escrito a mano: el store lee sin distinguir mayusculas, y la guarda de version corre ANTES que
            // cualquier otra comprobacion, asi que esto mide exactamente el major y nada mas.
            var futuro = "{\"schemaVersion\":\"9.0\",\"id\":\"" + RackA + "\",\"name\":\"Rack original\",\"bays\":[]}";

            Assert.False(SelectiveAuthoredRestamp.Restamp(futuro, CopyId, "Rack copia").IsSuccess);
        }

        [Fact]
        public void EL_RESULTADO_TIPADO_DISTINGUE_LOS_DOS_ESTADOS()
        {
            Assert.True(RestampResult.Success("{}").IsSuccess);
            Assert.False(RestampResult.Failure("no").IsSuccess);
            Assert.Null(RestampResult.Failure("no").DesignJson);
        }

        // ================================================================ 14, 15: caracterizacion para I-51 (ID15)

        // I-51 duplica VARIAS vistas de un mismo rack en un solo gesto, y todas se re-estampan con la misma
        // identidad. Estas dos pruebas no cambian produccion: fijan lo que esa copia multivista necesita del
        // re-estampado que ya existe. T14, que sobreviven los DOS vinculos reales de I-48; T15, que dos vistas
        // iguales re-estampadas con el mismo (id, nombre) siguen siendo UNA autoridad, y que un nombre por
        // vista fabrica una copia que nace divergente (contrato de I-51, INV-08 e INV-09).

        private const string VarToleranceId = "5b0c7e21-9a3d-4f6e-b812-3c4d5e6f7a80";

        private static string JsonConDosVinculos()
        {
            var design = Diseno(6.0);
            design.PalletTolerance = 3.0;

            var doc = SelectivePalletDesignDocument.From(design, RackA, "Rack original");
            doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [ProjectPropertyIds.SelectiveVerticalClearanceToken] = SelectivePropertyValueDocument.ToProjectVariable(VarId),
                [ProjectPropertyIds.SelectivePalletToleranceToken] = SelectivePropertyValueDocument.ToProjectVariable(VarToleranceId),
            };
            doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;

            return new SelectivePalletDesignStore().Serialize(doc);
        }

        [Fact]
        public void T14_UNA_COPIA_CON_DOS_VINCULOS_CONSERVA_AMBAS_VARIABLES_Y_AMBOS_LITERALES()
        {
            var store = new SelectivePalletDesignStore();
            var original = store.Deserialize(JsonConDosVinculos());

            var result = SelectiveAuthoredRestamp.Restamp(JsonConDosVinculos(), CopyId, "Rack copia");

            Assert.True(result.IsSuccess);
            var copia = store.Deserialize(result.DesignJson);

            // Lo unico que cambia es la identidad del rack.
            Assert.Equal(CopyId, copia.Id);
            Assert.Equal("Rack copia", copia.Name);

            // Cada vinculo conserva SU variable: ninguno se pierde y ninguno se cruza con el otro.
            Assert.Equal(2, copia.PropertyValues.Count);
            Assert.True(copia.TryGetBinding(ProjectPropertyIds.SelectiveVerticalClearance, out var clearance));
            Assert.Equal(VariableId.Parse(VarId), clearance);
            Assert.True(copia.TryGetBinding(ProjectPropertyIds.SelectivePalletTolerance, out var tolerance));
            Assert.Equal(VariableId.Parse(VarToleranceId), tolerance);

            // Los dos literales congelados sobreviven.
            Assert.Equal(6.0, copia.VerticalClearance);
            Assert.Equal(3.0, copia.PalletTolerance);

            // Y el resto del authored es identico: con la identidad igualada, el comparador TOTAL de I-47 no
            // encuentra ninguna otra diferencia (version, vinculos, literales, estructura y desconocidos).
            copia.Id = original.Id;
            copia.Name = original.Name;
            Assert.True(SelectiveAuthoredAuthority.IsSameAuthority(new[] { original, copia }));
        }

        [Fact]
        public void T15_VISTAS_IGUALES_REESTAMPADAS_CON_LA_MISMA_IDENTIDAD_SIGUEN_SIENDO_UNA_AUTORIDAD()
        {
            var store = new SelectivePalletDesignStore();

            SelectivePalletDesignDocument Copiar(string json, string name)
            {
                var result = SelectiveAuthoredRestamp.Restamp(json, CopyId, name);
                Assert.True(result.IsSuccess);
                return store.Deserialize(result.DesignJson);
            }

            // Dos vistas hermanas del mismo rack llevan el mismo documento authored.
            var frontal = JsonConDosVinculos();
            var lateral = JsonConDosVinculos();

            Assert.True(SelectiveAuthoredAuthority.IsSameAuthority(
                new[] { Copiar(frontal, "Rack copia"), Copiar(lateral, "Rack copia") }));

            // Un nombre distinto por vista basta para que la copia nazca divergente.
            Assert.False(SelectiveAuthoredAuthority.IsSameAuthority(
                new[] { Copiar(frontal, "Rack copia"), Copiar(lateral, "Rack copia 2") }));
        }

        // ================================================================ guardas de fuente

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.NotNull(dir);
            return dir;
        }

        private static string Plugin(params string[] relative)
            => File.ReadAllText(Path.Combine(
                new[] { RepoRoot().FullName, "src", "RackCad.Plugin" }.Concat(relative).ToArray()));

        private static int At(string source, string token)
        {
            var at = source.IndexOf(token, StringComparison.Ordinal);
            Assert.True(at >= 0, "no se encontró: " + token);
            return at;
        }

        [Fact]
        public void GUARDA_EL_CONTRATO_DE_RESTAMP_ES_TIPADO()
        {
            Assert.Contains("RestampResult RestampDesign(", Plugin("KindHandlers", "IRackKindHandler.cs"));
        }

        [Fact]
        public void GUARDA_EL_HANDLER_SELECTIVO_DELEGA_EN_LA_CAPA_PURA()
        {
            Assert.Contains("SelectiveAuthoredRestamp.Restamp(", Plugin("KindHandlers", "SelectiveKindHandler.cs"));
        }

        /// <summary>
        /// La regla entera en una guarda: el helper compartido ya no tiene un <c>catch</c> que devuelva el
        /// original. Un fallo de re-estampado no se degrada a "copia con la identidad vieja".
        /// </summary>
        [Fact]
        public void GUARDA_EL_RESTAMP_COMPARTIDO_NO_TIENE_MEJOR_ESFUERZO()
        {
            var source = Plugin("RackEnvelopeRestamp.cs");

            // La palabra suelta aparece en la documentacion que explica lo que se retiro; lo que no puede
            // volver es un manejador real.
            Assert.DoesNotContain("catch (", source);
            Assert.DoesNotContain("return designJson;", source);
            Assert.Contains("RestampResult RestampEnvelope(", source);
        }

        /// <summary>
        /// I-51 G-R1. G4 reapunto aqui <c>GUARDA_RACKDUPLICAR_DECIDE_ANTES_DE_CLONAR</c>, que comparaba la PRIMERA aparicion
        /// de dos textos y seguia verde con el re-estampado dentro de la transaccion; G5 la reapunta al LOTE por destino. La
        /// propiedad: cada destino re-estampa TODAS sus definiciones y mira TODOS sus <c>IsSuccess</c> antes de la UNICA
        /// transaccion que clona. Se comprueba sin fijar una forma: el miembro que clona no re-estampa, ni por si mismo ni
        /// por lo que llama; ninguna transaccion del comando re-estampa; una sola transaccion alcanza los clones y los
        /// contiene todos; el clon recibe el <c>DesignJson</c> preparado; el payload de origen solo entra al re-estampado;
        /// quien re-estampa mira <c>IsSuccess</c> y abandona antes de usar el resultado; y en cada destino esa preparacion
        /// termina antes de llamar a la mutacion.
        /// </summary>
        [Fact]
        public void GUARDA_G_R1_RACKDUPLICAR_DECIDE_ANTES_DE_CLONAR()
        {
            var code = PluginSourceCode.Mask(Plugin("RackDuplicarCommands.cs"));
            var members = PluginSourceCode.Members(code);

            // MUTATE: un solo miembro clona, y no re-estampa ni directa ni indirectamente.
            var mutation = Assert.Single(members, m => PluginSourceCode.Calls(m.Body, "CloneDefinition").Count > 0);
            Assert.False(PluginSourceCode.Reaches(members, mutation.Body, "RestampEnvelope"),
                mutation.Name + " re-estampa: la decision que puede fallar quedo dentro de la mutacion.");

            // Las transacciones se abren solo con InDocumentTransaction.Run: ninguna re-estampa, y una sola alcanza los clones.
            Assert.DoesNotMatch(@"\b(?:StartTransaction|StartOpenCloseTransaction|LockDocument)\s*\(", code);
            var transactions = PluginSourceCode.Calls(code, "Run");

            foreach (var transaction in transactions)
            {
                Assert.False(PluginSourceCode.Reaches(members, transaction.ArgumentText, "RestampEnvelope"),
                    "una transaccion se abre antes de terminar los re-estampados.");
            }

            var clones = PluginSourceCode.Calls(code, "CloneDefinition");
            var cloning = Assert.Single(transactions, t => PluginSourceCode.Reaches(members, t.ArgumentText, "CloneDefinition"));
            Assert.Equal(clones.Count, PluginSourceCode.Calls(cloning.ArgumentText, "CloneDefinition").Count);

            // El clon recibe lo PREPARADO, y el payload de origen solo entra al re-estampado.
            var payloadAt = ClonerPayloadIndex();

            foreach (var clone in clones)
            {
                Assert.True(Regex.IsMatch(clone.Arguments[payloadAt], @"^\w+\s*\.\s*DesignJson$"),
                    "el clon no recibe el DesignJson preparado: " + clone.Arguments[payloadAt]);
            }

            var restampedSources = PluginSourceCode.Calls(code, "RestampEnvelope")
                .Count(call => Regex.IsMatch(call.Arguments[0], @"^\w+\s*\.\s*(?:Raw)?Payload$"));
            Assert.True(Regex.Matches(code, @"\.\s*(?:Raw)?Payload\b").Count == restampedSources,
                "un payload de origen se usa fuera del re-estampado.");

            // PREPARE: el unico miembro que re-estampa mira IsSuccess y abandona antes de usar el resultado.
            var prepare = Assert.Single(members, m => PluginSourceCode.Calls(m.Body, "RestampEnvelope").Count > 0);
            var restamp = Regex.Match(prepare.Body, @"\b(?<result>\w+)\s*=\s*(?:\w+\s*\.\s*)*RestampEnvelope\s*\(");
            Assert.True(restamp.Success, prepare.Name + " no guarda el resultado del re-estampado.");

            var result = restamp.Groups["result"].Value;
            var check = Regex.Match(prepare.Body, @"\b" + result + @"\s*\.\s*IsSuccess\b");
            var use = Regex.Match(prepare.Body, @"\b" + result + @"\s*\.\s*DesignJson\b");
            Assert.True(check.Success && use.Success && restamp.Index < check.Index && check.Index < use.Index,
                "el IsSuccess de '" + result + "' no se mira antes de usar el resultado.");
            Assert.Matches(@"\b(?:throw|return|break|continue)\b", prepare.Body.Substring(check.Index, use.Index - check.Index));

            // En cada destino, la preparacion completa termina antes de llamar a la mutacion.
            var loop = Assert.Single(members, m => m != mutation && PluginSourceCode.Calls(m.Body, mutation.Name).Count > 0);
            var place = Assert.Single(PluginSourceCode.Calls(loop.Body, mutation.Name));
            var assigner = Regex.Match(loop.Body, @"\bCreateDestinationAssigner\s*\(");
            Assert.True(assigner.Success && PluginSourceCode.Calls(loop.Body, prepare.Name)
                    .Any(call => assigner.Index < call.Start && call.End <= place.Start),
                "el destino no se prepara entero antes de llamar a " + mutation.Name + ".");
        }

        /// <summary>Posicion del parametro <c>payload</c> en la firma vigente de <c>RackCloner.CloneDefinition</c>.</summary>
        private static int ClonerPayloadIndex()
        {
            var cloner = Assert.Single(
                PluginSourceCode.Members(PluginSourceCode.Mask(Plugin("RackCloner.cs"))), m => m.Name == "CloneDefinition");
            var index = cloner.Parameters.Select(p => p.Name).ToList().IndexOf("payload");

            Assert.True(index >= 0, "RackCloner.CloneDefinition ya no tiene un parametro 'payload'.");
            return index;
        }

        /// <summary>
        /// I-51 G4, G-R2 (reapunta la mitad de RACKDUPLICAR de <c>GUARDA_NINGUN_CAMINO_DE_COPIA_CAE_AL_PAYLOAD_DE_ORIGEN</c>).
        /// Ningun <c>CloneDefinition</c> del comando recibe un payload de ORIGEN, escriba como se escriba la llamada: ni el
        /// <c>Payload</c> de la instantanea del comando, ni el <c>RawPayload</c> de la instantanea del planificador (G3), ni
        /// una lectura directa del dibujo.
        /// </summary>
        [Fact]
        public void GUARDA_G_R2_RACKDUPLICAR_NUNCA_CLONA_EL_PAYLOAD_DE_ORIGEN()
        {
            var clones = PluginSourceCode.Calls(PluginSourceCode.Mask(Plugin("RackDuplicarCommands.cs")), "CloneDefinition");

            Assert.NotEmpty(clones);
            foreach (var clone in clones)
            {
                Assert.DoesNotMatch(@"\.\s*(?:Raw)?Payload\b|\bRackBlockData\s*\.\s*Read\s*\(", string.Join(", ", clone.Arguments));
            }
        }

        /// <summary>
        /// I-51 G-R4 (G4, PD-1; ampliada en G5). RACKDUPLICAR duplica lo SELECCIONADO: no barre el dibujo buscando hermanas,
        /// y su seleccion es la multiple de AutoCAD sin filtro de tipo; lo que no es un rack lo informa el plan.
        /// </summary>
        [Fact]
        public void GUARDA_G_R4_RACKDUPLICAR_NO_BARRE_EL_DIBUJO()
        {
            var code = PluginSourceCode.Mask(Plugin("RackDuplicarCommands.cs"));

            Assert.DoesNotMatch(@"\b(?:FindRackBlocks|ScanEnvelopes|SelectAll)\b", code);

            var selection = Assert.Single(PluginSourceCode.Calls(code, "GetSelection"));
            Assert.True(selection.Arguments.Count <= 1, "la seleccion lleva filtro: " + selection.ArgumentText);
            Assert.DoesNotMatch(@"\b(?:SelectionFilter|TypedValue|SingleOnly)\b", code);
        }

        /// <summary>
        /// I-51 G5 — RACKDUPLICAR esta cableado al planificador y no conserva un camino paralelo: seleccion multiple y no la
        /// de una entidad; UN plan como unica autoridad de grupos, definiciones y referencias; UN asignador de destinos; y
        /// ninguna agrupacion hecha a mano en el comando, ni por RackId, ni por kind, ni comparando disenos.
        /// </summary>
        [Fact]
        public void GUARDA_G5_RACKDUPLICAR_ESTA_CABLEADO_AL_PLANIFICADOR()
        {
            var code = PluginSourceCode.Mask(Plugin("RackDuplicarCommands.cs"));

            Assert.Single(Regex.Matches(code, @"\bRackDuplicationPlan\s*\.\s*Build\s*\("));
            Assert.Single(Regex.Matches(code, @"\.\s*CreateDestinationAssigner\s*\("));
            Assert.Single(Regex.Matches(code, @"\.\s*Next\s*\(\s*\)"));
            Assert.Single(PluginSourceCode.Calls(code, "GetSelection"));
            Assert.DoesNotMatch(@"\bGetEntity\s*\(", code);

            // Lo que forma cada copia sale del plan.
            Assert.Matches(@"\.\s*Groups\b", code);
            Assert.Matches(@"\.\s*Definitions\b", code);
            Assert.Matches(@"\.\s*References\b", code);

            // Ninguna agrupacion paralela.
            Assert.DoesNotMatch(
                @"\b(?:GroupBy|ToLookup|IsSameAuthority|SelectiveAuthoredAuthority|KindHandlerDispatch)\b|\bRackDuplicationSourceKey\s*\.\s*For\w+\s*\(|\.\s*Id\b",
                code);
        }

        /// <summary>
        /// I-51 G4, G-R5 (amplia, sin retirarlas, <see cref="GUARDA_EL_RESTAMP_COMPARTIDO_NO_TIENE_MEJOR_ESFUERZO"/> y la guarda
        /// de Push Back sobre el mismo archivo). Una sola implementacion y una sola identidad: la firma historica es el UNICO
        /// lugar que inventa un GUID, y solo delega; la entrada con <see cref="Guid"/> rechaza el vacio, convierte la
        /// identidad a texto UNA vez, y ese mismo texto es el Id del sobre y el que recibe el re-estampado interior
        /// (NI-1, NI-2). No exige nombres de variables ni formato.
        /// </summary>
        [Fact]
        public void GUARDA_G_R5_EL_RESTAMP_TIENE_UNA_IMPLEMENTACION_Y_UNA_IDENTIDAD()
        {
            var code = PluginSourceCode.Mask(Plugin("RackEnvelopeRestamp.cs"));

            // Sigue sin mejor esfuerzo y resolviendo el kind sin distinguir mayusculas.
            Assert.DoesNotContain("catch (", code);
            Assert.DoesNotContain("return designJson;", code);
            Assert.Contains("TryGetIgnoreCase(", code);

            // Dos entradas: la historica (payload, nombre) y la que recibe la identidad.
            var overloads = PluginSourceCode.Members(code).Where(m => m.Name == "RestampEnvelope").ToList();
            Assert.Equal(2, overloads.Count);
            var historic = Assert.Single(overloads, m => m.Parameters.Count == 2 && !m.Parameters.Any(p => IsGuid(p.Type)));
            var withId = Assert.Single(overloads, m => m.Parameters.Count(p => IsGuid(p.Type)) == 1);

            // Un unico GUID inventado en todo el archivo: el de la firma historica, que no hace nada mas que delegar.
            Assert.Single(Regex.Matches(code, @"\bNewGuid\s*\("));
            Assert.Matches(@"\bNewGuid\s*\(", historic.Body);
            Assert.Single(PluginSourceCode.Calls(historic.Body, "RestampEnvelope"));
            Assert.DoesNotMatch(@"\b(?:Deserialize|Serialize|RestampDesign)\s*\(", historic.Body);

            // La entrada con Guid rechaza el vacio y convierte la identidad a texto una sola vez...
            var id = withId.Parameters.Single(p => IsGuid(p.Type)).Name;
            Assert.Matches(@"\bGuid\s*\.\s*Empty\b", withId.Body);
            Assert.Single(Regex.Matches(withId.Body, @"\b" + id + @"\s*\.\s*ToString\s*\("));

            // ...y ese mismo texto es el Id del sobre y el que recibe el re-estampado interior.
            var envelope = Assert.Single(Regex.Matches(withId.Body, @"\b(?<target>\w+)\s*\.\s*Id\s*=(?!=)\s*(?<value>[^;]+);"));
            var value = Regex.Replace(envelope.Groups["value"].Value, @"\s+", string.Empty);
            var sameText = new List<string> { envelope.Groups["target"].Value + ".Id" };
            var text = Regex.Match(withId.Body, @"(?<!\.\s*)\b(?<text>\w+)\s*=(?!=)\s*" + id + @"\s*\.\s*ToString\s*\(");

            if (text.Success)
            {
                sameText.Add(text.Groups["text"].Value);
                Assert.Equal(text.Groups["text"].Value, value);
            }
            else
            {
                Assert.StartsWith(id + ".ToString(", value);
            }

            var design = Assert.Single(PluginSourceCode.Calls(withId.Body, "RestampDesign"));
            Assert.Contains(design.Arguments, argument => sameText.Contains(Regex.Replace(argument, @"\s+", string.Empty)));
        }

        private static bool IsGuid(string type) => type == "Guid" || type == "System.Guid";

        /// <summary>
        /// I-51 G-R6 (G4, INV-11 e INV-14; ampliada en G5). Duplicar no regenera, no renombra, no crea capas, no importa
        /// bloques ni purga; despues de colocar un destino solo informa; y la conversion UCS→WCS del desplazamiento ocurre
        /// UNA vez por destino: en quien pide el punto, despues de pedirlo y antes de colocar, nunca dentro de la mutacion.
        /// </summary>
        [Fact]
        public void GUARDA_G_R6_RACKDUPLICAR_NO_TOCA_DE_MAS_Y_TRANSFORMA_UNA_VEZ_POR_DESTINO()
        {
            var code = PluginSourceCode.Mask(Plugin("RackDuplicarCommands.cs"));

            Assert.DoesNotMatch(
                @"\b(?:Regen|SyncName|EnsureLayer|EnsureForPlan|EnsureBlocks|PurgeUnreferenced|PurgeAfterCommit)\s*\(", code);

            const string Ucs = @"\bCurrentUserCoordinateSystem\b";
            Assert.Single(Regex.Matches(code, Ucs));

            var members = PluginSourceCode.Members(code);
            var mutation = Assert.Single(members, m => PluginSourceCode.Calls(m.Body, "CloneDefinition").Count > 0);
            var loop = Assert.Single(members, m => m != mutation && PluginSourceCode.Calls(m.Body, mutation.Name).Count > 0);
            var place = Assert.Single(PluginSourceCode.Calls(loop.Body, mutation.Name));
            var transform = Assert.Single(Regex.Matches(loop.Body, Ucs));

            Assert.True(Regex.Matches(loop.Body, @"\bGetPoint\s*\(").Any(point => point.Index < transform.Index),
                "la conversion UCS->WCS no sigue a la lectura del punto de destino.");
            Assert.True(transform.Index < place.Start, "la conversion UCS->WCS no precede a " + mutation.Name + ".");

            // Despues de colocar el destino solo se informa: ni otra transaccion, ni clones, ni entidades nuevas o editadas.
            Assert.False(
                PluginSourceCode.Reaches(members, loop.Body.Substring(place.End), "Run", "CloneDefinition", "AppendEntity", "UpgradeOpen", "Erase"),
                "despues de " + mutation.Name + " el comando vuelve a tocar el dibujo.");
        }

        [Fact]
        public void GUARDA_RACKLAYOUT_DECIDE_ANTES_DE_CLONAR()
        {
            var source = Plugin("RackLayoutCommands.cs");

            Assert.True(At(source, "RestampEnvelope(") < At(source, "RackCloner.CloneDefinition"));
            Assert.Contains("IsSuccess", source);
        }

        /// <summary>
        /// Ningún camino de copia se queda con el payload de origen cuando el re-estampado falla. I-51 G4: la mitad de
        /// RACKDUPLICAR pasó a <see cref="GUARDA_G_R2_RACKDUPLICAR_NUNCA_CLONA_EL_PAYLOAD_DE_ORIGEN"/>; RACKLAYOUT sigue literal.
        /// </summary>
        [Fact]
        public void GUARDA_NINGUN_CAMINO_DE_COPIA_CAE_AL_PAYLOAD_DE_ORIGEN()
        {
            var source = Plugin("RackLayoutCommands.cs");

            Assert.DoesNotContain("source.Payload, copyName)", source.Replace("RestampEnvelope(source.Payload, copyName)", string.Empty));
            Assert.DoesNotContain("CloneDefinition(database, transaction, source.DefinitionId, copyName, source.Payload", source);
            Assert.DoesNotContain("CloneDefinition(database, transaction, seed.DefinitionId, copyName, seed.Payload", source);
        }
    }

    /// <summary>
    /// I-51 G4 — lectura ESTRUCTURAL de una fuente del Plugin para las guardas G-R1..G-R6. El Plugin no se carga en las
    /// pruebas (ADR-0003), así que las guardas leen texto; pero una guarda de literales se rompe con cualquier refactor
    /// honesto y deja pasar uno deshonesto escrito de otra forma. Esto enmascara los comentarios y el contenido de los
    /// literales (misma longitud, mismos saltos de línea) para que las comprobaciones vean solo código, y separa miembros
    /// y argumentos por llaves y paréntesis balanceados. No es un parser de C#: no entiende comillas dentro de los huecos
    /// de una cadena interpolada, y los archivos guardados no las usan.
    /// </summary>
    internal static class PluginSourceCode
    {
        internal sealed record Member(string Name, IReadOnlyList<(string Type, string Name)> Parameters, string Body);

        internal sealed record Call(int Start, int End, IReadOnlyList<string> Arguments)
        {
            public string ArgumentText => string.Join(", ", Arguments);
        }

        private static readonly Regex Signature = new Regex(
            @"\b(?:public|private|internal|protected)\s+(?:(?:static|override|virtual|sealed|async|unsafe|extern)\s+)*"
            + @"(?<type>[\w.]+(?:<[^<>()]*>)?[?\[\]]*)\s+(?<name>\w+)\s*\((?<parameters>[^()]*(?:\([^()]*\)[^()]*)*)\)\s*(?<open>\{|=>)");

        private static readonly Regex Parameter = new Regex(
            @"^(?:(?:this|ref|out|in|params)\s+)*(?<type>.+?)\s+(?<name>\w+)$", RegexOptions.Singleline);

        /// <summary>El código sin comentarios ni contenido de literales; los delimitadores quedan.</summary>
        public static string Mask(string source) => Scan(source, null);

        /// <summary>El contenido de cada literal de cadena, tal como está escrito.</summary>
        public static IReadOnlyList<string> StringLiterals(string source)
        {
            var literals = new List<string>();
            Scan(source, literals);
            return literals;
        }

        /// <summary>Métodos con modificador de acceso y su cuerpo: de llave a llave, o de <c>=&gt;</c> al <c>;</c>.</summary>
        public static IReadOnlyList<Member> Members(string code)
        {
            var members = new List<Member>();

            foreach (Match match in Signature.Matches(code))
            {
                var open = match.Groups["open"];
                var end = open.Value == "{" ? Closing(code, open.Index) : StatementEnd(code, open.Index);
                var parameters = Split(match.Groups["parameters"].Value, genericBrackets: true)
                    .Select(declaration => Parameter.Match(declaration.Split('=')[0].Trim()))
                    .Select(declaration => (declaration.Groups["type"].Value.Trim(), declaration.Groups["name"].Value))
                    .ToList();

                members.Add(new Member(match.Groups["name"].Value, parameters, code.Substring(open.Index, end - open.Index)));
            }

            return members;
        }

        /// <summary>Cada llamada a <paramref name="name"/> en <paramref name="code"/>, con sus argumentos de primer nivel.</summary>
        public static IReadOnlyList<Call> Calls(string code, string name)
        {
            var calls = new List<Call>();

            foreach (Match match in Regex.Matches(code, @"\b" + Regex.Escape(name) + @"\s*\("))
            {
                var open = match.Index + match.Length - 1;
                var close = Closing(code, open);
                calls.Add(new Call(match.Index, close, Split(code.Substring(open + 1, close - open - 2), genericBrackets: false)));
            }

            return calls;
        }

        /// <summary>
        /// Si <paramref name="text"/> llama a alguno de <paramref name="names"/>, directamente o a traves de los miembros de
        /// <paramref name="members"/> que llama, a cualquier profundidad.
        /// </summary>
        public static bool Reaches(IReadOnlyList<Member> members, string text, params string[] names)
        {
            var visited = new HashSet<Member>();
            var pending = new Stack<string>();
            pending.Push(text);

            while (pending.Count > 0)
            {
                var current = pending.Pop();

                if (names.Any(name => Calls(current, name).Count > 0))
                {
                    return true;
                }

                foreach (var member in members)
                {
                    if (Calls(current, member.Name).Count > 0 && visited.Add(member))
                    {
                        pending.Push(member.Body);
                    }
                }
            }

            return false;
        }

        private static string Scan(string source, List<string> literals)
        {
            var code = new StringBuilder(source.Length);
            var i = 0;

            while (i < source.Length)
            {
                var next = i + 1 < source.Length ? source[i + 1] : '\0';

                if (source[i] == '/' && next == '/')
                {
                    var end = source.IndexOf('\n', i);
                    i = Blank(source, i, end < 0 ? source.Length : end, code);
                }
                else if (source[i] == '/' && next == '*')
                {
                    var end = source.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    i = Blank(source, i, end < 0 ? source.Length : end + 2, code);
                }
                else if (source[i] == '\'')
                {
                    i = Literal(source, i, i + 1, LiteralEnd(source, i + 1, verbatim: false, closer: '\''), 1, code, null);
                }
                else if (StringStart(source, i, out var quote))
                {
                    var verbatim = source.IndexOf('@', i, quote - i) >= 0;
                    var quotes = 0;
                    while (quote + quotes < source.Length && source[quote + quotes] == '"')
                    {
                        quotes++;
                    }

                    if (!verbatim && quotes >= 3)
                    {
                        var end = source.IndexOf(new string('"', quotes), quote + quotes, StringComparison.Ordinal);
                        i = Literal(source, i, quote + quotes, end < 0 ? source.Length : end, quotes, code, literals);
                    }
                    else
                    {
                        i = Literal(source, i, quote + 1, LiteralEnd(source, quote + 1, verbatim, closer: '"'), 1, code, literals);
                    }
                }
                else
                {
                    code.Append(source[i]);
                    i++;
                }
            }

            return code.ToString();
        }

        private static bool StringStart(string source, int at, out int quote)
        {
            quote = at;
            while (quote < source.Length && (source[quote] == '$' || source[quote] == '@'))
            {
                quote++;
            }

            return quote < source.Length && source[quote] == '"';
        }

        private static int LiteralEnd(string source, int from, bool verbatim, char closer)
        {
            var at = from;

            while (at < source.Length)
            {
                if (source[at] == closer)
                {
                    if (verbatim && at + 1 < source.Length && source[at + 1] == closer)
                    {
                        at += 2;
                        continue;
                    }

                    break;
                }

                if (!verbatim && source[at] == '\n')
                {
                    break;
                }

                at += !verbatim && source[at] == '\\' ? 2 : 1;
            }

            return Math.Min(at, source.Length);
        }

        private static int Literal(string source, int start, int contentStart, int contentEnd, int closing, StringBuilder code, List<string> literals)
        {
            code.Append(source, start, contentStart - start);
            Blank(source, contentStart, contentEnd, code);
            literals?.Add(source.Substring(contentStart, contentEnd - contentStart));

            var end = Math.Min(contentEnd + closing, source.Length);
            code.Append(source, contentEnd, end - contentEnd);
            return end;
        }

        private static int Blank(string source, int from, int to, StringBuilder code)
        {
            for (var at = from; at < to; at++)
            {
                code.Append(source[at] == '\n' || source[at] == '\r' ? source[at] : ' ');
            }

            return to;
        }

        private static int Closing(string code, int open)
        {
            var depth = 0;

            for (var at = open; at < code.Length; at++)
            {
                if (code[at] == '(' || code[at] == '{' || code[at] == '[')
                {
                    depth++;
                }
                else if ((code[at] == ')' || code[at] == '}' || code[at] == ']') && --depth == 0)
                {
                    return at + 1;
                }
            }

            Assert.Fail("sin cierre balanceado desde la posicion " + open + ".");
            return code.Length;
        }

        private static int StatementEnd(string code, int from)
        {
            var depth = 0;

            for (var at = from; at < code.Length; at++)
            {
                if (code[at] == '(' || code[at] == '{' || code[at] == '[')
                {
                    depth++;
                }
                else if (code[at] == ')' || code[at] == '}' || code[at] == ']')
                {
                    depth--;
                }
                else if (code[at] == ';' && depth == 0)
                {
                    return at + 1;
                }
            }

            Assert.Fail("sin ';' de cierre desde la posicion " + from + ".");
            return code.Length;
        }

        private static IReadOnlyList<string> Split(string text, bool genericBrackets)
        {
            var parts = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return parts;
            }

            var depth = 0;
            var start = 0;

            for (var at = 0; at < text.Length; at++)
            {
                var c = text[at];

                if (c == '(' || c == '{' || c == '[' || (genericBrackets && c == '<'))
                {
                    depth++;
                }
                else if (c == ')' || c == '}' || c == ']' || (genericBrackets && c == '>'))
                {
                    depth--;
                }
                else if (c == ',' && depth == 0)
                {
                    parts.Add(text.Substring(start, at - start).Trim());
                    start = at + 1;
                }
            }

            parts.Add(text.Substring(start).Trim());
            return parts;
        }
    }
}
