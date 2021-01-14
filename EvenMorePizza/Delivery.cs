using System;
using System.Collections.Generic;
using System.Text;

namespace EvenMorePizza
{
    class Delivery
    {
        static Dictionary<int, List<int[]>> PAIRS_COMBINATIONS = new Dictionary<int, List<int[]>>()
        {
            { 4, new List<int[]>() 
                {
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,3},
                    new int[] {1,2},
                    new int[] {1,3},
                    new int[] {2,3}
                }
            },
            { 3, new List<int[]>()
                {
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {1,2}
                }
            },
            { 2, new List<int[]>()
                {
                    new int[] {0,1}
                }
            }
        };

        public int IngredientCount { get; private set; }

        public int[] IngredientsArray { get; private set; }

        public int Score { get; private set; }

        public List<Pizza> DeliveryPizzas { get; private set; }

        public List<int[]> DeliveryHashSetExcludingCurrentPizza { get; private set; }

        public List<int[]> Pairs { get; private set; }

        public List<int[]> DeliveryHashSetExcludingPairPizze { get; private set; }

        public List<int[]> DeliveryHashSetPairPizza { get; private set; }

        public int Count { get { return this.DeliveryPizzas.Count; } }

        public Delivery(List<Pizza> pizzas)
        {
            DeliveryPizzas = new List<Pizza>(pizzas);
            Calculate();
        }

        public void Calculate()
        {
            HashSet<int> ingredients = new HashSet<int>();
            foreach (Pizza pizza in this.DeliveryPizzas)
                ingredients.UnionWith(pizza.Ingredients);

            IngredientsArray = HashSetToArray(ingredients);
            IngredientCount = ingredients.Count;
            Score = IngredientCount * IngredientCount;

            // Build hash for single pizza
            DeliveryHashSetExcludingCurrentPizza = new List<int[]>();
            for (int i = 0; i < DeliveryPizzas.Count; i++)
            {
                HashSet<int> deliveryHashSetExcludingCurrentPizza = new HashSet<int>();
                for (int j = 0; j < DeliveryPizzas.Count; j++)
                {
                    if (i == j)
                        continue;

                    deliveryHashSetExcludingCurrentPizza.UnionWith(DeliveryPizzas[j].Ingredients);
                }

                DeliveryHashSetExcludingCurrentPizza.Add(HashSetToArray(deliveryHashSetExcludingCurrentPizza));
            }

            // Build hash for pizza pair
            Pairs = PAIRS_COMBINATIONS[this.DeliveryPizzas.Count];
            DeliveryHashSetPairPizza = new List<int[]>();
            DeliveryHashSetExcludingPairPizze = new List<int[]>();
            foreach (int[] pair in Pairs)
            {
                // Build pairs
                HashSet<int> deliveryHashSetPair = new HashSet<int>();
                HashSet<int> deliveryHashSetExcludinhPair = new HashSet<int>();
                for (int j = 0; j < DeliveryPizzas.Count; j++)
                {
                    if ((j == pair[0])||(j == pair[1]))
                        deliveryHashSetPair.UnionWith(DeliveryPizzas[j].Ingredients);
                    else
                        deliveryHashSetExcludinhPair.UnionWith(DeliveryPizzas[j].Ingredients);
                }
                DeliveryHashSetPairPizza.Add(HashSetToArray(deliveryHashSetPair));
                DeliveryHashSetExcludingPairPizze.Add(HashSetToArray(deliveryHashSetExcludinhPair));
            }
        }

        private static int[] HashSetToArray(HashSet<int> hashSet)
        {
            List<int> array = new List<int>(hashSet);
            array.Sort();

            return array.ToArray();
        }
    }
}
