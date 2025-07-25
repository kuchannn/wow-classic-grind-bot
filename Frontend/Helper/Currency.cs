namespace Frontend;

public readonly struct Currency
{
    public static readonly Currency Empty = new(0);

    public int Gold { get; }
    public int Silver { get; }
    public int Copper { get; }

    public Currency(int amount)
    {
        int sign = amount < 0 ? -1 : 1;

        Gold = 0;
        Silver = 0;
        Copper = Math.Abs(amount);

        if (Copper >= 10000)
        {
            Gold = Copper / 10000 * sign;
            Copper %= 10000;
        }

        if (Copper >= 100)
        {
            Silver = Copper / 100 * sign;
            Copper %= 100;
        }

        Copper *= sign;
    }
}
