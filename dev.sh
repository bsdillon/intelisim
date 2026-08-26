#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

IMAGE="intelisim-gui"
CONTAINER="intelisim-gui"
SESSION_NAME="intelisim"
PORT=5001
REBUILD=false


usage() {
    cat <<EOF
Usage:
  ./dev.sh [options]

Options:
  --rebuild       Rebuild the Podman image
  --port PORT     Host port for the GUI (default: 5000)
  -h, --help      Show this help
EOF
}


while [[ $# -gt 0 ]]; do
    case "$1" in
        --rebuild)
            REBUILD=true
            shift
            ;;

        --port)
            [[ $# -ge 2 ]] || {
                echo "ERROR: --port requires a value." >&2
                exit 1
            }

            PORT="$2"
            shift 2
            ;;

        -h|--help)
            usage
            exit 0
            ;;

        *)
            echo "ERROR: Unknown option: $1" >&2
            usage >&2
            exit 1
            ;;
    esac
done


# ------------------------------------------------------------
# Validation
# ------------------------------------------------------------

if ! command -v podman >/dev/null 2>&1; then
    echo "ERROR: podman is not installed or not in PATH." >&2
    exit 1
fi

if ! [[ "$PORT" =~ ^[0-9]+$ ]] || (( PORT < 1 || PORT > 65535 )); then
    echo "ERROR: Invalid port: $PORT" >&2
    exit 1
fi


# ------------------------------------------------------------
# Port check
# ------------------------------------------------------------

if command -v ss >/dev/null 2>&1; then
    if ss -ltnH | awk -v p=":$PORT" '$4 ~ p"$/" || $4 ~ p"$" { found=1 } END { exit found ? 0 : 1 }'; then
        cat >&2 <<EOF

ERROR: Port $PORT is already in use.

Try another port, for example:

    ./dev.sh --port 5001

EOF
        exit 1
    fi
fi


# ------------------------------------------------------------
# Build
# ------------------------------------------------------------

if [[ "$REBUILD" == true ]] || ! podman image exists "$IMAGE"; then
    echo "==> Building $IMAGE..."

    podman build \
        --tag "$IMAGE" \
        "$ROOT"
fi


# ------------------------------------------------------------
# Configuration
# ------------------------------------------------------------

GUI_URL="http://127.0.0.1:$PORT"

echo
echo "==> Intelisim GUI"
echo "    image:     $IMAGE"
echo "    container: $CONTAINER"
echo "    port:      $PORT"
echo "    URL:       $GUI_URL"
echo "    rebuild:   $REBUILD"
echo


# ------------------------------------------------------------
# Simulation selection
# ------------------------------------------------------------

find_simulations() {
    find "$ROOT" \
        -mindepth 2 \
        -type f \
        -name "dev.sh" \
        -print \
        | sort
}


choose_simulation() {
    local scripts=()
    local script
    local choice

    while IFS= read -r script; do
        scripts+=("$script")
    done < <(find_simulations)

    if (( ${#scripts[@]} == 0 )); then
        echo "No simulation dev.sh files found."
        return 1
    fi

    echo
    echo "Available simulations:"
    echo

    local i=1

    for script in "${scripts[@]}"; do
        local relative
        relative="${script#"$ROOT"/}"
        local simulation
        simulation="$(dirname "$relative")"

        printf "  %d) %s\n" "$i" "$simulation"

        ((i++))
    done

    echo

    while true; do
        read -rp "Which simulation should I run? [1-${#scripts[@]}]: " choice

        if [[ "$choice" =~ ^[0-9]+$ ]] &&
           (( choice >= 1 && choice <= ${#scripts[@]} )); then
            SELECTED_SIMULATION="${scripts[$((choice-1))]}"
            return 0
        fi

        echo "Please enter a number between 1 and ${#scripts[@]}."
    done
}



run_simulation() {
    choose_simulation || return 1

    local relative
    local simulation
    local python

    relative="${SELECTED_SIMULATION#"$ROOT"/}"
    simulation="$(dirname "$relative")"
    python="$ROOT/.venv/bin/python"

    echo
    echo "==> Starting simulation:"
    echo "    $simulation"
    echo

    if [[ ! -x "$python" ]]; then
        echo "ERROR: Python virtual environment not found:"
        echo "    $python"
        echo
        echo "Run:"
        echo "    ./packages.sh"
        return 1
    fi

    cd "$ROOT"

    "$python" -m "$simulation.model"
}

#run_simulation() {
#    choose_simulation || return 1
#
#    echo
#    echo "==> Starting simulation:"
#    echo "    ${SELECTED_SIMULATION#"$ROOT"/}"
#    echo
#
#    cd "$(dirname "$SELECTED_SIMULATION")"
#
#    ./dev.sh
#}
#

# ------------------------------------------------------------
# tmux
# ------------------------------------------------------------

run_tmux_session() {

    # Check if the session already exists.
    if tmux has-session -t "$SESSION_NAME" 2>/dev/null; then
        echo "Tmux session '$SESSION_NAME' already exists. Attaching to it."
        tmux attach-session -t "$SESSION_NAME"
        return
    fi

    # GUI pane
    GUI_SCRIPT="
        cd '$ROOT'
        podman rm -f '$CONTAINER' >/dev/null 2>&1 || true
        podman run --rm \
            --name '$CONTAINER' \
            -p '$PORT:5000' \
            '$IMAGE'
    "

    # Simulation pane
    SIM_SCRIPT="
        export INTELISIM_GUI_URL='$GUI_URL'
        cd '$ROOT'
        $(declare -f choose_simulation find_simulations run_simulation)
        run_simulation
    "

    # Create GUI pane.
    tmux new-session \
        -d \
        -s "$SESSION_NAME" \
        "bash -lc $(printf '%q' "$GUI_SCRIPT")"

    # Create simulation pane.
    tmux split-window \
        -h \
        -t "$SESSION_NAME" \
        "bash -lc $(printf '%q' "$SIM_SCRIPT")"

    tmux select-pane -t "$SESSION_NAME":0.1

    tmux attach-session -t "$SESSION_NAME"
}


# ------------------------------------------------------------
# Run
# ------------------------------------------------------------

if command -v tmux >/dev/null 2>&1; then
    run_tmux_session
else
    echo "==> tmux not found; running GUI in current shell."
    echo

    podman rm -f "$CONTAINER" >/dev/null 2>&1 || true

    export INTELISIM_GUI_URL="$GUI_URL"

    podman run --rm \
        --name "$CONTAINER" \
        -p "$PORT:5000" \
        "$IMAGE"
fi