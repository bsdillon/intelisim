what is nash equilibrium in game theory

Quick Answer

**Nash equilibrium** is a solution concept in **game theory** where no player can improve their expected outcome by unilaterally changing their strategy, assuming all other players' strategies remain constant. Named after mathematician **John Nash**, who was awarded the **1994 Nobel Prize in Economics** for this work, it represents a stable state in non-cooperative games where each participant’s choice is the optimal response to the others.

Key characteristics include:

-   **Mutual Best Response**: Each player is playing their "best response" given what they anticipate the others will do.
    
-   **No Incentive to Deviate**: Once this equilibrium is reached, no individual gains by switching actions alone; changing strategy would result in a worse or equal outcome.
    
-   **Existence**: Every finite game has at least one Nash equilibrium, which may involve pure strategies (specific actions) or mixed strategies (probabilistic choices).
    
-   **Suboptimality Potential**: In some scenarios, such as the **Prisoner’s Dilemma**, the Nash equilibrium may lead to a collectively suboptimal outcome where rational self-interest prevents cooperation.
    

To identify a Nash equilibrium, analysts typically create a payoff matrix and examine each cell to see if any player can benefit by deviating from their current strategy while others stay fixed. If no player benefits from such a change, that strategy combination constitutes a Nash equilibrium.

How does this relate to minimax?

The relationship depends on the type of game:

**In zero-sum games, they coincide.** Von Neumann's **Minimax Theorem** (1928) states that in any finite two-player zero-sum game, the minimax strategy (minimizing your worst-case loss) is exactly the Nash equilibrium. Both players receive the same payoff — the _value of the game_ — and no player can improve by deviating. In this setting, the minimax theorem is effectively a special case of Nash's existence theorem.

**In non-zero-sum games, they diverge.** The maximin strategy (pick the action with the best worst-case outcome) does not generally produce a Nash equilibrium. A player playing a maximin strategy is optimizing for security against the worst opponent, while a Nash equilibrium player is optimizing given the _actual_ strategies of others. For example, in the **Prisoner's Dilemma**, the Nash equilibrium is mutual defection, but a maximin approach might lead to a different choice depending on payoffs.

<table><tbody><tr><td></td><td><span>Minimax</span></td><td><span>Nash Equilibrium</span></td></tr><tr><td><strong><span>Applies to</span></strong></td><td><span>Primarily zero-sum (2-player)</span></td><td><span>Any finite game (2+ players)</span></td></tr><tr><td><strong><span>Goal</span></strong></td><td><span>Minimize worst-case loss</span></td><td><span>No incentive to deviate unilaterally</span></td></tr><tr><td><strong><span>Computability</span></strong></td><td><span>Solvable via linear programming (constructive)</span></td><td><span>PPAD-complete (hard to compute)</span></td></tr><tr><td><strong><span>Guaranteed payoff</span></strong></td><td><span>Single "value of the game"</span></td><td><span>Multiple equilibria with different payoffs possible</span></td></tr></tbody></table>

In short: minimax is a _constructive_ method that gives you the Nash equilibrium in the zero-sum case, while Nash equilibrium is the broader _existence_ result that covers all finite games.

Oh, so minimax is like chess, while Nash equilibrium is like Yugioh with its different win conditions.

Chess = minimax is spot on — two players, one winner, zero-sum.

Yu-Gi-Oh is actually still **zero-sum** in a standard 1v1 duel: one player wins, the other loses, regardless of _how_ (life points hit zero, deck-out, etc.). The multiple win conditions don't change the payoff structure — it's still a strict win/loss game, so minimax still applies.

A better analogy for Nash equilibrium would be something like **two competing restaurants choosing prices** — both can "win" (make money), but each's best move depends on what the other does. Or a **Prisoner's Dilemma** where mutual cooperation would be better for both, but each has an incentive to defect.

The key distinction:

<table><tbody><tr><td></td><td><span>Zero-sum (minimax)</span></td><td><span>Non-zero-sum (Nash)</span></td></tr><tr><td><strong><span>Example</span></strong></td><td><span>Chess, Go, Yu-Gi-Oh 1v1</span></td><td><span>Price wars, Prisoner's Dilemma, coordination games</span></td></tr><tr><td><strong><span>Payoffs</span></strong></td><td><span>One's gain = other's loss</span></td><td><span>Both can gain or lose independently</span></td></tr><tr><td><strong><span>Strategy logic</span></strong></td><td><span>"Minimize my worst case"</span></td><td><span>"What's my best response to what </span><em><span>they're actually doing</span></em><span>?"</span></td></tr></tbody></table>

