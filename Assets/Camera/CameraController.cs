using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float maxVerticalAngle = 90.0f;
	public float slowCameraSpeed = 20.0f;
	public float fastCameraSpeed = 60.0f;
	public List<GameObject> agentsToFollow = new List<GameObject>();
	private float cameraYRot = 0.0f;
	private float cameraXRot = 0.0f;
	

    public float speed_H = 2.0f;
    public float speed_V = 2.0f;

    public float yaw = 0.0f;
    public float pitch = 0.0f;
    public float speed = 5.0f;
    public Vector3 inital_rotation;

    float zoom;

    [Header("Zoom")]
    public float minZoom = 5.0f;
    public float maxZoom = 250.0f;

    // Start is called before the first frame update
    void Start()
    {
        inital_rotation = transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {

        Rotate();
        Move();
        Zoom();


    }

    void Rotate()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += speed_H * Input.GetAxis("Mouse X");

            pitch -= speed_V * Input.GetAxis("Mouse Y");


            transform.eulerAngles = inital_rotation + new Vector3(pitch, yaw, 0.0f);
        }
    }

    private void Move()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));
        }
    }

    void Zoom()
    {
        float ScrollWheelChange = Input.GetAxis("Mouse ScrollWheel");           //This little peece of code is written by JelleWho https://github.com/jellewie
        if (ScrollWheelChange != 0)
        {                                            //If the scrollwheel has changed
            float R = ScrollWheelChange * 5;                                   //The radius from current camera
            float PosX = transform.eulerAngles.x + 90;              //Get up and down
            float PosY = -1 * (transform.eulerAngles.y - 90);       //Get left to right
            PosX = PosX / 180 * Mathf.PI;                                       //Convert from degrees to radians
            PosY = PosY / 180 * Mathf.PI;                                       //^
            float X = R * Mathf.Sin(PosX) * Mathf.Cos(PosY);                    //Calculate new coords
            float Z = R * Mathf.Sin(PosX) * Mathf.Sin(PosY);                    //^
            float Y = R * Mathf.Cos(PosX);                                      //^
            float CamX = transform.position.x;                      //Get current camera postition for the offset
            float CamY = transform.position.y;                      //^
            float CamZ = transform.position.z;                      //^
            transform.position = new Vector3(CamX + X, CamY + Y, CamZ + Z);//Move the main camera
        }
    }

    // called when the skyview is activated
    public void SetCameraRot(float y, float x) {
		cameraYRot = y;
		cameraXRot = x;
	}
}
