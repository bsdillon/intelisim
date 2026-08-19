#!/usr/bin/env bash
set -e

python -m venv .venv
source .venv/bin/activate

python -m pip install --upgrade pip
python -m pip install flask flask-socketio eventlet python-socketio requests

# wator
python -m pip install networkx agents

# Optional but useful for debugging
python -c "import flask, flask_socketio, eventlet; print('imports OK'); print(flask.__file__)"