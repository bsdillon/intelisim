using BlackMesa.V2;

namespace BlackMesa.V2;

public sealed class RandomScheduler<TAgent>(
    Random random) : Scheduler<TAgent>
{
    public override void Step(Action<TAgent> action)
    {
        for (var i = Agents.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);

            (Agents[i], Agents[j]) =
                (Agents[j], Agents[i]);
        }

        base.Step(action);
    }
}