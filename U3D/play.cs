using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

public class play : MonoBehaviour {

    public static void ClearConsole()
    {
        /*using System; using System.Reflection; using UnityEditor;*/
        Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
        Type logEntries = assembly.GetType("UnityEditor.LogEntries");
        MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
        clearConsoleMethod.Invoke(new object(), null);
    }

    public static class FloatMath
    {
        static int precision = 1000;

        public static bool equals(float f, int i)
        {
            int F = (int)(f * precision);
            int I = (int)(i * precision);
            return (F == I);
        }

        public static Vector3 UnitDirection(Vector3 RawDirection)
        {
            return ( RawDirection / RawDirection.magnitude );
        }
    }

    Vector3 GetRealSize(GameObject obj)
    {
        Vector3 RawSize = this.GetComponent<MeshFilter>().mesh.bounds.size;
        Vector3 ScaleRatio = this.transform.localScale;
        Vector3 RealSize = new Vector3();
        RealSize.x = RawSize.x * ScaleRatio.x;
        RealSize.y = RawSize.y * ScaleRatio.y;
        RealSize.z = RawSize.z * ScaleRatio.z;
        return RealSize;
    }

    void SetRealSize(GameObject obj, Vector3 RealSize)
    {
        Vector3 RawSize = obj.GetComponent<MeshFilter>().mesh.bounds.size;
        Vector3 ScaleRatio = new Vector3();

        if ( FloatMath.equals(RawSize.x,0) )
        { ScaleRatio.x = 1;  }
        else
        { ScaleRatio.x = RealSize.x / RawSize.x; }

        if ( FloatMath.equals(RawSize.y, 0) )
        { ScaleRatio.y = 1;  }
        else
        { ScaleRatio.y = RealSize.y / RawSize.y; }

        if ( FloatMath.equals(RawSize.z, 0) )
        { ScaleRatio.z = 1;  }
        else
        { ScaleRatio.z = RealSize.z / RawSize.z; }

        obj.transform.localScale = ScaleRatio;
    }

    void Follow(GameObject FollowingObject, Vector3 PositionDiff, GameObject MovingObject)
    {
        FollowingObject.transform.position = MovingObject.transform.position + PositionDiff;
    }

    void Follow(GameObject FollowingObject, double Speed, GameObject MovingObject)
    {
        Vector3 UnitDirection = FloatMath.UnitDirection( MovingObject.transform.position - FollowingObject.transform.position );
        FollowingObject.transform.position = FollowingObject.transform.position + (float)Speed * UnitDirection;
    }
    void Follow(GameObject FollowingObject, double Speed, double MixDistance, GameObject MovingObject)
    {
        Vector3 Direction = MovingObject.transform.position - FollowingObject.transform.position;
        if (Direction.magnitude >= MixDistance)
        {
            Vector3 UnitDirection = FloatMath.UnitDirection(Direction);
            FollowingObject.transform.position = FollowingObject.transform.position + (float)Speed * UnitDirection;
        }
    }


    enum OneDimensionalDirection
    { Minus=-1, Origin=0, Plus=1 }
    enum ThreeDimensionalAxis
    { x,y,z }

    class MovingControl
    {
        float Speed;
        OneDimensionalDirection x, y, z;
        GameObject Object;

        public MovingControl(GameObject @object ,float speed)
        {
            Object = @object;
            x = 0; y = 0; z = 0;
            Speed = speed;
        }
        public MovingControl(GameObject @object, double speed)
        {
            Object = @object;
            x = 0; y = 0; z = 0;
            Speed = Convert.ToSingle(speed);
        }

        public void AddMovingDirection(ThreeDimensionalAxis axis, OneDimensionalDirection direction)
        {
            switch (axis)
            {
                case ThreeDimensionalAxis.x:
                    x = direction;
                    break;
                case ThreeDimensionalAxis.y:
                    y = direction;
                    break;
                case ThreeDimensionalAxis.z:
                    z = direction;
                    break;
            }
            RotationAngle();
        }

        public void RemoveMovingDirection(ThreeDimensionalAxis axis, OneDimensionalDirection direction)
        {
            switch (axis)
            {
                case ThreeDimensionalAxis.x:
                    if (x == direction) { x = 0; }
                    break;
                case ThreeDimensionalAxis.y:
                    if (y == direction) { y = 0; }
                    break;
                case ThreeDimensionalAxis.z:
                    if (z == direction) { z = 0; }
                    break;
            }
        }

