from mesa.space import SingleGrid

from network.model_extension import ModelExtension

from agents import Fish, Shark

STARVATION_TIME = "starvation_time"
FISH_REPRODUCE = "fish_reproduce"
SHARK_REPRODUCE = "shark_reproduce"
FISH_POP = "Fish Population"
SHARK_POP = "Shark Population"

class Ocean(ModelExtension):
    def __init__(self, width, height):
        super().__init__(width, height, SingleGrid(width, height, True),
                         model_reporters={FISH_POP: lambda m: sum(isinstance(a, Fish) for a in m.agents),
                                          SHARK_POP: lambda m: sum(isinstance(a, Shark) for a in m.agents)})

    def create_controls(self):
        self.simparams.add_parameter(STARVATION_TIME, 5)
        self.network.add_control({"name":STARVATION_TIME,"type":"Range","title":"Starvation", "min":2, "max": 10, "initial":5})
        self.network.register_control_handler(STARVATION_TIME,lambda value: self.simparams.update_parameter(STARVATION_TIME,int(value)))

        self.simparams.add_parameter(FISH_REPRODUCE, 50)
        self.network.add_control({"name":FISH_REPRODUCE,"type":"Range","title":"Fish Reproduce", "min":1, "max": 100, "initial":50})
        self.network.register_control_handler(FISH_REPRODUCE,lambda value: self.simparams.update_parameter(FISH_REPRODUCE,int(value)))

        self.simparams.add_parameter(SHARK_REPRODUCE, 50)
        self.network.add_control({"name":SHARK_REPRODUCE,"type":"Range","title":"Shark Reproduce", "min":1, "max": 100, "initial":50})
        self.network.register_control_handler("fish_reproduce",lambda value: self.simparams.update_parameter(SHARK_REPRODUCE,int(value)))

    def create_data_collection(self):
        self.network.add_datapoint({"name":"populations", "type":"MultiLinePlot","title":"Populations","datasets":["Fish Population","Shark Population"]})
        self.data_runners.append(lambda : {FISH_POP: self.datacollector.get_model_vars_dataframe()[FISH_POP].iloc[-1],
                                           SHARK_POP: self.datacollector.get_model_vars_dataframe()[SHARK_POP].iloc[-1]})

    def setup_agents(self):
        animals = list(Fish.create_agents(self, self.random.randint(int(self.width/4),self.width), self.renderer, self.simparams))
        animals.extend(list(Shark.create_agents(self, self.random.randint(int(self.width/4),int(self.width/2)), self.renderer, self.simparams)))

        for a in animals:
            coord = (self.random.randint(0,self.width-1),self.random.randint(0,self.height-1))
            while len(self.property_grid.get_cell_list_contents(coord))>0:
                coord = (self.random.randint(0,self.width-1),self.random.randint(0,self.height-1))
            self.property_grid.place_agent(a, coord)

    def add(self, some_type, pos):
        new_animal = list(some_type.create_agents(self, 1, self.renderer, self.simparams))[0]
        self.property_grid.place_agent(new_animal, pos)
        return new_animal

if __name__ == "__main__":
    size = 100
    
    model = Ocean(size, size)
    model.mainwindow.kick_off()