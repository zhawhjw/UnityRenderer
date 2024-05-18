using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using Evereal.VideoCapture;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;


public class AgentManager : MonoBehaviour
{
	public float worldScale = 1.0f;
	public string sourcePath;
	public string sourceFile;
	public string sourceWallFile;
	public string sourceOrientationFile;

	public string extension;

	public GameObject agentPrefab;
	public GameObject obstaclePrefab;
	public GameObject obstacleWallPrefab;

	public GameObject cameraObject; // the camera position will be passed to agents so they can activate/deactivate IK
	public GameObject FocusLight;
	public float A = 0f;
	public float B = 4f;
	public float T = 3f;

	public float delta = 0f;
	public float frameIndex = 0;
	public float frameStep = 1;
	public int frameCount = 0;
	public int groupToFollow = -1;
	public int lightGroupToFollow = -1;
	public int agentToColorOverride = -1;
	public Color agentColorOverride;
	
	public bool useCapsules = true;
	public bool useShadows = true;
	public bool useArrows = true;
	public bool colorCode = true;
	public List<Color> colorOverrides = new List<Color>();
	private List<AgentMovement> agents = new List<AgentMovement>();
	private float[,] frames;
	float[,] difference;
	float[,] normalized_diff;
	float[,] orientation_frames;

	private int agentCount = 0;

	List<List<Vector3>> agentpositions;
	public bool renderTrajectory = false;
	public float trajectoryThinckness = 0.15f;
	public int trajectoryLength = 800;
	public bool fullTrajectory = true;

	public bool gradientFlag = false;

	public bool BILAS = true;
	public bool orentationFlag = false;
	public bool sideSteppingFlag = true;
	public float shiftDegree = 90f;

	List<Vector3> wallData;
	List<Vector3> wallSizeData;

	// legacy variables
	// used for smooth trajectory in Unity
	// recommand to left it
	public int passes = 60;
	public float strength = 0.2f;
	public int span = 1;

	void Awake () {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
	}


	public float[,] CalculateDifferences(float[,] frames)
	{
		int numRows = frames.GetLength(0);
		int numCols = frames.GetLength(1);
		int numAgents = numCols / 2; // Since every agent has two coordinates

		// Create a new array to store the differences
		float[,] differences = new float[numRows, numAgents];

		// Loop through each frame
		for (int i = 0; i < numRows; i++)
		{
			// Loop through each agent
			for (int j = 0; j < numAgents; j++)
			{
				// Compute differences
				if (i == 0)
				{
					// First frame, set differences to 0
					differences[i, j] = 0f;
				}
				else
				{
					// Calculate the distance from the current frame to the previous frame
					float dx = frames[i, 2 * j] - frames[i - 1, 2 * j];
					float dy = frames[i, 2 * j + 1] - frames[i - 1, 2 * j + 1];
					differences[i, j] = Mathf.Sqrt(dx * dx + dy * dy); // Euclidean distance
				}
			}
		}

		return differences;
	}

	// Method to normalize the array elements between 'a' and 'b'
	public float[,] NormalizeArray(float[,] array, float a, float b)
	{
		int numRows = array.GetLength(0);
		int numCols = array.GetLength(1);

		// Find the minimum and maximum values in the array
		float min = float.MaxValue;
		float max = float.MinValue;

		for (int i = 0; i < numRows; i++)
		{
			for (int j = 0; j < numCols; j++)
			{
				if (array[i, j] < min)
				{
					min = array[i, j];
				}
				if (array[i, j] > max)
				{
					max = array[i, j];
				}
			}
		}

		// Create a new array to store the normalized values
		float[,] normalizedArray = new float[numRows, numCols];

		// Calculate the normalized values
		for (int i = 0; i < numRows; i++)
		{
			for (int j = 0; j < numCols; j++)
			{
				normalizedArray[i, j] = a + ((array[i, j] - min) * (b - a) / (max - min));
			}
		}

		// set the first value
		for (int j = 0; j < numCols; j++)
		{
			normalizedArray[0, j] = b;
		}
		 

		return normalizedArray;
	}

	// Method to set a threshold that filters out any value in 'array' that less than 's'
	public float[,] ApplyThreshold(float[,] array, float s)
	{
		int numRows = array.GetLength(0);
		int numCols = array.GetLength(1);

		// Create a new array to store the adjusted values
		float[,] adjustedArray = new float[numRows, numCols];

		// Adjust the values based on the threshold
		for (int i = 0; i < numRows; i++)
		{
			for (int j = 0; j < numCols; j++)
			{
				// If the current value is less than the threshold, set it to 's'

				// if there is really small value, make it to 0 
				// so animation will not look bad
				// it should be disabled in most of the time
				/*if (array[i, j] <= 0.0000001)
                {
					array[i, j] = 0.0f;

				}
				else */
				if ( array[i, j] < s)
				{
					adjustedArray[i, j] = s;
				}
				else
				{
					// Otherwise, keep the original value
					adjustedArray[i, j] = array[i, j];
				}
			}
		}

		return adjustedArray;
	}

