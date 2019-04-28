using System.Collections.Generic;

using EventSys;
using UnityEngine;
using UnityEngine.SceneManagement;
using  TMPro;

public sealed class GameState : MonoSingleton<GameState> {

	public GoatController   Goat       = null;
	public FarmerController Farmer     = null;
	public CamControl       CamControl = null;
	public TMP_Text         ScoreCountText = null;

	public GameObject       GoatCloneFab = null;

	public List<BoostInfo> BoostInfos = new List<BoostInfo>();

	[System.NonSerialized]
	public int Score = 10;


	[Header("Utilities")]
	public FadeScreen Fader = null;

	public readonly TimeController TimeController = new TimeController();
	public readonly BoostWatcher   BoostWatcher   = new BoostWatcher();

	protected override void Awake() {
		base.Awake();
		ScenePersistence.Instance.ClearData(); // move this if need to use data from previous tries.

		SoundManager.Instance.PlayMusic("level");
		EventManager.Subscribe<Event_Obstacle_Collided>(this, OnGoatHitObstacle);
		EventManager.Subscribe<Event_GoatDies>(this, OnGoatDie);
		EventManager.Subscribe<Event_AppleCollected>(this, OnAppleCollect);
		EventManager.Subscribe<Event_GameWin>(this, OnHitWinTrigger);
		BoostWatcher.Init(this);
		Fader.FadeToWhite(1f);
		ScoreCountText.text = string.Format("x{0}", Score);
	}

	void OnDestroy() {
		EventManager.Unsubscribe<Event_Obstacle_Collided>(OnGoatHitObstacle);
		EventManager.Unsubscribe<Event_GoatDies>(OnGoatDie);
		EventManager.Unsubscribe<Event_AppleCollected>(OnAppleCollect);
		EventManager.Unsubscribe<Event_GameWin>(OnHitWinTrigger);
		BoostWatcher.DeInit();
	}

	void Update() {
		TimeController.Update(Time.deltaTime);
		BoostWatcher.Update();
		HandleInput();
	}

	public bool IsDebug {
		get {
			return true; //Should be set to 'false' for release build.
		}
	}

	void HandleInput() {
		if ( IsDebug ) {
			if ( Input.GetKeyDown(KeyCode.P) ) {
				var pauseFlag = TimeController.AddOrRemovePause(this);
				Debug.LogFormat("Pause cheat used, new pause state is: {0}", pauseFlag);
			}

			if ( Input.GetKeyDown(KeyCode.LeftBracket) ) {
				Debug.Log("Win game cheat");
				WinGame();
			}

			if ( Input.GetKeyDown(KeyCode.RightBracket) ) {
				Debug.Log("Lose game cheat");
				LoseGame();
			}
		}
	}

	void LoseGame() {
		ScenePersistence.Instance.Data.IsWin = false;
		Fader.FadeToBlack(1f);
		Fader.OnFadeToBlackFinished.AddListener(GoToEndScene);
	}

	void WinGame() {
		ScenePersistence.Instance.Data.IsWin = true;
		Fader.FadeToBlack(1f);
		Fader.OnFadeToBlackFinished.AddListener(GoToEndScene);
	}

	void GoToEndScene() {
		SceneManager.LoadScene("EndScene");
	}


	void OnGoatHitObstacle(Event_Obstacle_Collided e) {
		if ( !Goat ) {
			return;
		}

		if ( e.Obstacle.Type == ObstacleType.Bush ) {
			Goat.CurrentState.TryChangeState(new SlowDownState(Goat));
			return;
		}

		if ( e.Obstacle.Type == ObstacleType.Stump ) {
			Goat.CurrentState.TryChangeState(new ObstacleState(Goat));
			return;
		}
	}

	void OnGoatDie(Event_GoatDies e) {
		CamControl.ReplaceTargetByDummy();
		LoseGame();
		SoundManager.Instance.PlaySound("Slap");
		//TODO: Game Over
	}

	void OnHitWinTrigger(Event_GameWin e) {
		WinGame();
	}

	void OnAppleCollect(Event_AppleCollected e) {
		Score++;
		EventManager.Fire(new Event_ScoreChanged {NewScore = Score});
		ScoreCountText.text = string.Format("x{0}", Score);
	}

	public void SpendScore(int count) {
		Score -= count;
		EventManager.Fire(new Event_ScoreChanged { NewScore = Score });
		ScoreCountText.text = string.Format("x{0}", Score);
	}
}

public class BoostWatcher {

	GameState _owner = null;
	BoostAction _activeBoost = null;


	public void Init(GameState owner) {
		_owner = owner;
		EventManager.Subscribe<Event_TryActivateBoost>(this, OnBoostButtonPush);
		EventManager.Subscribe<Event_TryActivateBoost>(this, OnBoostButtonPush);
		EventManager.Subscribe<Event_BoostEnded>(this, OnBoostEnded);
	}

	public void DeInit() {
		EventManager.Unsubscribe<Event_TryActivateBoost>(OnBoostButtonPush);
		EventManager.Unsubscribe<Event_BoostEnded>(OnBoostEnded);
	}

