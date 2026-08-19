#python -m venv .venv
#source .venv/bin/activate

python -m pip install --upgrade pip
python -m pip install flask flask_socketio eventlet python-socketio requests python-dispatch

python -c "import flask, flask_socketio, eventlet; print('imports OK')"
