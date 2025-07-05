using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using UnityEngine;

public static class SearchPath
{
    public static (String success, (int x, int y) opened) CheckOpenWall(int[,] maze, (int x, int y) start, (int x, int y) goal, (int x, int y) selected)
    {
        // 現在地からゴールまでのパスをBFSで探索
        var path = BFS(maze, start, goal);
        if (path != null)
        {
            return ("NeedNot", (-1, -1)); // すでにパスがある場合は何も開けない
        }
        return OpenWall(maze, start, goal, selected);

    }
    private static (String success, (int x, int y) opened) OpenWall(int[,] maze, (int x, int y) start, (int x, int y) goal, (int x, int y) selected)
    {
        UnityEngine.Debug.Log($"Start: {start}, maze[{start.x}, {start.y}]={maze[start.x, start.y]}");
        UnityEngine.Debug.Log($"Goal: {goal}, maze[{goal.x}, {goal.y}]={maze[goal.x, goal.y]}");
        if (maze[start.x, start.y] != 0 || maze[goal.x, goal.y] != 0)
        {
            UnityEngine.Debug.LogError("スタートが通路ではない、またはゴールが通路ではない");
            return ("Cannot", (-1, -1));
        }
        // 全ての(x, y)座標をリスト化し、ランダムにシャッフル
        var positions = new List<(int x, int y)>();
        for (int y = 1; y < maze.GetLength(1) - 1; y++)
        {
            for (int x = 1; x < maze.GetLength(0) - 1; x++)
            {
                positions.Add((x, y));
            }
        }
        // シャッフル
        System.Random rng = new System.Random();
        int n = positions.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = positions[k];
            positions[k] = positions[n];
            positions[n] = value;
        }

        // シャッフルした順で探索
        foreach (var (x, y) in positions)
        {
            if (maze[x, y] != 1) continue;
            if (selected.x == x && selected.y == y) continue;

            maze[x, y] = 0; // 仮に開ける
            var testPath = BFS(maze, start, goal);
            if (testPath != null)
            {
                return ("Open", (x, y));
            }
            maze[x, y] = 1; // 戻す
        }
        return ("Cannot", (-1, -1));
    }
    public static List<(int x, int y)> BFS(int[,] maze, (int x, int y) start, (int x, int y) goal)
    {
        var sw = Stopwatch.StartNew();
        var queue = new Queue<(int x, int y, List<(int, int)> path)>();
        var visited = new HashSet<(int, int)>();
        queue.Enqueue((start.x, start.y, new List<(int, int)>()));

        while (queue.Count > 0)
        {
            var (x, y, path) = queue.Dequeue();
            if (visited.Contains((x, y))) continue;
            visited.Add((x, y));

            var newPath = new List<(int, int)>(path) { (x, y) };
            if ((x, y) == goal)
            {
                sw.Stop();
                return newPath;
            }

            foreach (var (nx, ny) in GetNeighbors(maze, x, y))
            {
                if (!visited.Contains((nx, ny)))
                    queue.Enqueue((nx, ny, newPath));
            }
        }
        sw.Stop();
        return null;
    }

    private static IEnumerable<(int x, int y)> GetNeighbors(int[,] maze, int x, int y)
    {
        var dirs = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
        foreach (var (dx, dy) in dirs)
        {
            int nx = x + dx, ny = y + dy;
            if (nx >= 0 && ny >= 0 && nx < maze.GetLength(0) && ny < maze.GetLength(1) && maze[nx, ny] == 0)
            {
                yield return (nx, ny);
            }
        }
    }
}
