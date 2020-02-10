using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodScript : MonoBehaviour
{

    public TerrainGeneration Gen;
    public bool Work = false;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 down = transform.TransformDirection(Vector3.down) * 100;
        Debug.DrawRay(transform.position, down, Color.red);
        if (Work)
        {
            (int x, int z) = ((int)transform.position.x, (int)transform.position.z);
            try
            {
                Debug.Log($"HM:{Gen.TerrainInfo.HeightMap[z, x]} // MM: {Gen.TerrainInfo.MoistureMap[z, x]} // TM: {Gen.TerrainInfo.TemperatureMap[z, x]}");
            }
            catch { }
        }
    }
}
