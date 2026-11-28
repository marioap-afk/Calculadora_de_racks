namespace RackCad.UI.Systems.PushBack
{
    /// <summary>
    /// I-64 F4B PB-T2b - el modo de la ventana de Push Back (D-PBT2-04). <see cref="Classic"/> es la ventana de siempre, la de
    /// RACKEDITAR, RACKPUSHBACK y el menu; <see cref="Workspace"/> solo se obtiene por la factoria
    /// <c>RackPushBackSystemWindow.ForDraft</c>. El modo no es un booleano ni se cambia: lo fija el constructor.
    /// </summary>
    public enum PushBackEditorMode
    {
        Classic = 0,
        Workspace = 1
    }
}
