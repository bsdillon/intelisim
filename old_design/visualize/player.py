import threading
import time

class Player:
    '''
    Executes the automatic play/pause function for the simulation in conjuction with
    MainWindow. The worker function is run in a separate thread
    '''
    def __init__(self, function):
        def worker():
            #TODO Handle time events
            function()
            self.time += 1 # time increments AFTER execution
        
        self.worker = worker
        self.running = False
        self.thread = None
        self.delay = 0.25
        self.time = 0
        self.use_delays=True

    def get_sim_time(self):
        '''
        Returns the current sim time based on steps taken thus far
        '''
        return self.time

    def set_use_delays(self, use_them):
        '''
        By default delays are always "slept" through
        Under certain conditions we may turn them on/off
        '''
        self.use_delays = use_them

    def adjust_speed(self, value):
        '''
        Records the new delay between iterations of the worker thread
        '''
        self.delay = float(value)
    
    def step_forward(self):
        self.worker()
        
    def toggle_play(self):
        '''
        Alternates between play and pause functions
        '''
        if self.running:
            self.running = False
            if self.thread:
                self.thread.join(timeout=.5)
        else:
            self.running = True
            self.thread = threading.Thread(target=self._play, daemon=True)
            self.thread.start()

    def _play(self):
        '''
        Worker thread with average simulation time
        '''
        start = time.time()
        while self.running:
            self.worker()
            end = time.time()
            actual_delay = end-start
            # debugging
            # print(f"predicted delay {int(1000*self.delay)} ms")
            # print(f"actual delay {int(1000*actual_delay)} ms")

            delta = max(0,self.delay-actual_delay)
            if self.use_delays and delta>0:
                time.sleep(self.delay)

            start = time.time()
