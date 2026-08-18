import { reportGuiError } from './guierror.js';
import { SocketSignal } from './shared.js';

const RecordStates = {
    RECORDING: "RECORDING",
    NOT_RECORDING: "NOT_RECORDING"
}

const ControlStates = {
    PLAYING: "PLAYING",
    STOPPED: "STOPPED",
    BLOCKED: "BLOCKED"
};

const BUFFER_INCREMENT = 50;

export class BufferedPlayer {
    constructor(socket, renderer) {
        this.socket = socket;
        this.renderer = renderer;

        this.running = false;
        this.delay = 250; // ms
        this.time = 0;
        this.useDelays = true;
        this.currentStep = 0;
        this.skip_rate = 0;
        this.last_draw_time = -1;

        this.drawingData = new Map();
        this.bufferEnd = -1; // No data at present
        this.expectedTime = BUFFER_INCREMENT;
        socket.on(SocketSignal.NEW_FRAME, this._bufferFrame.bind(this));

        this._lock_updateProgressBar = false;
        this._lock_simTime = false;

        this.controlState = ControlStates.STOPPED;
        this.recordState = RecordStates.NOT_RECORDING;
    }

    build(container)
    {
        const buttonRow = document.createElement("div");
        buttonRow.classList.add("button-row");
        this._btnRestart = this._createButton(buttonRow, "restart.png", () => this._onRestart());
        this._btnStart = this._createButton(buttonRow, "start.png", () => this._onStart());
        this._btnStepBack = this._createButton(buttonRow, "step_back.png", () => this._onStepBack());
        this._btnRecord = this._createButton(buttonRow, "record_off.png", () => this._toggleRecord());
        this._btnPlayPause = this._createButton(buttonRow, "play.png", () => this._onPlayPause());
        this._btnStepForward = this._createButton(buttonRow, "step.png", () => this._onStepForward());
        this._btnEnd = this._createButton(buttonRow, "end.png", () => this._onEnd());

        // ---- assemble ----
        container.appendChild(buttonRow);

        let playerRow = document.createElement('div');
        playerRow.className = 'player';
 
        this.step_label = document.createElement('label');
        this.step_label.className = 'step_label';
        this.step_label.textContent = "----";
 
        this.progressbar = document.createElement('div');
        this.progressbar.className = 'progress-container';
        this.progressbar.onclick = (e) => this._onProgressbarClick(e);

        this.buffered = document.createElement('div');
        this.buffered.className = 'buffered';

        this.playTime = document.createElement('div');
        this.playTime.className = 'played';

        // ---- assemble ----
        playerRow.appendChild(this.step_label);
        this.progressbar.appendChild(this.buffered);
        this.progressbar.appendChild(this.playTime);
        playerRow.appendChild(this.progressbar);
        container.appendChild(playerRow);
    }

    // ------------------------- //
    //       API Functions       //
    // ------------------------- //
    adjustSpeed(value) {
        // value in ms
        this.delay = value;
    }

    setSkipRate(value) {
        this.skip_rate = value;
        this.useDelays = this.skip_rate == 0;
    }

    stopPlayer() {
        if (this.controlState === ControlStates.PLAYING) {
            this.running = false; 
            this.socket.emit(SocketSignal.HALT_ALL);
        }
    }
    // ------------------------- //
    //     End API Functions     //
    // ------------------------- //

    // ------------------------- //
    //        GUI Helpers        //
    // ------------------------- //
    _createButton(parent, imageFile, onClick)
    {
        const button = document.createElement("button");
        button.style.border = "none";
        button.style.background = "none";
        button.style.padding = "0";
        button.style.cursor = "pointer";

        const img = document.createElement("img");
        img.src = `/static/images/${imageFile}`;
        img.alt = "Button";
        img.width = 32;
        img.height = 32;

        button.appendChild(img);
        button.classList.add("player_button")

        button.addEventListener("click", onClick);

        parent.appendChild(button);
        return img;
    }

