cd ..
# 1. Fresh venv + packages (server + client side)
python -m venv .venv
source .venv/bin/activate

python -m pip install --upgrade pip
python -m pip install \
  flask flask-socketio eventlet \
  python-socketio requests \
  PyDispatcher \
  mesa          # only needed for the real sims (wator etc.)

# 2. Start the Flask server (regenerates shared.js automatically)
export PYTHONPATH=.
python flask/app.py
