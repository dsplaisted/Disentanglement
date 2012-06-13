using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace PuzzleSolver
{
	public class Solver
	{
		Stack<SolverFrame> Stack { get; set; }
        public PuzzleState CurrentState
        {
            get
            {
                if (Stack.Any())
                {
                    return Stack.Peek().PuzzleState;
                }
                else
                {
                    return Solution.PuzzleState;
                }
            }
        }
		private Dictionary<PuzzleState, SolverFrame> _visitedStates;

		public int NumSteps { get; private set; }

        public SolverFrame Solution { get; private set; }

		public bool Solved
		{
			get
			{
				//	Puzzle is solved if there are no pieces left
				//return !Stack.Peek().PuzzleState.Pieces.Any();
                return Solution != null;
			}
		}

        public bool Done
        {
            get
            {
                return Stack.Count == 0;
            }
        }

		public bool EnableDebugLog { get; set; }

		public Solver(PuzzleState initialState)
		{
            //EnableDebugLog = true;
			Stack = new Stack<SolverFrame>();

            Push(initialState.Normalize(), null, null);

			_visitedStates = new Dictionary<PuzzleState, SolverFrame>();
		}

		private void WriteLine(string s)
		{
			if (EnableDebugLog)
			{
				Debug.WriteLine(s);
			}
		}

		public void Step()
		{
            //if (Solved)
            //{
            //    return;
            //}

			NumSteps++;

			WriteLine("Step " + NumSteps);

			var frame = Stack.Peek();
            if (!frame.PuzzleState.Pieces.Any())
            {
                Solution = frame;
            }
			_visitedStates[frame.PuzzleState] = frame;

            if (Solved && Solution.SolutionDepth < frame.SolutionDepth)
            {
                WriteLine("Backtracking - more moves than best solution found so far");
                Stack.Pop();
                return;
            }

			//if (frame.PossibleMoves == null)
			//{
			//    var possibleMoves = frame.PuzzleState.GetLegalMoves();
			//    WriteLine("Possible moves:");
			//    foreach (var move in possibleMoves)
			//    {
			//        WriteLine("\t" + move.ToString());
			//    }
			//    frame.PossibleMoves = new Queue<PuzzleMove>(possibleMoves);
			//}

			while (true)
			{
				if (frame.PossibleMoves.Count == 0)
				{
					WriteLine("Backtracking");
					Stack.Pop();
					return;
				}
				else
				{
					frame.CurrentMove = frame.PossibleMoves.Dequeue();
					PuzzleState newState = frame.CurrentMove.GetEndingState().Normalize();

					if (_visitedStates.ContainsKey(newState))
					{
						WriteLine("Already visited " + frame.CurrentMove.ToString());
                        SolverFrame prevSolution = _visitedStates[newState];
                        int prevSolutionDepth = prevSolution.SolutionDepth;
                        if (frame.SolutionDepth + 1 < prevSolution.SolutionDepth)
                        {
                            //_visitedStates[newState].FoundBetterSolution = true;
                            WriteLine("Found fewer steps");
                            prevSolution.BestSolutionPrevFrame = frame;
                            prevSolution.BestSolutionPrevMove = frame.CurrentMove;
                            prevSolution.CalculateSolutionDepth();
                            if (prevSolution.SolutionDepth != frame.SolutionDepth + 1)
                            {
                                throw new InvalidOperationException("oops");
                            }
                        }
						continue;
					}

					WriteLine("Moving " + frame.CurrentMove.ToString());

					Push(newState, frame.CurrentMove, frame);

					return;
				}
			}
		}

		private void Push(PuzzleState newState, PuzzleMove move, SolverFrame prevFrame)
		{
			SolverFrame newFrame = new SolverFrame();
			newFrame.PuzzleState = newState;
			newFrame.PrevFame = prevFrame;
            newFrame.BestSolutionPrevFrame = prevFrame;
            newFrame.BestSolutionPrevMove = move;
            newFrame.ChildFrames = new Dictionary<SolverFrame, object>();

            newFrame.CalculateSolutionDepth();

			var possibleMoves = newFrame.PuzzleState.GetLegalMoves();
			WriteLine("Possible moves:");
			foreach (var possible in possibleMoves)
			{
				WriteLine("\t" + possible.ToString());
			}
			newFrame.PossibleMoves = new Queue<PuzzleMove>(possibleMoves);

            Stack.Push(newFrame);
		}

		public SolverFrame[] GetMoveSequence()
		{
            if (Solved)
            {
                List<SolverFrame> ret = new List<SolverFrame>();
                SolverFrame frame = Solution;
                while (frame != null)
                {
                    ret.Add(frame);
                    frame = frame.BestSolutionPrevFrame;
                }
                ret.Reverse();
                return ret.ToArray();
            }
            else
            {
                return Stack.Reverse().ToArray();
            }
		}
	}

	public class SolverFrame
	{
		public PuzzleState PuzzleState { get; set; }
		public SolverFrame PrevFame { get; set; }
		public PuzzleMove CurrentMove { get; set; }
		public Queue<PuzzleMove> PossibleMoves { get; set; }
        //public bool FoundBetterSolution { get; set; }
        public SolverFrame BestSolutionPrevFrame { get; set; }
        public PuzzleMove BestSolutionPrevMove { get; set; }
        public int SolutionDepth { get; set; }
        public Dictionary<SolverFrame, object> ChildFrames { get; set; }

        public void CalculateSolutionDepth()
        {
            if (BestSolutionPrevFrame == null)
            {
                SolutionDepth = 0;
            }
            else
            {
                SolutionDepth = BestSolutionPrevFrame.SolutionDepth + 1;
            }
            foreach (var child in ChildFrames.Keys)
            {
                child.CalculateSolutionDepth();
            }
        }
	}
}
