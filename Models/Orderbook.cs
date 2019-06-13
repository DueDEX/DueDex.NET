using System.Collections.Generic;

namespace DueDex.Models
{
    public class Orderbook
    {
        /// <summary>
        /// A two-dimensional array containing price levels on the ask side
        /// </summary>
        public IReadOnlyDictionary<decimal, long> Asks { get { return asks; } }
        /// <summary>
        /// A two-dimensional array containing price levels on the bid side
        /// </summary>
        public IReadOnlyDictionary<decimal, long> Bids { get { return bids; } }

        private readonly SortedDictionary<decimal, long> asks = new SortedDictionary<decimal, long>();
        private readonly SortedDictionary<decimal, long> bids = new SortedDictionary<decimal, long>(Comparer<decimal>.Create((x, y) => y.CompareTo(x)));

        public void UpdateAsk(decimal price, long size)
        {
            UpdateLevel(asks, price, size);
        }

        public void UpdateBid(decimal price, long size)
        {
            UpdateLevel(bids, price, size);
        }

        private void UpdateLevel(IDictionary<decimal, long> side, decimal price, long size)
        {
            if (size == 0)
            {
                // Level removal
                side.Remove(price);
            }
            else
            {
                // Level update
                side[price] = size;
            }
        }
    }
}