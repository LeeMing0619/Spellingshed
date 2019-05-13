using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Myscroll : MonoBehaviour {

    private Transform grid;
    public BoxCollider container;
    void Start ()
    {
        grid = this.transform;

    }
	
	// Update is called once per frame
	void Update ()
    {
        if (grid.childCount <= 9)
        {
            container.enabled = false;
        }
        else
        {
            container.enabled = true;
        }
	}
}
