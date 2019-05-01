using System;
using System.Collections.Generic;
using UnityEngine;

using EventSys;

public enum GoatState {
	None,
	Run,
	SlowDown,
	Obstacle,
	Slide,
	Jump,
	Yell,
	Die,
}

public class State {
	protected GoatState Type = GoatState.None;
	protected float TimeToExit = -1;
	protected List<GoatState> AvailableTransitions = new List<GoatState>();
	protected GoatState ExitState = GoatState.None;
	protected GoatController Controller = null;
	protected float EnterTime = 0f;

	public GoatState StateType {
		get { return Type; }
	}

	public State(GoatController controller) {
		EnterTime  = GameState.Instance.TimeController.CurrentTime;
		Controller = controller;
	}

	public void Initialize() {
		Init();
	}

	protected virtual void Init() {
		Controller.VisualController.SetState(Type);
	}

	public void Update() {
		if ( TimeToExit >= 0 && ExitState != GoatState.None && !GameState.Instance.TimeController.IsPause ) {
			var curTime = GameState.Instance.TimeController.CurrentTime;
			if ( curTime > TimeToExit + EnterTime ) {
				LeaveState();
				TryChangeState(CreateStateFromEnum(ExitState));
				return;
			}
		}
		ProcessState();
	}

	protected virtual void ProcessState() {}
	protected virtual void LeaveState() {}

	public void TryChangeState(State newState) {
		if ( newState != null && CanChangeState(newState.Type) ) {
			ChangeState(newState);
		}
	}

	public void ChangeState(State newState) {
		LeaveState();
		Controller.CurrentState = newState;
		Controller.CurrentState.Initialize();
	}

	public bool CanChangeState(GoatState newState) {
		foreach ( var state in AvailableTransitions ) {
			if ( state == newState ) {
				return true;
			}
		}
		return false;
	}

	public State CreateStateFromEnum(GoatState state) {
		switch ( state ) {
			case GoatState.Run:
				return new RunState(Controller);
			case GoatState.None:
				return new State(Controller);
			case GoatState.SlowDown:
				return new SlowDownState(Controller);
			case GoatState.Obstacle:
				return new ObstacleState(Controller);
			case GoatState.Slide:
				return new SlideState(Controller);
			case GoatState.Jump:
				return new JumpState(Controller);
			case GoatState.Yell:
				return new YellState(Controller);
			case GoatState.Die:
				return new DeadState(Controller);
			default:
				throw new ArgumentOutOfRangeException(nameof(state), state, null);
		}
	}
}

public sealed class RunState : State {
	public RunState(GoatController controller) : base(controller) {
		AvailableTransitions = new List<GoatState> {
			GoatState.Run,
			GoatState.Jump,
			GoatState.Slide,
			GoatState.SlowDown,
			GoatState.Obstacle,
			GoatState.Yell,
			GoatState.Die
		};
		Type = GoatState.Run;
	}
	protected override void Init() {
		base.Init();
		EventManager.Subscribe<Event_JumpButtonPushed> (this, OnJumpButtonPushed);
		EventManager.Subscribe<Event_SlideButtonPushed>(this, OnSlideButtonPushed);
		EventManager.Subscribe<Event_YellButtonPushed> (this, OnYellButtonPushed);
	}

	protected override void ProcessState() {
		base.ProcessState();
		Controller.SetRunSpeed(Controller.RunSpeed);
	}

	protected override void LeaveState() {
		base.LeaveState();
		EventManager.Unsubscribe<Event_JumpButtonPushed> (OnJumpButtonPushed);
		EventManager.Unsubscribe<Event_SlideButtonPushed>(OnSlideButtonPushed);
		EventManager.Unsubscribe<Event_YellButtonPushed> (OnYellButtonPushed);
	}

	void OnJumpButtonPushed(Event_JumpButtonPushed e) {
		if ( Controller.CharController.Grounded ) {
			TryChangeState(new JumpState(Controller));
		}
	}

	void OnSlideButtonPushed(Event_SlideButtonPushed e) {
		if ( Controller.CharController.Grounded ) {
			TryChangeState(new SlideState(Controller));
		}
	}

	void OnYellButtonPushed(Event_YellButtonPushed e) {
		TryChangeState(new YellState(Controller));
	}
}

