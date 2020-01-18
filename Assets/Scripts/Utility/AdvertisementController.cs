using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

using SMGCore;
using SMGCore.EventSys;
using KOZA.Events;
using System;

public class AdvertisementController : MonoSingleton<AdvertisementController> {

	public static readonly string LevelWinBanner = "LevelWinBanner";
	public static readonly string NextLevelHelpReward = "NextLevelHelpReward";

#if UNITY_ANDROID
	readonly bool   _enabled   = true;
	readonly bool   _testMode  = true;
	readonly string _gameAdsID = "3436080";
#elif UNITY_IOS
	readonly bool   _enabled   = true;
	readonly bool   _testMode  = true;
	readonly string _gameAdsID = "3436081";
#else
	readonly bool   _enabled   = false;
	readonly bool   _testMode  = true;
	readonly string _gameAdsID = "none";
#endif

	AdsListener _adListener     = null;
	int         _adWatchCounter = 0;

	//AdsListener _listener;
	Dictionary<string, Action<bool>> _activeAds = new Dictionary<string, Action<bool>>();

	public int NextLevelHelpRewardFailCount {
		get {
			return 3;
		}
	}

	void Start() {
		DontDestroyOnLoad(gameObject);

		_adWatchCounter = PlayerPrefs.GetInt("WatchedAdCount");

		Advertisement.Initialize(_gameAdsID, _testMode);

		_adListener = new AdsListener(_activeAds, IncrementViews);
		Advertisement.AddListener(_adListener);
	}

	public bool IsCanShowAd(string placementName) {
		return _enabled && Advertisement.IsReady(placementName);
	}

	public void ShowVideoAd(string name, Action<bool> onFinish) {
		if ( _activeAds.ContainsKey(name) ) {
			Debug.LogErrorFormat("Ad  {0} is already in progress", name);
			onFinish?.Invoke(false);
			return;
		}

		_activeAds.Add(name, onFinish);
		Advertisement.Show(name);
	}

	public void ShowBannerAd(string placement) {
		if ( !IsCanShowAd(placement) ) {
			return;
		}
		Advertisement.Banner.SetPosition(BannerPosition.TOP_CENTER);
		Advertisement.Banner.Show(placement);
	}

	public static void HideBannerAd() {
		Advertisement.Banner.Hide();
	}

	void IncrementViews() {
		_adWatchCounter++;
		PlayerPrefs.SetInt("WatchedAdCount", _adWatchCounter);
		Debug.Log("Ad watched.");
		//TODO: some analytics  or something
	}

}
sealed class AdsListener : IUnityAdsListener {
	readonly Dictionary<string, Action<bool>> _activeAds;
	Action _afterWatch;
	public AdsListener(Dictionary<string, Action<bool>> activeAds, Action afterWatch) {
		_activeAds = activeAds;
		_afterWatch = afterWatch;
	}

	public void OnUnityAdsDidError(string message) {
		Debug.LogErrorFormat("Error in unity ads: {0}", message);
	}
	public void OnUnityAdsReady(string placementId) {
		EventManager.Fire(new OnAdReady(placementId));
	}

	public void OnUnityAdsDidStart(string placementId) {
	}

	public void OnUnityAdsDidFinish(string placementId, ShowResult showResult) {
		if ( _activeAds.ContainsKey(placementId) ) {
			_activeAds[placementId]?.Invoke(showResult == ShowResult.Finished);
			_activeAds.Remove(placementId);
			if ( showResult == ShowResult.Finished ) {
				_afterWatch();
			}
		} else {
			Debug.LogErrorFormat("Ad {0} isn't in active ads dict", placementId);
		}
	}
}
 