using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace WowheadDB_Extractor
{
    public class GeneticTSPSolver
    {
        private readonly struct Score
        {
            public readonly int Position { get; init; }
            public readonly int Value { get; init; }
        }

        private static readonly Random random = new();

        private const float DRAW_OFFSET = 30;
        private const int DRAW_SIZE = 1000;
        private const float DRAW_SCALE = 10;

        private Vector2[] points;
        public int Length => points.Length;

        private const int POPULATION_SIZE = 30;
        private const float CROSSOVER_PROBABILITY = 0.9f;
        private const float MUTATION_PROBABILITY = 0.01f;

        public int CurrentGen { get; private set; }
        public int UnchangedGens { get; private set; }
        public int Mutations { get; private set; }
        public int BestValue { get; private set; } = int.MaxValue;

        private Score currentBest;

        private int[][] distances;
        private int[] best;
        private int[][] population = new int[POPULATION_SIZE][];
        private int[] evolution = new int[POPULATION_SIZE];
        private float[] fitness = new float[POPULATION_SIZE];
        private float[] roulette = new float[POPULATION_SIZE];

        public Vector2[] Result
        {
            get
            {
                Vector2[] result = new Vector2[Length];
                for (int i = 0; i < best.Length; i++)
                {
                    result[i] = points[best[i]];
                }
                return result;
            }
        }

        private GeneticTSPSolver() { }

        public GeneticTSPSolver(IEnumerable<Vector2> points)
        {
            SetPoints(points);
            Initialize();
        }

        public GeneticTSPSolver(int number)
        {
            SetRandomPoints(number);
            Initialize();
        }

        private void Initialize()
        {
            population = new int[POPULATION_SIZE][];
            evolution = new int[POPULATION_SIZE];
            fitness = new float[POPULATION_SIZE];
            roulette = new float[POPULATION_SIZE];

            CalculateDistances();
            for (int i = 0; i < POPULATION_SIZE; i++)
            {
                population[i] = RandomIndivials(points.Length);
            }
            DetermineChange();
        }

        public void SetRandomPoints(int number)
        {
            points = new Vector2[number];
            for (int i = 0; i < number; i++)
            {
                points[i] = RandomPoint();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPoints(IEnumerable<Vector2> points)
        {
            this.points = points.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 RandomPoint()
        {
            float randomx = DRAW_OFFSET + random.Next((int)(DRAW_SIZE - 2 * DRAW_OFFSET));
            float randomy = DRAW_OFFSET + random.Next((int)(DRAW_SIZE - 2 * DRAW_OFFSET));
            return new Vector2(randomx / DRAW_SCALE, randomy / DRAW_SCALE);
        }

        private void CalculateDistances()
        {
            distances = new int[points.Length][];
            for (int i = 0; i < points.Length; i++)
            {
                distances[i] = new int[points.Length];
                for (int j = 0; j < points.Length; j++)
                {
                    distances[i][j] = Distance(points[i], points[j]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Distance(Vector2 p1, Vector2 p2)
        {
            return EuclideanDistance((p1.X - p2.X) * 10, (p1.Y - p2.Y) * 10);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int EuclideanDistance(float dx, float dy)
        {
            return (int)MathF.Sqrt(dx * dx + dy * dy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] RandomIndivials(int n)
        {
            int[] random = new int[n];
            for (int i = 0; i < n; i++)
            {
                random[i] = i;
            }
            return Shuffle(random);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int[] Shuffle(int[] a)
        {
            for (int j, x, i = a.Length - 1; i > 0; j = random.Next(i), x = a[--i], a[i] = a[j], a[j] = x) ;
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Next(List<int> a, int index)
        {
            return index == a.Count - 1 ? a[0] : a[index + 1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Previous(List<int> a, int index)
        {
            return index == 0 ? a[^1] : a[index - 1];
        }

        private void DetermineChange()
        {
            for (int i = 0; i < population.Length; i++)
            {
                evolution[i] = Evaluate(population[i]);
            }
            currentBest = CurrentBest();
            if (BestValue > currentBest.Value)
            {
                best = population[currentBest.Position].Clone() as int[];
                BestValue = currentBest.Value;
                UnchangedGens = 0;
            }
            else
            {
                UnchangedGens += 1;
            }
        }

        private int Evaluate(int[] indivial)
        {
            int sum = distances[indivial[0]][indivial[^1]];
            for (int i = 1; i < indivial.Length; i++)
            {
                sum += distances[indivial[i]][indivial[i - 1]];
            }
            return sum;
        }

        private Score CurrentBest()
        {
            int bestPos = 0;
            int bestValue = evolution[0];

            for (int i = 1; i < population.Length; i++)
            {
                if (evolution[i] < bestValue)
                {
                    bestValue = evolution[i];
                    bestPos = i;
                }
            }

            return new Score { Position = bestPos, Value = bestValue };
        }

        public void Evolve()
        {
            CurrentGen++;
            Selection();
            Crossover();
            Mutation();
            DetermineChange();
        }

        private void Selection()
        {
            int[][] parents = new int[POPULATION_SIZE][];

            int baseSize = 0;
            parents[baseSize++] = population[currentBest.Position];
            parents[baseSize++] = SwapMutate(best.Clone() as int[]);
            parents[baseSize++] = ReorderMutate(best.Clone() as int[]);
            parents[baseSize++] = best.Clone() as int[];

            Roulette();
            for (int i = baseSize; i < POPULATION_SIZE; i++)
            {
                parents[i] = population[WheelOut(random.NextSingle())];
            }

            population = parents;
        }

        private int[] SwapMutate(int[] seq)
        {
            Mutations++;
            int n;
            int m;
            // m and n refers to the actual index in the array
            // m range from 0 to length-2, n range from 2...length-m
            do
            {
                m = random.Next(seq.Length - 2);
                n = random.Next(seq.Length);
            } while (m >= n);

            for (int i = 0, j = (n - m + 1) >> 1; i < j; i++)
            {
                (seq[m + i], seq[n - i]) = (seq[n - i], seq[m + i]);
            }
            return seq;
        }

        private int[] ReorderMutate(int[] seq)
        {
            Mutations++;
            int m, n;
            do
            {
                m = random.Next(seq.Length >> 1);
                n = random.Next(seq.Length);
            } while (m >= n);

            return seq[m..n].Concat(seq[0..m]).Concat(seq[n..]).ToArray();
        }

        private void Roulette()
        {
            for (int i = 0; i < evolution.Length; i++) { fitness[i] = 1.0f / evolution[i]; }

            float sum = 0;
            for (int i = 0; i < fitness.Length; i++) { sum += fitness[i]; }
            for (int i = 0; i < roulette.Length; i++) { roulette[i] = fitness[i] / sum; }
            for (int i = 1; i < roulette.Length; i++) { roulette[i] += roulette[i - 1]; }
        }

        private int WheelOut(float value)
        {
            for (int i = 0; i < roulette.Length; i++)
            {
                if (value <= roulette[i])
                {
                    return i;
                }
            }
            return random.Next(roulette.Length);
        }

        private void Crossover()
        {
            int[] queue = new int[POPULATION_SIZE];
            for (int i = 0; i < POPULATION_SIZE; i++)
            {
                if (random.NextSingle() < CROSSOVER_PROBABILITY)
                {
                    queue[i] = i;
                }
            }

            queue = Shuffle(queue);
            for (int i = 0, j = queue.Length - 1; i < j; i += 2)
            {
                DoCrossover(queue[i], queue[i + 1]);
            }
        }

        private void DoCrossover(int x, int y)
        {
            population[x] = GetChild(Next, x, y);
            population[y] = GetChild(Previous, x, y);
        }

        private int[] GetChild(Func<List<int>, int, int> fun, int x, int y)
        {
            int[] solution = new int[points.Length];
            int index = 0;
            List<int> px = population[x].ToList();
            List<int> py = population[y].ToList();
            int dx, dy;
            int c = px[random.Next(px.Count)];
            solution[index++] = c;
            while (px.Count > 1)
            {
                dx = fun(px, px.IndexOf(c));
                dy = fun(py, py.IndexOf(c));

                px.Remove(c);
                py.Remove(c);

                c = distances[c][dx] < distances[c][dy] ? dx : dy;
                solution[index++] = c;
            }
            return solution;
        }

        private void Mutation()
        {
            for (int i = 0; i < POPULATION_SIZE; i++)
            {
                if (random.NextSingle() < MUTATION_PROBABILITY)
                {
                    if (random.NextSingle() > 0.5f)
                    {
                        population[i] = ReorderMutate(population[i]);
                    }
                    else
                    {
                        population[i] = SwapMutate(population[i]);
                    }
                    i--;
                }
            }
        }

        public void Draw(string file = "graph.bmp")
        {
            Bitmap bitmap = new(DRAW_SIZE, DRAW_SIZE);
            Graphics g = Graphics.FromImage(bitmap);

            for (int i = 0; i < best.Length; i++)
            {
                int prev = i - 1;
                var p1 = points[best[prev < 0 ? ^1 : prev]];
                var p2 = points[best[i]];

                g.FillEllipse(Brushes.White, (int)(p1.X * DRAW_SCALE), (int)(p1.Y * DRAW_SCALE), DRAW_SCALE / 2, DRAW_SCALE / 2);
                g.DrawLine(Pens.Red, (int)(p1.X * DRAW_SCALE), (int)(p1.Y * DRAW_SCALE), (int)(p2.X * DRAW_SCALE), (int)(p2.Y * DRAW_SCALE));
            }

            g.Dispose();
            bitmap.Save(file, System.Drawing.Imaging.ImageFormat.Bmp);
        }
    }
}
