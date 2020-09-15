using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GameObject humanPrefab;
    public BaseHuman myHuman;
    public Dictionary<string, BaseHuman> otherHumans = new Dictionary<string, BaseHuman>();
    // Start is called before the first frame update
    void Start()
    {
        NetManager.AddListener("Enter", OnEnter);
        NetManager.AddListener("Move", OnMove);
        NetManager.AddListener("Leave", OnLeave);
        NetManager.Connect("127.0.0.1", 8888);
        GameObject obj = (GameObject)Instantiate(humanPrefab);
        float x = Random.Range(-5, 5);
        float z = Random.Range(-5, 5);
        obj.transform.position = new Vector3(x, 0, z);
        myHuman = obj.AddComponent<CtrlHuman>();
        myHuman.desc = NetManager.GetDesc();

        Vector3 position = myHuman.transform.position;
        Vector3 eul = myHuman.transform.eulerAngles;
        string sendStr = "Enter|";
        sendStr += NetManager.GetDesc() + ",";
        sendStr += position.x + ",";
        sendStr += position.y + ",";
        sendStr += position.z + ",";
        sendStr += eul.y;
        NetManager.Send(sendStr);

        NetManager.Send("List|");


    }
    void OnEnter(string msg)
    {
        Debug.Log("OnEnter:" + msg);
        /*string[] split = msg.Split(',');
        string desc = split[0];
        float x = float.Parse(split[1]);
        float y = float.Parse(split[2]);
        float z = float.Parse(split[3]);
        float eulY = float.Parse(split[4]);

        if (desc == NetManager.GetDesc())
        {
            return;
        }*/
        
    }
    void OnMove(string msg)
    {
        Debug.Log("OnMove:" + msg);
        string[] split = msg.Split(',');
        string desc = split[0];
        float x = float.Parse(split[1]);
        float y = float.Parse(split[2]);
        float z = float.Parse(split[3]);
       if (!otherHumans.ContainsKey(desc))
        {
            return;
        }
        BaseHuman h = otherHumans[desc];
        Vector3 targetPos = new Vector3(x, y, z);
        h.MoveTo(targetPos);
    }
    void OnLeave(string msg)
    {
        Debug.Log("OnLeave:" + msg);
        string[] split = msg.Split(',');
        string desc = split[0];
        if (!otherHumans.ContainsKey(desc))
        {
            return;
        }
        BaseHuman h = otherHumans[desc];
        Destroy(h.gameObject);
        otherHumans.Remove(desc);
    }
    void OnList(string msg)
    {
        Debug.Log("OnList:" + msg);
        string[] split = msg.Split(',');
        int count = (split.Length - 1) / 6;
        for (int i = 0; i < count; i++)
        {
            string desc = split[i*6 + 0];
            float x = float.Parse(split[i * 6 + 1]);
            float y = float.Parse(split[i * 6 + 2]);
            float z = float.Parse(split[i * 6 + 3]);
            float eulY = float.Parse(split[i * 6 + 4]);
            int hp = int.Parse(split[i * 6 + 5]);
            if (desc == NetManager.GetDesc())
            {
                continue;
            }
            GameObject obj = Instantiate(humanPrefab);
            obj.transform.position = new Vector3(x, y, z);
            obj.transform.eulerAngles = new Vector3(0, eulY, 0);
            BaseHuman h = obj.AddComponent<SyncHuman>();
            h.desc = desc;
            otherHumans.Add(desc, h);
        }
        

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
