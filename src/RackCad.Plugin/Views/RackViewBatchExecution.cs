using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Batch;
using RackCad.Application.Views.Preparation;

namespace RackCad.Plugin.Views
{
    internal static class RackViewBatchExecution
    {
        internal static RackViewBatchPlan<RackPreparedProductView<TPayload>> Run<TAuthored, TResolved, TPayload>(
            Document document,
            RackViewBatchProductSession<TAuthored, TResolved, TPayload> products,
            RackViewBatchRequest request,
            Func<IReadOnlyList<RackPreparedProductView<TPayload>>, RackViewBatchRedrawResult> redraw = null)
        {
            var driver = new RackViewBatchDriver<RackPreparedProductView<TPayload>>(
                document.Database.TransactionManager,
                products.Prepare,
                _ => RackViewBatchGateResult.Accept(),
                redraw ?? (_ => RackViewBatchRedrawResult.NotRequired()),
                (_, product) => products.Place(product));
            var result = driver.Execute(request);
            if (result.Report.PlacedCount > 0) document.Editor.Regen();
            document.Editor.WriteMessage("\nRackCad ID18: " + result.Outcome
                + " (" + result.Report.PlacedCount + "/" + result.Report.RequestedCount + ")."
                + (string.IsNullOrWhiteSpace(result.Report.Diagnostic)
                    ? string.Empty : " " + result.Report.Diagnostic));
            return result;
        }
    }
}
