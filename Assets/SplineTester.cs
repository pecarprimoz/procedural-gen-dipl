using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineTester : MonoBehaviour
{
    private Spline Spline;
    private float time;
    public Vector3 direction;
    void Start()
    {
        direction = new Vector3(1, 0, 0);
        Spline = GetComponent<Spline>();
    }

    // Update is called once per frame
    void Update()
    {
        if (time > 1.0f)
        {
            Debug.Log("Added road segment");
            var lastNode = Spline.nodes[Spline.nodes.Count - 1];
            Spline.AddNode(new SplineNode(lastNode.Direction, lastNode.Direction - direction));
            time = 0;
        }
        time += Time.deltaTime;
    }
}
