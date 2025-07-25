/*
 *  Part of PPather
 *  Copyright Pontus Borg 2008
 *
 */

namespace WowTriangles;

public sealed class ChunkEventArgs
{
    public int GridX { get; }
    public int GridY { get; }

    public ChunkEventArgs(int gridX, int gridY)
    {
        GridX = gridX;
        GridY = gridY;
    }
}