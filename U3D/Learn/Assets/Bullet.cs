using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BookView;
using System;
using MsgCenter;

public class Bullet : MonoBehaviour {

    private void OnCollisionEnter(Collision collision)
    {
        ViewMsg.CollisionMsg msg = new ViewMsg.CollisionMsg();
        msg.HappenTime = DateTime.Now;
        msg.Colliding = this.gameObject;
        msg.Collided = collision.gameObject;
        msg.Describe = msg.Colliding.name + "碰撞" + msg.Collided.name;
        Message.Packet packet = new Message.Packet(this.gameObject, Message.eMsgType.Collision, msg);
        Message.Center.Push(packet);
    }
}
