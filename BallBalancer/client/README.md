# BallBalancer Game/App

## Unity3D
This folder contains the Unity3D project for a Game that tries to balance a ball on a freely moving 3D surface. The surface movement can be controlled using keyboard keys (Left, Right, Up and Down). 

## Communication (NetMQ)
The client communicates to the server using NetMQ (https://netmq.readthedocs.io/en/latest/), which is a C# implementation of ZeroMQ (http://zeromq.org) distributed messaging.  

The NetMQ library was added to Unity project using the NuGet package manager in Unity. 


## Running the client (Auto mode)
<b>Make sure you have already started the tf-server before you run the client</b>

1. Make sure you have Unity downloaded and installed on your machine.
2. Open the Unity project from the BallBalancer folder.
3. Make sure you don't see any errors.
4. Play/Run the Unity Project 
5. The checkedin version defaults to auto mode i.e it will communicate with the server and start balancing the ball on its own. 

## Creating Test data for Training 
1. In the SurfaceController.cs script, change the isAutoMode to false.
2. Save and run the client. 
3. In this mode, you can control the surface using the keyboard arrow keys (Left, Right, Up and Down)
4. All the training data is saved the training.csv file under the tf-server folder. 
5. Once you have enough training data, head over to the tf-server folder and follow the instructions to training the server using this newly generated data. 

## Note:
This project <B>DOES NOT</B> use the Unity ML agents. Instead I have tried to implement the ML parts in the server component and all the communication is handled via  zmq<->NetMQ. 