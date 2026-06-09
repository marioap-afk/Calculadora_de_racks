using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using RackCad.Domain.RackFrames;

namespace RackCad.UI
{
    public sealed class BracingSegmentEditorRow : ObservableObject
    {
        private const double Tolerance = 0.0001;

        private readonly RackFrameConfiguratorViewModel owner;
        private readonly BracingPanel panel;
        private readonly string standardLowerHorizontalId;
        private readonly string standardUpperHorizontalId;
        private readonly BracingPattern standardPattern;
        private readonly FrameSide standardSideMode;
        private readonly DiagonalDirection standardDiagonalDirection;
        private readonly string standardBraceProfileId;
        private readonly string standardStartConnectionPointId;
        private readonly string standardEndConnectionPointId;
        private readonly bool isStandardPanel;

        public BracingSegmentEditorRow(RackFrameConfiguratorViewModel owner, BracingPanel panel, bool isStandardPanel = true)
        {
            this.owner = owner;
            this.panel = panel;
            this.isStandardPanel = isStandardPanel;
            standardLowerHorizontalId = panel.LowerHorizontalId;
            standardUpperHorizontalId = panel.UpperHorizontalId;
            standardPattern = panel.Arrangement;
            standardSideMode = panel.MountingFace;
            standardDiagonalDirection = panel.DiagonalDirection;
            standardBraceProfileId = panel.DiagonalProfileId;
            standardStartConnectionPointId = panel.StartConnectionPointId;
            standardEndConnectionPointId = panel.EndConnectionPointId;
        }

        public int Index => panel.Number;

        public bool IsStandardSegment => isStandardPanel;

        internal BracingPanel DomainPanel => panel;

        public double StartElevation => panel.StartElevation;

        public double EndElevation => panel.EndElevation;

        public double ClearHeight => Math.Max(0.0, panel.ClearHeight);

        public string ClearHeightText
        {
            get => FormatEditableNumber(ClearHeight);
            set => OnPropertyChanged();
        }

        public string LowerHorizontalId
        {
            get => panel.LowerHorizontalId;
            set
            {
                var normalizedValue = NormalizeText(value);

                if (panel.LowerHorizontalId == normalizedValue)
                {
                    return;
                }

                panel.LowerHorizontalId = normalizedValue;
                OnPropertyChanged();
                NotifyEdited();
            }
        }

        public string UpperHorizontalId
        {
            get => panel.UpperHorizontalId;
            set
            {
                var normalizedValue = NormalizeText(value);

                if (panel.UpperHorizontalId == normalizedValue)
                {
                    return;
                }

                panel.UpperHorizontalId = normalizedValue;
                OnPropertyChanged();
                NotifyEdited();
            }
        }

        public BracingPattern Pattern
        {
            get => panel.Arrangement;
            set
            {
                if (panel.Arrangement == value)
                {
                    return;
                }

                panel.Arrangement = value;
                OnPropertyChanged();
                NotifyEdited();
            }
        }

        public FrameSide SideMode
        {
            get => panel.MountingFace;
            set
            {
                if (panel.MountingFace == value)
                {
                    return;
                }

                panel.MountingFace = value;
                OnPropertyChanged();
                NotifyEdited();
            }
        }

        public DiagonalDirection DiagonalDirection
        {
            get => panel.DiagonalDirection;
            set
            {
                if (panel.DiagonalDirection == value)
                {
                    return;
                }

                panel.DiagonalDirection = value;
                OnPropertyChanged();
                NotifyEdited();
            }
        }

        public string BraceProfileId
        {
            get => panel.DiagonalProfileId;
            set
            {
                var normalizedValue = NormalizeText(value);

                if (panel.DiagonalProfileId == normalizedValue)
                {
                    return;
                }

                panel.DiagonalProfileId = normalizedValue;
                OnPropertyChanged();
                NotifyEdited();
            }
        }

        public string StartConnectionPointId
        {
            get => panel.StartConnectionPointId;
            set
            {
                var normalizedValue = NormalizeText(value);

                if (panel.StartConnectionPointId == normalizedValue)
                {
                    return;
                }

                panel.StartConnectionPointId = normalizedValue;
                OnPropertyChanged();
                NotifyEdited();
            }
        }

        public string EndConnectionPointId
        {
            get => panel.EndConnectionPointId;
            set
            {
                var normalizedValue = NormalizeText(value);

                if (panel.EndConnectionPointId == normalizedValue)
                {
                    return;
                }

                panel.EndConnectionPointId = normalizedValue;
                OnPropertyChanged();
                NotifyEdited();
            }
        }

        public bool IsModified =>
            !isStandardPanel ||
            LowerHorizontalId != standardLowerHorizontalId ||
            UpperHorizontalId != standardUpperHorizontalId ||
            Pattern != standardPattern ||
            SideMode != standardSideMode ||
            DiagonalDirection != standardDiagonalDirection ||
            BraceProfileId != standardBraceProfileId ||
            StartConnectionPointId != standardStartConnectionPointId ||
            EndConnectionPointId != standardEndConnectionPointId;

        public string StatusLabel => IsModified ? "Modificado" : "Estandar";

        public string StatusBrush => IsModified ? "#B7791F" : "#2F855A";

