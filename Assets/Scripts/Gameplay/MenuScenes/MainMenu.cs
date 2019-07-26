using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

		SoundManager.Instance.PlayMusic("menu");
    }

	void StartNewGame() {
		SoundManager.Instance.PlaySound("menuClick");
		Fader.OnFadeToBlackFinished.AddListener(LoadLevel);
		Fader.FadeToBlack(1f);
	}
	
    void LoadLevel() {
        //SceneManager.LoadScene("Gameplay");
        SceneManager.LoadScene("BaseLevelWithGrid");
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

	void ExitGame() {
		Application.Quit();
	}
}
