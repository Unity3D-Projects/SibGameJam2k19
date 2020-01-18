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
	[Header("Buttons")]
	public Button     WatchAdButton        = null;
	public GameObject BeforeWatchAdContent = null;
	public GameObject AfterWatchAdContent  = null;
	[Header("Utilities")]
	public FadeScreen Fader = null;

	bool _closing   = false;
	bool _adWatched = false;

	void Start() {
		var pData = ScenePersistence.Instance.Data as KOZAPersistence;
		var isWin = pData.IsWin;
		var isEndless = pData.EndlessLevel;
		pData.ConsecutiveFailCount++;

		WinGameHolder.SetActive (isWin && !isEndless);
		LoseGameHolder.SetActive(!isWin && !isEndless);
		EndlessGameHolder.SetActive(isEndless);
		if ( isEndless ) {
			SetupEndless(pData.TotalDistance);
		}
		SetupWatchAdButton(pData.ConsecutiveFailCount);
		Fader.FadeToWhite(1f);

		if ( (isWin || isEndless) && AdvertisementController.Instance.IsCanShowAd(AdvertisementController.LevelWinBanner) ) {
			AdvertisementController.Instance.ShowBannerAd(AdvertisementController.LevelWinBanner);
		}
	}

	void Update() {
		if ( Input.GetKeyDown(KeyCode.Escape) ) {
			GoToStart();
		}
	}

	void OnDestroy() {
		AdvertisementController.Instance.HideBannerAd();
	}

	void SetupWatchAdButton(int failCount) {
		var ac = AdvertisementController.Instance;
		if ( failCount >= ac.NextLevelHelpRewardFailCount && ac.IsCanShowAd(AdvertisementController.NextLevelHelpReward)) {
			WatchAdButton.gameObject.SetActive(true);
			BeforeWatchAdContent.SetActive(true);
			AfterWatchAdContent.SetActive(false);
		} else {
			WatchAdButton.gameObject.SetActive(false);
		}
	}

	void OnAdWatchFinished(bool success) {
		if ( !success ) {
			return;
		}
		BeforeWatchAdContent.SetActive(false);
		AfterWatchAdContent.SetActive(true);
		var pData = ScenePersistence.Instance.Data as KOZAPersistence;
		pData.AdditionalScore = 3;
		_adWatched = true;
	}

	public void WatchAd() {
		if ( _adWatched ) {
			return;
		}
		AdvertisementController.Instance.ShowVideoAd(AdvertisementController.NextLevelHelpReward, OnAdWatchFinished);
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
