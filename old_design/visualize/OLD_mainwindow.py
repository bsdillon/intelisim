import tkinter as tk
from tkinter import ttk, PhotoImage
from pydispatch import dispatcher
from data.datawidgets import Threshold, ScatterPlot, MultiLinePlot
from data.controlwidgets import Range
from data.signals import Signals
from data.simparams import SimParams
from visualize.player import Player
from visualize.renderer import Renderer
from enum import Enum
import os

class MainWindow:
    MAX_SPEED = 20
    MAX_SKIP = 10

    class _Internal_States(Enum):
        STOPPED = 0
        PLAYING = 1
        BLOCKED = 2
    
    def __init__(self, grid_size, step_function, restart_function=None):
        '''
        Creates a single window to display the whole simulation.
        '''
        self.root = tk.Tk()
        self.root.protocol("WM_DELETE_WINDOW", self._on_closing)
        self.root.title("Simulation Controller")
        self.root.geometry("1000x700")
        self.state = None

        self.simparams = SimParams()
        player = Player(step_function)

        # ----- Menu Bar -----
        menubar = tk.Menu(self.root)
        sim_menu = tk.Menu(menubar, tearoff=0)
        sim_menu.add_command(label="Restart", command=self._on_restart)
        if restart_function:
            self.register(Signals.SIM_RESTART,restart_function)
        menubar.add_cascade(label="Simulation", menu=sim_menu)
        self.root.config(menu=menubar)

        # ----- Layout Frames -----
        main_frame = tk.PanedWindow(self.root, orient=tk.HORIZONTAL, sashrelief=tk.RIDGE, sashwidth=10, bg="#C0C0C0")
        main_frame.pack(fill=tk.BOTH, expand=True)

        left_panel = tk.Frame(main_frame, width=250)
        main_frame.add(left_panel, minsize=100)
        left_panel.pack_propagate(False)

        right_panel = tk.Frame(main_frame, bg="white")
        main_frame.add(right_panel)

        # ----- Permanent Controls (top section) -----
        perm_controls = tk.Frame(left_panel)
        perm_controls.pack(fill=tk.X, padx=5, pady=5)

        # Button row - uses an extensive set of images for buttons
        button_frame = tk.Frame(perm_controls)
        button_frame.pack(fill=tk.X, expand=True, pady=(0, 5))

        #TODO there should be other function relationships between player and mainwindow buttons
        script_dir = os.path.dirname(__file__)
        self._start_image = PhotoImage(file=os.path.join(script_dir,"start.png")).subsample(2, 2)
        self._btn_restart = tk.Button(button_frame, image=self._start_image, command=self._on_start)
        self._step_back_image = PhotoImage(file=os.path.join(script_dir,"step_back.png")).subsample(2, 2)
        self._btn_step_back = tk.Button(button_frame, image=self._step_back_image, command=self._on_step_back)
        self._play_image = PhotoImage(file=os.path.join(script_dir,"play.png")).subsample(2, 2)
        self._pause_image = PhotoImage(file=os.path.join(script_dir,"pause.png")).subsample(2, 2)
        self._btn_play_pause = tk.Button(button_frame, image=self._play_image, command=self._on_play_pause)
        self.register(Signals.PLAY_PAUSE, player.toggle_play)
        self._step_image = PhotoImage(file=os.path.join(script_dir,"step.png")).subsample(2, 2)
        self._btn_step_forward = tk.Button(button_frame, image=self._step_image, command=self._on_step_forward)
        self.register(Signals.STEP, player.step_forward)
        self._end_image = PhotoImage(file=os.path.join(script_dir,"end.png")).subsample(2, 2)
        self._btn_end = tk.Button(button_frame, image=self._end_image, command=self._on_end)
        self._restart_permanent_disabled = False
        self._step_back_permanent_disabled = False
        self._end_permanent_disabled = False
        
        # ensures centering of the buttons
        tk.Label(button_frame).pack(side=tk.LEFT, expand=True)
        for b in [self._btn_restart, self._btn_step_back, self._btn_play_pause, self._btn_step_forward, self._btn_end]:
            b.pack(side=tk.LEFT, padx=1, pady=1)
        tk.Label(button_frame).pack(side=tk.LEFT, expand=True)

        # ----- Range controls -----
        ##
        speed_range = Range(perm_controls,"Speed",1,MainWindow.MAX_SPEED,MainWindow.MAX_SPEED-4)
        speed_range.pack(fill=tk.X, expand=True)
        #We want the maximum speed to be 50 ms and our default to be 300 ms
        #Every longer delay is in relation to the maximum and default values.
        speed_range.register(lambda value: player.adjust_speed(.05*(MainWindow.MAX_SPEED+1-value)))

        ##
        self.skipped = 0
        self.skip_rate = 0
        skip_range = Range(perm_controls,"Skip frame",0,MainWindow.MAX_SKIP,0)
        skip_range.pack(fill=tk.X, expand=True)
        def new_skip_rate(value):
            self.skip_rate=int(value)
            player.set_use_delays(self.skip_rate==0)
        skip_range.register(new_skip_rate)

        # ----- Scrollable Controls (middle) -----
        control_frame = self._make_scrollable_section(left_panel, "Controls")
        self.control_container = control_frame["container"]

        # ----- Scrollable Data (bottom) -----
        data_frame = self._make_scrollable_section(left_panel, "Data")
        self.data_container = data_frame["container"]

        # ----- Drawable Panel (right) -----
        self.simcanvas = tk.Canvas(right_panel, bg="black")
        right_panel.bind("<Configure>", self._on_resize)
        self.simcanvas.pack(fill=tk.BOTH, expand=True)
        self.renderer = Renderer(self.simcanvas, grid_size, "black")
        self.register(Signals.RESIZE, self.renderer.calculate_block_size)

        self._assert_state(MainWindow._Internal_States.STOPPED)

    # ----- Helper to create scrollable section -----
    def _make_scrollable_section(self, parent, title):
        # Repeatedly used to create a scrollable set of widgets added at run time
        section = tk.Frame(parent)
        section.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

        label = tk.Label(section, text=title, font=("Arial", 10, "bold"))
        label.pack(anchor=tk.W)

        canvas = tk.Canvas(section, width=100, height=150, bg="#C0C0C0")
        scrollbar = ttk.Scrollbar(section, orient="vertical", command=canvas.yview)
        scrollable_frame = tk.Frame(canvas)

        scrollable_frame.bind(
            "<Configure>", lambda e: canvas.configure(scrollregion=canvas.bbox("all"))
        )
        canvas.create_window((0, 0), window=scrollable_frame, anchor="nw")
        canvas.configure(yscrollcommand=scrollbar.set)

        #ensures all widgets resize with the canvas itself
        def _on_canvas_configure(event):
            for comp in canvas.find_all():
                canvas.itemconfig(comp, width=event.width)
        canvas.bind("<Configure>", _on_canvas_configure)
        
        canvas.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)

        return {"frame": section, "container": scrollable_frame}

    def kick_off(self):
        self.root.after(100, lambda : self._on_resize(None))
        self.root.mainloop()

    def get_simparams(self):
        '''
        Get the simulation parameters connected to this window
        '''
        return self.simparams

    def get_renderer(self):
        '''
        Get the Renderer connected to this window
        '''
        return self.renderer

    def draw_this_frame(self):
        '''
        Returns true if agents should draw at this time, mainly based
        on a skip-frame concept to reduce graphics processing
        '''
        if self.skipped < self.skip_rate:
            self.skipped +=1
            return False
        else:
            self.skipped = 0
            return True

