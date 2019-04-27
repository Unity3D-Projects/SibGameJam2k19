using EventSys;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameState : MonoSingleton<GameState> {

	public GoatController Goat = null;
	public CamControl CamControl = null;

	[Header("Utilities")]
	public FadeScreen Fader = null;

	public readonly TimeController TimeController = new TimeController();

	protected override void Awake() {
		base.Awake();
		ScenePersistence.Instance.ClearData(); // move this if need to use data from previous tries.

		SoundManager.Instance.PlayMusic("level");
		EventManager.Subscribe<Event_Obstacle_Collided>(this, OnGoatHitObstacle);
		EventManager.Subscribe<Event_GoatDies>(this, OnGoatDie);
	}

	void OnDestroy() {
		EventManager.Unsubscribe<Event_Obstacle_Collided>(OnGoatHitObstacle);
		EventManager.Unsubscribe<Event_GoatDies>(OnGoatDie);
	}

	void Update() {
		TimeController.Update(Time.deltaTime);
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
		//TODO: Game Over
	}
}
