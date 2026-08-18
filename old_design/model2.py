from mesa import Model
import dearpygui.dearpygui as dpg

print("create at import")
dpg.create_context()
print("    done")

class Table(Model):
    def __init__(self):
        super().__init__()
        self.WIDTH = 10
        self.HEIGHT = 1

        print("Creating in main")
        with dpg.window(tag="Primary Window"):
            print("step 1")
            dpg.add_text("Hello, world")
            print("step 2")
            dpg.add_button(label="Save")
            print("step 3")
            dpg.add_input_text(label="string", default_value="Quick brown fox")
            print("step 4")
            dpg.add_slider_float(label="float", default_value=0.273, max_value=1)            
        print("    done")

# model = Table()

print("create viewport")
dpg.create_viewport(title='Custom Title', width=600, height=600)
print("    done")

print("setup dearpygui")
dpg.setup_dearpygui()
print("    done")

print("show viewport")
dpg.show_viewport()
print("    done")

print("Identify primary window")
dpg.set_primary_window("Primary Window", True)
print("    done")

print("start dearpygui")
dpg.start_dearpygui()
print("    done")

print("destroy thread")
dpg.destroy_context()
print("    done")