    _onProgressbarClick(e) {
        this._assertState(ControlStates.BLOCKED);

        //finds the location of the click and MAY update the currentStep
        const rect = this.progressbar.getBoundingClientRect();
        const ratio = (e.clientX - rect.left) / rect.width;
        const newTime = Math.floor(ratio * this.bufferEnd);
        if (newTime <= this.bufferEnd) {
            this._setCurrentStep(newTime);
            this._updateSimTime();
        }

        this._assertState(ControlStates.STOPPED);
        this.stopPlayer();
    }

    _onRestart() {
        this._assertState(ControlStates.STOPPED);

        //action
        // fetch("/api/restart_call", {method: "POST"})
        // .then(response => response.json())
        // .then(data => {console.log("Python response:", data);})
        // .catch(err => console.error("Error calling Python:", err));

        this._assertState(ControlStates.STOPPED);
    }

    _onStart() {
        this._assertState(ControlStates.BLOCKED);

        this._setCurrentStep(0);
        this._updateSimTime();

        this._assertState(ControlStates.STOPPED);
    }

    _onStepBack() {
        this._assertState(ControlStates.BLOCKED);

        // TODO defect: only one click updates time, but not the GUI.
        this._setCurrentStep(this.currentStep - 1);
        this._updateSimTime();

        this._assertState(ControlStates.STOPPED);
    }

    _toggleRecord() {
        let wasRecording = this.recordState === RecordStates.RECORDING;

        if (wasRecording) {
            this._btnRecord.src = "/static/images/record_off.png";
            this.socket.emit(SocketSignal.HALT_ALL);
            this.recordState = RecordStates.NOT_RECORDING;
        } else {
            this._btnRecord.src = "/static/images/record_on.png";
            this.socket.emit(SocketSignal.RUN_INDEFINITE);
            this.recordState = RecordStates.RECORDING;
        }
    }

    _onPlayPause() {
        const wasRunning = this.controlState === ControlStates.PLAYING;
        this._assertState(ControlStates.BLOCKED);

        if (wasRunning) {
            this._btnPlayPause.src = "/static/images/play.png";
            this._assertState(ControlStates.STOPPED);
            this.running = false; 
        } else {
            this._btnPlayPause.src = "/static/images/pause.png";
            this._assertState(ControlStates.PLAYING);
            this.running = true;
            this._play();
        }
    }

    _onStepForward() {
        this._assertState(ControlStates.BLOCKED);

        // TODO defect: only one click updates time, but not the GUI.
        const waitForData = async () => {
            let currentStep_str = String(this.currentStep);
            if (!(currentStep_str in this.drawingData))// check if the data doesn't exist
            {
                // request the data
                this.socket.emit(SocketSignal.STEP_FUNCTION);
            }

            while (!(currentStep_str in this.drawingData))
            {
                await new Promise(r => setTimeout(r, 250));
            }
            this._updateSimTime();
            this._setCurrentStep(this.currentStep + 1);
            this._assertState(ControlStates.STOPPED);
        };

        waitForData();
    }

    _onEnd() {
        this._assertState(ControlStates.BLOCKED);

        this._setCurrentStep(this.bufferEnd);
        this._updateSimTime();

        this._assertState(ControlStates.STOPPED);
    }

    _enableButton(button, enabled)
    {
        if (enabled)
        {
            button.classList.remove("disabled");
        }
        else
        {
            button.classList.add("disabled");
        }
    }

