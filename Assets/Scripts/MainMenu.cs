using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public sealed class MainMenu : MonoBehaviour {
    [Header("Buttons")]
    public Button StartButton    = null;
    public Button SoundButton    = null;
    public Button ExitButton     = null;
    [Header("Button Sprites")]
    public Sprite SoundOnSprite  = null;
    public Sprite SoundOffSprite = null;

    bool _soundOn = false;

    void Start() {
        Cursor.visible = true;

        _soundOn = AudioListener.volume > 0.05f;
        SoundButton.image.sprite = _soundOn ? SoundOnSprite : SoundOffSprite;
        StartButton.onClick.AddListener(LoadLevel);
        SoundButton.onClick.AddListener(OnClickSoundToggle);
        ExitButton.onClick.AddListener(OnClickExit);
        if ( Application.platform == RuntimePlatform.WebGLPlayer ) {
            ExitButton.gameObject.SetActive(false);
        }

		SoundManager.Instance.PlayMusic("menu");
    }

    void LoadLevel() {
		SoundManager.Instance.PlaySound("menuClick");
        SceneManager.LoadScene("Gameplay");
    }

    public void OnClickSoundToggle() {
        _soundOn = !_soundOn;
        SoundButton.image.sprite = _soundOn ? SoundOnSprite : SoundOffSprite;
        AudioListener.volume = _soundOn ? 1f : 0f;
    }

    void OnClickExit() {
		SoundManager.Instance.PlaySound("menuClick");
		Application.Quit();
    }
}
