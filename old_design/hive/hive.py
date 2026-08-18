# file: hive_mouths.py
from mesa import Model, Agent
# from mesa.time import RandomActivation
from mesa.space import MultiGrid


class HiveAgent(Agent):
    def __init__(self, unique_id, model):
        super().__init__(model)
        self.my_id = unique_id
        self.eating = False

    def step(self):
        # randomly start/stop eating
        self.eating = self.random.random() < 0.5


class HiveModel(Model):
    def __init__(self, width=10, height=10, N=15):
        super().__init__()
        self.grid = MultiGrid(width, height, torus=True)
        # self.schedule = RandomActivation(self)
        self.agents.shuffle_do("step")

        # create agents
        for i in range(N):
            a = HiveAgent(i, self)
            x = self.random.randrange(width)
            y = self.random.randrange(height)
            self.grid.place_agent(a, (x, y))
            # self.schedule.add(a)

    def step(self):
        self.agents.shuffle_do("step")

    def render(self):
        """Return a string representation of the grid."""
        chars = [[" ." for _ in range(self.grid.height)] for _ in range(self.grid.width)]
        for (contents, location) in self.grid.coord_iter():
            for obj in contents:
                if isinstance(obj, HiveAgent):
                    chars[location[0]][location[1]] = ";o" if obj.eating else ";)"
        return "\n".join("".join(row) for row in chars)


if __name__ == "__main__":
    model = HiveModel(width=10, height=10, N=15)
    for t in range(25):
        model.step()
        print(f"Time {t:02d}")
        print(model.render())
        print()