using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using BookView;

public class GirlCtrl : MonoBehaviour {
    
    GameObject Girl;
    GameObject Eye;
    GameObject Ball;
    Movement.MovingControl mctrl;

    // Use this for initialization
    void Start ()
    {
        ForDebug.ClearConsole();

        Girl = GameObject.Find("Girl");
        Eye = GameObject.Find("Eye");
        Ball = GameObject.Find("Ball");
        mctrl = new Movement.MovingControl(Girl, 0.1);

        Sensce.CreateAirWallBox(new Vector3(0, 8, 0), new Vector3(15, 20, 28));
        
    }

    // Update is called once per frame
    void Update ()
    {
        mctrl.ListenKey();
        //CollisionFlags flag = Girl.GetComponent<Rigidbody>().mo
    }

    private void FixedUpdate()
    {
        mctrl.Moving();

        Movement.Follow(Eye, new Vector3(5, 5, 0), Girl);
        Movement.Follow(Ball, 0.05, 2, Girl);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log( this.gameObject.name + "碰撞" + collision.gameObject.name);
        string cube = collision.gameObject.name.Substring(0,4);
        Debug.Log(cube+"!!");
        if (cube == "Cube")
        {
            Debug.Log("销毁"+ collision.gameObject.name);
            Destroy(collision.gameObject,1);
        }
    }

}
