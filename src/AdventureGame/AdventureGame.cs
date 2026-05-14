
using System.Collections;
using System.Collections.Generic;

namespace AdventureGame;

public class AdventureGame
{
	public readonly string GO_NORTH = "W";
	public readonly string GO_SOUTH = "S";
	public readonly string GO_EAST = "D";
	public readonly string GO_WEST = "A";
	public readonly string GET_LAMP = "L";
	public readonly string GET_KEY = "K";
	public readonly string OPEN_CHEST = "O";
	public readonly string QUIT = "Q";

	private Adventurer adventurer;
	private Room[,] dungeon;
	private int currentRow;
	private int currentCol;
	private bool isChestOpen;
	private bool hasPlayerQuit;
	private bool isAdventureAlive;
	private string lastDirection;
	static public int greuDelay = 1;

	public AdventureGame()
	{

	}

	public void Start()
	{
		Init();

		ShowGameStartScreen();

		string input;

		do
		{
			ShowScene();

			do
			{
				ShowInputOptions();

				input = GetInput();
			}
			while (!IsValidInput(input));

			Console.Clear();
			ProcessInput(input);

			UpdateGameState();
		}
		while (!IsGameOver());

		ShowGameOverScreen();
	}

	private void Init()
	{
		string basePath = AppContext.BaseDirectory;
		string path = Path.Combine(basePath, "../", "../", "../", "../", "../", "res", "Dungeon2.txt");
		adventurer = new Adventurer();
		dungeon = DungeonLoader.Load(path);


		currentRow = 1;
		currentCol = 1;

		isChestOpen = false;
		hasPlayerQuit = false;
		isAdventureAlive = true;

		lastDirection = string.Empty;
	}

	private void ShowGameStartScreen()
	{
		Console.WriteLine("Welcome to Adventure Game!");
	}

	private void ShowScene()
	{
		var r = dungeon[currentRow, currentCol];
		Console.WriteLine($"Greu Delay: {greuDelay}");
		Console.WriteLine($"player position: {currentRow}, {currentCol}");
		Console.WriteLine();

		if (adventurer.HasLamp() || r.IsLit())
		{
			Console.WriteLine(r.GetDescription());
		}
		else
		{
			Console.WriteLine("This room is pitch black!");
		}
	}

	private void ShowInputOptions()
	{
		string options = ""
		+ $"GO NORTH [{GO_NORTH}] | GO EAST [{GO_EAST}] | GET LAMP [{GET_LAMP}] | OPEN CHEST [{OPEN_CHEST}]\n"
		+ $"GO SOUTH [{GO_SOUTH}] | GO WEST [{GO_WEST}] | GET KEY  [{GET_KEY}] | QUIT       [{QUIT}]\n"
		+ $"> ";

		Console.Write(options);
	}

	private string GetInput()
	{
		return Console.ReadLine()!.ToUpper();
	}

	private bool IsValidInput(string input)
	{
		string[] validInputs = { GO_NORTH, GO_SOUTH, GO_EAST, GO_WEST, GET_LAMP, GET_KEY, OPEN_CHEST, QUIT };

		if (!validInputs.Contains(input))
		{
			Console.WriteLine("ERROR: Invalid input. Please try again.");
			return false;
		}

		return true;
	}

	private void ProcessInput(string input)
	{
		Room r = dungeon[currentRow, currentCol];

		if (!adventurer.HasLamp() && !r.IsLit() && input != lastDirection)
		{
			isAdventureAlive = false;
		}
		else if (input == GO_NORTH)
		{
			GoNorth(r);
		}
		else if (input == GO_SOUTH)
		{
			GoSouth(r);
		}
		else if (input == GO_EAST)
		{
			GoEast(r);
		}
		else if (input == GO_WEST)
		{
			GoWest(r);
		}
		else if (input == GET_LAMP)
		{
			GetLamp(r);
		}
		else if (input == GET_KEY)
		{
			GetKey(r);
		}
		else if (input == OPEN_CHEST)
		{
			OpenChest(r);
		}
		else// if(input == QUIT)
		{
			Quit();
		}
	}

