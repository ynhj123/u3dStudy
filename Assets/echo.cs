using System;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class echo : MonoBehaviour
{
    //定义套接字
    Socket socket;
    //数据缓存
    
    byte[] readBuff = new byte[1024];
    int buffCount = 0;
    string recvStr = "";
    //ui
    public InputField inputField;
    public Text text;
    //

    public void Connection()
    {
        string ip = "127.0.0.1";
        int port = 8888;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //bio
        //socket.Connect("127.0.0.1", 8888);
        //no b 防止服务器响应或响应过长卡住
        socket.BeginConnect(ip, port, (IAsyncResult) =>
        {
            try
            {
                Socket asyncSocket = (Socket)IAsyncResult.AsyncState;
                asyncSocket.EndConnect(IAsyncResult);
                Debug.Log("Socket connect success");
                asyncSocket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallBack, socket);
            }
            catch (SocketException ex)
            {
                Debug.Log("Socket connect faild,reason" + ex.ToString());
            }
        }, socket);
    }
    public void ReceiveCallBack(IAsyncResult result)
    {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            int count = socket.EndReceive(result);
            buffCount += count;
            socket.BeginReceive(readBuff, buffCount, 1024-buffCount, 0, ReceiveCallBack, socket);
            //recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            //socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallBack, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket receive faild,reason" + ex.ToString());
        }
    }
    public void OnReceiveData()
    {
        Debug.Log("[Receive1] buffcount=" + buffCount);
        Debug.Log("[Receive2] readBuff=" + BitConverter.ToString(readBuff));
        if(buffCount <= 2)
        {
            return;
        }
        Int16 bodyLength = BitConverter.ToInt16(readBuff, 0);
        Debug.Log("[Receive3] bodyLength=" + bodyLength);
        if(buffCount < 2 + bodyLength)
        {
            return;
        }
        string s = System.Text.Encoding.UTF8.GetString(readBuff, 2, buffCount);
        Debug.Log("[Receive4] string=" + s);
        int start = 2 + bodyLength;
        int count = buffCount - start;
        Array.Copy(readBuff, start, readBuff, 0, count);
        buffCount -= start;
        Debug.Log("[Receive5] buffcount=" + buffCount);
        recvStr = s + "\n" + recvStr;
        OnReceiveData();

    }

    public void Send()
    {
        string sendStr = inputField.text;
        byte[] bodyBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        int length = (Int16)bodyBytes.Length;
        byte[] lengthBytes = BitConverter.GetBytes(length);
        byte[] sendBytes = lengthBytes.Concat(bodyBytes).ToArray();
       /* byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);*/
        /*socket.Send(sendBytes);*/

        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, (result) =>
        {
            try
            {
                Socket requestSocket = (Socket)result.AsyncState;
                int count = socket.EndSend(result);
                Debug.Log("Socket send success,count=" + count);
            }
            catch (SocketException ex)
            {
                Debug.Log("Socket send faild,reason" + ex.ToString());
            }
        }, socket);

        /*byte[] readBuff = new Byte[1024];
        int count = socket.Receive(readBuff);
        string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
        text.text = recvStr;
        socket.Close();*/
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        text.text = recvStr;
    }
}
