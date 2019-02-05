import math
import random

import numpy as np
import pandas as pd
import tensorflow as tf
from sklearn.model_selection import train_test_split
from tensorflow import keras
from tensorflow.keras.callbacks import ModelCheckpoint
from tensorflow.keras.layers import (Activation, Conv2D, Dense, Dropout,
                                     Flatten, Input, Lambda, MaxPool2D)
from tensorflow.keras.models import Model, load_model
from tensorflow.keras.utils import Sequence

PROJECT_LOCATION = '.'


class BallBalancerServer(object):

    def __init__(self):
        # location of our data
        self.data_location = '%s/data/' % PROJECT_LOCATION
        self.model_location = '%s/data/model.h5' % PROJECT_LOCATION
    
    def load(self):
        print('Loading the saved model from %s' % self.model_location)
        self.model = load_model(self.model_location)

    def load_data(self):
        # load the data using the pandas
        print('loading data')
        column_names = ['ballPrevX', 'ballPrevY', 'ballPrevZ', 'surfacePrevX', 'surfacePrevY',
                        'surfacePrevZ', 'surfacePrevW', 'leftKey', 'rightKey', 'upKey', 'downKey']
        self.data_frame = pd.read_csv(
            self.data_location + 'training.csv', header=None, names=column_names)
        self.x = self.data_frame[['ballPrevX', 'ballPrevY', 'ballPrevZ',
                                  'surfacePrevX', 'surfacePrevY', 'surfacePrevZ', 'surfacePrevW']].values
        self.y = self.data_frame[['leftKey',
                                  'rightKey', 'upKey', 'downKey']].values
        self.train_x, self.test_x, self.train_y, self.test_y = train_test_split(
            self.x, self.y, test_size=0.2, random_state=0)
        print(self.train_x.shape)
        print(self.train_y.shape)

    def create_model(self):
        model = keras.Sequential([
            Dense(32, input_dim=self.train_x.shape[1], activation='relu'),
            Dense(128, activation='relu'),
            Dropout(0.5),
            Dense(256, activation='relu'),
            Dropout(0.5),
            Dense(64, activation='relu'),
            Dense(32, activation='relu'),
            Dense(self.test_y.shape[1], activation='sigmoid')])
        model.summary()
        model.compile(loss='binary_crossentropy',
                      optimizer='Adam', metrics=['accuracy'])
        self.model = model

    def train_model(self):
        checkpoint = ModelCheckpoint(self.model_location,
                                     monitor='loss',
                                     verbose=0,
                                     save_best_only=True,
                                     mode='auto')
        self.model.fit(self.train_x, self.train_y, epochs=20,
                       callbacks=[checkpoint], batch_size=10)
        print(self.model.evaluate(self.test_x,
                                  self.test_y, batch_size=10, verbose=1))

    def predict(self, input):
        preds = self.model.predict(input)
        preds[preds >= 0.5] = 1
        preds[preds < 0.5] = 0
        return preds

    def learn(self):
        self.load_data()
        self.create_model()
        self.train_model()

    def test_sanity(self):
        self.load()
        self.predict(pd.DataFrame([ 
            [-8.148539E-07,1.495561,-0.611804,-0.08735534,-1.676692E-11,3.691955E-07,0.9961772],
            [-0.1495571,1.27732,-0.7711414,0.04857238,-0.008549369,0.07306287,0.9961072]
        ]))



    def run_prediction_server(self):
        import time
        import zmq
        import sys
        import signal
        import traceback
        import json

        # def signal_term_handler(signal, fname):
        #     socket.close()
        #     sys.exit(0)

        context = zmq.Context()
        socket = context.socket(zmq.REP)
        #signal.signal(signal.SIGINT, signal_term_handler)


        socket.bind("tcp://*:5555")
        self.load()

        while True:
            try:
                #  Wait for next request from client
                message = socket.recv_json( flags = zmq.NOBLOCK)
                print("Received request: %s" % message)

                #  Do some 'work'
                #time.sleep(1)
                pred = self.predict(pd.DataFrame([message]))
                pred_json = json.dumps(['%s' % (x == 1) for x in pred[0].tolist()])
                #pred_json = json.dumps(pred[0].tolist())
                print (pred_json)
                socket.send_string(pred_json)
            except KeyboardInterrupt as ke:
                socket.close()
                context.term()
                sys.exit(0)
            except zmq.error.Again as expected:
                pass
            except Exception as e:
                print(e)
                traceback.print_tb(e.__traceback__)                                          # handle other, exceptions "un-handled" above
                sys.exit(1)

def main():
    server = BallBalancerServer()
    server.run_prediction_server()
    #server.learn()

if __name__ == "__main__":
    main()
 