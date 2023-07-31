using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    Random rnd = new();
    const int MAX_INT = int.MaxValue / 2;
    const int MIN_INT = -MAX_INT;

    static readonly int[] pieceValues = {
        100, 320, 330, 500, 900, 0,
        -100, -320, -330, -500, -900, 0
    };


    public Move Think(Board board, Timer timer)
    {
        int depth = 4;

        //Console.WriteLine("Evaluate: " + Evaluate(board));
       
        var allMoves = board.GetLegalMoves();

        if(allMoves.Length == 0)
            throw new Exception("No legal moves found");

        Move bestMove = allMoves[rnd.Next(0,allMoves.Length)];
       
        int bestScore = int.MinValue;

        foreach (var move in allMoves)
        {
            board.MakeMove(move);
            int score = -Negamax(board, depth - 1);      
            board.UndoMove(move);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }
      
        return bestMove;
    }


    int Evaluate(Board board)
    {
        var pieceLists = board.GetAllPieceLists();
        int score = 0;

        for (int i = 0; i < pieceLists.Length; i++)
        {
            score += pieceLists[i].Count * pieceValues[i];
        }

        // If black is to move, reverse the score
        if (!board.IsWhiteToMove)
            score = -score;

        return score;
    }


    int Negamax(Board board, int depth)
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
            int score = -Negamax(board, depth - 1);
            board.UndoMove(move);

            maxScore = Math.Max(score, maxScore);
        }

        return maxScore;
    }
}