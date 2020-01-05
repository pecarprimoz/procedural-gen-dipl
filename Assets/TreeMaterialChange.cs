using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeMaterialChange : MonoBehaviour
{
    private MeshRenderer TreeRender;
    // Start is called before the first frame update
    void Start()
    {
        TreeRender = GetComponent<MeshRenderer>();
        var leafMat = TreeRender.materials[0];
        //leafMat.SetColor("_Color", Color.white);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
