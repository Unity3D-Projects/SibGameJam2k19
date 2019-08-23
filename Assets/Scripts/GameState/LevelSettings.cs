using UnityEngine;

using SMGCore;

public class LevelSettings : MonoSingleton<LevelSettings> {
	public LevelCompleteAction CompleteAction = LevelCompleteAction.NextLevel;
	public string              NextSceneName  = null;
	public string              DialogName     = null;
	public bool                ShowTutorial   = false;
	public string              TutorialName   = null;
	public int                 ApplesNumber   = 0; 

	[Header("Excluding Perks")] 
	public bool ExcludeYell     = false;
	public bool ExcludePiano    = false;
	public bool ExcludeMushroom = false;
}

public enum LevelCompleteAction {
	ContinueWindow,
	NextLevel,
	FinalScene,
	MainMenu
}
