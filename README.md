# Intelisim

## Purpose

TBD

## Installation

TBD

## Architecture Notes

(The initial design of this project can be found in this [docx file](DevelopersConcept.docx).
Additional, supporting notes will be maintained within `/docs`)

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

## Known Experimental Questions

- How many simulation steps/sec can Intelisim execute?
- How many frames/sec can the GUI consume?
- Are all simulation steps persisted?
- Can a simulation be reproduced from its initial conditions?
- Where is the random seed recorded?
- Where are simulation parameters recorded?
- Can results be analyzed without the GUI?
- Can a simulation run headlessly?