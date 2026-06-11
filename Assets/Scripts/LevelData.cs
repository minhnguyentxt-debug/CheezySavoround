using System;
using System.Collections.Generic;

[Serializable]
public class JSONPlateData
{
    public int x;
    public int z;
    public List<string> toppings; // Chuỗi chữ như "Red", "Yellow"
}

[Serializable]
public class LevelData
{
    public int levelNumber;
    public int targetScore;
    public int gridColumns = 0; // 0 = dùng giá trị mặc định của GridManager
    public int gridRows = 0;    // 0 = dùng giá trị mặc định của GridManager
    public List<JSONPlateData> initialPlates;
}