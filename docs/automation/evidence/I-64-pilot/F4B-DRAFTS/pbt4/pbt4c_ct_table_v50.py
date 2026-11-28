"""PB-T4c (paquete v5.0): la lista cerrada CT-PBT4C-01 a 15, como datos: que fija cada CT, sus lineas esperadas literales (el Assert.Equal
de las lineas unidas por \n), su clase, su proyecto y su ExpectRed. make_v50.py la escribe en §5.3 (una fila por CT, con las lineas entre
comillas invertidas, como lee la cadena: rr3_table_check) y en §6.0 (la tabla de RR-3). Las lineas de CT-03, 04, 05 y 12 son las que dio la sonda
de la precondicion 6 sobre el GREEN literal (pbt4ccc/compile, `pbt4c-probe-editall.cs`), y las de CT-09, las de la sonda de UI
(`pbt4c-probe-close-ui.cs`; D-S10); las demas, del texto literal del GREEN.
"""
FORBIDDEN = ['RackPushBackCommands', 'RackMenuCommands', 'EditorModules', 'RackViewBatchExecution', 'DrawAndPlace', 'RedrawInPlace',
             'InsertRequested', 'InsertionRequest']
SUBJECTS = ['RackWorkspaceEditorWindow.cs', 'FullEditorModal', 'TryEditAll', 'EditAll', 'PushBackFullDraftLaunch.cs', 'PushBackFullDraftStatePort.cs']
GL1 = ['a', 'b', 'c', 'e', 'f1', 'f2', 'f3', 'g1', 'g2', 'h1', 'h2', 'i', 'j', 'k1', 'k2']
PURITY = ['Autodesk', 'RackCad.Plugin', 'RackCad.UI', 'System.Windows', 'Transaction', 'Database', 'async', 'await', 'Task', 'File.', 'System.IO',
          'RackEmbedComposer', 'new RackEmbedDocument']
BUSY = '[Hay una lectura del piloto en curso.]'
REFUSED = '[El borrador no admite el editor completo en su estado actual.]'

