using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BookView;
using System;

public class Bullet : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter(Collision collision)
    {
        Message.MsgCollision msg = new Message.MsgCollision();
        msg.HappenTime = DateTime.Now;
        msg.Colliding = this.gameObject;
        msg.Collided = collision.gameObject;
        msg.Describe = msg.Colliding.name + "碰撞" + msg.Collided.name;
        Message.Packet packet = new Message.Packet(this.gameObject, Message.eMsgType.Collision, msg);
        Message.Center.Push(packet);
    }
}
