using DocumentsSender.Common.Models;
using DocumentsSender.Core.BusinessServices;
using DocumentsSender.Core.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentsSenderConsole
{
    internal class Program
    {
        /// <summary>
        /// В этом методе - вариант топорной тестовой реализации функционала сервиса.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            int a = 1;

            Console.WriteLine(a);

            List<DocumentsQueueBasicService> servicesList = new();

            for (int i = 0; i < 10; i++)
            {
                CancellationTokenSource src = new CancellationTokenSource();
                CancellationToken ct = src.Token;

                DocumentsQueueBasicService qService = new DocumentsQueueBasicService(new ExternalSystemConnector(), src, ct,
                (message) => Console.WriteLine(message),
                new Progress<string>((message) => Console.WriteLine(message)));

                servicesList.Add(qService);

                if (qService.TryStartSendingDocuments(5000))
                {
                    Console.WriteLine("Started! Press any key to stop the execution.");
                }
            }


            servicesList.AsParallel().ForAll((service) => 
            {
                for (int j = 0; j < 15; j++)
                {
                    Random random = new Random();
                    Task.Run(() =>
                    {
                        try
                        {
                            Document doc = new Document(random.Next(1, 500));
                            service.Enqueue(doc);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    });
                }
            });

            Console.ReadKey();

            foreach (var service in servicesList)
            {
                service.TryStopSendingDocuments();
            }

            Console.ReadKey();

            servicesList.AsParallel().ForAll((service) =>
            {
                for (int j = 0; j < 15; j++)
                {
                    Random random = new Random();
                    Task.Run(() =>
                    {
                        try
                        {
                            Document doc = new Document(random.Next(1, 500));
                            service.Enqueue(doc);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    });
                }
            });

            foreach (var service in servicesList)
            {
                service.TryStartSendingDocuments(5000);
            }

            Console.ReadKey();

            foreach (var service in servicesList)
            {
                service.Dispose();
            }

            Console.ReadKey();
        }
    }
}
