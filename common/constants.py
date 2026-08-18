from enum import Enum
from pathlib import Path

SERVER_VERSION = "1.2.2"
DEBUGGING = True

class SocketCommand(Enum):
    REGISTRATION = "register_role"
    TEST_GUI = "signals/test_gui"
    ADD_CONTROL = "signals/add_control"
    ADD_DATAPOINT = "signals/add_datapoint"
    UPDATE_DATAPOINTS = "signals/update_datapoints"
    NEW_FRAME = "signals/new_frame"

class SocketSignal(Enum):
    REGISTRATION_RECEIVED = "registration_received"
    TEST_GUI = "test_gui"
    ADD_CONTROL = "add_control"
    CONTROL_CHANGED = "control_changed"
    JAVASCRIPT_ERROR = "javascript_error"
    GUI_ERROR = "gui_error"
    UPDATE_DATAPOINTS = "update_datapoints"
    ADD_DATAPOINT = "add_datapoint"
    START_GUI = "start_gui"
    STEP_FUNCTION = "step_function"
    NEW_FRAME = "new_frame"
    HALT_ALL = "halt_all"
    RUN_INDEFINITE = "run_indefinite"

def export_enums_to_js(enum_cls):
    lines = [f"export const {enum_cls.__name__} = Object.freeze({{"]
    for member in enum_cls:
        lines.append(f'  {member.name}: "{member.value}",')
    lines.append("});\n")
    return "\n".join(lines)

CONTROL_TYPES = {"Range": ["name","title", "min", "max", "initial"]
                }

DATAPOINT_TYPES = {"Threshold": ["name","title", "min", "max", "colorScale"],
                   "ScatterPlot": ["name","independentVar", "dependentVar"],
                   "MultiLinePlot": ["name", "title", "dependentVars","maxMemory"]
                   }

def export_types_to_js(isControl):
    name = "DATA_TYPES"
    collection = DATAPOINT_TYPES
    if isControl:
        name = "CONTROL_TYPES"
        collection = CONTROL_TYPES

    lines = [f"export const {name}"+" = {"]
    for key, members in collection.items():
        members_js = ", ".join(f'"{m}"' for m in members)
        lines.append(f'  "{key}": [{members_js}],')
    lines.append("};")
    return "\n".join(lines)

def validate_type(json, isControl):
    try:
        widget_type = json.get("type")
        if not widget_type:
            print("Missing 'type' field in widget definition")
            return False

        collection = DATAPOINT_TYPES
        if isControl:
            collection = CONTROL_TYPES

        required = collection.get(widget_type)
        if not required:
            print(f"Unknown widget type: {widget_type}")
            return False

        missing = [key for key in required if key not in json]
        if missing:
            print(f"Missing required fields for {widget_type}: {missing}")
            return False

        return True
    except:
        return False

VALIDATE_CONTROL_JS = """
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
"""

INITIALIZETYPE_JS = """
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
"""

def create_javascript(static_path):
    """Export Python constants/validators to JS in real time."""
    static_file = Path(static_path) / "shared.js"
    lines = []

    # --- Import GUI error reporting ---
    lines.append("import { reportGuiError, GuiTypeError } from './guierror.js';\n")

    # --- Server version ---
    lines.append(f'export const SERVER_VERSION = "{SERVER_VERSION}";\n')

    # --- Enums ---
    for enum_cls in [SocketCommand, SocketSignal]:
        lines.append(export_enums_to_js(enum_cls))

    # --- Control and Data types ---
    lines.append(export_types_to_js(isControl=True))    # CONTROL_TYPES
    lines.append(export_types_to_js(isControl=False))   # DATAPOINT_TYPES

    # --- Validation functions ---
    lines.append(VALIDATE_CONTROL_JS)
    lines.append(INITIALIZETYPE_JS)

    # export_line = (
    #     "export { SERVER_VERSION, SocketCommand, SocketSignal, "
    #     "CONTROL_TYPES, DATA_TYPES, validateTypeJSON, initializeType };"
    # )
    # lines.append(export_line)

    # --- Write all content to file ---
    content = "\n\n".join(lines)
    static_file.write_text(content)
    
    print(f"Generated JS at {static_file}")