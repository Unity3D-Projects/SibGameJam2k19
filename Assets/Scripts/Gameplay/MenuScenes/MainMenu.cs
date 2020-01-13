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
	public GameObject       LevelChoiceCanvas      = null;
	public Button           LevelChoiceCloseButton = null;
	public Button           EndlessRunButton       = null;
	public List<GameObject> LevelButtons           = null;

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
			button.GetComponent<Button>().onClick.AddListener(delegate { StartLevel(button.GetComponent<LevelButton>().LevelName); });
		}
		LevelChoiceCloseButton.onClick.AddListener(OnClickClose);
		EndlessRunButton.onClick.AddListener(OnClickEndlessRun);

		SoundManager.Instance.PlayMusic("menu");
		LevelChoiceCanvas.gameObject.SetActive(false);
		UpdateLevelButtons();
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

	void UpdateLevelButtons() {
		string appleNum = PlayerPrefs.GetInt(LevelButtons[0].GetComponent<LevelButton>().LevelName).ToString();
		if ( appleNum == "" ) {
			appleNum = "0";
		}
		var txt = LevelButtons[0].transform.Find("Apples").GetComponent<Text>();
		txt.text = txt.text.Substring(txt.text.IndexOf('/')); 
		txt.text = txt.text.Insert(0, appleNum);
		if ( PlayerPrefs.HasKey(LevelButtons[0].GetComponent<LevelButton>().LevelName + ".Star") ) {
			LevelButtons[0].transform.Find("Star").gameObject.SetActive(true);
		} else {
			LevelButtons[0].transform.Find("Star").gameObject.SetActive(false);
		}
		for ( int i = 1; i < LevelButtons.Count; i++ ) {
			if ( PlayerPrefs.HasKey(LevelButtons[i-1].GetComponent<LevelButton>().LevelName) ) {
				LevelButtons[i].GetComponent<Button>().interactable = true;
				//LevelButtons[i].transform.Find("Apples").GetComponent<Text>().text.Insert(0, PlayerPrefs.GetInt(LevelButtons[i].GetComponent<LevelButton>().LevelName).ToString()); 
			} else {
				LevelButtons[i].GetComponent<Button>().interactable = false; 
			}
			appleNum = PlayerPrefs.GetInt(LevelButtons[i].GetComponent<LevelButton>().LevelName).ToString();
			if ( appleNum == "" ) {
				appleNum = "0";
			}
			txt = LevelButtons[i].transform.Find("Apples").GetComponent<Text>();
			txt.text = txt.text.Substring(txt.text.IndexOf('/'));
			txt.text = txt.text.Insert(0, appleNum);

			if ( PlayerPrefs.HasKey(LevelButtons[i].GetComponent<LevelButton>().LevelName + ".Star") ) {
				LevelButtons[i].transform.Find("Star").gameObject.SetActive(true);
			} else {
				LevelButtons[i].transform.Find("Star").gameObject.SetActive(false); 
			}
		}
	}

	public void OnClickClearPlayerData() {
		PlayerPrefs.DeleteAll();
		UpdateLevelButtons();
	}
	public void OnClickUnlockAll() {
		UnlockAll();
	}
	void UnlockAll() {
		for ( int i = 1; i < LevelButtons.Count; i++ ) {
			LevelButtons[i].GetComponent<Button>().interactable = true;
		}
	}

	void OnClickEndlessRun() {
		StartLevel("EndlessRun");
	}
}
