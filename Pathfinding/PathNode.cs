using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Pathfinding
{
  public class PathNode
  {
    private DebugCell debugCell;

    public PathNode(GridPosition gridPosition)
    {
      GridPosition = gridPosition;
    }

    public int GCost { get; set; } // distance from start node to this node
    public int HCost { get; set; } // heuristic cost from this node to the target node
    public int FCost => GCost + HCost;
    public GridPosition GridPosition { get; }
    public PathNode PreviousNode { get; set; }

    public DebugCell DebugCell
    {
      get => debugCell;
      set
      {
        debugCell = value;
        UpdateDebugCell();
      }
    }

    public bool IsWalkable { get; set; }

    public void UpdateDebugCell()
    {
      DebugCell?.SetCost(HCost, GCost, FCost);
    }

    public void Init()
    {
      GCost = int.MaxValue;
      HCost = 0;
      PreviousNode = null;

      UpdateDebugCell();
    }

    public override string ToString()
    {
      return GridPosition.ToString();
    }
  }
}