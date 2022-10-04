using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Events;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Level;
using TurnBasedStrategyCourse_godot.Unit.Actions;
using TurnBasedStrategyCourse_godot.Unit.Stats;
using TurnBasedStrategyCourse_godot.Unit.UI;

namespace TurnBasedStrategyCourse_godot.Unit
{
  public class Unit : Spatial
  {
    [Export] private NodePath bulletSpawnPositionPath;
    [Export] private bool isEnemy;
    [Export] private PackedScene ragdollScene;
    [Export] private UnitStats unitStats;
    [Export] private NodePath unitWorldUiPath;
    
    [Signal] public delegate void UnitMoving(Unit selectedUnit, GridPosition oldPosition, GridPosition newPosition);
    [Signal] public delegate void UnitSelected(Unit selectedUnit);

    private readonly Dictionary<string, UnitAction> actions = new Dictionary<string, UnitAction>();
    private AnimationNodeStateMachinePlayback animationStateMachine;

    private AnimationTree animationTree;
    private bool selected;
    private Spatial selectedVisual;
    private UnitWorldUI unitWorldUi;
    private UnitAction currentAction;
    
    private bool IsUnitDead => unitStats.Health <= 0;

    public UnitAction CurrentAction
    {
      get => currentAction;
      private set
      {
        currentAction?.Exit();
        currentAction = value;
        currentAction?.Enter();
      }
    }
    public UnitAction DefaultAction => IdleAction;
    private UnitAction IdleAction { get; set; }
    public LevelGrid LevelGrid { get; private set; }
    public GridPosition GridPosition { get; private set; }
    public bool IsEnemy => isEnemy;
    public int CurrentHealth => unitStats.Health;
    public int MaxHealth => unitStats.MaxHealth;

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
    public int TotalActionPoints => unitStats.TotalActionPoints;

    public Vector3 BulletSpawnPosition => GetNode<Position3D>(bulletSpawnPositionPath).GlobalTranslation;

    public Vector3 TargetPosition { get; private set; } = Vector3.Zero;

    public IEnumerable<UnitAction> Actions => actions.Values.Where(x => x != IdleAction);
    public int ActionPoints { get; private set; }
    public bool Busy => CurrentAction != IdleAction;
    public Position3D CameraPosition { get; private set; }

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
      CameraPosition = GetNode<Position3D>("CameraPosition3D");
      
      EventBus.Instance.Connect(nameof(EventBus.TurnChanged), this, nameof(OnTurnChanged));

      unitWorldUi = GetNode<UnitWorldUI>(unitWorldUiPath);
      unitWorldUi.UpdateUI(this);
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

      unitStats = (UnitStats)unitStats.Duplicate();
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
      EmitSignal(nameof(UnitMoving), this, oldPosition, GridPosition);
    }

    // ReSharper disable once UnusedMember.Local
    private void _on_SelectorArea_input_event(Node camera, InputEvent @event, Vector3 position, Vector3 normal,
      int shape_idx)
    {
      if (!(@event is InputEventMouseButton eventMouseButton) || !eventMouseButton.Pressed ||
          eventMouseButton.ButtonIndex != 1) return;
      
      EmitSignal(nameof(UnitSelected), this);
    }

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

      CurrentAction = newAction;
      
      EventBus.Instance.EmitSignal(CurrentAction == IdleAction ? nameof(EventBus.UnitIdle) : nameof(EventBus.UnitBusy), this);
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

    public void Damage(int damageAmount)
    {
      unitStats.TakeDamage(damageAmount);
      unitWorldUi.UpdateUI(this);

      if (IsUnitDead)
      {
        Die();
      }
    }

    private void Die()
    {
      LevelGrid.RemoveUnitAtGridPosition(GridPosition, this);
      SpawnRagdoll();
      QueueFree();
    }

    private void SpawnRagdoll()
    {
      var ragdoll = ragdollScene.Instance<UnitRagdoll>();
      
      GetTree().Root.AddChild(ragdoll);
      ragdoll.GlobalTranslation = GlobalTranslation;
      ragdoll.GlobalRotation = GlobalRotation;
      
      var mesh = GetNode<MeshInstance>("Armature/Skeleton/characterMedium001");
      ragdoll.Init(mesh.GetSurfaceMaterial(0));

      ragdoll.StartRagdoll();
    }
  }
}