        public void RestoreStandard()
        {
            panel.LowerHorizontalId = standardLowerHorizontalId;
            panel.UpperHorizontalId = standardUpperHorizontalId;
            panel.Arrangement = standardPattern;
            panel.MountingFace = standardSideMode;
            panel.DiagonalDirection = standardDiagonalDirection;
            panel.DiagonalProfileId = standardBraceProfileId;
            panel.StartConnectionPointId = standardStartConnectionPointId;
            panel.EndConnectionPointId = standardEndConnectionPointId;

            OnPropertyChanged(nameof(LowerHorizontalId));
            OnPropertyChanged(nameof(UpperHorizontalId));
            OnPropertyChanged(nameof(Pattern));
            OnPropertyChanged(nameof(SideMode));
            OnPropertyChanged(nameof(DiagonalDirection));
            OnPropertyChanged(nameof(BraceProfileId));
            OnPropertyChanged(nameof(StartConnectionPointId));
            OnPropertyChanged(nameof(EndConnectionPointId));
            NotifyEdited();
        }

        internal void RefreshDerivedValues()
        {
            OnPropertyChanged(nameof(Index));
            OnPropertyChanged(nameof(LowerHorizontalId));
            OnPropertyChanged(nameof(UpperHorizontalId));
            OnPropertyChanged(nameof(StartElevation));
            OnPropertyChanged(nameof(EndElevation));
            OnPropertyChanged(nameof(ClearHeight));
            OnPropertyChanged(nameof(ClearHeightText));
            RefreshModificationState();
        }

        internal void AddExceptionsTo(ICollection<FrameExceptionOverride> domainExceptions, ObservableCollection<FrameExceptionEditorRow> visualExceptions)
        {
            var targetId = "Panel " + Index.ToString(CultureInfo.InvariantCulture);

            if (!isStandardPanel)
            {
                AddException(domainExceptions, visualExceptions, targetId, "Panel agregado", ExceptionType.SegmentAdded, "No existe", FormatNumber(ClearHeight));
            }

            if (LowerHorizontalId != standardLowerHorizontalId)
            {
                AddException(domainExceptions, visualExceptions, targetId, "Horizontal inferior", ExceptionType.PatternChange, standardLowerHorizontalId, LowerHorizontalId);
            }

            if (UpperHorizontalId != standardUpperHorizontalId)
            {
                AddException(domainExceptions, visualExceptions, targetId, "Horizontal superior", ExceptionType.PatternChange, standardUpperHorizontalId, UpperHorizontalId);
            }

            if (Pattern != standardPattern)
            {
                AddException(domainExceptions, visualExceptions, targetId, "Arreglo de panel", GetPatternExceptionType(Pattern), standardPattern.ToString(), Pattern.ToString());
            }

            if (SideMode != standardSideMode)
            {
                AddException(domainExceptions, visualExceptions, targetId, "Cara de montaje", ExceptionType.SideChange, standardSideMode.ToString(), SideMode.ToString());
            }

            if (DiagonalDirection != standardDiagonalDirection)
            {
                AddException(domainExceptions, visualExceptions, targetId, "Direccion diagonal", ExceptionType.PatternChange, standardDiagonalDirection.ToString(), DiagonalDirection.ToString());
            }

            if (BraceProfileId != standardBraceProfileId)
            {
                AddException(domainExceptions, visualExceptions, targetId, "Perfil diagonal", ExceptionType.ProfileChange, standardBraceProfileId, BraceProfileId);
            }

            if (StartConnectionPointId != standardStartConnectionPointId)
            {
                AddException(domainExceptions, visualExceptions, targetId, "Punto inicial", ExceptionType.ConnectionPointChange, standardStartConnectionPointId, StartConnectionPointId);
            }

            if (EndConnectionPointId != standardEndConnectionPointId)
            {
                AddException(domainExceptions, visualExceptions, targetId, "Punto final", ExceptionType.ConnectionPointChange, standardEndConnectionPointId, EndConnectionPointId);
            }
        }

        internal void RefreshModificationState()
        {
            panel.IsException = IsModified;
            OnPropertyChanged(nameof(IsModified));
            OnPropertyChanged(nameof(StatusLabel));
            OnPropertyChanged(nameof(StatusBrush));
        }

        private void NotifyEdited()
        {
            RefreshModificationState();
            owner.SegmentWasEdited(this);
        }

        private static void AddException(ICollection<FrameExceptionOverride> domainExceptions, ObservableCollection<FrameExceptionEditorRow> visualExceptions, string targetId, string fieldName, ExceptionType exceptionType, string standardValue, string overrideValue)
        {
            domainExceptions.Add(new FrameExceptionOverride
            {
                ExceptionType = exceptionType,
                TargetId = targetId,
                StandardValue = standardValue,
                OverrideValue = overrideValue,
                Reason = "Cambio manual desde configurador MVP"
            });

            visualExceptions.Add(new FrameExceptionEditorRow(targetId, fieldName, exceptionType, standardValue, overrideValue));
        }

        private static ExceptionType GetPatternExceptionType(BracingPattern pattern)
        {
            if (pattern == BracingPattern.NoBracing)
            {
                return ExceptionType.NoBracing;
            }

            if (pattern == BracingPattern.DoubleDiagonal)
            {
                return ExceptionType.DoubleBracing;
            }

            return ExceptionType.PatternChange;
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture) + " in";
        }

        private static string FormatEditableNumber(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string NormalizeText(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }
}
