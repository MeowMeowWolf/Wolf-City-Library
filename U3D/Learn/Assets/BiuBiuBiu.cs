using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BookView;

public class BiuBiuBiu : MonoBehaviour {

    public GameObject Girl;
    public GameObject BallPrefab;

    // Use this for initialization
    void Start() {
        Girl = GameObject.Find("Girl");
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("发射！");
            Vector3 dir = Calculate.UnitDirection(Girl.transform.forward); /*角度方向的矢量*/
            Vector3 pos = Girl.transform.position + dir * (float)1.2;
            Others.CreateAndBiu(BallPrefab, "NewBall", pos, dir);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector3 click;
            GameObject obj;
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("^Plane$");
            if ( Operation.ClickPoint(out click,  out obj, regex) )
            {
                string objName = obj.name;
                Debug.Log("点击：" + obj.name);
                if (objName == "Plane")
                {
                    Others.CreateAndBiu(BallPrefab, "NewBall", click + new Vector3(0, 2, 0), new Vector3(0, -2, 0));
                }
            }
        }

    } // Update


    private void FixedUpdate()
    {
    }

    
    

}
