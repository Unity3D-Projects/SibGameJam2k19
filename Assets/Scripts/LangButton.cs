using UnityEngine;
using UnityEngine.UI;

using EventSys;

public sealed class LangButton : MonoBehaviour {
	public Button Button = null;

	public Sprite EnglishSprite = null;
	public Sprite RussianSprite = null;

	void Start() {
		var lc = LocalizationController.Instance;
		var curLang = lc.CurrentLanguage;

		EventManager.Subscribe<Event_LanguageChanged>(this, OnLanguageChanged);
		SetupImage(curLang);
		Button.onClick.RemoveAllListeners();
		Button.onClick.AddListener(SwitchLanguage);
	}

	void OnDestroy() {
		EventManager.Unsubscribe<Event_LanguageChanged>(OnLanguageChanged);
	}

	void SwitchLanguage() {
		SoundManager.Instance.PlaySound("menuClick");
		var lc = LocalizationController.Instance;
		var newLanguage = lc.CurrentLanguage == SystemLanguage.English ? SystemLanguage.Russian : SystemLanguage.English;
		lc.ChangeLanguage(newLanguage);
	}

	void SetupImage(SystemLanguage lang) {
		Button.image.sprite = lang == SystemLanguage.Russian ? RussianSprite : EnglishSprite;
	}

	void OnLanguageChanged(Event_LanguageChanged e) {
		SetupImage(e.Language);
	}
}
