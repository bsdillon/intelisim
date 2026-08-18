# This is general Mesa capabilities
from mesa import Model
from mesa.datacollection import DataCollector
from mesa.space import SingleGrid

# This is specific to THIS simulation
from philosopher import Philosopher, Chopstick, State
import dearpygui.dearpygui as dpg

# This is the heart of the visualization
from visualize.mainwindow import MainWindow
from data.signals import Signals

class Table(Model):
    def __init__(self):
        super().__init__()
        self.WIDTH = 10
        self.HEIGHT = 1

        # ----- Setup Visualization -----
        print("thus far 0.1")
        self.mainwindow = MainWindow((self.WIDTH, self.HEIGHT), self.step)
        print("thus far 0.2")
        self.simparams = self.mainwindow.get_simparams()
        print("thus far 0.3")
        self.renderer = self.mainwindow.get_renderer()
        print("thus far 0.4")
        self.mainwindow.real_time_only()
        
        print("thus far")
        # ----- Data Collection -----
        ##
        def count_hungry(my_model):
            count = 0
            for a in my_model.agents:
                if type(a) is Philosopher and a.is_hungry():
                    count +=1
            return count

        ##
        self.datacollector = DataCollector(model_reporters={"Hungry": lambda m: count_hungry(m)},
                                           agent_reporters={"Location": lambda a: a.pos[0], "Icon": lambda a: a.get_icon()},
                                           agenttype_reporters={Philosopher: {"ID": "unique_id", "State": lambda a: a.state.name}},
                                           tables = {"History": ["Visual"]}
                                           )

        ##
        self.data_runners = []
        print("thus far 2")

        ##
        hunger_data = self.mainwindow.add_datapoint({"Type":"Threshold", "Title":"Hungry", "Min":0, "Max":5, 
                                                     "Scale":{1: (0, 255, 0, 255),
                                                            3: (172, 255, 0, 255),
                                                            4: (255, 172, 0, 255),
                                                            5: (255, 0, 0, 255)}})
        self.data_runners.append(hunger_data.get_update_runner(self.datacollector, self.simparams))

        ##
        hunger_plot = self.mainwindow.add_datapoint({"Type":"ScatterPlot","Independent":"hunger_rate","Dependent":"Hungry"})
        self.data_runners.append(hunger_plot.get_update_runner(self.datacollector, self.simparams, parameters=["x"]))
        self.mainwindow.register(Signals.WINDOW_CLOSING, hunger_plot.get_window_close_function())

        print("thus far 3")
        # ----- Controls -----
        ##
        self.simparams.add_parameter("hunger_rate",.95)
        hunger_control = self.mainwindow.add_control({"Type":"Range","Title":"Hunger Rate", "Min":1, "Max": 99, "Initial":95})
        hunger_control.register(lambda value: self.simparams.update_parameter("hunger_rate",(value/100.0)))

        # ----- Actual Simulation -----
        self.deadlocked = False
        self.grid = SingleGrid(self.WIDTH, self.HEIGHT,True)
        table_setup = [(0,0),(2,0),(4,0),(6,0),(8,0)]

        chpstx = list(Chopstick.create_agents(self, 5, self.renderer, self.simparams))
        philos = list(Philosopher.create_agents(self, 5, self.renderer, self.simparams))
        print("thus far 4")

        for i in range(len(chpstx)):
            self.grid.place_agent(chpstx[i],table_setup[i])
            self.grid.place_agent(philos[i],(table_setup[i][0]+1,table_setup[i][1]))

        for i in range(len(chpstx)):
            philos[i].link_chopstick(chpstx[i],True)
            philos[i].link_chopstick(chpstx[(i+1)%len(chpstx)],False)
        print("done")

    def step(self):
        # ----- skip frame and drawing -----
        if self.mainwindow.draw_this_frame():
            self.agents.do("draw_canvas")

        # ----- Possible early exit -----
        if self._short_circuit():
            return

        # ----- Data Collection steps -----
        self.datacollector.collect(self)
        for runner in self.data_runners:
            runner()

        # ----- Main process -----
        self._main_process()
    
    def _short_circuit(self):
        if self.deadlocked:
            self.renderer.text("DEADLOCK",(5,2),color="red",size=24)
            return True

        return False
    
    def _main_process(self):
        deadlock = True
        state = ""
        for agent,_ in self.grid.coord_iter():
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
    dpg.create_context()
    model = Table()
    model.mainwindow.kick_off()
    dpg.destroy_context()    