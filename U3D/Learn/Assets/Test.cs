using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    public static Vector3 UnitDirection(Vector3 RawDirection)
    {
        Debug.Log(RawDirection + "/" + RawDirection.magnitude);
        return (RawDirection / RawDirection.magnitude);
    }

    public GameObject BallPrefab;
    public GameObject Girl;

	// Use this for initialization
	void Start () {
        Girl = GameObject.Find("Girl");
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    private void FixedUpdate()
    {
        Girl.GetComponent<Rigidbody>().ResetInertiaTensor();

        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("发射！");
            Vector3 dir = UnitDirection(Girl.transform.forward); /*角度方向的矢量*/
            Vector3 pos = Girl.transform.position + dir * (float)1.2;
            BallBiu(pos, dir);
        }
        
    }

    void BallBiu(Vector3 position, Vector3 forward)
    {
        GameObject NewBall = Instantiate(BallPrefab);
        NewBall.name = "NewBall";
        NewBall.transform.position = position;
        forward.y = forward.y + 1;
        NewBall.GetComponent<Rigidbody>().AddForce(forward * 500);
        NewBall.GetComponent<Rigidbody>().useGravity = true;
    }
}
