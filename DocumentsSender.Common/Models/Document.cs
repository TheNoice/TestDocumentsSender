namespace DocumentsSender.Common.Models
{
    /// <summary>
    /// Примитивный вариант отправляемых документов
    /// </summary>
    public class Document
    {
        public string? Name { get; set; }
        public int DocumentId { get; set; }

        public Document(int documentId)
        {
            DocumentId = documentId;
        }

        public override bool Equals(object obj)
        {
           return obj is Document other && other.DocumentId == DocumentId;
        }

        public override int GetHashCode()
        {
            int hashCode = 889650705;
            hashCode = hashCode * -1521134295 + DocumentId.GetHashCode();
            return hashCode;
        }
    }
}
