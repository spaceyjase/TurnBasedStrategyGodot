using Godot;
using TurnBasedStrategyCourse_godot.Events;

namespace TurnBasedStrategyCourse_godot.Turn
{
  public class TurnSystem : Node
  {
    private bool isPlayerTurn = true;
    private int turnNumber = 1;

    private void NextTurn()
    {
      isPlayerTurn = !isPlayerTurn;
      EventBus.Instance.EmitSignal(nameof(EventBus.TurnChanged), ++turnNumber, isPlayerTurn);
    }

    private void _on_UI_EndTurnPressed()
    {
      NextTurn();
    }
  }
}