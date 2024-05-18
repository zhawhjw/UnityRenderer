using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AgentIKHandler : MonoBehaviour
{

	private Animator animator;
	private Vector3 rightFootPosition = new Vector3(0,0,0);
	private Vector3 leftFootPosition = new Vector3(0,0,0);
	private Vector3 oldRightFootPosition = new Vector3(0,0,0);
	private Vector3 oldLeftFootPosition = new Vector3(0,0,0);
	private Vector3 finalLeftFootPosition = new Vector3(0,0,0);
	private Vector3 finalRightFootPosition = new Vector3(0,0,0);
	
	public float stepDistance = 0.3f;
	public float footSpacing = 0.2f;
	public float ankleHeight = 0.08f;
	public float randomnessScale = 0.2f;
	public float ikActivationSpeed = 1.5f;
	public float ikDeactivationSpeed = 1.5f;
	public float ikTransitionStrictness = 0.1f;
	private float originalStrictness;
	public float ikTransitionStrictnessRelaxSpeed = 0.1f;
	public float stepHeight = 0.1f;
	public float stepLerpSpeed = 0.1f;
	public bool active = false;
	public Vector2 currentVelocity;

	public GameObject leftFootObject;
	public GameObject rightFootObject;

	private float ikForce = 0.0f;
	private float leftFootLerp = 1.0f;
	private float rightFootLerp = 1.0f;

	private int animIDPlaybackLocation = Animator.StringToHash("PlaybackLocation");

    void Start() {
		// grab references
        animator = GetComponent<Animator>();

		originalStrictness = ikTransitionStrictness;
    }

	void OnAnimatorIK() {
		if (!active) {
			// phase out IK
			ikForce -= ikDeactivationSpeed * 1.0f/60.0f;
			if (ikForce <= 0.0f) {
				return;
			}
		} else {
			// phase in IK
			ikForce += ikActivationSpeed * 1.0f/60.0f;
		}

		// clamp IK
		ikForce = Mathf.Clamp01(ikForce);

		// ensure neither foot is currently being moved
		// and that we are active (no retargeting during transitions)
		if (active && rightFootLerp == 1.0f && leftFootLerp == 1.0f) {
			// check to see if either foot needs to be moved
			float leftFootDistance = Vector3.Distance(leftFootPosition, transform.position);
			float rightFootDistance = Vector3.Distance(rightFootPosition, transform.position);
			bool shouldMoveLeft = leftFootDistance > stepDistance && Vector3.Dot(leftFootPosition - transform.position, new Vector3(currentVelocity.x, 0, currentVelocity.y)) < 0;
			bool shouldMoveRight = rightFootDistance > stepDistance && Vector3.Dot(rightFootPosition - transform.position, new Vector3(currentVelocity.x, 0, currentVelocity.y)) < 0;
			if (shouldMoveLeft && shouldMoveRight) {
				// if they both need to be moved, move the further one
				if (leftFootDistance > rightFootDistance) {
					MoveLeftLeg();
				} else {
					MoveRightLeg();
				}
			} else if (shouldMoveLeft) {
				// if the left foot needs to be moved and it's NOT in the direction we are moving, move it
				MoveLeftLeg();
			} else if (shouldMoveRight) {
				// see above
				MoveRightLeg();
			} else if (Vector3.Dot(leftFootPosition - transform.position, transform.right) > 0) {
				// if the left leg is on the wrong side of the body, move it
				MoveLeftLeg();
			} else if (Vector3.Dot(rightFootPosition - transform.position, transform.right) < 0) {
				// see above
				MoveRightLeg();
			}

		}

		// set IK params
		animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, ikForce);
		animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, ikForce);
		animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, ikForce);
		animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, ikForce);

		// step interpolation
		if (leftFootLerp != 1.0f) {
			leftFootLerp += stepLerpSpeed * 1.0f/60.0f * Mathf.Max(currentVelocity.magnitude * 3.0f, 1.0f);
			if (leftFootLerp >= 1.0f) {
				leftFootLerp = 1.0f;
				leftFootPosition += transform.position;
				oldLeftFootPosition += transform.position;
			}
		}

		if (rightFootLerp != 1.0f) {
			rightFootLerp += stepLerpSpeed * 1.0f/60.0f * Mathf.Max(currentVelocity.magnitude * 3.0f, 1.0f);
			if (rightFootLerp >= 1.0f) {
				rightFootLerp = 1.0f;
				rightFootPosition += transform.position;
				oldRightFootPosition += transform.position;
			}
		}

		// set IK positions
		if (leftFootLerp != 1.0f) {
			float x = leftFootLerp - (Mathf.Sin(2.0f*Mathf.PI*leftFootLerp) / (2.0f*Mathf.PI));
			float y = (stepHeight/2.0f) - ((stepHeight*Mathf.Cos(2.0f*Mathf.PI*leftFootLerp)) / 2.0f);
			animator.SetIKPosition(AvatarIKGoal.LeftFoot, transform.position + Vector3.Lerp(oldLeftFootPosition, leftFootPosition, x) + new Vector3(0, y + ankleHeight, 0));
		} else {
			animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPosition + new Vector3(0, ankleHeight, 0));
		}

		if (rightFootLerp != 1.0f) {
			float x = rightFootLerp - (Mathf.Sin(2.0f*Mathf.PI*rightFootLerp) / (2.0f*Mathf.PI));
			float y = (stepHeight/2.0f) - ((stepHeight*Mathf.Cos(2.0f*Mathf.PI*rightFootLerp)) / 2.0f);
			animator.SetIKPosition(AvatarIKGoal.RightFoot, transform.position + Vector3.Lerp(oldRightFootPosition, rightFootPosition, x) + new Vector3(0, y + ankleHeight, 0));
		} else {
			animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPosition + new Vector3(0, ankleHeight, 0));
		}

		// set IK rotations (feet should always be flat on the floor)
		animator.SetIKRotation(AvatarIKGoal.RightFoot, transform.rotation);
		animator.SetIKRotation(AvatarIKGoal.LeftFoot, transform.rotation);
	}

	private void MoveRightLeg() {
		// grab center of mass
		Vector3 centerOfMass = transform.position;

		// remember current position
		oldRightFootPosition = new Vector3(rightFootPosition.x, rightFootPosition.y, rightFootPosition.z);

		// compute where we think the other foot will be
		Vector3 expectedLeftFootPosition = leftFootPosition - new Vector3(currentVelocity.x, 0, currentVelocity.y) * 1.0f/stepLerpSpeed * Mathf.Max(currentVelocity.magnitude * 3.0f, 1.0f);
		
		// reflect across the agents's center of mass to approximate balance
		float dx = expectedLeftFootPosition.x - centerOfMass.x;
		float dz = expectedLeftFootPosition.z - centerOfMass.z;
		rightFootPosition = new Vector3(centerOfMass.x - dx, 0, centerOfMass.z - dz);
		
		// clamp to the leg's movement axis (no crossing of feet allowed)
		Vector3 toLeg = rightFootPosition - transform.position;
		Vector3 projection = Vector3.Project(toLeg, transform.forward);
		rightFootPosition = transform.position + projection + transform.right * footSpacing;

		// add some random noise to mix up the stepping a bit
		rightFootPosition += new Vector3(UnityEngine.Random.Range(-stepDistance*randomnessScale, stepDistance*randomnessScale), 0, UnityEngine.Random.Range(-stepDistance*randomnessScale, stepDistance*randomnessScale));

		// make the position relative
		rightFootPosition -= transform.position;
		oldRightFootPosition -= transform.position;

		// damp
		rightFootPosition *= 0.8f;

		if (rightFootPosition.magnitude > stepDistance) {
			rightFootPosition = rightFootPosition.normalized * stepDistance;
		}
		
		// reset interpolation
		rightFootLerp = 0.0f;
	}

	private void MoveLeftLeg() {
		// grab center of mass
		Vector3 centerOfMass = transform.position;

		// remember current position
		oldLeftFootPosition = leftFootPosition;

		// compute where we think the other foot will be
		Vector3 expectedRightFootPosition = rightFootPosition - new Vector3(currentVelocity.x, 0, currentVelocity.y) * 1.0f/stepLerpSpeed * Mathf.Max(currentVelocity.magnitude * 3.0f, 1.0f);

		// reflect across the agents's center of mass to approximate balance
		float dx = expectedRightFootPosition.x - centerOfMass.x;
		float dz = expectedRightFootPosition.z - centerOfMass.z;
		leftFootPosition = new Vector3(centerOfMass.x - dx, 0, centerOfMass.z - dz);

		// clamp to the leg's movement axis (no crossing of feet allowed)
		Vector3 toLeg = leftFootPosition - transform.position;
		Vector3 projection = Vector3.Project(toLeg, transform.forward);
		leftFootPosition = transform.position + projection - transform.right * footSpacing;
		
		// add some random noise to mix up the stepping a bit
		leftFootPosition += new Vector3(UnityEngine.Random.Range(-stepDistance*randomnessScale, stepDistance*randomnessScale), 0, UnityEngine.Random.Range(-stepDistance*randomnessScale, stepDistance*randomnessScale));

		// make the position relative
		leftFootPosition -= transform.position;
		oldLeftFootPosition -= transform.position;

		// damp
		leftFootPosition *= 0.8f;

		if (leftFootPosition.magnitude > stepDistance) {
			leftFootPosition = leftFootPosition.normalized * stepDistance;
		}
		
		// reset interpolation
		leftFootLerp = 0.0f;
	}

	public void activate() {
		if (!active) {
			// put the legs where they currently are in the animation
			leftFootPosition = leftFootObject.transform.position;
			leftFootPosition.y = 0.0f;
			leftFootLerp = 1.0f;
			finalLeftFootPosition = leftFootPosition;
			rightFootPosition = rightFootObject.transform.position;
			rightFootPosition.y = 0.0f;
			rightFootLerp = 1.0f;
			finalRightFootPosition = rightFootPosition;

			active = true;
		}
	}

	public void deactivate() {
		if (Mathf.Abs(animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0f - 0.4f) < ikTransitionStrictness
			&& rightFootLerp <= 0.1f
			&& leftFootLerp == 1.0f
			&& Vector3.Dot(leftFootPosition - transform.position, transform.forward) > 0
			&& Vector3.Dot(rightFootPosition - transform.position, transform.forward) < 0) {
			active = false;
			ikTransitionStrictness = originalStrictness;
		} else if (Mathf.Abs(animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0f - 0.8f) < ikTransitionStrictness
			&& rightFootLerp == 1.0f
			&& leftFootLerp <= 0.1f
			&& Vector3.Dot(leftFootPosition - transform.position, transform.forward) < 0
			&& Vector3.Dot(rightFootPosition - transform.position, transform.forward) > 0) {
			active = false;
			ikTransitionStrictness = originalStrictness;
		} else {
			ikTransitionStrictness += ikTransitionStrictnessRelaxSpeed * 1.0f/60.0f;
			if(ikTransitionStrictness > 6.0f * originalStrictness) {
				active = false;
				ikTransitionStrictness = originalStrictness;
			}
		}
	}
}