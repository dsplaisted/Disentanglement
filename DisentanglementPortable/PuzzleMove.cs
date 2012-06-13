using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuzzleSolver
{
    public class PuzzleMove
    {
        public PuzzleState StartingState { get; private set; }
        public IEnumerable<PuzzlePiece> MovingPieces { get; private set; }
        public Point Direction { get; private set; }
		public bool IsRemoval { get; private set; }

        private Dictionary<PuzzlePiece, PuzzlePiece> _movingDict;
		public PuzzleMove(PuzzleState startingState, PuzzlePiece movingPiece, Point direction, bool isRemoval) :
			this(startingState, movingPiece, direction)
		{
			this.IsRemoval = isRemoval;
		}

		public PuzzleMove(PuzzleState startingState, PuzzlePiece movingPiece, Point direction) :
			this(startingState, new[] { movingPiece }, direction)
		{

		}

        public PuzzleMove(PuzzleState startingState, IEnumerable<PuzzlePiece> movingPieces, Point direction)
        {
            this.StartingState = startingState;
            this.MovingPieces = movingPieces;
            this.Direction = direction;

            _movingDict = MovingPieces.ToDictionary(p => p);
        }

        public bool IsLegal()
        {
            //Dictionary<PuzzlePiece, PuzzlePiece> movingPieces = Pieces.ToDictionary(p => p);

            //return Pieces.Select(piece => StartingState.GetPiecePosition(piece))
            //    .SelectMany(piece => piece.CurrentPoints)
            //    .Select(point => Point.Add(point, Direction))
            //    .All(point =>
            //    {
            //        var hitPiece = StartingState.GetPieceAtPoint(point);
            //        return hitPiece == null || movingPieces.ContainsKey(hitPiece.Piece);
            //    });

            return !GetBlockingPieces().Any();
        }

        public IEnumerable<PuzzlePiece> GetBlockingPieces()
        {
            var destinationPoints = MovingPieces.Select(piece => StartingState.GetPiecePosition(piece))
                .SelectMany(piece => piece.CurrentPoints)
                .Select(point => Point.Add(point, Direction));

            var blockingPieces = destinationPoints.Select(point => StartingState.GetPieceAtPoint(point))
                .Where(piece => piece != null)
                .Select(piece => piece.Piece)
                .Distinct()
                .Where(piece => !_movingDict.ContainsKey(piece));

            return blockingPieces;
        }

        public PuzzleState GetEndingState()
        {
			if (IsRemoval)
			{
				return new PuzzleState(StartingState.Pieces.Where(piece =>
					{
						return !_movingDict.ContainsKey(piece.Piece);
					}));
			}
			else
			{
				return new PuzzleState(StartingState.Pieces.Select(piece =>
					{
						if (_movingDict.ContainsKey(piece.Piece))
						{
							return piece.Move(Direction);
						}
						else
						{
							return piece;
						}
					}));
			}
        }

		public override string ToString()
		{
			string pieces = string.Join(", ", MovingPieces.Select(p => p.Name).ToArray());
			if (IsRemoval)
			{
				return "Remove " + pieces;
			}
			else
			{
				return pieces + " " + Direction.ToString();
			}

		}

		public bool Equals(PuzzleMove other)
		{
			if (other == null)
			{
				return false;
			}

			return other.StartingState == StartingState &&
				other.GetEndingState() == GetEndingState();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is PuzzleMove))
			{
				return false;
			}
			return Equals((PuzzleMove)obj);
		}

		public static bool operator ==(PuzzleMove s1, PuzzleMove s2)
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

		public static bool operator !=(PuzzleMove s1, PuzzleMove s2)
        {
            return !(s1 == s2);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
