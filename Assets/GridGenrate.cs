using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenrate : MonoBehaviour
{

    public GameObject cube;
    public float size = 1f;
    public float margin = 0.1f;
    public int row = 100;
    public int column = 100;
    public Vector3 start = new Vector3(0, 0, 0);
   

    // Start is called before the first frame update
    void Start()
    {

        cube.transform.localScale = new Vector3(size, 0.01f, size);

        Vector3 updatedposition = start;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {

                updatedposition  = start + new Vector3(size * i, 0, size * j) + new Vector3( i * margin, 0,   j *   margin);
                GameObject plate = Instantiate(cube, updatedposition, Quaternion.identity); ;

                plate.transform.parent = transform;
                // plate.transform.GetComponent<MeshRenderer>().material.color = color;
            }   
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
