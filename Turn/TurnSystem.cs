using Godot;
using TurnBasedStrategyCourse_godot.Events;

namespace TurnBasedStrategyCourse_godot.Turn;

public class TurnSystem : Node
{
  private bool isPlayerTurn = true;
  private int turnNumber = 1;

  public override void _Ready()
  {
    base._Ready();
      
    EventBus.Instance.Connect(nameof(EventBus.FinishedAiTurn), this, nameof(OnFinishedAiTurn));
  }

  private void NextTurn()
  {
    isPlayerTurn = !isPlayerTurn;
    EventBus.Instance.EmitSignal(nameof(EventBus.TurnChanged), ++turnNumber, isPlayerTurn);
  }

  private void _on_UI_EndTurnPressed()
  {
    NextTurn();
  }
    
  private void OnFinishedAiTurn()
  {
    NextTurn();
  }
}