using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static string player;

    public static void Main()
    {
        UdpClient client = new UdpClient();
        IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);

        byte[] joinMessage = Encoding.ASCII.GetBytes("JOIN");
        client.Send(joinMessage, joinMessage.Length, serverEndpoint);

        byte[] data = client.Receive(ref serverEndpoint);
        string response = Encoding.ASCII.GetString(data);
        if (response.StartsWith("PLAYER"))
        {
            player = response.Split(' ')[1];
            Console.WriteLine($"You are player {player}.");
        }

        while (true)
        {
            int row = -1, col = -1;
            bool validInput = false;

            while (!validInput)
            {
                Console.WriteLine("Enter your move (row and column between 0 and 2, separated by a space):");
                string moveInput = Console.ReadLine();

                string[] moveParts = moveInput.Split(' ');
                if (moveParts.Length != 2 ||
                    !int.TryParse(moveParts[0], out row) ||
                    !int.TryParse(moveParts[1], out col))
                {
                    Console.WriteLine("Invalid input. Please enter two numbers separated by a space.");
                    continue;
                }

                if (row < 0 || row > 2 || col < 0 || col > 2)
                {
                    Console.WriteLine("Invalid move. Row and column must be between 0 and 2.");
                    continue;
                }

                validInput = true;
            }

            string moveMessage = $"MOVE {player} {row} {col}";
            byte[] moveData = Encoding.ASCII.GetBytes(moveMessage);
            client.Send(moveData, moveData.Length, serverEndpoint);

            byte[] boardData = client.Receive(ref serverEndpoint);
            string boardResponse = Encoding.ASCII.GetString(boardData);

            if (boardResponse.StartsWith("INVALID"))
            {
                Console.WriteLine("It's not your turn, or the move is invalid. Try again.");
                continue;
            }

            Console.WriteLine(boardResponse);

            if (boardResponse.StartsWith("WIN"))
            {
                string winningPlayer = boardResponse.Split(' ')[1];
                Console.WriteLine($"Player {winningPlayer} wins!");
                break;
            }
            else if (boardResponse.StartsWith("DRAW"))
            {
                Console.WriteLine("The game is a draw!");
                break;
            }
        }
    }
}