CT = [
    ('01', '**El botón, en texto** (D-PBT4-12): en `WorkspacePanelView.xaml`, `BrowserFieldEditAllButton` una vez, en el `WrapPanel` de los '
           'botones del campo y justo tras `BrowserFieldEditButton`, con sus cuatro atributos literales; en `WorkspacePanelView.xaml.cs`, '
           '`OnBrowserFieldEditAllClick` una vez, con su cuerpo literal',
     ['xaml BrowserFieldEditAllButton=1 en-el-panel-de-los-botones=True tras=BrowserFieldEditButton',
      'xaml Content=[Editar todo] AutomationProperties.Name=[Editar el rack completo en el editor]',
      'xaml Visibility=[{Binding Browser.Field.ShowEditAllButton, Converter={StaticResource FlagToVisibility}}] Click=[OnBrowserFieldEditAllClick]',
      'code-behind OnBrowserFieldEditAllClick=1 cuerpo=[Contained(() => _viewModel.Browser.Field.EditAll())]'],
     'PushBackEditAllGuardTests', 'Core', True),
    ('02', '**El gesto y sus avisos** (D-PBT4-12; R-PB-10), con la FVM real sobre un doble nuevo del puerto, en la clase, que registra y responde '
           '`Closed` sin aplicar: con texto pendiente, `CommitField` `Invalid` (mensaje `Valor no numérico.`), `Busy` (motivo '
           '`Hay una lectura del piloto en curso.`) o `Rejected` (motivo `La petición no es del documento activo.`) no llaman a `TryEditAll`; '
           '`Applied` y `NotEditable`, sí',
     ['invalido TryEditAll=0 mensaje=[Valor no numérico.] aviso=[No se abrió el editor: Valor no numérico.]',
      'ocupado TryEditAll=0 aviso=[El texto no se aplicó: Hay una lectura del piloto en curso.]',
      'rechazado TryEditAll=0 aviso=[El texto no se aplicó: La petición no es del documento activo.]',
      'valido TryEditAll=1 aviso=[Editor cerrado sin cambios en el borrador.]',
      'no-editable TryEditAll=1 aviso=[Editor cerrado sin cambios en el borrador.]'],
     'PushBackEditAllWindowTests', 'UI', True),
    ('03', '**`TryEditAll` del anfitrión** (D-PBT4-13), por el puerto (`IWorkspacePilotEditPort`) sobre `FullReadRig` y el lector del kit, con un '
           'doble de `IWorkspaceFullEditHost` que registra y no aplica: sin anfitrión; sin borrador (lo empieza, con su lectura); la cola aparcada '
           '(`pb-21-cola`); Clean, Dirty y Conflict; Applying; Unknown y Error; texto pendiente; y otra sesión',
     ['sin-anfitrion estado=NotAvailable motivo=[No editable aquí: el panel no tiene el editor completo.] abierto=False lecturas=0 borrador=False',
      'sin-borrador estado=Closed abierto=True lecturas=1 borrador=Clean',
      'cola estado=NotBegun inicio=NotAdmitted diagnostico-fuera-del-piloto=True pista=True abierto=False borrador=False',
      'clean estado=Closed abierto=True lecturas=0',
      'dirty estado=Closed abierto=True lecturas=0',
      'conflict estado=Closed abierto=True lecturas=0',
      'applying estado=Waiting motivo=[El borrador se está aplicando o se retiró: el editor no se abre.] abierto=False',
      'unknown estado=Refused motivo=' + REFUSED + ' abierto=False',
      'error estado=Refused motivo=' + REFUSED + ' abierto=False',
      'texto-pendiente estado=Refused motivo=[Hay texto sin confirmar en el campo.] abierto=False',
      'otra-sesion estado=Rejected motivo=[La petición no es del documento activo.] abierto=False lecturas=0'],
     'PushBackEditAllTests', 'Core', True),
    ('04', '**El lanzamiento y el adaptador del puerto** (D-PBT4-13): dentro de `ShowModal`, el doble lee el lanzamiento (el estado del borrador, '
           'el rack y el proyecto del sobre del origen) y aplica dos veces por su puerto (escala `2.5` y `3`); el origen se conserva; tras cerrar, '
           '`Closed` con las dos y el borrador Dirty. Y el adaptador solo, sobre un borrador Applying y sobre uno Clean',
     ['lanzamiento estado-igual=True rack=True proyecto=True proyecto-con-diseno=True',
      'aplicar-1 estado=Applied isdirty=True origen-igual=True escala=2.5',
      'aplicar-2 estado=Applied isdirty=True escala=3',
      'tras-aplicar estado=Closed aplicadas=2 borrador=Dirty origen-igual=True',
      'adaptador-no-editable estado=NotEditable llamadas=0',
      'adaptador-aplica estado=Applied llamadas=1'],
     'PushBackEditAllTests', 'Core', True),
    ('05', '**K = 1** (D-PBT4-14; R-PB-10): dentro de `ShowModal` (el doble pone el modal con `SetModalRackCadActive`), cada operación del puerto '
           'sobre el borrador abierto, con su respuesta y su motivo; ninguna lectura del dibujo dentro; y, tras cerrar, el panel vuelve a responder',
     ['k1 SetPendingText Busy ' + BUSY, 'k1 CommitField Busy ' + BUSY, 'k1 CrossBoundary Busy ' + BUSY, 'k1 DiscardRack Busy ' + BUSY,
      'k1 DiscardDocument Busy ' + BUSY, 'k1 TryBeginEdit Waiting ' + BUSY, 'k1 TryCheckNow Waiting ' + BUSY, 'k1 TryReload Waiting ' + BUSY,
      'k1 TryConfirmBase Waiting ' + BUSY, 'k1 TryKeepMine Waiting ' + BUSY, 'k1 TryDiscardAndReload Waiting ' + BUSY,
      'k1 TryEditAll Waiting ' + BUSY, 'k1 RequestUpdate Busy ' + BUSY, 'k1 GetField existe=True estado=Clean',
      'k1 GetRacksAtRisk conocido=True', 'k1 TakeLossNotices 0 TakeCloseNotices 0', 'k1 TakeUpdateProgress en-vuelo=False',
      'k1-cerrado estado=Closed lecturas-dentro=0', 'despues SetPendingText Applied TryBeginEdit AlreadyOpen'],
     'PushBackEditAllTests', 'Core', True),
    ('06', '**El resumen y «Modificado» al cerrar** (A2-9), con la FVM real sobre el doble del puerto de CT-02: con un aplicar, el doble pasa su campo '
           'a Dirty y responde `Closed` con 1; sin aplicar, `Closed` con 0; el navegador recibe su aviso de cambio una vez',
     ['con-aplicar estado=[Modificado] aviso=[Editor cerrado: sus cambios están en el borrador.] cambiado=1',
      'sin-aplicar estado=[Limpio] aviso=[Editor cerrado sin cambios en el borrador.] cambiado=1'],
     'PushBackEditAllWindowTests', 'UI', True),
    ('07', '**La guarda de OB-01.3** (D-PBT4-16), en texto sin comentarios, sobre los seis sujetos: `RackWorkspaceEditorWindow.cs`, el cuerpo de '
           '`FullEditorModal` en WCo, los de `TryEditAll` y `EditAllOfPilot` en WHC, el de `EditAll` en FVM, `Draft/PushBackFullDraftLaunch.cs` y '
           '`Draft/PushBackFullDraftStatePort.cs`. Ninguno nombra los ocho; la autoprueba inyecta cada nombre en cada sujeto. Un sujeto que falta '
           '(en el RED) es su texto vacío',
     ['sujeto %s prohibidos=[]' % s for s in SUBJECTS] + ['autoprueba-%s-en-%s=falla' % (n, s) for s in SUBJECTS for n in FORBIDDEN],
     'PushBackOpenerClassicPathGuardTests', 'Core', False),
    ('08', '**Las dos piezas del abridor, en texto** (D-PBT4-15; M-30): `FullEditorModal` en WCo, que implementa `IWorkspaceFullEditHost`, con un '
           '`using (_controller.EnterPanelModal())` y la llamada a `_open`, sin `RackCad.UI` ni `PushBack`; `RackWorkspaceEditorWindow.Show`, con la '
           'conversión y la llamada literales, una vez cada una, sin `Owner =` ni `RackCad.Application.Workspace`; y el cableado en WCo, una vez, tras '
           '`SetPilotUpdateHost`',
     ['modal FullEditorModal=1 implementa=IWorkspaceFullEditHost using-EnterPanelModal=1 _open=1 RackCad.UI=0 PushBack=0',
      'abridor conversion=[var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);] veces=1',
      'abridor mostrar=[AcApplication.ShowModalWindow(window);] veces=1 Owner-asignado=0 RackCad.Application.Workspace=0',
      'cableado [_host.SetFullEditHost(new FullEditorModal(this, RackWorkspaceEditorWindow.Show));] veces=1 tras-SetPilotUpdateHost=True'],
     'PushBackEditAllGuardTests', 'Core', True),
    ('09', '**«Cerrar» con las dos cosas** (D-PBT4-17; A-T2b-OWNER-01; D-S7 y D-S10), con la ventana real: `PendingWork()` sin cerrar la ventana. '
           'En WORKSPACE (`pb-31-v2-modulos`), la cabecera escenificada con `SelectModule(w, "M3")`, `HeaderConfiguratorDialog` (una copia con '
           '`PanelClear` 40) y `ConfigureModuleHeaderButton`, sin construir el configurador, que deja también cambios sin aplicar (D-S10); en WORKSPACE '
           '(`pb-01-base`), solo cambios sin aplicar (`ToleranceBox` a 4.5) y ninguno; en CLASSIC (`pb-31-v2-modulos`), la misma cabecera',
     ['ambas pregunta=[Hay un cambio de módulo sin confirmar y cambios sin aplicar al borrador; se perderán los dos al cerrar. ¿Deseas continuar?]',
      'solo-sin-aplicar pregunta=[Hay cambios sin aplicar al borrador que se perderán al cerrar. ¿Deseas continuar?]',
      'ninguna pendiente=False',
      'clasico-modulo pregunta=[Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar?]'],
     'PushBackEditAllWindowTests', 'UI', True),
    ('10', '**De punta a punta sin AutoCAD** (A2-11; F4B-E30): el puerto real (`WorkspacePilotEditGate` sobre WHC) → `TryEditAll` → el doble de '
           '`IWorkspaceFullEditHost` aplica por el puerto del lanzamiento un gesto compuesto (un frente menos y la escala), una vez → «Actualizar» '
           'con `PilotEraseScopeDouble` (como CT-PBT4B-05) → `Applied` y Clean',
     ['editar-todo estado=Closed aplicadas=1 borrador=Dirty',
      'lo-aplicado-es-el-editado=True',
      'actualizar estado=Applied begin-mutation=1 seal=1 borrador=Clean',
      'borrado EraseUnit:B2'],
     'PushBackEditAllTests', 'Core', True),
    ('11', '**D2 en `ApplyToDraft`** (D-PBT4-18): la huella de las dos referencias compartidas antes y después de cada aplicar, y la condición 7 '
           'por el puerto (la línea base resuelta del estado aplicado, sustituida por la de otro fixture)',
     ['ApplyToDraft-1 estado=Applied huellas-iguales=True',
      'ApplyToDraft-2 estado=Applied huellas-iguales=True',
      'sustituir-referencia estado=Applied isdirty=False'],
     'PushBackEditAllTests', 'Core', True),
    ('12', '**La compuerta reenvía `TryEditAll`** (B-05): `new WorkspacePilotEditGate(core, depth)`, por el puerto: dentro de un aviso, `Waiting` '
           'con `InsideNoticeReason` y sin abrir; fuera, llega al anfitrión y abre',
     ['compuerta-dentro estado=Waiting motivo=[Se está entregando un aviso del panel.] abierto=False lecturas=0',
      'compuerta-fuera estado=Closed abierto=True lecturas=1'],
     'PushBackEditAllTests', 'Core', True),
    ('13', '**El adaptador del Plugin reenvía `TryEditAll`** (B-05): `ControllerViolations` (PEG:659-707) copiada con las 18 operaciones (la tabla '
           'del equivalente de PEG:155-163), con dos autopruebas: un adaptador sin el reenvío y uno que no solo delega',
     ['adaptador operaciones=18 violaciones=[]', 'autoprueba-sin-reenvio-de-TryEditAll=falla', 'autoprueba-reenvio-que-no-solo-delega=falla'],
     'PushBackEditAllGuardTests', 'Core', True),
    ('14', '**La FVM llama al puerto en su orden** (I-20), sobre el doble de CT-02: con texto pendiente, `CommitField` y después `TryEditAll`; sin '
           'él, solo `TryEditAll`; la petición de D-PBT4-12; y cuándo se ofrece el botón',
     ['con-texto llamadas=[GetField,CommitField,TryEditAll,GetField] cambiado=1',
      'sin-texto llamadas=[GetField,TryEditAll,GetField] cambiado=1',
      'peticion sesion-del-campo=True rack-del-campo=True referencia=null testigo=null',
      'boton sin-borrador=True editable=True no-editable=False vista-previa=False'],
     'PushBackEditAllWindowTests', 'UI', True),
    ('15', '**I-24:** G-L1 y la pureza sobre los dos archivos nuevos de `Draft/` de PB-T4c: `PushBackFullDraftLaunch.cs` y '
           '`PushBackFullDraftStatePort.cs` (§5.0; como CT-PBT4B-19). **Cubre `ApplyToDraft`**',
     ['g-l1 archivos=2 reglas-sin-coincidencias=True'] + ['autoprueba-%s=falla' % r for r in GL1]
     + ['pureza archivos=2 violaciones=[]', 'autoprueba-#nullable=falla', 'autoprueba-namespace RackCad.Application.Systems.PushBack.Draft=falla']
     + ['autoprueba-%s=falla' % t for t in PURITY] + ['cubre ApplyToDraft=PushBackFullDraftStatePort.cs'],
     'PushBackDraftLaunchGuardTests', 'Core', False),
]

CLASSES = {}
for _n, _what, _lines, _cls, _proj, _red in CT:
    CLASSES.setdefault((_cls, _proj, _red), []).append(_n)
    for _l in _lines:
        assert '`' not in _l and '|' not in _l and '…' not in _l, (_n, _l)
assert len(CT) == 15 and [c[0] for c in CT] == ['%02d' % k for k in range(1, 16)]
assert len(CT[6][2]) == 54 and len(CT[14][2]) == 33
CORE_TRUE = sum(1 for c in CT if c[4] == 'Core' and c[5])
CORE_FALSE = sum(1 for c in CT if c[4] == 'Core' and not c[5])
UI_TRUE = sum(1 for c in CT if c[4] == 'UI' and c[5])
assert (CORE_TRUE, CORE_FALSE, UI_TRUE) == (9, 2, 4)
