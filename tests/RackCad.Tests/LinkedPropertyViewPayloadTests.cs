using System;
using System.Collections.Generic;
using System.IO;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-48 gate G4C.1 — el PAYLOAD que porta una vista insertada durante una edicion.
    ///
    /// <para>
    /// G4C dejo el documento final en manos del reconciler y lo serializo UNA vez para redibujar las vistas
    /// que ya existian. Pero insertar una vista NUEVA en esa misma operacion seguia reconstruyendo el
    /// documento desde el <see cref="SelectivePalletDesign"/> EFECTIVO, y eso no es el mismo documento: un
    /// diseno de dominio no puede llevar vinculos, asi que la vista nueva nacia sin
    /// <c>PropertyValues</c> y con el valor de la variable escrito como si fuera el literal del rack.
    /// </para>
    /// <para>
    /// El resultado es divergencia AUTHORED entre hermanas del mismo rack — el estado que
    /// <c>SelectiveAuthoredAuthority</c> declara irreconciliable y que bloquea toda operacion sobre ese rack.
    /// Un rack vinculado quedaba roto por el gesto normal de insertar una vista mas.
    /// </para>
    /// <para>
    /// La frontera que este correctivo arregla es <b>authored reconciliado → payload de la vista</b>, y nada
    /// mas: la geometria sigue saliendo del sistema que el editor resolvio, y la semantica de edicion no se
    /// toca.
    /// </para>
    /// </summary>
    public class LinkedPropertyViewPayloadTests
    {
        private const string RackId = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string IdX = "11111111-1111-1111-1111-111111111111";
        private const string Token = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        private static VariableId Id(string guid) => VariableId.Parse(guid);

        private static PropertyId Clearance => ProjectPropertyIds.SelectiveVerticalClearance;

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

        /// <summary>El rack tal y como esta guardado: literal 6 congelado y gobernado por X.</summary>
        private static SelectivePalletDesignDocument Vinculado()
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(6.0), RackId, "Rack A");

            doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [Token] = SelectivePropertyValueDocument.ToProjectVariable(IdX),
            };
            doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            return doc;
        }

        /// <summary>El registro: X vale 10.</summary>
        private static ProjectVariablesReadResult Registro()
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                new ProjectVariableDocument
                {
                    VariableId = IdX,
                    Name = "Holgura General",
                    Type = VariableType.Length.ToString(),
                    Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = 10.0 },
                },
            };

            return ProjectVariablesReadResult.Readable(document);
        }

        /// <summary>La sesion de edicion tal cual la deja abrir un rack vinculado sin tocar nada.</summary>
        private static LinkedPropertyReconciliation Reconciliado()
        {
            var final = new Dictionary<PropertyId, LinkedPropertyEditState>
            {
                [Clearance] = LinkedPropertyEditState.Reference(6.0, Id(IdX)),
            };

            // El diseno que la ventana construye lleva el EFECTIVO (10): es el valor en vigor y el dibujo
            // tiene que reflejarlo.
            var result = LinkedPropertyReconciler.Reconcile(
                Vinculado(), Diseno(10.0), final, Registro(), RackId, "Rack A");

            Assert.True(result.IsSuccess);
            return result;
        }

        // ================================================================ A. el comportamiento puro

        /// <summary>
        /// G4C.1 (A). El punto de partida: reconciliar produce un authored con el literal CONGELADO y su
        /// vinculo, y un efectivo que vale lo que vale la variable. Son dos documentos distintos a proposito.
        /// </summary>
        [Fact]
        public void RECONCILIAR_SEPARA_EL_AUTHORED_CONGELADO_DEL_EFECTIVO()
        {
            var reconciled = Reconciliado();

            Assert.Equal(6.0, reconciled.Authored.VerticalClearance);
            Assert.True(reconciled.Authored.TryGetBinding(Clearance, out var bound));
            Assert.Equal(Id(IdX), bound);

            Assert.Equal(10.0, reconciled.Effective.VerticalClearance);
        }

        // ================================================================ D. por que importa

        /// <summary>
        /// G4C.1 (D) — la regresion EXPLICATIVA. Reconstruir el documento desde el diseno efectivo no produce
        /// «casi» el mismo authored: produce uno que la autoridad heredada declara DIVERGENTE.
        ///
        /// <para>
        /// Y la divergencia es doble: desaparece el vinculo, y el literal pasa a ser el valor de la variable.
        /// Una hermana asi, junto a las que si llevan el authored reconciliado, deja el rack entero sin
        /// autoridad — que es exactamente el estado que ninguna operacion puede resolver eligiendo una vista.
        /// </para>
        /// </summary>
        [Fact]
        public void RECONSTRUIR_EL_DOCUMENTO_DESDE_EL_EFECTIVO_PRODUCE_DIVERGENCIA_AUTHORED()
        {
            var reconciled = Reconciliado();

            // Lo que hacia el camino de insercion: SelectivePalletDesignDocument.From(design efectivo).
            var desdeElEfectivo = SelectivePalletDesignDocument.From(reconciled.Effective, RackId, "Rack A");

            // 1) pierde el vinculo...
            Assert.True(reconciled.Authored.TryGetBinding(Clearance, out _));
            Assert.False(desdeElEfectivo.TryGetBinding(Clearance, out _));

            // 2) ...y materializa el efectivo como si fuera el literal del rack.
            Assert.Equal(6.0, reconciled.Authored.VerticalClearance);
            Assert.Equal(10.0, desdeElEfectivo.VerticalClearance);

            // 3) por eso las dos hermanas NO son la misma autoridad.
            Assert.False(SelectiveAuthoredAuthority.IsSameAuthority(
                new[] { reconciled.Authored, desdeElEfectivo }));

            // Centinela: dos copias del authored reconciliado SI lo son, asi que el fallo anterior es por el
            // contenido y no porque la comparacion rechace cualquier par.
            Assert.True(SelectiveAuthoredAuthority.IsSameAuthority(
                new[] { reconciled.Authored, reconciled.Authored }));
        }

        // ================================================================ C. envolver NO reinterpreta

        /// <summary>
        /// G4C.1 (C). El oraculo de comportamiento del correctivo: envolver un documento YA serializado lo
        /// transporta verbatim, y cambiar la vista o la seccion no toca el diseno embebido.
        ///
        /// <para>
        /// Es lo que permite que las hermanas existentes y la vista nueva porten el MISMO authored: lo unico
        /// que puede variar entre ellas es el sobre.
        /// </para>
        /// </summary>
        [Fact]
        public void ENVOLVER_UN_DOCUMENTO_YA_SERIALIZADO_NO_LO_REINTERPRETA()
        {
            var designJson = new SelectivePalletDesignStore().Serialize(Reconciliado().Authored);

            var frontal = RackEmbedComposer.Compose(
                null, RackEmbedDocument.KindSelective, RackId, "Rack A", RackEmbedDocument.ViewFrontal, 0, designJson);
            var lateral = RackEmbedComposer.Compose(
                null, RackEmbedDocument.KindSelective, RackId, "Rack A", RackEmbedDocument.ViewLateral, 2, designJson);
            var planta = RackEmbedComposer.Compose(
                null, RackEmbedDocument.KindSelective, RackId, "Rack A", RackEmbedDocument.ViewPlanta, -1, designJson);

            // Verbatim: el sobre no reinterpreta el documento.
            Assert.Equal(designJson, frontal.Design);

            // Y la vista y la seccion son lo UNICO que cambia entre hermanas.
            Assert.Equal(frontal.Design, lateral.Design);
            Assert.Equal(frontal.Design, planta.Design);
            Assert.NotEqual(frontal.View, lateral.View);
            Assert.NotEqual(frontal.Section, lateral.Section);
        }

        [Fact]
        public void UN_DOCUMENTO_ENVUELTO_SE_VUELVE_A_LEER_CON_SU_VINCULO_Y_SU_LITERAL_CONGELADO()
        {
            // El viaje completo: reconciliar -> serializar -> envolver -> leer. Lo que la vista nueva guarda
            // es lo que RACKEDITAR volvera a abrir.
            var store = new SelectivePalletDesignStore();
            var designJson = store.Serialize(Reconciliado().Authored);

            var embed = RackEmbedComposer.Compose(
                null, RackEmbedDocument.KindSelective, RackId, "Rack A", RackEmbedDocument.ViewLateral, 1, designJson);

            var releido = store.Deserialize(embed.Design);

            Assert.Equal(6.0, releido.VerticalClearance);
            Assert.True(releido.TryGetBinding(Clearance, out var bound));
            Assert.Equal(Id(IdX), bound);
        }

        // ================================================================ B. el wiring del Plugin

        /// <summary>
        /// G4C.1 (B). El acto fisico de insertar necesita AutoCAD, asi que la integracion se comprueba sobre la
        /// FUENTE. La guarda es especifica a proposito: no basta con que la rama mencione <c>designJson</c>,
        /// tiene que dejar de pasar el <c>design</c> de dominio por el camino de insercion.
        /// </summary>
        [Fact]
        public void GUARDA_INSERTAR_DURANTE_UNA_EDICION_PORTA_EL_AUTHORED_RECONCILIADO()
        {
            var statement = EditInsertStatement();

            // Lo que tiene que llevar: el documento ya reconciliado y serializado.
            Assert.Contains("designJson", statement);

            // Lo que NO puede volver a hacer: entregar el diseno de dominio para que alguien lo reconstruya.
            Assert.DoesNotContain(" design,", statement);
        }

        /// <summary>
        /// La inversa, para que la guarda anterior no se pueda satisfacer rompiendo la insercion NUEVA: un rack
        /// recien creado no tiene portador previo ni vinculos, asi que ahi construir desde el diseno es correcto
        /// y sigue siendo el camino.
        /// </summary>
        [Fact]
        public void GUARDA_LA_INSERCION_DE_UN_RACK_NUEVO_SIGUE_CONSTRUYENDO_DESDE_EL_DISENO()
        {
            Assert.Contains(
                "DrawSelectiveView(window.InsertView, window.SystemToInsert, window.DesignToInsert",
                Commands());
        }

        /// <summary>
        /// La SENTENCIA que inserta la vista nueva dentro de <c>EditSelective</c>.
        ///
        /// <para>
        /// Se ancla en <c>window.InsertView</c> dentro del cuerpo de ese metodo, y no en
        /// <c>if (!window.UpdateOnly)</c>: esa condicion aparece DOS veces —la primera es la guarda de unidades
        /// (I-05)— asi que anclar ahi medía un bloque que no es el que se esta probando. El ancla tambien
        /// acota al metodo, porque la insercion de un rack NUEVO usa la misma propiedad en otro sitio.
        /// </para>
        /// </summary>
        private static string EditInsertStatement()
        {
            var body = Body(Commands(), "internal static void EditSelective(");
            var at = body.IndexOf("window.InsertView", StringComparison.Ordinal);

            Assert.True(at >= 0, "EditSelective tiene que insertar la vista pedida");

            var start = body.LastIndexOf('\n', at) + 1;
            var end = body.IndexOf(';', at);

            Assert.True(end > at, "la insercion no termina en ;");

            return body.Substring(start, end - start);
        }

        /// <summary>El cuerpo de un metodo, por emparejamiento de llaves desde su firma.</summary>
        private static string Body(string source, string signature)
        {
            var at = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.True(at >= 0, "no se encontro la firma: " + signature);

            var open = source.IndexOf('{', at);
            var depth = 0;

            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{')
                {
                    depth++;
                }
                else if (source[i] == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        return source.Substring(open, i - open + 1);
                    }
                }
            }

            Assert.Fail("no se pudo delimitar el cuerpo de " + signature);
            return null;
        }

        private static string Commands()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
            {
                directory = directory.Parent;
            }

            Assert.NotNull(directory);

            var path = Path.Combine(
                directory.FullName, "src", "RackCad.Plugin", "RackSelectivoCommands.cs");

            Assert.True(File.Exists(path), "no se encontro la fuente: " + path);
            return File.ReadAllText(path);
        }
    }
}
