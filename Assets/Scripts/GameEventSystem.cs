using System;
using UnityEngine;

public static class GameEventSystem
{
    // Các event cũ của bạn (ví dụ):
    public static Action<ToppingType> OnPizzaCompleted;
    public static Action<int> OnScoreChanged;
    public static Action<int> OnGoldChanged;
    public static Action<int> OnHighScoreChanged;
    public static Action<string> OnSkinSelected;
    public static System.Action<int> OnCoinsChanged;
    public static System.Action<string, int> OnItemAdded;
}