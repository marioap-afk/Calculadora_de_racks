using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using I52Ct21d.HostFacts.Core;
using I52Ct21d.HostFacts.Rs.Core;

namespace I52Ct21d.HostFacts.Rs;

/// <summary>
/// THE only type of the RS DLL that calls a write API of the host (Owner Q-O-1: disposable side databases used only by the instrument). Every
/// <c>new Database(...)</c> of the RS DLL is here, and every database it writes is one it created itself with that constructor (the constructor
/// always has <c>noDocument = true</c>: never attached to a document, never registered as a governed document). It never touches
/// <c>HostApplicationServices.WorkingDatabase</c>, never a document's database, never opens a document. The scan allows the write members
/// (object creation, <c>AppendEntity</c>, the property setters, <c>Commit</c>, <c>OpenMode.ForWrite</c>, <c>WblockCloneObjects</c>, <c>ReadDwgFile</c>)
/// ONLY inside this type, pins its surface and its callers, and requires the guard check before every <c>ReadDwgFile</c>.
///
/// Every host behaviour below that the repository cannot establish is marked HOST-TO-CONFIRM; the raw output of the exact build governs.
/// </summary>
internal sealed class SideDbWriter
{
    internal const string TolScaleTarget = "CT21D_TOLSCALE_TARGET";

    private readonly SideDbLedger _ledger;
    private readonly ScratchRootGuard _guard;

    internal SideDbWriter(SideDbLedger ledger, ScratchRootGuard guard)
    {
        _ledger = ledger;
        _guard = guard;
    }

    // ---- creating and opening side databases ------------------------------------------------------------------------------------
    /// <summary>A disposable scratch database with the default drawing (model space, standard tables). HOST-TO-CONFIRM: <c>new Database(true, true)</c> in a plugin.</summary>
    internal SideDbHandle CreateScratch(string purpose)
    {
        var db = new Database(true, true);
        var id = _ledger.Register("SCRATCH_WRITE", "new Database(true, true)", "NONE", purpose);
        return new SideDbHandle(db, id, _ledger);
    }

    /// <summary>
    /// A side database filled by <c>ReadDwgFile</c> from a source the guard authorized: the designated private copy (never the library) or a
    /// scratch file of this run's ledger (the OP2 reopen). The guard re-checks the source right before the read.
    /// </summary>
    internal SideDbHandle OpenForRead(ReadableSource source)
    {
        var role = source.Kind == ReadableKind.PrivateCopy ? "PRIVATE_COPY_READ" : "SAVED_REOPEN_READ";
        _guard.AssertReadable(source); // straight-line from here to ReadDwgFile (the scan requires it): no branch between the check and the read
        var db = new Database(false, true);
        var id = _ledger.Register(role, "new Database(false, true)", "ReadDwgFile", Path.GetFileName(source.FullPath));
        var handle = new SideDbHandle(db, id, _ledger);
        try
        {
            // HOST-TO-CONFIRM: ReadDwgFile(string, FileShare, bool, string) and CloseInput(true) releasing the input file.
            db.ReadDwgFile(source.FullPath, FileShare.Read, true, "");
            db.CloseInput(true);
        }
        catch (System.Exception)
        {
            handle.Dispose();
            throw;
        }
        return handle;
    }

