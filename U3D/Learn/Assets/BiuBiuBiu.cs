using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BookView;
using MsgCenter;

public class BiuBiuBiu : MonoBehaviour {

    public GameObject Girl;
    public GameObject BallPrefab;
    public GameObject wall;
    public WallReceiver wallReceiver;

    public class WallReceiver : Message.Receiver /* MsgType=2 */
    {
        public int Life;
        public GameObject Obj;
        public override void ReceiveToDo(Message.Packet packet)
        {
            ViewMsg.CollisionMsg e = packet.Msg as ViewMsg.CollisionMsg;
            if (e.Collided == Obj)
            {
                Debug.Log(e.Describe);
                Life = Life - 1;
                if (Life <= 0)
                {
                    Destroy(Obj);
                    ReceiveClose(Message.eMsgType.Collision);
                }
                Destroy(e.Colliding);
            }
        }
    }

    void Awake()
    {
        ForDebug.ClearConsole();
        Girl = GameObject.Find("Girl");
        wall = GameObject.Find("WallBlue");
        wallReceiver = new WallReceiver();
        wallReceiver.Life = 5;
        wallReceiver.Obj = wall;
        wallReceiver.ReceiveOpen(Message.eMsgType.Collision);

        Debug.Log("加载预制体");
        //BallPrefab = (GameObject)Instantiate(Resources.Load("BallPrefab"));
        BallPrefab = Resources.Load(@"Bullet\BallPrefab") as GameObject;
        Debug.Log("预制体：" + BallPrefab.name);
    }

    // Use this for initialization
    void Start()
    {
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
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("^Ground");
            if (Operation.ClickPoint(out click, out obj, regex))
            {
                string objName = obj.name;
                Debug.Log("点击：" + obj.name);
                if (objName == "Ground")
                {
                    Others.CreateAndBiu(BallPrefab, "NewBall", click + new Vector3(0, 2, 0), new Vector3(0, -2, 0));
                }
            }
        }

    } // Update

    private void FixedUpdate()
    {
    }


    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        Debug.Log("当前点击的UI是：" + eventData.pointerEnter);
    }

    private void OnGUI()
    {
        if (wall != null)
        {
            MyGui.ObjectTopText(wallReceiver.Obj, 0.2F, wallReceiver.Obj.name + " " + wallReceiver.Life);
        }
    }

}
