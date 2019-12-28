using UnityEngine;
using UnityEngine.UI;

using SMGCore;

public sealed class EndGame : MonoBehaviour {
	[Header("Game Result Effects Holder")]
	public GameObject WinGameHolder     = null;
	public GameObject LoseGameHolder    = null;
	public GameObject EndlessGameHolder = null;
	[Header("Titles")]
	public GameObject EndTitles = null;
	[Header("Utilities")]
	public FadeScreen Fader = null;

	bool _closing = false;

	void Start() {
		var pData = ScenePersistence.Instance.Data as KOZAPersistence;
		var isWin = pData.IsWin;
		var isEndless = pData.EndlessLevel;

		WinGameHolder.SetActive (isWin && !isEndless);
		LoseGameHolder.SetActive(!isWin && !isEndless);
		EndlessGameHolder.SetActive(isEndless);
		if ( isEndless ) {
			SetupEndless(pData.TotalDistance);
		}
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
		Fader.OnFadeToBlackFinished.AddListener(LevelManager.Instance.LoadMainMenu);
	}

	public void FastRestart() {
		if ( _closing ) {
			return;
		}
		_closing = true;
		Fader.FadeToBlack(1f);
		var persistence = ScenePersistence.Instance.Data as KOZAPersistence;
		persistence.FastRestart = true;
		Fader.OnFadeToBlackFinished.AddListener(() => LevelManager.Instance.LoadLevel(persistence.LastLevelName));
	}

	void SetupEndless(float dist) {
		var txt = EndlessGameHolder.GetComponent<Text>();
		var lc = LocalizationController.Instance;
		var maxDist = PlayerPrefs.GetFloat("Gameplay.EndlessRecord");
		var str = lc.Translate("FinalScreen.RunMeter");
		txt.text = string.Format(str, dist.ToString("0"), maxDist.ToString("0"));
	}
}
