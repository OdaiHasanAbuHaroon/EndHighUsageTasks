namespace EndHighUsageTasks.Models
{
    public class EmailSettings
    {
        public int SmtpPort { get; set; } = 0;

        public string? SmtpServer { get; set; }

        public string? EmailFrom { get; set; }

        public string? EmailTo { get; set; }

        public string? EmailSubject { get; set; }

        public string? GmailApplicationPassword { get; set; }
    }
}