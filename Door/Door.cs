using Godot;

namespace TurnBasedStrategyCourse_godot.Door;

public class Door : Spatial
{
  private AnimationPlayer animationPlayer;
  private bool isOpen;

  public bool IsOpen
  {
    get => isOpen;
    private set
    {
      isOpen = value;
      animationPlayer.Play(isOpen ? "open" : "close");
    }
  }

  public override void _Ready()
  {
    base._Ready();

    animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
  }

  public void Interact()
  {
    IsOpen = !IsOpen;
  }
}