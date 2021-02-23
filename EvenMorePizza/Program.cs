using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace EvenMorePizza
{
    class Program
    {
        static string[] inputFiles = {
            @"c:\temp\hashcode\a_example.in",
            @"c:\temp\hashcode\b_little_bit_of_everything.in",
            @"c:\temp\hashcode\c_many_ingredients.in",
            @"c:\temp\hashcode\d_many_pizzas.in",
            @"c:\temp\hashcode\e_many_teams.in"
        };

        static void Main(string[] args)
        {
            DateTime startTime = DateTime.Now;

            foreach (string fileName in inputFiles)
            {
                DateTime solveStartTime = DateTime.Now;
                Solve(fileName);
                Console.WriteLine("Solve time: {0}", new TimeSpan(DateTime.Now.Ticks - solveStartTime.Ticks));
            }

            Console.WriteLine("Runtime: {0}", new TimeSpan(DateTime.Now.Ticks - startTime.Ticks));
        }

        static void Solve(string fileName)
        {
            // Load problem
            Problem p = Problem.LoadProblem(fileName);
            Console.WriteLine("*****************");
            Console.WriteLine("{0}, Teams2: {1}, Teams3: {2}, Teams4: {3}, Pizzas: {4}, Ingredients: {5}, Ingredients/Pizza: {6}",
                fileName, p.Teams2, p.Teams3, p.Teams4, p.Pizzas.Count, p.IngredientCount, (float)p.GetTotalIngredients()/p.Pizzas.Count);

            // Build solution
            Solution solution = BuildSolution(p);
            OptimizeSolution(solution);

            // Write solution to output file
            using (StreamWriter sw = new StreamWriter(fileName + ".out"))
            {
                sw.WriteLine(solution.Deliveries.Count);
                foreach (Delivery delivery in solution.Deliveries)
                {
                    sw.Write(delivery.Count);
                    foreach (Pizza pizza in delivery.DeliveryPizzas)
                    {
                        sw.Write(' ');
                        sw.Write(pizza.ID);
                    }
                    sw.WriteLine();
                }
                Console.WriteLine("Solution score: {0}", ScoreSolution(solution.Deliveries));
            }
        }

        static Solution BuildSolution(Problem p)
        {
            List<Pizza> pizzas = p.Pizzas.OrderByDescending(o => o.IngredientCount).ToList();
            List<Delivery> deliveries = new List<Delivery>();
            int[] teamSizes = new int[] { 0, 0, p.Teams2, p.Teams3, p.Teams4 };

            while (pizzas.Count >= 2)
            {
                int maxDeliverySize = 0;
                for (int i = 4; i >= 2; i--)
                {
                    if (teamSizes[i] > 0)
                    {
                        maxDeliverySize = i;
                        break;
                    }
                }

                if (maxDeliverySize == 0)
                    break;

                //List<Pizza> currentDelivery = PrepareDeliveryFirstAvailablePizza(pizzas, maxDeliverySize);
                List<Pizza> currentDelivery = PrepareDeliveryMaxUnion(pizzas, maxDeliverySize);

                // Delivery smaller than max delivery - no teams at this size.
                if (teamSizes[currentDelivery.Count] == 0)
                {
                    // Increase delivery size with the pizzas with the least ingredients
                    while (pizzas.Count > 0)
                    {
                        int nextPizza = pizzas.Count - 1;
                        currentDelivery.Add(pizzas[nextPizza]);
                        pizzas.RemoveAt(nextPizza);

                        if (teamSizes[currentDelivery.Count] > 0)
                            break;
                    }
                }

                teamSizes[currentDelivery.Count]--;
                deliveries.Add(new Delivery(currentDelivery));
            }

            return new Solution(deliveries, pizzas);
        }

        static void OptimizeSolution(Solution solution)
        {
            while (true)
            {
                while (true)
                {
                    bool optimized = false;
                    optimized = optimized || OptimizeSolution(OptimizeSolutionPairSwapBeteenDeliveries, solution, 2000);
                    optimized = optimized || OptimizeSolution(OptimizeSolutionSwapBeteenDeliveries, solution, 2000);
                    optimized = optimized || OptimizeSolution(OptimizeSolutionUsePizzas, solution, 2000);

                    if (!optimized)
                        break;
                }

                if (!OptimizeSolution(OptimizeSolutionMoveBeteenDeliveries, solution, 20000))
                    break;
            }
        }

        delegate bool OptimizationMethod(Solution solution, int maxDeliveries);
        static bool OptimizeSolution(OptimizationMethod optimizationMethod, Solution solution, int maxDeliveries)
        {
            bool optimized = false;
            while (optimizationMethod(solution, maxDeliveries))
            {
                Console.WriteLine("Optimizing score: {0}", ScoreSolution(solution.Deliveries));
                optimized = true;
            }

            return optimized;
        }

        static bool OptimizeSolutionUsePizzas(Solution solution, int maxDeliveries)
        {
            if (solution.Deliveries.Count > maxDeliveries)
                return false;

            bool optimized = false;
            foreach (Delivery delivery in solution.Deliveries)
            {
                for (int pizzaIndex = 0; pizzaIndex < solution.UnusedPizzas.Count; pizzaIndex++)
                {
                    Pizza pizza = solution.UnusedPizzas[pizzaIndex];
                    for (int i = 0; i < delivery.Count; i++)
                    {
                        if (delivery.DeliveryHashSetExcludingCurrentPizza[i].Length + pizza.Ingredients.Count < delivery.IngredientCount)
                            continue;

                        int scoreBefore = delivery.Score;
                        int scoreAfter =
                            delivery.DeliveryHashSetExcludingCurrentPizza[i].Length + pizza.Ingredients.Count -
                            IntersectSize(delivery.DeliveryHashSetExcludingCurrentPizza[i], pizza.IngredientsArray);
                        scoreAfter *= scoreAfter;

                        if (scoreAfter > scoreBefore)
                        {
                            Pizza tmp = delivery.DeliveryPizzas[i];
                            delivery.DeliveryPizzas[i] = pizza;
                            delivery.Calculate();

                            solution.UnusedPizzas[pizzaIndex] = tmp;
                            pizza = tmp;

                            optimized = true;
                        }
                    }
                }
            }

            return optimized;
        }

        static bool OptimizeSolutionPairSwapBeteenDeliveries(Solution solution, int maxOptimizedDeliveries)
        {
            List<Delivery> deliveries = solution.Deliveries;

            bool optimized = false;
            for (int i = 0; (i < deliveries.Count)&&(i < maxOptimizedDeliveries); i++)
            {
                Delivery deliveryI = deliveries[i];
                for (int j = i + 1; (j < deliveries.Count)&&(j < maxOptimizedDeliveries); j++)
                {
                    Delivery deliveryJ = deliveries[j];
                    if ((deliveryI.Count < 4)&&(deliveryJ.Count < 4))
                        continue;

                    bool optimizedDelivery = OptimizeDeliveryPairSwapPizzaBetweenDeliveries(deliveryI, deliveryJ);
                    optimized = optimized || optimizedDelivery;
                }
            }

            return optimized;
        }

        private static bool OptimizeDeliveryPairSwapPizzaBetweenDeliveries(Delivery delivery1, Delivery delivery2)
        {
            bool optimized = false;

            int scoreBefore = delivery1.Score + delivery2.Score;

            for (int pizzaPairIndex1 = 0; pizzaPairIndex1 < delivery1.Pairs.Count; pizzaPairIndex1++)
                for (int pizzaPairIndex2 = 0; pizzaPairIndex2 < delivery2.Pairs.Count; pizzaPairIndex2++)
                {
                    int scoreAfter1 =
                        delivery1.DeliveryHashSetExcludingPairPizze[pizzaPairIndex1].Length + 
                        delivery2.DeliveryHashSetPairPizza[pizzaPairIndex2].Length -
                        IntersectSize(delivery1.DeliveryHashSetExcludingPairPizze[pizzaPairIndex1], delivery2.DeliveryHashSetPairPizza[pizzaPairIndex2]);
                    scoreAfter1 *= scoreAfter1;

                    int scoreAfter2 =
                        delivery2.DeliveryHashSetExcludingPairPizze[pizzaPairIndex2].Length +
                        delivery1.DeliveryHashSetPairPizza[pizzaPairIndex1].Length -
                        IntersectSize(delivery2.DeliveryHashSetExcludingPairPizze[pizzaPairIndex2], delivery1.DeliveryHashSetPairPizza[pizzaPairIndex1]);
                    scoreAfter2 *= scoreAfter2;

                    if (scoreAfter1 + scoreAfter2 > scoreBefore)
                    {
                        SwapItems(delivery1.DeliveryPizzas, delivery1.Pairs[pizzaPairIndex1][0],
                            delivery2.DeliveryPizzas, delivery2.Pairs[pizzaPairIndex2][0]);
                        SwapItems(delivery1.DeliveryPizzas, delivery1.Pairs[pizzaPairIndex1][1],
                            delivery2.DeliveryPizzas, delivery2.Pairs[pizzaPairIndex2][1]);
                        delivery1.Calculate();
                        delivery2.Calculate();
                        optimized = true;
                    }
                }

            return optimized;
        }

        static bool OptimizeSolutionSwapBeteenDeliveries(Solution solution, int maxOptimizedDeliveries)
        {
            List<Delivery> deliveries = solution.Deliveries;

            bool optimized = false;
            for (int i = 0; (i < deliveries.Count)&&(i < maxOptimizedDeliveries); i++)
            {
                for (int j = i + 1; (j < deliveries.Count)&&(j < maxOptimizedDeliveries); j++)
                {
                    Delivery deliveryI = deliveries[i];
                    Delivery deliveryJ = deliveries[j];
                    bool optimizedDelivery = OptimizeDeliverySwapPizzaBetweenDeliveries(deliveryI, deliveryJ);
                    optimized = optimized || optimizedDelivery;
                }
            }

            return optimized;
        }

        private static bool OptimizeDeliverySwapPizzaBetweenDeliveries(Delivery delivery1, Delivery delivery2)
        {
            bool optimized = false;

            int scoreBefore = delivery1.Score + delivery2.Score;

            for (int pizzaIndex1 = 0; pizzaIndex1 < delivery1.Count; pizzaIndex1++)
                for (int pizzaIndex2 = 0; pizzaIndex2 < delivery2.Count; pizzaIndex2++)
                {
                    Pizza pizza1 = delivery1.DeliveryPizzas[pizzaIndex1];
                    Pizza pizza2 = delivery2.DeliveryPizzas[pizzaIndex2];

                    int scoreAfter1 =
                        delivery1.DeliveryHashSetExcludingCurrentPizza[pizzaIndex1].Length + pizza2.Ingredients.Count -
                        IntersectSize(delivery1.DeliveryHashSetExcludingCurrentPizza[pizzaIndex1], pizza2.IngredientsArray);
                    scoreAfter1 *= scoreAfter1;

                    int scoreAfter2 =
                        delivery2.DeliveryHashSetExcludingCurrentPizza[pizzaIndex2].Length + pizza1.Ingredients.Count -
                        IntersectSize(delivery2.DeliveryHashSetExcludingCurrentPizza[pizzaIndex2], pizza1.IngredientsArray);
                    scoreAfter2 *= scoreAfter2;

                    if (scoreAfter1 + scoreAfter2 > scoreBefore)
                    {
                        delivery1.DeliveryPizzas[pizzaIndex1] = pizza2;
                        delivery1.Calculate();
                        delivery2.DeliveryPizzas[pizzaIndex2] = pizza1;
                        delivery2.Calculate();
                        optimized = true;
                    }
                }

            return optimized;
        }

        static bool OptimizeSolutionMoveBeteenDeliveries(Solution solution, int maxDeliveries)
        {
            List<Delivery> deliveries = solution.Deliveries;

            if (deliveries.Count > maxDeliveries)
                return false;

            bool optimized = false;
            for (int i = 0; i < deliveries.Count; i++)
            {
                for (int j = i + 1; j < deliveries.Count; j++)
                {
                    Delivery deliveryI = deliveries[i];
                    Delivery deliveryJ = deliveries[j];

                    bool optimizedDelivery = OptimizeDeliveryMovePizzaBetweenDeliveries(deliveryI, deliveryJ);
                    optimized = optimized || optimizedDelivery;
                }
            }

            return optimized;
        }

        static bool OptimizeDeliveryMovePizzaBetweenDeliveries(Delivery deliveryI, Delivery deliveryJ)
        {
            Delivery large;
            Delivery small;
            if (deliveryI.Count > deliveryJ.Count)
            {
                large = deliveryI;
                small = deliveryJ;
            }
            else
            {
                large = deliveryJ;
                small = deliveryI;
            }

            // Allow moving between deliveries with a difference of 1 pizza between 
            // TODO - Handle other options like (4,2) & (3,3) - this will require checking if that's possible
            //        within problem constraints.
            if (large.Count - small.Count != 1)
                return false;

            int scoreBefore = large.Score + small.Score;
            for (int i = 0; i < large.Count; i++)
            {
                int scoreAfterLarge = large.DeliveryHashSetExcludingCurrentPizza[i].Length;
                scoreAfterLarge *= scoreAfterLarge;

                int scoreAfterSmall = small.IngredientCount + large.DeliveryPizzas[i].IngredientCount -
                    IntersectSize(small.IngredientsArray, large.DeliveryPizzas[i].IngredientsArray);
                scoreAfterSmall *= scoreAfterSmall;
                
                if (scoreAfterSmall + scoreAfterLarge > scoreBefore)
                {
                    small.DeliveryPizzas.Add(large.DeliveryPizzas[i]);
                    large.DeliveryPizzas.RemoveAt(i);
                    small.Calculate();
                    large.Calculate();
                    return true;
                }
            }

            return false;
        }

        static List<Pizza> PrepareDeliveryFirstAvailablePizza(List<Pizza> pizzas, int maxDeliverySize)
        {
            List<Pizza> currentDelivery = new List<Pizza>();

            while ((currentDelivery.Count < maxDeliverySize)&&(pizzas.Count > 0))
            {
                currentDelivery.Add(pizzas[0]);
                pizzas.RemoveAt(0);
            }

            return currentDelivery;
        }

        static List<Pizza> PrepareDeliveryMaxUnion(List<Pizza> pizzas, int maxDeliverySize)
        {
            HashSet<int> currentDeliveryIngredients = new HashSet<int>();

            List<Pizza> currentDelivery = InitDeliveryChooseFirst(pizzas);
            //List<Pizza> currentDelivery = InitDeliveryMaxPairUnion(pizzas);
            foreach (Pizza pizza in currentDelivery)
                currentDeliveryIngredients.UnionWith(pizza.Ingredients);

            while (currentDelivery.Count < maxDeliverySize)
            {
                // Add pizzas to serving to will add maximum number of ingredients 
                // with pizza with least number of ingredients.             
                int nextPizzaNum = -1;
                int nextPizzaOverlap = 0;
                int nextPizzaNewIngredients = 0;
                for (int i = 0; i < pizzas.Count; i++)
                {
                    // Early pruning - no better pizza possible
                    if (pizzas[i].IngredientCount < nextPizzaNewIngredients)
                        break;

                    int overlapSize = IntersectSize(currentDeliveryIngredients, pizzas[i].Ingredients);
                    int newIngredients = pizzas[i].IngredientCount - overlapSize;

                    if (
                        (newIngredients > nextPizzaNewIngredients)
                        ||
                        ((newIngredients == nextPizzaNewIngredients) && (overlapSize < nextPizzaOverlap))
                        )
                    {
                        nextPizzaNum = i;
                        nextPizzaOverlap = overlapSize;
                        nextPizzaNewIngredients = newIngredients;

                        // Early pruning - no better pizza possible
                        if (nextPizzaOverlap == 0)
                            break;
                    }
                }

                // Could not add new pizza that will increase the score - stop adding new pizzas
                if (nextPizzaNewIngredients == 0)
                    break;

                currentDelivery.Add(pizzas[nextPizzaNum]);
                currentDeliveryIngredients.UnionWith(pizzas[nextPizzaNum].Ingredients);
                pizzas.RemoveAt(nextPizzaNum);
            }

            return currentDelivery;
        }

        static List<Pizza> InitDeliveryChooseFirst(List<Pizza> pizzas)
        {
            List<Pizza> delivery = new List<Pizza>();
            if (pizzas.Count == 0)
                return delivery;

            delivery.Add(pizzas[0]);
            pizzas.RemoveAt(0);

            return delivery;
        }

        static List<Pizza> InitDeliveryMaxPairUnion(List<Pizza> pizzas)
        {
            List<Pizza> delivery = new List<Pizza>();
            if (pizzas.Count < 2)
            {
                delivery.AddRange(pizzas);
                pizzas.Clear();
                return delivery;
            }

            int best1 = -1;
            int best2 = -1;
            int bestUnion = -1;
            int bestIntersectSize = int.MaxValue;

            bool foundBest = false;

            for (int i = 0; i < pizzas.Count; i++)
            {
                if (foundBest)
                    break;

                for (int j = i + 1; j < pizzas.Count; j++)
                {
                    Pizza pizzaI = pizzas[i];
                    Pizza pizzaJ = pizzas[j];

                    int intersectSize = IntersectSize(pizzaI.Ingredients, pizzaJ.Ingredients);
                    int countBothSize = pizzaI.IngredientCount + pizzaJ.IngredientCount;
                    if (countBothSize < bestUnion)
                        break;

                    int unionSize = countBothSize - intersectSize;
                    if ((unionSize > bestUnion)||
                        ((unionSize == bestUnion)&&(intersectSize < bestIntersectSize))
                        )
                    {
                        best1 = i;
                        best2 = j;
                        bestUnion = unionSize;
                        bestIntersectSize = intersectSize;

                        if (intersectSize == 0)
                            break;
                    }
                }
            }

            delivery.Add(pizzas[best1]);
            delivery.Add(pizzas[best2]);
            pizzas.RemoveAt(best2);
            pizzas.RemoveAt(best1);

            return delivery;
        }

        private static void SwapItems<T>(List<T> list1, int item1, List<T> list2, int item2)
        {
            T tmp = list1[item1];
            list1[item1] = list2[item2];
            list2[item2] = tmp;
        }

        static int IntersectSize(int[] a, int[] b)
        {
            int size = 0;
            int posA = 0;
            int posB = 0;

            while ((posA < a.Length)&&(posB < b.Length))
            {
                if (a[posA] < b[posB])
                    posA++;
                else if (a[posA] > b[posB])
                    posB++;
                else if (a[posA] == b[posB])
                {
                    size++;
                    posA++;
                    posB++;
                }
                else
                    throw new Exception("Ha?");
            }

            return size;
        }

        static int IntersectSize<T>(HashSet<T> a, HashSet<T> b)
        {
            HashSet<T> small;
            HashSet<T> big;
            if (a.Count < b.Count)
            {
                small = a;
                big = b;
            }
            else
            {
                small = b;
                big = a;
            }

            int size = 0;
            // Iterate on the smaller HashSet (faster)
            foreach (T val in small)
                if (big.Contains(val))
                    size++;

            return size;
        }

        static int ScoreSolution(List<Delivery> solution)
        {
            int score = 0;
            foreach (Delivery delivery in solution)
                score += delivery.Score;

            return score;
        }
    }
}
