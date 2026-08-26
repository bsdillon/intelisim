#!/usr/bin/env bash
set -euo pipefail

# This file is meant to bootstrap python packages on Linux (or Linux-tollerant WSL2) machines for quick, ad-hoc
# development and testing, and is by no means meant to replace our existing Dockerfile or images.
# (It can be used for exploration or as a fallback, if Dockerfile fails to build for whatever reason).


ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
VENV="$ROOT/.venv"

echo "==> Intelisim Python environment"

if [[ ! -d "$VENV" ]]; then
    echo "Creating virtual environment..."
    python -m venv "$VENV"
fi

PYTHON="$VENV/bin/python"
PIP="$PYTHON -m pip"

echo "==> Installing Python dependencies..."

$PIP install --upgrade pip

$PIP install \
    mesa \
    flask \
    flask-socketio \
    eventlet \
    python-socketio \
    requests \
    python-dispatch \
    structlog




#python -m venv .venv
#source .venv/bin/activate
#
## Install the requisite packages for agent work:
#pip install mesa network
