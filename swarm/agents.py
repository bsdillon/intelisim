from mesa import Agent

class Bird(Agent):
    def __init__(self, model, renderer, simparams):
        super().__init__(model)
        self.model = model
        self.renderer = renderer
        self.simparams = simparams
        self.figure = None
        self.moved = False
        self.vx = self.random.randint(-1,1)
        self.vy = self.random.randint(-1,1)

    def __str__(self):
        return f"Bird[{self.unique_id}]@{self.pos}"

    def draw_grid(self):
        self.renderer.control_grid(self.pos[0],self.pos[1],fill="blue")

    def draw_canvas(self):
        if self.figure and not self.moved:
            return
        
        if self.figure == None:
            self.figure = self.renderer.rectangle((self.pos[0]+.25, self.pos[1]+.25),
                                                  (self.pos[0]+.75, self.pos[1]+.75),
                                                    fill="red")
        else:
            self.renderer.update(self.figure,(self.pos[0]+.25, self.pos[1]+.25),
                                            (self.pos[0]+.75, self.pos[1]+.75),
                                            fill="red")
            self.moved = False

    def remove(self):
        self.model.grid.remove_agent(self)
        super().remove()
        self.renderer.remove(self.figure)

    def step(self):
        self.moved = False

        all_birds = self.model.agents.select(agent_type=Bird)
        dx = 0
        dy = 0
        for other in all_birds:
            if other.unique_id != self.unique_id:
                dx+=other.pos[0]-self.pos[0]
                dy+=other.pos[1]-self.pos[1]

        # print(f"Dpos ({dx},{dy})")
        # print(f"int value {int(2.0/(len(all_birds)-1))}")
        # print(f"frac value {2.0/(len(all_birds)-1)}")
        # print(f"denom value {len(all_birds)-1}")

        #average
        dx *= 2.0/(len(all_birds)-1)
        dy *= 2.0/(len(all_birds)-1)

        # print(f"Dpos_average ({dx},{dy})")
        self.vx += dx
        self.vy += dy

        #normalize
        max_value = max(abs(self.vx), abs(self.vy))
        self.vx *= 5.0/max_value
        self.vy *= 5.0/max_value

        # print(f"Dpos_norm ({dx},{dy})")

        new_position = self.model.gridsize(int(self.pos[0]+self.vx),int(self.pos[1]+self.vy))

        # print(f"old position ({self.pos[0]},{self.pos[1]})")
        # print(f"new position ({new_position[0]},{new_position[1]})")

        if self.model.contains_rock(new_position):
            neighbors = self.model.grid.get_neighborhood(self.pos, moore=True, include_center=False)
            move_candidates = []
            for n in neighbors:
                if not self.model.contains_rock(n):
                    move_candidates.append(n)

            if len(move_candidates)>0:
                self.model.grid.move_agent_to_one_of(self, move_candidates)
                self.moved = True
        else:
            self.model.grid.move_agent(self,new_position)
            self.moved = True

class Rock(Agent):
    def __init__(self, model, renderer, simparams):
        super().__init__(model)
        self.model = model
        self.renderer = renderer
        self.simparams = simparams
        self.figure = None

    def __str__(self):
        return f"Rock[{self.unique_id}]@{self.pos}"

    def draw_grid(self):
        self.renderer.control_grid(self.pos[0],self.pos[1],fill="gray")

    def draw_canvas(self):
        if self.figure:
            return
        
        if self.figure == None:
            self.figure = self.renderer.rectangle((self.pos[0], self.pos[1]),
                                                  (self.pos[0]+1, self.pos[1]+1),
                                                    fill="gray")

    def remove(self):
        self.model.grid.remove_agent(self)
        super().remove()
        self.renderer.remove(self.figure)

    def step(self):
        #rocks don't do anything
        pass