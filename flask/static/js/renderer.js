import {reportGuiError} from './guierror.js';

export class Renderer {
    constructor(canvas, grid_x, grid_y, base_color) {
        this.canvas = canvas;
        this.ctx = canvas.getContext("2d");
        this.grid_x = grid_x;
        this.grid_y = grid_y;
        this.base_color = base_color;
        this.blockSize = 0;

        // Skip-frame logic
        this.skipRate = 0;  // equivalent of skip_rate
        this.skipped = 0;
    }

    calculateBlockSize(grid_width, grid_height, width, height) {
        // size of one square in the grid
        this.blockSize = Math.min(
            width / grid_width,
            height / grid_height
        );
    }

    // Decide if we should draw this frame
    drawFrame(list_of_drawables) {
        if (this.skipped < this.skipRate) {
        this.skipped += 1;
        return false;
        }
        this.skipped = 0;

        const ctx = this.ctx;
        ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

        for (let widget of list_of_drawables)
        {
            this.draw(widget);
        }
    }

    draw(details)
    {
        let object_type = details["type"]
        switch (object_type) {
            case "Circle":
                this.drawCircle(parseFloat(details["x1"]), parseFloat(details["y1"]), parseFloat(details["x2"]), parseFloat(details["y2"]), details["color"], details["outline"]);
                break;
            case "Square":
                this.drawSquare(parseFloat(details["x1"]), parseFloat(details["y1"]), parseFloat(details["x2"]), parseFloat(details["y2"]), details["color"], details["outline"]);
                break;
            case "Pie":
                this.drawPie(parseFloat(details["x1"]), parseFloat(details["y1"]), parseFloat(details["x2"]), parseFloat(details["y2"]), parseFloat(details["arc_start"]), parseFloat(details["arc_length"]), details["color"], details["outline"]);
                break;
            case "Line":
                this.drawLine(parseFloat(details["x1"]), parseFloat(details["y1"]), parseFloat(details["x2"]), parseFloat(details["y2"]), details["color"], details["width"]);
                break;
            case "Polygon":
                this.drawPolygon(details["points"].map(pair => pair.map(coord => parseFloat(coord))), details["color"], details["outline"]);
                break;
            case "Text":
                this.drawText(details["text"], parseFloat(details["x1"]), parseFloat(details["y1"]), details["color"], details["angle"], details["align"], details["size"]);
                break;
            default:
                reportGuiError(this, `Unknown drawable object: ${object_type}`);
        }
    }

    _test() {
        this.drawSquare(0, 0, 1, 1, "#FF0000");
        this.drawCircle(1, 0, 2, 1, "red");
        this.drawCircle(1.1, 0.1, 1.9, 0.9, "white");
        this.drawCircle(1.2, 0.2, 1.8, 0.8, "red");
        this.drawCircle(1.3, 0.3, 1.7, 0.7, "white");
        this.drawCircle(1.4, 0.4, 1.6, 0.6, "red");
        this.drawPie(0, 1, 1, 2, 100, 340, "black", "#00FF00");
        this.drawPie(1, 1, 2, 2, 10, 340, "#FFFF00");
        this.drawPie(2, 1, 3, 2, -30, 60, "#00FFFF");
        this.drawLine(0, 2, 1, 3, "white");
        this.drawPolygon([[2.5, 0], [3, 1], [2.5, 0.7], [2, 1]], "#E9BF13", "#767812");
        this.drawText("Hello world!", 2, 2.5, "#FFFFFF", 25, "center", 16);

        const xStart = 3;
        const yStart = 3;
        const xEnd = 15;
        const yEnd = 15;
        const squares = 100; // 100x100 grid
        const cellSizeX = (xEnd - xStart) / squares;
        const cellSizeY = (yEnd - yStart) / squares;
        let toggle = false;
        let count = 0;
        const maxFlips = 40; // 40 * 250ms = 10 seconds

        const flicker = setInterval(() => {
            toggle = !toggle;
            let evenColor = toggle ? "black" : "white";
            let oddColor = toggle ? "white" : "black";
            count++;

            for (let i = 0; i < squares; i++) {
                for (let j = 0; j < squares; j++) {
                    const isEven = (i + j) % 2 === 0;
                    const color = isEven ? evenColor : oddColor
                    const x1 = xStart + i * cellSizeX;
                    const y1 = yStart + j * cellSizeY;
                    const x2 = x1 + cellSizeX;
                    const y2 = y1 + cellSizeY;
                    this.drawSquare(x1, y1, x2, y2, color);
                }
            }

            if (count >= maxFlips) {
                //end test
                clearInterval(flicker);
            }
        }, 100);
    }

