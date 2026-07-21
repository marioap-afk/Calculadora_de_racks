using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RackCad.Application.Catalogs.Validation
{
    /// <summary>
    /// The single diagnostic the catalog validator produces (idea-futuras #14): every issue found in one
    /// pass, plus the roll-ups a caller needs — is it valid, is it valid in STRICT mode (deployments treat a
    /// warning as fatal), how many of each, and a formatted report for the command line or a dialog.
    /// </summary>
    public sealed class CatalogValidationReport
    {
        private readonly List<CatalogValidationIssue> _issues = new List<CatalogValidationIssue>();

        public IReadOnlyList<CatalogValidationIssue> Issues => _issues;

        public void Add(CatalogValidationIssue issue)
        {
            if (issue != null)
            {
                _issues.Add(issue);
            }
        }

        public void AddRange(IEnumerable<CatalogValidationIssue> issues)
        {
            if (issues == null)
            {
                return;
            }

            foreach (var issue in issues)
            {
                Add(issue);
            }
        }

        public IEnumerable<CatalogValidationIssue> Errors =>
            _issues.Where(issue => issue.Severity == CatalogValidationSeverity.Error);

        public IEnumerable<CatalogValidationIssue> Warnings =>
            _issues.Where(issue => issue.Severity == CatalogValidationSeverity.Warning);

        public IEnumerable<CatalogValidationIssue> Infos =>
            _issues.Where(issue => issue.Severity == CatalogValidationSeverity.Info);

        public int ErrorCount => _issues.Count(issue => issue.Severity == CatalogValidationSeverity.Error);

        public int WarningCount => _issues.Count(issue => issue.Severity == CatalogValidationSeverity.Warning);

        public int InfoCount => _issues.Count(issue => issue.Severity == CatalogValidationSeverity.Info);

        public bool HasErrors => _issues.Any(issue => issue.Severity == CatalogValidationSeverity.Error);

        public bool HasWarnings => _issues.Any(issue => issue.Severity == CatalogValidationSeverity.Warning);

        /// <summary>
        /// Valid when there are no errors. In <paramref name="strict"/> mode a warning is also fatal, so a
        /// silently dropped row or a library missing an expected parameter blocks a deployment.
        /// </summary>
        public bool IsValid(bool strict = false) =>
            strict
                ? _issues.All(issue => issue.Severity < CatalogValidationSeverity.Warning)
                : !HasErrors;

        /// <summary>Worst severity present; <see cref="CatalogValidationSeverity.Info"/> when empty.</summary>
        public CatalogValidationSeverity MaxSeverity =>
            _issues.Count == 0 ? CatalogValidationSeverity.Info : _issues.Max(issue => issue.Severity);

        public int Count(CatalogValidationCategory category) =>
            _issues.Count(issue => issue.Category == category);

        /// <summary>Every issue with a given stable code (tests and UI filters use this, never the message).</summary>
        public IEnumerable<CatalogValidationIssue> WithCode(string code) =>
            _issues.Where(issue => string.Equals(issue.Code, code, StringComparison.Ordinal));

        /// <summary>The consolidated, human-readable diagnostic: worst issues first, deterministically ordered.</summary>
        public string Format()
        {
            if (_issues.Count == 0)
            {
                return "Catálogo válido: sin incidencias.";
            }

            var builder = new StringBuilder();
            builder
                .Append("Validación de catálogo: ")
                .Append(ErrorCount).Append(" error(es), ")
                .Append(WarningCount).Append(" advertencia(s), ")
                .Append(InfoCount).AppendLine(" informativa(s).");

            foreach (var issue in _issues
                .OrderByDescending(issue => issue.Severity)
                .ThenBy(issue => issue.Category)
                .ThenBy(issue => issue.Code, StringComparer.Ordinal)
                .ThenBy(issue => issue.Location, StringComparer.Ordinal))
            {
                builder.Append("- ").AppendLine(issue.ToString());
            }

            return builder.ToString();
        }
    }
}
