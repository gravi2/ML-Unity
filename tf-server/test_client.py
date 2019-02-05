import zmq
import sys
import traceback
import random

context = zmq.Context()

#  Socket to talk to server
print("Connecting to hello world server…")
socket = context.socket(zmq.REQ)
socket.connect("tcp://localhost:5555")

test_msg = [ 
                [-8.148539E-07,1.495561,-0.611804,-0.08735534,-1.676692E-11,3.691955E-07,0.9961772],
                [-0.1495571,1.27732,-0.7711414,0.04857238,-0.008549369,0.07306287,0.9961072],
                [-1.155302,1.153959,-0.9104737,-0.01309931,-0.02241104,0.02048985,0.9994531],
                [-1.226738,1.825662,-1.00893,0.008193184,-0.02753256,-0.2002331,0.9793271],
                [-0.660715,1.380969,-1.035964,0.0001744045,-0.02864391,-0.1094343,0.9935812],
                [-0.4908707,1.277127,-1.034786,-0.003924141,-0.02895198,-0.05993516,0.9977747],
                [-0.2454373,1.149884,-1.040188,-0.01386471,-0.02944416,0.0803959,0.9962316],
                [-0.214322,1.159986,-1.051584,0.004065523,-0.02975653,0.1612345,0.9864591],
                [-0.3019785,1.089999,-1.063499,-0.005004947,-0.03353205,0.1990161,0.9794096]        
            ]

#  Do 10 requests, waiting each time for a response
for request in range(10):
    try:
        print("Sending request %s …" % request)
        socket.send_string( "%s" % random.choice(test_msg))

        #  Get the reply.
        message = socket.recv_json()
        print("Received reply %s [ %s ]" % (request, message))
    except KeyboardInterrupt as ke:
        socket.close()
        context.term()
        sys.exit(0)
    except zmq.error.Again as expected:
        pass
    except Exception as e:
        print(e)
        traceback.print_tb(tb=e.__traceback__)
        sys.exit(1)
