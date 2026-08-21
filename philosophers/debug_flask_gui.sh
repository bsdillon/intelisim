curl -v 'http://127.0.0.1:5000/socket.io/?transport=polling&EIO=4'

python -c "import socketio, engineio; print('socketio:', socketio.__version__); print('engineio:', engineio.__version__)"

pip show python-socketio python-engineio
pip show flask-socketio python-socketio python-engineio eventlet

python - <<'PY'
import socketio

sio = socketio.Client(logger=True, engineio_logger=True)

@sio.event
def connect():
    print("CONNECTED")

@sio.event
def connect_error(data):
    print("CONNECT ERROR:", data)

@sio.event
def disconnect():
    print("DISCONNECTED")

sio.connect("http://127.0.0.1:5000")
print("sid:", sio.sid)

input("Press Enter to disconnect...")
sio.disconnect()
PY