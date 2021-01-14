using System;
using System.Collections.Generic;
using System.Text;

namespace EvenMorePizza
{
    class Solution
    {
        public List<Delivery> Deliveries;
        public List<Pizza> UnusedPizzas;

        public Solution(List<Delivery> deliveries, List<Pizza> unusedPizzas)
        {
            this.Deliveries = deliveries;
            this.UnusedPizzas = unusedPizzas;
        }
    }
}
