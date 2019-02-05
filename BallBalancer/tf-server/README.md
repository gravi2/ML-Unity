# Server

## Tensorflow
This folder contains a server implemented using Tensorflow and Keras. The server uses a Neural Network to train and predict the control (Left, Right, Top , Down) keys, used for moving the surface that balances a moving ball.

## Communication
The server communicates to the client using ZeroMQ messaging server to reduce the communication latency between the client and server. The client is a Unity3D app that uses the predictions in real time during each frame. 

## Python Environment
The python environment is setup using PipEnv (https://pipenv.readthedocs.io/en/latest/), which allows managing the python virtual environments easily.

## Running the Server
1. Make sure you have PipEnv available on your machine
2. Clone this repository and navigate to the tf-server folder.
3. Running following command to install all the dependencies

   <code>pipenv install</code>

4. Once all the dependencies are installed, activate our virtual environment

   <code>pipenv shell</code>

5. Then inside the virtual environment, run the server using the following command:
    
    <code>python ball_balancer_server.py</code>
    
6. This will start the server and wait for the communication from the BallBalancer Unity Game. 
7. Now follow the instructions from the client folder to run the Unity3D project

   
## Retraining the server
1. First follow the instructions from the client readme to create the learning data i.e data/training.csv file.
2. Once you have the newly generated training.csv file, you can train the tf-server to generate a new model (saved in data/model.h5)
3. To generate the new model, open the ball_balancer_server.py file and comment the server.run_prediction_server() line and uncomment the server.learn() line. 
4. Save and run the following command to let server get retrained using the TensorFlow Neural Network.
    
    <code>python ball_balancer_server.py</code>
