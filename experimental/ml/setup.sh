###
# This script is for setting up runnable Python files on a CachyOs Linux machine.
# It is not intended for use with all Linux distros and can be rewritten.
###

## initial activation (uncomment to use)

# python -m venv .venv
# source .venv/bin/activate

## installs
python -m pip install --upgrade pip
python -m pip install numpy pandas matplotlib scikit-learn jupyter ipykernel

## ensure ipykernel can bridge

python -m ipykernel install --user \
    --name ml \
    --display-name "Python (ML)"



