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
    public static System.Action OnLevelComplete; // Kích hoạt khi đạt đủ targetScore của màn
    public static System.Action<int> OnLevelChanged; // Kích hoạt khi chuyển sang màn mới (truyền số màn)
    public static System.Action<int> OnLevelTargetChanged; // Kích hoạt khi load màn mới (truyền targetScore)
}