        public void RotationAngle()
        {
            int angle = 0;
            int Z = (int)z;
            int X = (int)x;
            if (Z == 1 && X == 0) { angle = 0; }
            if (Z == 1 && X == 1) { angle = 45; }
            if (Z == 0 && X == 1) { angle = 90; }
            if (Z == -1 && X == 1) { angle = 135; }
            if (Z == -1 && X == 0) { angle = 180; }
            if (Z == -1 && X == -1) { angle = 225; }
            if (Z == 0 && X == -1) { angle = 270; }
            if (Z == 1 && X == -1) { angle = 315; }

            this.Object.transform.rotation = Quaternion.Euler(0, angle, 0);
        }

        public void ListenKey()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("您按下了W键");
                AddMovingDirection(ThreeDimensionalAxis.x, OneDimensionalDirection.Minus);
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                Debug.Log("您松开了W键");
                RemoveMovingDirection(ThreeDimensionalAxis.x, OneDimensionalDirection.Minus);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("您按下了S键");
                AddMovingDirection(ThreeDimensionalAxis.x, OneDimensionalDirection.Plus);
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                Debug.Log("您松开了S键");
                RemoveMovingDirection(ThreeDimensionalAxis.x, OneDimensionalDirection.Plus);
            }

            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("您按下了Q/A键");
                AddMovingDirection(ThreeDimensionalAxis.z, OneDimensionalDirection.Minus);
            }
            if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.A))
            {
                Debug.Log("您松开了Q/A键");
                RemoveMovingDirection(ThreeDimensionalAxis.z, OneDimensionalDirection.Minus);
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.D))
            {
                Debug.Log("您按下了E/D键");
                AddMovingDirection(ThreeDimensionalAxis.z, OneDimensionalDirection.Plus);
            }
            if (Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.D))
            {
                Debug.Log("您松开了E/D键");
                RemoveMovingDirection(ThreeDimensionalAxis.z, OneDimensionalDirection.Plus);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("您按下了空格键");
                AddMovingDirection(ThreeDimensionalAxis.y, OneDimensionalDirection.Plus);
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                Debug.Log("您松开了空格键");
                RemoveMovingDirection(ThreeDimensionalAxis.y, OneDimensionalDirection.Plus);
            }

            if (Input.GetKeyUp(KeyCode.Return))
            {
                Debug.Log("您按下了回车键");
                this.Object.GetComponent<Rigidbody>().Sleep();
                this.Object.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            }
            if (Input.GetKeyUp(KeyCode.Return))
            {
                Debug.Log("您松开了回车键");
                this.Object.GetComponent<Rigidbody>().WakeUp();
            }

        }
        

        public void Moving()
        {
            if ((int)x != 0 || (int)y != 0 || (int)z != 0)
            {
                Vector3 Temp = new Vector3((int)x, (int)y, (int)z);
                Temp = FloatMath.UnitDirection(Temp);
                Temp = Temp * Speed;
                Object.transform.position = Object.transform.position + Temp;
            }
        }
        
    }

    // 建立一个由6面Quad组成的空气墙盒子
    void CreateAirWallBox(Vector3 position, Vector3 scale)
    {
        Debug.Log("建一个空气墙盒子");

        GameObject AirWall = new GameObject("AirWall");

        GameObject AirWallZPlus = GameObject.CreatePrimitive(PrimitiveType.Quad);
        AirWallZPlus.name = "AirWallZPlus";
        AirWallZPlus.GetComponent<MeshCollider>().convex = true;
        Destroy(AirWallZPlus.GetComponent<MeshRenderer>() );
        AirWallZPlus.transform.position = position + new Vector3(0, 0, scale.z / 2);
        AirWallZPlus.transform.localScale = new Vector3(scale.x, scale.y, 1);
        AirWallZPlus.transform.Rotate(0, 0, 0);

        GameObject AirWallZMinus = GameObject.CreatePrimitive(PrimitiveType.Quad);
        AirWallZMinus.name = "AirWallZMinus";
        AirWallZMinus.GetComponent<MeshCollider>().convex = true;
        Destroy(AirWallZMinus.GetComponent<MeshRenderer>());
        AirWallZMinus.transform.position = position - new Vector3(0, 0, scale.z / 2);
        AirWallZMinus.transform.localScale = new Vector3(scale.x, scale.y, 1);
        AirWallZMinus.transform.Rotate(0, 0, 0);

        GameObject AirWallXPlus = GameObject.CreatePrimitive(PrimitiveType.Quad);
        AirWallXPlus.name = "AirWallXPlus";
        AirWallXPlus.GetComponent<MeshCollider>().convex = true;
        Destroy(AirWallXPlus.GetComponent<MeshRenderer>());
        AirWallXPlus.transform.position = position + new Vector3(scale.x / 2, 0, 0);
        AirWallXPlus.transform.localScale = new Vector3(scale.z, scale.y, 1);
        AirWallXPlus.transform.Rotate(0, 90, 0);

        GameObject AirWallXMinus = GameObject.CreatePrimitive(PrimitiveType.Quad);
        AirWallXMinus.name = "AirWallXMinus";
        AirWallXMinus.GetComponent<MeshCollider>().convex = true;
        Destroy(AirWallXMinus.GetComponent<MeshRenderer>());
        AirWallXMinus.transform.position = position - new Vector3(scale.x / 2, 0, 0);
        AirWallXMinus.transform.localScale = new Vector3(scale.z, scale.y, 1);
        AirWallXMinus.transform.Rotate(0, 90, 0);

        GameObject AirWallYPlus = GameObject.CreatePrimitive(PrimitiveType.Quad);
        AirWallYPlus.name = "AirWallYPlus";
        AirWallYPlus.GetComponent<MeshCollider>().convex = true;
        Destroy(AirWallYPlus.GetComponent<MeshRenderer>());
        AirWallYPlus.transform.position = position + new Vector3( 0 , scale.y / 2, 0);
        AirWallYPlus.transform.localScale = new Vector3(scale.x, scale.z, 1);
        AirWallYPlus.transform.Rotate(90, 0, 0);

        GameObject AirWallYMinus = GameObject.CreatePrimitive(PrimitiveType.Quad);
        AirWallYMinus.name = "AirWallYPlus";
        AirWallYMinus.GetComponent<MeshCollider>().convex = true;
        Destroy(AirWallYMinus.GetComponent<MeshRenderer>());
        AirWallYMinus.transform.position = position - new Vector3(0, scale.y / 2, 0);
        AirWallYMinus.transform.localScale = new Vector3(scale.x, scale.z, 1);
        AirWallYMinus.transform.Rotate(90, 0, 0);

        AirWall.transform.position = position;
        //AirWall.transform.localScale = scale;
        AirWallXPlus.transform.parent = AirWall.transform;
        AirWallXMinus.transform.parent = AirWall.transform;
        AirWallYPlus.transform.parent = AirWall.transform;
        AirWallYMinus.transform.parent = AirWall.transform;
        AirWallZPlus.transform.parent = AirWall.transform;
        AirWallZMinus.transform.parent = AirWall.transform;
    }
    
    
    GameObject Girl;
    GameObject Eye;
    GameObject Ball;
    MovingControl mctrl;

    // Use this for initialization
    void Start ()
    {
        ClearConsole();

        Girl = GameObject.Find("Girl");
        Eye = GameObject.Find("Eye");
        Ball = GameObject.Find("Ball");
        mctrl = new MovingControl(Girl, 0.1);
        

        CreateAirWallBox(new Vector3(0, 8, 0), new Vector3(15, 20, 28));

        // 只有在 Scene能看见
        Debug.DrawLine(new Vector3(0,0,0), new Vector3(10, 5, 10), Color.blue);

    }

    // Update is called once per frame
    void Update ()
    {
    }

    private void FixedUpdate()
    {
        mctrl.ListenKey();
        mctrl.Moving();

        Follow(Eye, new Vector3(5, 5, 0), Girl);
        Follow(Ball, 0.05, 2, Girl);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log( this.gameObject.name + "碰撞" + collision.gameObject.name);
        string cube = collision.gameObject.name.Substring(0,4);
        Debug.Log(cube+"!!");
        Debug.Log(cube+"!!");
        if (cube == "Cube")
        {
            Debug.Log("销毁"+ collision.gameObject.name);
            Destroy(collision.gameObject,1);
        }
    }
    
}
