import { reportGuiError } from './guierror.js';
import { initializeType } from './shared.js';

export class Range {
    /**
     * Creates a slider with a title and a displayed value.
     * Fires a callback when the value changes.
     */
    constructor(widgetJson) {
        initializeType(this, widgetJson, true);

        this.value = this.initial;
        this.onChange = null;

        if (this.value < this.min || this.value > this.max || this.min > this.max) {
            reportGuiError(this.constructor.name, `Range values must be ${this.min}<=${this.value}<=${this.max}`)
        }
    }

    build(container) {
        // container group
        const group = document.createElement("div");
        group.classList.add("singlerow");

        // title
        const titleElem = document.createElement("label");
        titleElem.innerText = this.title;
        titleElem.style.color = "#000088";
        titleElem.style.fontWeight = "bold";
        titleElem.style.paddingRight = "10px";
        group.appendChild(titleElem);

        // value display
        this.valueElem = document.createElement("label");
        this.valueElem.innerText = this.value;
        this.valueElem.style.paddingRight = "10px";
        group.appendChild(this.valueElem);

        // slider
        this.sliderElem = document.createElement("input");
        this.sliderElem.type = "range";
        this.sliderElem.min = this.min;
        this.sliderElem.max = this.max;
        this.sliderElem.value = this.value;
        this.sliderElem.style.width = "auto";
        this.sliderElem.style.flex = "1"; 
        this.sliderElem.addEventListener("input", (e) => {
            this.value = parseInt(e.target.value);
            this.valueElem.innerText = this.value;
            if (this.onChange) {
                this.onChange(this.value);
            }
        });

        group.appendChild(this.sliderElem);

        // append to container
        container.appendChild(group);
    }

    /**
     * Update the slider value programmatically
     * @param {number} newValue
     */
    setValue(newValue) {
        if (newValue < this.min || newValue > this.max) return;
        this.value = newValue;
        this.sliderElem.value = newValue;
        this.valueElem.innerText = newValue;
        if (this.onChange) {
            this.onChange(newValue);
        }
    }

    /**
     * Register a callback to be fired when value changes
     * @param {function} callback
     */
    register(callback) {
        this.onChange = callback;
    }
}