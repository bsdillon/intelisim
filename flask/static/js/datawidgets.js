import { initializeType } from './shared.js';

export class Threshold {
    //Title, output value, and colored rectangle to show threshold values
    //All other values in the range [min,max] are extrapolated

    constructor(widgetJson) {
        initializeType(this, widgetJson, false);

        this.value = this.min;

        //color_scale is a dictionary of known color values e.g. {0:"red", 1:"green", 2:"red"}
        this.colorScale = Object.fromEntries(
            Object.entries(widgetJson["colorScale"]).sort((a,b) => a[0]-b[0])
        );

        this.valueElem = null;
        this.barCanvas = null;
        this.barCtx = null;
    }

    build(container) {
        // Container
        const group = document.createElement("div");
        group.style.marginBottom = "10px";

        // Title
        const titleElem = document.createElement("div");
        titleElem.innerText = this.title;
        titleElem.style.color = "#000088";
        titleElem.style.fontWeight = "bold";
        group.appendChild(titleElem);

        // Value display
        this.valueElem = document.createElement("div");
        this.valueElem.innerText = this.value.toFixed(2);
        group.appendChild(this.valueElem);

        // Drawlist / bar canvas
        this.barCanvas = document.createElement("canvas");
        this.barCanvas.width = 300;
        this.barCanvas.height = 20;
        this.barCtx = this.barCanvas.getContext("2d");
        group.appendChild(this.barCanvas);

        container.appendChild(group);

        // Initial draw
        this.drawBar(this.value);
    }

    drawBar(value) {
        const ctx = this.barCtx;
        const width = this.barCanvas.width;
        const height = this.barCanvas.height;

        ctx.clearRect(0, 0, width, height);

        // Find color for this value
        let drawColor = "gray";
        for (let key of Object.keys(this.colorScale).map(Number).sort((a,b)=>a-b)) {
            if (value >= key) {
                drawColor = this.colorScale[key];
            } else {
                break;
            }
        }

        ctx.fillStyle = drawColor;
        const percent = (value - this.min) / (this.max - this.min);
        ctx.fillRect(0, 0, width * percent, height);
    }

    setValue(value) {
        this.value = Math.max(this.min, Math.min(this.max, value));
        this.valueElem.innerText = this.value.toFixed(2);
        this.drawBar(this.value);
    }

    getUpdateRunner() {
        return (paramJson) => {
            const value = paramJson[this.title];
            if (value != null) this.setValue(value);
        };
    }
}

export class ScatterPlot {
    constructor(widgetJson) {
        initializeType(this, widgetJson, false);

        //create title from variables
        this.title = `${this.independentVar} vs. ${this.dependentVar}`;

        this.rSquared = 0.0;
        this.mSlope = 0.0;
        this.intercept = 0.0;

        // DOM elements
        this.titleElem = null;
        this.mLabelElem = null;
        this.rLabelElem = null;
        this.popupButton = null;
        this.popup = null;

        // Data arrays
        this.xData = [];
        this.yData = [];
    }

    build(container) {
        // Container div for the widget
        const group = document.createElement("div");
        group.style.marginBottom = "10px";

        // Title label
        this.titleElem = document.createElement("div");
        this.titleElem.innerText = this.title;
        this.titleElem.style.fontWeight = "bold";
        this.titleElem.style.color = "#000088";
        this.titleElem.style.marginBottom = "2px";
        group.appendChild(this.titleElem);

        // Slope label
        this.mLabelElem = document.createElement("span");
        this.mLabelElem.innerText = "M: NA";
        this.mLabelElem.style.marginRight = "10px";
        group.appendChild(this.mLabelElem);

        // R² label
        this.rLabelElem = document.createElement("span");
        this.rLabelElem.innerText = "R²: NA";
        this.rLabelElem.style.marginRight = "10px";
        group.appendChild(this.rLabelElem);

        // Button to show scatter plot
        this.popupButton = document.createElement("button");
        this.popupButton.innerText = "Show Plot";
        this.popupButton.disabled = true; // initially disabled
        this.popupButton.addEventListener("click", () => this.showPopup());
        group.appendChild(this.popupButton);

        // Append to parent container
        container.appendChild(group);
    }

