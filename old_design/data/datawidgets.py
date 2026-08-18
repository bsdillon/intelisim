import dearpygui.dearpygui as dpg
import numpy as np
import matplotlib.pyplot as plt
import warnings
from abc import ABC, abstractmethod
from matplotlib.animation import FuncAnimation
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg

import tkinter as tk
from tkinter import ttk

class DataPoint(ABC):
    def __init__(self, embedded=False):
        super().__init__()
        self._fig = None
        self._plot_window = None
        self._anim = None
        self._x_axis = None
        self._embedded = embedded

    def _create_plot(self):
        #not all child classes create a plot, so this version raises a defensive error
        raise NotImplementedError(f"{type(self).__name__} does not implement _create_plot")

    def _show_popup(self):
        #not all child classes create a popup, but this version allows for either implementation
        if self._embedded:
            # embed in a Tk Toplevel
            self._plot_window = dpg.add_window(label="Plot Popup", width=600, height=400, modal=False, autosize=False)

            self._create_plot()

            self._plot_window.protocol("WM_DELETE_WINDOW", self._close_plot)
        else:
            # use Matplotlib's own window
            self._create_plot()
            self._fig.canvas.mpl_connect("close_event", self._close_plot)
            manager = plt.get_current_fig_manager()
            self._plot_window = getattr(manager, "window", None)
            plt.show()

    def _no_op_close():
        #one option to "close" when there is nothing to do
        pass

    def _close_plot(self, event=None):
        """Stop animation and safely close figure and window."""
        if self._anim:
            self._anim.event_source.stop()

        if self._embedded and self._plot_window:
            dpg.delete_item(self._plot_window)

        if self._fig:
            plt.close(self._fig)

        self._anim = None
        self._plot_window = None
        self._fig = None

    def _external_close(self):
        """Allow outside event to close popup."""
        if self._embedded and self._plot_window:
            self._plot_window.destroy()
        elif self._fig:
            plt.close(self._fig)

        self._fig = None
        self._plot_window = None
        self._anim = None
        self._x_axis = None

    @abstractmethod
    def get_window_close_function(self):
        '''
        Returns the cleanup function this DataPoint should execute when the main window (and sim) closes
        '''
        pass

    @abstractmethod
    def get_update_runner(self, datacollector, simparams, parameters=[]):
        '''
        Returns a function that will update the data widget
        function = my_datapoint.get_update_runner
        
        ... later ...
        
        function(new_value) -- updates this GUI component
        '''
        pass

    def _get_latest_data(self, datacollector, data_name):
        """
        Return the latest value of `data_name` from any reporter in the datacollector.
        """
        # 1. Check model reporters
        try:
            df = datacollector.get_model_vars_dataframe()[data_name]
            if len(df)==0:
                return None
            return df.iloc[-1]
        except Exception as e:
            pass

        # 2. Check agent reporters
        try:
            df = datacollector.get_agent_vars_dataframe()[data_name]
            if len(df)==0:
                return None
            return df.iloc[-1]
        except Exception:
            pass

        # 3. Check agenttype reporters (if extension used)
        if hasattr(datacollector, "get_agenttype_vars_dataframe"):
            try:
                parts = data_name.split('::')
                if len(parts)==2:
                    #parts is AgentType:Data_Name
                    for agent_type in datacollector.agenttype_reporters.keys():
                        if parts[0] == agent_type.__name__:
                            df = datacollector.get_agenttype_vars_dataframe(agent_type)[parts[1]]
                            if len(df)==0:
                                return None
                            return df.iloc[-1]
            except Exception:
                pass

        # Nothing found
        raise KeyError(f"{data_name} not found in any datacollector reporter.")

