using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIEventHandler : MonoBehaviour
{	
	public GameObject agentManagerObject;
	public GameObject skyView;
	private AgentManager agentManager;
	private UIDocument uiDocument;
	private SliderInt frameSlider;
	private Slider simulationSpeedSlider;
	private Toggle capsuleToggle;
	private Toggle rotateCapsuleToggle;
	private Toggle shadowToggle;
	private Toggle arrowToggle;
	private Button skyViewButton;
	private Button hideUIButton;
	private bool hidden = false;

	public void Start() {
		// grab various references
		uiDocument = GetComponent<UIDocument>();
		frameSlider = uiDocument.rootVisualElement.Q<SliderInt>("FrameSlider");
		simulationSpeedSlider = uiDocument.rootVisualElement.Q<Slider>("SpeedSlider");
		capsuleToggle = uiDocument.rootVisualElement.Q<Toggle>("UseCapsuleToggle");
		rotateCapsuleToggle = uiDocument.rootVisualElement.Q<Toggle>("UseRotateCapsuleToggle");
		shadowToggle = uiDocument.rootVisualElement.Q<Toggle>("ShadowToggle");
		arrowToggle = uiDocument.rootVisualElement.Q<Toggle>("ArrowToggle");
		skyViewButton = uiDocument.rootVisualElement.Q<Button>("SkyViewButton");
		hideUIButton = uiDocument.rootVisualElement.Q<Button>("HideUIButton");
		agentManager = agentManagerObject.GetComponent<AgentManager>();

		// configure the slider max value to correspond to the number of frames
		// this works because this script is set to run AFTER agent manager has completed initiallization and thus counted the frames
		frameSlider.highValue = agentManager.frameCount-1;

		// register various input callbacks
		frameSlider.RegisterValueChangedCallback(FrameChanged);
		simulationSpeedSlider.RegisterValueChangedCallback(SpeedChanged);
		capsuleToggle.RegisterValueChangedCallback(CapsuleChanged);
		shadowToggle.RegisterValueChangedCallback(ShadowChanged);
		arrowToggle.RegisterValueChangedCallback(ArrowChanged);
		hideUIButton.clicked += () => {
			hidden = true;
			uiDocument.panelSettings.scale = 0.0f;
		};
		skyViewButton.clicked += () => {
			
		};

		skyView.GetComponent<CameraSkyView>().TeleportCamera();
	}

	public void Update() {
		// watch for UI re-enable events
		if (hidden) {
			if (Input.GetKeyDown(KeyCode.Escape)) {
				hidden = false;
				uiDocument.panelSettings.scale = 1.0f;
			}
		}

		// update the slider value based on the frame index of the agent manager
		frameSlider.SetValueWithoutNotify((int) agentManager.frameIndex);
	}

	private void FrameChanged(ChangeEvent<int> evt) {
		// update agent manager frame index
		agentManager.SetFrameIndex(evt.newValue);
	}

	private void SpeedChanged(ChangeEvent<float> evt) {
		// update agent manager speed
		agentManager.SetSimulationSpeed(evt.newValue);
	}

	private void CapsuleChanged(ChangeEvent<bool> evt) {
		// update agent manager capsule mode
		agentManager.SetCapsuleMode(evt.newValue);
	}

	private void ShadowChanged(ChangeEvent<bool> evt) {
		// toggle shadows
		agentManager.SetShadowMode(evt.newValue);
	}

	private void ArrowChanged(ChangeEvent<bool> evt) {
		// toggle arrows
		agentManager.SetArrowMode(evt.newValue);
	}
}
