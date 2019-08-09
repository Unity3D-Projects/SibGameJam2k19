using UnityEngine;

using SMGCore;

public class LevelSettings : MonoSingleton<LevelSettings> {
	public LevelCompleteAction CompleteAction = LevelCompleteAction.NextLevel;
	public string              NextSceneName  = null;
	public string              DialogName     = null;
	public bool                ShowTutorial   = false;
}

public enum LevelCompleteAction {
	NextLevel,
	FinalScene,
	MainMenu
}
