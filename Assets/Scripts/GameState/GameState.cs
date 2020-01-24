using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using SMGCore;
using SMGCore.EventSys;
using KOZA.Events;

using TMPro;

public sealed class GameState : MonoSingleton<GameState> {

	public Dialog           StartDialog    = null;
	public CamFollow2D      CamControl     = null;
	public GameObject       GoatCloneFab   = null;

	[Header("UI")]
	public TMP_Text         ScoreCountText = null;
	public GameObject       UICanvas       = null;
	public GameObject       FinishWindow   = null;
	public GameObject       HelpScreen     = null;
	public GameObject       DialogBG       = null;
	public List<BoostInfo>  BoostInfos     = new List<BoostInfo>();
	public List<GameObject> BoostButtons   = new List<GameObject>();

	[Header("Utilities")]
	public FadeScreen  Fader  = null;
	public CameraShake Shaker = null;

	[System.NonSerialized]
	public int Score = 0;
	public int ApplesCounter = 0;
	public bool ObstacleHit = false;

	public readonly TimeController TimeController = new TimeController();
	public readonly BoostWatcher   BoostWatcher   = new BoostWatcher();

	public GoatController Goat { get; private set; }
	public FarmerController Farmer { get; private set; }

	protected override void Awake() {
		base.Awake();

		EventManager.Subscribe<Event_Obstacle_Collided>(this, OnGoatHitObstacle);
		EventManager.Subscribe<Event_GoatDies>(this, OnGoatDie);
		EventManager.Subscribe<Event_AppleCollected>(this, OnAppleCollect);
		EventManager.Subscribe<Event_GameWin>(this, OnHitWinTrigger);
		EventManager.Subscribe<Event_StartDialogComplete>(this, OnDialogComplete);
		EventManager.Subscribe<Event_HelpScreenClosed>(this, OnHelpClosed);
		EventManager.Subscribe<Event_GoatYell>(this, OnGoatYell);
		EventManager.Subscribe<Event_SceneLoaded>(this, OnSceneLoaded);

	}

	void OnDestroy() {
		EventManager.Unsubscribe<Event_Obstacle_Collided>(OnGoatHitObstacle);
		EventManager.Unsubscribe<Event_GoatDies>(OnGoatDie);
		EventManager.Unsubscribe<Event_AppleCollected>(OnAppleCollect);
		EventManager.Unsubscribe<Event_GameWin>(OnHitWinTrigger);
		EventManager.Unsubscribe<Event_StartDialogComplete>(OnDialogComplete);
		EventManager.Unsubscribe<Event_HelpScreenClosed>(OnHelpClosed);
		EventManager.Unsubscribe<Event_GoatYell>(OnGoatYell);
		EventManager.Unsubscribe<Event_SceneLoaded>(OnSceneLoaded);
		BoostWatcher.DeInit();

		AdvertisementController.HideBannerAd();
	}

	void Update() {
		TimeController.Update(Time.deltaTime);
		BoostWatcher.Update();
		HandleInput();
	}

	public bool IsDebug {
		get {
			return false; //Should be set to 'false' for release build.
		}
	}

	void SetupLevel() {
		UICanvas.gameObject.SetActive(true); 

		Goat = FindObjectOfType<GoatController>();
		Farmer = FindObjectOfType<FarmerController>();

		var camFollow = FindObjectOfType<CamFollow2D>();
		if ( !Goat || !Farmer || !camFollow ) {
			Debug.LogError("Wrong level setup");
			return;
		}
		//TODO: better player init
		camFollow.player = Goat.transform;
		Farmer.gameObject.SetActive(false);
		Goat.gameObject.SetActive(false);

		var ls = LevelSettings.Instance;

		//Kostyly
		if ( ls.ExcludeYell ) {
			UICanvas.transform.Find("ButtonYell").gameObject.SetActive(false);
		}
		if ( ls.ExcludeClone ) {
			UICanvas.transform.Find("ButtonClone").gameObject.SetActive(false); 
		}
		if ( ls.ExcludeMushroom ) {
			UICanvas.transform.Find("ButtonSpeedUp").gameObject.SetActive(false); 
		}
		if ( ls.ExcludePiano ) {
			UICanvas.transform.Find("ButtonPiano").gameObject.SetActive(false); 
		}
		if ( ls.ChangeFarmerSpeed ) {
			Farmer.MoveSpeed = ls.NewFarmerSpeed; 
		}

		BoostWatcher.Init(this);

		HelpScreen.gameObject.SetActive(false);
		FinishWindow.gameObject.SetActive(false);
		if ( ScenePersistence.Instance.Data == null ) {
			ScenePersistence.Instance.SetupHolder(new KOZAPersistence());
		}
		var persistence = ScenePersistence.Instance.Data as KOZAPersistence;
		if ( persistence.FastRestart ) {
			DialogBG.gameObject.SetActive(false);
			EventManager.Fire(new Event_HelpScreenClosed());
		} else {
			var hasDialog = (ls.Dialog != null);
			if ( hasDialog ) {
				DialogBG.gameObject.SetActive(true);
				var d = Instantiate(ls.Dialog, DialogBG.transform);
				d.transform.SetAsFirstSibling();
				StartDialog = d.GetComponent<Dialog>();
			}
			if ( !hasDialog && ls.ShowTutorial ) {
				EventManager.Fire(new Event_StartDialogComplete());
			}
			if ( !ls.ShowTutorial ) {
				HelpScreen.gameObject.SetActive(false);
				EventManager.Fire(new Event_HelpScreenClosed());
			}
		}
		var cachedFailCount = 0;
		var cachedAddScore = persistence.AdditionalScore;
		if ( persistence.FastRestart ) {
			cachedFailCount = persistence.ConsecutiveFailCount;
		}		
		ScenePersistence.Instance.SetupHolder(new KOZAPersistence() { ConsecutiveFailCount = cachedFailCount, AdditionalScore = cachedAddScore });
		Score = cachedAddScore;
		Fader.FadeToWhite(1f);
		ScoreCountText.text = string.Format("x{0}", Score);
		UpdateBoostButtonsAvailability();
	}

