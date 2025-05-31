using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Data;
using course.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Linq;

namespace course.Controllers
{
    [Authorize] // Все действия в этом контроллере требуют авторизации
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public MessagesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Messages/Create
        //Создание нового сообщения в чате. Это действие будет вызываться из формы на странице деталей чата.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ChatId,Content")] Message message)
        {
            // Получаем ID текущего пользователя, который является отправителем
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                ModelState.AddModelError(string.Empty, "Вы должны быть авторизованы, чтобы отправить сообщение.");
                // В случае ошибки, перенаправляем обратно на страницу чата, чтобы избежать пустой страницы
                return RedirectToAction("Details", "Chats", new { id = message.ChatId });
            }

            message.SenderId = int.Parse(userIdString);
            message.SentDate = DateTime.UtcNow; // Устанавливаем текущую дату отправки
            message.IsRead = false; // По умолчанию новое сообщение непрочитано

            // Проверяем, существует ли чат, к которому пытаются отправить сообщение
            var chatExists = await _context.Chats.AnyAsync(c => c.Id == message.ChatId);
            if (!chatExists)
            {
                return NotFound("Чат, к которому вы пытаетесь отправить сообщение, не найден.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(message);
                await _context.SaveChangesAsync();
                // Перенаправляем пользователя обратно на страницу деталей чата
                return RedirectToAction("Details", "Chats", new { id = message.ChatId });
            }

            // Если ModelState невалиден, перенаправляем обратно на страницу чата.
            // В реальном приложении, возможно, вам захочется передать ошибки валидации обратно в представление.
            return RedirectToAction("Details", "Chats", new { id = message.ChatId });
        }

        // POST: Messages/MarkAsRead/5
        //Отметить сообщение как прочитанное. Это может быть AJAX-вызов.
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            // В идеале здесь должна быть проверка, что текущий пользователь
            // является получателем этого сообщения, чтобы отметить его как прочитанное.
            // Для простоты, пока опустим эту логику, но имейте в виду для продакшена.

            message.IsRead = true;
            _context.Update(message);
            await _context.SaveChangesAsync();

            // Возвращаем пустой ответ или статус успеха для AJAX
            return Ok();
        }

        // GET: Messages/Delete/5 (для модераторов/администраторов)
        //Отображает страницу подтверждения удаления сообщения.
        [Authorize(Roles = "Модератор")] // Только модераторы могут видеть эту страницу
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                return NotFound();
            }

            // Загружаем имя отправителя
            var sender = await _userManager.FindByIdAsync(message.SenderId.ToString());
            ViewData["SenderName"] = sender?.UserName;

            // Загружаем название чата
            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == message.ChatId);
            ViewData["ChatName"] = chat?.Name;

            return View(message);
        }

        // POST: Messages/Delete/5
        //Обрабатывает подтверждение удаления сообщения.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Модератор")] // Только модераторы могут удалять сообщения
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            var chatId = message.ChatId; // Сохраняем ChatId для перенаправления

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            // Перенаправляем обратно на страницу чата, из которого было удалено сообщение
            return RedirectToAction("Details", "Chats", new { id = chatId });
        }
    }
}