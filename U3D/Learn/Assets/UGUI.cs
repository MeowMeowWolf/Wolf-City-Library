using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UGUI : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnGUI()
    {
        if (GUI.Button(new Rect(200, 100, 50, 50), "NewButton"))
        {
            Debug.Log("按钮创建");
        }
    }
}
