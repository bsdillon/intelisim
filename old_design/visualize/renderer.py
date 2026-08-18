import tkinter as tk
from tkinter import font

class Renderer:
    '''
    Managers all tkinter drawing processes in conjunction with mainwindow
    Accepts all drawing poisitions in continuous values relative to the simulation space
    '''
    def __init__(self, canvas, grid_dimensions, base_color):
        '''
            canvas - the tk.Canvas where all drawing will take place
            grid_dimensions - the dimensions of the simulation space
            base_color - the background color of the drawing space
        '''
        self.canvas = canvas
        self.grid = grid_dimensions
        self.base_color = base_color
        self._all_shapes = []
        self.block_size = 0

    def populate_all(self):
        for x in range(0,self.grid[0]-1):
            self._all_shapes.append([])
            for y in range(0,self.grid[1]-1):
                self._all_shapes[x].append(self.rectangle((x,y), (x+1,y+1), fill="white"))
    
    def control_grid(self, x, y, fill="white", outline="black"):
        reference = self._all_shapes[x-1][y-1]
        self.canvas.itemconfig(reference, fill=fill, outline=outline)

    def _test(self):
        # creates a test output to validate all functions. Should match render_test.png

        # for debugging purposes
        # from time import sleep
        # import threading
        # def test_runner():
        #     sleep(0.75)  # wait for window to initialize
        #     model.renderer._test()
        # threading.Thread(target=test_runner, daemon=True).start()
        
        self.rectangle((0,0), (1,1), fill="#FF0000")
        self.circle((1,0),(2,1),fill="red")
        self.circle((1.1,0.1),(1.9,0.9),fill="white")
        self.circle((1.2,0.2),(1.8,0.8),fill="red")
        self.circle((1.3,0.3),(1.7,0.7),fill="white")
        self.circle((1.4,0.4),(1.6,0.6),fill="red")
        self.pie((0,1),(1,2), 100, 340, outline="#00FF00")
        self.pie((1,1), (2,2), 10, 340, fill="#FFFF00")
        self.pie((2,1),(3,2), -30, 60, fill="#00FFFF")
        self.line((0,2), (1,3), "white")
        self.polygon([(2.5,0),(3,1),(2.5,.7),(2,1)], fill="#E9BF13", outline="#767812")
        self.text("Hello world!", (2,2.5), "#FFFFFF", angle=25, justify=tk.CENTER, size=16)
        
    def calculate_block_size(self, width, height):
        '''
        returns the size of square drawing space derived from the drawable and simulation dimensions
        '''
        self.block_size = min(self.canvas.winfo_width()/self.grid[0], self.canvas.winfo_height()/self.grid[1])

        if len(self._all_shapes)>0:
            for x in range(0,self.grid[0]-1):
                for y in range(0,self.grid[1]-1):
                    self.update(self._all_shapes[x][y],(x,y),(x+1,y+1),self.base_color)

    def remove(self, reference):
        self.canvas.delete(reference)

    def circle(self, upper_left, lower_right, fill="white", outline="black"):
        '''
        Draws a circle within the square defined by the upper left and lower right points in simulation space
        Default fill/outline is black around white
        '''
        block = self.block_size
        return self.canvas.create_oval((int(upper_left[0]*block), int(upper_left[1]*block)), 
                                (int(lower_right[0]*block), int(lower_right[1]*block)),
                                fill=fill, outline=outline)

    def update(self, reference, upper_left, lower_right, fill="white", outline="black"):
        '''
        Updates the location and coloring of an object bound by the two points. 
        
        Works on:
        * Circles
        * Rectangles
        '''
        block = self.block_size
        self.canvas.coords(reference, int(upper_left[0]*block), int(upper_left[1]*block), 
                                int(lower_right[0]*block), int(lower_right[1]*block))

        # update color or outline
        self.canvas.itemconfig(reference, fill=fill, outline=outline)

    def rectangle(self, upper_left, lower_right, fill="white", outline="black"):
        '''
        Draws a rectangle within the square defined by the upper left and lower right points in simulation space
        Default fill/outline is black around white
        '''
        block = self.block_size
        return self.canvas.create_rectangle((block*upper_left[0], block*upper_left[1]), 
                                     (block*lower_right[0], block*lower_right[1]), 
                                     fill=fill, outline=outline)

    def pie(self, upper_left, lower_right, angle_start, angle_length, fill="white", outline="black"):
        '''
        Draws a pie within the square defined by the upper left and lower right points in simulation space
        Default fill/outline is black around white
        Angles are in degrees
        '''
        block = self.block_size
        return self.canvas.create_arc((upper_left[0]*block, upper_left[1]*block),
                                (lower_right[0]*block, lower_right[1]*block),
                                start=angle_start, extent=angle_length,
                                fill=fill, outline=outline, width=1)

    def update_pie(self, reference, upper_left, lower_right, angle_start, angle_length, fill="white", outline="black"):
        '''
        Updates an existing pie (reference)
        TODO probably could be joined with update()
        '''
        block = self.block_size
        self.canvas.coords(reference, upper_left[0]*block, upper_left[1]*block, 
                                lower_right[0]*block, lower_right[1]*block)
        self.canvas.itemconfig(reference, start=angle_start, extent=angle_length, fill=fill, outline=outline)

    def polygon(self, points, fill="white", outline="black"):
        '''
        Draws a polygon defined by the set of points in simulation space
        Default fill/outline is black around white
        '''
        block = self.block_size
        point_set = [(int(p[0]*block), int(p[1]*block)) for p in points]
        return self.canvas.create_polygon(point_set, fill=fill, outline=outline)

    def update_polygon(self, reference, points, fill="white", outline="black"):
        '''
        Updates an existing polygon (reference)
        TODO probably could be joined with update()
        '''
        block = self.block_size
        point_set = [coord for p in points for coord in (int(p[0]*block), int(p[1]*block))]
        self.canvas.coords(reference, *point_set)
        self.canvas.itemconfig(reference, fill=fill, outline=outline)

    def line(self, point1, point2, color="black", width=1):
        '''
        Draws a line within the square defined by the two points in simulation space
        Default color is white
        '''
        block = self.block_size
        return self.canvas.create_line((point1[0]*block, point1[1]*block), 
                                (point2[0]*block, point2[1]*block), 
                                fill=color, width=width)

    def update_line(self, reference, point1, point2, color="black", width=1):
        '''
        Updates an existing line (reference)
        TODO probably could be joined with update()
        '''
        block = self.block_size
        self.canvas.coords(reference, point1[0]*block, point1[1]*block, 
                            point2[0]*block, point2[1]*block)
        self.canvas.itemconfig(reference, fill=color, width=width)

    def text(self, full_text, point, color="white", angle=0, justify=tk.LEFT, size=12):
        '''
        Draws text at the designated point
        Default text is 12pt, white, at 0 degrees, and left justified
        '''
        block = self.block_size
        new_font = font.Font(family="Arial",size=size)
        return self.canvas.create_text((point[0]*block, point[1]*block), 
                                text=full_text, angle=angle, fill=color, 
                                justify=justify, font=new_font)
    
    def update_text(self, reference, full_text, point, color="white", angle=0, justify=tk.LEFT, size=12):
        '''
        Updates an existing text (reference)
        TODO probably could be joined with update()
        '''
        block = self.block_size
        new_font = font.Font(family="Arial",size=size)
        self.canvas.coord(reference, (point[0]*block, point[1]*block))
        self.canvas.itemconfig(reference, text=full_text, angle=angle, fill=color, 
                                justify=justify, font=new_font)

