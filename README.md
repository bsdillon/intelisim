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

## ML

### Samples

1. Reverse Engineering, using RAG:

- [REA](https://github.com/morluto/rea)
- [REA-RAG](https://github.com/mytechnotalent/rea)

## CodeMechanic

1. Issues

#### CodeMechanic.RegularExpressions

- [ ] v9 itself still drops or misreads real C#:

  operator is only \+. -, ==, implicit, and explicit are out.
  <generic> allows one nested level. Map<T, Dictionary<string, List<U>>> does not fit.
  <params> is [^)]*. Foo(Bar(1)) splits at the inner ).
  The return alternation still has [\w\?]+(?:<[^>]+>)? above Dictionary<> and Task<>. It works on the torture file
  because those returns happened to fit a later alt after backtracking. A longer return will lose that race.
  Local functions stay out on purpose. abstract and interface members only match on the second arm, at line start.
  Attributes are not part of the match. partial, extern, and required are not modifiers.

  So the change, if you want one, is not another torture pass. Seed v9, then run the two-body check. Scoring and the
  labeled "right method / wrong method" set come after those bodies differ.

#### CodeMechanic.Neo4j

- [ ] ...