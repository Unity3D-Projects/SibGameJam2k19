
namespace EventSys {
	public struct Event_JumpButtonPushed {
	}

	public struct Event_SlideButtonPushed {
	}

	public struct Event_SlideButtonReleased {
	}

	public struct Event_YellButtonPushed {
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
	
}
