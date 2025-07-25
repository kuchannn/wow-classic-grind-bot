namespace Frontend;

public static class ItemPrice
{
    public static Currency ToSellPrice(int sellPrice)
    {
        if (sellPrice == 0)
            return Currency.Empty;

        return new Currency(sellPrice);
    }
}
