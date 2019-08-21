using UnityEngine;

using SMGCore;

public class LevelSettings : MonoSingleton<LevelSettings> {
	public LevelCompleteAction CompleteAction = LevelCompleteAction.NextLevel;
	public string              NextSceneName  = null;
	public string              DialogName     = null;
	public bool                ShowTutorial   = false;
	public int                 ApplesNumber   = 0;
}

public enum LevelCompleteAction {
	ContinueWindow,
	NextLevel,
	FinalScene,
	MainMenu
}