#---------------------------------

# --- Renderer adapter (minimal) ---
class RendererAdapter:
    """
    Minimal adapter to stand in for your previous Renderer(tk.Canvas,...).
    Provide calculate_block_size(width, height) so existing code that sends RESIZE
    will work. Expand with drawing methods your sim expects.
    """
    def __init__(self, drawlist_tag, grid_size, bg="black"):
        self.drawlist_tag = drawlist_tag
        self.grid_size = grid_size
        self.bg = bg
        self.block_w = 0
        self.block_h = 0

    def calculate_block_size(self, **kwargs):
        # kwargs expected: width, height
        width = kwargs.get("width", 640)
        height = kwargs.get("height", 480)
        # reserve left panel area if caller doesn't subtract it
        # here we simply compute block size as floor division
        if self.grid_size and self.grid_size[0] > 0 and self.grid_size[1] > 0:
            self.block_w = max(1, width // self.grid_size[0])
            self.block_h = max(1, height // self.grid_size[1])
        else:
            self.block_w = width
            self.block_h = height
        # clear drawlist background (simple rectangle)
        try:
            # remove any previous background rect if present
            with dpg.mutex():  # harmless if not required, ensures thread safety
                # we'll just delete and recreate all children to simplify
                children = dpg.get_item_children(self.drawlist_tag, 1)
                if children:
                    for c in list(children):
                        dpg.delete_item(c)
                # draw background rect that covers the drawlist
                dpg.draw_rectangle((0, 0), (width, height), fill=self.bg, parent=self.drawlist_tag)
        except Exception:
            pass

    # Placeholder draw methods your simulation can call:
    def clear(self):
        try:
            children = dpg.get_item_children(self.drawlist_tag, 1)
            if children:
                for c in list(children):
                    dpg.delete_item(c)
        except Exception:
            pass

    def draw_agent_rect(self, x, y, w, h, color=(255, 255, 255, 255)):
        # color as RGBA 0-255
        dpg.draw_rectangle((x, y), (x + w, y + h), color=color, fill=color, parent=self.drawlist_tag)