So the real dividing line isn't "different win conditions" — it's whether the players' payoffs are **opposites** (zero-sum) or **independent** (non-zero-sum).

Is Nash Equilibrium pretty easy to implement? I've done minimax in C# before.

Short answer: **no, it's not as straightforward as minimax**, unless your game is zero-sum (in which case you already have it).

**Why it's harder:**

Minimax is a single recursive tree walk — one deterministic algorithm, clean termination, polynomial time for fixed depth. Nash equilibrium in general-sum games is **PPAD-complete** (2-player) or **FIXP-complete** (3+ players), meaning no polynomial-time algorithm is known. You're not searching a tree; you're solving a fixed-point problem over a continuous strategy space.

**What you'd actually implement (2-player general-sum):**

The most common practical approach for small games is **support enumeration**:

1.  Enumerate all pairs of supports (subsets of strategies with nonzero probability) — `2^m × 2^n` combinations.
    
2.  For each support pair, set up a linear system: each strategy in the support must yield the same expected payoff, probabilities must be non-negative and sum to 1.
    
3.  Solve the LP / linear system.
    
4.  Check feasibility. If it passes, you've found an equilibrium.
    

For a 4×4 game that's 256 support pairs — totally fine. For a 20×20 game it's ~16 million — painful but still doable. Beyond that you need heuristics.

**Other approaches (less clean):**

<table><tbody><tr><td><span>Method</span></td><td><span>Pros</span></td><td><span>Cons</span></td></tr><tr><td><strong><span>Lemke-Howson</span></strong></td><td><span>Exact, always terminates</span></td><td><span>Exponential worst case, can cycle</span></td></tr><tr><td><strong><span>Fictitious play</span></strong></td><td><span>Simple to code (iterate best responses)</span></td><td><span>No convergence guarantee in general-sum games</span></td></tr><tr><td><strong><span>ε-approximation</span></strong><span> (gradient descent, etc.)</span></td><td><span>Scales to larger games</span></td><td><span>Only approximate, no clean stopping criterion</span></td></tr></tbody></table>

**In C# terms:**

Minimax was probably ~50 lines of recursion. A support-enumeration Nash solver for small 2-player games is maybe 150–300 lines (you need an LP solver or at least a linear system solver — `MathNet.Numerics` or a simplex implementation). The logic is more involved: you're solving feasibility constraints, not just picking max/min at each node.

**Bottom line:** For a small 2-player game (≤ ~10 strategies each), it's very doable as a weekend project. It's not "easy" in the way minimax is, but it's not research-level either. The moment you go to 3+ players or large strategy spaces, it gets genuinely hard.

Hm.... sounds fun. I think I'd like to try cap at 4x player game. I'm doing my own research for a job, and would like to impress my boss. Even a 3 player implementation would be impressive, I'd think. Good to hear it may only be 150-300 LOC. Very nice. I've looked at MathNet.Numerics and it doesn't seem terribly hard. Know any good example implementations of Nash in C#, with decent explanations? Happy to raw-dog it, if not.

Bad news: **there's no C# Nash equilibrium implementation** I can point you to. The ecosystem is Python (Nashpy), C++ (Gambit), and Julia (GameTheory.jl). You'll be the first to write one in C#, which is... actually kind of the point for impressing your boss.

**Best references to port from:**

<table><tbody><tr><td><span>Resource</span></td><td><span>Language</span></td><td><span>What it gives you</span></td></tr><tr><td><a href="https://github.com/drvinceknight/Nashpy" target="_blank" rel="noopener"><span>Nashpy</span></a></td><td><span>Python</span></td><td><span>Clean, well-commented support enumeration (~150 lines of core logic). The </span><code>support_enumeration.py</code><span> file is essentially the algorithm in one place.</span></td></tr><tr><td><a href="https://uio-ccse.github.io/algoritmisk-tenkning-humanister/04_game_theory/W1_Lecture.html" target="_blank" rel="noopener"><span>UIO Lecture</span></a></td><td><span>Python</span></td><td><span>A single self-contained </span><code>supportEnumeration()</code><span> function with inline comments explaining each step. Very readable.</span></td></tr><tr><td><span><span>Gambit</span></span></td><td><span>C++</span></td><td><span>Battle-tested, has n-player support via </span><code>enumnash</code><span>. Heavier to study but the gold standard.</span></td></tr></tbody></table>

