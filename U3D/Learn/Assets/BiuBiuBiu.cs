using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BookView;

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
            Message.MsgCollision e = packet.Msg as Message.MsgCollision;
            if (e.Collided == Obj)
            {
                Debug.Log(e.Describe);
                Life = Life - 1;
                if (Life <= 0)
                {
                    Destroy(Obj);
                    ReceiveClose(Message.eMsgType.Collision);
                }
                Destroy(e.Colliding, 0.1f);
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        Girl = GameObject.Find("Girl");

        wall = GameObject.Find("Cube-+");
        wallReceiver = new WallReceiver();
        wallReceiver.Life = 5;
        wallReceiver.Obj = wall;
        wallReceiver.ReceiveOpen(Message.eMsgType.Collision);
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
            if (Operation.ClickPoint(out click, out obj, regex))
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
