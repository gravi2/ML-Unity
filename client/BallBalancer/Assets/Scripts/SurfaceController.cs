using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SurfaceController : MonoBehaviour
{
    public Rigidbody ball;
    public int speed;
    public GameObject pointer;

    private TextWriter tw;
    private Vector3 leftTop, leftCenter, leftBottom, rightTop, rightCenter, rightBottom, top, bottom;
    private NetMqListener _netMqListener;
    private Boolean isAutoMode = true;
    private Vector3 forcePosition;


    [System.Serializable]
    public class MessageData
    {
        public string[] perdictions;
    }

        private void HandleMessage(string message)
    {
        string msg = "{\"perdictions\":" + message + "}";
        MessageData messageData = JsonUtility.FromJson<MessageData>(msg);
        if (messageData.perdictions.Length > 0)
        {
            string[] keys = messageData.perdictions;
            this.MoveSurface(Convert.ToBoolean(keys[0]), Convert.ToBoolean(keys[1]), Convert.ToBoolean(keys[2]), Convert.ToBoolean(keys[3]));
        }
    }

    private void InitAutoMode()
    {
        _netMqListener = new NetMqListener(HandleMessage);
        _netMqListener.Start();
    }

    private void InitTrainMode() {
        tw = new StreamWriter("..\\tf-server\\data\\training.csv", true);

    }

    private void Init() { 
        float width, depth;
        Vector3 position;

        forcePosition = pointer.transform.position;

        width = this.gameObject.GetComponent<Renderer>().bounds.size.x;
        depth = this.gameObject.GetComponent<Renderer>().bounds.size.z;
        position = this.gameObject.transform.position;

        leftTop = new Vector3(position.x - width / 2, position.y, position.z + depth / 2);
        leftCenter = new Vector3(position.x - width / 2, position.y, position.z);
        leftBottom = new Vector3(position.x - width / 2, position.y, position.z - depth / 2);

        rightTop = new Vector3(position.x + width / 2, position.y, position.z + depth / 2);
        rightCenter = new Vector3(position.x + width / 2, position.y, position.z);
        rightBottom = new Vector3(position.x + width / 2, position.y, position.z - depth / 2);

        top = new Vector3(position.x,  position.y, position.z + depth / 2);
        bottom = new Vector3(position.x, position.y, position.z - depth / 2);

        if (this.isAutoMode)
        {
            this.InitAutoMode();
        }
        else
        {
            this.InitTrainMode();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        this.Init();

    }


    private void Update()
    {

        Boolean leftKey , rightKey , upKey , downKey;
        //record ball position & surface angles before adjustment
        Vector3 ballBeforePos = ball.transform.position;
        Quaternion surfaceBeforeRot = transform.rotation;

        if (this.isAutoMode)
        {
            string features = "[" + ballBeforePos.x + "," + ballBeforePos.y + "," + ballBeforePos.z + "," +
                surfaceBeforeRot.x + "," + surfaceBeforeRot.y + "," + surfaceBeforeRot.z + "," + surfaceBeforeRot.w + "]";
            _netMqListener.Update(features);
        }
        else
        {
            leftKey = Input.GetKeyDown(KeyCode.LeftArrow);
            rightKey = Input.GetKeyDown(KeyCode.RightArrow);
            upKey = Input.GetKeyDown(KeyCode.UpArrow);
            downKey = Input.GetKeyDown(KeyCode.DownArrow);
            this.MoveSurface(leftKey, rightKey, upKey, downKey);

            if ( (!this.isAutoMode) && (forcePosition != pointer.transform.position) )
            {
                this.SaveToFile(ballBeforePos, surfaceBeforeRot, Convert.ToSingle(leftKey), Convert.ToSingle(rightKey), Convert.ToSingle(upKey), Convert.ToSingle(downKey));
            }
        }

        if (forcePosition != pointer.transform.position)
        {
            gameObject.GetComponent<Rigidbody>().AddForceAtPosition(new Vector3(0, -10, 0), forcePosition);
            pointer.transform.position = new Vector3(forcePosition.x, pointer.transform.position.y, forcePosition.z);
        }


    }

    private void MoveSurface(Boolean leftKey, Boolean rightKey, Boolean upKey, Boolean downKey)
    {

        if (leftKey && upKey)
        {
            forcePosition = leftTop;
        }
        else if (leftKey && downKey)
        {
            forcePosition = leftBottom;
        }
        else if (leftKey)
        {
            forcePosition = leftCenter;
        }
        else if (rightKey && upKey)
        {
            forcePosition = rightTop;
        }
        else if (rightKey && downKey)
        {
            forcePosition = rightBottom;
        }
        else if (rightKey)
        {
            forcePosition = rightCenter;
        }
        else if (upKey)
        {
            forcePosition = top;
        }
        else if (downKey)
        {
            forcePosition = bottom;
        }

    }

    private void SaveToFile(Vector3 ballBeforePos, Quaternion surfaceBeforeRot,float leftKey,float rightKey,float upKey, float downKey)
    {
        Debug.Log("Saving");
        //save to file
        tw.WriteLine(ballBeforePos.x + "," + ballBeforePos.y + "," + ballBeforePos.z + "," +
            surfaceBeforeRot.x + "," + surfaceBeforeRot.y + "," + surfaceBeforeRot.z + "," + surfaceBeforeRot.w + "," +
            leftKey + "," + rightKey + "," + upKey + "," + downKey);
    }


    private void CleanUp()
    {
        if (this.isAutoMode) {
            _netMqListener.Stop();
        }

        if (tw != null)
        {
            tw.Close();
        } 
    }

    void OnApplicationQuit()
    {
        this.CleanUp();
    }

    private void OnDestroy()
    {
        this.CleanUp();
    }

}