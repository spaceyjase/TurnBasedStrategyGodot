using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Unit.Ai;

public struct EnemyAiAction
{
    public GridPosition GridPosition { get; set; }
    public int Score { get; set; }
}