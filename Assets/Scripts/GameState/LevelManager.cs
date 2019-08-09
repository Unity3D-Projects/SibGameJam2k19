using UnityEngine.SceneManagement;

using SMGCore;
using SMGCore.EventSys;
using KOZA.Events;


public sealed class LevelManager : MonoSingleton<LevelManager> {

	const string LevelCommonSceneName = "LevelCommon";
	const string EndSceneName         = "EndScene";
	const string MainMenuScreneName   = "MainMenu";

	string    _curLoadingLevel = null;
	SceneType _curLoadType     = SceneType.Generic;

	protected override void Awake() {
		base.Awake();
		DontDestroyOnLoad(this.gameObject);
		SceneManager.sceneLoaded += OnSceneLoaded;
		CurrentScene = SceneManager.GetActiveScene().name;
	}

	void OnDestroy() {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	public string CurrentScene { get; private set; }

	public void LoadScene(string sceneName, SceneType loadType) {
		_curLoadingLevel = sceneName;
		_curLoadType     = loadType;
		if ( loadType == SceneType.LevelScene ) {
			SceneManager.LoadScene(LevelCommonSceneName);
			return;
		}
		SceneManager.LoadScene(_curLoadingLevel);
	}

	public void LoadLevel(string levelName) {
		LoadScene(levelName, SceneType.LevelScene);
	}

	public void LoadMainMenu() {
		LoadScene(MainMenuScreneName, SceneType.Generic);
	}

	public void LoadEndScene() {
		LoadScene(EndSceneName, SceneType.Generic);
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		if ( scene.name == LevelCommonSceneName ) {
			SceneManager.LoadScene(_curLoadingLevel, LoadSceneMode.Additive);
			return;
		}
		EventManager.Fire(new Event_SceneLoaded { SceneName = _curLoadingLevel, SceneType = _curLoadType });
		CurrentScene = _curLoadingLevel;
		_curLoadingLevel = null;
		_curLoadType     = SceneType.Generic;
	}
}

public enum SceneType {
	Generic,
	LevelScene
}
