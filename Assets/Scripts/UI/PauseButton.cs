using UnityEngine;
using SMGCore;

public sealed class PauseButton : MonoBehaviour {

	public GameObject PauseMenu = null;
	public void OnClickPause() {
		var result =  GameState.Instance.TimeController.AddOrRemovePause(this);
		Time.timeScale = result ? 0f : 1f;
		if ( PauseMenu.activeSelf ) {
			PauseMenu.SetActive(false);
		} else {
			PauseMenu.SetActive(true);
		}
	}


	public void PauseResetClick() {
		var persistence = ScenePersistence.Instance.Data as KOZAPersistence;
		persistence.FastRestart = true;
		//Fader.OnFadeToBlackFinished.AddListener(() => LevelManager.Instance.LoadLevel(persistence.LastLevelName));
		LevelManager.Instance.LoadLevel(LevelManager.Instance.CurrentScene);
		AdvertisementController.HideBannerAd(); 
		OnClickPause();
	}

	public void PauseMainMenuClick() { 
		OnClickPause();
		LevelManager.Instance.LoadMainMenu();
	}
}