class Threshold(DataPoint):
    '''
    Title, output value, and colored rectangle to show threshold values
    All other values in the range [min,max] are extrapolated
    '''
    def __init__(self, title, min_val, max_val, color_scale):
        '''
        
        '''
        super().__init__()
        self.data_name = title # used to retrieve data
        self.min_val = min_val
        self.max_val = max_val
        self.color_scale = dict(sorted(color_scale.items()))
        self.value_label_id = None
        self.drawlist_id = None
        self.bar_rect_id = None
    
    def build(self, parent):
        self.parent = parent
        with dpg.group(parent=parent):
            dpg.add_text(self.title, color=(0, 0, 136))  # bold blue
            self.value_label_id = dpg.add_text(f"{self.min_val:.2f}")

            self.drawlist_id = dpg.add_drawlist(width=300, height=20)
            self.bar_rect_id = dpg.draw_rectangle((0, 0), (0, 20), fill=(0, 255, 0, 255), parent=self.drawlist_id)

            # initial draw
            self._draw_slider(self.min_val)

    def get_window_close_function(self):
        return self._no_op_close

    def get_update_runner(self, datacollector, simparams, parameters=[]):
        '''
        Returns a function that will update the data widget
        function = my_threshold.get_update_runner
        
        ... later ...
        
        function(new_value) -- updates this GUI component
        '''
        if len(parameters)>0: #there is only ONE parameter possible
            def read_param():
                self._update_value(simparams.get_parameter(self.data_name))
            return read_param

        def read_data(): #the ONE value must be in datacollector
            data = self._get_latest_data(datacollector, self.data_name)
            #suppress None data returned b/c no data yet available.
            if data is not None:
                self._update_value(data)
        return read_data

    def _draw_slider(self, val):
        #check for value out of bounds
        if self.min_val > val or self.max_val < val:
            raise ArithmeticError(f"Initial value {val} is not in range [{self.min_val},{self.max_val}]")

        dpg.set_value(self.value_label_id, f"{val:.2f}")

        # Compute fill color
        selected_color = list(self.color_scale.values())[-1]
        for threshold, color in self.color_scale.items():
            #TODO create extrapolation based on the whole scale
            if val <= threshold:
                selected_color = color
                break

        # Compute bar width as fraction of parent width
        width, height = dpg.get_item_rect_size(self.parent)
        frac = (val - self.min_val) / (self.max_val - self.min_val)
        bar_width = int(frac * width)

        # Update the rectangle
        dpg.configure_item(self.bar_rect_id, pmax=(bar_width,20), fill=selected_color)

    def _update_value(self, val):
        #refresh the bar and text.
        dpg.set_value(self.value_label_id, f"{val:.2f}")
        self._draw_slider(val)
    
class ScatterPlot(DataPoint):
    '''
    Title, correlation slope, R^2 value, and button to pop up a matplotlib scatter plot
    '''
    def __init__(self, independent_var, dependent_var):
        super().__init__()

        self.x_name = independent_var
        self.y_name = dependent_var
        title = self.x_name + " vs. " + self.y_name
        self.r_squared = 0.0  # placeholder value
        self.m_slope = 0.0  # placeholder value

        ttk.Label(self, text=title, font=("TkDefaultFont", 10, "bold"), foreground="#000088").pack(anchor="w", pady=(0, 2))
        self.m_label = ttk.Label(self, text=f"M: NA")
        self.m_label.pack(side=tk.LEFT, pady=(0, 5))
        self.r_label = ttk.Label(self, text=f"R²: NA")
        self.r_label.pack(side=tk.LEFT, pady=(0, 5))

        # Button to pop up scatterplot window
        self.popup_button = ttk.Button(self, text="Show Plot", command=self._show_popup)
        self.popup_button.pack(side=tk.LEFT, pady=(0, 2))
        self.popup_button.config(state=tk.DISABLED)

        # Store data points (lists)
        self.x_data = []
        self.y_data = []

    def get_window_close_function(self):
        return self._external_close

    def get_update_runner(self, datacollector, simparams, parameters=[]):
        x_in_data = not "x" in parameters
        y_in_data = not "y" in parameters
        
        def readall():
            x = 0
            if x_in_data: #reading from data
                data = self._get_latest_data(datacollector, self.x_name)
                #suppress None data returned b/c no data yet available.
                if not data:
                    return

                x = data
            else: #reading from system parameters
                x = simparams.get_parameter(self.x_name)
                
            y = 0
            if y_in_data: #reading from data
                data = self._get_latest_data(datacollector, self.y_name)
                #suppress None data returned b/c no data yet available.
                if not data:
                    return
                y = data
            else: #reading from system parameters
                y = simparams.get_parameter(self.y_name)

            self._add_data_point(x,y)
        return readall

    def _add_data_point(self, x, y):
        # Helper to absorb new data
        self.x_data.append(x)
        self.y_data.append(y)
        if len(self.x_data):
            self._update_r_squared()
            self.popup_button.config(state=tk.NORMAL)
    
    def _update_r_squared(self):
        # Helper function to revise R^2 value using NumPy
        with warnings.catch_warnings():
            warnings.simplefilter("ignore", np.RankWarning)
            slope, intercept = np.polyfit(self.x_data, self.y_data, 1)
            y_fit = np.array(self.x_data) * slope + intercept
            ss_res = np.sum((np.array(self.y_data) - y_fit) ** 2)
            ss_tot = np.sum((np.array(self.y_data) - np.mean(self.y_data)) ** 2)
            self.r_squared = 1 - ss_res / ss_tot if ss_tot != 0 else 0.0
            self.m_slope = slope

            self.m_label.config(text=f"M: {self.m_slope:.3f}")
            self.r_label.config(text=f"R²: {self.r_squared:.2f}")

    def _create_plot(self):
        self._fig, self._x_axis = plt.subplots()                
        self._x_axis.scatter(self.x_data, self.y_data, label="Data")

        with warnings.catch_warnings():
            warnings.simplefilter("ignore", np.RankWarning)
            slope, intercept = np.polyfit(self.x_data, self.y_data, 1)
            self._x_axis.plot(self.x_data, np.array(self.x_data) * slope + intercept, color="red", label="Fit")
            self._x_axis.set_xlabel(self.x_name)
            self._x_axis.set_ylabel(self.y_name)
            self._x_axis.set_title(f"{self.x_name} vs {self.y_name}")

