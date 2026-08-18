from abc import ABC, abstractmethod
from typing import final

from mesa import Model
from mesa.space import _PropertyGrid
from mesa.datacollection import DataCollector

from network.networkconnection import NetworkConnection
from network.simparams import SimParams

class ModelExtension(ABC, Model):
    def __init__(self, width: int, height: int, grid: _PropertyGrid, model_reporters={}, agent_reporters={}, agenttype_reporters={}, tables = {}):
        '''
        Create all default model components before implementing the Template pattern
        - create_data_collection
        - create_controls
        - setup_agents
        '''
        super().__init__()
        self.width = width
        self.height = height
        self.property_grid = grid
        self.datacollector = DataCollector(model_reporters=model_reporters, agent_reporters=agent_reporters, agenttype_reporters=agenttype_reporters, tables=tables)

        self.network = NetworkConnection()
        self.simparams = SimParams()
        self.network.register_step_functions(self.step, self.run_continually, self.halt)
        self.step_count = 0
        self.running = False
        self.data_runners = []

        #Template Pattern
        self.create_controls()
        self.create_data_collection()
        self.setup_agents()

    def create_controls(self):
        '''
        Optional ability to add any controls required for the simulation

        self.network.add_control({"name":<unique_ID>,"type":<See common.constants.CONTROL_TYPES>,"title":<Any>, ... others})
        self.network.register_control_handler(<unique_ID>,lambda value: <updates simulation based on new value>)

        Default version creates no simulation controls
        '''
        pass

    def create_data_collection(self):
        '''
        Optional ability to add any datapoints required for the simulation

        #The data_runner for <unique_ID> is a self-contained JSON of all terms passed
        self.network.add_datapoint({"name":<unique_ID>, "type":<See common.constants.DATAPOINT_TYPES>, ... others})
        self.data_runners.append(lambda : {<unique_ID>: {<term1>:<get-term1>, ... others}})

        Default version creates no datapoints
        '''
        pass

    @abstractmethod
    def setup_agents(self):
        '''
        Required ability to create all agents based on specific simulation requirements.

        ex.
            some_agents = list(<ClassName>.create_agents(self, ... others))
            for sa in some_agents:
                self.property_grid.place_agent(sa, <coordinates>)
                #other agent configuration steps
        '''
        pass

    def preempt_step_function(self):
        '''
        Optional ability to prevent execution of a step function under some conditions
        Returns True if the main_process should be preempted.

        Default does NOT preempt the main_process.
        '''
        return False

    def main_process(self):
        '''
        Optional ability that replaces the concept of the step function in Mesa.Model.

        The default version just calls the step function on all agents in the model.
        '''
        self.agents.shuffle_do("step")

    # ------------------------------------------------- #
    # ----- Functions that should not be overriden ---- #
    # ------------------------------------------------- #
    @final
    def halt(self):
        '''
        Stops continually running the model
        '''
        self.running = False

    @final
    def run_continually(self):
        '''
        Runs the model as fast as possible until halted
        '''
        self.running = True

        while self.running:
            self.step()

    @final
    def step(self):
        # Required for Mesa.Model and this design
        # uses the Template pattern
        self.getDrawingFrame()

        if self.preempt_step_function():
            return

        self.collect_data()

        self.main_process()

        self.step_count += 1

    @final
    def getDrawingFrame(self):
        '''
        Required ability to send the drawing frame of the model
        '''
        frame = {"Step":self.step_count}
        frame_list = []
        for agent in list(self.agents):
            frame_list.append(agent.draw_frame())
        frame["Drawings"] = frame_list
        self.network.send_frame(self.step_count, frame)

    @final
    def restart_simulation(self):
        if len(self.agents)>0:
            old_agents = list(self.agents)
            for a in old_agents:
                a.remove()

    @final
    def collect_data(self):
        self.datacollector.collect(self)
        for runner in self.data_runners:
            runner()
