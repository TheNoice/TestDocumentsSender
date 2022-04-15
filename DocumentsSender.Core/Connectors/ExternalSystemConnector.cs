using DocumentsSender.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentsSender.Core.Connectors
{
    /// <summary>
    /// Представляет коннектор для отправки документов во внешнюю систему.
    /// </summary>
    public sealed class ExternalSystemConnector
    {
        /// <summary>
        /// Выполняет отправку документов во внешнюю систему.
        /// </summary>
        /// <param name="documents">
        /// Документы, которые нужно отправить.
        /// </param>
        /// <returns>
        /// Асинхронная операция, завершение которой означает отправку документов.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Возникакет при попытке отправить более 10 документов за раз.
        /// </exception>
        public async Task SendDocument(IReadOnlyCollection<Document> documents)
        {
            if (documents.Count > 10)
            {
                throw new ArgumentException("Can't send more than 10 documents at once.", nameof
                (documents));
            }
            // тестовая реализация, просто ничего не делаем 2 секунды
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}