class MultiLinePlot(DataPoint):
    def __init__(self, title, dependent_vars, max_memory):
        super().__init__(embedded=True)

        self.max_memory = max_memory
        self.title = title
        self.datasets = {}
        for data_name in dependent_vars:
            self.datasets[data_name] = []

        self.timestamps = []

        ttk.Label(self, text=title, font=("TkDefaultFont", 10, "bold"), foreground="#000088").pack(anchor="w", pady=(0, 2))
        ttk.Button(self, text="Show Plot", command=self._show_popup).pack(side=tk.LEFT, pady=(0, 2))

        # Plot window
        self._lines = {}

    def get_window_close_function(self):
        return self._external_close
        
    def get_update_runner(self, datacollector, simparams, parameters=[]):
        def runner():
            temp_datasets = {}

            for data_name in self.datasets:
                temp_datasets[data_name]=[]
                if data_name in parameters:
                    temp_datasets[data_name].append(simparams.get_parameter(data_name))
                else:
                    data = self._get_latest_data(datacollector, data_name)
                    #suppress None data returned b/c no data yet available.
                    if not data:
                        return

                    temp_datasets[data_name].append(data)


            self.timestamps.append(len(self.timestamps))
            #save all temporary data permanently
            for data_name in self.datasets:
                self.datasets[data_name].append(temp_datasets[data_name][0])

        return runner

    def _create_plot(self):
        self._fig, self._x_axis = plt.subplots()
        self._x_axis.set_title(self.title)
        self._x_axis.set_xlabel("Timestep")
        self._x_axis.set_ylabel("Value")

        # Create line objects for each dataset
        for name in self.datasets.keys():
            (line,) = self._x_axis.plot([], [], label=name)
            self._lines[name] = line

        self._x_axis.legend(loc="upper left")

        # must save to get the benefit of the animation
        self._anim = FuncAnimation(self._fig, self._update_plot, interval=1000, save_count=self.max_memory)

        # Embed Matplotlib self.fig in popup window
        canvas = FigureCanvasTkAgg(self._fig, master=self._plot_window)
        canvas.get_tk_widget().pack(fill=tk.BOTH, expand=True)
        canvas.draw()

    def _update_plot(self, frame):
        #short circuit if window is closed
        if self._fig is None or self._x_axis is None or len(self._x_axis.lines) == 0:
            return
    
        """Redraw the live plot."""
        if not self.datasets:
            return

        for name, data in self.datasets.items():
            line = self._lines.get(name)
            if line:
                line.set_data(self.timestamps[:len(data)], data)

        self._x_axis.relim()
        self._x_axis.autoscale_view()

    # OLD MEMBERS
    def OLD_build(self):
        import tkinter as tk
        from tkinter import ttk
        self.value = tk.DoubleVar(value=self.min_val) # assumes minimum value

        ttk.Label(self, text=self.title, font=("TkDefaultFont", 10, "bold"), foreground="#000088").pack(side=tk.LEFT, anchor="w")
        self.value_label = ttk.Label(self, text=f"{self.value.get():.2f}")
        self.value_label.pack(side=tk.LEFT, padx=(5,5))

        # Canvas-based “slider” so we can color the track
        self.canvas = tk.Canvas(self, height=20, highlightthickness=0)
        self.canvas.pack(fill=tk.X, expand=True, pady=4)
        self.rect = self.canvas.create_rectangle(0, 0, 0, 20, fill="green", width=0)
        self.rect2 = self.canvas.create_rectangle(0, 0, 0, 10, fill="white", width=0)

        # ensure redraw if resized
        self.canvas.bind("<Configure>", lambda e: self._draw_slider(self.value.get()))

        self._draw_slider(self.value.get())

    def OLD_draw_slider(self, val):
        #check for value out of bounds
        if self.min_val > val or self.max_val < val:
            raise ArithmeticError(f"Initial value {val} is not in range [{self.min_val},{self.max_val}]")

        # Compute fill color
        selected_color = list(self.color_scale.values())[-1]
        for threshold, color in self.color_scale.items():
            #TODO create extrapolation based on the whole scale
            if val <= threshold:
                selected_color = color
                break

        # Compute fill length
        width = self.canvas.winfo_width() or 1
        frac = (val - self.min_val) / (self.max_val - self.min_val)
        fill_width = int(width * frac)
        backup_width = int(width * (1-frac))
        self.canvas.coords(self.rect, 0, 0, fill_width, 20)
        self.canvas.itemconfig(self.rect, fill=selected_color)
        self.canvas.coords(self.rect2, fill_width, 0, fill_width+backup_width, 20)

