using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAK
{
    internal class WeightedRNG
    {
        private List<WeightedNumber> numbers;
        private Random random;
        private int totalWeight;

        public WeightedRNG()
        {
            numbers = new List<WeightedNumber>();
            random = new Random();
        }

        public void AddNumber(int number, int weight)
        {
            numbers.Add(new WeightedNumber(number, weight));
            totalWeight += weight;
        }

        public void RemoveNumber(int number, int weight)
        {
            numbers.Remove(new WeightedNumber(number, weight));
            totalWeight -= weight;
        }

        public void RemoveLargeNumbers()
        {
            numbers.RemoveAll(n => n.Number >= 10);
            totalWeight = 0;
            foreach (WeightedNumber number in numbers)
            {
                totalWeight += number.Weight;
            }

        }

        public int GetRandomNumber()
        {
            int randomNumber = random.Next(totalWeight);
            int sum = 0;

            foreach (var weightedNumber in numbers)
            {
                sum += weightedNumber.Weight;
                if (randomNumber < sum)
                {
                    if (weightedNumber.Number > 10)
                    {
                        RemoveLargeNumbers();
                    }
                    return weightedNumber.Number;
                }
            }
            throw new InvalidOperationException("There are no numbers added.");
        }


    }
    public class WeightedNumber
    {
        public int Number { get; set; }
        public int Weight { get; set; }

        public WeightedNumber(int number, int weight)
        {
            Number = number;
            Weight = weight;
        }
    }
}
