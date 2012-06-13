using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuzzleSolver
{
    public class Point
    {
        public Point(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }

		public Point Negate()
		{
			return new Point(-X, -Y, -Z);
		}

		private string ElementToStringHelper(int value, string posDesc, string negDesc)
		{
			if (value == 0)
			{
				return null;
			}
			else if (value == 1)
			{
				return posDesc;
			}
			else if (value == -1)
			{
				return negDesc;
			}
			else if (value > 0)
			{
				return posDesc + " " + value.ToString();
			}
			else
			{
				return negDesc + " " + Math.Abs(value).ToString();
			}
		}

		public override string ToString()
		{
			List<string> elementDescs = new List<string>();
			elementDescs.Add(ElementToStringHelper(X, "Right", "Left"));
			elementDescs.Add(ElementToStringHelper(Y, "Up", "Down"));
			elementDescs.Add(ElementToStringHelper(Z, "Back", "Forward"));

			elementDescs = elementDescs.Where(s => !string.IsNullOrEmpty(s)).ToList();
			if (elementDescs.Count == 0)
			{
				return "None";
			}

			return string.Join(", ", elementDescs.ToArray());
		}

        public static Point Add(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        public static Point Multiply(Point point, int multiplier)
        {
            return new Point(point.X * multiplier, point.Y * multiplier, point.Z * multiplier);
        }

        public bool Equals(Point other)
        {
            if (other == null)
            {
                return false;
            }
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point))
            {
                return false;
            }
            return Equals((Point)obj);
        }

        public override int GetHashCode()
        {
            return (X + (Y * 100) + (Z * 10000)).GetHashCode();
        }

        public static bool operator ==(Point p1, Point p2)
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

        public static bool operator !=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

        public static readonly Point Zero = new Point(0, 0, 0);

        public static readonly Point Up = new Point(0, 1, 0);
        public static readonly Point Down = new Point(0, -1, 0);
        public static readonly Point Left = new Point(-1, 0, 0);
        public static readonly Point Right = new Point(1, 0, 0);
        public static readonly Point Forward = new Point(0, 0, -1);
        public static readonly Point Back = new Point(0, 0, 1);

        public static readonly IEnumerable<Point> Directions = new[] { Up, Down, Left, Right, Forward, Back };
    }
}