# ----- Verified on new GUI ----- #
    # ----- Allow specific simulation actions -----
    def real_time_only(self):
        '''
        Designates this as a real-time simulation.
        A real-time sim can only step forward and play. There is no option to 
        go back, or fast forward
        '''
        self.allow_go_start(False)
        self.allow_step_back(False)
        self.allow_end(False)

    def allow_go_start(self, allowed):
        '''
        Permanently en/disable the control to jump to simulation time=0
        '''
        self._restart_permanent_disabled = not allowed
        if not allowed:
            self._btn_restart.config(state=tk.DISABLED)

    def allow_step_back(self, allowed):
        '''
        Permanently en/disable the control to jump to step BACK in simulation time
        '''
        self._step_back_permanent_disabled = not allowed
        if not allowed:
            self._btn_step_back.config(state=tk.DISABLED)

    def allow_end(self, allowed):
        '''
        Permanently en/disable the control to jump to simulation time=end
        '''
        self._end_permanent_disabled = not allowed
        if not allowed:
            self._btn_end.config(state=tk.DISABLED)

    # ----- Control & Data Adders -----
    def add_control(self, details):
        '''
        details is a dictionary to define the control widget, which is built and returned
        Type - required to know which kind of widget is needed
        + Range {Title, Min, Max, Initial*}
        
        * optional
        '''
        widget = None
        match details["Type"]:
            case "Range":
                widget = Range(self.control_container,details["Title"],int(details["Min"]),int(details["Max"]),int(details["Initial"]))
            case _:
                raise AttributeError(f"Unknown control type {details['Type']}")
        widget.pack(in_=self.control_container, fill=tk.X, pady=2, expand=True)

        return widget

    def add_datapoint(self, details):
        '''
        details is a dictionary to define the datapoint widget, which is built and returned
        Type - required to know which kind of widget is needed
        + Threshold {Title, Min, Max, Scale}
        + ScatterPlot {Independent, Dependent}
        + MultiLinePlot {Title, Datasets:[str...], Memory*}

        * optional
        '''
        widget = None
        match details["Type"]:
            case "Threshold":
                widget = Threshold(self.data_container,details["Title"],float(details["Min"]),float(details["Max"]),details["Scale"])
            case "ScatterPlot":
                widget = ScatterPlot(self.data_container,details["Independent"],details["Dependent"])
            case "MultiLinePlot":
                memory=100
                if "Memory" in details:
                    memory = details["Memory"]
                widget = MultiLinePlot(self.data_container,details["Title"],details["Datasets"], 100)

            case _:
                raise AttributeError(f"Unknown data type {details['Type']}")
        widget.pack(in_=self.data_container, fill=tk.X, pady=2, expand=True)

        return widget

    # ----- Data Signal Registration -----
    def register(self, signal, function, arg_set=[]):
        '''
        Attach the designated function for this signal
        If parameters are required, they can be specified in the arg_set
        '''
        def handler(**kwargs):
            # Extract only the keys in `args` from kwargs
            selected = {key: kwargs.get(key) for key in arg_set}
            function(**selected)

        dispatcher.connect(handler, signal=signal, sender=dispatcher.Any, weak=False)

    # ----- Button Event Handlers -----
    def _on_restart(self):
        self._assert_state(MainWindow._Internal_States.BLOCKED)
        dispatcher.send(signal=Signals.SIM_RESTART, sender=self)
        self._assert_state(MainWindow._Internal_States.STOPPED)

    def _on_start(self):
        self._assert_state(MainWindow._Internal_States.BLOCKED)
        self._stop_player()
        dispatcher.send(signal=Signals.GO_START, sender=self)
        self._assert_state(MainWindow._Internal_States.STOPPED)

    def _on_step_back(self):
        self._assert_state(MainWindow._Internal_States.BLOCKED)
        self._stop_player()
        dispatcher.send(signal=Signals.STEP_BACK, sender=self)
        self._assert_state(MainWindow._Internal_States.STOPPED)

    def _on_play_pause(self):
        was_running = self.state == MainWindow._Internal_States.PLAYING 
        self._assert_state(MainWindow._Internal_States.BLOCKED)
        
        if was_running:
            self._btn_play_pause.config(image=self._play_image)
            self._assert_state(MainWindow._Internal_States.STOPPED)
        else:
            self._btn_play_pause.config(image=self._pause_image)
            self._assert_state(MainWindow._Internal_States.PLAYING)
        dispatcher.send(signal=Signals.PLAY_PAUSE, sender=self)

    def _on_step_forward(self):
        self._assert_state(MainWindow._Internal_States.BLOCKED)
        self._stop_player()
        dispatcher.send(signal=Signals.STEP, sender=self)
        self._assert_state(MainWindow._Internal_States.STOPPED)

    def _on_end(self):
        self._assert_state(MainWindow._Internal_States.BLOCKED)
        self._stop_player()
        dispatcher.send(signal=Signals.GO_END, sender=self)
        self._assert_state(MainWindow._Internal_States.STOPPED)
    
    # ----- State helpers -----
    def _on_resize(self, _):
        dispatcher.send(signal=Signals.RESIZE, sender=self, width=self.simcanvas.winfo_width(), height=self.simcanvas.winfo_height())

    def _enable_restart(self, enable):
        if self._restart_permanent_disabled and enable:
            self._btn_restart.config(state=tk.DISABLED)
        elif enable:
            self._btn_restart.config(state=tk.NORMAL)
        else:
            self._btn_restart.config(state=tk.DISABLED)            

    def _enable_step_back(self, enable):
        if self._restart_permanent_disabled and enable:
            self._btn_step_back.config(state=tk.DISABLED)
        elif enable:
            self._btn_step_back.config(state=tk.NORMAL)
        else:
            self._btn_step_back.config(state=tk.DISABLED)            

    def _enable_play_pause(self, enable):
        if enable:
            self._btn_play_pause.config(state=tk.NORMAL)
        else:
            self._btn_play_pause.config(state=tk.DISABLED)            
        
    def _enable_step(self, enable):
        if enable:
            self._btn_step_forward.config(state=tk.NORMAL)
        else:
            self._btn_step_forward.config(state=tk.DISABLED)            

    def _enable_end(self, enable):
        if self._restart_permanent_disabled and enable:
            self._btn_end.config(state=tk.DISABLED)
        elif enable:
            self._btn_end.config(state=tk.NORMAL)
        else:
            self._btn_end.config(state=tk.DISABLED)

    def _stop_player(self):
        if self.state == MainWindow._Internal_States.PLAYING:
            dispatcher.send(signal=Signals.PLAY_PAUSE, sender=self)        

    def _on_closing(self):
        self._stop_player()
        dispatcher.send(signal=Signals.WINDOW_CLOSING, sender=self)
        self.root.quit()
        self.root.destroy()

    def _assert_state(self, state):
        # updates the control state between all buttons
        match state:
            case MainWindow._Internal_States.PLAYING:
                self._enable_restart(False)
                self._enable_step_back(False)
                self._enable_play_pause(True)
                self._enable_step(False)
                self._enable_end(False)
            case MainWindow._Internal_States.STOPPED:
                self._enable_restart(True)
                self._enable_step_back(True)
                self._enable_play_pause(True)
                self._enable_step(True)
                self._enable_end(True)
            case MainWindow._Internal_States.BLOCKED:
                self._enable_restart(False)
                self._enable_step_back(False)
                self._enable_play_pause(False)
                self._enable_step(False)
                self._enable_end(False)
            case _:
                raise AttributeError(f"Unknown state {state.name}")
        self.state = state
