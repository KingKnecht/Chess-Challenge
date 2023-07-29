using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
    private Move _bestMove = Move.NullMove;
    private bool PlayerIsWhite;
    private int _moveCount = 0;
    public Move Think(Board board, Timer timer)
    {

        Console.WriteLine("Moves: " + ++_moveCount);

        var maxDepth = 8;
        int depth = maxDepth - 1;
        PlayerIsWhite = board.IsWhiteToMove;

        var result = AlphaBeta(board, depth, maxDepth, true, board.IsWhiteToMove, float.MinValue, float.MaxValue, Move.NullMove);
        return _bestMove;
    }


    private int AlphaBeta(Board board, int depth, int maxDepth, bool isMaximizer, bool isWhite, float alpha, float beta, Move lastMove)
    {

        // If max depth is reached or Game is Over
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw())
        {
            var value = GetPositionValue(board, isWhite, isMaximizer);
            //Console.WriteLine(value.ToString());
            return value;
        }
        
        var orderedMoves = board.GetLegalMoves()
                .OrderByDescending(m => GetPieceValue(m.CapturePieceType))
                .ThenByDescending(m => m.PromotionPieceType)
                .ThenByDescending(m => m.TargetSquare.Name == "e4"
                                       || m.TargetSquare.Name == "e5"
                                       || m.TargetSquare.Name == "d4"
                                       || m.TargetSquare.Name == "d5")
            ;


        if (isMaximizer)
        {
            var hValue = int.MinValue;

            foreach (var move in orderedMoves)
            {

                board.MakeMove(move);
                var value = AlphaBeta(board, depth - 1, maxDepth, !isMaximizer, !isWhite, alpha, beta, move);

                //Console.WriteLine($"{move}: {value}");

                board.UndoMove(move);

                if (hValue < value)
                {
                    hValue = value;

                    // Remember which move gave the highest hValue
                    if (depth == maxDepth - 1)
                    {
                        _bestMove = move;
                        //Console.WriteLine(_bestMove);
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
            int hValue = int.MaxValue;

            foreach (var move in orderedMoves)
            {
                board.MakeMove(move);

                var value = AlphaBeta(board, depth - 1, maxDepth, !isMaximizer, !isWhite, alpha, beta, move);

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

    public int GetPositionValue(Board board, bool isWhite, bool isMaximizer)
    {
        var lastMoveWasWhiteMove = !board.IsWhiteToMove;


        var diff = GetMaterialDifference(board, lastMoveWasWhiteMove);

        if (board.IsDraw())
        {
            //if (lastMoveWasWhiteMove && PlayerIsWhite)
            //{
            //    if (diff < 0) return 10000;
            //    if (diff > 0) return -10000;
            //}
            //else if (!lastMoveWasWhiteMove && !PlayerIsWhite)
            //{
            //    if (diff < 0) return -10000;
            //    if (diff > 0) return 10000;
            //}

            //return  -1000;
        }

        if (board.IsInCheckmate())
        {
            if (lastMoveWasWhiteMove && PlayerIsWhite)
            {
                return 100000;
            }

            if (!lastMoveWasWhiteMove && !PlayerIsWhite)
            {
                return 100000;
            }

            return -100000;
        }

        return diff;
    }

    private int GetMaterialDifference(Board board, bool isWhite)
    {
        int[] totals = { 0, 0 };
        foreach (var list in board.GetAllPieceLists())
        {
            foreach (var piece in list)
            {
                totals[(list.IsWhitePieceList) ? 0 : 1] += GetPieceValue(piece.PieceType);
            }
        }

        var result = isWhite ? totals[0] - totals[1] : totals[1] - totals[0];
        return result;
    }

    public int GetPieceValue(PieceType pieceType)
    {
        switch (pieceType)
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
                return 100;

        }
        return 0;
    }

}