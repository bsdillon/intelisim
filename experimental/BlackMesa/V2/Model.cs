namespace BlackMesa.V2;

public abstract class Model
{
    public Random Random { get; private set; } = Random.Shared;

    public int Tick { get; protected internal set; }

    public void SetSeed(int seed)
    {
        Random = new Random(seed);
    }

    public virtual void Step()
    {
        Tick++;
    }
}

// public abstract class Model(int? seed = null)
// {
//     public Random Random { get; } =
//         seed.HasValue
//             ? new Random(seed.Value)
//             : Random.Shared;
//
//     public int Tick { get; protected internal set; }
//
//     public virtual void Step()
//     {
//         Tick++;
//     }
// }