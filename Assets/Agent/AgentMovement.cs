using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AgentMovement : MonoBehaviour
{
	public GameObject CapsuleModel;
	public GameObject RobotModel;
	public GameObject DirectionIndicator;
	public bool isCapsule = true;
	public bool useShadow = true;
	public bool useArrow = true;
	public Color color;
	public float turnSpeed = 1.0f;
	private float walkSpeed = 0.0f;
	private float targetRotation = 0.0f;
	public float _animationBlend = 0.0f;
	private int _animIDSpeed;
	private int _animIDMotionSpeed;
	private Animator _animator;
	private float[,] positionBuffer = new float[30,3];
	private int positionBufferLoc = -1;
	public float IKTreshold = 0.7f;
	public float IKDistance = 10.0f;
	public float deadzone = 0.4f;
	private bool inIKRange = false;
	private AgentIKHandler IKHandler;

	public void Start() {
		// grab references
		_animIDSpeed = Animator.StringToHash("Speed");
		_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
		_animator = RobotModel.GetComponent<Animator>();
		
		walkSpeed = Random.Range(0.99f, 1.01f);
		IKHandler = RobotModel.GetComponent<AgentIKHandler>();
		
		UpdateModel();
	}

	public void MoveTo(Vector2 goal, float frameIndex, int frameCount, float delta, float normalize, float orientation, float shift_angle,  bool orentation_flag = false) {
		if (float.IsNaN(_animationBlend)) { // why is ONE AGENT having an invalid animation blend
			_animationBlend = 0.0f;
		}

		// increment location in position buffer
		positionBufferLoc = (positionBufferLoc + 1) % positionBuffer.GetLength(0);

		// save current position
		positionBuffer[positionBufferLoc, 0] = goal.x;
		positionBuffer[positionBufferLoc, 1] = goal.y;
		positionBuffer[positionBufferLoc, 2] = Time.frameCount;



		// star to set animation speed and phase

		// legacy code; The computation is confusing and better to left it
		// compute how fast we are going
		Vector2 vel = InferVelocityVector();

		// legacy code; these three lines not used in simulation
		_animationBlend = Mathf.Lerp(_animationBlend, vel.magnitude, Time.deltaTime);
		float factor = walkSpeed * 1.0f / (60.0f * Time.deltaTime);
		_animator.SetFloat(_animIDMotionSpeed, walkSpeed * factor);

		// if we want to use raw orientation data
		// used for bottleneck rendering
		if (orentation_flag)
        {
			// used for bottleneck 

			// this block aims to solve the abnormal jittering of facing direction
			// when agents are colliding, the velocity direction is changed rapidly

			// notice the default angle setting in threejs is different
			// so we need a shift, this shift depends on the how much we shifted on 
			// the start of threejs simulation: we have 90 degree in threejs
			float shift_radiant = shift_angle * Mathf.Deg2Rad;
			targetRotation = (orientation - shift_radiant) * 180 / Mathf.PI;
			// limit the maximum angle it could rotate
			float threshold = 90f;
			if (Mathf.Abs(targetRotation - transform.eulerAngles.y) < threshold)
			{
				transform.eulerAngles = new Vector3(0, targetRotation, 0);

			}

		}
        else
        {
			// yes, this is an atan(x,y). The fact that Unity coordinates have the agent facing the negative y direction make this correct.
			targetRotation = Mathf.Atan2(goal.x - transform.position.x, goal.y - transform.position.z) * 180.0f / Mathf.PI;
			transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(transform.eulerAngles.y, targetRotation, turnSpeed * 1.0f / 60.0f * Mathf.Pow(normalize, 2.0f)), 0);
		}

		


		// '_animator' is blend tree and '_animIDSpeed' is the value used for blending animation
		_animator.SetFloat(_animIDSpeed, normalize);
		
		// legacy code; IK related from first developer, not work well.
		// activate IK if we are moving slowly
		// the deadzone prevents the IK from jittering on and off
		if (vel.magnitude < IKTreshold * (1.0f - deadzone) && inIKRange)
        {
            IKHandler.activate();
            IKHandler.currentVelocity = vel;
            _animationBlend -= 1.0f / 60.0f * 3.0f;
        }
        else if (vel.magnitude > IKTreshold * (1.0f + deadzone))
        {
            IKHandler.deactivate();
        }



        transform.position = new Vector3(goal.x, 0, goal.y);
    }

	private Vector2 InferVelocityVector() {
		// computes the average direction we've been going in based on the frames in positionBuffer
		int end = positionBufferLoc;
		int start = (positionBufferLoc + 1) % positionBuffer.GetLength(0);
		float deltax = positionBuffer[end, 0] - positionBuffer[start, 0];
		float deltay = positionBuffer[end, 1] - positionBuffer[start, 1];
		float deltat = positionBuffer[end, 2] - positionBuffer[start, 2];
		return new Vector2(deltax / deltat, deltay / deltat) * 60.0f;
	}

	public void SetShadow(bool v) {
		useShadow = v;
		UpdateModel();
	}

	public void SetArrow(bool v) {
		useArrow = v;
		UpdateModel();
	}

	public void SetCapsule(bool v) {
		isCapsule = v;
		UpdateModel();
	}

	public void SetColor(Color c) {
		color = c;
		CapsuleModel.GetComponent<Renderer>().material.SetColor("_Color", c);
		// RotateCapsuleModel.GetComponent<Renderer>().material.SetColor("_Color", c);
		Renderer[] renderers = RobotModel.GetComponentsInChildren<Renderer>();
		foreach (Renderer r in renderers)
        {
			foreach (Material m in r.materials)
			{
				m.SetColor("_Color", c);
			}
		}

			
		/*foreach (Material m in RobotModel.GetComponentInChildren<Renderer>().materials) {
			m.SetColor("_Color", c);
		}*/
	}

	private void UpdateModel() {
		if (isCapsule) {
			CapsuleModel.GetComponent<Renderer>().enabled = true;
			RobotModel.GetComponentInChildren<Renderer>().enabled = false;
			RobotModel.GetComponent<Animator>().enabled = false;
		} else {
			CapsuleModel.GetComponent<Renderer>().enabled = false; 
			RobotModel.GetComponentInChildren<Renderer>().enabled = true;
			RobotModel.GetComponent<Animator>().enabled = true;
		}

	
		if (useShadow) {
			CapsuleModel.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
			RobotModel.GetComponentInChildren<Renderer>().shadowCastingMode = ShadowCastingMode.On;
		} else {
			CapsuleModel.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
			RobotModel.GetComponentInChildren<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
		}

		if (useArrow) {
			DirectionIndicator.GetComponent<Renderer>().enabled = true;
		} else {
			DirectionIndicator.GetComponent<Renderer>().enabled = false;
		}
	}

	public void CheckIKDistance(Vector3 cameraPosition) {
		if (Vector3.Distance(cameraPosition, transform.position) < IKDistance) {
			inIKRange = true;
		} else {
			inIKRange = false;
		}
	}
}