	private void UpdateGameState()
	{
		if (isChestOpen)
		{
			if (greuDelay > 0)
			{
				greuDelay -= 1;
				return;
			}
			MoveGrue();
			if (DungeonLoader.grueCol == currentCol && DungeonLoader.grueRow == currentRow)
			{
				isAdventureAlive = false;
			}
		}
	}

	private void MoveGrue()
	{
		Room playerPos = dungeon[currentRow, currentCol];
		Room greuPos = dungeon[DungeonLoader.grueRow, DungeonLoader.grueCol];

		Dictionary<Room, Room>? path = BFS(greuPos, playerPos, dungeon);

		if (path is null)
		{
			return;
		}

		Room move = greuPos;
		Console.WriteLine("Greu Path:");
		for (int i = 0; i < path.Count; i++)
		{
			move = path[move];
			Console.Write($"#{i}({move.row},{move.col})");
			if (move == playerPos)
			{
				break;
			}
		}
		Console.WriteLine();

		DungeonLoader.grueRow = path[greuPos].row;
		DungeonLoader.grueCol = path[greuPos].col;

	}

	Dictionary<Room, Room>? BFS(Room goal, Room start, Room[,] dungeon)
	{
		if (goal is null || start is null)
		{
			return null;
		}
		Dictionary<Room, Room> path = new Dictionary<Room, Room>();
		Queue<Room> open = new Queue<Room>();

		path.Add(start, start);
		open.Enqueue(start);

		while (open.Count > 0)
		{
			Room r = open.Dequeue();
			if (r.Equals(goal))
			{
				return path;
			}
			else
			{
				foreach (Room adjRoom in r.getAdjacents(dungeon))
				{
					if (!path.ContainsKey(adjRoom))
					{
						path.Add(adjRoom, r);
						open.Enqueue(adjRoom);
					}
				}
			}
		}
		return null;
	}

	private bool IsGameOver()
	{
		return (isChestOpen && currentCol == DungeonLoader.exitCol && currentRow == DungeonLoader.exitRow) || hasPlayerQuit || !isAdventureAlive;
	}

	private void ShowGameOverScreen()
	{
		Console.Clear();
		Console.WriteLine("Game Over!");
		if (!isAdventureAlive) Console.WriteLine("You have been devourered");
		else Console.WriteLine("You have Escaped!");
	}

	private void GoNorth(Room r)
	{
		if (r.HasNorth())
		{
			currentRow -= 1;
			lastDirection = GO_SOUTH;
		}
		else
		{
			Console.WriteLine("You cannot go north!\a");
		}
	}

	private void GoSouth(Room r)
	{
		if (r.HasSouth())
		{
			currentRow += 1;
			lastDirection = GO_NORTH;
		}
		else
		{
			Console.WriteLine("You cannot go south!\a");
		}
	}

	private void GoEast(Room r)
	{
		if (r.HasEast())
		{
			currentCol += 1;
			lastDirection = GO_WEST;
		}
		else
		{
			Console.WriteLine("You cannot go east!\a");
		}
	}

	private void GoWest(Room r)
	{
		if (r.HasWest())
		{
			currentCol -= 1;
			lastDirection = GO_EAST;
		}
		else
		{
			Console.WriteLine("You cannot go west!\a");
		}
	}

	private void GetLamp(Room r)
	{
		if (r.HasLamp())
		{
			Console.WriteLine("You got the lamp!");
			adventurer.SetLamp(true);
			r.SetLamp(false);
		}
		else
		{
			Console.WriteLine("There is no lamp in this room.");
		}
	}

	private void GetKey(Room r)
	{
		if (r.HasKey())
		{
			Console.WriteLine("You got the key!");
			adventurer.SetKey(true);
			r.SetKey(false);
		}
		else
		{
			Console.WriteLine("There is no key in this room.");
		}
	}

	private void OpenChest(Room r)
	{
		if (r.HasChest())
		{
			if (adventurer.HasKey())
			{
				Console.WriteLine("You got the treasure!");
				Console.WriteLine("You suddenly feel the need to run to the exit");
				isChestOpen = true;
			}
			else
			{
				Console.WriteLine("You do not have the key!");
			}
		}
		else
		{
			Console.WriteLine("There is no chest in this room.");
		}
	}

	private void Quit()
	{
		Console.WriteLine("You quit the game!");
		hasPlayerQuit = true;
	}
}
