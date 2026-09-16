namespace BlackMesa.V2;

public abstract class Model(int? seed = null)
{
    public Random Random { get; } =
        seed.HasValue
            ? new Random(seed.Value)
            : Random.Shared;

    public int Tick { get; protected internal set; }

    public virtual void Step()
    {
        Tick++;
    }
}