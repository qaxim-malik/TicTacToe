using Microsoft.JSInterop;

namespace TicTacToe.Components.Pages
{
    public partial class TicTacToeBoard
    {
        private enum GameStatus
        {
            InProgress,
            Finished,
            Concluded
        }

        private string[] board;
        private string currentPlayer;
        private string winner;
        private GameStatus gameStatus;
        private int selectedBoardSize = 3; // Default board size
        private int xWinsCount = 0;
        private int oWinsCount = 0;
        private bool boardSizeSelected = false;

        public void InitializeGame()
        {
            board = new string[selectedBoardSize * selectedBoardSize];
            currentPlayer = "X";
            gameStatus = GameStatus.InProgress;
            boardSizeSelected = true;
        }

        public async Task MakeMove(int row, int col)
        {
            try
            {
                // Check for game over or invalid moves
                if (gameStatus != GameStatus.InProgress || board[row * selectedBoardSize + col] != null)
                {
                    await JSRuntime.InvokeAsync<bool>("confirm", "Invalid move, please try again.");
                    return;
                }

                int index = row * selectedBoardSize + col;
                board[index] = currentPlayer;
                if (CheckWin(row, col))
                {
                    winner = currentPlayer;
                    UpdateWinsCountAndResetBoard();
                    if (await JSRuntime.InvokeAsync<bool>("confirm", $"Player {winner} won!!! Play again?"))
                    {
                        gameStatus = GameStatus.InProgress;
                    }
                    else
                    {
                        gameStatus = GameStatus.Concluded;
                        currentPlayer = "X";
                        StateHasChanged();
                    }
                }
                else if (board.All(cell => cell != null))
                {
                    await JSRuntime.InvokeAsync<bool>("confirm", "Game tied!!!");
                    gameStatus = GameStatus.InProgress;
                    currentPlayer = "X";
                    board = new string[selectedBoardSize * selectedBoardSize];
                }
                else
                {
                    currentPlayer = currentPlayer == "X" ? "O" : "X";
                }
            }
            catch (Exception ex)
            {
                await JSRuntime.InvokeAsync<bool>("confirm", "Game crashed due to unknown reason!!!");
                throw;
            }
        }

        public bool CheckWin(int row, int col)
        {
            // Check row, column, and diagonals for a win
            return WonByMatchingRow(row) || WonByMatchingColumn(col) || WonByMatchingDiagonals();
        }

        public bool WonByMatchingRow(int row)
        {
            for (int i = 0; i < selectedBoardSize; i++)
            {
                if (board[row * selectedBoardSize + i] != currentPlayer)
                    return false;
            }
            return true;
        }

        public bool WonByMatchingColumn(int col)
        {
            for (int i = 0; i < selectedBoardSize; i++)
            {
                if (board[i * selectedBoardSize + col] != currentPlayer)
                    return false;
            }
            return true;
        }

        public bool WonByMatchingDiagonals()
        {
            bool topLeftToBottomRight = true;
            bool topRightToBottomLeft = true;

            for (int i = 0; i < selectedBoardSize; i++)
            {
                if (board[i * selectedBoardSize + i] != currentPlayer)
                    topLeftToBottomRight = false;

                if (board[i * selectedBoardSize + (selectedBoardSize - 1 - i)] != currentPlayer)
                    topRightToBottomLeft = false;
            }

            return topLeftToBottomRight || topRightToBottomLeft;
        }

        public void UpdateWinsCountAndResetBoard()
        {
            if (winner == "X")
                xWinsCount++;
            else if (winner == "O")
                oWinsCount++;
            board = new string[selectedBoardSize * selectedBoardSize];
        }

        public async Task ResetGame()
        {
            if ((gameStatus == GameStatus.Concluded) && await ConfirmReset())
            {
                boardSizeSelected = false;
                xWinsCount = 0;
                oWinsCount = 0;
            }
        }

        public async Task<bool> ConfirmReset()
        {
            return await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to reset the game?");
        }

        public async Task AbandonGame()
        {
            if (await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to abandon current game?"))
            {
                gameStatus = GameStatus.Concluded;
            }
        }
    }
}