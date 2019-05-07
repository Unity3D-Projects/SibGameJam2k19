using System.Collections.Generic;
using EventSys;
using UnityEngine;

public class PhysicsObject : MonoBehaviour {

	public float MinGroundNormalY = 0.65f;
	public float GravityModifier  = 1f;

	[HideInInspector]
	public float HorizontalSpeedMultiplier = 1f;

	public Collider2D NormalCollider     = null;
	public Collider2D LowProfileCollider = null;

	protected Vector2 targetVelocity;

	public bool Grounded {
		get => _grounded;

		protected set {
			if ( value && !_grounded ) {
				EventManager.Fire(new Event_PhysicsObjectGrounded(this));
			}
			_grounded = value;

		}
	}

	protected bool _grounded = false;

	protected Vector2 groundNormal;
	protected Rigidbody2D rb2d;
	protected Vector2 velocity;
	protected ContactFilter2D contactFilter;
	protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
	protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);


	protected const float minMoveDistance = 0.001f;
	protected const float shellRadius = 0.01f;

	void OnEnable() {
		rb2d = GetComponent<Rigidbody2D>();
	}

	void Start() {
		SetLowProfile(false);
		contactFilter.useTriggers = false;
		contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
		contactFilter.useLayerMask = true;
	}

	void Update() {
		ComputeVelocity();
	}

	protected virtual void ComputeVelocity() {

	}

	void FixedUpdate() {
		if ( GameState.Instance.TimeController.IsPause ) {
			return;
		}
		velocity += GravityModifier * Physics2D.gravity * Time.deltaTime;
		velocity.x = targetVelocity.x * HorizontalSpeedMultiplier;

		Vector2 deltaPosition = velocity * Time.deltaTime;

		Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

		Vector2 move = moveAlongGround * deltaPosition.x;

		Movement(move, false, false);

		move = Vector2.up * deltaPosition.y;

		Movement(move, true, true);
	}

	void Movement(Vector2 move, bool yMovement, bool updateGrounded) {
		var distance = move.magnitude;
		var tmpGrounded = false;
		if ( distance > minMoveDistance ) {
			var count = rb2d.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
			hitBufferList.Clear();
			for ( var i = 0; i < count; i++ ) {
				hitBufferList.Add(hitBuffer[i]);
			}

			foreach (var hit in hitBufferList) {
				var currentNormal = hit.normal;
				if ( currentNormal.y > MinGroundNormalY ) {
					tmpGrounded = true;
					if ( yMovement ) {
						groundNormal = currentNormal;
						currentNormal.x = 0;
					}
				}
				var projection = Vector2.Dot(velocity, currentNormal);
				if ( projection < 0 ) {
					velocity = velocity - projection * currentNormal;
				}
				var modifiedDistance = hit.distance - shellRadius;
				distance = modifiedDistance < distance ? modifiedDistance : distance;
			}
			if ( updateGrounded ) {
				Grounded = tmpGrounded;
			}
		}

		
		rb2d.position = rb2d.position + move.normalized * distance;
	}

	public void Jump(float startSpeed) {
		if ( !Grounded ) {
			return;
		}
		velocity.y += startSpeed;
	}
    public void JumpFall(float fallStartSpeed) {
        velocity.y -= fallStartSpeed;
    }

	public void SetMoveSpeed(float speed) {
		targetVelocity.x = speed;
	}

	public void SetLowProfile(bool yesNo) {
		NormalCollider.enabled = !yesNo;
		LowProfileCollider.enabled = yesNo;
	}

	public void DisableAllColliders() {
		var cols = gameObject.GetComponentsInChildren<Collider2D>();
		foreach ( var col in cols ) {
			col.enabled = false;
		}
	}

}
