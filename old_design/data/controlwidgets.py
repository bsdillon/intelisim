from pydispatch import dispatcher
from data.signals import Signals
import dearpygui.dearpygui as dpg

class Range():
    '''
    Title, output value, and slider part on a [min,max] range
    '''
    def __init__(self, title, min_val, max_val, initial):
        self.title = title
        self.min_val = min_val
        self.max_val = max_val

        if min_val > initial or max_val < initial:
            raise ArithmeticError(f"Initial value {initial} is not in range [{min_val},{max_val}]")

        #determine if the initial text should be represented as a float or int
        self.initial_val = int(initial)
        self.initial_txt = str(self.initial_val)

    def build(self, parent):
        with dpg.group(parent=parent):
            dpg.add_text(self.title, color=(0, 0, 136))  # title in bold color
            self.output_text_id = dpg.add_text(self.initial_txt)

            def callback(sender, app_data, user_data):
                self.value = int(app_data)
                dpg.set_value(self.output_text_id, str(self.value))
                dispatcher.send(signal=Signals.SET_VALUE, sender=self, value=self.value)
            
            dpg.add_slider_int(label="", min_value=self.min_val, max_value=self.max_val,
                               default_value=self.value, callback=callback)
   
    def register(self, function, signal=None):
        '''
        Connects the function to updates on the value
        If a signal name is given, it will be used, otherwise, Signals.SET_VALUE is assumed
        '''
        if signal:
            dispatcher.connect(function, signal=signal, sender=self, weak=False)
        else:
            dispatcher.connect(function, signal=Signals.SET_VALUE, sender=self, weak=False)

    def OLD_build(self, parent):
        import tkinter as tk
        from tkinter import ttk
        self.columnconfigure(2, weight=1)

        ttk.Label(self, text=self.title, font=("TkDefaultFont", 10, "bold"), foreground="#000088").grid(row=0, column=0, sticky="w")
        output_text = ttk.Label(self, text=self.initial_txt, width=4)
        output_text.grid(row=0, column=1, sticky="ew")

        self.value = tk.DoubleVar(value=self.initial)
        def update(val):
            #update converts discrete values into integers before updating the text and subscribers
            answer = int(float(val))
            answer_txt = str(answer)
            output_text.config(text=answer_txt)
            dispatcher.send(signal=Signals.SET_VALUE,sender=self, value=answer)

        ttk.Scale(self, from_=self.min_val, to=self.max_val, orient=tk.HORIZONTAL, variable=self.value, command=update).grid(row=0, column=2, sticky="ew")
