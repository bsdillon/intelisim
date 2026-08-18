from network.networkconnection import NetworkConnection
import time
import random
import math
from test.testscripts import *

nc = NetworkConnection()

test_renderer(nc)

test_error_messages(nc)

test_widgets(nc)

nc.indefinite_listen(5)
# nc.add_control({"type": "Range", "name": "range1", "title": "Sliding", "min": 0, "max": 100, "initial": 50})
# nc.add_datapoint({"type":"Threshold","name":"threshold1","title":"Temperature","min":1,"max":5,"colorScale":{"1":"red","2":"orange","3":"yellow","4":"green","5":"blue"}})
# nc.add_datapoint({"type":"ScatterPlot","name":"scatter1","independentVar":"x-axis","dependentVar":"y-axis"})
# nc.add_datapoint({"type":"MultiLinePlot","name":"multiline1","title":"TrigPlots", "dependentVars":["Sin(x)","Cos(x)","Tan(x)"],"maxMemory":500})

# nc.update_datapoints({"threshold1":{"Temperature":3},"scatter1":{"x":2, "y":4}, "multiline1":{"Sin(x)":0,"Cos(x)":1,"Tan(x)":0}})
# nc.update_datapoints({"scatter1":{"x":3, "y":5}})
# nc.update_datapoints({"threshold1":{"Temperature":2}})

# for n in range(40):
#     theta = n*math.pi/20
#     mu = 2        # center
#     sigma = 1         # standard deviation
#     value = random.gauss(mu, sigma)

#     if(n in [10,30]):
#         nc.update_datapoints({"threshold1":{"Temperature":n%5},"scatter1":{"x":n%5, "y":(n%5)+value}, "multiline1":{"Sin(x)":math.sin(theta),"Cos(x)":math.cos(theta),"Tan(x)":None}})
#     else:
#         nc.update_datapoints({"threshold1":{"Temperature":n%5},"scatter1":{"x":n%5, "y":(n%5)+value}, "multiline1":{"Sin(x)":math.sin(theta),"Cos(x)":math.cos(theta),"Tan(x)":math.tan(theta)}})

