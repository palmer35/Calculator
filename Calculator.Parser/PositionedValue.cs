using Sprache;

namespace Calculator.Parser;

public class PositionedValue<T> : IPositionAware<PositionedValue<T>>
{
    public T Value { get; }
    public int Position { get; private set; }

    public PositionedValue(T value)
    {
        Value = value;
    }

    public PositionedValue<T> SetPos(Position startPos, int length)
    {
        Position = startPos.Pos;
        return this;
    }
}