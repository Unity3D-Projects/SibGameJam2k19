namespace KOZA.Events {

	/// <summary>
	/// Ads
	/// </summary>
	///
	public struct OnAdReady {
		public string PlacementId;
		public OnAdReady(string placement) {
			PlacementId = placement;
		}
	}
	public struct Event_JumpButtonPushed {
	}

	public struct Event_SlideButtonPushed {
	}

	public struct Event_SlideButtonReleased {
	}

	public struct Event_YellButtonPushed {
	}

	public struct Event_GoatYell {
	}

	public struct Event_PhysicsObjectGrounded {
		public PhysicsObject Object;

		public Event_PhysicsObjectGrounded(PhysicsObject obj) {
			Object = obj;
		}
	}

	public struct Event_Obstacle_Collided {
		public Obstacle Obstacle;
	}

	public struct Event_FarmerActionTrigger {
		public FarmerActionTrigger Trigger;
	}

	public struct Event_GoatDies {
	}

	public struct Event_AppleCollected {
	}

	public struct Event_TryActivateBoost {
		public BoostType Type;
	}

	public struct Event_BoostActivated {
		public BoostType Type;
	}

	public struct Event_BoostEnded {
		public BoostType Type;
	}

	public struct Event_ScoreChanged {
		public int NewScore;
	}

	public struct Event_GameWin {

	}

	public struct Event_StartDialogComplete {}

	public struct Event_HelpScreenClosed { }
	public struct Event_JumpMaxHeightReached { }

	public struct Event_SceneLoaded {
		public string    SceneName;
		public SceneType SceneType;
	}
}
