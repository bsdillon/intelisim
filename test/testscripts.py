import random
import math

def test_error_messages(networkconnection):
    #should fail
    networkconnection.add_control({"name": "No type given", "min": 0, "max": 100, "initial": 50})
    networkconnection.add_control({"type": "Range", "name": "slider_missing_title", "min": 0, "max": 100, "initial": 50})
    networkconnection.add_control({"type": "Range", "title": "No Name Slider", "min": 0, "max": 100, "initial": 50})
    networkconnection.add_control({"type": "Range", "name": "slider_min_gt_max", "title": "Bad Range", "min": 100, "max": 0, "initial": 50})
    networkconnection.add_control({"type": "Range", "name": "slider_initial_lt_min", "title": "Too Low", "min": 10, "max": 100, "initial": 5})
    networkconnection.add_control({"type": "Range", "name": "slider_initial_gt_max", "title": "Too High", "min": 0, "max": 50, "initial": 80})
    #should pass
    networkconnection.add_control({"type": "Range", "name": "slider_valid", "title": "Good Slider", "min": 0, "max": 100, "initial": 50})

def test_renderer(networkconnection):
    networkconnection.test_gui()

def test_widgets(networkconnection):
    #setup the widgets
    networkconnection.add_control({"type": "Range", "name": "range1", "title": "Sliding", "min": 0, "max": 100, "initial": 50})
    networkconnection.add_datapoint({"type":"Threshold","name":"threshold1","title":"Temperature","min":1,"max":5,"colorScale":{"1":"red","2":"orange","3":"yellow","4":"green","5":"blue"}})
    networkconnection.add_datapoint({"type":"ScatterPlot","name":"scatter1","independentVar":"x-axis","dependentVar":"y-axis"})
    networkconnection.add_datapoint({"type":"MultiLinePlot","name":"multiline1","title":"TrigPlots", "dependentVars":["Sin(x)","Cos(x)","Tan(x)"],"maxMemory":500})

    #demonstrate adding data to only one or other of the widgets
    networkconnection.update_datapoints({"threshold1":{"Temperature":3},"scatter1":{"x":2, "y":4}, "multiline1":{"Sin(x)":0,"Cos(x)":1,"Tan(x)":0}})
    networkconnection.update_datapoints({"scatter1":{"x":3, "y":5}})
    networkconnection.update_datapoints({"threshold1":{"Temperature":2}})

    #show a range of values on all widgets
    for n in range(40):
        #will show one period of each of the trig functions
        theta = n*math.pi/20

        #will create a normal distribution around +2 with a standard deviation of +/-1
        mu = 2        # center
        sigma = 1         # standard deviation
        value = random.gauss(mu, sigma)

        if(n in [10,30]):
            # Use 'None' for trig function discontinuities
            networkconnection.update_datapoints({"threshold1":{"Temperature":n%5},"scatter1":{"x":n%5, "y":(n%5)+value}, "multiline1":{"Sin(x)":math.sin(theta),"Cos(x)":math.cos(theta),"Tan(x)":None}})
        else:
            networkconnection.update_datapoints({"threshold1":{"Temperature":n%5},"scatter1":{"x":n%5, "y":(n%5)+value}, "multiline1":{"Sin(x)":math.sin(theta),"Cos(x)":math.cos(theta),"Tan(x)":math.tan(theta)}})
