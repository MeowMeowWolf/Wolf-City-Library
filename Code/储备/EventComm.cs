using System;
using System.Collections.Generic;

namespace NsEventComm
{
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
        public uint MsgType;
        public MsgBase Msg;
        public Packet(uint msgType, MsgBase msg)
        {
            Sender = null;
            MsgType = msgType;
            Msg = msg;
        }
        public Packet(Object sender, uint msgType, MsgBase msg)
        {
            Sender = sender;
            MsgType = msgType;
            Msg = msg;
        }
    }

    // 消息接收者的消息处理函数的委托
    public delegate void ReceiverDelegate(Packet packet);

    // 消息中心
    public static class MsgCenter
    {
        // 监听列表
        private static Dictionary<uint, ReceiverDelegate> Receivers = new Dictionary<uint, ReceiverDelegate>();

        // 注册消息监听
        public static void AddReceiver(uint msgType, ReceiverDelegate Receive)
        {
            if (!Receivers.ContainsKey(msgType))
            {
                ReceiverDelegate del = null; //定义方法
                Receivers[msgType] = del;// 给委托变量赋值
            }
            Receivers[msgType] += Receive; //注册接收者的监听
        }

        // 注销消息监听
        public static void RemoveReceiver(uint msgType, ReceiverDelegate Receive)
        {
            if (Receivers.ContainsKey(msgType))
            {
                Receivers[msgType] -= Receive;
                if (Receivers[msgType] == null)
                {
                    Receivers.Remove(msgType);
                }
            }
        }

        // 消息推送
        public static void Push(Packet packet)
        {
            if (Receivers.ContainsKey(packet.MsgType))
            {
                packet.Msg.Alive = true;
                Receivers[packet.MsgType](packet);
            }
        }
    }

    // 接收者的基类
    public class Receiver
    {
        // 注册监听
        public void ReceiveOpen(uint msgType)
        {
            MsgCenter.AddReceiver(msgType, ReceiveToDo);
        }
        // 注销监听
        public void ReceiveClose(uint msgType)
        {
            MsgCenter.RemoveReceiver(msgType, ReceiveToDo);
        }
        // 消息处理
        public virtual void ReceiveToDo(Packet packet)
        {
            // Do Something
        }
    }

}//namespace
