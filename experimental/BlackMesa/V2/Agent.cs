namespace BlackMesa.V2;

public abstract class Agent<TModel>(TModel model)
{
    public TModel Model { get; } = model;

    public virtual void Step()
    {
    }
}