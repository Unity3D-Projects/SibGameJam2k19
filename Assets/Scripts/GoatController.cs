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

	protected virtual void Init() {}

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
				return  new RunState(Controller);
			case GoatState.None:
				return new State(Controller);
			case GoatState.SlowDown:
				return new SlowDownState(Controller);
			case GoatState.Obstacle:
				break;
			case GoatState.Slide:
				return new SlideState(Controller);
			case GoatState.Jump:
				return new JumpState(Controller);
			case GoatState.Yell:
				break;
			case GoatState.Die:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(state), state, null);
		}
		return null;
	}
}

public class RunState : State {
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
		EventManager.Subscribe <Event_JumpButtonPushed>(this, OnJumpButtonPushed);
		EventManager.Subscribe<Event_SlideButtonPushed>(this, OnSlideButtonPushed);
	}

	protected override void ProcessState() {
		base.ProcessState();
		Controller.SetRunSpeed(Controller.RunSpeed);
	}

	protected override void LeaveState() {
		base.LeaveState();
		EventManager.Unsubscribe<Event_JumpButtonPushed> (OnJumpButtonPushed);
		EventManager.Unsubscribe<Event_SlideButtonPushed>(OnSlideButtonPushed);
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
}

public class JumpState : State {
	public JumpState(GoatController controller) : base(controller) {
		AvailableTransitions = new List<GoatState> {
			GoatState.Run,
			GoatState.Die,
			GoatState.Obstacle,
			GoatState.SlowDown,
		};
		Type = GoatState.Jump;
	}

	protected override void Init() {
		base.Init();
		EventManager.Subscribe<Event_PhysicsObjectGrounded>(this, OnGrounded);
		Controller.Jump();
	}

	protected override void LeaveState() {
		base.LeaveState();
		EventManager.Unsubscribe<Event_PhysicsObjectGrounded>(OnGrounded);
	}

	void OnGrounded(Event_PhysicsObjectGrounded e) {
		if ( e.Object != Controller.CharController ) {
			return;
		}
		TryChangeState(new RunState(Controller));
	} 
}

public class SlideState : State {
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

public class SlowDownState : State {
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
		Controller.SetRunSpeed(Controller.SlowSpeed);
		EventManager.Subscribe<Event_JumpButtonPushed>(this, OnJumpButtonPushed);
	}

	protected override void LeaveState() {
		base.LeaveState();
		Debug.Log("SlowDown off");
		EventManager.Unsubscribe<Event_JumpButtonPushed>(OnJumpButtonPushed);
	}

	void OnJumpButtonPushed(Event_JumpButtonPushed e) {
		if ( Controller.CharController.Grounded ) {
			TryChangeState(new SlideState(Controller));
		}
	}
}

public class GoatController : MonoBehaviour {
	public float JumpForce = 7f;
	public float RunSpeed  = 3f;
	public float SlowSpeed = 2f;
	public float ObstacleSpeed = 0.5f;

	public PhysicsObject CharController = null;

	[NonSerialized] public State CurrentState = null;

	float _targetSpeed = 0f;
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

		//TODO: TEMP
		if ( Input.GetKey(KeyCode.Space) ) {
			EventManager.Fire(new Event_JumpButtonPushed());
		}

		if ( Input.GetKey(KeyCode.S) ) {
			EventManager.Fire(new Event_SlideButtonPushed());
		}

		if ( Input.GetKeyUp(KeyCode.S) ) {
			EventManager.Fire(new Event_SlideButtonReleased());
		}

	}

	void OnGUI() {
		GUI.Label(new Rect(new Vector2(10,10), new Vector2(100,20)), string.Format("State: {0}", CurrentState.StateType ) );
	}

	public void Jump() {
		CharController.Jump(JumpForce);
	}

	public void Crouch() {
		CharController.SetLowProfile(true);
	}

	public void StandUp() {
		CharController.SetLowProfile(false);
	}

	public void SetRunSpeed(float speed, bool instant = false) {
		_targetSpeed = speed;
		if ( instant ) {
			_currentSpeed = speed;
		}
	}
}
