using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    Random rnd = new();
    const int MAX_INT = int.MaxValue / 2;
    const int MIN_INT = -MAX_INT;

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
        //if (bestMove == allMoves[0])
         //   Console.WriteLine("fallback move");

        return bestMove;
    }


    int Evaluate(Board board)
    {
        var pieceLists = board.GetAllPieceLists();

        int score = 0;

        // White pieces
        score += pieceLists[0].Count * 100;  // White Pawns
        score += pieceLists[1].Count * 320;  // White Knights
        score += pieceLists[2].Count * 330;  // White Bishops
        score += pieceLists[3].Count * 500;  // White Rooks
        score += pieceLists[4].Count * 900;  // White Queens                                  

        // Black pieces
        score -= pieceLists[6].Count * 100;  // Black Pawns
        score -= pieceLists[7].Count * 320;  // Black Knights
        score -= pieceLists[8].Count * 330;  // Black Bishops
        score -= pieceLists[9].Count * 500;  // Black Rooks
        score -= pieceLists[10].Count * 900; // Black Queens

        // If black is to move, reverse the score
        if (!board.IsWhiteToMove)  
            score = -score;
        
        return score;
    }


    public int Negamax(Board board, int depth)
    {     
        if (board.IsInCheckmate())
        {
            // return board.IsWhiteToMove ? maxScore + depth : -maxScore - depth;
            //Console.WriteLine("mate: " + depth);
            //Console.WriteLine("returning: "+MAX_INT+depth);

            return MIN_INT - depth;
        }
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