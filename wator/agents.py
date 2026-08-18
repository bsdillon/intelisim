from mesa import Agent

class Fish(Agent):
    def __init__(self, model, renderer, simparams):
        super().__init__(model)
        self.model = model
        self.renderer = renderer
        self.simparams = simparams
        self.figure = None

    def __str__(self):
        return f"Fish[{self.unique_id}]@{self.pos}"

    def draw_grid(self):
        self.renderer.control_grid(self.pos[0],self.pos[1],fill="green")

    def draw_canvas(self):
        if self.figure:
            return
        
        if self.figure == None:
            self.figure = self.renderer.rectangle((self.pos[0], self.pos[1]),
                                                  (self.pos[0]+1, self.pos[1]+1),
                                                    fill="green")

    def remove(self, remove_reference=True):
        self.model.grid.remove_agent(self)
        super().remove()
        if remove_reference and self.figure:
            self.renderer.remove(self.figure)
        else:
            return self.figure

    def step(self):
        #look for empty space        
        neighbors = self.model.grid.get_neighborhood(self.pos, moore=True, include_center=False)
        move_candidates = []
        for n in neighbors:
            if self.model.grid.is_cell_empty(n):
                move_candidates.append(n)

        if len(move_candidates)>0:
            #create child
            coord = move_candidates[self.random.randint(0,len(move_candidates)-1)]
            if self.random.randint(0,100)<int(self.simparams.get_parameter("fish_reproduce")):
                self.model.add(Fish, coord)
            else:
                self.model.grid.move_agent(self, coord)

class Shark(Agent):    
    def __init__(self, model, renderer, simparams):
        super().__init__(model)
        self.model = model
        self.renderer = renderer
        self.simparams = simparams
        self.figure = None
        self.starving = 0

    def __str__(self):
        return f"Shark[{self.unique_id}]@{self.pos} - {self.starving}"

    def draw_grid(self):
        self.renderer.control_grid(self.pos[0],self.pos[1],fill="red")

    def draw_canvas(self, reused_id=None):
        if reused_id==None and self.figure:
            #we already have a figure, no need to redraw
            return
        elif not reused_id == None:
            # reusable ID provided; keep the ID
            self.figure = reused_id

        if self.figure == None:
            self.figure = self.renderer.rectangle((self.pos[0], self.pos[1]),
                                                  (self.pos[0]+1, self.pos[1]+1),
                                                    fill="red")
        else:
            self.renderer.update(self.figure, (self.pos[0], self.pos[1]),
                                                  (self.pos[0]+1, self.pos[1]+1),
                                                    fill="red")

    def remove(self, remove_reference=True):
        self.model.grid.remove_agent(self)
        super().remove()
        if remove_reference and self.figure:
            self.renderer.remove(self.figure)
        else:
            return self.figure

    def step(self):
        #eat any fish
        others = self.model.grid.get_neighbors(self.pos, True)
        for other in others:
            if type(other) is Fish:
                self.starving = 0
                new_pos = other.pos
                
                #we want to keep the reference
                old_id = other.remove(remove_reference=False)
                
                if self.random.randint(0,100)<int(self.simparams.get_parameter("shark_reproduce")):
                    #create child - only if you eat
                    new_shark = self.model.add(Shark, new_pos)
                    new_shark.draw_canvas(reused_id=old_id)
                else:
                    self.renderer.remove(self.figure)
                    self.figure=old_id
                return
        
        # die
        self.starving += 1
        if self.starving>=int(self.simparams.get_parameter("starvation_time")):
            reference = self.remove()
            self.renderer.remove(reference)
            return

        #look for empty space        
        neighbors = self.model.grid.get_neighborhood(self.pos, moore=True, include_center=False)
        move_candidates = []
        for n in neighbors:
            if self.model.grid.is_cell_empty(n):
                move_candidates.append(n)

        if len(move_candidates)>0:
            self.model.grid.move_agent_to_one_of(self, move_candidates)
            self.moved = True
