# main_window_dpg.py
import os
from enum import Enum
from pydispatch import dispatcher
import dearpygui.dearpygui as dpg

from data.signals import Signals
from data.simparams import SimParams
from visualize.player import Player
from visualize.renderer import Renderer
from data.datawidgets import Threshold, ScatterPlot, MultiLinePlot
from data.controlwidgets import Range

class MainWindow:
    MAX_SPEED = 20
    MAX_SKIP = 10

    class _Internal_States(Enum):
        STOPPED = 0
        PLAYING = 1
        BLOCKED = 2

    def __init__(self, grid_size, step_function, restart_function=None, title="Simulation Controller", width=1000, height=700):
        """
        Creates a single window to display the whole simulation.
        """
        self.simparams = SimParams()
        player = Player(step_function)

        # Layout: left control column + right drawing area
        # Use a single window that fills the viewport
        with dpg.window(tag="Prime Window", no_close=True, no_title_bar=True, pos=(0, 0), width=width, height=height):
            print("Another label 1.1")
            # create a horizontal layout: left column fixed, right stretch
            # We'll use two child windows
            left_width = 300
            with dpg.child_window(tag="left_panel", width=left_width, autosize_y=False, height=height, border=True):
                print("Another label 1.2")
                # Permanent controls area
                permanent_controls = dpg.last_item()
                dpg.add_text("Controls", bullet=False)
                # button row
                with dpg.group(horizontal=True):
                    dpg.add_spacer(width=10)
                    with dpg.texture_registry():
                        script_dir = os.path.dirname(__file__)
                        # lambdas were lambda s, a, u: self._on_play_pause()
                        self._btn_start = self._create_button(script_dir, "start.png", self._on_start)
                        self._btn_step_back = self._create_button(script_dir, "step_back.png", self._on_step_back)
                        print("Another label 2")

                        #remember the play/pause textures
                        self._btn_play_pause = self._create_button(script_dir, "play.png", self._on_play_pause)
                        self.play_texture = self.textures[-1]
                        image_path = os.path.join(script_dir, "pause.png")
                        width, height, channels, data = dpg.load_image(image_path)
                        self.textures.append(dpg.add_static_texture(width, height, data))
                        self.pause_texture = self.textures[-1]

                        self._btn_step_forward = self._create_button(script_dir, "step.png", self._on_step_forward)
                        self._btn_end = self._create_button(script_dir, "end.png", self._on_end)

                        self._create_button(script_dir, "step.png", self._on_step_forward)
                        self._create_button(script_dir, "end.png", self._on_end)
                        self._restart_permanent_disabled = False
                        self._step_back_permanent_disabled = False
                        self._end_permanent_disabled = False
                        print("Another label 3")

                    dpg.add_spacer(width=10)
                dpg.add_separator()

                # ----- Range controls -----
                ##
                speed_range = Range("Speed",1,MainWindow.MAX_SPEED,MainWindow.MAX_SPEED-4)
                speed_range.build(permanent_controls)
                #We want the maximum speed to be 50 ms and our default to be 300 ms
                #Every longer delay is in relation to the maximum and default values.
                speed_range.register(lambda value: player.adjust_speed(.05*(MainWindow.MAX_SPEED+1-value)))
                print("Another label 4")

                # Skip range
                self.skipped = 0
                self.skip_rate = 0
                skip_range = Range("Skip frame",0,MainWindow.MAX_SKIP,0)
                skip_range.build(permanent_controls)
                def new_skip_rate(value):
                    self.skip_rate = int(value)
                    player.set_use_delays(self.skip_rate == 0)
                skip_range.register(new_skip_rate)
                print("Another label 5")

                dpg.add_separator()

                # Scrollable "Controls" container (where add_control will append)
                dpg.add_text("Dynamic Controls", bullet=False)
                self.controls_container = dpg.child_window(width=-1, height=200, autosize_x=True, border=True)

                dpg.add_spacing(count=1)
                dpg.add_separator()
                dpg.add_text("Data Widgets", bullet=False)
                self.data_container = dpg.child_window(width=-1, height=250, autosize_x=True, border=True)
                print("Another label 6")

            # right panel: drawing area
            with dpg.child_window(tag="right_panel", width=-1, autosize_x=True, autosize_y=False, height=height, border=False):
                self.sim_drawlist = dpg.drawlist(width=-1, height=-1)

        # create renderer adapter
        self.renderer = Renderer("sim_drawlist", grid_size, (0, 0, 0, 255))
        self.register(Signals.RESIZE, self.renderer.calculate_block_size, arg_set=["width", "height"])

        # wire play toggle to player
        self.register(Signals.PLAY_PAUSE, player.toggle_play)
        print("Another label 7")

        # initially stopped
        self._assert_state(MainWindow._Internal_States.STOPPED)

        # bind viewport resize callback
        dpg.set_viewport_resize_callback(lambda sender, data: self._on_resize())

        print("Another label 8")
        # ----- Menu Bar -----
        with dpg.viewport_menu_bar():
            print("Another label 8.1")
            with dpg.menu(label="Simulation"):
                print("Another label 8.2")
                dpg.add_menu_item(label="Restart", callback=self._on_restart)
                if restart_function:
                    self.register(Signals.SIM_RESTART,restart_function)
        print("Another label 9")


    def _create_button(self, script_dir, img_file, callback):
        image_path = os.path.join(script_dir, img_file)
        width, height, channels, data = dpg.load_image(image_path)
        self.textures.append(dpg.add_static_texture(width, height, data))
        return dpg.add_image_button(texture_id=self.textures[-1], width=width//2, height=height//2, callback=callback)

    def kick_off(self):
        # perform an initial resize calc and start loop

        # dpg.set_frame_callback(1, self._on_resize)
        dpg.create_viewport(title='Custom Title', width=800, height=600)
        dpg.setup_dearpygui()
        dpg.show_viewport()
        dpg.set_primary_window("main_window", True)
        dpg.start_dearpygui()

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
            self.skipped += 1
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
            dpg.configure_item(self._btn_restart, enabled=False)

    def allow_step_back(self, allowed):
        '''
        Permanently en/disable the control to jump to step BACK in simulation time
        '''
        self._step_back_permanent_disabled = not allowed
        if not allowed:
            dpg.configure_item(self._btn_step_back, enabled=False)

    def allow_end(self, allowed):
        '''
        Permanently en/disable the control to jump to simulation time=end
        '''
        self._end_permanent_disabled = not allowed
        if not allowed:
            dpg.configure_item(self._btn_end, enabled=False)

    # ----- Control & Data Adders -----
    def add_control(self, details):
        """
        details is a dictionary to define the control widget, which is built and returned
        Type - required to know which kind of widget is needed
        + Range {Title, Min, Max, Initial*}
        
        * optional
        """
        widget = None
        widget_type = details.get("Type")
        match widget_type:
            case "Range":
                widget = Range(details["Title"],int(details["Min"]),int(details["Max"]),int(details["Initial"]))
            case _:
                raise AttributeError(f"Unknown control type {details['Type']}")

        widget.build(self.controls_container)
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
        widget_type = details["Type"]
        match widget_type:
            case "Threshold":
                widget = Threshold(details["Title"],float(details["Min"]),float(details["Max"]),details["Scale"])
            case "ScatterPlot":
                widget = ScatterPlot(details["Independent"],details["Dependent"])
            case "MultiLinePlot":
                memory=100
                if "Memory" in details:
                    memory = details["Memory"]
                widget = MultiLinePlot(details["Title"],details["Datasets"], memory)
            case _:
                raise AttributeError(f"Unknown data type {widget_type}")

        widget.build(self.data_container)
        return widget

    # Signal registration (keeps same semantics)
    def register(self, signal, function, arg_set=[]):
        '''
        Attach the designated function for this signal
        If parameters are required, they can be specified in the arg_set
        '''
        def handler(**kwargs):
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
            dpg.configure_item(self._btn_play_pause, texture_id=self.play_texture)
            self._assert_state(MainWindow._Internal_States.STOPPED)
        else:
            dpg.configure_item(self._btn_play_pause, texture_id=self.pause_texture)
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

    def _on_resize(self):
        # get current right panel/drawlist size and send RESIZE
        vp_w = dpg.get_viewport_width()
        vp_h = dpg.get_viewport_height()
        # determine left panel width (we set 300)
        left_w = dpg.get_item_width("left_panel")
        # fallback if not set
        if not left_w or left_w <= 0:
            left_w = 300
        # compute drawable area size
        width = max(100, vp_w - left_w - 10)
        height = max(100, vp_h - 10)
        dispatcher.send(signal=Signals.RESIZE, sender=self, width=width, height=height)

    def _stop_player(self):
        if self.state == MainWindow._Internal_States.PLAYING:
            dispatcher.send(signal=Signals.PLAY_PAUSE, sender=self)

    def _on_closing(self):
        self._stop_player()
        dispatcher.send(signal=Signals.WINDOW_CLOSING, sender=self)
        dpg.stop_dearpygui()
        dpg.destroy_context()
 
    # State helpers
    def _assert_state(self, state):
        # updates the control state between all buttons
        match state:
            case MainWindow._Internal_States.PLAYING:
                dpg.configure_item(self._btn_restart, enabled=False)
                dpg.configure_item(self._btn_step_back, enabled=False)
                dpg.configure_item(self._btn_play_pause, enabled=True)
                dpg.configure_item(self._btn_step_forward, enabled=False)
                dpg.configure_item(self._btn_end, enabled=False)
            case MainWindow._Internal_States.STOPPED:
                dpg.configure_item(self._btn_restart, enabled=True)
                dpg.configure_item(self._btn_step_back, enabled=True)
                dpg.configure_item(self._btn_play_pause, enabled=True)
                dpg.configure_item(self._btn_step_forward, enabled=True)
                dpg.configure_item(self._btn_end, enabled=True)
            case MainWindow._Internal_States.BLOCKED:
                dpg.configure_item(self._btn_restart, enabled=False)
                dpg.configure_item(self._btn_step_back, enabled=False)
                dpg.configure_item(self._btn_play_pause, enabled=False)
                dpg.configure_item(self._btn_step_forward, enabled=False)
                dpg.configure_item(self._btn_end, enabled=False)
            case _:
                raise AttributeError(f"Unknown state {state}")
        self.state = state