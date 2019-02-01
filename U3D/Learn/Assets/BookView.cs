using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MsgCenter;

namespace BookView
{
    // 一维方向
    public enum OneDimensionalDirection
    { Minus = -1, Origin = 0, Plus = 1 }

    // 三个坐标轴
    public enum ThreeDimensionalAxis
    { x, y, z }

    public class ForDebug
    {
        // 清理debug日志
        private static bool isClean = false;
        public static void ClearConsole()
        {
            if (isClean == false)
            {
                /*using System; using System.Reflection; using UnityEditor;*/
                Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
                Type logEntries = assembly.GetType("UnityEditor.LogEntries");
                MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
                clearConsoleMethod.Invoke(new object(), null);
                isClean = true;

                Debug.Log("模块的完整路径：" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                Debug.Log("当前目录：" + System.Environment.CurrentDirectory);
                Debug.Log("程序的当前工作目录：" + System.IO.Directory.GetCurrentDirectory());
                Debug.Log("程序的基目录：" + System.AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        // 画一条线（只有在Scene能看见）
        public static void DrawLine(Vector3 p1,Vector3 p2)
        {
            Debug.DrawLine(p1, p2, Color.blue);
        }

    } //ForDebug

    // 用户操作类
    public class Operation
    {

        public static bool ClickOnUI()
        {
            return false;
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("点在UI上");
                return true;
            }
            else
            {
                Debug.Log("点在场景里");
                return false;
            }
        }
       
        // 鼠标点击位置及点击物体检测
        public static bool ClickPoint(out Vector3 point, out GameObject gameObject)
        {
            // 需要把摄像头物件的tag标记为MainCamera
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!ClickOnUI() && Physics.Raycast(ray, out hit))
            {
                point = hit.point;
                gameObject = hit.collider.gameObject;
                return true;
            }
            else
            {
                point = Vector3.zero;
                gameObject = null;
                return false;
            }
        } // ClickPoint

        // 鼠标点击位置及点击物体检测，物体名称需匹配正则表达式，不匹配则跳过该物体，检测射线上的下一物体
        public static bool ClickPoint(out Vector3 point, out GameObject gameObject, System.Text.RegularExpressions.Regex gameObjectNameRegex)
        {
            float accuracy = 0.001F; // 表示检测点最小间隔
            int count = 10; // 最大循环次数
            point = Vector3.zero;
            gameObject = null;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            while (true)
            {
                if (!ClickOnUI() && Physics.Raycast(ray, out hit)  )
                {
                    Debug.Log("判断"+ hit.collider.gameObject.name);
                    if (gameObjectNameRegex.IsMatch(hit.collider.gameObject.name))
                    {
                        point = hit.point;
                        gameObject = hit.collider.gameObject;
                        return true;
                    }
                    else
                    {
                        ray.origin = hit.point + Calculate.UnitDirection(ray.direction) * accuracy;
                    }
                }
                else
                {
                    point = Vector3.zero;
                    gameObject = null;
                    return false;
                }
                
                count--;
                if (count == 0)
                {
                    return false;
                }
            } // while
        } // ClickPoint


    } // Operation

    // 一些数学计算（坑爹的float格式）
    public class Calculate : MonoBehaviour
    {
        // 计算精度
        static int precision = 1000;

        // 判断相等
        public static bool equals(float f, int i)
        {
            int F = (int)(f * precision);
            int I = (int)(i * precision);
            return (F == I);
        }

        // 获取一个矢量的单位矢量
        public static Vector3 UnitDirection(Vector3 RawDirection)
        {
            return (RawDirection / RawDirection.magnitude);
        }

        // 获得物件的真实尺寸
        public static Vector3 GetRealSize(GameObject obj)
        {
            Vector3 RawSize = obj.GetComponent<MeshFilter>().mesh.bounds.size;
            Vector3 ScaleRatio = obj.transform.localScale;
            Vector3 RealSize = new Vector3();
            RealSize.x = RawSize.x * ScaleRatio.x;
            RealSize.y = RawSize.y * ScaleRatio.y;
            RealSize.z = RawSize.z * ScaleRatio.z;
            return RealSize;
        }

        // 设置物件的真实尺寸
        public static void SetRealSize(GameObject obj, Vector3 RealSize)
        {
            Vector3 RawSize = obj.GetComponent<MeshFilter>().mesh.bounds.size;
            Vector3 ScaleRatio = new Vector3();

            if (equals(RawSize.x, 0))
            { ScaleRatio.x = 1; }
            else
            { ScaleRatio.x = RealSize.x / RawSize.x; }

            if (equals(RawSize.y, 0))
            { ScaleRatio.y = 1; }
            else
            { ScaleRatio.y = RealSize.y / RawSize.y; }

            if (equals(RawSize.z, 0))
            { ScaleRatio.z = 1; }
            else
            { ScaleRatio.z = RealSize.z / RawSize.z; }

            obj.transform.localScale = ScaleRatio;
        }

    } // Calculate

    // 运动相关
    public class Movement
    {
        // 物体跟随另一物体而移动，保持角度和间距不变
        public static void Follow(GameObject FollowingObject, Vector3 PositionDiff, GameObject MovingObject)
        {
            FollowingObject.transform.position = MovingObject.transform.position + PositionDiff;
        }

        // 物体跟随另一物体而移动，按固定速度向后者移动
        public static void Follow(GameObject FollowingObject, double Speed, GameObject MovingObject)
        {
            Vector3 UnitDirection = Calculate.UnitDirection(MovingObject.transform.position - FollowingObject.transform.position);
            FollowingObject.transform.position = FollowingObject.transform.position + (float)Speed * UnitDirection;
        }

        // 物体跟随另一物体而移动，按固定速度向后者移动，且设定最小间隔
        public static void Follow(GameObject FollowingObject, double Speed, double MixDistance, GameObject MovingObject)
        {
            Vector3 Direction = MovingObject.transform.position - FollowingObject.transform.position;
            if (Direction.magnitude >= MixDistance)
            {
                Vector3 UnitDirection = Calculate.UnitDirection(Direction);
                FollowingObject.transform.position = FollowingObject.transform.position + (float)Speed * UnitDirection;
            }
        }

        // 人物移动控制器
        public class MovingControl
        {
            GameObject Object; // 被控制物件
            float Speed; // 移动速度
            OneDimensionalDirection x, y, z; // 三个坐标轴上的移动分状态

            public MovingControl(GameObject @object, double speed)
            {
                Object = @object;
                x = 0; y = 0; z = 0;
                Speed = Convert.ToSingle(speed);
            }

            // 添加某坐标轴上的移动状态
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

            // 终止某坐标轴上的移动状态
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

            // 面向控制
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

            // 监听键盘输入 (需要放在update中执行，否则容易错过输入四将)
            public void ListenKey()
            {
                //前进
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

                // 后退
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

                // 左走
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

                // 右走
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

                // 跳跃
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

                // 重置运动状态和物体角度
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

            // 执行移动
            public void Moving()
            {
                if ((int)x != 0 || (int)y != 0 || (int)z != 0)
                {
                    Vector3 Temp = new Vector3((int)x, (int)y, (int)z);
                    Temp = Calculate.UnitDirection(Temp);
                    Temp = Temp * Speed;
                    Object.transform.position = Object.transform.position + Temp;
                }
            }

        } // MovingControl

    } // Movement

    public class Sensce : MonoBehaviour
    {
        // 建立一个由6面Quad组成的空气墙盒子
        public static void CreateAirWallBox(Vector3 position, Vector3 scale)
        {
            Debug.Log("建一个空气墙盒子");

            GameObject AirWall = new GameObject("AirWall");

            GameObject AirWallZPlus = GameObject.CreatePrimitive(PrimitiveType.Quad);
            AirWallZPlus.name = "AirWallZPlus";
            AirWallZPlus.GetComponent<MeshCollider>().convex = true;
            Destroy(AirWallZPlus.GetComponent<MeshRenderer>());
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
            AirWallYPlus.transform.position = position + new Vector3(0, scale.y / 2, 0);
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
        
    } // Sensce

    public class Others
    {
        // 创造并发射一个物体
        public static void CreateAndBiu(GameObject Object, string NewObjectName, Vector3 position, Vector3 forward)
        {
            GameObject NewBall = GameObject.Instantiate(Object);
            NewBall.name = NewObjectName;
            NewBall.transform.position = position;
            forward.y = forward.y + 1;
            NewBall.GetComponent<Rigidbody>().AddForce(forward * 500);
            NewBall.GetComponent<Rigidbody>().useGravity = true;
        }

    } // Others

    public class ViewMsg
    {
        // 物体碰撞消息
        public class CollisionMsg : Message.MsgBase
        {
            public GameObject Colliding;
            public GameObject Collided;
        }

        // GameObject销毁命令消息
        public class DestroyOrderMsg : Message.MsgBase
        {
            public GameObject DestroyedObj;
        }
    }


    public class MyGui
    {

        // 顶部文字
        public static void ObjectTopText(GameObject Obj, float HeightOverObjTop, string Text)
        {
            Vector3 pV3 = Obj.transform.position;
            pV3.y = pV3.y + Obj.GetComponent<Collider>().bounds.size.y + HeightOverObjTop; // 物体头部位置

            pV3 = Camera.main.WorldToScreenPoint(pV3); // 转化为屏幕坐标
            Vector2 pV2 = new Vector2(pV3.x, Screen.height - pV3.y); // 3D场景和2D屏幕的Y轴方向是相反的，倒转很重要

            Vector2 StrSize = GUI.skin.label.CalcSize(new GUIContent(Text)); // 文字需要占用的屏幕面积
            pV2.x = pV2.x - StrSize.x / 2; // 由于文字输出以左上角为position点
            pV2.y = pV2.y - StrSize.y / 2; // 所以需要调整位置，让文字居中
            GUI.color = Color.red; // 文字颜色
            GUI.Label(new Rect(pV2, StrSize), Text);
        }


    }
    
}// BookView