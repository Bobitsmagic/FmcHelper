using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace NISSHelper
{
	public partial class Window : Form
	{
		List<Move> moves;
		List<Move> reverseMoves;

		List<Move> solution = new List<Move>();
		List<Move> inverse = new List<Move>();

		static string ScrambleToString(List<Move> moves, int mod = 4)
		{
			string s = "";
			for (int i = 0; i < moves.Count; i++)
			{
				s += MoveToString(moves[i]);

				if (i != moves.Count - 1)
				{
					if (i % mod == mod - 1)
					{
						s += "    ";
					}
					else
					{
						s += " ";
					}
				}
			}

			return s;
		}

		static Move FromString(string s)
		{
			int sum = (int)(Move)Enum.Parse(typeof(Move), s[0].ToString(), true);

			if (s.Length == 2)
				sum += s[1] == '2' ? 1 : 2;

			return (Move)sum;
		}

		static Move ReverseMove(Move m)
		{
			int val = (int)m;
			return (Move)(val +
				((val % 3) == 0 ? 2 :
				(val % 3) == 1 ? 0 : -2));
		}

		static string MoveToString(Move m)
		{
			return m.ToString().Replace('P', '\'');
		}
		public Window()
		{
			InitializeComponent();
		}

		private void ParseScrambleB_Click(object sender, EventArgs e)
		{
			moves = ScrambleTB.Text.Split(' ').Select(x => FromString(x)).ToList();
			reverseMoves = moves.Select(x => ReverseMove(x)).Reverse().ToList();

			ScrambleLabel.Text = ScrambleToString(moves);
			ReverseScrambleLabel.Text = ScrambleToString(reverseMoves);
		}

		private void ParseSolutionB_Click(object sender, EventArgs e)
		{
			try
			{
				solution = TBSolution.Text.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => FromString(x)).ToList();
			}
			catch
			{
				solution = new List<Move>();
			}

			SolutionReverseLabel.Text = ScrambleToString(solution.Select(x => ReverseMove(x)).Reverse().ToList()) + " (" + solution.Count.ToString() + ")";

			Combine();
		}

		private void ParseInverseB_Click(object sender, EventArgs e)
		{
			try
			{
				inverse = TBInverse.Text.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => FromString(x)).ToList();
			}
			catch
			{
				inverse = new List<Move>();
			}

			InverseReverseLabel.Text = ScrambleToString(inverse.Select(x => ReverseMove(x)).Reverse().ToList()) + " (" + inverse.Count.ToString() + ")";

			Combine();
		}

		private void Combine()
		{
			List<Move> list = new List<Move>(solution);
			list.AddRange(inverse.Select(x => ReverseMove(x)).Reverse());

			CombinedSolutionLabel.Text = ScrambleToString(list);
		}
	}
}
