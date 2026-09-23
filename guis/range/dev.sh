# The following is a convenience script for running dotnet projects in hot reload mode.  

# Explanation:

# dotnet watch run    - Runs in hot reload mode, so changes to C# or HTML (or even Python, json, etc) auto-refresh the app.
# --no-restore        - Basically stops nuget commands from rebuilding every time, and assumes you're caching locally on your dev machine anyways (default Nuget behavior).  Speeds up hot reload when used with --non-interactive.
# --non-interactive   - By default, Hot Reload likes to prompt you, the dev, when a change is made; this says, "no, go ahead!".  Best used with --no-restore.
# web                 - runs the app as a webapp (duh).
# --debug             - (also, duh)

dotnet watch run web --debug --no-restore --non-interactive

# NOTE: Recommend as a `dev () {...}` function in your ~/.bashrc, for best results.  I keep scripts like these on my box, for speed-coding.