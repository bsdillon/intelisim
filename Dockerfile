FROM python:3.11-slim

WORKDIR /app

RUN pip install --no-cache-dir \
    flask \
    flask-socketio \
    eventlet \
    python-socketio \
    requests \
    python-dispatch

COPY common ./common
COPY flask/app.py ./app.py
COPY flask/templates ./templates
COPY flask/static ./static

EXPOSE 5000

CMD ["python", "app.py"]
