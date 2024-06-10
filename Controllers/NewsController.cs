using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using News.Entities;

namespace News.Controllers
{
    public class NewsController : Controller
    {
        private readonly NewsContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NewsController(NewsContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: News
        public async Task<IActionResult> Index()
        {
            return View(await _context.News.ToListAsync());
        }

        // GET: News/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News
                .FirstOrDefaultAsync(m => m.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // GET: News/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile image, string title, string content)
        {
            if (image != null && image.Length > 0)
            {
                var supportedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/tiff" };
                if (!supportedTypes.Contains(image.ContentType))
                {
                    ModelState.AddModelError("image", "Invalid image type. Only JPEG, PNG, GIF, BMP, and TIFF are allowed.");
                    return View();
                }
                var newsEntry = new Entities.News
                {
                    Title = title,
                    Content = content
                };
                _context.News.Add(newsEntry);
                await _context.SaveChangesAsync();
                newsEntry.Id = newsEntry.Id;
                var fileExtension = Path.GetExtension(image.FileName);
                var newFileName = newsEntry.Id + fileExtension;
                var relativePath = Path.Combine("images", newFileName);
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var filePath = Path.Combine(uploadsFolder, newFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                newsEntry.Image = relativePath;
                _context.Update(newsEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("image", "Image is required.");
            return View();
        }

        // GET: News/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }
            return View(news);
        }

        // POST: News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile newImage, string title, string content)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (newImage != null && newImage.Length > 0)
                {
                    var supportedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/tiff" };
                    if (!supportedTypes.Contains(newImage.ContentType))
                    {
                        ModelState.AddModelError("newImage", "Invalid image type. Only JPEG, PNG, GIF, BMP, and TIFF are allowed.");
                        return View(news);
                    }

                    if (!string.IsNullOrEmpty(news.Image))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, news.Image);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    var fileExtension = Path.GetExtension(newImage.FileName);
                    var newFileName = news.Id + fileExtension;
                    var relativePath = Path.Combine("images", newFileName);
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var filePath = Path.Combine(uploadsFolder, newFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await newImage.CopyToAsync(stream);
                    }

                    news.Image = relativePath;
                }

                news.Title = title;
                news.Content = content;

                try
                {
                    _context.Update(news);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NewsExists(news.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            news.Title = title;
            news.Content = content;

            try
            {
                _context.Update(news);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsExists(news.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }



        // GET: News/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News
                .FirstOrDefaultAsync(m => m.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                if (!string.IsNullOrEmpty(news.Image))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, news.Image);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                _context.News.Remove(news);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.Id == id);
        }
    }
}
