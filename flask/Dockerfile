FROM python:3.11-slim

RUN pip install flask flask_socketio eventlet

EXPOSE 5000

WORKDIR /app

COPY flask/warning.txt /tmp/warning.txt
# default value is empty
ARG FROM_SCRIPT=""

# fail if not built from script
RUN if [ -z "$FROM_SCRIPT" ]; then \
        cat /tmp/warning.txt \
        exit 1; \
    fi

COPY common /app/common
COPY flask/app.py /app/
COPY flask/templates /app/templates/
COPY flask/static /app/static

CMD ["python", "app.py"]