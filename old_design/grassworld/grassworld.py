from mesa import Model, Agent
from mesa.time import RandomActivation
from mesa.space import MultiGrid
from mesa.datacollection import DataCollector
from mesa.visualization.modules import CanvasGrid
from mesa.visualization.ModularVisualization import ModularServer


class GrassPatch(Agent):
    def __init__(self, unique_id, model, fully_grown=True):
        super().__init__(unique_id, model)
        self.fully_grown = fully_grown
        self.countdown = 0

    def step(self):
        if not self.fully_grown:
            self.countdown -= 1
            if self.countdown <= 0:
                self.fully_grown = True


class Grazer(Agent):
    def __init__(self, unique_id, model, energy=5):
        super().__init__(unique_id, model)
        self.energy = energy

    def step(self):
        # move randomly
        possible_steps = self.model.grid.get_neighborhood(
            self.pos, moore=True, include_center=False
        )
        new_position = self.random.choice(possible_steps)
        self.model.grid.move_agent(self, new_position)

        # eat grass if present
        cellmates = self.model.grid.get_cell_list_contents([self.pos])
        for obj in cellmates:
            if isinstance(obj, GrassPatch) and obj.fully_grown:
                self.energy += 4
                obj.fully_grown = False
                obj.countdown = self.model.grass_regrow_time
                break

        self.energy -= 1
        if self.energy <= 0:
            self.model.grid.remove_agent(self)
            self.model.schedule.remove(self)


class GrassWorld(Model):
    def __init__(self, width=20, height=20, N=10, grass_regrow_time=5):
        super().__init__()
        self.num_agents = N
        self.grid = MultiGrid(width, height, torus=True)
        self.schedule = RandomActivation(self)
        self.grass_regrow_time = grass_regrow_time

        # create grass
        for (x, y) in self.grid.coord_iter():
            patch = GrassPatch((x, y), self, fully_grown=True)
            self.grid.place_agent(patch, (x, y))
            self.schedule.add(patch)

        # create grazers
        for i in range(self.num_agents):
            a = Grazer(i, self)
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            self.grid.place_agent(a, (x, y))
            self.schedule.add(a)

    def step(self):
        self.schedule.step()


# visualization
def agent_portrayal(agent):
    if isinstance(agent, Grazer):
        return {"Shape": "circle", "Color": "blue", "r": 0.5, "Layer": 1}
    elif isinstance(agent, GrassPatch):
        color = "green" if agent.fully_grown else "brown"
        return {"Shape": "rect", "Color": color, "w": 1, "h": 1, "Layer": 0}


if __name__ == "__main__":
    grid = CanvasGrid(agent_portrayal, 20, 20, 400, 400)
    server = ModularServer(
        GrassWorld,
        [grid],
        "Grass World",
        {"width": 20, "height": 20, "N": 15, "grass_regrow_time": 5},
    )
    server.port = 8521
    server.launch()