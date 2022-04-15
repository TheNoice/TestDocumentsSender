using DocumentsSender.Common.Models;
using DocumentsSender.Common.Utils;
using DocumentsSender.Core.BusinessServices.Interfaces;
using DocumentsSender.Core.Connectors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DocumentsSender.Core.BusinessServices
{
    public class DocumentsQueueBasicService : IDocumentsQueue, IDisposable
    {
        private protected System.Timers.Timer timer;
        private static readonly ConcurrentQueue<Document> documentsQueue = new ConcurrentQueue<Document>();
        private ExternalSystemConnector ExternalSystemConnector { get; set; }
        private Action<string> ErrorHandler { get; set; }
        private IProgress<string> ProgressNotifier { get; set; }
        private static object syncObj = new object();

        public CancellationTokenSource CancellationSource { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public static bool DocumentSendingStarted { get; set; }

        public DocumentsQueueBasicService(ExternalSystemConnector externalSystemConnector, CancellationTokenSource cancellationSource, CancellationToken cancellationToken, 
            Action<string> errorHandler = null, IProgress<string> progressNotifier = null)
        {
            ExternalSystemConnector = externalSystemConnector;
            CancellationSource = cancellationSource;
            CancellationToken = cancellationToken;
            ErrorHandler = errorHandler;
            ProgressNotifier = progressNotifier;
        }

        public void Enqueue(Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (documentsQueue.Contains(document))
                throw new DocumentQueueException("This document is already in the queue.");

            documentsQueue.Enqueue(document);
        }

        public bool TryStartSendingDocuments(double timerMsInterval = 5000)
        {
            lock (syncObj)
            {
                if (DocumentSendingStarted)
                    return false;
                else
                {
                    timer = new System.Timers.Timer(timerMsInterval);
                    timer.Elapsed += (obj, ev) => SendDocuments();

                    timer.Start();
                    DocumentSendingStarted = true;
                    return true;
                }
            }
        }

        public bool TryStopSendingDocuments()
        {
            lock (syncObj)
            {
                if (!DocumentSendingStarted)
                    return false;
                else
                {
                    timer.Elapsed -= (obj, ev) => SendDocuments();
                    timer.Stop();
                    DocumentSendingStarted = false;
                    return true;
                }
            }
        }

        private async void SendDocuments()
        {
            if (!documentsQueue.Any())
                return;

            try
            {
                timer?.Stop();

                if (CancellationToken.IsCancellationRequested)
                {
                    documentsQueue.Clear();
                    CancellationToken.ThrowIfCancellationRequested();
                }

                var documentsForSending = new List<Document>();

                for (int i = 0; i < 10; i++)
                {
                    if (documentsQueue.TryDequeue(out Document document))
                        documentsForSending.Add(document);
                    else
                        break;
                }

                var task = ExternalSystemConnector.SendDocument(documentsForSending);
                await task;

                if (task.IsCompletedSuccessfully)
                    ProgressNotifier?.Report("Success.");
            }
            catch (Exception ex)
            {
                ErrorHandler?.Invoke(ex.Message);
            }
            finally
            {
                if (DocumentSendingStarted)
                    timer?.Start();
            }
        }

        public virtual void Dispose()
        {
            lock (syncObj)
            {
                if (DocumentSendingStarted && TryStopSendingDocuments())
                    timer?.Dispose();
            }
        }
    }
}
