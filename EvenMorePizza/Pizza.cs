using System;
using System.Collections.Generic;
using System.Text;

namespace EvenMorePizza
{
    class Pizza
    {
        private int mId;
        private HashSet<int> mIngredients;
        private int[] mIngredientsArray;

        public int ID { get { return mId; } }

        public HashSet<int> Ingredients { get { return mIngredients; } }

        public int[] IngredientsArray { get { return mIngredientsArray; } }

        public int IngredientCount { get { return mIngredients.Count; } }

        public Pizza(int id, HashSet<int> ingredients)
        {
            mId = id;
            mIngredients = ingredients;
            List<int> list = new List<int>(mIngredients);
            list.Sort();
            mIngredientsArray = list.ToArray();
        }
    }
}
