using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Используется для ToListAsync, FindAsync, AsNoTracking
using course.Data; // Ваш контекст базы данных
using course.Models; // Ваши модели
using Microsoft.AspNetCore.Authorization; // Для атрибута [Authorize]
using Microsoft.AspNetCore.Identity; // Для UserManager
using System.Security.Claims; // Для User.FindFirstValue
using System.Linq; // Для Linq методов (Select, Distinct, ToDictionary)

namespace course.Controllers
{
	public class PostsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<User> _userManager; // Для получения информации о пользователях

		public PostsController(ApplicationDbContext context, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		// GET: Posts
		public async Task<IActionResult> Index()
		{
			// Получаем все посты, которые не скрыты
			var posts = await _context.Posts.Where(p => !p.IsHidden).ToListAsync();

			// Если нужно отобразить имя автора для каждого поста в Index:
			// 1. Собираем все уникальные AuthorId из полученных постов.
			var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();
			// 2. Загружаем всех соответствующих пользователей.
			var authors = await _userManager.Users.Where(u => authorIds.Contains(u.Id)).ToListAsync();
			// 3. Создаем словарь для быстрого поиска имени пользователя по ID в представлении.
			ViewData["Authors"] = authors.ToDictionary(u => u.Id, u => u.UserName); // Или u.Email, в зависимости от того, что хотите отобразить.

			return View(posts);
		}

		// GET: Posts/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var post = await _context.Posts.FirstOrDefaultAsync(m => m.Id == id);

			if (post == null || post.IsHidden)
			{
				return NotFound();
			}

			// Загружаем имя автора поста
			var author = await _userManager.FindByIdAsync(post.AuthorId.ToString());
			ViewData["AuthorName"] = author?.UserName; // Сохраняем имя автора в ViewData

			// Загружаем комментарии для этого поста
			var comments = await _context.Comments
										 .Where(c => c.PostId == post.Id)
										 .OrderBy(c => c.CreatedDate) // Отсортируем комментарии по дате
										 .ToListAsync();
			ViewData["Comments"] = comments;

			// Загружаем авторов комментариев (если они нужны в представлении)
			var commentAuthorIds = comments.Select(c => c.AuthorId).Distinct().ToList();
			var commentAuthors = await _userManager.Users.Where(u => commentAuthorIds.Contains(u.Id)).ToListAsync();
			ViewData["CommentAuthors"] = commentAuthors.ToDictionary(u => u.Id, u => u.UserName);

			return View(post);
		}

		// GET: Posts/Create
		[Authorize] // Только залогиненные пользователи могут создавать посты
		public IActionResult Create()
		{
			return View();
		}

		// POST: Posts/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Create([Bind("Title,Content")] Post post)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Получаем ID текущего пользователя
			if (string.IsNullOrEmpty(userId))
			{
				// Если пользователь не аутентифицирован (что маловероятно с [Authorize], но хорошая проверка)
				ModelState.AddModelError(string.Empty, "Для создания поста необходимо быть авторизованным пользователем.");
				return View(post);
			}

			post.AuthorId = int.Parse(userId); // Преобразуем ID в int и присваиваем посту
			post.CreatedDate = DateTime.UtcNow; // Устанавливаем текущую дату
			post.IsEdited = false; // Новый пост, еще не редактировался
			post.IsHidden = false; // По умолчанию пост не скрыт

			if (ModelState.IsValid)
			{
				_context.Add(post);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			// Если модель не валидна, возвращаем представление с ошибками
			return View(post);
		}

		// GET: Posts/Edit/5
		[Authorize] // Только залогиненные пользователи могут редактировать
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var post = await _context.Posts.FindAsync(id);
			if (post == null || post.IsHidden)
			{
				return NotFound();
			}

			var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			// Проверка: только автор поста или модератор могут редактировать
			if (post.AuthorId != int.Parse(currentUserId) && !User.IsInRole("Модератор"))
			{
				return Forbid(); // Запрещаем доступ
			}

			return View(post);
		}
		// POST: Posts/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content")] Post post) // Убираем CreatedDate, AuthorId, IsHidden из Bind
		{
			if (id != post.Id)
			{
				return NotFound();
			}

			// Получаем существующий пост из базы данных для сохранения неизменяемых полей
			var existingPost = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
			if (existingPost == null || existingPost.IsHidden)
			{
				return NotFound();
			}

			var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			// Повторная проверка прав доступа, если пост был изменен другим способом до отправки формы
			if (existingPost.AuthorId != int.Parse(currentUserId) && !User.IsInRole("Модератор"))
			{
				return Forbid();
			}

			if (ModelState.IsValid)
			{
				try
				{
					// Важно: переносим значения, которые не должны меняться из формы
					post.AuthorId = existingPost.AuthorId;
					post.CreatedDate = existingPost.CreatedDate;
					post.IsHidden = existingPost.IsHidden; // Сохраняем текущее состояние IsHidden
					post.IsEdited = true; // Отмечаем, что пост был отредактирован
					post.LastEditedDate = DateTime.UtcNow; // Обновляем дату последнего редактирования

					_context.Update(post);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!PostExists(post.Id))
					{
						return NotFound();
					}
					else
					{
						throw; // Повторно бросаем исключение, если это не ошибка "не найден"
					}
				}
				return RedirectToAction(nameof(Index));
			}
			return View(post);
		}

		// GET: Posts/Delete/5
		[Authorize(Roles = "Модератор")] // Только модераторы могут удалять посты
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var post = await _context.Posts.FirstOrDefaultAsync(m => m.Id == id);
			if (post == null || post.IsHidden)
			{
				return NotFound();
			}

			// Если вам нужно имя автора для представления удаления:
			var author = await _userManager.FindByIdAsync(post.AuthorId.ToString());
			ViewData["AuthorName"] = author?.UserName;

			// Дополнительная проверка на возможность удаления для автора (если нужно)
			// var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			// if (post.AuthorId != int.Parse(currentUserId) && !User.IsInRole("Модератор"))
			// {
			//     return Forbid();
			// }

			return View(post);
		}

		// POST: Posts/Delete/5
		[HttpPost, ActionName("Delete")] // Связываем с методом Delete (GET)
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Модератор")] // Только модераторы могут подтвердить удаление
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var post = await _context.Posts.FindAsync(id);
			if (post != null)
			{
				// Физическое удаление (если IsHidden = true означает скрытие, то удалять не нужно)
				_context.Posts.Remove(post); // Удаляем запись из базы данных
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		private bool PostExists(int id)
		{
			return _context.Posts.Any(e => e.Id == id && !e.IsHidden);
		}
	}
}