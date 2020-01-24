using UnityEngine;

using SMGCore;

public class LevelSettings : MonoSingleton<LevelSettings> {
	public LevelCompleteAction CompleteAction = LevelCompleteAction.NextLevel;
	public string              NextSceneName  = null;
	public GameObject          Dialog         = null;
	public bool                ShowTutorial   = false;
	public string              TutorialName   = null;
	public int                 ApplesNumber   = 0;
	public bool                Endless        = false;

	[Header("Excluding Perks")] 
	public bool ExcludeYell     = false;
	public bool ExcludeClone    = false;
	public bool ExcludeMushroom = false;
	public bool ExcludePiano    = false;

	[Header("Speed")]
	public bool  ChangeFarmerSpeed = false;
	public float NewFarmerSpeed    = 0;
}

public enum LevelCompleteAction {
	ContinueWindow,
	NextLevel,
	FinalScene,
	MainMenu
}
