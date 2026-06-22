using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.Headers
{
    /// <summary>
    /// Builds a lateral header as a sequence of block insertions anchored on connection points — NOT a free
    /// visual composition. It renders the SAME model the editor/preview shows: it walks the configuration's
    /// horizontals (honouring quantity = double horizontal) and panels (honouring the bracing arrangement and
    /// the alternating diagonal direction), then applies the real lateral geometry. Every connection lands on
    /// the post's troquel grid (nominal editor elevations are snapped to the nearest troquel). A post may carry
    /// a reinforcement: a second post mated at FIN_POSTE (Y=0) with its own LONGITUD, covering the lower zone;
    /// celosía endpoints inside a reinforced zone attach to the reinforcement's inner troquel instead of the
    /// post's. Pure and unit-testable.
    ///
    /// Lateral-view axes: X = depth direction (between posts), Y = height.
    /// </summary>
    public sealed class LateralHeaderLayoutBuilder
    {
        private const double Tolerance = 1e-6;

        public LateralHeaderLayout Build(RackFrameConfiguration config, LateralHeaderParameters p, RackCatalog catalog)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (p == null) throw new ArgumentNullException(nameof(p));

            var view = p.View;
            var paso = p.PasoTroquel > 0 ? p.PasoTroquel : 2.0;
            var height = config.Height > 0 ? config.Height : p.Height;
            var depth = config.Depth > 0 ? config.Depth : p.Depth;

            if (height <= 0.0) throw new ArgumentOutOfRangeException(nameof(config), "La altura debe ser > 0.");
            if (depth <= 0.0) throw new ArgumentOutOfRangeException(nameof(config), "El fondo debe ser > 0.");

            var postId = FirstNonEmpty(config.LeftPost?.PostCatalogId, p.PostId);
            var plateId = FirstNonEmpty(config.LeftBasePlate?.PlateCatalogId, p.BasePlateId);

            var instances = new List<HeaderBlockInstance>();

            // 1 & 2. Left post + plate at X=0, right post + plate mirrored at X=Depth.
            var montaje = Local(catalog, plateId, p.MontajePostePoint, view);
            var postBlock = Block(catalog, postId, view);
            var plateBlock = Block(catalog, plateId, view);
            var leftOrigin = new Point2D(0.0, 0.0);
            var rightOrigin = new Point2D(depth, 0.0);
            AddPostWithPlate(instances, p, postId, plateId, postBlock, plateBlock, montaje, leftOrigin, height, mirrored: false);
            AddPostWithPlate(instances, p, postId, plateId, postBlock, plateBlock, montaje, rightOrigin, height, mirrored: true);

            // 3. Troquel grid from the post's TROQUEL_CELOSIA. Its Y is the base of the grid (everything snaps
            // to troquelBaseY + k*paso); its X gives each post's troquel line.
            var troquel = Local(catalog, postId, p.TroquelCelosiaPoint, view);
            var finPoste = Local(catalog, postId, p.FinPostePoint, view);
            var troquelBaseY = troquel.Y;
            var doubleStep = p.HorizontalDoubleOffsetTroqueles * paso;

            var leftPostTroquelX = leftOrigin.X + troquel.X;
            var rightPostTroquelX = rightOrigin.X - troquel.X;

            // Reinforcements (optional, per side): a second post mated at FIN_POSTE, drawn here, with its inner
            // troquel line used by celosía inside its zone.
            var leftReinf = AddReinforcement(instances, p, catalog, config.LeftPost, leftOrigin, finPoste, troquel, height, mirrored: false);
            var rightReinf = AddReinforcement(instances, p, catalog, config.RightPost, rightOrigin, finPoste, troquel, height, mirrored: true);

            double LeftXAt(double y) =>
                leftReinf.Enabled && y <= leftReinf.Height + Tolerance ? leftReinf.TroquelX : leftPostTroquelX;
            double RightXAt(double y) =>
                rightReinf.Enabled && y <= rightReinf.Height + Tolerance ? rightReinf.TroquelX : rightPostTroquelX;

            // 4 & 5. Horizontals: one travesaño per horizontal, snapped to the troquel grid; quantity > 1
            // stacks extra travesaños up by HorizontalDoubleOffsetTroqueles troqueles each (double horizontal).
            var horizontalsById = new Dictionary<string, FrameHorizontal>(StringComparer.OrdinalIgnoreCase);
            var orderedHorizontals = (config.Horizontals ?? new List<FrameHorizontal>())
                .OrderBy(horizontal => horizontal.Elevation)
                .ThenBy(horizontal => horizontal.Number)
                .ToList();

            var topHorizontalY = 0.0;
            foreach (var horizontal in orderedHorizontals)
            {
                if (!string.IsNullOrWhiteSpace(horizontal.Id) && !horizontalsById.ContainsKey(horizontal.Id))
                {
                    horizontalsById[horizontal.Id] = horizontal;
                }

                var trussId = FirstNonEmpty(horizontal.ProfileId, p.TrussProfileId);
                var celosia = Local(catalog, trussId, p.CelosiaPoint, view);
                var block = Block(catalog, trussId, view);
                var quantity = Math.Max(1, horizontal.Quantity);
                var baseY = SnapToTroquel(horizontal.Elevation, troquelBaseY, paso);

                for (var copy = 0; copy < quantity; copy++)
                {
                    var y = baseY + copy * doubleStep;
                    var leftX = LeftXAt(y);
                    var rightX = RightXAt(y);
                    topHorizontalY = Math.Max(topHorizontalY, y);
                    instances.Add(MakeMember(p, HeaderBlockRole.Horizontal, trussId, block, celosia,
                        new Point2D(leftX, y), rotation: 0.0, length: rightX - leftX));
                }
            }

            // 6. Diagonals: walk the panels, honouring arrangement and alternating direction. Each endpoint
            // takes the innermost available troquel (reinforcement where present, else the post).
            var diagonalCount = 0;
            foreach (var panel in (config.BracingPanels ?? new List<BracingPanel>()).OrderBy(panel => panel.Number))
            {
                if (!TryResolvePanel(panel, horizontalsById, troquelBaseY, paso, doubleStep, out var startBaseY, out var endBaseY))
                {
                    continue;
                }

                var trussId = FirstNonEmpty(panel.DiagonalProfileId, p.TrussProfileId);
                var celosia = Local(catalog, trussId, p.CelosiaPoint, view);
                var block = Block(catalog, trussId, view);

                foreach (var diagonal in MakeDiagonals(p, panel, trussId, block, celosia, startBaseY, endBaseY, LeftXAt, RightXAt))
                {
                    instances.Add(diagonal);
                    diagonalCount++;
                }
            }

            var horizontalLength = rightPostTroquelX - leftPostTroquelX;
            var closingGap = Math.Max(0.0, height - topHorizontalY);
            return new LateralHeaderLayout(instances, horizontalLength, orderedHorizontals.Count, diagonalCount, closingGap);
        }

        // ---- Steps 1 & 2: a post with its base plate ----
        private static void AddPostWithPlate(
            ICollection<HeaderBlockInstance> instances, LateralHeaderParameters p,
            string postId, string plateId, string postBlock, string plateBlock,
            Point2D montaje, Point2D postOrigin, double height, bool mirrored)
        {
            var sign = mirrored ? -1.0 : 1.0;

            // Plate first: insert it so its MONTAJE_POSTE coincides with the post origin.
            instances.Add(new HeaderBlockInstance
            {
                Role = HeaderBlockRole.BasePlate,
                PieceId = plateId,
                BlockName = plateBlock,
                View = p.View,
                MirroredX = mirrored,
                ConnectionAnchor = postOrigin,
                Insertion = new Point2D(postOrigin.X - sign * montaje.X, postOrigin.Y - montaje.Y)
            });

            // Post: its own origin is the reference; stretch its length parameter to the header height.
            var post = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Post,
                PieceId = postId,
                BlockName = postBlock,
                View = p.View,
                MirroredX = mirrored,
                ConnectionAnchor = postOrigin,
                Insertion = postOrigin
            };
            post.DynamicParameters[p.PostLengthParameter] = height;
            instances.Add(post);
        }

        // ---- Reinforcement: a second post mated at the post's FIN_POSTE ----
        private static ReinforcementZone AddReinforcement(
            ICollection<HeaderBlockInstance> instances, LateralHeaderParameters p, RackCatalog catalog,
            PostAssembly post, Point2D postOrigin, Point2D finPoste, Point2D postTroquel, double height, bool mirrored)
        {
            if (post == null || !post.HasReinforcement || post.ReinforcementHeight <= 0.0)
            {
                return ReinforcementZone.None;
            }

            var sign = mirrored ? -1.0 : 1.0;
            var reinforcementHeight = Math.Min(post.ReinforcementHeight, height);
            var reinforcementId = FirstNonEmpty(post.ReinforcementCatalogId, post.PostCatalogId, p.PostId);

            // The reinforcement is itself a post: its 0,0 mates onto the post's FIN_POSTE; its own troquel sits
            // one post-width further in, so the celosía in this zone attaches to that inner troquel.
            var reinforcementTroquel = Local(catalog, reinforcementId, p.TroquelCelosiaPoint, p.View);
            if (reinforcementTroquel.X == 0.0 && reinforcementTroquel.Y == 0.0)
            {
                reinforcementTroquel = postTroquel; // same post profile shares the troquel layout
            }

            var originX = postOrigin.X + sign * finPoste.X;
            var origin = new Point2D(originX, postOrigin.Y + finPoste.Y);

            var reinforcement = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Post,
                PieceId = reinforcementId,
                BlockName = Block(catalog, reinforcementId, p.View),
                View = p.View,
                MirroredX = mirrored,
                ConnectionAnchor = origin,
                Insertion = origin
            };
            reinforcement.DynamicParameters[p.PostLengthParameter] = reinforcementHeight;
            instances.Add(reinforcement);

            var troquelX = origin.X + sign * reinforcementTroquel.X;
            return new ReinforcementZone(true, reinforcementHeight, troquelX);
        }

        // ---- Step 6: resolve a panel's diagonal start/end reference Y on the troquel grid ----
        private static bool TryResolvePanel(
            BracingPanel panel, IReadOnlyDictionary<string, FrameHorizontal> horizontalsById,
            double troquelBaseY, double paso, double doubleStep, out double startBaseY, out double endBaseY)
        {
            startBaseY = 0.0;
            endBaseY = 0.0;

            if (panel == null ||
                !horizontalsById.TryGetValue(panel.LowerHorizontalId ?? string.Empty, out var lower) ||
                !horizontalsById.TryGetValue(panel.UpperHorizontalId ?? string.Empty, out var upper))
            {
                return false;
            }

            var bottom = lower.Elevation <= upper.Elevation ? lower : upper;
            var top = lower.Elevation <= upper.Elevation ? upper : lower;
            var bottomY = SnapToTroquel(bottom.Elevation, troquelBaseY, paso);
            var topY = SnapToTroquel(top.Elevation, troquelBaseY, paso);

            if (topY - bottomY <= Tolerance)
            {
                return false;
            }

            // The diagonal starts above the topmost travesaño of the bottom group (handles double horizontals).
            startBaseY = bottomY + (Math.Max(1, bottom.Quantity) - 1) * doubleStep;
            endBaseY = topY;
            return true;
        }

        private static IEnumerable<HeaderBlockInstance> MakeDiagonals(
            LateralHeaderParameters p, BracingPanel panel, string trussId, string block, Point2D celosia,
            double startBaseY, double endBaseY, Func<double, double> leftXAt, Func<double, double> rightXAt)
        {
            if (panel.Arrangement == BracingPattern.NoBracing || panel.Arrangement == BracingPattern.Custom)
            {
                yield break;
            }

            var paso = p.PasoTroquel > 0 ? p.PasoTroquel : 2.0;
            var startOffset = p.OffsetDiagonalInicioTroqueles * paso;
            var endOffset = p.OffsetDiagonalFinTroqueles * paso;
            var doubleSpacing = p.DiagonalDoubleSpacingTroqueles * paso;
            var direction = ResolveDirection(panel);

            if (panel.Arrangement == BracingPattern.DoubleDiagonal)
            {
                // Two parallel diagonals, one a troquel above the other (V-style celosía). The lower one keeps
                // the start offset and pushes its end up by the spacing; the upper one mirrors that.
                yield return MakeDiagonal(p, trussId, block, celosia,
                    startBaseY + startOffset, endBaseY - (endOffset + doubleSpacing), direction, leftXAt, rightXAt);
                yield return MakeDiagonal(p, trussId, block, celosia,
                    startBaseY + (startOffset + doubleSpacing), endBaseY - endOffset, direction, leftXAt, rightXAt);
                yield break;
            }

            if (panel.Arrangement == BracingPattern.XBracing)
            {
                yield return MakeDiagonal(p, trussId, block, celosia,
                    startBaseY + startOffset, endBaseY - endOffset, DiagonalDirection.UpRight, leftXAt, rightXAt);
                yield return MakeDiagonal(p, trussId, block, celosia,
                    startBaseY + startOffset, endBaseY - endOffset, DiagonalDirection.UpLeft, leftXAt, rightXAt);
                yield break;
            }

            // SingleDiagonal (and any other single-line arrangement): one diagonal in the panel's direction.
            yield return MakeDiagonal(p, trussId, block, celosia,
                startBaseY + startOffset, endBaseY - endOffset, direction, leftXAt, rightXAt);
        }

        /// <summary>Alternate the diagonal per panel when AutoAlternating; otherwise honour the explicit direction.</summary>
        private static DiagonalDirection ResolveDirection(BracingPanel panel)
        {
            if (panel.DiagonalDirection == DiagonalDirection.UpRight ||
                panel.DiagonalDirection == DiagonalDirection.UpLeft)
            {
                return panel.DiagonalDirection;
            }

            return panel.Number % 2 == 0 ? DiagonalDirection.UpLeft : DiagonalDirection.UpRight;
        }

        private static HeaderBlockInstance MakeDiagonal(
            LateralHeaderParameters p, string trussId, string block, Point2D celosia,
            double yStart, double yEnd, DiagonalDirection direction, Func<double, double> leftXAt, Func<double, double> rightXAt)
        {
            // UpRight rises left→right; UpLeft rises right→left. Each end takes the innermost troquel at its own
            // height, so a diagonal can start on a reinforcement and end on the plain post.
            var start = direction == DiagonalDirection.UpLeft
                ? new Point2D(rightXAt(yStart), yStart)
                : new Point2D(leftXAt(yStart), yStart);
            var end = direction == DiagonalDirection.UpLeft
                ? new Point2D(leftXAt(yEnd), yEnd)
                : new Point2D(rightXAt(yEnd), yEnd);

            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var length = Math.Sqrt(dx * dx + dy * dy);
            var rotation = Math.Atan2(dy, dx);

            var instance = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Diagonal,
                PieceId = trussId,
                BlockName = block,
                View = p.View,
                RotationRadians = rotation,
                ConnectionAnchor = start,
                Insertion = Insertion(start, celosia, rotation)
            };
            instance.DynamicParameters[p.MemberLengthParameter] = length;
            return instance;
        }

        // ---- Step 5: a horizontal travesaño ----
        private static HeaderBlockInstance MakeMember(
            LateralHeaderParameters p, HeaderBlockRole role, string pieceId, string block, Point2D celosia,
            Point2D connectionAnchor, double rotation, double length)
        {
            var instance = new HeaderBlockInstance
            {
                Role = role,
                PieceId = pieceId,
                BlockName = block,
                View = p.View,
                RotationRadians = rotation,
                ConnectionAnchor = connectionAnchor,
                Insertion = Insertion(connectionAnchor, celosia, rotation)
            };
            instance.DynamicParameters[p.MemberLengthParameter] = length;
            return instance;
        }

        // ---- Geometry helpers ----

        /// <summary>Snap a nominal elevation to the nearest post troquel (troquelBaseY + k*paso).</summary>
        private static double SnapToTroquel(double y, double troquelBaseY, double paso)
        {
            if (paso <= 0.0)
            {
                return y;
            }

            return troquelBaseY + Math.Round((y - troquelBaseY) / paso, MidpointRounding.AwayFromZero) * paso;
        }

        private static Point2D Local(RackCatalog catalog, string pieceId, string connectionPointId, string view)
        {
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(pieceId, connectionPointId, view);
            return entry == null ? new Point2D(0.0, 0.0) : new Point2D(entry.LocalX, entry.LocalY);
        }

        private static string Block(RackCatalog catalog, string pieceId, string view)
            => catalog?.Blocks.FindBlock(pieceId, view)?.BlockName;

        /// <summary>Block origin so that its local CELOSIA point lands on the anchor, accounting for rotation.</summary>
        private static Point2D Insertion(Point2D anchor, Point2D localConnection, double rotation)
        {
            var rotated = Rotate(localConnection, rotation);
            return new Point2D(anchor.X - rotated.X, anchor.Y - rotated.Y);
        }

        private static Point2D Rotate(Point2D p, double radians)
        {
            var cos = Math.Cos(radians);
            var sin = Math.Sin(radians);
            return new Point2D(p.X * cos - p.Y * sin, p.X * sin + p.Y * cos);
        }

        private static string FirstNonEmpty(params string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return candidate.Trim();
                }
            }

            return null;
        }

        /// <summary>A side's reinforcement zone: whether present, how high it reaches, and its inner troquel X.</summary>
        private readonly struct ReinforcementZone
        {
            public ReinforcementZone(bool enabled, double height, double troquelX)
            {
                Enabled = enabled;
                Height = height;
                TroquelX = troquelX;
            }

            public bool Enabled { get; }
            public double Height { get; }
            public double TroquelX { get; }

            public static ReinforcementZone None => new ReinforcementZone(false, 0.0, 0.0);
        }
    }
}