**The 3-player problem:**

Support enumeration **does not extend to 3+ players**. For 3-4 player games, your realistic options are:

1.  **Fictitious play / GDA** (Govindan-Daskalakis-Even-Dar) — the simplest path. Each iteration: every player plays a best response to the _empirical frequency_ of others' past plays, then updates their strategy as a weighted average. ~50-80 lines. Converges generically (almost always in practice for small games) but has no worst-case guarantee. For a 4×4×4×4 game it'll converge in a few hundred iterations.
    
2.  **Ganzfried's QCP** (2020) — the state-of-the-art exact method for multiplayer. Formulates it as a quadratically-constrained feasibility problem and solves with a QP solver. Powerful but you'd need a QP solver (e.g., [CvxBase](https://github.com/cvxr/cvxbase) or port to a C# LP/QP library).
    

**My recommendation for your project:**

-   **Phase 1:** Port support enumeration from Nashpy to C# (2-player, exact, all equilibria). This is the "impressive but bounded" deliverable.
    
-   **Phase 2:** Add GDA/fictitious play for 3-4 players. It's _easier_ to code than support enumeration, and for small normal-form games it works reliably. Frame it as "converged to ε-Nash equilibrium" which is what all the serious multiplayer solvers report anyway.
    

That combination — exact 2-player + approximate 3-4 player — is a genuinely impressive portfolio piece, and the total code is probably 300-400 lines in C# with `MathNet.Numerics` for the linear algebra.

Ok, I red this article and examples. https://uio-ccse.github.io/algoritmisk-tenkning-humanister/04\_game\_theory/W1\_Lecture.html You're right, the code would be simple. Very little numpy code is used. Basic 2x2 matrices (structurally speaking, easy). How to use it? No clue. I read the paper, but don't understand much. They seem to skip from "these are matrices" mechanical explanations to using terms I don't understand and feel like I missed a chapter. (this is why I bloody hated college and skipped class) Can you explain 2x2 Nash like I'm 5? I know C# and basic python, arrays, lists, dictionaries, LINQ, etc. Mechanics are solid. I can even understand how they balanced their formulae and came up with the fraction. Awesome. But beyond that..... I'm lost as to their point.

No search needed — this is a "what does it _mean_" question, not a "what is it" question. Let me just talk through it.

___

## The one-sentence point

**"Given that both players are rational and can't talk to each other, what's the stable outcome where nobody wants to change their mind?"**

That's it. That's the whole point. You're finding the _predictable_ outcome of a strategic interaction.

___

## Concrete 2×2 example

Two companies, A and B, both deciding whether to **Advertise** or **Not Advertise**. Payoffs (A's profit, B's profit) in millions:

<table><tbody><tr><td></td><td><span>B: Advertise</span></td><td><span>B: Don't</span></td></tr><tr><td><strong><span>A: Advertise</span></strong></td><td><span>(2, 2)</span></td><td><span>(5, 0)</span></td></tr><tr><td><strong><span>A: Don't</span></strong></td><td><span>(0, 5)</span></td><td><span>(1, 1)</span></td></tr></tbody></table>

**Step 1: "Is there an obvious best move?"**

Look at A's row. If B advertises, A prefers to Advertise (2 > 0). If B doesn't, A _still_ prefers to Advertise (5 > 1). So A has a **dominant strategy**: always Advertise.

Same for B. Both have dominant strategies.

**Step 2: The equilibrium is (Advertise, Advertise) → payoff (2, 2).**

**Step 3: The "so what?"**

Here's the part that felt missing in the lecture. The _point_ is:

> "Even though both companies would be _better off_ if they both stayed quiet (1, 1 → wait, no, (1,1) is worse... let me redo this)."

Let me use a better example where the tension is obvious:

<table><tbody><tr><td></td><td><span>B: Cooperate</span></td><td><span>B: Defect</span></td></tr><tr><td><strong><span>A: Cooperate</span></strong></td><td><span>(3, 3)</span></td><td><span>(0, 5)</span></td></tr><tr><td><strong><span>A: Defect</span></strong></td><td><span>(5, 0)</span></td><td><span>(1, 1)</span></td></tr></tbody></table>

Now:

-   If B cooperates, A wants to defect (5 > 3).
    
-   If B defects, A _still_ wants to defect (1 > 0).
    
-   Same for B.
    

**Nash equilibrium: (Defect, Defect) → payoff (1, 1).**

**The "so what":** Both players _know_ that (Cooperate, Cooperate) gives them (3, 3) — better for both. But neither can unilaterally switch to Cooperate without getting screwed (you'd get 0 while they get 5). So they're _locked in_ at the worse outcome.

**That's the entire point of Nash equilibrium.** It tells you: _"Here's what will actually happen when rational, self-interested players can't make binding agreements."_ And it's often **worse for everyone than the cooperative outcome.**

___

## When you get a _mixed_ equilibrium (the fractions)

The 2×2 above has a _pure_ equilibrium — everyone just picks Defect. Boring.

Now consider **Matching Pennies**:

<table><tbody><tr><td></td><td><span>B: Heads</span></td><td><span>B: Tails</span></td></tr><tr><td><strong><span>A: Heads</span></strong></td><td><span>(+1, -1)</span></td><td><span>(-1, +1)</span></td></tr><tr><td><strong><span>A: Tails</span></strong></td><td><span>(-1, +1)</span></td><td><span>(+1, -1)</span></td></tr></tbody></table>

No dominant strategy. If A always plays Heads, B exploits by playing Tails. If B always plays Tails, A exploits by playing Heads. There's **no stable pure strategy**.

The only stable solution: **both flip a fair coin.** A plays Heads 50% of the time, B plays Heads 50% of the time. Neither can exploit the other.

**That's where your fractions come from.** You solved a linear system and got "A plays Top with probability 1/2." The _meaning_ is: "This is the randomization that makes the other player _indifferent_ — they can't tell you're going to play what, so they can't exploit you."

___

## How you'd actually USE it

You don't "use" a Nash equilibrium the way you use a sort algorithm. You use it as an **analysis tool**:

1.  **Model a real situation** as a game (who are the players, what are their choices, what are the payoffs).
    
2.  **Find the equilibrium** (your code does this).
    
3.  **Read the result:**
    
    -   Pure equilibrium → "Player A will do X, Player B will do Y. That's the prediction."
        
    -   Mixed equilibrium → "Player A will randomize with these probabilities. That's the prediction."
        
    -   Multiple equilibria → "Any of these could happen; you need more info to predict which."
        

**Practical examples:**

-   Auction design: "Will bidders shade their bids? By how much?" → model as a game, find the equilibrium bidding strategy.
    
-   Network routing: "How will traffic distribute across links?" → each user is a player choosing a route.
    
-   Security: "How should an attacker and defender split their resources?" → zero-sum, minimax = Nash.
    

___

## TL;DR for your boss

> "I built a solver that takes any small game (payoff matrices) and tells you the stable outcome — i.e., what rational players will _actually_ do, and what the expected payoffs will be. For 2-player games it's exact; for 3-4 players it converges to an approximate equilibrium."

The math you already did (solving the linear system, getting the fractions) _is_ the algorithm. The "point" is just: **the output is a prediction of behavior.** You're not computing a number for its own sake — you're answering "what will happen?"

Ok, I'm still stuck on "what does (2,2) mean - like, what are those numbers? Units? Ratios? weights?" and "where do these numbers come from?". Sorry, but you've lost me at the two companies example. Please explain only what those numbers mean. Then I'll backtrack and reread everything else you've written.

They're **dollars** (in my example). I literally said "payoffs in millions."

(2, 2) means: "Company A makes $2M, Company B makes $2M."

That's it. They're not ratios, weights, or abstract scores. I just picked arbitrary dollar amounts to make the example concrete. You could have used (10, 10) or (100, 100) — the _math_ doesn't care what the numbers are, only their **relative ordering** matters. What matters is:

-   2 > 0 → "Advertise is better than Don't Advertise _in this column_"
    
-   5 > 1 → "Advertise is better than Don't Advertise _in this other column_"
    

**Where do they come from in real life?** You _make them up_ (or estimate them) when you model a real situation. If you're modeling two companies, you'd look at revenue data and say "if both advertise, each makes $2M; if only one advertises, that one makes $5M and the other makes $0." The numbers encode your _assumptions_ about how the world works.

The solver doesn't care what units you use. It only cares about which number is bigger than which. So (2,2) and (200, 200) and (0.7, 0.7) are all the same _game_ as far as the algorithm is concerned.

"The solver doesn't care what units you use" You mean the solver that's written in python ,right?

No — I meant **any** solver. Your C# one, Nashpy, Gambit, all of them. It's not a property of a particular implementation; it's a property of the _math_.

The algorithm only ever does comparisons (`>` `<` `=`) and linear algebra on the numbers. It never asks "what are these units?" So whether you feed it (2, 2) in millions of dollars or (0.7, 0.7) in utility points or (200, 200) in arbitrary score units, the equilibrium it returns is the same _strategy_ (same probabilities, same "who does what"). The payoff numbers in the output just come back in whatever units you put in.

Oh, ok. If it's just basic compares and some linear algebra on simple 2x2 matrices, I think I can handle that in C#. I'll probably end up modeling each of the example matrices you provided here.

Yep, you'll be fine. The three examples from this conversation give you a nice progression:

1.  **Prisoner's Dilemma** — pure equilibrium, easy to verify by hand (both defect, nobody wants to switch).
    
2.  **Matching Pennies** — mixed equilibrium, the one where you actually exercise the linear system solver.
    
3.  **The advertising game** — also pure, but a different shape (dominant strategies on both sides).
    

One tip for validating: once your solver returns a strategy profile, **brute-force check it.** For each player, try every possible unilateral deviation and confirm their payoff doesn't go up. For 2×2 that's like 4 checks per player. If any deviation improves their payoff, your solver has a bug. That's a trivial unit test and it'll catch 90% of the edge cases (off-by-one in the linear system, wrong sign on a constraint, etc.).

Good luck impressing the boss.

Neat, will add that to the solver. Now, 3+ players is just a 3x3 matrix, right? It just takes more linear algebra / computationally intensive and possibly has more equilibrium instances to find, right? I'm trying to deflate any fears I have in figuring these things out. I tool linear algebra in college and it was an easy B. Wish I'd studied, but I'm sure I'll catch up, now that I understand the 2x2 and the scores ( the (2,2), (5,1), etc.). I'll watch the videos you linked, just to be sure. Maybe hand-write these little 2x2 grids, too. Oh, and where does that formula with the "a,b,c,..." variables come in? https://uio-ccse.github.io/algoritmisk-tenkning-humanister/04\_game\_theory/W1\_Lecture.html Definitely will write a solver in C#. That'll give me intimate understanding of how this all works.

**No, it's not a 3×3 matrix.** That's the one thing I have to correct before you go build it.

A 2-player game with 2 strategies each is a 2×2 matrix because you have two axes: "what does A do" and "what does B do." Add a third player and you need a _third axis_ — it becomes a 3D tensor (3×3×3 = 27 cells). Four players → 4D (3×3×3×3 = 81 cells). In C# you'd represent it as a multi-dimensional array or just a flat `double[]` with index math.

So yes, "more linear algebra, more computationally intensive" — but the _structural_ change is the bigger deal. Your 2×2 solver logic (solve one linear system, check feasibility) doesn't directly extend. That's why I recommended GDA/fictitious play for 3+ players: it sidesteps the combinatorial explosion by just iterating best responses.

**The "a, b, c, d" formula:**

For a 2×2 game:

```
        L       R
T    [ a, e ] [ b, f ]
B    [ c, g ] [ d, h ]<!---->
```

If the equilibrium is mixed (both players randomize), you solve: "What probability p does A play T with, that makes B _indifferent_ between L and R?"

B's expected payoff from L: `p·e + (1-p)·g`  
B's expected payoff from R: `p·f + (1-p)·h`

Set them equal, solve for p. That's it. One equation, one unknown. The "a, b, c, d" notation is just a way to label the cells so you can write that equation. You already did this in your head when you said "I can understand how they balanced their formulae and came up with the fraction." You _did_ the Nash equilibrium derivation. You just didn't have the label for it.

**Your fear deflation checklist:**

-   2×2 mixed: one linear equation. ✅ You get it.
    
-   2×n support enumeration: solve a small linear system per support. ✅ MathNet.Numerics handles it.
    
-   3+ player GDA: loop, compute best response (argmax over a dot product), update average. ✅ That's like 20 lines.
    

You're not behind. You're just past the "what are the numbers" hurdle, which is the only real conceptual wall. The rest is plumbing.