using Godot;
using TurnBasedStrategyCourse_godot.Events;

namespace TurnBasedStrategyCourse_godot.Turn
{
  public class TurnSystem : Node
  {
    private int turnNumber = 1;

    private void NextTurn()
    {
      EventBus.Instance.EmitSignal(nameof(EventBus.TurnChanged), ++turnNumber);
    }

    private void _on_UI_EndTurnPressed()
    {
      NextTurn();
    }
  }
}