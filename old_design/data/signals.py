from enum import Enum

class Signals(Enum):
    # Player
    GO_START = "GoStart"
    STEP_BACK = "StepBack"
    PLAY_PAUSE = "PlayPause"
    STEP = "Step"
    GO_END = "GoEnd"
    SIM_RESTART = "SimRestart"
    WINDOW_CLOSING = "WindowClosing"
    
    # MainWindow
    RESIZE = "Resize"

    # Data/controls
    SET_VALUE = "SetValue"
