import os
import subprocess
import sys

def build_docker_image():
    cwd_orig = os.getcwd()
    cwd = cwd_orig
    dirname = os.path.basename(cwd_orig)

    # If we're in 'flask', move up one directory and recheck
    if dirname == "flask":
        print(f"Starting in the flask directory. Moving...")
        os.chdir("..")
        cwd = os.getcwd()
        dirname = os.path.basename(cwd)

    if dirname != "intelisim":
        print(f"Error: You must run this script from the 'intelisim' directory (current: '{cwd_orig}')")
        sys.exit(1)

    print(f"Building Docker image from directory: {cwd}")
    try:
        subprocess.run(
                ["podman", "build", "--build-arg", "FROM_SCRIPT=1", "-t", "flask_server", "-f", "/flask/Dockerfile", "."],
                check=True
            )
    except subprocess.CalledProcessError as e:
        print(f"Docker build failed: {e}")
        sys.exit(e.returncode)

    # Run the container, mapping port 5000
    print(f"Running Podman container from image: flask_server")
    try:
        subprocess.run(
            ["podman", "run", "-p", "5000:5000", "flask_server"],
            check=True
        )
    except KeyboardInterrupt:
        print("Container run interrupted by user (CTRL+C), exiting gracefully")
    except subprocess.CalledProcessError as e:
        print(f"Podman run failed: {e}")
        sys.exit(e.returncode)

if __name__ == "__main__":
    build_docker_image()