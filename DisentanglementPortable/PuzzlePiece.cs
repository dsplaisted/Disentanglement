using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuzzleSolver
{
    public class PuzzlePiece
    {
        public PuzzlePiece(string name, IEnumerable<Point> points)
        {
            this.Name = name;
            this.Points = points.ToList();
        }

        public string Name { get; private set; }
        public IEnumerable<Point> Points { get; private set; }
    }

    public class PuzzlePiecePosition
    {
        public PuzzlePiecePosition(PuzzlePiece piece)
            : this(piece, new Point(0,0,0))
        {
        }

        public PuzzlePiecePosition(PuzzlePiece piece, Point position)
        {
            this.Piece = piece;
            this.Position = position;
        }

        public PuzzlePiece Piece { get; private set; }
        public Point Position { get; private set; }

        public IEnumerable<Point> CurrentPoints
        {
            get
            {
                return Piece.Points.Select(p => Point.Add(p, Position));
            }
        }

        public PuzzlePiecePosition Move(Point vector)
        {
            return new PuzzlePiecePosition(Piece, Point.Add(Position, vector));
        }

        public bool Equals(PuzzlePiecePosition other)
        {
            if (other == null)
            {
                return false;
            }

            return Piece == other.Piece &&
                Position == other.Position;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PuzzlePiecePosition))
            {
                return false;
            }
            return Equals((PuzzlePiecePosition)obj);
        }

        public override int GetHashCode()
        {
            return Piece.GetHashCode() ^ Position.GetHashCode();
        }

        public static bool operator ==(PuzzlePiecePosition p1, PuzzlePiecePosition p2)
        {
            if (object.ReferenceEquals(p1, p2))
            {
                return true;
            }
            if (object.ReferenceEquals(p1, null) || object.ReferenceEquals(p2, null))
            {
                return false;
            }

            return p1.Equals(p2);
        }

        public static bool operator !=(PuzzlePiecePosition p1, PuzzlePiecePosition p2)
        {
            return !(p1 == p2);
        }
    }
}
