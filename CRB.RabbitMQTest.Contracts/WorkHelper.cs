using System.Collections.Generic;
using System.Threading;

namespace CRB.RabbitMQTest.Contracts
{
    public static class WorkHelper
    {
        public static void FindPrimeNumbers(CancellationToken cancellationToken)
        {
            int i, ctr;
            var primes = new List<int>();

            for (var num = 0; num <= int.MaxValue; num++)
            {
                ctr = 0;

                for (i = 2; i <= num / 2; i++)
                {
                    if (num % i == 0)
                    {
                        ctr++;
                        break;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (ctr == 0 && num != 1)
                    primes.Add(num);

                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
