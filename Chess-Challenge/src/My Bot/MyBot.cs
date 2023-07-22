using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ChessChallenge.API;
using static MyBot;

public class MyBot : IChessBot
{
    public class Evaluation
    {
        public Evaluation(Move move, int value)
        {
            Move = move;
            Value = value;
        }

        public Move Move { get;  }
        public int Value { get;  }

        public List<Evaluation> Children { get; } = new List<Evaluation>();
    }

    private int GetPieceValue(Piece piece)
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
    private int GetMaterialDifference(Board board, bool is_white)
    {
        var white = board.GetAllPieceLists().Where(l => l.IsWhitePieceList).SelectMany( p => p).ToArray();
        var black = board.GetAllPieceLists().Where(l => !l.IsWhitePieceList).SelectMany(p => p).ToArray();

        var white_total = white.Aggregate(0, (value, piece) => GetPieceValue(piece) + value);
        var black_total = black.Aggregate(0, (value, piece) => GetPieceValue(piece) + value);

        return is_white ? white_total - black_total : black_total - white_total;

    }
    public Move Think(Board board, Timer timer)
    {
        Random rand = new Random();
        bool is_white = board.PlyCount % 2 == 0;

        Move[] moves = board.GetLegalMoves();


        Evaluation evaluation = new Evaluation(moves[0], 0);
        ThinkRecursive(board, timer, is_white, evaluation, 0);

        //return new_differences.MaxBy(kvp => kvp.Value).Key;
        return moves[0];
    }

    private void ThinkRecursive(Board board, Timer timer,bool is_white, Evaluation evaluation, int currentDepth)
    {
        currentDepth++;

        if(currentDepth > 1) return;

        Move[] moves = board.GetLegalMoves();
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int new_material_difference = GetMaterialDifference(board, is_white);
            
            var childEvaluation = new Evaluation(move, new_material_difference);
            evaluation.Children.Add(childEvaluation);
            
            ThinkRecursive(board, timer, is_white, childEvaluation, currentDepth);

            board.UndoMove(move);
        }
    }

   
}