    showPopup() {
        this.popup = window.open("./scatter.html", this.title, "width=400,height=300");
        this.popup.addEventListener("load", () => {
            this.popup.createPlot();

            if (this.xData.length > 3) {
                this._updateRSquared();
                if (this.popupButton) this.popupButton.disabled = false; // enable button
            }
        });

        window.addEventListener("beforeunload", () => {
            if (this.popup && !this.popup.closed) {
                this.popup.close();
            }
            this.popup = null;
            if (this.popupButton) this.popupButton.disabled = false; // enable button
        });
    }

    getUpdateRunner() {
        return (paramJson) => {
            const x = paramJson["x"];
            const y = paramJson["y"];

            // Add new data to arrays
            this.xData.push(x);
            this.yData.push(y);

            if (this.xData.length > 3) {
                this._updateRSquared();
                if (this.popupButton) this.popupButton.disabled = false; // enable button
            }
        };
    }

    _updateRSquared() {
        // Compute slope (m) and intercept (b) using least squares
        const n = this.xData.length;
        const xMean = this.xData.reduce((a,b) => a+b, 0)/n;
        const yMean = this.yData.reduce((a,b) => a+b, 0)/n;

        let num = 0, denom = 0;
        for (let i = 0; i < n; i++) {
            num += (this.xData[i] - xMean) * (this.yData[i] - yMean);
            denom += (this.xData[i] - xMean) ** 2;
        }

        const slope = denom !== 0 ? num / denom : 0;
        this.intercept = yMean - slope * xMean;

        // Compute R²
        let ssRes = 0, ssTot = 0;
        for (let i = 0; i < n; i++) {
            const yFit = slope * this.xData[i] + this.intercept;
            ssRes += (this.yData[i] - yFit) ** 2;
            ssTot += (this.yData[i] - yMean) ** 2;
        }
        const rSquared = ssTot !== 0 ? 1 - ssRes / ssTot : 0;

        this.mSlope = slope;
        this.rSquared = rSquared;

        // Update labels
        if (this.mLabelElem) this.mLabelElem.innerText = `M: ${slope.toFixed(3)}`;
        if (this.rLabelElem) this.rLabelElem.innerText = `R²: ${rSquared.toFixed(2)}`;
        if (this.popup && !this.popup.closed) {
            this.popup.update_plot(this.xData, this.yData, this.mSlope, this.intercept, this.rSquared, this.independentVar, this.dependentVar);
        }
    }
}

export class MultiLinePlot {
    constructor(widgetJson) {
        initializeType(this, widgetJson, false);

        this.maxMemory = widgetJson["maxMemory"] || 1000;

        // Data storage
        this.timestamps = [];
        this.datasets = {};
        for (const varName of this.dependentVars) {
            this.datasets[varName] = [];
        }

        // DOM elements
        this.titleElem = null;
        this.popupButton = null;
        this.popup = null;
    }

    build(container) {
        const group = document.createElement("div");
        group.style.marginBottom = "10px";

        // Title label
        this.titleElem = document.createElement("div");
        this.titleElem.innerText = this.title;
        this.titleElem.style.fontWeight = "bold";
        this.titleElem.style.color = "#000088";
        this.titleElem.style.marginBottom = "2px";
        group.appendChild(this.titleElem);

        // Button to show plot
        this.popupButton = document.createElement("button");
        this.popupButton.innerText = "Show Plot";
        this.popupButton.disabled = true;
        this.popupButton.addEventListener("click", () => this.showPopup());
        group.appendChild(this.popupButton);

        container.appendChild(group);
    }

    showPopup() {
        this.popup = window.open("./multiline.html", this.title, "width=800,height=600");
        this.popup.addEventListener("load", () => {
            this.popup.createMultiLinePlot();
            if (this.timestamps.length > 0) {
                this._updatePopup();
            }
        });

        window.addEventListener("beforeunload", () => {
            if (this.popup && !this.popup.closed) this.popup.close();
            this.popup = null;
        });
    }

    getUpdateRunner() {
        return (paramJson) => {
            const t = this.timestamps.length;
            this.timestamps.push(t);

            for (const name of this.dependentVars) {
                const value = paramJson[name];
                this.datasets[name].push(value);
                if (this.datasets[name].length > this.maxMemory) {
                    this.datasets[name].shift();
                }
                // if (value !== undefined && value !== null) {
                // }
            }

            if (this.timestamps.length > 1 && this.popupButton) {
                this.popupButton.disabled = false;
            }

            if (this.popup && !this.popup.closed) {
                this._updatePopup();
            }
        };
    }

    _updatePopup() {
        this.popup.update_multi_line_plot(
            this.datasets,
            this.timestamps,
            this.title,
            "Value"
        );
    }
}