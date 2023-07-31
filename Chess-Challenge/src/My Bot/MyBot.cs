using System;
using System.Linq;
using ChessChallenge.API;
using ChessChallenge.Chess;
using Board = ChessChallenge.API.Board;
using Move = ChessChallenge.API.Move;


public class MyBot : IChessBot
{
    private Move _bestMove = Move.NullMove;
    private bool _playerIsWhite;
    private int _moveCount = 0;
    private int _resultsCalculated;
    private Random _rnd;

    public Move Think(Board board, Timer timer)
    {
        _rnd = new Random(DateTime.Now.Millisecond);
        
        _resultsCalculated = 0;

        int maxDepth = 6;

        int depth = maxDepth - 1;
        _playerIsWhite = board.IsWhiteToMove;

        var result = AlphaBeta(board, depth, maxDepth, true, board.IsWhiteToMove, float.MinValue, float.MaxValue);

        Console.WriteLine($"Move #{++_moveCount}");
        Console.WriteLine($"Best {_bestMove}");
        Console.WriteLine($"Value: {result}");
        Console.WriteLine($"Time per move: {timer.MillisecondsElapsedThisTurn / 1000f}s");
        Console.WriteLine($"# Calcs: {_resultsCalculated}");
        Console.WriteLine($"# Calcs/s: {_resultsCalculated / (timer.MillisecondsElapsedThisTurn / 1000f)}");
        Console.WriteLine();

        return _bestMove;
    }


    private int AlphaBeta(Board board, int depth, int maxDepth, bool isMaximizer, bool isWhite, float alpha, float beta)
    {

        // If max depth is reached or Game is Over
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw())
        {
            var value = GetPositionValue(board, isWhite, isMaximizer);
            _resultsCalculated++;
            return value;
        }

        var orderedMoves = board.GetLegalMoves()
                .OrderByDescending(m =>
                {
                    board.MakeMove(m);
                    var isInCheck = board.IsInCheck();
                    board.UndoMove(m);

                    return isInCheck;
                })
                .ThenByDescending(m => MoveValue(m.MovePieceType, m.CapturePieceType))
                .ThenByDescending(m => m.IsPromotion)
                .ThenByDescending(m => m.IsCastles)
                .ThenBy(m => DevelopPiece(m.MovePieceType, board))
                .ThenByDescending(m => m.TargetSquare.Index == 27)
                .ThenByDescending(m => m.TargetSquare.Index == 28)
                .ThenByDescending(m => m.TargetSquare.Index == 20)
                .ThenByDescending(m => m.TargetSquare.Index == 35)
                .ThenByDescending(m => m.TargetSquare.Index == 36)
                .ThenByDescending(m => m.TargetSquare.Index == 44)
            ;


        if (isMaximizer)
        {
            var best = int.MinValue;

            foreach (var move in orderedMoves)
            {
                board.MakeMove(move);

                _pieceCount = BitboardHelper.GetNumberOfSetBits(board.AllPiecesBitboard);//board.GetAllPieceLists().SelectMany(l => l).Count();
               
                var isInterestingMove = false;
                if (board.IsInCheck() && maxDepth < 8)
                {
                    isInterestingMove = true;
                    maxDepth += 2;
                    depth += 2;
                }

                var value = AlphaBeta(board, depth - 1, maxDepth, !isMaximizer, !isWhite, alpha, beta);

                //Console.WriteLine($"{move}: {value}");

                board.UndoMove(move);
                
                if (isInterestingMove)
                {
                    maxDepth -= 2;
                    depth -= 2;
                }

                if (best < value)
                {
                    best = value;

                    // Remember which move gave the highest hValue
                    if (depth == maxDepth - 1)
                    {
                        _bestMove = move;
                        //Console.WriteLine(_bestMove);
                    }
                }

                if (best > alpha)
                    alpha = best;

                if (beta <= alpha)
                    break;
            }

            return best;
        }
        else
        {
            var best = int.MaxValue;

            foreach (var move in orderedMoves)
            {
                board.MakeMove(move);

                var value = AlphaBeta(board, depth - 1, maxDepth, !isMaximizer, !isWhite, alpha, beta);

                board.UndoMove(move);

                if (best > value)
                    best = value;

                if (best < beta)
                    beta = best;

                if (beta <= alpha)
                    break;
            }

            return best;
        }
    }

    
    private int DevelopPiece(PieceType movePieceType, Board board)
    {
        //return 0;
        switch (movePieceType)
        {
            case PieceType.None:
                return 0;
            case PieceType.Pawn:
                return board.PlyCount < 4 ? 0 : _rnd.Next(_pieceCount < 10 ? 0 : 50, 100);
            case PieceType.Knight:
                return board.PlyCount > 6 ? 10 : 80;
            case PieceType.Bishop:
                return board.PlyCount > 6 ? 20 : 90;
            case PieceType.Rook:
                return board.PlyCount > 20 ? 0 : 100;
            case PieceType.Queen:
                return board.PlyCount > 20 ? 0 : 100;
            case PieceType.King:
                return board.PlyCount > 60 ? 0 : 1000;
            default:
                throw new ArgumentOutOfRangeException(nameof(movePieceType), movePieceType, null);
        }
    }

    private int MoveValue(PieceType movePieceType, PieceType capturePieceType)
    {
        return (int)((10000f / (float)movePieceType) * (float)capturePieceType);
    }

    public int GetPositionValue(Board board, bool isWhite, bool isMaximizer)
    {
        var lastMoveWasWhiteMove = !board.IsWhiteToMove;

        var diff = GetMaterialDifference(board, lastMoveWasWhiteMove);

        if (board.IsDraw())
        {
            //return diff < 0 
            //    ? 10000 + 10000 / board.PlyCount 
            //    : -10000 - (10000 / board.PlyCount);
            return -10000 - (10000 / board.PlyCount);
        }

        if (board.IsInCheckmate())
        {
            if (lastMoveWasWhiteMove && _playerIsWhite)
            {
                return 100000 + 10000 / board.PlyCount;
            }

            if (!lastMoveWasWhiteMove && !_playerIsWhite)
            {
                return 100000 + 10000 / board.PlyCount;
            }

            if (board.PlyCount == 0) return -100000;

            return -100000 - (10000 / board.PlyCount);
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

    // Piece values: null, pawn, knight, bishop, rook, queen, king
    static readonly int[] PieceValues = { 0, 1, 3, 3, 5, 9, 10 };
    private int _pieceCount;

    public static int GetPieceValue(PieceType pieceType)
    {
        return PieceValues[(int)pieceType];
    }

}