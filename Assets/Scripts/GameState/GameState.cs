using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameState : MonoSingleton<GameState> {

	protected override void Awake() {
		base.Awake();
		SoundManager.Instance.PlayMusic("level");
	}
}
