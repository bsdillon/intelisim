from blinker import signal
import inspect

# Define a signal
task_completed = signal("task_completed")


# Define receiver functions
def receiver(sender, **extra):
    func_name = inspect.currentframe().f_code.co_name
    print(f"'{func_name}' received signal from '{sender}' with values '{extra}'")
    return f"Response from '{func_name}'"


# connect the signal
task_completed.connect(receiver)

# iterate 5 times
for i in range(1, 3):
    # Send the signal and capture responses
    responses = task_completed.send("main_process", task_id=i)

    print(f"Iteration {i}")
    # Print the responses
    for receiver, response in responses:
        print(f"{receiver.__name__} returned: {response}")
    print("")