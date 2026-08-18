import { Range } from './controlwidgets.js';
import { ScatterPlot, Threshold, MultiLinePlot } from './datawidgets.js';
import {reportGuiError} from './guierror.js';
import { BufferedPlayer } from './bufferedplayer.js';

// Simple event dispatcher
const dispatcher = new EventTarget();

export class WidgetManager {
    static MAX_SPEED = 20
    static MAX_SKIP = 10

    constructor(left_panel_id, socket, player) {
        this.socket = socket;

        const left_panel = document.getElementById(left_panel_id);
        // ----- Create permanent controls -----
        this.permanent_controls = document.createElement("div");
        left_panel.appendChild(this.permanent_controls);

        var temp_tag = document.createElement("hr")
        temp_tag.classList.add("divider")
        left_panel.appendChild(temp_tag);

        // ----- Create dynamic controls -----
        var temp_tag = document.createElement("h3")
        temp_tag.innerText = "Controls";
        left_panel.appendChild(temp_tag);

        this.dynamic_controls = document.createElement("div");
        this.dynamic_controls.classList.add("scrollable");
        left_panel.appendChild(this.dynamic_controls);

        var temp_tag = document.createElement("hr")
        temp_tag.classList.add("divider")
        left_panel.appendChild(temp_tag);
        
        // ----- Create dynamic data -----
        var temp_tag = document.createElement("h3")
        temp_tag.innerText = "Data";
        left_panel.appendChild(temp_tag);

        this.dynamic_data = document.createElement("div");
        this.dynamic_data.classList.add("scrollable");
        left_panel.appendChild(this.dynamic_data);

        var temp_tag = document.createElement("hr")
        temp_tag.classList.add("divider")
        left_panel.appendChild(temp_tag);

        // ----- Add permanent controls -----
        player.build(this.permanent_controls);

        var temp_tag = document.createElement("hr")
        temp_tag.classList.add("light_divider")
        this.permanent_controls.appendChild(temp_tag);
        let speed_range = new Range({"name":"speed", "type": "Range", "title": "Speed", "min": 1, "max": WidgetManager.MAX_SPEED, "initial": 15})
        speed_range.build(this.permanent_controls);
        //We want the maximum speed to be 50 ms and our default to be 300 ms
        //Every longer delay is in relation to the maximum and default values.
        speed_range.register(value => {player.adjustSpeed(50 * (WidgetManager.MAX_SPEED + 1 - value))})

        temp_tag = document.createElement("hr")
        temp_tag.classList.add("light_divider")
        this.permanent_controls.appendChild(temp_tag);
        let skip_range = new Range({"name":"skip", "type": "Range", "title": "Skip frame", "min": 0, "max": WidgetManager.MAX_SKIP, "initial": 0})
        skip_range.build(this.permanent_controls);
        skip_range.register(value => {
            player.setSkipRate(value);
        });
    }

    add_control(widgetJson) {
        if (!widgetJson.type) {
            reportGuiError(this.constructor.name, `No control type: ${widgetJson}`)
        }
        var widget = null;

        switch (widgetJson.type) {
            case "Range":
                widget = new Range(widgetJson)
                break;
            default:
                reportGuiError(this.constructor.name, `Unknown control type: ${widgetJson.type}`)
        }
        widget.build(this.dynamic_controls);

        //TODO should the socket event occur within the widget?
        //TODO should the json be generated within the widget?
        widget.register(value => {
            this.socket.emit("control_changed", {
                name: widget.widget_name,
                value: value
            });
        });

        return widget;
    }

    add_datapoint(widgetJson) {
        if (!widgetJson.type) {
            reportGuiError(this.constructor.name, `No datapoint type: ${widgetJson}`)
        }
        var widget = null;

        switch (widgetJson.type) {
            case "Threshold":
                widget = new Threshold(widgetJson);
                break;
            case "ScatterPlot":
                widget = new ScatterPlot(widgetJson);
                break;
            case "MultiLinePlot":
                widget = new MultiLinePlot(widgetJson);
                break;
            default:
                reportGuiError(this.constructor.name, `Unknown datapoint type: ${widgetJson.type}`)
        }
        widget.build(this.dynamic_data);
        return widget;
    }
}