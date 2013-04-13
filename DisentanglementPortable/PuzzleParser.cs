using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PuzzleSolver
{
    public static class PuzzleParser
    {
        public static PuzzleState GetGordionCubePuzzle()
        {
			using (var sr = new StreamReader(typeof(PuzzleParser).Assembly.GetManifestResourceStream("PuzzleSolver.GordionCube.txt")))
            {
                string text = sr.ReadToEnd();
                return ReadPuzzle(text);
            }
        }

        public static PuzzleState ReadPuzzle(string s)
        {
#if ANDROID
			string[] lines = s.Split (new[] { "\r\n" }, StringSplitOptions.None);
#else
            string[] lines = s.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
#endif

            List<string> pieceStrings = new List<string>();

            StringBuilder sb = new StringBuilder();
            foreach (string l in lines.Where(l => !l.StartsWith("#")))
            {
                sb.AppendLine(l);
                if (l.Length == 0)
                {
                    pieceStrings.Add(sb.ToString());
                    sb.Length = 0;
                }
            }

            if (sb.Length > 0)
            {
                pieceStrings.Add(sb.ToString());
            }

            var pieces = pieceStrings.Select(p => ReadPiece(p));
            var positions = pieces.Select(p => new PuzzlePiecePosition(p));

            return new PuzzleState(positions);
        }

        public static PuzzlePiece ReadPiece(string s)
        {
            string[] lines = s.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            string name = lines[0];
            Point origin = ReadPoint(lines[1]);
            Point rowDirection = ReadPoint(lines[2]);
            Point colDirection = ReadPoint(lines[3]);

            List<Point> points = new List<Point>();

            Point lineOrigin = origin;

            foreach (var line in lines.Skip(4))
            {
                Point point = lineOrigin;
                foreach (char ch in line)
                {
                    if (ch == '1')
                    {
                        points.Add(point);
                    }
                    point = Point.Add(point, rowDirection);
                }

                lineOrigin = Point.Add(lineOrigin, colDirection);
            }

            return new PuzzlePiece(name, points);
        }

        public static Point ReadPoint(string s)
        {
            string[] fields = s.Split(',');
            if (fields.Length < 3)
            {
                throw new InvalidOperationException("Can't convert '" + s + "' to point.");
            }

            int x;
            int y;
            int z;

            x = int.Parse(fields[0]);
            y = int.Parse(fields[1]);
            z = int.Parse(fields[2]);

            return new Point(x, y, z);

        }
    }
}