    _assertState(state) {
        switch (state) {
            case ControlStates.PLAYING:
                this._enableButton(this._btnStart,false);
                this._enableButton(this._btnStepBack,false);
                this._enableButton(this._btnPlayPause,true);
                this._enableButton(this._btnStepForward,false);
                this._enableButton(this._btnEnd,false);
                this._enableButton(this._btnRecord, false);
                break;
            case ControlStates.STOPPED:
                this._enableButton(this._btnStart,true);
                this._enableButton(this._btnStepBack,true);
                this._enableButton(this._btnPlayPause,true);
                this._enableButton(this._btnStepForward,true);
                this._enableButton(this._btnEnd,true);
                this._enableButton(this._btnRecord, true);
                this.stopPlayer();
                break;
            case ControlStates.BLOCKED:
                this._enableButton(this._btnStart,false);
                this._enableButton(this._btnStepBack,false);
                this._enableButton(this._btnPlayPause,false);
                this._enableButton(this._btnStepForward,false);
                this._enableButton(this._btnEnd,false);
                this._enableButton(this._btnRecord, false);
                break;
            default:
                reportGuiError(this.constructor.name, `GUI in unknown state ${state}`)
        }
        this.controlState = state;
    }
    // ------------------------- //
    //      End GUI Helpers      //
    // ------------------------- //

    _setCurrentStep(stepNumber)
    {
        this.currentStep = stepNumber;
        this.step_label.textContent = String(stepNumber).padStart(4, "0");
    }

    async _updateProgressBar()
    {
        // Mutex will only allow the last update to the progress bar but is idempotent
        if (this._lock_updateProgressBard) {
            return;
        }
        this._lock_updateProgressBar = true;

        const current_buffer = Math.max(this.bufferEnd, 0);
        const current_expectation = Math.max(this.expectedTime, 0);
        const currentStep = Math.min(Math.max(this.currentStep, 0), current_buffer);

        if (current_buffer >= current_expectation) //we have arrived at the expected time
        {
            //we can extend the expected time by a standard 50 frames
            this.expectedTime += BUFFER_INCREMENT;
        }

        // Apply CSS widths safely
        this.buffered.style.width = `${((current_buffer / current_expectation) * 100).toFixed()}%`;
        this.playTime.style.width = `${((currentStep / current_expectation) * 100).toFixed(2)}%`;

        this._lock_updateProgressBar = false;
    }

    async _updateSimTime()
    {
        // Mutex will only allow the last update to sim time but is idempotent
        if (this._lock_simTime) {
            return;
        }
        this._lock_simTime = true;

        try
        {
            this._updateProgressBar();

            let temp_time = this.currentStep;
            // let update_draw = this.last_draw_time != temp_time;
            // if (update_draw)
            // {
                let step_str = String(temp_time);
                if(!(step_str in this.drawingData))
                {
                    reportGuiError(this, `Drawing frame for time ${temp_time} does not exist`);
                }

                this.renderer.drawFrame(this.drawingData[step_str]);
                this.last_draw_time = temp_time;
            // }
        }
        finally
        {
            this._lock_simTime = false;
        }
    }

    async _bufferFrame(data)
    {
        // Allows data as often as it comes in
        let step = data["Step"];
        let step_str = String(step);
        let list = data["Drawings"];
        if(step_str in this.drawingData)
        {
            reportGuiError(this, `Drawing frame for time ${step} already exists`);
        }

        this.drawingData[step_str] = list;

        this.bufferEnd = Math.max(this.bufferEnd, step);
        this._updateProgressBar();
    }

    async _play() {
        // Runs continuously until stopped
        while (this.running) {
            let currentStep_str = String(this.currentStep);
            const start = performance.now();

            while (this.running && !(currentStep_str in this.drawingData)) //allows exit on signal to stop running
            {
                //there is no data so we need to wait for it
                await new Promise(resolve => setTimeout(resolve, 250));
            }

            if (this.running && currentStep_str in this.drawingData) //allows exit on signal to stop running
            {
                this._updateSimTime(); //do all the work
                this._setCurrentStep(this.currentStep + 1);

                //check for delay
                const end = performance.now();
                const actualDelay = end - start;
                const delta = Math.max(0, this.delay - actualDelay);

                if (this.useDelays && delta > 0) //IF WE STILL NEED A DELAY
                {
                    await new Promise(resolve => setTimeout(resolve, delta));
                }
            }
        }
    }
}