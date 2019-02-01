using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MsgCenter
{
    public class Message
    {
        // 消息类型 (非负整数！)
        public enum eMsgType
        {
            Null = 0, // 无用的消息
            Destroy = 1, // 销毁自身
            Collision = 2, // 碰撞事件
        }

        // 消息基类
        public class MsgBase : EventArgs
        {
            public DateTime HappenTime;
            public bool Alive = false;
            public string Describe;
        }

        // 消息包
        public class Packet
        {
            public Object Sender;
            public eMsgType MsgType;
            public MsgBase Msg;
            public Packet(eMsgType msgType, MsgBase msg)
            {
                Sender = null;
                MsgType = msgType;
                Msg = msg;
            }
            public Packet(Object sender, eMsgType msgType, MsgBase msg)
            {
                Sender = sender;
                MsgType = msgType;
                Msg = msg;
            }
        }

        // 消息接收者的消息处理函数的委托
        public delegate void ReceiverDelegate(Packet packet);

        // 消息中心
        public static class Center
        {
            // 监听列表
            private static Dictionary<uint, ReceiverDelegate> Receivers = new Dictionary<uint, ReceiverDelegate>();

            // 注册消息监听
            public static void AddReceiver(eMsgType msgType, ReceiverDelegate Receive)
            {
                uint msgTypeCode = (uint)msgType;
                if (!Receivers.ContainsKey(msgTypeCode))
                {
                    ReceiverDelegate del = null; //定义方法
                    Receivers[msgTypeCode] = del;// 给委托变量赋值
                }
                Receivers[msgTypeCode] += Receive; //注册接收者的监听
            }

            // 注销消息监听
            public static void RemoveReceiver(eMsgType msgType, ReceiverDelegate Receive)
            {
                uint msgTypeCode = (uint)msgType;
                if (!Receivers.ContainsKey(msgTypeCode))
                    return;
                Receivers[msgTypeCode] -= Receive;
                if (Receivers[msgTypeCode] == null)
                {
                    Receivers.Remove(msgTypeCode);
                }
            }

            // 消息推送
            public static void Push(Packet packet)
            {
                uint msgTypeCode = (uint)packet.MsgType;
                if (Receivers.ContainsKey(msgTypeCode))
                {
                    packet.Msg.Alive = true;
                    Receivers[msgTypeCode](packet);
                }
            }
        }

        // 接收者的基类
        public class Receiver
        {
            // 注册监听
            public void ReceiveOpen(eMsgType MsgType)
            {
                Center.AddReceiver(MsgType, ReceiveToDo);
            }
            // 注销监听
            public void ReceiveClose(eMsgType MsgType)
            {
                Center.RemoveReceiver(MsgType, ReceiveToDo);
            }
            // 消息处理
            public virtual void ReceiveToDo(Packet packet)
            {
                // Do Something
            }
        }

    }
}//namespace
