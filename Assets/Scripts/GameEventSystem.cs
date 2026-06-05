using System;

public static class GameEventSystem
{
    public static Action<ToppingType> OnPizzaCompleted;

    public static Action<int> OnScoreChanged;

    public static Action<int> OnGoldChanged;
}