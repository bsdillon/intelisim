import { io } from "https://cdn.socket.io/4.6.1/socket.io.esm.min.js"; // ESM version
import {SocketSignal} from "./shared.js"

const socket = io();

export class GuiTypeError extends Error {
    constructor(source, message) {
        super(`${source}: ${message}`);
        this.name = "GuiTypeError";
        this.source = source;
    }
}

export function reportGuiError(source, message) {
    socket.emit(SocketSignal.GUI_ERROR, { src: source, msg: message});
    throw new GuiTypeError(source, message);
}



