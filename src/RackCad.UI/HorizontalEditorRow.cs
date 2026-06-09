using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using RackCad.Domain.RackFrames;

namespace RackCad.UI
{
    public sealed class HorizontalEditorRow : ObservableObject
    {
        private const double Tolerance = 0.0001;

        private readonly RackFrameConfiguratorViewModel owner;
        private readonly FrameHorizontal horizontal;
        private readonly double standardElevation;
        private readonly string standardProfileId;
        private readonly int standardQuantity;
        private readonly FrameSide standardMountingFace;
        private readonly FrameComponentState standardState;
        private readonly string standardNotes;
        private readonly bool isStandardHorizontal;

        public HorizontalEditorRow(RackFrameConfiguratorViewModel owner, FrameHorizontal horizontal, bool isStandardHorizontal = true)
        {
            this.owner = owner;
            this.horizontal = horizontal;
            this.isStandardHorizontal = isStandardHorizontal;
            standardElevation = horizontal.Elevation;
            standardProfileId = horizontal.ProfileId;
            standardQuantity = horizontal.Quantity;
            standardMountingFace = horizontal.MountingFace;
            standardState = horizontal.State;
            standardNotes = horizontal.Notes;
        }

        public string Id => horizontal.Id;

        public int Number => horizontal.Number;

        public string Label => Id;

        public double Elevation
        {
            get => horizontal.Elevation;
            set
            {
                var normalizedValue = Math.Max(0.0, value);

                if (AreClose(horizontal.Elevation, normalizedValue))
                {
                    return;
                }

                horizontal.Elevation = normalizedValue;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ElevationText));
                NotifyEdited();
            }
        }

        public string ElevationText
        {
            get => FormatEditableNumber(horizontal.Elevation);
            set
            {
                if (TryParseDimension(value, out var parsedValue))
                {
                    Elevation = parsedValue;
                }

                OnPropertyChanged();
            }
        }

        public string ProfileId
        {
            get => horizontal.ProfileId;
            set
            {
                var normalizedValue = NormalizeText(value);

                if (horizontal.ProfileId == normalizedValue)
                {
                    return;
                }

                horizontal.ProfileId = normalizedValue;
                OnPropertyChanged();
                NotifyEdited();
            }
        }

        public int Quantity
        {
            get => horizontal.Quantity;
            set
            {
                var normalizedValue = Math.Max(1, value);

                if (horizontal.Quantity == normalizedValue)
                {
                    return;
                }

                horizontal.Quantity = normalizedValue;
                OnPropertyChanged();
                NotifyEdited();
            }
        }

        public FrameSide MountingFace
        {
            get => horizontal.MountingFace;
            set
            {
                if (horizontal.MountingFace == value)
                {
                    return;
                }

                horizontal.MountingFace = value;
                OnPropertyChanged();
                NotifyEdited();
            }
        }

        public FrameComponentState State
        {
            get => horizontal.State;
            set
            {
                if (horizontal.State == value)
                {
                    return;
                }

                horizontal.State = value;
                OnPropertyChanged();
                NotifyEdited();
            }
        }

        public string Notes
        {
            get => horizontal.Notes;
            set
            {
                var normalizedValue = NormalizeText(value);

                if (horizontal.Notes == normalizedValue)
                {
                    return;
                }

                horizontal.Notes = normalizedValue;
                OnPropertyChanged();
                NotifyEdited();
            }
        }

        public bool IsModified =>
            !isStandardHorizontal ||
            !AreClose(Elevation, standardElevation) ||
            ProfileId != standardProfileId ||
            Quantity != standardQuantity ||
            MountingFace != standardMountingFace ||
            State != standardState ||
            Notes != standardNotes;

        public string StatusLabel => IsModified ? "Modificado" : "Estandar";

        public string StatusBrush => IsModified ? "#B7791F" : "#2F855A";

        internal FrameHorizontal DomainHorizontal => horizontal;

        internal void SetNumber(int number)
        {
            horizontal.Number = number;
            OnPropertyChanged(nameof(Number));
            OnPropertyChanged(nameof(Label));
        }

        internal void RefreshModificationState()
        {
            horizontal.IsStandard = !IsModified;
            OnPropertyChanged(nameof(IsModified));
            OnPropertyChanged(nameof(StatusLabel));
            OnPropertyChanged(nameof(StatusBrush));
        }

        internal void AddExceptionsTo(ICollection<FrameExceptionOverride> domainExceptions, ObservableCollection<FrameExceptionEditorRow> visualExceptions)
        {
            var targetId = Label;

            if (!isStandardHorizontal)
            {
                AddException(domainExceptions, visualExceptions, targetId, "Horizontal agregada", ExceptionType.HorizontalChange, "No existe", FormatNumber(Elevation));
            }

            if (!AreClose(Elevation, standardElevation))
            {
                AddException(domainExceptions, visualExceptions, targetId, "Elevacion", ExceptionType.HorizontalChange, FormatNumber(standardElevation), FormatNumber(Elevation));
            }

            if (ProfileId != standardProfileId)
            {
                AddException(domainExceptions, visualExceptions, targetId, "Perfil", ExceptionType.ProfileChange, standardProfileId, ProfileId);
            }

            if (Quantity != standardQuantity)
            {
                AddException(domainExceptions, visualExceptions, targetId, "Cantidad", ExceptionType.HorizontalChange, standardQuantity.ToString(CultureInfo.InvariantCulture), Quantity.ToString(CultureInfo.InvariantCulture));
            }

            if (MountingFace != standardMountingFace)
            {
                AddException(domainExceptions, visualExceptions, targetId, "Cara de montaje", ExceptionType.SideChange, standardMountingFace.ToString(), MountingFace.ToString());
            }

            if (State != standardState)
            {
                AddException(domainExceptions, visualExceptions, targetId, "Estado", ExceptionType.HorizontalChange, standardState.ToString(), State.ToString());
            }
        }

        private void NotifyEdited()
        {
            RefreshModificationState();
            owner.HorizontalWasEdited(this);
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

        private static bool TryParseDimension(string value, out double dimension)
        {
            dimension = 0.0;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalizedValue = value
                .Trim()
                .Replace("in", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("\"", string.Empty)
                .Trim();

            if (double.TryParse(normalizedValue, NumberStyles.Float, CultureInfo.CurrentCulture, out dimension) ||
                double.TryParse(normalizedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out dimension))
            {
                return dimension >= 0.0;
            }

            var invariantValue = normalizedValue.Replace(',', '.');

            if (double.TryParse(invariantValue, NumberStyles.Float, CultureInfo.InvariantCulture, out dimension))
            {
                return dimension >= 0.0;
            }

            return false;
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture) + " in";
        }

        private static string FormatEditableNumber(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static bool AreClose(double left, double right)
        {
            return Math.Abs(left - right) < Tolerance;
        }

        private static string NormalizeText(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }
}