    other() {
        const block = this.blockSize;

        const x1 = shape.upperLeft[0] * this.blockSize;
        const y1 = shape.upperLeft[1] * this.blockSize;
        const x2 = shape.lowerRight[0] * this.blockSize;
        const y2 = shape.lowerRight[1] * this.blockSize;

    }

    drawCircle(x1, y1, x2, y2, fill, outline) {
        const block = this.blockSize;
        const left = x1 * this.blockSize;
        const top = y1 * this.blockSize;
        const right = x2 * this.blockSize;
        const bottom = y2 * this.blockSize;

        const centerX = (left + right) / 2;
        const centerY = (top + bottom) / 2;
        const radiusX = (right - left) / 2;
        const radiusY = (bottom - top) / 2;
        const radius = Math.min(radiusX, radiusY);

        this.ctx.beginPath();
        this.ctx.arc(centerX, centerY, radius, 0, Math.PI * 2);
        this.ctx.fillStyle = fill || "white";
        this.ctx.fill();

        this.ctx.strokeStyle = outline || "black";
        this.ctx.stroke();
    }

    drawSquare(x1, y1, x2, y2, fill, outline) {
        const block = this.blockSize;
        const left = x1 * this.blockSize;
        const top = y1 * this.blockSize;
        const right = x2 * this.blockSize;
        const bottom = y2 * this.blockSize;

        const width = right - left;
        const height = bottom - top;

        this.ctx.fillStyle = fill || "white";
        this.ctx.fillRect(left, top, width, height);

        this.ctx.strokeStyle = outline || "black";
        this.ctx.strokeRect(left, top, width, height);
    }

    drawPie(x1, y1, x2, y2, angleStart, angleLength, fill, outline) {
        const block = this.blockSize;
        const left = x1 * this.blockSize;
        const top = y1 * this.blockSize;
        const right = x2 * this.blockSize;
        const bottom = y2 * this.blockSize;

        const centerX = (left + right) / 2;
        const centerY = (top + bottom) / 2;
        const radiusX = (right - left) / 2;
        const radiusY = (bottom - top) / 2;
        const radius = Math.min(radiusX, radiusY);

        const startRad = (angleStart * Math.PI) / 180;
        const endRad = ((angleStart + angleLength) * Math.PI) / 180;

        this.ctx.beginPath();
        this.ctx.moveTo(centerX, centerY);
        this.ctx.arc(centerX, centerY, radius, startRad, endRad);
        this.ctx.closePath(); // closes path back to center

        this.ctx.fillStyle = fill || "white";
        this.ctx.fill();

        this.ctx.strokeStyle = outline || "black";
        this.ctx.stroke();
    }

    drawPolygon(points, fill, outline) {
        const block = this.blockSize;

        if (!points || points.length < 2) return; // need at least 2 points

        this.ctx.beginPath();
        this.ctx.moveTo(points[0][0] * block, points[0][1] * block);

        for (let i = 1; i < points.length; i++) {
            this.ctx.lineTo(points[i][0] * block, points[i][1] * block);
        }

        this.ctx.closePath(); // closes back to the first point

        this.ctx.fillStyle = fill || "white";
        this.ctx.fill();

        this.ctx.strokeStyle = outline || "black";
        this.ctx.stroke();
    }

    drawLine(x1, y1, x2, y2, outline, lineWidth = 1) {
        const block = this.blockSize;

        this.ctx.beginPath();
        this.ctx.moveTo(x1 * block, y1 * block);
        this.ctx.lineTo(x2 * block, y2 * block);
        this.ctx.lineWidth = lineWidth;

        this.ctx.strokeStyle = outline || "black";
        this.ctx.stroke();
    }

    drawText(text, x, y, color, angle = 0, align, size = 12) {
        const block = this.blockSize;

        this.ctx.save(); // save current state

        // Move to text position
        const px = x * block;
        const py = y * block;
        this.ctx.translate(px, py);

        // Rotate (angle in degrees -> radians)
        this.ctx.rotate((angle * Math.PI) / 180);

        // Set font and alignment
        this.ctx.fillStyle = color || "white";
        this.ctx.font = `${size}px Arial`;
        this.ctx.textAlign = align || "left"; // "left", "center", "right"
        this.ctx.textBaseline = "middle"; // vertical alignment

        // Draw text at origin (because we translated)
        this.ctx.fillText(text, 0, 0);

        this.ctx.restore(); // restore state so rotation doesn't affect other drawings
    }

    // Optional: update canvas size and redraw
    resize(width, height) {
        this.canvas.width = width;
        this.canvas.height = height;
        this.draw();
    }
}