using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Events;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Level;
using TurnBasedStrategyCourse_godot.Unit.Actions;
using TurnBasedStrategyCourse_godot.Unit.Stats;

namespace TurnBasedStrategyCourse_godot.Unit
{
  public class Unit : Spatial
  {
    [Signal]
    public delegate void UnitSelected(Unit selectedUnit);

    [Signal]
    public delegate void OnUnitMoving(Unit selectedUnit, GridPosition oldPosition, GridPosition newPosition);

    [Export] private UnitStats unitStats;
    [Export] private bool isEnemy;
    [Export] private NodePath bulletSpawnPositionPath;

    private AnimationTree animationTree;
    private AnimationNodeStateMachinePlayback animationStateMachine;
    private Spatial selectedVisual;
    private bool selected;

    public UnitAction CurrentAction { get; private set; }
    public UnitAction DefaultAction => IdleAction;
    private UnitAction IdleAction { get; set; }
    public LevelGrid LevelGrid { get; private set; }
    public GridPosition GridPosition { get; private set; }
    public bool IsEnemy => isEnemy;

    public bool Selected
    {
      get => selected;
      set
      {
        selected = value;
        if (selected)
        {
          selectedVisual.Show();
        }
        else
        {
          selectedVisual.Hide();
        }
      }
    }

    public float MovementSpeed => unitStats.MovementSpeed;
    public double StoppingDistance => unitStats.StoppingDistance;
    public float RotateSpeed => unitStats.RotateSpeed;
    public int MaxMoveDistance => unitStats.MaxMoveDistance;
    public int MaxShootDistance => unitStats.MaxShootDistance;
    private int TotalActionPoints => unitStats.TotalActionPoints;

    private readonly Dictionary<string, UnitAction> actions = new Dictionary<string, UnitAction>();

    public Vector3 BulletSpawnPosition => GetNode<Position3D>(bulletSpawnPositionPath).GlobalTranslation;

    public void SetAnimation(string animationName)
    {
      animationStateMachine.Travel(animationName);
    }

    public override void _Ready()
    {
      animationTree = GetNode<AnimationTree>("AnimationTree");
      animationStateMachine = animationTree.Get("parameters/playback") as AnimationNodeStateMachinePlayback;
      selectedVisual = GetNode<Spatial>("SelectedVisual");
      selectedVisual.Hide();

      if (isEnemy)
      {
        GetNode<Area>("SelectorArea").QueueFree();
      }

      foreach (var child in GetChildren())
      {
        if (!(child is UnitAction action)) continue;

        if (actions.Count == 0)
        {
          IdleAction = action;
        }

        actions[action.Name] = action;
      }
      
      CurrentAction = IdleAction;
      TargetPosition = Translation;
      
      EventBus.Instance.Connect(nameof(EventBus.TurnChanged), this, nameof(OnTurnChanged));
    }

    private void OnTurnChanged(int turn, bool isPlayerTurn)
    {
      if (!isEnemy) GetNode<CollisionShape>("SelectorArea/CollisionShape").Disabled = !isPlayerTurn;
      if (isEnemy == isPlayerTurn) return;

      ResetActionPoints();
    }

    public void Initialise(LevelGrid levelGrid)
    {
      LevelGrid = levelGrid;
      GridPosition = LevelGrid.GetGridPosition(Translation);
      
      unitStats.Initialise();

      ActionPoints = TotalActionPoints;
    }

    public override void _Process(float delta)
    {
      base._Process(delta);
      
      ProcessAction(delta);
    }

    private void ProcessAction(float delta)
    {
      CurrentAction.Execute(delta);

      var newGridPosition = LevelGrid.GetGridPosition(Translation);
      if (newGridPosition == GridPosition) return;

      var oldPosition = GridPosition;
      GridPosition = newGridPosition;
      EmitSignal(nameof(OnUnitMoving), this, oldPosition, GridPosition);
    }

    // ReSharper disable once UnusedMember.Local
    private void _on_SelectorArea_input_event(Node camera, InputEvent @event, Vector3 position, Vector3 normal,
      int shape_idx)
    {
      if (!(@event is InputEventMouseButton eventMouseButton) || !eventMouseButton.Pressed ||
          eventMouseButton.ButtonIndex != 1) return;
      
      EmitSignal(nameof(UnitSelected), this);
    }

    public Vector3 TargetPosition { get; private set; } = Vector3.Zero;

    public IEnumerable<UnitAction> Actions => actions.Values.Where(x => x != IdleAction);
    public int ActionPoints { get; private set; }
    public bool Busy => CurrentAction != IdleAction;

    private void TryChangeAction(string name)
    {
      if (actions.ContainsKey(name) == false)
      {
        GD.PrintErr($"State {name} does not exist!");
        return;
      }

      var action = actions[name];
      if (!CanTakeAction(action)) return;

      SpendActionPoints(action.ActionPointCost);
      
      ChangeAction(actions[name]);
    }

    public void ChangeAction(string name)
    {
      if (actions.ContainsKey(name) == false)
      {
        GD.PrintErr($"State {name} does not exist!");
        return;
      }
      
      ChangeAction(actions[name]);
    }

    private void ChangeAction(UnitAction newAction)
    {
      if (newAction == null)
      {
        GD.PrintErr("Cannot transition to a null state!");
        return;
      }

      CurrentAction?.OnExit?.Invoke();

      CurrentAction = newAction;
      
      EventBus.Instance.EmitSignal(CurrentAction == IdleAction ? nameof(EventBus.UnitIdle) : nameof(EventBus.UnitBusy), this);
      
      newAction.OnEnter?.Invoke();
    }

    public void DoAction(string actionName)
    {
      if (CurrentAction.Name != IdleAction.ActionName) return;
      
      TryChangeAction(actionName);
    }

    private bool CanTakeAction(UnitAction action)
    {
      return ActionPoints >= action.ActionPointCost;
    }

    private void SpendActionPoints(int points)
    {
      ActionPoints -= points;
    }

    private void ResetActionPoints()
    {
      ActionPoints = TotalActionPoints;
    }

    public bool TrySetTargetPositionForAction(UnitAction action, Vector3 position)
    {
      var gridPosition = LevelGrid.GetGridPosition(position);
      if (!action.IsValidGridPosition(gridPosition)) return false;

      TargetPosition = LevelGrid.GetWorldPosition(gridPosition);
      
      return true;
    }

    public void Damage()
    {
      GD.Print($"Damaged {Name}");
    }
  }
}