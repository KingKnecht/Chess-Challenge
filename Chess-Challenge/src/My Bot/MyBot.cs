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
        public Move BranchMove; public List<Evaluation> Children = new();
        public bool IsRoot { get; }

        public float Value { get; private set; }

        public Evaluation Parent;

        public Evaluation(Move branchMove, Evaluation parent, int depth)
        {
            Parent = parent;
            BranchMove = branchMove;
            Value = depth % 2 == 0 ? float.MaxValue : float.MinValue;
        }
        public Evaluation()
        {
            IsRoot = true;
        }

        public void SetValue(float value, int depth)
        {
            Value = value;

            if (IsRoot) return;

            var minMax = depth % 2 == 0
                ? Math.Max(value, Parent.Value)
                : Math.Min(value, Parent.Value);

            Parent.SetValue(minMax, --depth);
        }

        public string ToBranchString()
        {

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(this.ToString());

            if (!Children.Any()) return stringBuilder.ToString();

            var minChild = Children.MinBy(p => p.Value);
            stringBuilder.AppendLine(minChild.ToString());

            ToBranchStringRec(stringBuilder, minChild);

            return stringBuilder.ToString();
        }

        private void ToBranchStringRec(StringBuilder stringBuilder, Evaluation parent)
        {
            if (!parent.Children.Any()) return;

            var minChild = parent.Children.MinBy(p => p.Value);
            stringBuilder.AppendLine(minChild.ToString());
            ToBranchStringRec(stringBuilder, minChild);
        }

        public override string ToString()
        {
            return $"{BranchMove} ({Value})";
        }

        public void GenerateTree(Board board, int depth, Evaluation parent, bool isWhite)
        {
            if (depth == 0) return;

            var moves = board.GetLegalMoves();
            for (var i = 0; i < moves.Length; i++)
            {
                var newMove = moves[i];
                board.MakeMove(newMove);

                var value = GetMaterialDifference(board, isWhite);

                if (!parent.IsRoot)
                {
                    var uncles = parent.Parent.Children;
                    if (depth % 2 != 0)
                    {
                        if (uncles.Any(n => n.Value > value))
                        {
                            board.UndoMove(newMove);
                            return;
                        }
                    }
                    //else
                    //{
                    //    if (uncles.Any(n => n.Value < value))
                    //    {
                    //        board.UndoMove(new_move);
                    //        return;
                    //    }
                    //}

                }

                var eval = new Evaluation(newMove, parent, depth);
                Children.Add(eval);
                eval.SetValue(value, depth);


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
        node.GenerateTree(board, 4, node, isWhite);


        var selectedMove = node.Children.MaxBy(n => n.Value);
        Console.WriteLine(selectedMove.ToBranchString());

        return selectedMove.BranchMove;
    }




}