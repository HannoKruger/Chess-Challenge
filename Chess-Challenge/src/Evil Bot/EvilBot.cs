using ChessChallenge.API;
using System;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        Random rnd = new(100);
        const int MAX_INT = int.MaxValue / 2;
        const int MIN_INT = -MAX_INT;

        static readonly int[] pieceValues = {
        100, 320, 330, 500, 800, 0,
        -100, -320, -330, -500, -800, 0
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

        int Evaluate(Board board)
        {
            var pieceLists = board.GetAllPieceLists();
            int score = 0;

            for (int i = 0; i < pieceLists.Length; i++)
            {
                //material
                int count = pieceLists[i].Count;
                score += count * pieceValues[i];

                //position
                //for(int)
                //bool isWhite = i < 6;
                //board.GetPieceBitboard(PieceType.Pawn,)

                //pieceLists[i]

            }


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
}