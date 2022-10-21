using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Level;

namespace TurnBasedStrategyCourse_godot.Pathfinding
{
  public class Pathfinding : Spatial
  {
    [Export] private NodePath levelGridNodePath;
    [Export] private bool debug;
    [Export] private PackedScene debugCellScene;

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14; // 1.4 * 10

    private GridSystem<PathNode> gridSystem;
    private LevelGrid levelGrid;

    public override void _Ready()
    {
      base._Ready();

      levelGrid = GetNode<LevelGrid>(levelGridNodePath);
      gridSystem = new GridSystem<PathNode>(levelGrid.Width, levelGrid.Height, levelGrid.CellSize,
        (system, position) => new PathNode(position));

      var space = GetWorld().DirectSpaceState;
      foreach (var node in gridSystem.GridObjects())
      {
        var position = gridSystem.GetWorldPosition(node.GridPosition);
        var hit = space.IntersectRay(position, position + Vector3.Up * 100, null,
          1 << 5 - 1); // TODO: layer mask (walls = 5)
        node.IsWalkable = hit.Count <= 0;
      }

      if (!debug) return;

      levelGrid.Connect(nameof(LevelGrid.GroundClicked), this, nameof(OnGroundClicked));
      foreach (var cell in gridSystem.CreateDebugObjects(debugCellScene))
      {
        var gridPosition = gridSystem.GetGridPosition(cell.Translation);
        AddChild(cell);

        if (!(cell is DebugCell debugCell)) continue;

        gridSystem.GetGridObject(gridPosition).DebugCell = debugCell;
      }
    }

    private IEnumerable<GridPosition> FindPath(GridPosition start, GridPosition end)
    {
      var queue = new List<PathNode>();
      var visited = new List<PathNode>();

      var startNode = gridSystem.GetGridObject(start);
      var endNode = gridSystem.GetGridObject(end);
      queue.Add(startNode);

      foreach (var gridObject in gridSystem.GridObjects())
      {
        gridObject.Init();
      }

      startNode.GCost = 0;
      startNode.HCost = CalculateDistance(start, end);

      while (queue.Count > 0)
      {
        var currentNode = GetLowestFCostNode(queue);
        if (currentNode == endNode)
        {
          // Reached the final node
          return CalculatePath(endNode);
        }

        queue.Remove(currentNode);
        visited.Add(currentNode);

        foreach (var neighbourNode in GetNeighbours(currentNode))
        {
          if (visited.Contains(neighbourNode))
          {
            continue;
          }

          if (!neighbourNode.IsWalkable)
          {
            visited.Add(neighbourNode);
            continue;
          }

          var tentativeGCost =
            currentNode.GCost + CalculateDistance(currentNode.GridPosition, neighbourNode.GridPosition);
          if (tentativeGCost >= neighbourNode.GCost) continue;

          neighbourNode.PreviousNode = currentNode;
          neighbourNode.GCost = tentativeGCost;
          neighbourNode.HCost = CalculateDistance(neighbourNode.GridPosition, end);

          if (!queue.Contains(neighbourNode))
          {
            queue.Add(neighbourNode);
          }
        }
      }

      // no path
      return null;
    }

    private PathNode GetPathNode(int x, int z) => gridSystem.GetGridObject(new GridPosition(x, z));

    private IEnumerable<PathNode> GetNeighbours(PathNode node)
    {
      var position = node.GridPosition;

      var neighbours = new List<PathNode>();
      if (position.X - 1 >= 0)
      {
        neighbours.Add(GetPathNode(position.X - 1, position.Z));
        if (position.Z - 1 >= 0)
        {
          neighbours.Add(GetPathNode(position.X - 1, position.Z - 1));
        }

        if (position.Z + 1 < gridSystem.Height)
        {
          neighbours.Add(GetPathNode(position.X - 1, position.Z + 1));
        }
      }

      if (position.X + 1 < gridSystem.Width)
      {
        neighbours.Add(GetPathNode(position.X + 1, position.Z));
        if (position.Z - 1 >= 0)
        {
          neighbours.Add(GetPathNode(position.X + 1, position.Z - 1));
        }

        if (position.Z + 1 < gridSystem.Height)
        {
          neighbours.Add(GetPathNode(position.X + 1, position.Z + 1));
        }
      }

      if (position.Z - 1 >= 0)
      {
        neighbours.Add(GetPathNode(position.X, position.Z - 1));
      }

      if (position.Z + 1 < gridSystem.Height)
      {
        neighbours.Add(GetPathNode(position.X, position.Z + 1));
      }

      return neighbours;
    }

    private static IEnumerable<GridPosition> CalculatePath(PathNode endNode)
    {
      var gridPositions = new List<GridPosition>()
      {
        endNode.GridPosition
      };

      var currentNode = endNode;
      while (currentNode.PreviousNode != null)
      {
        gridPositions.Add(currentNode.PreviousNode.GridPosition);

        currentNode = currentNode.PreviousNode;
      }

      gridPositions.Reverse();

      return gridPositions;
    }

    private static PathNode GetLowestFCostNode(IReadOnlyList<PathNode> nodeList)
    {
      var lowestNode = nodeList[0];

      foreach (var node in nodeList.Where(node => node.FCost < lowestNode.FCost))
      {
        lowestNode = node;
      }

      return lowestNode;
    }

    private static int CalculateDistance(GridPosition a, GridPosition b)
    {
      var distance = a - b;
      var xDistance = Mathf.Abs(distance.X);
      var zDistance = Mathf.Abs(distance.Z);
      var remaining = Mathf.Abs(xDistance - zDistance);

      return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private void OnGroundClicked(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx)
    {
      if (!debug) return;
      if (!(@event is InputEventMouseButton eventMouseButton) || !eventMouseButton.Pressed) return;

      var path = FindPath(new GridPosition(0, 0), levelGrid.GetGridPosition(position));
      var gridPositions = path as GridPosition[] ?? path.ToArray();

      foreach (var cell in gridSystem.GridObjects())
      {
        if (!gridPositions.Contains(cell.GridPosition)) continue;

        cell.UpdateDebugCell();
      }
    }
  }
}