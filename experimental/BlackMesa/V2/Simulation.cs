namespace BlackMesa.V2;

public abstract class Simulation
{
    public int Tick { get; private set; }

    public void Run(int ticks)
    {
        Initialize();

        for (Tick = 0; Tick < ticks; Tick++)
        {
            BeforeStep();
            Step();
            AfterStep();
        }

        Complete();
    }

    protected virtual void Initialize()
    {
    }

    protected abstract void Step();

    protected virtual void BeforeStep()
    {
    }

    protected virtual void AfterStep()
    {
    }

    protected virtual void Complete()
    {
    }
}