	void OnSceneLoaded(Event_SceneLoaded e) {

		if ( e.SceneType == SceneType.LevelScene ) {
			SetupLevel();
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
		OnLevelEnd(false);
		var sm = LevelManager.Instance;
		Fader.OnFadeToBlackFinished.AddListener(sm.LoadEndScene);
	}

	void WinGame() {
		OnLevelEnd(true); 

		var ls = LevelSettings.Instance;
		var sm = LevelManager.Instance; 

		if ( !PlayerPrefs.HasKey(sm.CurrentScene) || (PlayerPrefs.GetInt(sm.CurrentScene) < ApplesCounter) ) {
			PlayerPrefs.SetInt(sm.CurrentScene, ApplesCounter);
			PlayerPrefs.Save();
		}
		if ( !ObstacleHit ) {
			if ( !PlayerPrefs.HasKey(sm.CurrentScene + ".Star") ) {
				PlayerPrefs.SetInt(sm.CurrentScene + ".Star", 1);
			}
		}

		if ( ls.CompleteAction == LevelCompleteAction.FinalScene ) {
			Fader.OnFadeToBlackFinished.AddListener(sm.LoadEndScene);
		} else if ( ls.CompleteAction == LevelCompleteAction.NextLevel ) {
			var nextLevel = ls.NextSceneName;
			Fader.OnFadeToBlackFinished.AddListener(() => sm.LoadLevel(nextLevel));
		} else if ( ls.CompleteAction == LevelCompleteAction.ContinueWindow ) {
			Fader.OnFadeToBlackFinished.AddListener(OnContinueWindowShow);
		} else if ( ls.CompleteAction == LevelCompleteAction.MainMenu ) {
			Fader.OnFadeToBlackFinished.AddListener(sm.LoadMainMenu);
		}
	}

	void OnLevelEnd(bool win) {
		var persistence           = ScenePersistence.Instance.Data as KOZAPersistence;
		persistence.IsWin         = win;
		persistence.LastLevelName = LevelManager.Instance.CurrentScene;
		persistence.TotalDistance = Goat.RunDistance;
		persistence.EndlessLevel  = LevelSettings.Instance.Endless;
		if ( LevelSettings.Instance.Endless ) {
			var maxDist = PlayerPrefs.GetFloat("Gameplay.EndlessRecord");
			if ( maxDist < Goat.RunDistance ) {
				PlayerPrefs.SetFloat("Gameplay.EndlessRecord", Goat.RunDistance);
			}
		}

		if ( win ) {
			AnalyticsController.Instance.LevelWin(LevelManager.Instance.CurrentScene, Score, persistence.AdditionalScore > 0);
		} else {
			AnalyticsController.Instance.LevelLose(LevelManager.Instance.CurrentScene, Score, persistence.AdditionalScore > 0);
		}
		

		Fader.FadeToBlack(1f);
	}

	void OnContinueWindowShow() {
		FinishWindow.SetActive(true);
		var fv = FinishWindow.GetComponent<FinishWindow>();
		if ( AdvertisementController.Instance.IsCanShowAd(AdvertisementController.LevelWinBanner) ) {
			AdvertisementController.Instance.ShowBannerAd(AdvertisementController.LevelWinBanner);
		}
		fv.UpdateApplesCounter();
		fv.UpdateStar();
		fv.UpdateResetButton(); 
	}

	void OnDialogComplete(Event_StartDialogComplete e) {
		HelpScreen.SetActive(true);
		HelpScreen.transform.Find(LevelSettings.Instance.TutorialName).gameObject.SetActive(true);
	}

	void OnHelpClosed(Event_HelpScreenClosed e) {
		Farmer.gameObject.SetActive(true);
		Goat.gameObject.SetActive(true);
		SoundManager.Instance.PlayMusic("level");
	}

	void OnGoatYell(Event_GoatYell e) {
		if ( Shaker != null ) {
			Shaker.ShakeCamera(1, 1);
			Shaker.Decay();
		}
	}

	void OnGoatHitObstacle(Event_Obstacle_Collided e) {
		if ( !Goat ) {
			return;
		}

		if ( !ObstacleHit ) {
			ObstacleHit = true; 
		}

		if ( e.Obstacle.Type == ObstacleType.Bush ) {
			Goat.CurrentState.TryChangeState(new SlowDownState(Goat, 1f));
			return;
		}

		if ( e.Obstacle.Type == ObstacleType.Bee ) {
			Goat.CurrentState.TryChangeState(new SlowDownState(Goat, 2f));
			return;
		}

		if ( e.Obstacle.Type == ObstacleType.Stump ) {
			Goat.CurrentState.TryChangeState(new ObstacleState(Goat));
			return;
		}

		if ( e.Obstacle.Type == ObstacleType.Rock ) {
			Goat.CurrentState.TryChangeState(new ObstacleState(Goat));
			return;
		}

		if ( e.Obstacle.Type == ObstacleType.Hedgehog ) {
			Goat.CurrentState.TryChangeState(new ObstacleState(Goat));
			return;
		}
	}

	void OnGoatDie(Event_GoatDies e) {

		//костыль пока, может где-то еще это проверяется но я не нашел 
		var persistence           = ScenePersistence.Instance.Data as KOZAPersistence;
		if ( persistence.IsWin ) {
			return; 
		} 
		//

		CamControl.ReplaceTargetByDummy();
		LoseGame();
		SoundManager.Instance.PlaySound("Slap");
	}

	void OnHitWinTrigger(Event_GameWin e) {
		WinGame();
	}

	void OnAppleCollect(Event_AppleCollected e) {
		Score++;
		ApplesCounter++;
		EventManager.Fire(new Event_ScoreChanged { NewScore = Score });
		ScoreCountText.text = string.Format("x{0}", Score);
		UpdateBoostButtonsAvailability();
	}

	public void SpendScore(int count) {
		Score -= count;
		EventManager.Fire(new Event_ScoreChanged { NewScore = Score });
		ScoreCountText.text = string.Format("x{0}", Score);
		UpdateBoostButtonsAvailability();
	}

	public void UpdateBoostButtonsAvailability() {
		foreach ( var button in BoostButtons ) {
			var type = button.GetComponent<BoostButton>().BoostType;
			if ( !Instance.BoostWatcher.CanActivateBoost(type) ) {
				button.GetComponent<Button>().interactable = false;
				continue;
			}
			var price =BoostWatcher.GetBoostPrice(type);
			if ( Score >= price ) {
				button.GetComponent<Button>().interactable = true;
			} else {
				button.GetComponent<Button>().interactable = false;
			} 
		}
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

	public bool CanActivateBoost(BoostType type) {
		if ( _activeBoost != null ) {
			return false;
		}
		switch ( type ) {
			case BoostType.SpeedUp:
				return true;
			case BoostType.Clone:
				return true;
			case BoostType.Piano:
				return true;
				//return _owner.Farmer.Controller.Grounded;
			default:
				break;
		}
		return false;
	}

	void OnBoostButtonPush(Event_TryActivateBoost e) {
		if (!CanActivateBoost(e.Type) ) {
			return;
		}
		var price = GetBoostPrice(e.Type);
		if ( _owner.Score >= price ) {
			_owner.SpendScore(price);
			_activeBoost = GetAction(e.Type);
		}
		_owner.UpdateBoostButtonsAvailability();
	}

	void OnBoostEnded(Event_BoostEnded e) {
		_activeBoost = null;
		_owner.UpdateBoostButtonsAvailability();
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
	public virtual BoostType Type { get { return BoostType.SpeedUp; } }
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
	public override BoostType Type { get { return BoostType.SpeedUp; } }
	public SpeedUpBoostAction() :base() {	
	GameState.Instance.Goat.CharController.HorizontalSpeedMultiplier = 1.5f;
		SoundManager.Instance.PlaySound("Speedup");
	}

	protected override void DeInit() {
		base.DeInit();
		GameState.Instance.Goat.CharController.HorizontalSpeedMultiplier = 1f;
	}
}

public class CloneBoostAction : BoostAction {
	public override BoostType Type { get { return BoostType.Clone; } }

	FarmerActionTrigger _trigger = null;
	public CloneBoostAction() : base() {
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
	public override BoostType Type { get { return BoostType.Piano; } }

	public PianoBoostAction() : base() {
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