	// Start is called before the first frame update
	void Start() {
		
		wallData = new List<Vector3>();
		wallSizeData = new List<Vector3>();

		agentpositions = new List<List<Vector3>>();
		
		
		// read the raw trajectory data, wall position data, and store them
		// wall data is not necessary for every scenario.
		string[] rawWallFrames;
		string[] rawFrames;
		if (string.IsNullOrEmpty(sourceWallFile))
        {
			rawWallFrames = new string[] { };
        }
        else
        {
			rawWallFrames = System.IO.File.ReadAllLines(sourcePath + @"/" + sourceWallFile + @"." + extension);
		}
		rawFrames = System.IO.File.ReadAllLines(sourcePath + @"/" + sourceFile + @"." + extension);
		// read the raw orientation data
		string[] rawOrientations;

		if (orentationFlag)
        {
			rawOrientations = System.IO.File.ReadAllLines(sourcePath + @"/" + sourceOrientationFile + @"." + extension);
        }
        else
        {
			rawOrientations = new string[] { };

		}

		// grab the group counts from the first line of the raw trajectory data
		string[] rawGroupCounts = rawFrames[0].Split(",");

		// parse the group counts
		int[] groupCounts = new int[rawGroupCounts.Length];
		for (int i = 0; i < groupCounts.Length; i++) {
			groupCounts[i] = Int32.Parse(rawGroupCounts[i], CultureInfo.InvariantCulture.NumberFormat);
		}


		// process the wall position data, do the process logic depends on the input information from txt file
		// format 1: x,z 
		// format 2: x,z,widthX,widthZ
		// if is format 1, the widthX and widthZ will be 1 in default
		// the valid input size should either be 2 or 4
		for (int i =0;i< rawWallFrames.Length; i++){
			string[] line = rawWallFrames[i].Split(",");
			string blockX = line[0].Replace(" ", ""); 
			string blockZ = line[1].Replace(" ", ""); 
			Vector3 blockPosition = new Vector3(float.Parse(blockX), 0f, float.Parse(blockZ));

			float widthX = 1f; 
			float widthZ = 1f; 

			if(line.Length == 4)
            {
				widthX = float.Parse(line[2].Replace(" ", ""));
				widthZ = float.Parse(line[3].Replace(" ", ""));

			}

			Vector3 blockSize = new Vector3(widthX, 10f, widthZ);

			wallData.Add(blockPosition);
			wallSizeData.Add(blockSize);
		}
		// instantiate wall object in Unity Scene
		for (int i =0;i< wallData.Count; i++){
			
			GameObject wall = Instantiate(obstacleWallPrefab, new Vector3(wallData[i].x, 0, wallData[i].z), Quaternion.identity);
			wall.transform.localScale = wallSizeData[i];
			

		}

	

		// select randomly chosen group colors
		Color[] groupColors = new Color[groupCounts.Length];
		float hueStep = 1.0f/groupCounts.Length;
		for (int i = 0; i < groupCounts.Length; i++) {
			groupColors[i] = Color.HSVToRGB(hueStep*i, 1.0f, 1.0f);
		}

		// apply color overrides
		for (int i = 0; i < colorOverrides.Count; i++) {
			groupColors[i] = colorOverrides[i];
		}

		// initialize size of frames array
		frames = new float[rawFrames.Length-2, rawFrames[2].Split(',').Length];
		float[,] originalFrames = new float[rawFrames.Length-2, rawFrames[2].Split(',').Length];
		orientation_frames = new float[rawFrames.Length -1 , rawFrames[2].Split(',').Length / 2];


		// parse frames from strings
		for (int i = 0; i < frames.GetLength(0); i++) {

			string[] rawCoords = rawFrames[i+2].Split(","); // rawFrames[i+1] is using i+1 because the first line of the file is irrelevant
			
			for (int j = 0; j < rawCoords.Length; j++) {
				try {
					frames[i, j] = float.Parse(rawCoords[j], CultureInfo.InvariantCulture.NumberFormat) * worldScale;
				} catch (FormatException _) {
					// propagate the last position forward
					frames[i, j] = frames[i-1, j];
				}
				originalFrames[i, j] = frames[i, j];
			}
		}
		// parse orientation from strings
		for (int i = 0; i < rawOrientations.Length; i++)
		{
			// since the raw orientation data is transformed by same sript
			// that transforms the raw trajectory data. The first line
			// is the group count so skip it.
			if (i == 0)
            {
				continue;
            }
			string[] line = rawOrientations[i].Split(",");
			for (int j = 0; j < line.Length; j++)
			{
				orientation_frames[i - 1,  j] = float.Parse(line[j]);
			}

		}

		// count agents - each frame contains 2N numbers (2 per agent)
		agentCount = frames.GetLength(1) / 2;

		// count frames
		frameCount = frames.GetLength(0);

		// legacy code; smooth agent positions to mitigate position jitter from PBD
		// the smooth result is not that good. Recommand ignore it and smooth
		// before input to Unity
		for (int j = 0; j < passes; j++) {
			for (int agent_i = 0; agent_i < agentCount; agent_i++) {
				for (int frame_i = span; frame_i < frameCount - span; frame_i++) {
					Vector2 previous = new Vector2(frames[frame_i-span, 2*agent_i], frames[frame_i-span, 2*agent_i+1]);
					Vector2 next = new Vector2(frames[frame_i+span, 2*agent_i], frames[frame_i+span, 2*agent_i+1]);
					Vector2 current = new Vector2(frames[frame_i, 2*agent_i], frames[frame_i, 2*agent_i+1]);
					var mid = (previous + next)/2.0f;
					var offset = mid - current;
					current += offset * strength;
					frames[frame_i, 2*agent_i] = current.x;
					frames[frame_i, 2*agent_i+1] = current.y;
				}
			}
		}


		// calculate distance difference. It is used as velocity for agent.
		// no need to consider delta time since it will finally be normalized 
		// between value a and b, default value: a=0, b=4
		// The normalized velocity is for matching the parameter threshold in blend tree 
		// standstill: 0
		// walking: 4
		// running: 8
		// The value will be used to blending animation
		difference = CalculateDifferences(frames);
		normalized_diff = NormalizeArray(difference, A, B);
		// apply threshold after the normalization.
		// used for avoiding abnormal jittering
		// magic number and need to be tuned for each scenario
		normalized_diff = ApplyThreshold(normalized_diff, T);

		// instantiate all agents
		int colorIndex = 0;
		int agentIndexInGroup = 0;

		for (int i = 0; i < agentCount; i++) {
			
 			if (colorCode) {
				if (agentIndexInGroup > groupCounts[colorIndex]-1) {
					agentIndexInGroup = 0;
					colorIndex++;
				}
				agentIndexInGroup += 1;
			}
			// since we have too many different pattern of agent order in input file
			// the below code is used for coloring in some special cases

			// if the trajectory file from Bilas
            if (BILAS)
            {
				// if the scenario is "circle" related one
				if (!sourceFile.Contains("Circle") && !sourceFile.Contains("circle") && !sourceFile.Contains("Quad"))
				{
					if (agentIndexInGroup % 2 == 0)
					{
						colorIndex = 0;

					}
					else
					{
						colorIndex = 1;
					}

					if (agentCount == 2)
					{
						colorIndex = i;

					}
				}
			}
			// there is a small trick that instantiate agent in an opposite facing direction (0, 0, 0) -> (0, 180, 0)
			// to the original Unity setting, which is used for threejs bottleneck case
			// If the scenario comes from threejs and wish to have sidestep,
			// the flag should not be disabled until correcting orientation + facing orientation data from raw orientation file
			GameObject agent;
			if (sideSteppingFlag)
            {
				agent  = Instantiate(agentPrefab, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0, 180, 0))); //Quaternion.identity
            }
            else
            {
				agent = Instantiate(agentPrefab, new Vector3(0, 0, 0), Quaternion.identity);

			}

