using System.Collections.Generic;
using UnityEngine;

public class DirectionUtil
{
    public const int InvalidAddress = -1;
    public enum Direction
    {
        None = -1,
        South,
        North,
        East,
        West,
        Count,
    }

    public static bool IsNeighbor(int baseAddress, int targetAddress, int width, int height)
    {
        int max = width * height - 1;
        if (targetAddress < 0 || targetAddress > max || baseAddress < 0 || baseAddress > max)
            return false;

        if (targetAddress == baseAddress + width)
            return true;
        if (targetAddress == baseAddress - width)
            return true;
        if (targetAddress == baseAddress + 1)
            return true;
        if (targetAddress == baseAddress - 1)
            return true;

        return false;
    }

    public static int GetNeighborAddress(int baseAddress, Direction direction, int width, int height)
    {
        int address = InvalidAddress;

        switch (direction)
        {
            case Direction.North: address = baseAddress - width; break;
            case Direction.South: address = baseAddress + width; break;
            case Direction.East:
                {
                    if (baseAddress % width != width - 1)
                        address = baseAddress + 1;
                    break;
                }
            case Direction.West:
                {
                    if (baseAddress % width != 0)
                        address = baseAddress - 1;
                    break;
                }
        }

        if (address >= width * height)
            address = InvalidAddress;

        return address;
    }

    public static List<Direction> AddressesToDirections(List<int> addresses, int width, int height)
    {
        List<Direction> directions = new List<Direction>();

        for (int i = 0; i < addresses.Count - 1; i++)
        {
            int address = addresses[i];
            int nextAddress = addresses[i + 1];
            Direction direction = DirectionUtil.DirectionBetweenTwoAddresses(address, nextAddress, width, height);
            directions.Add(direction);
        }

        return directions;
    }

    public static Direction DirectionBetweenTwoAddresses(int fromAddress, int toAddress, int width, int height)
    {
        if (toAddress == fromAddress + width)
            return Direction.South;
        if (toAddress == fromAddress - width)
            return Direction.North;
        if (toAddress == fromAddress + 1)
            return Direction.East;
        if (toAddress == fromAddress - 1)
            return Direction.West;

        return Direction.None;
    }

    public static Direction RelativeDirection(Vector3 basePos, Vector3 targetPos)
    {
        Vector3 distance = targetPos - basePos;

        // East-West
        if (Mathf.Abs(distance.x) > Mathf.Abs(distance.z))
            if (distance.x > 0)
                return Direction.West;
            else
                return Direction.East;

        // North-South
        if (distance.z > 0)
            return Direction.South;
        else
            return Direction.North;
    }

    public static Direction ClockwiseDestination(Direction baseDirection, int distance)
    {
        int modDistance = distance % (int)Direction.Count;
        if (modDistance < 0)
            modDistance = (int)Direction.Count + modDistance;

        switch (baseDirection)
        {
            case Direction.North:
                {
                    if (modDistance == 0)
                        return Direction.North;
                    else if (modDistance == 1)
                        return Direction.East;
                    else if (modDistance == 2)
                        return Direction.South;
                    else if (modDistance == 3)
                        return Direction.West;
                    break;
                }
            case Direction.South:
                {
                    if (modDistance == 0)
                        return Direction.South;
                    else if (modDistance == 1)
                        return Direction.West;
                    else if (modDistance == 2)
                        return Direction.North;
                    else if (modDistance == 3)
                        return Direction.East;
                    break;
                }
            case Direction.East:
                {
                    if (modDistance == 0)
                        return Direction.East;
                    else if (modDistance == 1)
                        return Direction.South;
                    else if (modDistance == 2)
                        return Direction.West;
                    else if (modDistance == 3)
                        return Direction.North;
                    break;
                }
            case Direction.West:
                {
                    if (modDistance == 0)
                        return Direction.West;
                    else if (modDistance == 1)
                        return Direction.North;
                    else if (modDistance == 2)
                        return Direction.East;
                    else if (modDistance == 3)
                        return Direction.South;
                    break;
                }
        }

        return baseDirection;
    }
}
