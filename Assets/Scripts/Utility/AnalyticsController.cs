using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Analytics;

using SMGCore;

public class AnalyticsController : MonoSingleton<AnalyticsController> {
	void Start() {
		DontDestroyOnLoad(gameObject);	
	}

	public void LevelWin(string levelName, int score, bool adHelp) {
		Debug.LogFormat("Analytics: level win: {0}, {1}", levelName, score);
		AnalyticsEvent.Custom("level_win", new Dictionary<string, object> {
			{"scene_name", levelName },
			{"score", score },
			{"with_ad", adHelp },
		});
	}

	public void LevelLose(string levelName, int score, bool adHelp) {
		Debug.LogFormat("Analytics: level lose: {0}, {1}", levelName, score);
		AnalyticsEvent.Custom("level_lose", new Dictionary<string, object> {
			{"scene_name", levelName },
			{"score", score },
			{"with_ad", adHelp },
		});
	}

	public void VideoAdWatchedOnLoseScreen(string levelName) {
		Debug.LogFormat("Analytics: ad watched: {0}", levelName);
		AnalyticsEvent.Custom("video_ad_lose_screen", new Dictionary<string, object> {
			{"scene_name", levelName }
		});
	}
}
