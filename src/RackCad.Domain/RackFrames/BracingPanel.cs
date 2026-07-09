using System.Collections.Generic;

namespace RackCad.Domain.RackFrames
{
    public sealed class BracingPanel
    {
        public BracingPanel()
        {
            Members = new List<FrameMember>();
            DiagonalDirection = DiagonalDirection.AutoAlternating;
        }

        public string PanelId { get; set; }
        public int Number { get; set; }
        public int Index
        {
            get => Number;
            set => Number = value;
        }
        public string LowerHorizontalId { get; set; }
        public string UpperHorizontalId { get; set; }
        public double StartElevation { get; set; }
        public double EndElevation { get; set; }
        public double ClearHeight => EndElevation - StartElevation;
        public BracingPattern Arrangement { get; set; }
        public FrameSide MountingFace { get; set; }
        public string DiagonalProfileId { get; set; }
        public DiagonalDirection DiagonalDirection { get; set; }
        public string StartConnectionPointId { get; set; }
        public string EndConnectionPointId { get; set; }
        public bool IsStandard { get; set; }
        public bool IsException { get; set; }
        public IList<FrameMember> Members { get; private set; }

        /// <summary>
        /// The direction this panel's diagonal actually runs: the explicit one when set, otherwise
        /// alternating per panel number (odd rises left→right). ONE rule shared by the configurator
        /// preview and the drawn lateral — duplicated copies once let them alternate in opposite senses.
        /// </summary>
        public DiagonalDirection ResolveDiagonalDirection()
        {
            if (DiagonalDirection == DiagonalDirection.UpRight || DiagonalDirection == DiagonalDirection.UpLeft)
            {
                return DiagonalDirection;
            }

            return Number % 2 == 0 ? DiagonalDirection.UpLeft : DiagonalDirection.UpRight;
        }
    }
}
