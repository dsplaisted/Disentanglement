using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuzzleSolver
{
    public class PuzzleState
    {
        private Dictionary<Point, PuzzlePiecePosition> _pointLookup;
        private Point _minBounds;
        private Point _maxBounds;

        static Dictionary<Point, Func<IEnumerable<Point>, Point, Point, bool>> _exitTests;

        static PuzzleState()
        {
            _exitTests = new Dictionary<Point, Func<IEnumerable<Point>, Point, Point, bool>>();
            _exitTests[new Point(1, 0, 0)] = (points, minBounds, maxBounds) => points.All(p => p.X > maxBounds.X);
            _exitTests[new Point(-1, 0, 0)] = (points, minBounds, maxBounds) => points.All(p => p.X < minBounds.X);
            _exitTests[new Point(0, 1, 0)] = (points, minBounds, maxBounds) => points.All(p => p.Y > maxBounds.Y);
            _exitTests[new Point(0, -1, 0)] = (points, minBounds, maxBounds) => points.All(p => p.Y < minBounds.Y);
            _exitTests[new Point(0, 0, 1)] = (points, minBounds, maxBounds) => points.All(p => p.Z > maxBounds.Z);
            _exitTests[new Point(0, 0, -1)] = (points, minBounds, maxBounds) => points.All(p => p.Z < minBounds.Z);
        }

        public PuzzleState(IEnumerable<PuzzlePiecePosition> pieces)
        {
			//this.Pieces = pieces.ToList().AsReadOnly();
			this.Pieces = pieces.ToList();
        }

        public IEnumerable<PuzzlePiecePosition> Pieces { get; private set; }

        public Point MinBounds
        {
            get
            {
                if (_minBounds == null)
                {
                    CalculateBounds();
                }
                return _minBounds;
            }
        }

        public Point MaxBounds
        {
            get
            {
                if (_maxBounds == null)
                {
                    CalculateBounds();
                }
                return _maxBounds;
            }
        }

        public PuzzleState Normalize()
        {
            var allPoints = Pieces.SelectMany(piece => piece.CurrentPoints);
            if (!allPoints.Any())
            {
                return this;
            }
            var normalizationVector = Point.Multiply(allPoints.First(), -1);
            if (normalizationVector == Point.Zero)
            {
                return this;
            }

            return new PuzzleState(Pieces.Select(piece => piece.Move(normalizationVector)));
        }

        public PuzzlePiecePosition GetPieceAtPoint(Point point)
        {
            if (_pointLookup == null)
            {
                CalculatePointLookup();
            }

            PuzzlePiecePosition ret;
            _pointLookup.TryGetValue(point, out ret);
            return ret;
        }

        public PuzzlePiecePosition GetPiecePosition(PuzzlePiece piece)
        {
            return Pieces.Single(p => p.Piece == piece);
        }

        private void CalculatePointLookup()
        {
            _pointLookup = new Dictionary<Point, PuzzlePiecePosition>();
            foreach (var p in Pieces)
            {
                foreach (var point in p.CurrentPoints)
                {
                    if (_pointLookup.ContainsKey(point))
                    {
                        throw new InvalidOperationException("Invalid state.  Piece " +
                            _pointLookup[point].Piece.Name + " overlaps with piece " +
                            p.Piece.Name);
                    }
                    _pointLookup[point] = p;
                }
            }
        }

        public void CalculateBounds(IEnumerable<PuzzlePiece> piecesToExclude, out Point minBounds, out Point maxBounds)
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int minZ = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            int maxZ = int.MinValue;

            foreach (var p in Pieces.Except(piecesToExclude.Select(p => GetPiecePosition(p))))
            {
                foreach (var point in p.CurrentPoints)
                {
                    minX = Math.Min(minX, point.X);
                    minY = Math.Min(minY, point.Y);
                    minZ = Math.Min(minZ, point.Z);

                    maxX = Math.Max(maxX, point.X);
                    maxY = Math.Max(maxY, point.Y);
                    maxZ = Math.Max(maxZ, point.Z);
                }
            }

            minBounds = new Point(minX, minY, minZ);
            maxBounds = new Point(maxX, maxY, maxZ);
        }

        private void CalculateBounds()
        {
            Point minBounds;
            Point maxBounds;
            CalculateBounds(Enumerable.Empty<PuzzlePiece>(), out minBounds, out maxBounds);

            _minBounds = minBounds;
            _maxBounds = maxBounds;
        }

		private Point GetExitDirection(PuzzlePiecePosition piece)
		{
			Point minBounds;
			Point maxBounds;
			CalculateBounds(new[] { piece.Piece }, out minBounds, out maxBounds);

            foreach (var kvp in _exitTests)
            {
                Point direction = kvp.Key;
                var test = kvp.Value;
                var testPosition = piece;
                Point vector = Point.Zero;
                while (true)
                {
                    if (test(testPosition.CurrentPoints, minBounds, maxBounds))
                    {
                        return direction;
                    }

                    testPosition = testPosition.Move(direction);
                    vector = Point.Add(vector, direction);
                    PuzzleMove testMove = new PuzzleMove(this, piece.Piece, vector);
                    if (!testMove.IsLegal())
                    {
                        break;
                    }
                }
            }

            //if (piece.CurrentPoints.All(p => p.X > maxBounds.X))
            //{
            //    return new Point(1, 0, 0);
            //}
            //if (piece.CurrentPoints.All(p => p.X < minBounds.X))
            //{
            //    return new Point(-1, 0, 0);
            //}

            //if (piece.CurrentPoints.All(p => p.Y > maxBounds.Y))
            //{
            //    return new Point(0, 1, 0);
            //}

            //if (piece.CurrentPoints.All(p => p.Y < minBounds.Y))
            //{
            //    return new Point(0, -1, 0);
            //}

            //if (piece.CurrentPoints.All(p => p.Z > maxBounds.Z))
            //{
            //    return new Point(0, 0, 1);
            //}
            //if (piece.CurrentPoints.All(p => p.Z < minBounds.Z))
            //{
            //    return new Point(0, 0, -1);
            //}

			return Point.Zero;

		}

        public IEnumerable<PuzzleMove> GetLegalMoves()
        {
            Queue<PuzzleMove> potentialExitMoves = new Queue<PuzzleMove>();
            Queue<PuzzleMove> potentialMoves = new Queue<PuzzleMove>();
            List<PuzzleMove> ret = new List<PuzzleMove>();

			foreach (var piece in Pieces)
			{
				Point exitDirection = GetExitDirection(piece);
				if (exitDirection != Point.Zero)
				{
					//	If a piece can be removed, the only valid moves are removing it, and putting it back in the puzzle
					PuzzleMove removeMove = new PuzzleMove(this, piece.Piece, exitDirection, true);
					//	We've already verified that the remove move is valid, and we want to take removal moves first anyway
					//ret.Add(removeMove);
                    potentialExitMoves.Enqueue(removeMove);

                    //PuzzleMove move = new PuzzleMove(this, piece.Piece, new Point(-exitDirection.X, -exitDirection.Y, -exitDirection.Z));
                    //potentialExitMoves.Enqueue(move);
				}
				else
				{
					foreach (var direction in Point.Directions)
					{
						PuzzleMove move = new PuzzleMove(this, piece.Piece, direction);
						potentialMoves.Enqueue(move);
					}
				}
			}

            if (potentialExitMoves.Any())
            {
                //  If any pieces can be removed from the puzzle, only consider those as valid moves
                potentialMoves = potentialExitMoves;
            }

			while (potentialMoves.Count > 0)
			{
				var move = potentialMoves.Dequeue();
				if (move.IsLegal())
				{
					if (!ret.Contains(move))
					{
						ret.Add(move);
					}
				}
				else
				{
					var blockers = move.GetBlockingPieces();
					var newMovePieces = move.MovingPieces.Concat(blockers);

					if (newMovePieces.Count() <= this.Pieces.Count() / 2)
					{
						var newMove = new PuzzleMove(this, newMovePieces, move.Direction);
						potentialMoves.Enqueue(newMove);
					}					
				}
			}

			

			return ret;
        }

        public bool Equals(PuzzleState other)
        {
            if (other == null)
            {
                return false;
            }

            if (Pieces.Count() != other.Pieces.Count())
            {
                return false;
            }

            if (Pieces.Select(p => p.Piece).Except(other.Pieces.Select(p => p.Piece)).Any())
            {
                return false;
            }

            foreach (var piece in Pieces)
            {
                if (other.GetPiecePosition(piece.Piece) != piece)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PuzzleState))
            {
                return false;
            }
            return Equals((PuzzleState)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                uint ret = 0;
                foreach (var piece in Pieces)
                {
                    ret = RotateLeft(ret, 5);
                    ret = ret ^ (uint)piece.GetHashCode();
                }
                return (int)ret;
            }
        }

        private static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        public static bool operator ==(PuzzleState s1, PuzzleState s2)
        {
            if (object.ReferenceEquals(s1, s2))
            {
                return true;
            }
            if (object.ReferenceEquals(s1, null) || object.ReferenceEquals(s2, null))
            {
                return false;
            }

            return s1.Equals(s2);
        }

        public static bool operator !=(PuzzleState s1, PuzzleState s2)
        {
            return !(s1 == s2);
        }

    }
}
