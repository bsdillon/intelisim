from mesa import Model
from mesa.datacollection import DataCollector
from mesa.space import MultiGrid

from agents import Rock, Bird

from visualize.mainwindow import MainWindow
from data.signals import Signals

class Swarm(Model):
    def __init__(self, width, height):
        super().__init__()
        self.width=width
        self.height=height

        self.mainwindow = MainWindow((width, height), self.step, self.setup)
        self.simparams = self.mainwindow.get_simparams()
        self.renderer = self.mainwindow.get_renderer()
        self.mainwindow.real_time_only()

        self.datacollector = DataCollector(model_reporters={},
                                           agent_reporters={},
                                           agenttype_reporters={},
                                           tables = {})
        self.data_runners = []
        self.grid = MultiGrid(width, height, True)
        
        self.setup()

    def setup(self):
        if len(self.agents)>0:
            old_agents = list(self.agents)
            for a in old_agents:
                a.remove()

        rocks = list(Rock.create_agents(self, 50, self.renderer, self.simparams))
        animals = list(Bird.create_agents(self, 150, self.renderer, self.simparams))

        for r in rocks:
            coord = (self.random.randint(0,self.width-1),self.random.randint(0,self.height-1))
            while len(self.grid.get_cell_list_contents(coord))>0:
                coord = (self.random.randint(0,self.width-1),self.random.randint(0,self.height-1))
            self.grid.place_agent(r, coord)

        for a in animals:
            coord = (self.random.randint(0,self.width-1),self.random.randint(0,self.height-1))
            while len(self.grid.get_cell_list_contents(coord))>0:
                coord = (self.random.randint(0,self.width-1),self.random.randint(0,self.height-1))
            self.grid.place_agent(a, coord)

    def gridsize(self, dx, dy):
        return (dx%self.width,dy%self.height)

    def contains_rock(self, location):
        for a in self.grid.get_cell_list_contents(location):
            if type(a) is Rock:
                return True

        return False
    
    def step(self):
        # ----- skip frame and drawing -----
        if self.mainwindow.draw_this_frame():
            self.agents.do("draw_canvas")

        self.datacollector.collect(self)
        for runner in self.data_runners:
            runner()

        self.agents.shuffle_do("step")        

if __name__ == "__main__":
    size = 100
    
    model = Swarm(size, size)
    model.mainwindow.kick_off()