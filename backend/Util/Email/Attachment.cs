namespace SnoopyAirlines.Util.Email
{
    public sealed class Attachment
    {
        public Attachment(string fileName, string format, Func<Stream> openRead)
        {
            FileName = string.IsNullOrWhiteSpace(fileName)
                ? throw new ArgumentException("Attachment file name is required.", nameof(fileName))
                : fileName;

            Format = string.IsNullOrWhiteSpace(format)
                ? throw new ArgumentException("Attachment format is required.", nameof(format))
                : format;

            OpenRead = openRead ?? throw new ArgumentNullException(nameof(openRead));
        }

        public string FileName { get; }
        public string Format { get; }
        public Func<Stream> OpenRead { get; }

        public static Attachment FromBytes(
            string fileName,
            string format,
            byte[] content)
        {
            if (content is null || content.Length == 0)
            {
                throw new ArgumentException("Attachment content is required.", nameof(content));
            }

            return new Attachment(fileName, format, () => new MemoryStream(content));
        }

        public static Attachment FromFile(
            string fileName,
            string format,
            string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Attachment path is required.", nameof(path));
            }

            return new Attachment(fileName, format, () => File.OpenRead(path));
        }
    }
}
