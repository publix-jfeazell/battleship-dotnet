
namespace Battleship.Ascii
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Numerics;
    using Battleship.Ascii.TelemetryClient;
    using Battleship.GameController;
    using Battleship.GameController.Contracts;

    public class Program
    {
        private static List<Ship> myFleet;

        private static List<Ship> enemyFleet;

        private static ITelemetryClient telemetryClient;
        private static ConsoleColor defaultColor = ConsoleColor.White;
        private static bool IsGameOver = false;
        public static int PlayerWon = 0;

        static void Main()
        {
            telemetryClient = new ApplicationInsightsTelemetryClient();
            telemetryClient.TrackEvent("ApplicationStarted", new Dictionary<string, string> { { "Technology", ".NET"} });

            try
            {
                Console.Title = "Battleship";
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Clear();

                Console.WriteLine("                                     |__");
                Console.WriteLine(@"                                     |\/");
                Console.WriteLine("                                     ---");
                Console.WriteLine("                                     / | [");
                Console.WriteLine("                              !      | |||");
                Console.WriteLine("                            _/|     _/|-++'");
                Console.WriteLine("                        +  +--|    |--|--|_ |-");
                Console.WriteLine(@"                     { /|__|  |/\__|  |--- |||__/");
                Console.WriteLine(@"                    +---------------___[}-_===_.'____                 /\");
                Console.WriteLine(@"                ____`-' ||___-{]_| _[}-  |     |_[___\==--            \/   _");
                Console.WriteLine(@" __..._____--==/___]_|__|_____________________________[___\==--____,------' .7");
                Console.WriteLine(@"|                        Welcome to Battleship                         BB-61/");
                Console.WriteLine(@" \_________________________________________________________________________|");
                Console.WriteLine();

                InitializeGame();

                StartGame();
            }
            catch (Exception e)
            {
                Console.WriteLine("A serious problem occured. The application cannot continue and will be closed.");
                telemetryClient.TrackException(e);
                Console.WriteLine("");
                Console.WriteLine("Error details:");      
                throw new Exception("Fatal error", e);
            }

        }

        private static void StartGame()
        {
            Console.Clear();
            Console.WriteLine("                  __");
            Console.WriteLine(@"                 /  \");
            Console.WriteLine("           .-.  |    |");
            Console.WriteLine(@"   *    _.-'  \  \__/");
            Console.WriteLine(@"    \.-'       \");
            Console.WriteLine("   /          _/");
            Console.WriteLine(@"  |      _  /""");
            Console.WriteLine(@"  |     /_\'");
            Console.WriteLine(@"   \    \_/");
            Console.WriteLine(@"    """"""""");
            Console.Clear();

            do
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("--------------- PLAYER ONE TURN ---------------");
                IsGameOver = PlayerOneTurn(out int playerWon);
                Console.ForegroundColor = ConsoleColor.Magenta;
                if (!IsGameOver)
                {
                    Console.WriteLine("--------------- PLAYER TWO TURN ---------------");
                    IsGameOver = PlayerTwoTurn();
                }
            }
            while (!IsGameOver);

            if (PlayerWon == 1)
            {
                int coinsWon = new Random().Next(1, 5);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"In recognition of your extreme competence, you have earned {coinsWon} gold coins for this battle.");
            }

            Console.ForegroundColor = defaultColor;
            Console.WriteLine("Game is Over!");
            Console.WriteLine("Do you want to play again? Y or N");
            var answer = "";
            do
            {
                answer = Console.ReadLine();
                try
                {
                    if (answer.ToUpper() == "Y")
                    {
                        Console.Clear();
                        InitializeGame();
                        StartGame();
                    }
                    else if (answer.ToUpper() == "N")
                    {
                        Console.WriteLine("Thank you for playing!");
                        break;
                    }
                }
                catch
                {
                    Console.WriteLine("Not a valid answer please enter Y or N.");
                    answer = "";
                }

            }
            while (String.IsNullOrEmpty(answer));

        }

        public static Position ParsePosition(string input)
        {
            var letter = (Letters)Enum.Parse(typeof(Letters), input.ToUpper().Substring(0, 1));
            var number = int.Parse(input.Substring(1, 1));
            return new Position(letter, number);
        }

        private static Position GetRandomPosition()
        {
            int rows = 8;
            int lines = 8;
            var random = new Random();
            var letter = (Letters)random.Next(lines);
            var number = random.Next(rows);
            var position = new Position(letter, number);
            return position;
        }

        private static void InitializeGame()
        {
            InitializeMyFleet();
            InitializeEnemyFleet();
        }

        private static bool PlayerOneTurn(out int playerWon)
        {
            Console.ForegroundColor = defaultColor;
            Console.WriteLine();
            Console.WriteLine("Enter coordinates for your shot :");
            var position = new Position();
            do
            {
                try
                {
                    var textEntered = Console.ReadLine();
                    if (textEntered.ToUpper() == "STATUS")
                    {
                        DisplayFleetStatus(myFleet, enemyFleet);
                        Console.WriteLine("Enter coordinates for your shot :");
                        textEntered = Console.ReadLine();
                    }
                    position = ParsePosition(textEntered);
                    break;
                }
                catch
                {
                    DisplayErrorMessage("Invalid Position Please try again!");
                }
            }
            while (true);

            var isHit = GameController.CheckIsHit(enemyFleet, position);

            telemetryClient.TrackEvent("Player_ShootPosition", new Dictionary<string, string>() { { "Position", position.ToString() }, { "IsHit", isHit.ToString() } });
            if (isHit)
            {
                Console.Beep();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Yeah ! Nice hit !");
                Console.WriteLine();
                foreach (var ship in enemyFleet.Where(x => x.IsSunk == false))
                {
                    if (GameController.CheckIsSunk(ship))
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        DisplaySunkShipArt(ConsoleColor.Blue);
                        //Console.WriteLine(@"                \         .  ./");
                        //Console.WriteLine(@"              \      .:"";'.:..""   /");
                        //Console.WriteLine(@"                  (M^^.^~~:.'"").");
                        //Console.WriteLine(@"            -   (/  .    . . \ \)  -");
                        //Console.WriteLine(@"               ((| :. ~ ^  :. .|))");
                        //Console.WriteLine(@"            -   (\- |  \ /  |  /)  -");
                        //Console.WriteLine(@"                 -\  \     /  /-");
                        //Console.WriteLine(@"                   \  \   /  /");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"You sunk your enemy's {ship.Name}");
                        Console.ForegroundColor = defaultColor;
                    }
                }
            }
            else
            {
                DisplayErrorMessage("Miss!");
            }
            Console.WriteLine();

            return GameController.CheckGameOver(myFleet, enemyFleet, out playerWon);
        }

        private static bool PlayerTwoTurn()
        {
            Console.ForegroundColor = defaultColor;
            var position = GetRandomPosition();

            var isHit = GameController.CheckIsHit(myFleet, position);

            telemetryClient.TrackEvent("Computer_ShootPosition", new Dictionary<string, string>() { { "Position", position.ToString() }, { "IsHit", isHit.ToString() } });
            Console.WriteLine();
            Console.WriteLine("Computer shot in {0}{1} and {2}", position.Column, position.Row, isHit ? "has hit your ship !" : "missed");
            if (isHit)
            {
                Console.Beep();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("You were Hit!");
                Console.WriteLine();
                foreach (var ship in myFleet.Where(x => x.IsSunk == false))
                {
                    if (GameController.CheckIsSunk(ship))
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        DisplaySunkShipArt(ConsoleColor.Magenta);
                        //Console.WriteLine(@"                \         .  ./");
                        //Console.WriteLine(@"              \      .:"";'.:..""   /");
                        //Console.WriteLine(@"                  (M^^.^~~:.'"").");
                        //Console.WriteLine(@"            -   (/  .    . . \ \)  -");
                        //Console.WriteLine(@"               ((| :. ~ ^  :. .|))");
                        //Console.WriteLine(@"            -   (\- |  \ /  |  /)  -");
                        //Console.WriteLine(@"                 -\  \     /  /-");
                        //Console.WriteLine(@"                   \  \   /  /");
                        DisplayErrorMessage($"The enemy sunk your {ship.Name}");
                    }
                }
            }
            else
            {
                DisplaySucessMessage("Miss!");
            }
            Console.WriteLine();

            return GameController.CheckGameOver(myFleet, enemyFleet);
        }

        private static void InitializeMyFleet()
        {
            myFleet = GameController.InitializeShips().ToList();

            Console.WriteLine("Please position your fleet (Game board size is from A to H and 1 to 8) :");

            foreach (var ship in myFleet)
            {
                Console.WriteLine();
                Console.WriteLine("Please enter the positions for the {0} (size: {1})", ship.Name, ship.Size);
                var position = "";
                do
                {
                    Console.WriteLine("Enter the coordinates for your ship placements (e.g., A1 A2 A3):");
                    string input = Console.ReadLine();

                    input = input.Trim();
                    // Split the input into individual coordinates
                    string[] coordinates = input.Split(' ');

                    // Validate and store the coordinates
                    if (AreCoordinatesValid(coordinates, ship.Size))
                    {
                        ship.AddPosition(coordinates);
                        break;
                    }
                    else
                    {
                        DisplayErrorMessage("The coordinates are invalid or do not form a line.");
                    }
                }
                while (true);
                //telemetryClient.TrackEvent("Player_PlaceShipPosition", new Dictionary<string, string>() { { "Position", position }, { "Ship", ship.Name }, { "PositionInShip", i.ToString() } });
            }
        }

        private static bool AreCoordinatesValid(string[] coordinates, int shipSize)
        {
            if (coordinates.Length < shipSize)
            {
                DisplayErrorMessage($"Those coordinates do not meet the ship length of {shipSize}");
                return false; // A ship must have at least two coordinates
            }

            List<(int row, int col)> parsedCoordinates = new List<(int, int)>();

            foreach (string coordinate in coordinates)
            {
                if (!IsValidCoordinate(coordinate, out int row, out int col))
                {
                    return false;
                }
                parsedCoordinates.Add((row, col));
            }

            // Check if all coordinates are in a line
            bool isHorizontal = parsedCoordinates.All(c => c.row == parsedCoordinates[0].row);
            bool isVertical = parsedCoordinates.All(c => c.col == parsedCoordinates[0].col);

            if (!isHorizontal && !isVertical)
            {
                DisplayErrorMessage("Those coordinates do not form a line.");
                return false; // Not in a line
            }

            // Check if all coordinates are adjacent
            parsedCoordinates = parsedCoordinates.OrderBy(c => isHorizontal ? c.col : c.row).ToList();

            for (int i = 1; i < parsedCoordinates.Count; i++)
            {
                if (isHorizontal && parsedCoordinates[i].col != parsedCoordinates[i - 1].col + 1)
                {
                    DisplayErrorMessage("Those coordinates are not adjacent horizontally.");
                    return false; // Not adjacent horizontally
                }
                if (isVertical && parsedCoordinates[i].row != parsedCoordinates[i - 1].row + 1)
                {
                    DisplayErrorMessage("Those coordinates are not adjacent vertically.");
                    return false; // Not adjacent vertically
                }
            }

            return true;
        }

        private static void DisplaySucessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = defaultColor;
        }

        private static void DisplayErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = defaultColor;
        }

        private static void DisplaySunkShipArt(ConsoleColor color)
        {
            Console.ForegroundColor = defaultColor;
            Console.WriteLine("                                            |__");
            Console.WriteLine(@"                                           |\/"); Console.ForegroundColor = color;
            Console.Write("    \\         .  ./"); Console.ForegroundColor = defaultColor; Console.WriteLine("                        ---"); Console.ForegroundColor = color;
            Console.Write("\\      .:\";'.:..\"   /"); Console.ForegroundColor = defaultColor; Console.WriteLine("                     / | ["); Console.ForegroundColor = color;
            Console.Write("       (M^^.^~~:.'\")."); Console.ForegroundColor = defaultColor; Console.WriteLine("               !      | |||"); Console.ForegroundColor = color;
            Console.Write("-   (/  .    . . \\ \\)  -"); Console.ForegroundColor = defaultColor; Console.WriteLine("         _/|     _/|-++'"); Console.ForegroundColor = color;
            Console.Write("    ((| :. ~ ^  :. .|))"); Console.ForegroundColor = defaultColor; Console.WriteLine("        +  +--|    |--|--|_ |-"); Console.ForegroundColor = color;
            Console.Write(@"-   (\- |  \ /  |  /)  -"); Console.ForegroundColor = defaultColor; Console.WriteLine(@"       { /|__|  |/\__|  |--- |||__/"); Console.ForegroundColor = color;
            Console.Write(@"     -\  \     /  /-"); Console.ForegroundColor = defaultColor; Console.WriteLine(@"  +---------------___[}-_===_.'____                 /\"); Console.ForegroundColor = color;
            Console.Write(@"       \  \   /  /"); Console.ForegroundColor = defaultColor; Console.WriteLine(@" ___`-' ||___-{]_| _[}-  |     |_[___\==--            \/   _");
            Console.WriteLine(@" ____..._____--==/___]_|__|_____________________________[___\==--____,------' .7");
            Console.WriteLine(@"|                                                                      BB-61/");
            Console.WriteLine(@" \___________________________________________________________________________|");
        }

        private static bool IsValidCoordinate(string coordinate, out int row, out int col)
        {
            row = -1;
            col = -1;

            if (coordinate.Length != 2)
            {
                return false;
            }

            char rowChar = Char.ToUpper(coordinate[0]);
            char colChar = coordinate[1];

            // Check if the row is between A and H
            if (rowChar < 'A' || rowChar > 'H')
            {
                return false;
            }

            // Check if the column is between 1 and 8
            if (colChar < '1' || colChar > '8')
            {
                return false;
            }

            row = rowChar - 'A'; // Convert row to 0-based index
            col = colChar - '1'; // Convert column to 0-based index

            return true;
        }

        private static void InitializeEnemyFleet()
        {
            enemyFleet = GameController.InitializeShips().ToList();

            enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 4 });
            enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 5 });
            enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 6 });
            enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 7 });
            enemyFleet[0].Positions.Add(new Position { Column = Letters.B, Row = 8 });

            enemyFleet[1].Positions.Add(new Position { Column = Letters.E, Row = 6 });
            enemyFleet[1].Positions.Add(new Position { Column = Letters.E, Row = 7 });
            enemyFleet[1].Positions.Add(new Position { Column = Letters.E, Row = 8 });
            enemyFleet[1].Positions.Add(new Position { Column = Letters.E, Row = 9 });

            enemyFleet[2].Positions.Add(new Position { Column = Letters.A, Row = 3 });
            enemyFleet[2].Positions.Add(new Position { Column = Letters.B, Row = 3 });
            enemyFleet[2].Positions.Add(new Position { Column = Letters.C, Row = 3 });

            enemyFleet[3].Positions.Add(new Position { Column = Letters.F, Row = 8 });
            enemyFleet[3].Positions.Add(new Position { Column = Letters.G, Row = 8 });
            enemyFleet[3].Positions.Add(new Position { Column = Letters.H, Row = 8 });

            enemyFleet[4].Positions.Add(new Position { Column = Letters.C, Row = 5 });
            enemyFleet[4].Positions.Add(new Position { Column = Letters.C, Row = 6 });
        }

        private static void DisplayFleetStatus(IEnumerable<Ship> playerOneFleet, IEnumerable<Ship> playerTwoFleet)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("---------------PLAYER ONE FLEET--------------");
            Console.ForegroundColor = defaultColor;
            foreach (Ship ship in playerOneFleet)
            {
                Console.Write($"Is {ship.Name} Sunk:");
                Console.ForegroundColor = ship.IsSunk ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($" {ship.IsSunk}");
                Console.ForegroundColor = defaultColor;
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("---------------PLAYER TWO FLEET--------------");
            Console.ForegroundColor = defaultColor;
            foreach (Ship ship in playerTwoFleet)
            {
                Console.Write($"Is {ship.Name} Sunk:");
                Console.ForegroundColor = ship.IsSunk ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($" {ship.IsSunk}");
                Console.ForegroundColor = defaultColor;
            }
            Console.WriteLine();
        }
    }
}
