using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using UnityEngine;
/**
 * 待处理 粘包分包，线程冲突
 */
public static class NetManager
{
    static Socket socket;
    static byte[] readBuff = new byte[1024];
    public delegate void MsgListener(string str);
    private static Dictionary<string, MsgListener> listeners = new Dictionary<string, MsgListener>();
    static List<string> msgList = new List<string>();
    static Queue<string> msgQueue =  new Queue<string>();
    public static void AddListener(string msgName, MsgListener listener)
    {
        UnityEngine.Debug.Log("AddListener:" + msgName);
        listeners[msgName] = listener;
    }

    public static string GetDesc()
    {
        if (socket == null)
        {
            return "";
        }
        if (!socket.Connected)
        {
            return "";
        }
        return socket.LocalEndPoint.ToString();
    }
    public static void Connect(string ip, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //同步方法简化
        socket.Connect(ip, port);
        socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallBack, socket);
    }

    private static void ReceiveCallBack(IAsyncResult result)
    {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            int count = socket.EndReceive(result);
            string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            //msgList.Add(recvStr);
            msgQueue.Enqueue(recvStr);
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallBack, socket);
        }
        catch (SocketException ex)
        {

            UnityEngine.Debug.Log("Socket Receive fail" + ex.ToString());
        }
    }

    public static void Send(string sendStr)
    {
        UnityEngine.Debug.Log("sendMsg:" + sendStr);
        if (socket == null)
        {
            UnityEngine.Debug.Log("socket null;");
            return;
        }
        if (!socket.Connected)
        {
            UnityEngine.Debug.Log("connect null;");
            return;
        }

        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.Send(sendBytes);
    }

    public static void Update()
    {
        /*if (msgList.Count <= 0)
        {
            return;
        }*/
        if (msgQueue.Count <= 0)
        {
            return;
        }
        /*string msgStr = msgList[0];
        msgList.RemoveAt(0);*/
        string msgStr = msgQueue.Dequeue();
        string[] split = msgStr.Split('|');
        string msgName = split[0];
        string msgArgs = split[1];
        if (listeners.ContainsKey(msgName))
        {
            listeners[msgName](msgArgs);
        }
    }

   
}
