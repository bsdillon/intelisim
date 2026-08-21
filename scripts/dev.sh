
#!/usr/bin/env bash
set -e

# Always activate the venv first
source .venv/bin/activate

# Make sure we are at the project root (intelisim/)
# so that PYTHONPATH and relative imports work
cd "$(dirname "$0")"

export PYTHONPATH=.
python flask/app.py

#PYTHONPATH=. python flask/app.py
#python flask/app.py
