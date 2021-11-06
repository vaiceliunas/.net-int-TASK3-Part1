using System;

namespace CentralizedControlService.Task1
{
    class Program
    {
        static void Main(string[] args)
        {

            RabbitMqActions.InitializeTransferQueue();
            Console.WriteLine("Centralized service is running now");
            Console.WriteLine("In case you want to cancel, press Q");
            Console.WriteLine("Else, insert number between 1000 and 5000 ms (defines how often should services send their status)");

            var input = Console.ReadLine();
            while (input.Trim().ToUpper() != "Q")
            {
                if (int.TryParse(input, out var n))
                {
                    RabbitMqActions.SendInstructions(n);
                }
                else
                {
                    Console.WriteLine($"Invalid integer: '{input}'. Please try again.");
                }

                input = Console.ReadLine();

            }
        }
    }
}
