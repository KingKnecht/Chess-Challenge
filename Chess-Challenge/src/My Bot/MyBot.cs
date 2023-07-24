using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessChallenge.API;
using static MyBot;


public class MyBot : IChessBot
{
    public class Evaluation
    {
        public int Depth { get; }
        public Move BranchMove;
        public List<Evaluation> Children = new();
        public bool IsRoot { get; }

        public float Alpha { get; private set; } = float.MinValue;
        public float Beta { get; private set; } = float.MaxValue;

        public float Value => IsMaximizerNode ? Alpha : Beta;

        public Evaluation Parent;

        public Evaluation(Move branchMove, Evaluation parent, int depth)
        {
            Depth = depth;
            Parent = parent;
            BranchMove = branchMove;
            Alpha = float.MinValue;
            Beta = float.MaxValue;
            IsMaximizerNode = depth % 2 == 0;
        }

        public bool IsMaximizerNode { get; }

        public Evaluation()
        {
            IsRoot = true;
            IsMaximizerNode = true;
        }

        public void SetValue(float value)
        {

            if (IsMaximizerNode)
            {
                Alpha = Math.Max(Alpha, value);
            }
            else
            {
                Beta = Math.Min(Beta, value);
            }

            if (IsRoot) return;

            Parent.SetValue(value);
        }

        public string ToBranchString()
        {

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(this.ToString());

            if (!Children.Any()) return stringBuilder.ToString();

            var minChild = Children.MaxBy(p => p.Alpha);
            stringBuilder.AppendLine(minChild.ToString());

            ToBranchStringRec(stringBuilder, minChild);

            return stringBuilder.ToString();
        }

        private void ToBranchStringRec(StringBuilder stringBuilder, Evaluation parent)
        {
            if (!parent.Children.Any()) return;

            var minChild = parent.Children.MinBy(p => p.Alpha);
            stringBuilder.AppendLine(minChild.ToString());
            ToBranchStringRec(stringBuilder, minChild);
        }

        public override string ToString()
        {
            return $"{BranchMove} ({Alpha})";
        }

        private int _lastDepth = 0;
        public void GenerateTree(Board board, int depth, Evaluation parent, bool isWhite)
        {

            if (depth == 0) return;

            var moves = board.GetLegalMoves();
            foreach (var newMove in moves)
            {
                board.MakeMove(newMove);

                var value = GetMaterialDifference(board, isWhite);
                var eval = new Evaluation(newMove, parent, depth);
                Children.Add(eval);
                eval.SetValue(value);

                eval.GenerateTree(board, depth - 1, eval, isWhite);
                board.UndoMove(newMove);
            }
        }

        public static float GetMaterialDifference(Board board, bool is_white)
        {
            float[] totals = { 0, 0 };
            foreach (var list in board.GetAllPieceLists())
            {
                foreach (var piece in list)
                {
                    totals[(list.IsWhitePieceList) ? 0 : 1] += GetPieceValue(piece);
                }
            }
            var result = is_white ? totals[0] - totals[1] : totals[1] - totals[0];
            return result;
        }

        public static float GetPieceValue(Piece piece)
        {
            switch (piece.PieceType)
            {
                case PieceType.Pawn:
                    return 1;
                case PieceType.Knight:
                case PieceType.Bishop:
                    return 3;
                case PieceType.Rook:
                    return 5;
                case PieceType.Queen:
                    return 9;
                case PieceType.King:
                    return 1000;

            }
            return 0;
        }


    }

    public Move Think(Board board, Timer timer)
    {
        bool isWhite = board.PlyCount % 2 == 0;

        Evaluation node = new Evaluation();
        node.GenerateTree(board, 2, node, isWhite);


        var selectedMove = node.Children.MinBy(n => n.Alpha);
        Console.WriteLine(selectedMove.ToBranchString());

        return selectedMove.BranchMove;
    }




}