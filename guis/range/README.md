## Purpose

The purpose of this project is to provide demonstrations of different agent-based simulations with the aid of HTMX,
websockets, SSE - whatever gets the sims running, replayable, and recorded - FAST.

This is not meant to replace any existing Python or mesa code - merely to supplement my own learning process and
encourage
eventual headless simulation runs, per Brian Dillon's original design.

These GUIs are meant to provide a powerful insights into making our GUIs work for us and not we for our GUIs, using a "
less is more" approach, and keeping the focus on 'backend' simulations. .NET and C# are merely the rendering and
possible running vehicles, as of this writing.

## References

[Using these to make backend-happy GUI controls](https://khalidabuhakmeh.com/posts/dynamic-htmx-islands-with-aspnet-core/)
[Use this as an alternative to Websockets](https://khalidabuhakmeh.com/posts/server-sent-events-in-aspnet-core-and-dotnet-10/)

[Math expression evaluation](https://ncalc.gumbarros.com.br/articles/index.html)
[Optimizing Monte Carlo simulation of particle movement in 2D space with C++](https://stackoverflow.com/questions/74955564/optimizing-monte-carlo-simulation-of-particle-movement-in-2d-space-with-c)

- [Reducing particle pair checks](https://gameprogrammingpatterns.com/spatial-partition.html)
- [Useful?](https://www.flawofaverages.com/#)

### Math libraries:

- [Meta Numerics](https://github.com/dcwuser/metanumerics)
- [BigInteger](find)
- [Linear Algebra](https://numerics.mathdotnet.com/)
- [ILNumberics - NumPy- and MATLAB-style array programming](https://ilnumerics.net/#gsc.tab=0)

## Concepts

### Nash equilibrium

Nash equilibrium is a solution concept in game theory where no player can improve their expected outcome by unilaterally
changing their strategy, assuming all other players' strategies remain constant. Named after mathematician John Nash,
who was awarded the 1994 Nobel Prize in Economics for this work, it represents a stable state in non-cooperative games
where each participant’s choice is the optimal response to the others.

Key characteristics include:

1. Mutual Best Response: Each player is playing their "best response" given what they anticipate the others will do.
2. No Incentive to Deviate: Once this equilibrium is reached, no individual gains by switching actions alone; changing
   strategy would result in a worse or equal outcome.
3. Existence: Every finite game has at least one Nash equilibrium, which may involve pure strategies (specific actions)
   or
   mixed strategies (probabilistic choices).
4. Suboptimality Potential: In some scenarios, such as the Prisoner’s Dilemma, the Nash equilibrium may lead to a
   collectively suboptimal outcome where rational self-interest prevents cooperation.

To identify a Nash equilibrium, analysts typically create a payoff matrix and examine each cell to see if any player can
benefit by deviating from their current strategy while others stay fixed. If no player benefits from such a change, that
strategy combination constitutes a Nash equilibrium.

#### Guides

- https://www.youtube.com/watch?v=3EOlRF7EjKU
-

### Minimax

Von Neumann's Minimax Theorem (1928) states that in any finite two-player zero-sum
game, the minimax strategy (minimizing your worst-case loss) is exactly the Nash equilibrium. Both players receive the
same payoff — the value of the game — and no player can improve by deviating. In this setting, the minimax theorem is
effectively a special case of Nash's existence theorem.

In non-zero-sum games, they diverge. The maximin strategy (pick the action with the best worst-case outcome) does not
generally produce a Nash equilibrium. A player playing a maximin strategy is optimizing for security against the worst
opponent, while a Nash equilibrium player is optimizing given the actual strategies of others. For example, in the
Prisoner's Dilemma, the Nash equilibrium is mutual defection, but a maximin approach might lead to a different choice
depending on payoffs.

### Genetic Algorithms

### Projects

* [NeuroObstacle Course](https://github.com/argonautcode/neuro-obstacle-course)


Look into:
- P(t) = I - E + (B-D)*P(t-1)
- Three bodies problem
- sensitivity analysis
- slack variables

