namespace BlackMesa.V2;
public abstract class Simulation
{
    public int Seed { get; private set; }
    public int Tick { get; private set; }

    public void Run(int seed, int ticks)
    {
        Seed = seed;
        Tick = 0;

        Initialize();

        for (; Tick < ticks; Tick++)
        {
            BeforeStep();
            Step();
            AfterStep();
        }

        Complete();
    }

    protected virtual void Initialize() { }

    protected abstract void Step();

    protected virtual void BeforeStep() { }

    protected virtual void AfterStep() { }

    protected virtual void Complete() { }
}

//
// public abstract class Simulation
// {
//     public int Tick { get; private set; }
//     public int Seed { get; private set; }
//
//     public void Run(int seed, int ticks)
//     {
//         Seed = seed;
//         Tick = 0;
//
//         Initialize();
//
//         for (; Tick < ticks; Tick++)
//         {
//             BeforeStep();
//             Step();
//             AfterStep();
//         }
//
//         Complete();
//     }
//
//
//     protected virtual void Initialize()
//     {
//     }
//
//     protected abstract void Step();
//
//     protected virtual void BeforeStep()
//     {
//     }
//
//     protected virtual void AfterStep()
//     {
//     }
//
//     protected virtual void Complete()
//     {
//     }
// }