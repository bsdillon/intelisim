import { reportGuiError, GuiTypeError } from './guierror.js';


export const SERVER_VERSION = "1.2.2";


export const SocketCommand = Object.freeze({
  REGISTRATION: "register_role",
  TEST_GUI: "signals/test_gui",
  ADD_CONTROL: "signals/add_control",
  ADD_DATAPOINT: "signals/add_datapoint",
  UPDATE_DATAPOINTS: "signals/update_datapoints",
  NEW_FRAME: "signals/new_frame",
});


export const SocketSignal = Object.freeze({
  REGISTRATION_RECEIVED: "registration_received",
  TEST_GUI: "test_gui",
  ADD_CONTROL: "add_control",
  CONTROL_CHANGED: "control_changed",
  JAVASCRIPT_ERROR: "javascript_error",
  GUI_ERROR: "gui_error",
  UPDATE_DATAPOINTS: "update_datapoints",
  ADD_DATAPOINT: "add_datapoint",
  START_GUI: "start_gui",
  STEP_FUNCTION: "step_function",
  NEW_FRAME: "new_frame",
  HALT_ALL: "halt_all",
  RUN_INDEFINITE: "run_indefinite",
});


export const CONTROL_TYPES = {
  "Range": ["name", "title", "min", "max", "initial"],
};

export const DATA_TYPES = {
  "Threshold": ["name", "title", "min", "max", "colorScale"],
  "ScatterPlot": ["name", "independentVar", "dependentVar"],
  "MultiLinePlot": ["name", "title", "dependentVars", "maxMemory"],
};


export function validateTypeJSON(json, isControl) { 
  try {
    const widgetType = json["type"];
    if (!widgetType) {
      reportGuiError("validateTypeJSON", "Missing 'type' field in widget definition");
    }

    let collection = DATA_TYPES;
    if (isControl) {
        collection = CONTROL_TYPES
    }

    const required = collection[widgetType];
    if (!required) {
      reportGuiError("validateTypeJSON", `Unknown widget type: ${widgetType}`);
    }

    const missing = required.filter(key => !(key in json));
    if (missing.length > 0) {
      reportGuiError("validateTypeJSON", `Missing required fields for ${widgetType}: ${missing.join(", ")}`);
    }
  } catch (err) {
    if (err instanceof GuiTypeError) {
        // Let GuiType errors propagate
        throw err;
    } else {
        reportGuiError("validateTypeJSON", `Validation error: ${err.message}`);
    }
  }
  return true;
}



export function initializeType(instance, widgetJson, isControl) {
  if (validateTypeJSON(widgetJson, isControl))
  {
    // --- Assign fields to the instance ---
    let collection = DATA_TYPES;
    if (isControl) {
        collection = CONTROL_TYPES
    }

    const widgetType = widgetJson["type"];
    for (const field of collection[widgetType]) {
        let value = widgetJson[field];

        // convert numeric strings to numbers
        if (!isNaN(value) && value !== "" && value !== null) {
        const num = Number(value);
        if (!Number.isNaN(num)) value = num;
        }

        instance[field] = value;
    }
    return; //valid instantiation
  }
  //Probably never reaches this point, but defensively throwing an error
  reportGuiError("initializeType", `Validation error: UNKNOWN`);
}
