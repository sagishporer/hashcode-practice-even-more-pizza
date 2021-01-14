using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EvenMorePizza
{
    class Problem
    {
        private List<Pizza> mPizzas;
        private int mTeams2;
        private int mTeams3;
        private int mTeams4;
        private Dictionary<int, List<Pizza>> mIngredientToPizzas;

        public List<Pizza> Pizzas { get { return mPizzas; } }

        public int Teams2 { get { return mTeams2; } }
        public int Teams3 { get { return mTeams3; } }
        public int Teams4 { get { return mTeams4; } }

        public int IngredientCount { get { return mIngredientToPizzas.Count; } }

        private Problem(int teams2, int teams3, int teams4, List<Pizza> pizzas)
        {
            mPizzas = pizzas;
            mTeams2 = teams2;
            mTeams3 = teams3;
            mTeams4 = teams4;

            mIngredientToPizzas = new Dictionary<int, List<Pizza>>();
            foreach (Pizza pizza in pizzas)
                foreach (int ingredient in pizza.Ingredients)
                {
                    List<Pizza> ingredientPizzas;
                    if (!mIngredientToPizzas.TryGetValue(ingredient, out ingredientPizzas))
                    {
                        ingredientPizzas = new List<Pizza>();
                        mIngredientToPizzas.Add(ingredient, ingredientPizzas);
                    }
                    ingredientPizzas.Add(pizza);
                }
        }

        public int GetTotalIngredients()
        {
            int count = 0;
            foreach (KeyValuePair<int, List<Pizza>> kvp in mIngredientToPizzas)
                count += kvp.Value.Count;

            return count;
        }

        public static Problem LoadProblem(string fileName)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line = sr.ReadLine();
                string[] parts = line.Split(' ');
                int pizzaCount = int.Parse(parts[0]);
                int teams2 = int.Parse(parts[1]);
                int teams3 = int.Parse(parts[2]);
                int teams4 = int.Parse(parts[3]);
                List<Pizza> pizzas = new List<Pizza>();
                Dictionary<string, int> ingredientsMap = new Dictionary<string, int>();
                int nextIngredientId = 0;

                for (int i = 0; i < pizzaCount; i++)
                {
                    line = sr.ReadLine();
                    parts = line.Split(' ');
                    int ingredientCount = int.Parse(parts[0]);
                    HashSet<int> ingredients = new HashSet<int>();
                    for (int j = 0; j < ingredientCount; j++) 
                    {
                        string ingredient = parts[1 + j];
                        if (!ingredientsMap.ContainsKey(ingredient))
                            ingredientsMap.Add(ingredient, nextIngredientId++);

                        ingredients.Add(ingredientsMap[ingredient]);
                    }

                    pizzas.Add(new Pizza(i, ingredients));
                }

                return new Problem(teams2, teams3, teams4, pizzas);
            }
        }
    }
}
