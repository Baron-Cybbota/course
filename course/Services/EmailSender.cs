using Microsoft.AspNetCore.Identity.UI.Services; // <-- ЭТО ОЧЕНЬ ВАЖНО!
using System.Threading.Tasks;
using System.Diagnostics; // Для Debug.WriteLine

namespace course.Services
{
    public class EmailSender : IEmailSender // <-- ЭТО ОЧЕНЬ ВАЖНО! Ваш класс ДОЛЖЕН реализовать этот интерфейс
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Для целей тестирования и отладки просто выведем сообщение в консоль/вывод отладки.
            // В реальном приложении здесь была бы логика отправки email через внешний сервис.
            Debug.WriteLine($"Simulating email send to: {email}");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine($"Body: {htmlMessage}");

            // Если вы хотите увидеть это в консоли dotnet run, вы можете использовать Console.WriteLine:
            Console.WriteLine($"[EMAIL SENT TO: {email}] Subject: {subject}");
            Console.WriteLine($"Body: {htmlMessage}");

            return Task.CompletedTask;
        }
    }
}