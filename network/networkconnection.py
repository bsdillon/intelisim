import os
import sys
import socketio
import requests
import threading
from urllib.parse import urljoin
import time
from datetime import datetime, timedelta
from common.constants import *
from pydispatch import dispatcher

# ----- CONNECT TO THE GUI SERVER -----

FLASK_URL = os.getenv(
    "INTELISIM_GUI_URL",
    "http://127.0.0.1:5000"
)

# FLASK_URL = "http://localhost:5000"
# FLASK_URL = "http://127.0.0.1:5000"

class NetworkConnection():
    def __init__(self):
        # Create the TCP (or HTTP under the hood) connection
        self.sio = socketio.Client()
        self.waiting_for_registration = True

        # ----- REGISTER FOR BUILT-IN EVENTS
        @self.sio.event
        def connect():
            """Fires when the client successfully connects to the server."""
            if DEBUGGING:
                print("Connected, sending registration", flush=True)
            self.sio.emit(SocketCommand.REGISTRATION.value, {"role": "desktop"})

        @self.sio.event
        def disconnect():
            """Fires when the client disconnects from the server."""
            if DEBUGGING:
                print("Disconnected from server", flush=True)

        @self.sio.event
        def connect_error(data):
            """
            Fires when the client fails to connect to the server.
            'data' may include a reason string from the server.
            """
            if DEBUGGING:
                print(f"Connect error: {data}", flush=True)

        @self.sio.event
        def connect_timeout():
            """
            Fires when the client cannot connect within the configured timeout.
            Useful for logging or retry logic.
            """
            if DEBUGGING:
                print("Connect attempt timed out", flush=True)

        @self.sio.event
        def reconnect(data):
            """
            Fires after the client successfully reconnects after a disconnection.
            'data' may include reconnection attempt info.
            """
            if DEBUGGING:
                print(f"Reconnected to server {data}", flush=True)

        @self.sio.event
        def reconnect_attempt(attempt_number):
            """
            Fires when the client is attempting to reconnect.
            'attempt_number' is the current attempt (1, 2, ...).
            """
            if DEBUGGING:
                print(f"Reconnect attempt #{attempt_number}", flush=True)

        # Alias for older versions / convenience
        reconnecting = reconnect_attempt

        @self.sio.event
        def reconnect_error(data):
            """
            Fires when a reconnect attempt fails.
            'data' may include an error message or code.
            """
            if DEBUGGING:
                print(f"Reconnect error: {data}", flush=True)

        @self.sio.event
        def reconnect_failed():
            """
            Fires when the client has reached the maximum number of reconnect attempts.
            No further reconnect attempts will be made unless manually triggered.
            """
            if DEBUGGING:
                print("Reconnect failed: maximum attempts reached", flush=True)

        @self.sio.on(SocketSignal.REGISTRATION_RECEIVED.value)
        def registration_recieved():
            if DEBUGGING:
                print("Registration complete")
            self.waiting_for_registration = False

        @self.sio.on("message")
        def handle_message(data):
            """
            Default catch-all event for simple messages sent from the server using
            'socket.send()' or 'emit("message")'.
            'data' can be any JSON-serializable object.
            """
            print(f"Message received: {data}", flush=True)
            
        # ----- REGISTER FOR APPLICATION-SPECIFIC EVENTS
        self.control_handlers = {}
        @self.sio.on(SocketSignal.CONTROL_CHANGED.value)
        def control_changed(data):
            name = data.get("name")
            value = data.get("value")
            if DEBUGGING:
                print(f"Control {name} has new value {value}", flush=True)

            if name in self.control_handlers:
                for handler in self.control_handlers[name]:
                    handler(value)

        @self.sio.on(SocketSignal.GUI_ERROR.value)
        def gui_error(data):
            print(f"GUI ERROR: {data['msg']}", file=sys.stderr, flush=True)

            # TODO handle error?            

        @self.sio.on(SocketSignal.STEP_FUNCTION.value)
        def step_function():
            if DEBUGGING:
                print(f"Step function called by GUI" )
            dispatcher.send(signal=SocketSignal.STEP_FUNCTION, sender=self)

        @self.sio.on(SocketSignal.RUN_INDEFINITE.value)
        def run_indefinite():
            if DEBUGGING:
                print(f"Run indefinitely called by GUI" )
            dispatcher.send(signal=SocketSignal.RUN_INDEFINITE, sender=self)

        @self.sio.on(SocketSignal.HALT_ALL.value)
        def halt_all():
            if DEBUGGING:
                print(f"Halt run called by GUI" )
            dispatcher.send(signal=SocketSignal.HALT_ALL, sender=self)

        self._stop_event = threading.Event()
        self._wait_thread = None
        self.sio.connect(FLASK_URL)

        while self.waiting_for_registration:
            time.sleep(1)

    def register_step_functions(self, one_step, run_all, halt_all):
        '''
        Attach the designated functions for signalling
        '''
        def one_handler(**kwargs):
            one_step()

        def all_handler(**kwargs):
            run_all()

        def halt_handler(**kwargs):
            halt_all()

        dispatcher.connect(one_handler, signal=SocketSignal.STEP_FUNCTION, sender=self, weak=False)
        dispatcher.connect(all_handler, signal=SocketSignal.RUN_INDEFINITE, sender=self, weak=False)
        dispatcher.connect(halt_handler, signal=SocketSignal.HALT_ALL, sender=self, weak=False)

    def indefinite_listen(self, total_seconds=-1):
        '''
        Creates a separate thread that waits on service
        Calling thread MUST still block its own progress at some point

        By default the block is indefinite BUT if total_seconds is set
        to a positive value, the thread will listen for only that time
        '''
        start_time = datetime.now()
        end_time = start_time + timedelta(seconds=total_seconds)
        if total_seconds <= 0:
            end_time = None

        if self._wait_thread is None or not self._wait_thread.is_alive():
            def run_wait():
                try:
                    self.sio.wait()  # This blocks the socket client
                except Exception as e:
                    if DEBUGGING:
                        print("Socket wait terminated:", e)
                finally:
                    self._stop_event.set()  # Signal that wait ended

            self._wait_thread = threading.Thread(target=run_wait, daemon=True)
            self._wait_thread.start()

            try:
                while (end_time is None or datetime.now() < end_time) and not self._stop_event.is_set() :
                    self._stop_event.wait(timeout=0.1)  # Allows CTRL-C
            except KeyboardInterrupt:
                if DEBUGGING:
                    print("Keyboard interrupt received, disconnecting...")

                if self.sio.connected:
                    self.sio.disconnect()

                self._stop_event.set()

                if self._wait_thread is not None:
                    self._wait_thread.join(timeout=1)

    def register_control_handler(self, control_name, handler):
        if control_name not in self.control_handlers:
            self.control_handlers[control_name] = []
        self.control_handlers[control_name].append(handler)

    def test_gui(self):
        '''
        Draw the test pattern on the GUI
        '''
        requests.post(urljoin(FLASK_URL,SocketCommand.TEST_GUI.value), json={})

    def add_control(self, controljson):
        '''
        Validates control values and sends the request to the GUI
        '''
        if validate_type(controljson, True):
            def send():
                requests.post(urljoin(FLASK_URL, SocketCommand.ADD_CONTROL.value), json=controljson)

            threading.Thread(target=send, daemon=True).start()

    def add_datapoint(self, datapointjson):
        '''
        Validates datapoint values and sends the request to the GUI
        '''
        if validate_type(datapointjson, False):
            def send():
                requests.post(urljoin(FLASK_URL, SocketCommand.ADD_DATAPOINT.value), json=datapointjson)

            threading.Thread(target=send, daemon=True).start()

    def send_frame(self, step, framejson):
        '''
        Sends a new frame to the GUI
        '''
        if DEBUGGING:
            print(f"Sending frame {step}")

        def task():
            requests.post(urljoin(FLASK_URL, SocketCommand.NEW_FRAME.value), json=framejson)
            
        threading.Thread(target=task, daemon=True).start()

    def update_datapoints(self, datapointjson):
        '''
        Updates all data points of interest
        '''
        def send():
            requests.post(urljoin(FLASK_URL, SocketCommand.UPDATE_DATAPOINTS.value), json=datapointjson)

        threading.Thread(target=send, daemon=True).start()