    // ---- U-RS-3: TOL_SCALE ----------------------------------------------------------------------------------------------------------
    /// <summary>Design 2.4 S2: the target block <c>CT21D_TOLSCALE_TARGET</c> with one line (a plain, non-dynamic, non-anonymous target).</summary>
    internal void BuildTolScaleTarget(SideDbHandle side)
    {
        var db = side.Database;
        using var tr = db.TransactionManager.StartTransaction();
        var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite);
        var record = new BlockTableRecord { Name = TolScaleTarget };
        blockTable.Add(record);
        tr.AddNewlyCreatedDBObject(record, true);
        var line = new Line(new Point3d(0.0, 0.0, 0.0), new Point3d(1.0, 0.0, 0.0));
        record.AppendEntity(line);
        tr.AddNewlyCreatedDBObject(line, true);
        tr.Commit();
    }

    /// <summary>
    /// Design 2.4 S3 (T1): one <c>new BlockReference(point, targetId)</c> per row with <c>ScaleFactors = new Scale3d(x, y, z)</c> (the product's own
    /// construction and assignment pattern, V17 2039), appended to model space; a row the host rejects is recorded with the error text. COMMITS.
    /// </summary>
    internal IReadOnlyList<RowConstruction> AppendScaledReferences(SideDbHandle side, IReadOnlyList<ProbeRow> rows)
    {
        var db = side.Database;
        var result = new List<RowConstruction>();
        using var tr = db.TransactionManager.StartTransaction();
        var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var targetId = blockTable[TolScaleTarget];
        var modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                var s = TolScaleRowMath.Values(row.Intended);
                var reference = new BlockReference(new Point3d(100.0 * i, 0.0, 0.0), targetId);
                reference.ScaleFactors = new Scale3d(s[0], s[1], s[2]);
                modelSpace.AppendEntity(reference);
                tr.AddNewlyCreatedDBObject(reference, true);
                result.Add(new RowConstruction(row.Id, reference.Handle.ToString(), null));
            }
            catch (System.Exception ex)
            {
                result.Add(new RowConstruction(row.Id, null, ex.GetType().Name + ":" + ex.Message));
            }
        }
        tr.Commit();
        return result;
    }

    // ---- U-RS-2: dimension write-back -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The product's pattern (<c>LateralHeaderDrawer.AppendDimension</c>): a <c>RotatedDimension</c> of the drawing's current style appended to
    /// model space, its variables set by the property setters, then <c>RecomputeDimensionBlock(true)</c>. COMMITS. A scenario the host rejects is recorded.
    /// </summary>
    internal IReadOnlyList<ScenarioConstruction> AppendDimensionScenarios(SideDbHandle side, IReadOnlyList<WriteBackScenario> scenarios)
    {
        var db = side.Database;
        var result = new List<ScenarioConstruction>();
        using var tr = db.TransactionManager.StartTransaction();
        var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
        for (var i = 0; i < scenarios.Count; i++)
        {
            var scenario = scenarios[i];
            try
            {
                var x = 50.0 * i;
                var dimension = new RotatedDimension(0.0, new Point3d(x, 0.0, 0.0), new Point3d(x + 10.0, 0.0, 0.0), new Point3d(x + 5.0, 5.0, 0.0), string.Empty, db.Dimstyle);
                modelSpace.AppendEntity(dimension);
                tr.AddNewlyCreatedDBObject(dimension, true);
                foreach (var a in scenario.Assignments) Assign(dimension, a);
                dimension.RecomputeDimensionBlock(true);
                result.Add(new ScenarioConstruction(scenario.Id, dimension.Handle.ToString(), null));
            }
            catch (System.Exception ex)
            {
                result.Add(new ScenarioConstruction(scenario.Id, null, ex.GetType().Name + ":" + ex.Message));
            }
        }
        tr.Commit();
        return result;
    }

    private static void Assign(RotatedDimension dimension, OverrideAssignment a)
    {
        switch (a.Variable)
        {
            case "DIMSCALE": dimension.Dimscale = a.Value; break;
            case "DIMTXT": dimension.Dimtxt = a.Value; break;
            case "DIMASZ": dimension.Dimasz = a.Value; break;
            case "DIMEXE": dimension.Dimexe = a.Value; break;
            case "DIMEXO": dimension.Dimexo = a.Value; break;
            case "DIMGAP": dimension.Dimgap = a.Value; break;
            case "DIMTAD": dimension.Dimtad = (int)a.Value; break;
            case "DIMDEC": dimension.Dimdec = (int)a.Value; break;
            default: throw new ArgumentException("not a product dimension variable: " + a.Variable);
        }
    }

    // ---- U-RS-1: dynamic-property census ----------------------------------------------------------------------------------------------
    /// <summary>
    /// BA-05 V5 section 5 step 2: CLONES the definition of <paramref name="blockName"/> from the side database filled from the private copy into the
    /// scratch side database (the destination is the block table of the scratch database). The private copy is only the source.
    /// HOST-TO-CONFIRM: <c>WblockCloneObjects</c> of a dynamic block definition (its extension dictionary) into a database made with <c>new Database(true, true)</c>.
    /// </summary>
    internal void CloneDefinition(SideDbHandle source, SideDbHandle scratch, string blockName)
    {
        ObjectId definition;
        using (var tr = source.Database.TransactionManager.StartOpenCloseTransaction())
        {
            var blockTable = (BlockTable)tr.GetObject(source.Database.BlockTableId, OpenMode.ForRead);
            definition = blockTable[blockName];
        }
        var ids = new ObjectIdCollection();
        ids.Add(definition);
        source.Database.WblockCloneObjects(ids, scratch.Database.BlockTableId, new IdMapping(), DuplicateRecordCloning.Ignore, false);
    }

    /// <summary>Inserts a reference to the cloned definition in the scratch side database and COMMITS. Returns its handle.</summary>
    internal string InsertReference(SideDbHandle scratch, string blockName)
    {
        var db = scratch.Database;
        using var tr = db.TransactionManager.StartTransaction();
        var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var definition = blockTable[blockName];
        var modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
        var reference = new BlockReference(Point3d.Origin, definition);
        modelSpace.AppendEntity(reference);
        tr.AddNewlyCreatedDBObject(reference, true);
        tr.Commit();
        return reference.Handle.ToString();
    }
}
