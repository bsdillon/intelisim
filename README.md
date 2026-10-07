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

2. Tracks
    - Chats
        - [ML Roadmap](https://chatgpt.com/share/6ac6a224-6390-83ea-b22d-2edc2a9fca0c) + [copy](./chats/chatgpt/ml_roadmap.md)
        -
3. Courses
    - [Datacamp](https://www.datacamp.com/tracks/machine-learning-engineer)
    - [SciKit Learn](https://scikit-learn.org/stable/user_guide.html)

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

* [convo](https://grok.com/c/c63eece6-5a90-4b03-9480-e26532d72f60?rid=4b5a3aa3-8723-4f35-bce0-b5513f0197c4)
  The embedder is fine. The string you hashed is not. Railway currently has no CsharpMethod rows, so this is the homelab
  graph you already measured, not a new failure.

  Extract<T> and AutoExtract<T> were both stored as 1253 characters starting at var options = RegexOptions.Compiled.
  Same span, same vector. Cosine then cannot prefer the method you asked for. A second backfill also embedded Namespace
  nodes of 6–32 characters, and names like if and foreach, because upload\_to\_neo4j was false and the regex had not yet
  rejected them. mxbai-embed-large did what it was asked. It embedded the wrong text, and it is a general English model,
  not a C# one. Nothing was trained on your "right method / wrong method" pairs. model\_name on the node is the right
  escape hatch. Re-embed after the body starts at public static. Do not tune anything until those two previews differ.

  Training is a different skill. You have only run inference.

  Do these in order, and skip the 500-lesson lists:

    1. [karpathy/nn-zero-to-hero](https://github.com/karpathy/nn-zero-to-hero) — micrograd, then makemore. Loss,
       backprop,
       train versus validation. This is the part you do not have.
    2. [karpathy/nanoGPT](https://github.com/karpathy/nanoGPT) — one small GPT trained on text you can overfit on
       purpose,
       then fail to generalize. [karpathy/nanochat](https://github.com/karpathy/nanochat) is the same loop with a chat
       UI,
       if you want the full path later.
    3. [rasbt/LLMs-from-scratch](https://github.com/rasbt/LLMs-from-scratch) — the same model, written as a book so the
       tensor shapes are not magic.
    4. Only then [UKPLab/sentence-transformers](https://github.com/UKPLab/sentence-transformers). That is how you would
       train an embedder: pairs of (query, Extract<T> body) against (query, AutoExtract body). Useless until you have
       fifty
       labeled pairs and a held-out set.

  > Ok. My team wants me to incorporate computer vision into my ML learnings. Same list, with some extras, right?

  Only the first step is the same. After that the list changes. A vision model is a function from pixels to a label or a
  box. nanoGPT and sentence-transformers train on tokens. They will not teach the part your team is asking for.

  Keep [karpathy/nn-zero-to-hero](https://github.com/karpathy/nn-zero-to-hero) through micrograd. Loss, backprop, train
  versus validation. Stop before makemore. That series turns into language models.

  Then, in order:

    1. Justin Johnson's Deep Learning for Computer Vision assignments, the public notebook set. Classification first: a
       linear model on pixels, then a small conv net. The metric is accuracy on a held-out image folder, not
       cosine. [stanford-cs231n](http://cs231n.stanford.edu/) is the lecture track those notebooks come from.
    2. [pytorch/vision](https://github.com/pytorch/vision) only as the library under that. Transforms, ImageFolder, a
       pretrained ResNet you fine-tune on your own split. Fine-tune means the weights change. Calling a model on a
       screenshot does not.
    3. [ultralytics/ultralytics](https://github.com/ultralytics/ultralytics) after the classifier beats a dumb baseline.
       Detection is a different label: a box plus a class, scored with mAP, not accuracy. Do not start here. YOLO will
       hide
       the training loop you still do not have.

  What carries over from the graph is the failure mode. You embedded the wrong span and the ranker looked smart. In
  vision
  the same bug is a crop, a label, or a leaked frame from the same video in both train and val. Build a frozen image set
  before you train anything. A hundred labeled frames and a split is enough to learn the loop. The headset project can
  be
  that set later. It is not the course.


