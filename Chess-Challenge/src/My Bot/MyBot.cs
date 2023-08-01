using ChessChallenge.API;
using System;
using System.Diagnostics;

public class MyBot : IChessBot
{
    static int[,] pieceSquareTables =
    {
    // White Pawn
    {
        0,  0,  0,  0,  0,  0,  0,  0,
        50, 50, 50, 50, 50, 50, 50, 50,
        10, 10, 20, 30, 30, 20, 10, 10,
        5,  5, 10, 25, 25, 10,  5,  5,
        0,  0,  0, 20, 20,  0,  0,  0,
        5, -5,-10,  0,  0,-10, -5,  5,
        5, 10, 10,-20,-20, 10, 10,  5,
        0,  0,  0,  0,  0,  0,  0,  0
    },

    // White Knight
    {
       -20, -10,  -10,  -10,  -10,  -10,  -10,  -20,
    -10,  -5,   -5,   -5,   -5,   -5,   -5,  -10,
    -10,  -5,   15,   15,   15,   15,   -5,  -10,
    -10,  -5,   15,   15,   15,   15,   -5,  -10,
    -10,  -5,   15,   15,   15,   15,   -5,  -10,
    -10,  -5,   10,   15,   15,   15,   -5,  -10,
    -10,  -5,   -5,   -5,   -5,   -5,   -5,  -10,
    -20,   0,  -10,  -10,  -10,  -10,    0,  -20
    },

    // White Bishop
    {
       -20,-10,-10,-10,-10,-10,-10,-20,
       -10,  0,  0,  0,  0,  0,  0,-10,
       -10,  0,  5, 10, 10,  5,  0,-10,
       -10,  5,  5, 10, 10,  5,  5,-10,
       -10,  0, 10, 10, 10, 10,  0,-10,
       -10, 10, 10, 10, 10, 10, 10,-10,
       -10,  5,  0,  0,  0,  0,  5,-10,
       -20,-10,-10,-10,-10,-10,-10,-20,
    },

    // White Rook
    {
          0,   0,   0,   0,   0,   0,   0,   0,
           15,  15,  15,  20,  20,  15,  15,  15,
            0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,  10,  10,  10,   0,   0
    },

    // White Queen
    {
       -20,-10,-10, -5, -5,-10,-10,-20,
       -10,  0,  0,  0,  0,  0,  0,-10,
       -10,  0,  5,  5,  5,  5,  0,-10,
        -5,  0,  5,  5,  5,  5,  0, -5,
         0,  0,  5,  5,  5,  5,  0, -5,
       -10,  5,  5,  5,  5,  5,  0,-10,
       -10,  0,  5,  0,  0,  0,  0,-10,
       -20,-10,-10, -5, -5,-10,-10,-20
    },

    // White King (Middle game)
    {
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -20,-30,-30,-40,-40,-30,-30,-20,
        -10,-20,-20,-20,-20,-20,-20,-10,
         20, 20,  0,  0,  0,  0, 20, 20,
         20, 30, 10,  0,  0, 10, 30, 20
    },
    };


    Random rnd = new(100);
    const int MAX_INT = int.MaxValue / 2;
    const int MIN_INT = -MAX_INT;

    static readonly int[] pieceValues = {
        100, 320, 330, 500, 800, 0
    };

    public Move Think(Board board, Timer timer)
    {
        int depth = 5;
        int alpha = MIN_INT;
        int beta = MAX_INT;

        var allMoves = board.GetLegalMoves();

        if (allMoves.Length == 0)
            throw new Exception("No legal moves found");

        Move bestMove = allMoves[rnd.Next(0, allMoves.Length)];

        int bestScore = MIN_INT;

        foreach (var move in allMoves)
        {
            board.MakeMove(move);
            int score = -Negamax(board, depth - 1, -beta, -alpha);
            board.UndoMove(move);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }

            alpha = Math.Max(alpha, score);
        }

        return bestMove;
    }

    int EvaluateSide(Board board,PieceList[] pieces, bool isWhite)
    {
        int score = 0;

        for (int i = 0; i < 6; i++)
        {
            //material score
            int count = pieces[isWhite ? i : i + 6].Count;
            score += count * pieceValues[i];

            //position score
            var bitboard = board.GetPieceBitboard((PieceType)(i + 1), isWhite);

            while (bitboard > 0)
            {
                int pos = BitboardHelper.ClearAndGetIndexOfLSB(ref bitboard);
                score += isWhite ? pieceSquareTables[i, pos] : pieceSquareTables[i, 63 - pos];
            }
        }

        return score;
    }

    int Evaluate(Board board)
    {
        PieceList[] pieceLists = board.GetAllPieceLists();
        Debug.Assert(pieceLists.Length == 12,"piece list too short");
        int score = 0;

        score += EvaluateSide(board,pieceLists, true);
        score -= EvaluateSide(board,pieceLists, false);

        // If black is to move, reverse the score
        if (!board.IsWhiteToMove)
            score = -score;

        return score;
    }

    int Negamax(Board board, int depth, int alpha, int beta)
    {
        if (board.IsInCheckmate())
            return MIN_INT - depth;
        if (board.IsDraw())
            return 0;
        if (depth == 0)
            return Evaluate(board);

        int maxScore = MIN_INT;
        foreach (var move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int score = -Negamax(board, depth - 1, -beta, -alpha);
            board.UndoMove(move);

            maxScore = Math.Max(score, maxScore);

            alpha = Math.Max(alpha, score);
            if (alpha >= beta)
                break;
        }

        return maxScore;
    }
}