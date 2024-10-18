using System.Net.Sockets;
using System.Net;
using System.Text;
class Program
{
    private static string[,] gameBoard = new string[3, 3];
    private static string currentPlayer = "X";
    private static int movesCount = 0;

    public static void Main()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                gameBoard[i, j] = " ";
            }
        }

        UdpClient server = new UdpClient(8888);
        IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);

        Console.WriteLine("Waiting for players...");

        while (true)
        {
            byte[] data = server.Receive(ref clientEndpoint);
            string moveMessage = Encoding.ASCII.GetString(data);
            Console.WriteLine($"Received: {moveMessage}");

            if (moveMessage.StartsWith("JOIN"))
            {
                string assignedPlayer = currentPlayer;
                server.Send(Encoding.ASCII.GetBytes($"PLAYER {currentPlayer}"), clientEndpoint);
                Console.WriteLine($"Player {currentPlayer} has joined.");

                currentPlayer = (currentPlayer == "X") ? "O" : "X";
            }
            else if (moveMessage.StartsWith("MOVE"))
            {
                string[] parts = moveMessage.Split(' ');
                string player = parts[1];
                int row = int.Parse(parts[2]);
                int col = int.Parse(parts[3]);

                if (gameBoard[row, col] == " ")
                {
                    if (player == currentPlayer)
                    {
                        gameBoard[row, col] = player;
                        movesCount++;

                        string boardState = GetGameBoard();
                        server.Send(Encoding.ASCII.GetBytes($"BOARD\n{boardState}"), clientEndpoint);

                        Console.WriteLine($"Player {player} moved to {row},{col}.");
                        Console.WriteLine(boardState);

                        if (CheckWin(player))
                        {
                            Console.WriteLine($"Player {player} wins!");
                            server.Send(Encoding.ASCII.GetBytes($"WIN {player}"), clientEndpoint);
                            ResetGame();
                        }
                        else if (movesCount == 9)
                        {
                            Console.WriteLine("The game is a draw.");
                            server.Send(Encoding.ASCII.GetBytes("DRAW"), clientEndpoint);
                            ResetGame();
                        }

                        currentPlayer = (currentPlayer == "X") ? "O" : "X";
                    }
                    else
                    {
                        server.Send(Encoding.ASCII.GetBytes($"INVALID {player}"), clientEndpoint);
                        Console.WriteLine($"It's not player {player}'s turn.");
                    }
                }
            }
        }
    }

    static string GetGameBoard()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                sb.Append(gameBoard[i, j]);
                if (j < 2) sb.Append("|");
            }
            if (i < 2) sb.Append("\n-----\n");
        }
        return sb.ToString();
    }

    static bool CheckWin(string player)
    {
        for (int i = 0; i < 3; i++)
        {
            if (gameBoard[i, 0] == player && gameBoard[i, 1] == player && gameBoard[i, 2] == player)
                return true;
            if (gameBoard[0, i] == player && gameBoard[1, i] == player && gameBoard[2, i] == player)
                return true;
        }
        if (gameBoard[0, 0] == player && gameBoard[1, 1] == player && gameBoard[2, 2] == player)
            return true;
        if (gameBoard[0, 2] == player && gameBoard[1, 1] == player && gameBoard[2, 0] == player)
            return true;

        return false;
    }

    static void ResetGame()
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                gameBoard[i, j] = " ";
        movesCount = 0;
        currentPlayer = "X";
    }
}