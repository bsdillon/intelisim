from mesa import Model, Agent
# from mesa.time import RandomActivation
from mesa.space import MultiGrid
from mesa.visualization import SolaraViz, make_plot_component, make_space_component
# from mesa.visualization.ModularServer import ModularServer
# from mesa.visualization.modules import CanvasGrid


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
        neighbors = self.model.grid.get_neighborhood(self.pos, moore=True, include_center=False)
        new_pos = self.random.choice(neighbors)
        self.model.grid.move_agent(self, new_pos)

        # eat grass if present
        cellmates = self.model.grid.get_cell_list_contents([self.pos])
        for obj in cellmates:
            if isinstance(obj, GrassPatch) and obj.fully_grown:
                self.energy += 4
                obj.fully_grown = False
                obj.countdown = self.model.grass_regrow_time
                break

        # metabolism and death
        self.energy -= 1
        if self.energy <= 0:
            try:
                self.model.schedule.remove(self)
                self.model.grid.remove_agent(self)
            except Exception:
                pass


class GrassWorld(Model):
    def __init__(self, width=20, height=20, N=10, grass_regrow_time=5):
        super().__init__()
        self.num_agents = N
        self.grid = MultiGrid(width, height, torus=True)
        # self.schedule = RandomActivation(self)
        self.agents.shuffle_do("step")
        self.grass_regrow_time = grass_regrow_time

        # create grass patches (unique ids avoid collisions)
        for x in range(self.grid.width):
            for y in range(self.grid.height):
                pid = f"patch-{x}-{y}"
                patch = GrassPatch(pid, self, fully_grown=True)
                self.grid.place_agent(patch, (x, y))
                self.schedule.add(patch)

        # create grazers
        for i in range(self.num_agents):
            gid = f"grazer-{i}"
            a = Grazer(gid, self)
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            self.grid.place_agent(a, (x, y))
            self.schedule.add(a)

    def step(self):
        self.schedule.step()


# visualization portrayal
def agent_portrayal(agent):
    if isinstance(agent, Grazer):
        return {
            "Shape": "circle",
            "r": 0.5,
            "Filled": "true",
            "Layer": 1,
            "Color": "blue",
        }
    if isinstance(agent, GrassPatch):
        color = "green" if agent.fully_grown else "saddlebrown"
        return {"Shape": "rect", "w": 1, "h": 1, "Layer": 0, "Color": color}
    return {}


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