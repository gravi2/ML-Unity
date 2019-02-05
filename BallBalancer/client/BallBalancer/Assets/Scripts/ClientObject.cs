using System.Collections.Concurrent;
using System.Threading;
using NetMQ;
using UnityEngine;
using NetMQ.Sockets;
using System;

public class NetMqListener
{
    private readonly Thread _listenerWorker;

    private bool _listenerCancelled;

    public delegate void MessageDelegate(string message);

    private readonly MessageDelegate _messageDelegate;

    private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        
        using (var reqSocket = new RequestSocket())
        {
            try
            {
                reqSocket.Options.ReceiveHighWatermark = 1000;
                reqSocket.Connect("tcp://localhost:5555");
                while (!_listenerCancelled)
                {
                    while (!_messageQueue.IsEmpty)
                    {
                        string message;
                        if (_messageQueue.TryDequeue(out message))
                        {
                            reqSocket.SendFrame(message);
                            string frameString;
                            //if (!reqSocket.TryReceiveFrameString(out frameString)) continue;
                            frameString = reqSocket.ReceiveFrameString();
                            Debug.Log(frameString);
                            //_messageQueue.Enqueue(frameString);
                            _messageDelegate(frameString);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                reqSocket.Close();
                NetMQConfig.Cleanup();
            }
        }
    }

    public void Update(string jsonFeatures)
    {
        _messageQueue.Enqueue(jsonFeatures);
    }

    public NetMqListener(MessageDelegate messageDelegate)
    {
        _messageDelegate = messageDelegate;
        _listenerWorker = new Thread(ListenerWork);
    }

    public void Start()
    {
        _listenerCancelled = false;
        _listenerWorker.Start();
    }

    public void Stop()
    {
        _listenerCancelled = true;
        _listenerWorker.Join();
    }
}