			AgentMovement script = agent.GetComponent<AgentMovement>();
			script.SetCapsule(useCapsules);
		

			if (groupToFollow == colorIndex) {
				cameraObject.GetComponent<CameraController>().agentsToFollow.Add(agent);
			}
			if (lightGroupToFollow == colorIndex) {
				FocusLight.GetComponent<PointLightGroupFollowController>().agentsToFollow.Add(agent);
			}

			if (colorCode) {
				script.SetColor(groupColors[colorIndex]);
			}
			if (i == agentToColorOverride) {
				script.SetColor(agentColorOverride);
			}
			
			agents.Add(script);
			agentpositions.Add(new List<Vector3>());


		}

		// legacy code; ueue agent IK updates
		// The IK related code is from first developer
		// be careful it works not in expectation, since it kinda makes the
		// leg animation and upper body animation not synchronized
		// recommand to disable the IK script on agent prefab if possible
		InvokeRepeating("UpdateIK", 0.25f, 0.25f);
 

	}

	private void UpdateIK() {
		foreach (AgentMovement agent in agents) {
			agent.CheckIKDistance(cameraObject.transform.position);
		}
	}

	void Update() {

		
		// trigger next frame
		if (frameIndex < (float)frameCount - delta)
		{
 
			NextFrame();

		}



	}

	// convinience method to go to the next frame
	public void NextFrame() {
		// increment frame
		frameIndex += frameStep;

		// clamp to valid bounds
		if (frameIndex > (float)frameCount- delta) {
			frameIndex = (float)frameCount- delta;

			// GameObject.Find("VideoCapture").GetComponent<VideoCaptureGUI>().GetVideoCaptureObject().StopCapture();
			// GameObject.Find("VideoCapture").GetComponent<VideoCapture>().StopCapture();
		}
		if (frameIndex < 0) {
			frameIndex = 0;
		}
		
		// actually activate the frame
		MoveToCurrentFrame();
	}

	public void MoveToCurrentFrame() {
		// move agents by reading from frames
		for (int i = 0; i < agentCount; i++) {
			int integer = (int)frameIndex;
			float fractional = frameIndex - integer;
			
			float x = frames[integer, 2*i];
			float y = frames[integer, 2*i+1];
			float n = normalized_diff[integer, i];
			float r = orientation_frames[integer, i];

			// if the 'frame step' is not integer
			// the agent position, velocity, orientation will be interpolated
			if (fractional != 0) {
				x = Mathf.Lerp(x, frames[integer+1, 2*i], fractional);
				y = Mathf.Lerp(y, frames[integer+1, 2*i+1], fractional);
				n = Mathf.Lerp(n, normalized_diff[integer + 1, i], fractional);
				r = Mathf.Lerp(r, orientation_frames[integer + 1, i], fractional);

			}

			Vector2 pos = new Vector2(x,y);
			// 'MoveTo' is another core code to process the animation
            agents[i].MoveTo(pos, frameIndex, frameCount, delta, n, r, shiftDegree, orentationFlag);
            agentpositions[i].Add(new Vector3(pos.x, 0.1f, pos.y));

			// display trajectory
            if (renderTrajectory)
            {
				RenderTrajectory(i, trajectoryLength, fullTrajectory);

			}
		}

	}

	public void SetFrameIndex(int i) {
		// called by UI when the user changes the frame
		// explicitely a seperate function to not be affected by pausing (since FixedUpdate is no longer called when paused)
		frameIndex = i;
		MoveToCurrentFrame();
	}

	public void SetSimulationSpeed(float s) {
		// called by UI when a user changes the speed
		frameStep = s;
	}

	public void SetCapsuleMode(bool v) {
		// called by UI when a user changes the display mode
		useCapsules = v;
		foreach (AgentMovement agent in agents) {
			agent.SetCapsule(useCapsules);
		}
	}

	 
	public void SetShadowMode(bool v) {
		// called by UI when a user toggles shadows
		useShadows = v;
		foreach (AgentMovement agent in agents) {
			agent.SetShadow(useShadows);
		}
	}

	public void SetArrowMode(bool v) {
		// called by UI when a user toggle arrows
		useArrows = v;
		foreach (AgentMovement agent in agents) {
			agent.SetArrow(useArrows);
		}
	}

	void RenderTrajectory(int playerIndex,  int renderedPoints = 3000, bool all = false)
	{

		GameObject agent = agents[playerIndex].gameObject;

		LineRenderer lr = agent.GetComponent<LineRenderer>();

		lr.material = new Material(Shader.Find("Sprites/Default"));

		// Set some positions
		List<Vector3> positions = new List<Vector3>();

		int end = agentpositions[playerIndex].Count;
		int start = 0;

		if (!all && end - renderedPoints > 0)
		{
			start = end - renderedPoints;
		}

		 

		for (int i = start; i < end; i++)
		{
			Vector3 pos = agentpositions[playerIndex][i];
			positions.Add(pos);

		}

		lr.startWidth = trajectoryThinckness;
		lr.endWidth = trajectoryThinckness;
		lr.positionCount = positions.Count;
		lr.SetPositions(positions.ToArray());

		// A simple 2 color gradient with a fixed alpha of 1.0f.
		float alpha = 1.0f;
		Gradient gradient = new Gradient();

		if (gradientFlag)
        {
			


			gradient.SetKeys(
				new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(agents[playerIndex].color, 1.0f) },
				new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
			);

			
        }
        else
        {
			gradient.SetKeys(
				new GradientColorKey[] { new GradientColorKey(agents[playerIndex].color, 0f), new GradientColorKey(agents[playerIndex].color, 1.0f) },
				new GradientAlphaKey[] { new GradientAlphaKey(alpha, 1.0f), new GradientAlphaKey(alpha, 1.0f) }
			);
		}

		lr.colorGradient = gradient;

	}

	
}
