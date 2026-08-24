## Architecture Notes

Intelisim consists of three primary concerns:

1. Simulation
2. Network communication
3. Web visualization

The simulation runs as a Python process and communicates with the
Flask GUI through the network layer.

The GUI is therefore a visualization/control surface, not the
simulation engine itself.

### Important distinction

A simulation step is not necessarily equivalent to a useful
visualization frame.

The simulation may execute thousands of steps while the GUI receives
or renders those states at a different rate.
