#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

IMAGE="intelisim-gui"
CONTAINER="intelisim-gui"
PORT=5000
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

if ! command -v podman >/dev/null 2>&1; then
    echo "ERROR: podman is not installed or not in PATH." >&2
    exit 1
fi

if ! [[ "$PORT" =~ ^[0-9]+$ ]] || (( PORT < 1 || PORT > 65535 )); then
    echo "ERROR: Invalid port: $PORT" >&2
    exit 1
fi

if [[ "$REBUILD" == true ]] || ! podman image exists "$IMAGE"; then
    echo "==> Building $IMAGE..."

    podman build \
        --tag "$IMAGE" \
        "$ROOT"
fi

echo "==> Starting $IMAGE on port $PORT..."

# Remove stale container if one exists.
podman rm -f "$CONTAINER" >/dev/null 2>&1 || true

RUN_ARGS=(
    --name "$CONTAINER"
    -p "$PORT:5000"
    -e "INTELISIM_GUI_URL=http://127.0.0.1:$PORT"
)

run_gui() {
    podman run --rm "${RUN_ARGS[@]}" "$IMAGE"
}

if command -v tmux >/dev/null 2>&1; then
    echo "==> Starting GUI in tmux..."

    tmux new-session \
        -A \
        -s intelisim \
        "cd '$ROOT' && podman run --rm ${RUN_ARGS[*]} '$IMAGE'"
else
    echo "==> tmux not found; running in current shell."
    run_gui
fi

## DEBUG:
echo "==> Intelisim GUI"
echo "    image:     $IMAGE"
echo "    port:      $PORT"
echo "    URL:       http://127.0.0.1:$PORT"
echo "    rebuild:   $REBUILD"