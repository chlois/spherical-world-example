using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityStandardAssets.Characters.ThirdPerson {
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SphereGravityObject))]
	public class SphereCharacter : MonoBehaviour {
		[SerializeField] float m_MovingTurnSpeed = 360;
		[SerializeField] float m_StationaryTurnSpeed = 180;
		[SerializeField] float m_JumpPower = 8.0f;
		[Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
		[SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
		[SerializeField] float m_MoveSpeedMultiplier = 1f;
		[SerializeField] float m_AnimSpeedMultiplier = 1f;
		[SerializeField] float m_GroundCheckDistance = 0.3f;

		public SphereGravityObject m_SphereObject;
		Rigidbody m_Rigidbody;
		Animator m_Animator;
		bool m_IsGrounded;
		float m_OrigGroundCheckDistance;
		const float k_Half = 0.5f;
		float m_TurnAmount;
		float m_ForwardAmount;
		Vector3 m_GroundNormal;
		float m_CapsuleHeight;
		Vector3 m_CapsuleCenter;
		CapsuleCollider m_Capsule;
		bool m_Crouching;

		// Ragdoll switch related
		public bool enableRagdoll = false;
		List<Rigidbody> RagdollRigidbodys = new List<Rigidbody>();
		List<Collider> RagdollColliders = new List<Collider>();
		bool lastRagdollStatus = false;

		// Path finding related
		public List<Vector3> path = null;
		private Pathfinder pathfinder;
		private int pathIndex = 1;

		void Start() {
			m_Animator = GetComponent<Animator>();
			m_Rigidbody = GetComponent<Rigidbody>();
            m_SphereObject = GetComponent<SphereGravityObject>();
			m_Capsule = GetComponent<CapsuleCollider>();
			m_CapsuleHeight = m_Capsule.height;
			m_CapsuleCenter = m_Capsule.center;

			m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			m_OrigGroundCheckDistance = m_GroundCheckDistance;
            m_Rigidbody.useGravity = false;

			pathfinder = GetComponent<Pathfinder>();

			InitRagdoll();
		}

		void Update() {
			if (enableRagdoll) {
				m_SphereObject.attractor.AttractRagDoll(this.gameObject);
			}
			if (enableRagdoll != lastRagdollStatus) {
				lastRagdollStatus = enableRagdoll;
				if (enableRagdoll) {
					EnableRagdoll();
				}
				else {
					DisableRagdoll();
				}
			}

			// move along path
			if (path != null)
				MoveAlongPath();
			else {
				RandomMove();
			}
		}

		void InitRagdoll() {
			Rigidbody[] Rigidbodys = GetComponentsInChildren<Rigidbody>();
			for (int i = 0; i < Rigidbodys.Length; i++) {
				Rigidbodys[i].useGravity = false;
				if (Rigidbodys[i] == GetComponent<Rigidbody>()) {
					continue;
				}
				
				RagdollRigidbodys.Add(Rigidbodys[i]);
				Rigidbodys[i].isKinematic = true;
				Collider RagdollCollider = Rigidbodys[i].gameObject.GetComponent<Collider>();
				RagdollCollider.isTrigger = true;
				RagdollColliders.Add(RagdollCollider);
			}
		}

		void EnableRagdoll() {
			for (int i = 0; i < RagdollRigidbodys.Count; i++) {
				RagdollRigidbodys[i].isKinematic = false;
				RagdollColliders[i].isTrigger = false;
			}
			StartCoroutine(SetAnimatorEnable(false));
		}
		
		void DisableRagdoll() {
			for (int i = 0; i < RagdollRigidbodys.Count; i++) {
				RagdollRigidbodys[i].isKinematic = true;
				RagdollColliders[i].isTrigger = true;
			}
			GetComponent<Collider>().enabled = true;
			StartCoroutine(SetAnimatorEnable(true));
		}
		
		IEnumerator SetAnimatorEnable(bool Enable) {
			yield return new WaitForEndOfFrame();
			m_Animator.enabled = Enable;
		}

		public void Move(Vector3 move, bool crouch, bool jump) {
			move.Normalize();
			CheckGroundStatus();
			m_TurnAmount = Mathf.Atan2(move.x, move.z);
			m_ForwardAmount = move.z;

			var currentPos = transform.position;
			
			ApplyExtraTurnRotation();

			// control and velocity handling is different when grounded and airborne:
			if (m_IsGrounded) {
				HandleGroundedMovement(crouch, jump);
			}
			else {
				HandleAirborneMovement();
			}

			ScaleCapsuleForCrouching(crouch);
			PreventStandingInLowHeadroom();

			// send input and other state parameters to the animator
			UpdateAnimator(move);
		}

		public void SetTarget(Vector3 target) {
			path = pathfinder.FindPath(target);
		}

		public void ClearTarget() {
			path = null;
		}

		void MoveAlongPath() {
			if (path == null || path.Count == 0) 
				return;
			for (int i = pathIndex; i < path.Count; i++) {
				if (Vector3.Dot(path[i-1]-path[i],transform.position-path[i])<=0.01f) {
					pathIndex = i+1;
					break;
				}
			}
			if (pathIndex >= path.Count) {
				path = null;
				pathIndex = 1;
				return;
			}
			Vector3 gravityUp = m_SphereObject.GetGravityUp();
			Vector3 m_Forward = Vector3.ProjectOnPlane(transform.forward, gravityUp).normalized;
			Vector3 m_Right = Vector3.ProjectOnPlane(transform.right, gravityUp).normalized;
			Vector3 moveDir = path[pathIndex] - transform.position;
			float localV = Vector3.Dot(moveDir, m_Forward);
			float localH = Vector3.Dot(moveDir, m_Right);
			Move(new Vector3(localH, 0, localV), false, false);
		}

		void RandomMove() {
			var moveTarget = new Vector3(Random.Range(-100,100)/100f, Random.Range(-100,100)/100f, Random.Range(-100,100)/100f);
			SetTarget(moveTarget.normalized * 5f);
		}

		void ScaleCapsuleForCrouching(bool crouch) {
			if (m_IsGrounded && crouch) {
				if (m_Crouching) return;
				m_Capsule.height = m_Capsule.height / 2f;
				m_Capsule.center = m_Capsule.center / 2f;
				m_Crouching = true;
			}
			else {
				Ray crouchRay = new Ray(m_Rigidbody.position + transform.up * m_Capsule.radius * k_Half, transform.up);
				float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
				if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
				{
					m_Crouching = true;
					return;
				}
				m_Capsule.height = m_CapsuleHeight;
				m_Capsule.center = m_CapsuleCenter;
				m_Crouching = false;
			}
		}

		void PreventStandingInLowHeadroom() {
			// prevent standing up in crouch-only zones
			if (!m_Crouching) {
				Ray crouchRay = new Ray(m_Rigidbody.position + transform.up * m_Capsule.radius * k_Half, transform.up);
				float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
				if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
					m_Crouching = true;
				}
			}
		}

		void UpdateAnimator(Vector3 move) {
			// update the animator parameters
			m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
			m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
			m_Animator.SetBool("Crouch", m_Crouching);
			m_Animator.SetBool("OnGround", m_IsGrounded);
			if (!m_IsGrounded) {
                float jumpVerticalVelocity = Vector3.Dot(m_Rigidbody.velocity, m_SphereObject.GetGravityUp());
				m_Animator.SetFloat("Jump", jumpVerticalVelocity);
			}

			// calculate which leg is behind, so as to leave that leg trailing in the jump animation
			// (This code is reliant on the specific run cycle offset in our animations,
			// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
			float runCycle =
				Mathf.Repeat(
					m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
			float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
			if (m_IsGrounded) {
				m_Animator.SetFloat("JumpLeg", jumpLeg);
			}

			// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
			// which affects the movement speed because of the root motion.
			if (m_IsGrounded && move.magnitude > 0) {
				m_Animator.speed = m_AnimSpeedMultiplier;
			}
			else {
				// don't use that while airborne
				m_Animator.speed = 1;
			}
		}

		void HandleAirborneMovement() {
			// apply extra gravity from multiplier:
			var gravityForce = m_SphereObject.GetGravityForce();
			Vector3 extraGravityForce = gravityForce * (m_GravityMultiplier - 1);
			m_Rigidbody.AddForce(extraGravityForce, ForceMode.Acceleration);
			float velocityUp = Vector3.Dot(m_Rigidbody.velocity, m_SphereObject.GetGravityUp());
			m_GroundCheckDistance = velocityUp < 0 ? m_OrigGroundCheckDistance : 0.01f;
		}

		void HandleGroundedMovement(bool crouch, bool jump) {
			// check whether conditions are right to allow a jump:
			if (jump && !crouch && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded")) {
				// jump!
				Vector3 gravityUp = m_SphereObject.GetGravityUp();
				m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x+Vector3.Dot(gravityUp,new Vector3(1,0,0))*m_JumpPower, m_Rigidbody.velocity.y+Vector3.Dot(gravityUp,new Vector3(0,1,0))*m_JumpPower, m_Rigidbody.velocity.z+Vector3.Dot(gravityUp,new Vector3(0,0,1))*m_JumpPower);
				m_IsGrounded = false;
				m_Animator.applyRootMotion = false;
				m_GroundCheckDistance = 0.1f;
			}
		}

		void ApplyExtraTurnRotation() {
			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
			transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
		}

		public void OnAnimatorMove() {
			// we implement this function to override the default root motion.
			// this allows us to modify the positional speed before it's applied.
			if (m_IsGrounded && Time.deltaTime > 0) {
				Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;
				m_Rigidbody.velocity = v;
			}
		}

		void CheckGroundStatus() {
			RaycastHit hitInfo;
			Vector3 gravityUp = m_SphereObject.GetGravityUp();
			//Vector3 bottomPosition = transform.position - transform.up * (m_CapsuleHeight * transform.localScale.y / 2.0f) + m_CapsuleCenter;
#if UNITY_EDITOR
			// helper to visualise the ground check ray in the scene view
			Debug.DrawLine(transform.position + (gravityUp * 0.1f), transform.position + (gravityUp * 0.1f) + (-gravityUp * m_GroundCheckDistance));
#endif
			// 0.1f is a small offset to start the ray from inside the character
			// it is also good to note that the transform position in the sample assets is at the base of the character			
			if (Physics.Raycast(transform.position + (gravityUp * 0.1f), -gravityUp, out hitInfo, m_GroundCheckDistance)) {
				m_GroundNormal = hitInfo.normal;
				m_IsGrounded = true;
				m_Animator.applyRootMotion = true;
			}
			else {
				m_IsGrounded = false;
				m_GroundNormal = gravityUp;
				m_Animator.applyRootMotion = false;
			}
		}
	}
}