public sealed class JumpState : State {
	public JumpState(GoatController controller) : base(controller) {
		AvailableTransitions = new List<GoatState> {
			GoatState.Run,
			GoatState.Die,
			GoatState.Obstacle,
			GoatState.SlowDown,
			GoatState.Yell,
		};
		Type = GoatState.Jump;
	}

	protected override void Init() {
		base.Init();
		EventManager.Subscribe<Event_PhysicsObjectGrounded>(this, OnGrounded);
		EventManager.Subscribe<Event_YellButtonPushed>(this, OnYellButtonPushed);
		SoundManager.Instance.PlaySound("Jump");
		Controller.Jump();
	}

	protected override void LeaveState() {
		base.LeaveState();
		EventManager.Unsubscribe<Event_PhysicsObjectGrounded>(OnGrounded);
		EventManager.Unsubscribe<Event_YellButtonPushed>(OnYellButtonPushed);
	}

	void OnGrounded(Event_PhysicsObjectGrounded e) {
		if ( e.Object != Controller.CharController ) {
			return;
		}
		TryChangeState(new RunState(Controller));
	}

	void OnYellButtonPushed(Event_YellButtonPushed e) {
		TryChangeState(new YellState(Controller));
	}
}

public sealed class SlideState : State {
	public SlideState(GoatController controller) : base(controller) {
		AvailableTransitions = new List<GoatState> {
			GoatState.Run,
			GoatState.Die,
			GoatState.Obstacle,
			GoatState.SlowDown,
			GoatState.Jump,
		};
		Type = GoatState.Slide;
		TimeToExit = 1.5f;
		ExitState = GoatState.Run;
	}

	protected override void Init() {
		base.Init();
		Controller.CharController.SetLowProfile(true);
		Debug.Log("Slide on");
		EventManager.Subscribe<Event_JumpButtonPushed>(this, OnJumpButtonPushed);
		EventManager.Subscribe<Event_SlideButtonReleased>(this, OnSlideButtonReleased);
		SoundManager.Instance.PlaySound("Whoosh");
	}

	protected override void LeaveState() {
		base.LeaveState();
		Debug.Log("Slide off");
		Controller.CharController.SetLowProfile(false);
		EventManager.Unsubscribe<Event_JumpButtonPushed>(OnJumpButtonPushed);
		EventManager.Unsubscribe<Event_SlideButtonReleased>(OnSlideButtonReleased);
	}

	void OnJumpButtonPushed(Event_JumpButtonPushed e) {
		if ( Controller.CharController.Grounded ) {
			TryChangeState(new SlideState(Controller));
		}
	}

	void OnSlideButtonReleased(Event_SlideButtonReleased e) {
		TryChangeState(new RunState(Controller));
	}
}

public sealed class SlowDownState : State {
	public SlowDownState(GoatController controller, float slowdownTime) : this(controller) {
		TimeToExit = slowdownTime;
	}

	public SlowDownState(GoatController controller) : base(controller) {
		AvailableTransitions = new List<GoatState> {
			GoatState.Run,
			GoatState.Die,
			GoatState.Obstacle,
			GoatState.Jump,
			GoatState.Yell
		};
		Type = GoatState.SlowDown;
		TimeToExit = 1f;
		ExitState = GoatState.Run;
	}

	protected override void Init() {
		base.Init();
		Controller.SetRunSpeed(Controller.SlowSpeed);
		EventManager.Subscribe<Event_JumpButtonPushed>(this, OnJumpButtonPushed);
		EventManager.Subscribe<Event_YellButtonPushed>(this, OnYellButtonPushed);
	}

	protected override void LeaveState() {
		base.LeaveState();
		Debug.Log("SlowDown off");
		EventManager.Unsubscribe<Event_JumpButtonPushed>(OnJumpButtonPushed);
		EventManager.Unsubscribe<Event_YellButtonPushed>(OnYellButtonPushed);
	}

	void OnJumpButtonPushed(Event_JumpButtonPushed e) {
		if ( Controller.CharController.Grounded ) {
			TryChangeState(new JumpState(Controller));
		}
	}

	void OnYellButtonPushed(Event_YellButtonPushed e) {
		TryChangeState(new YellState(Controller));
	}
}

