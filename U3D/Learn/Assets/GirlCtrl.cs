using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using BookView;

public class GirlCtrl : MonoBehaviour {

    GameObject Girl;
    GameObject ChrCamera;
    GameObject Ball;
    Movement.MovingControl mctrl;

    // Use this for initialization
    void Start()
    {
        ForDebug.ClearConsole();

        Girl = GameObject.Find("Girl");
        ChrCamera = GameObject.Find("Camera");
        Ball = GameObject.Find("Ball");
        mctrl = new Movement.MovingControl(Girl, 0.1);

        Sensce.CreateAirWallBox(new Vector3(0, 8, 0), new Vector3(15, 20, 28));
    }

    // Update is called once per frame
    void Update()
    {
        mctrl.ListenKey();
    }

    private void FixedUpdate()
    {
        mctrl.Moving();

        Movement.Follow(ChrCamera, new Vector3(10, 10, 0), Girl);
        if (Ball != null)
        {
            Movement.Follow(Ball, 0.05, 2, Girl);
        }
    }

    private void OnGUI()
    {
        MyGui.ObjectTopText(Girl, 0.3F, "肉便七 8/8");
    }


}
