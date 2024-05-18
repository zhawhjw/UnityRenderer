using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointLightGroupFollowController : MonoBehaviour
{
    public List<GameObject> agentsToFollow = new List<GameObject>();
    public float height = 5.0f;

    void Update() {
        if (agentsToFollow.Count > 0) {
			Vector3 center = new Vector3(0,0,0);
			foreach (GameObject a in agentsToFollow) {
				center += a.transform.position;
			}
			center /= agentsToFollow.Count;
			center += new Vector3(0.0f, height, 0.0f);
			transform.position = center;
		}
    }
}