using System;
using System.Collections.Generic;

public class SearchNode
{
	public const int ImpassibleScore = -1;
	
	public int score = ImpassibleScore;
	public int address = -1;

	public SearchNode(int score, int address)
	{
		this.score = score;
		this.address = address;
	}

	public bool IsImpassible()
	{
		return this.score == SearchNode.ImpassibleScore;
	}
}

public class Astar
{
	public const int Passible = 1;
	public const int Impassible = 0;

	private static List<int> Neighbors(int baseAddress, int[] map, int width, int height)
	{
		List<int> addresses = new List<int>();
		// TODO: not rect
		int max = width * height;
		
		// up
		if (baseAddress >= width) addresses.Add(baseAddress - width);
		// down
		if (baseAddress < max - width) addresses.Add(baseAddress + width);
		// right
		if (baseAddress < max - 1 && (baseAddress % width) != width - 1) addresses.Add(baseAddress + 1);
		// left
		if (baseAddress > 0 && (baseAddress % width) != 0) addresses.Add(baseAddress - 1);

		return addresses;
	}

	private static bool OpenNode(bool found, Dictionary<int, SearchNode> openedNode, SearchNode baseNode, int goal, int[] map, int width, int height)
	{
		List<int> neighbors = Astar.Neighbors(baseNode.address, map, width, height);

		for (int i = 0; i < neighbors.Count; i++)
		{
			if (!found)
			{
				int nextAddress = neighbors[i];
				int currentScore = baseNode.score + 1;
				if (!openedNode.ContainsKey(nextAddress))
				{
					SearchNode nextNode = new SearchNode(baseNode.score + 1, nextAddress);
					openedNode[nextAddress] = nextNode;
					if (nextAddress == goal)
					{
						return true;
					}

					if (!found)
					{
						found = Astar.OpenNode(found, openedNode, nextNode, goal, map, width, height);
					}
				}
				else
				{
					if (openedNode[nextAddress].score > currentScore + 1)
					{
						openedNode[nextAddress].score = currentScore + 1;
					}
				}
			}
		}

		return found;
	}

	public static List<int> MakeRoute(Dictionary<int, int> path, int start, int goal)
	{
		List<int> route = new List<int>();
		int to = goal;

		while (path.ContainsKey(to) && to != start)
		{
			route.Add(to);
			to = path[to];
		}
		route.Add(start);
		route.Reverse();
		return route;
	}

	public static List<int> Exec(int start, int goal, int[] map, int width)
    {
		return Astar.Exec(start, goal, map, width, (int)Math.Ceiling((double)map.Length / (double)width));
    }
	public static List<int> Exec(int start, int goal, int[] map, int width, int height)
	{
		try
		{
			if (map[start] == 0 || map[goal] == 0)
				return new List<int>() { start };
		}
		catch (Exception)
		{
			return new List<int>() { start };
		}
		

		Dictionary<int, int> path = new Dictionary<int, int>();

		Dictionary<int, SearchNode> openedNode = new Dictionary<int, SearchNode>();
		Dictionary<int, int> score = new Dictionary<int, int>();
		SearchNode startNode = new SearchNode(0, start);
		openedNode.Add(start, startNode);

		while (openedNode.Keys.Count > 0)
		{
			SearchNode current = null;
			foreach (KeyValuePair<int, SearchNode> entry in openedNode)
            {
				SearchNode node = entry.Value;
				if (current == null || (node.score < current.score && !node.IsImpassible()))
				{
					current = node;
				}
			}

			if (current.address == goal)
			{
				// TODO: overhead
				return Astar.MakeRoute(path, start, goal);
			}

			openedNode.Remove(current.address);

			if (current.IsImpassible())
			{
				continue;
			}

			List<int> neighbors = Astar.Neighbors(current.address, map, width, height);
			for (int i = 0; i < neighbors.Count; i++)
			{
				int neighborAddress = neighbors[i];
				if (!score.ContainsKey(neighborAddress))
				{
					// TODO: heuristic
					if (map[neighborAddress] == Astar.Impassible)
					{
						score[neighborAddress] = SearchNode.ImpassibleScore;
					}
					else
					{
						score[neighborAddress] = current.score + 1;
						path[neighborAddress] = current.address;
					}
					SearchNode neighborNode = new SearchNode(score[neighborAddress], neighborAddress);
					openedNode.Add(neighborAddress, neighborNode);
				}
			}
		}

		return new List<int>();
	}
}