	public void Update() {
		if ( _activeBoost != null ) {
			_activeBoost.Update();
		}

		if ( Input.GetKeyDown(KeyCode.Alpha1) ) {
			EventManager.Fire(new Event_TryActivateBoost() { Type = BoostType.SpeedUp });
		}
		if ( Input.GetKeyDown(KeyCode.Alpha2) ) {
			EventManager.Fire(new Event_TryActivateBoost() { Type = BoostType.Clone });
		}
		if ( Input.GetKeyDown(KeyCode.Alpha3) ) {
			EventManager.Fire(new Event_TryActivateBoost() { Type = BoostType.Piano });
		}
	}

	void OnBoostButtonPush(Event_TryActivateBoost e) {
		if ( _activeBoost != null || !_owner.Goat.CharController.Grounded || !_owner.Farmer.Controller.Grounded ) {
			// TODO reject effect
			return;
		}
		var price = GetBoostPrice(e.Type);
		if ( _owner.Score >= price ) {
			_owner.SpendScore(price);
			_activeBoost = GetAction(e.Type);
		}
	}

	void OnBoostEnded(Event_BoostEnded e) {
		_activeBoost = null;
	}

	BoostAction GetAction(BoostType type) {
		if ( type == BoostType.Clone ) {
			return new CloneBoostAction() { Info = GetBoostInfo(type) };
		}
		if ( type == BoostType.Piano ) {
			return new PianoBoostAction() { Info = GetBoostInfo(type) };
		}
		if ( type == BoostType.SpeedUp ) {
			return new SpeedUpBoostAction() { Info = GetBoostInfo(type) };
		}
		return new BoostAction();
	}

	public BoostInfo GetBoostInfo(BoostType type) {
		foreach ( var info in _owner.BoostInfos ) {
			if ( info.Type == type ) {
				return info;
			}
		}
		return null;
	}

	public int GetBoostPrice(BoostType type) {
		var info = GetBoostInfo(type);
		return info != null ? info.Price : 0;
	}
}

[System.Serializable]
public class BoostInfo {
	public BoostType Type       = BoostType.SpeedUp;
	public int       Price      = 1;
	public float     EffectTime = 3f;
}

public class BoostAction {
	public BoostType Type = BoostType.SpeedUp;
	public BoostInfo Info = null;
	float _timeCreated = 0f;

	public BoostAction() {
		EventManager.Fire(new Event_BoostActivated() {Type = Type}) ;
		_timeCreated = GameState.Instance.TimeController.CurrentTime;
	}

	public void Update() {
		var curTime = GameState.Instance.TimeController.CurrentTime;
		if ( curTime > _timeCreated + Info.EffectTime ) {
			DeInit();
		}
	}

	protected virtual void DeInit() {
		EventManager.Fire(new Event_BoostEnded { Type = Type });
	}
}

public class SpeedUpBoostAction : BoostAction {
	public SpeedUpBoostAction() :base() {
		Type = BoostType.SpeedUp;
		GameState.Instance.Goat.CharController.HorizontalSpeedMultiplier = 1.5f;
		SoundManager.Instance.PlaySound("Speedup");
	}

	protected override void DeInit() {
		base.DeInit();
		GameState.Instance.Goat.CharController.HorizontalSpeedMultiplier = 1f;
	}
}

public class CloneBoostAction : BoostAction {

	FarmerActionTrigger _trigger = null;
	public CloneBoostAction() : base() {
		Type = BoostType.Clone;
		var gs = GameState.Instance;
		var fakeGoat = GameObject.Instantiate(gs.GoatCloneFab, gs.Goat.transform.position, gs.Goat.transform.rotation);
		fakeGoat.GetComponent<TimedDestroy>().Activate(4f);
		_trigger = fakeGoat.GetComponentInChildren<FarmerActionTrigger>();
		EventManager.Subscribe<Event_FarmerActionTrigger>(this, OnFarmerHitObstacle);
		SoundManager.Instance.PlaySound("Clone");
	}

	protected override void DeInit() {
		base.DeInit();
		GameState.Instance.Farmer.Controller.HorizontalSpeedMultiplier = 1f;
		EventManager.Unsubscribe<Event_FarmerActionTrigger>(OnFarmerHitObstacle);
		
	}

	void OnFarmerHitObstacle(Event_FarmerActionTrigger e) {
		if ( e.Trigger == _trigger ) {
			var gs = GameState.Instance;
			gs.Farmer.Controller.HorizontalSpeedMultiplier = 0.6f;
			SoundManager.Instance.PlaySound("Anger2");
		}
	}
}

public class PianoBoostAction : BoostAction {
	public PianoBoostAction() : base() {
		Type = BoostType.Piano;
		var gs = GameState.Instance;
		gs.Farmer.Controller.HorizontalSpeedMultiplier = 0.0f;
		gs.Farmer.GetComponent<Animation>().Play("PianoFall");
		SoundManager.Instance.PlaySound("PianoFall");
	}

	protected override void DeInit() {
		base.DeInit();
		GameState.Instance.Farmer.Controller.HorizontalSpeedMultiplier = 1f;
		var anim = GameState.Instance.Farmer.GetComponent<Animation>();
		anim.Rewind();
		anim.Stop("PianoFall");
		
		anim.Play("FarmerRun");
	}
}