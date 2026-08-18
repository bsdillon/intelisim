from mesa import Agent
from enum import Enum
from network.simparams import SimParams
from common.constants import DEBUGGING

class State(Enum):
    FULL = 1
    HUNGRY = 2
    GOT_LEFT = 3
    READY_TO_EAT = 4
    DONE_EATING = 5
    DROPPED_LEFT = 6

def agent_portrayal(agent):
    return agent.draw()
    
class Philosopher(Agent):
    def __init__(self, model, network, simparams):
        super().__init__(model)
        self.left = None
        self.right = None
        self.state = State.FULL
        self.network = network
        self.simparams = simparams
        
    def __str__(self):
        return f"Philosopher_{self.unique_id}({self.pos})[{self.state}]"

    def is_hungry(self):
        return self.state not in [State.FULL, State.DONE_EATING, State.DROPPED_LEFT]

    def draw_frame(self):
        color = "green"
        if self.is_hungry():
            color = "blue"
        
        return {"type":"Circle","x1":self.pos[0],"y1":self.pos[1], "x2":self.pos[0]+1, "y2":self.pos[1]+1, "color":color}
    
    def get_icon(self):
        if self.is_hungry():
            return f"( )"
        return f"(*)"
    
    def change_state(self, from_state, to_state):
        if not self.state == from_state:
            raise AttributeError(f"{self} expected state mismatch {from_state}")
        self.state = to_state
    
    def link_chopstick(self, chopstick, on_left):
        if on_left:
            self.left = chopstick
            chopstick.link_philosopher(self, False)
        else:
            self.right = chopstick
            chopstick.link_philosopher(self, True)
    
    def pickup(self, on_left):
        if on_left and self.left.is_available():
            self.change_state(State.HUNGRY, State.GOT_LEFT)
            self.left.reserve(self)
        elif not on_left and self.right.is_available():
            self.change_state(State.GOT_LEFT, State.READY_TO_EAT)
            self.right.reserve(self)

    def drop(self, on_left):
        if on_left:
            self.change_state(State.DONE_EATING, State.DROPPED_LEFT)
            self.left.release(self)
        else:
            self.change_state(State.DROPPED_LEFT, State.FULL)
            self.right.release(self)
        
    def eat(self):
        if self.left.is_reserved_by(self) and self.right.is_reserved_by(self):
            self.change_state(State.READY_TO_EAT, State.DONE_EATING)
        else:
            raise AttributeError(f"{self} cannot eat with {self.left} and {self.right}")
    
    def step(self):
        prior = self.state
        match(self.state):
            case State.FULL:
                rate = self.simparams.get_parameter("hunger_rate")
                if self.random.random() > rate:
                    self.change_state(State.FULL, State.HUNGRY)
            case State.HUNGRY:
                self.pickup(True)
            case State.GOT_LEFT:
                self.pickup(False)
            case State.READY_TO_EAT:
                self.eat()
            case State.DONE_EATING:
                self.drop(True)
            case State.DROPPED_LEFT:
                self.drop(False)
            case _:
                raise AttributeError(f"{self} cannot act in unknown state")
        novel = self.state

        # way too verbose for debugging        
        # if DEBUGGING:
        #     print(f"{self} {prior}->{novel}")

class Chopstick(Agent):
    def __init__(self, model, network, simparams):
        super().__init__(model)
        self.is_reserved = False
        self.reservation = None
        self.network = network
        self.simparams = simparams

    def __str__(self):
        if self.is_reserved:
            return f"Chopstick_{self.unique_id}-{self.pos}[{self.reservation}]"
        return f"Chopstick_{self.unique_id}-{self.pos}[]"

    def draw_frame(self):
        top = (self.pos[0]+.5, self.pos[1])
        points = [self.pos[0], self.pos[0]+.5, self.pos[0]+1]

        bottom_x = points[1]
        if self.reservation == self.right:
            bottom_x = points[2]
        elif self.reservation == self.left:
            bottom_x = points[0]

        return {"type":"Line","x1":self.pos[0]+.5,"y1":self.pos[1], "x2":bottom_x, "y2":self.pos[1]+1, "color":"brown", "width":3}
    
    def get_icon(self):
        if self.reservation == None:
            return " | "
        elif self.reservation == self.left:
            return "_/ "
        else:
            return " \\_"

    def link_philosopher(self, philosopher, on_left):
        if on_left:
            self.left = philosopher
        else:
            self.right = philosopher

    def is_available(self):
        return not self.is_reserved

    def is_reserved_by(self, philosopher):
        if not self.is_reserved:
            raise AttributeError(f"{self} is not reserved by {philosopher}")
        return self.reservation == philosopher

    def reserve(self, philosopher):
        if self.is_reserved:
            raise AttributeError(f"{self} is already reserved")
        self.is_reserved = True
        self.reservation = philosopher
        
    def release(self, philosopher):
        if not self.is_reserved:
            raise AttributeError(f"{self} is not reserved")
        if not self.reservation ==  philosopher:
            raise AttributeError(f"{self} is not reserved by {philosopher}")
        self.reservation = None
        self.is_reserved = False