public sealed class ObstacleState : State {
	public ObstacleState(GoatController controller) : base(controller) {
		AvailableTransitions = new List<GoatState> {
			GoatState.Run,
			GoatState.Die,
			GoatState.Obstacle,
			GoatState.Jump,
		};
		Type = GoatState.Obstacle;
		TimeToExit = 1.7f;
		ExitState = GoatState.Run;
	}

	protected override void Init() {
		base.Init();
		Controller.SetRunSpeed(0, true);
		EventManager.Subscribe< Event_JumpButtonPushed>(this,OnJumpButtonPushed);
		SoundManager.Instance.PlaySound("Hit");
	}

	protected override void ProcessState() {
		base.ProcessState();
		var curTime = GameState.Instance.TimeController.CurrentTime;
		var stateTime = curTime - EnterTime;
		var speed = stateTime / TimeToExit;
		Controller.SetRunSpeed(Mathf.Clamp(0.75f + speed * Controller.RunSpeed, 0, Controller.RunSpeed));
	}

	protected override void LeaveState() {
		base.LeaveState();
		EventManager.Unsubscribe<Event_JumpButtonPushed>(OnJumpButtonPushed);
	}

	void OnJumpButtonPushed(Event_JumpButtonPushed e) {
		if ( Controller.CharController.Grounded ) {
			TryChangeState(new JumpState(Controller));
		}
	}
}

public sealed class YellState : State {
	public YellState(GoatController controller) : base(controller) {
		AvailableTransitions = new List<GoatState> {
			GoatState.Run,
			GoatState.Die,
			GoatState.Obstacle,
			GoatState.Jump,
		};
		Type = GoatState.Yell;
		TimeToExit = 0.8f;
		ExitState = GoatState.Run;
	}

	protected override void Init() {
		base.Init();
		SoundManager.Instance.PlaySound("Yell");
		EventManager.Fire(new Event_GoatYell());
		EventManager.Subscribe<Event_JumpButtonPushed>(this, OnJumpButtonPushed);
	}

	protected override void LeaveState() {
		base.LeaveState();
		EventManager.Unsubscribe<Event_JumpButtonPushed>(OnJumpButtonPushed);
	}

	void OnJumpButtonPushed(Event_JumpButtonPushed e) {
		if ( Controller.CharController.Grounded ) {
			TryChangeState(new JumpState(Controller));
		}
	}
}

public sealed class DeadState : State {
	public DeadState(GoatController controller) : base(controller) {
		AvailableTransitions = new List<GoatState>();
		Type = GoatState.Die;
	}

	protected override void Init() {
		base.Init();
		Controller.CharController.DisableAllColliders();
		Controller.SetRunSpeed(0f, true);
		Controller.CharController.Jump(Controller.JumpForce * 0.75f);
	}
}

public sealed class GoatController : MonoBehaviour {
	public float JumpForce = 7f;
	public float RunSpeed  = 3f;
	public float SlowSpeed = 2f;

	public PhysicsObject CharController   = null;
	public GoatVisual    VisualController = null;

	[NonSerialized] public State CurrentState = null;

	float _targetSpeed  = 0f;
	float _currentSpeed = 0f;

	private void Start() {
		CurrentState = new RunState(this);
		CurrentState.Initialize();
	}

	private void Update() {
		if ( CurrentState != null ) {
			CurrentState.Update();
		}

		_currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, Time.deltaTime * 20f);
		CharController.SetMoveSpeed(_currentSpeed);

		if ( Input.GetKey(KeyCode.Space) ) {
			EventManager.Fire(new Event_JumpButtonPushed());
		}

		if ( Input.GetKey(KeyCode.S) ) {
			EventManager.Fire(new Event_SlideButtonPushed());
		}

		if ( Input.GetKeyUp(KeyCode.S) ) {
			EventManager.Fire(new Event_SlideButtonReleased());
		}

		if ( Input.GetKeyDown(KeyCode.W) ) {
			EventManager.Fire(new Event_YellButtonPushed());
		}

	}

	void OnGUI() {
		if ( GameState.Instance.IsDebug ) {
			GUI.Label(new Rect(new Vector2(10, 10), new Vector2(100, 20)), string.Format("State: {0}", CurrentState.StateType));
		}
	}

	public void Jump() {
		CharController.Jump(JumpForce);
	}

	public void SetRunSpeed(float speed, bool instant = false) {
		_targetSpeed = speed;
		if ( instant ) {
			_currentSpeed = speed;
		}
	}
}
