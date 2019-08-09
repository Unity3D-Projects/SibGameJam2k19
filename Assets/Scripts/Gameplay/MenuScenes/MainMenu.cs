using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

using SMGCore;

public sealed class MainMenu : MonoBehaviour {
    [Header("Buttons")]
    public Button     StartButton    = null;
    public Button     SoundButton    = null;
    public Button     ExitButton     = null;

    [Header("Button Sprites")]
    public Sprite     SoundOnSprite  = null;
    public Sprite     SoundOffSprite = null;
	[Header("Utilities")]
	public FadeScreen Fader          = null;

	bool _soundOn = false;

	[Header("Level Selection")]
	public GameObject   LevelChoiceCanvas      = null;
	public Button       LevelChoiceCloseButton = null;
	public List<Button> LevelButtons           = null;

	string _levelName = null;

	void Start() {
        Cursor.visible = true;
		Fader.FadeToWhite(1f, false);

        _soundOn = AudioListener.volume > 0.05f;
        SoundButton.image.sprite = _soundOn ? SoundOnSprite : SoundOffSprite;
        StartButton.onClick.AddListener(StartNewGame);
        SoundButton.onClick.AddListener(OnClickSoundToggle);
        ExitButton.onClick.AddListener(OnClickExit);
        if ( Application.platform == RuntimePlatform.WebGLPlayer ) {
            ExitButton.gameObject.SetActive(false);
        }

		foreach ( var button in LevelButtons ) {
			button.onClick.AddListener(delegate { StartLevel(button.GetComponent<LevelButton>().LevelName); });
		}
		LevelChoiceCloseButton.onClick.AddListener(OnClickClose);

		SoundManager.Instance.PlayMusic("menu");
		LevelChoiceCanvas.gameObject.SetActive(false);
    }

	void StartNewGame() {
		LevelChoiceCanvas.SetActive(true);
	}

	void StartLevel(string name) {
		_levelName = name;
		SoundManager.Instance.PlaySound("menuClick");
		Fader.OnFadeToBlackFinished.AddListener(() => LevelManager.Instance.LoadLevel(name));
		Fader.FadeToBlack(1f); 
	}
	
    public void OnClickSoundToggle() {
        _soundOn = !_soundOn;
        SoundButton.image.sprite = _soundOn ? SoundOnSprite : SoundOffSprite;
        AudioListener.volume = _soundOn ? 1f : 0f;
    }

	public void PlaySound(string soundName) {
		SoundManager.Instance.PlaySound(soundName);
	}

    void OnClickExit() {
		SoundManager.Instance.PlaySound("menuClick");
		Fader.OnFadeToBlackFinished.AddListener(ExitGame);
		Fader.FadeToBlack(0.5f);
	}

	void OnClickClose() {
		LevelChoiceCanvas.SetActive(false);
	}

	void ExitGame() {
		Application.Quit();
	}
}
