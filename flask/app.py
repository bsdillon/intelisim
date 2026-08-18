from flask import Flask, render_template, request, jsonify
from common.constants import *
from flask_socketio import SocketIO, join_room, leave_room
from pathlib import Path

WEB_CLIENT_ROOM = "web_clients"
app = Flask(__name__)

create_javascript(static_path=Path(app.static_folder + "/js"))

socketio = SocketIO(app, async_mode="eventlet", cors_allowed_origins="*")

desktop_sid = None

def no_desktop_registered(event):
    if desktop_sid == None:
        error = f"No desktop app has be registered upon {event}"
        print(error)
        socketio.emit("gui_error", error)

# ----- SERVICE TO WEB -----
@app.route("/")
def index():
    return render_template("index.html", server_version=SERVER_VERSION)

@app.route("/scatter.html")
def scatter():
    return render_template("scatter.html")

@app.route("/temp.html")
def temp_page():
    return render_template("temp.html")

@app.route("/multiline.html")
def multiline():
    return render_template("multiline.html")

@socketio.on("connect")
def handle_connect():
    sid = request.sid
    join_room(WEB_CLIENT_ROOM)
    theIO = app.extensions['socketio']
    count = len(socketio.server.manager.rooms.get('/', {}).get(WEB_CLIENT_ROOM, set()))
    print(f"Registered web client #{count}: {sid}", flush=True)
    
def run_server():
    print(f"Version {SERVER_VERSION} Serving...", flush=True)
    socketio.run(app, host="0.0.0.0", port=5000)

# ----- SIGNALS FROM DESKTOP -----
@socketio.on(SocketCommand.REGISTRATION.value)
def register_role(data):
    sid = request.sid
    print(f"New registration from {sid}", flush=True)
    role = data.get("role")
    if role == "desktop":
        global desktop_sid
        desktop_sid = sid
        leave_room(WEB_CLIENT_ROOM)
        socketio.emit(SocketSignal.START_GUI.value, {}, room="web_clients")
        socketio.emit(SocketSignal.REGISTRATION_RECEIVED.value, to=desktop_sid)
        print(f"Changed registration for a desktop application: {sid}", flush=True)
    else:
        print(f"Unrecognized client role {role}", flush=True)

@app.route("/"+SocketCommand.ADD_CONTROL.value, methods=["POST"])
def add_control():
    if no_desktop_registered(SocketCommand.ADD_CONTROL.value):
        return jsonify({"status": "error", "message": "Not accepted"}), 406
    else:
        data = request.get_json()
        print(f"{SocketCommand.ADD_CONTROL.value}: {data}", flush=True)
        if not data:
            return jsonify({"error": "invalid payload"}), 400

        # emit the widget JSON immediately to all connected clients
        socketio.emit(SocketSignal.ADD_CONTROL.value, data, room="web_clients")
        return jsonify({"status": "ok"})

@app.route("/"+SocketCommand.ADD_DATAPOINT.value, methods=["POST"])
def add_datapoint():
    if no_desktop_registered(SocketCommand.ADD_DATAPOINT.value):
        return jsonify({"status": "error", "message": "Not accepted"}), 406
    else:
        data = request.get_json()
        print(f"{SocketCommand.ADD_DATAPOINT.value}: {data}", flush=True)
        if not data:
            return jsonify({"error": "invalid payload"}), 400

        socketio.emit(SocketSignal.ADD_DATAPOINT.value, data, room="web_clients")
        return jsonify({"status": "ok"})

@app.route("/"+SocketCommand.UPDATE_DATAPOINTS.value, methods=["POST"])
def update_datapoints():
    if no_desktop_registered(SocketCommand.UPDATE_DATAPOINTS.value):
        return jsonify({"status": "error", "message": "Not accepted"}), 406
    else:
        data = request.get_json()
        print(f"{SocketCommand.UPDATE_DATAPOINTS.value}: {data}", flush=True)
        if not data:
            return jsonify({"error": "invalid payload"}), 400

        socketio.emit(SocketSignal.UPDATE_DATAPOINTS.value, data, room="web_clients")
        return jsonify({"status": "ok"})

@app.route("/"+SocketCommand.TEST_GUI.value, methods=["POST"])
def test_gui():
    if no_desktop_registered(SocketCommand.TEST_GUI.value):
        return jsonify({"status": "error", "message": "Not accepted"}), 406
    else:
        print(f"{SocketCommand.TEST_GUI.value}: {request.path}", flush=True)
        socketio.emit(SocketSignal.TEST_GUI.value, room="web_clients")
        return jsonify({"status": "ok"})

@app.route("/"+SocketCommand.NEW_FRAME.value, methods=["POST"])
def new_frame():
    if no_desktop_registered(SocketCommand.NEW_FRAME.value):
        return jsonify({"status": "error", "message": "Not accepted"}), 406
    else:
        data = request.get_json()
        print(f"{SocketCommand.NEW_FRAME.value}: {data['Step']}", flush=True)
        socketio.emit(SocketSignal.NEW_FRAME.value, data, room="web_clients")
        return jsonify({"status": "ok"})

# ----- SIGNALS FROM WEB -----
@socketio.on(SocketSignal.CONTROL_CHANGED.value)
def handle_control_changed(data):
    if no_desktop_registered(SocketSignal.CONTROL_CHANGED.value):
        return
    else:
        print(f"{SocketSignal.CONTROL_CHANGED.value}: {data}", flush=True)
        socketio.emit(SocketSignal.CONTROL_CHANGED.value, data, to=desktop_sid)

@socketio.on(SocketSignal.JAVASCRIPT_ERROR.value)
def send_error(data):
    if no_desktop_registered(SocketSignal.JAVASCRIPT_ERROR.value):
        return
    else:
        print(f"{SocketSignal.JAVASCRIPT_ERROR.value}: {data['msg']}", flush=True)
        socketio.emit(SocketSignal.GUI_ERROR.value, data, to=desktop_sid)

@socketio.on(SocketSignal.STEP_FUNCTION.value)
def step_function():
    if no_desktop_registered(SocketSignal.STEP_FUNCTION.value):
        return
    else:
        if DEBUGGING:
            print(f"{SocketSignal.STEP_FUNCTION.value}", flush=True)
        socketio.emit(SocketSignal.STEP_FUNCTION.value, to=desktop_sid)

@socketio.on(SocketSignal.RUN_INDEFINITE.value)
def run_all():
    if no_desktop_registered(SocketSignal.RUN_INDEFINITE.value):
        return
    else:
        if DEBUGGING:
            print(f"{SocketSignal.RUN_INDEFINITE.value}", flush=True)
        socketio.emit(SocketSignal.RUN_INDEFINITE.value, to=desktop_sid)

@socketio.on(SocketSignal.HALT_ALL.value)
def halt_all():
    if no_desktop_registered(SocketSignal.HALT_ALL.value):
        return
    else:
        if DEBUGGING:
            print(f"{SocketSignal.HALT_ALL.value}", flush=True)
        socketio.emit(SocketSignal.HALT_ALL.value, to=desktop_sid)

if __name__ == "__main__":
    run_server()
