using UnityEngine;
using UnityEngine.SceneManagement;

using SMGCore;

public sealed class EndGame : MonoBehaviour {
	[Header("Game Result Effects Holder")]
	public GameObject WinGameHolder  = null;
	public GameObject LoseGameHolder = null;
	[Header("Titles")]
	public GameObject EndTitles = null;
	[Header("Utilities")]
	public FadeScreen Fader = null;

	bool _closing = false;

	void Start() {
		var pData = ScenePersistence.Instance.Data;
		var isWin = (pData as KOZAPersistence).IsWin;
		WinGameHolder.SetActive (isWin );
		LoseGameHolder.SetActive(!isWin);

		Fader.FadeToWhite(1f);
	}

	void Update() {
		if ( Input.GetKeyDown(KeyCode.Escape) ) {
			GoToStart();
		}
	}

	public void GoToStart() {
		if ( _closing ) {
			return;
		}
		_closing = true;
		Fader.FadeToBlack(1f);
		Fader.OnFadeToBlackFinished.AddListener(LoadStartScene);
	}

	public void FastRestart() {
		if ( _closing ) {
			return;
		}
		_closing = true;
		Fader.FadeToBlack(1f);
		var persistence = ScenePersistence.Instance.Data as KOZAPersistence;
		persistence.FastRestart = true;
		Fader.OnFadeToBlackFinished.AddListener(LoadLevel);
	}

	void LoadStartScene() {
		SceneManager.LoadScene("MainMenu");
	}

	void LoadLevel() {
		SceneManager.LoadScene("Gameplay");
	}
}
