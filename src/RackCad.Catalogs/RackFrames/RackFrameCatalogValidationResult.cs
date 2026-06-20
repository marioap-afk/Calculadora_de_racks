using System.Collections.Generic;
using System.Linq;

namespace RackCad.Catalogs.RackFrames;

public sealed class RackFrameCatalogValidationResult
{
    public RackFrameCatalogValidationResult(IEnumerable<string> errors)
    {
        Errors = errors.ToList();
    }

    public IList<string> Errors { get; }

    public bool IsValid => Errors.Count == 0;
}
