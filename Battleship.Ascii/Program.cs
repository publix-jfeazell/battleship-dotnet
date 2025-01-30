
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
                IsGameOver = PlayerOneTurn();
                Console.ForegroundColor = ConsoleColor.Magenta;
                if (!IsGameOver)
                {
                    Console.WriteLine("--------------- PLAYER TWO TURN ---------------");
                    IsGameOver = PlayerTwoTurn();
                }
            }
            while (!IsGameOver);
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

        private static bool PlayerOneTurn()
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

            return GameController.CheckGameOver(myFleet, enemyFleet);
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

            var coords = GenerateNonOverlappingCoordinates();

            foreach (var ship in enemyFleet)
            {
                var coordsToAdd = coords.Where(x => x.Count == ship.Size).First();
                coords = coords.Where(x => x != coordsToAdd).ToList();
                var newCoords = coordsToAdd.ToString();

                foreach(var pos in coordsToAdd)
                {
                    var letter = (Letters)Enum.Parse(typeof(Letters), pos.ToUpper().Substring(0, 1));
                    var number = int.Parse(pos.Substring(1, 1));
                    ship.Positions.Add(new Position { Column = letter, Row = number, IsHit = false });
                }
                //telemetryClient.TrackEvent("Player_PlaceShipPosition", new Dictionary<string, string>() { { "Position", position }, { "Ship", ship.Name }, { "PositionInShip", i.ToString() } });
            }
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

        static string GenerateRandomCoordinates(int numberOfCoordinates)
        {
            Random random = new Random();
            List<string> coordinates = new List<string>();
            char[] rows = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
            int[] columns = { 1, 2, 3, 4, 5, 6, 7, 8 };

            bool isHorizontal = random.Next(2) == 0; // Randomly choose horizontal or vertical

            if (isHorizontal)
            {
                // Generate horizontal line
                int rowIndex = random.Next(rows.Length);
                int startColumnIndex = random.Next(columns.Length - numberOfCoordinates + 1);

                for (int i = 0; i < numberOfCoordinates; i++)
                {
                    coordinates.Add($"{rows[rowIndex]}{columns[startColumnIndex + i]}");
                }
            }
            else
            {
                // Generate vertical line
                int columnIndex = random.Next(columns.Length);
                int startRowIndex = random.Next(rows.Length - numberOfCoordinates + 1);

                for (int i = 0; i < numberOfCoordinates; i++)
                {
                    coordinates.Add($"{rows[startRowIndex + i]}{columns[columnIndex]}");
                }
            }

            return String.Join(" ",coordinates);
        }

        static List<List<string>> GenerateNonOverlappingCoordinates()
        {
            int[] sizes = new[] { 5, 4, 3, 3, 2 }; 
            Random random = new Random();
            List<List<string>> allCoordinates = new List<List<string>>();
            HashSet<string> usedCoordinates = new HashSet<string>();
            char[] rows = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
            int[] columns = { 1, 2, 3, 4, 5, 6, 7, 8 };

            foreach (int size in sizes)
            {
                List<string> coordinates;
                bool isValid;

                do
                {
                    coordinates = new List<string>();
                    isValid = true;
                    bool isHorizontal = random.Next(2) == 0;

                    if (isHorizontal)
                    {
                        int rowIndex = random.Next(rows.Length);
                        int startColumnIndex = random.Next(columns.Length - size + 1);

                        for (int i = 0; i < size; i++)
                        {
                            string coord = $"{rows[rowIndex]}{columns[startColumnIndex + i]}";
                            if (usedCoordinates.Contains(coord))
                            {
                                isValid = false;
                                break;
                            }
                            coordinates.Add(coord);
                        }
                    }
                    else
                    {
                        int columnIndex = random.Next(columns.Length);
                        int startRowIndex = random.Next(rows.Length - size + 1);

                        for (int i = 0; i < size; i++)
                        {
                            string coord = $"{rows[startRowIndex + i]}{columns[columnIndex]}";
                            if (usedCoordinates.Contains(coord))
                            {
                                isValid = false;
                                break;
                            }
                            coordinates.Add(coord);
                        }
                    }
                } while (!isValid);

                foreach (var coord in coordinates)
                {
                    usedCoordinates.Add(coord);
                }

                allCoordinates.Add(coordinates);
            }

            return allCoordinates;
        }
    }
}
