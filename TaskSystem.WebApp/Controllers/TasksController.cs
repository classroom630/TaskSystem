using Microsoft.AspNetCore.Mvc;
using TaskSystem.WebApp.Models;
using TaskSystem.WebApp.Services;

namespace TaskSystem.WebApp.Controllers
{
    public class TasksController : Controller
    {
        private readonly TaskApiService _taskApiService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(TaskApiService taskApiService, ILogger<TasksController> logger)
        {
            _taskApiService = taskApiService;
            _logger = logger;
        }

        // GET: Tasks
        public async Task<IActionResult> Index()
        {
            var tasks = await _taskApiService.GetAllTasksAsync();
            return View(tasks);
        }

        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var task = await _taskApiService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        // GET: Tasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskViewModel createTask)
        {
            if (ModelState.IsValid)
            {
                var task = await _taskApiService.CreateTaskAsync(createTask);
                if (task != null)
                {
                    TempData["SuccessMessage"] = "Task created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Failed to create task. Please try again.");
            }
            return View(createTask);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _taskApiService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            var editTask = new EditTaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate
            };

            return View(editTask);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditTaskViewModel editTask)
        {
            if (id != editTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var success = await _taskApiService.UpdateTaskAsync(id, editTask);
                if (success)
                {
                    TempData["SuccessMessage"] = "Task updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Failed to update task. Please try again.");
            }
            return View(editTask);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskApiService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _taskApiService.DeleteTaskAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Task deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete task.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Tasks/Status/{status}
        public async Task<IActionResult> Status(TaskItemStatus status)
        {
            var tasks = await _taskApiService.GetTasksByStatusAsync(status);
            ViewBag.Status = status;
            return View("Index", tasks);
        }

        // POST: Tasks/MarkComplete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkComplete(int id)
        {
            var task = await _taskApiService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            var editTask = new EditTaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = TaskItemStatus.Completed,
                Priority = task.Priority,
                DueDate = task.DueDate
            };

            var success = await _taskApiService.UpdateTaskAsync(id, editTask);
            if (success)
            {
                TempData["SuccessMessage"] = "Task marked as completed!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update task.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}