cd ../../

source .venv/bin/activate

# Confirm you are really inside the venv
which python
# → should point to .../intelisim/.venv/bin/python

# Confirm Flask is there
python -c "import flask; print(flask.__file__); print(flask.__version__)"

# Look for name-shadowing
ls -la flask/          # is there a flask.py or __init__.py that could be confusing things?
find . -name "flask.py" -o -name "flask"
