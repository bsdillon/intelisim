from mesa.datacollection import DataCollector
from mesa.space import SingleGrid

from network.model_extension import ModelExtension

from philosophers.philosopher import Philosopher, Chopstick, State

HUNGER_DATA = "hunger_data"
HUNGER_PLOT = "hunger_plot"
HUNGER_RATE = "hunger_rate"

def count_hungry(my_model):
    count = 0
    for a in my_model.agents:
        if type(a) is Philosopher and a.is_hungry():
            count +=1
    return count

class Table(ModelExtension):
    def __init__(self, width, height):
        super().__init__(width, height, SingleGrid(self.width, self.height,True),
                            model_reporters={HUNGER_DATA: lambda m: count_hungry(m)},
                            agent_reporters={"Location": lambda a: a.pos[0], 
                                             "Icon": lambda a: a.get_icon()},
                            agenttype_reporters={Philosopher: {"ID": "unique_id", "State": lambda a: a.state.name}},
                            tables = {"History": ["Visual"]})

        #other simulation members
        self.deadlocked = False

    def create_controls(self):
        self.simparams.add_parameter(HUNGER_RATE,.95)
        self.network.add_control({"name":"hunger_control","type":"Range","title":"Hunger Rate", "min":1, "max": 99, "initial":95})
        self.network.register_control_handler("hunger_control",lambda value: self.simparams.update_parameter(HUNGER_RATE,(value/100.0)))

    def create_data_collection(self):
        self.network.add_datapoint({"name":HUNGER_DATA, "type":"Threshold", "title":"Hungry", "min":0, "max":5, 
                                                     "colorScale":{1: (0, 255, 0, 255),
                                                            3: (172, 255, 0, 255),
                                                            4: (255, 172, 0, 255),
                                                            5: (255, 0, 0, 255)}})
        self.data_runners.append(lambda : {HUNGER_DATA: self.datacollector.get_model_vars_dataframe()[HUNGER_DATA].iloc[-1]})

        self.network.add_datapoint({"name":HUNGER_PLOT, "type":"ScatterPlot","independentVar":HUNGER_RATE,"dependentVar":"Hungry"})
        #TODO we need to create a lamda that will update hunmger_data
        self.data_runners.append(lambda : {HUNGER_PLOT: {"x":self.simparams.get_parameter(HUNGER_RATE), "y": self.datacollector.get_model_vars_dataframe()[HUNGER_DATA].iloc[-1]}})

    def setup_agents(self):
        table_setup = [(0,0),(2,0),(4,0),(6,0),(8,0)]

        chpstx = list(Chopstick.create_agents(self, 5, self.network, self.simparams))
        philos = list(Philosopher.create_agents(self, 5, self.network, self.simparams))

        for i in range(len(chpstx)):
            self.property_grid.place_agent(chpstx[i],table_setup[i])
            self.property_grid.place_agent(philos[i],(table_setup[i][0]+1,table_setup[i][1]))

        for i in range(len(chpstx)):
            philos[i].link_chopstick(chpstx[i],True)
            philos[i].link_chopstick(chpstx[(i+1)%len(chpstx)],False)
    
    def preempt_step_function(self):
        if self.deadlocked:
            self.renderer.text("DEADLOCK",(5,2),color="red",size=24)
            return True

        return False
    
    def main_process(self):
        deadlock = True
        state = ""
        for agent,_ in self.property_grid.coord_iter():
            if agent:
                state += f"{agent.get_icon()}"
                if type(agent) is Philosopher:
                    deadlock &= agent.state == State.GOT_LEFT

        if deadlock:
            state += "-- DEADLOCK"

        self.datacollector.add_table_row("History",{"Visual":state})

        if deadlock:
            self.deadlocked=True
            return

        self.agents.shuffle_do("step")

if __name__ == "__main__":
    model = Table(10,1)
    model.network.indefinite_listen()
