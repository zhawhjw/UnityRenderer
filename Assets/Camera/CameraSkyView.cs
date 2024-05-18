using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSkyView : MonoBehaviour
{
    public GameObject playerCamera;

	public void TeleportCamera() {
		playerCamera.transform.position = transform.position;
		playerCamera.transform.rotation = transform.rotation;
		playerCamera.GetComponent<CameraController>().SetCameraRot(transform.eulerAngles.x, transform.eulerAngles.y);
	}
}
