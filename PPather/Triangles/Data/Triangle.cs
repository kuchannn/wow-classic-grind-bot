namespace PPather.Triangles;

public readonly record struct Triangle<T>(T V0, T V1, T V2, TriangleType Flags);
