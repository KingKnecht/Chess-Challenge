using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ChessChallenge.API;
using ChessChallenge.Chess;
using Raylib_cs;
using static MyBot;
using Board = ChessChallenge.API.Board;
using Move = ChessChallenge.API.Move;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        var maxDepth = 5;
        int depth = maxDepth - 1;
        var isWhite = board.PlyCount % 2 == 0;

        var result = AlphaBeta(board, depth, maxDepth, true, isWhite, float.MinValue, float.MaxValue);
        return _bestMove;
    }

    private Move _bestMove = Move.NullMove;

    private float AlphaBeta(Board board, int depth, int maxDepth, bool isMaximizer, bool isWhite, float alpha,
        float beta)
    {
  
        // If max depth is reached or Game is Over
        if (depth == 0 || board.IsInCheckmate())
        {
            var value = GetMaterialDifference(board, isWhite);
            return value;
        }

        if (isMaximizer)
        {
            float hValue = float.MinValue;

            foreach (var move in board.GetLegalMoves().OrderByDescending(m => m.IsCapture))
            {
                board.MakeMove(move);

                var value = AlphaBeta(board, depth - 1, maxDepth, isWhite, !isMaximizer, alpha, beta);
                board.UndoMove(move);

                if (hValue < value)
                {
                    hValue = value;

                    // Remember which move gave the highest hValue
                    if (depth == maxDepth - 1)
                    {
                        _bestMove = move;
                    }
                }

                if (hValue > alpha)
                    alpha = hValue;

                if (beta <= alpha)
                    break;
            }

            return hValue;
        }
        else
        {
            float hValue = float.MaxValue;

            foreach (var move in board.GetLegalMoves().OrderByDescending(m => m.IsCapture))

            {
                board.MakeMove(move);

                var value = AlphaBeta(board, depth - 1, maxDepth, !isMaximizer, isWhite, alpha, beta);

                board.UndoMove(move);

                if (hValue > value) 
                    hValue = value;

                if (hValue < beta)
                    beta = hValue;

                if (beta <= alpha)
                    break;
            }

            return hValue;
        }
    }

    public static float GetMaterialDifference(Board board, bool isWhite)
    {
        if (board.IsInCheckmate())
        {
            return isWhite ? 100000 : -100000;
        }

        float[] totals = { 0, 0 };
        foreach (var list in board.GetAllPieceLists())
        {
            foreach (var piece in list)
            {
                totals[(list.IsWhitePieceList) ? 0 : 1] += GetPieceValue(piece);
            }
        }

        var result = isWhite ? totals[0] - totals[1] : totals[1] - totals[0];
        //var result = totals[0] - totals[1];
        return result;
    }

    static int index = 0;
    public static float FakeGetMaterialDifference(Board board, bool is_white)
    {
        var arr = new[] { 3, 12, 8, 2, 4, 6, 14, 5, 2 };
        return arr[index++];
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