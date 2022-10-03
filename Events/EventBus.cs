using Godot;
using TurnBasedStrategyCourse_godot.Unit.Actions;

// Event bus for distant nodes to communicate using signals, avoiding
// complex coupling or substantially increasing code complexity.
namespace TurnBasedStrategyCourse_godot.Events
{
  public class EventBus : Node
  {
    [Signal]
    public delegate void ActionStarted(UnitAction action);
    [Signal]
    public delegate void ActionCompleted(UnitAction action);
    
    [Signal]
    public delegate void UnitBusy(Unit.Unit unit);
  
    [Signal]
    public delegate void UnitIdle(Unit.Unit unit);
    
    [Signal]
    public delegate void TurnChanged(int turn, bool isPlayerTurn);
    
    public static EventBus Instance { get; } = new EventBus();
  }
}