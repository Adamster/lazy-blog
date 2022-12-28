using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lazy.Presentation.Models.Author;
using Mapster;
using Lazy.DataContracts.Author;
using Lazy.Services.Author;

namespace Lazy.Presentation.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly IAuthorService _authorService;

        public AuthorsController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        // GET: Authors
        public async Task<IActionResult> Index()
        {
            IList<AuthorItemDto> authors = await  _authorService.GetAllAuthors();

            if (!authors.Any())
            {
                return View(new List<AuthorItemModel>());
            }
            var authorItems = authors.Adapt<List<AuthorItemModel>>();
            return View(authorItems);

        }

        // GET: Authors/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            AuthorItemDto? author = await _authorService.GetAuthorById(id);
            if (author == null)
            {
                return NotFound();
            }

            return View(author.Adapt<AuthorItemModel>());
        }

        // GET: Authors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Authors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAuthorModel author)
        {
            if (ModelState.IsValid)
            {
                AuthorItemDto newAuthor = await  _authorService.CreateAuthor(author.Adapt<CreateAuthorDto>());
        
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Authors/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _authorService.GetAuthorById(id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author.Adapt<EditAuthorModel>());
        }

        // POST: Authors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditAuthorModel author)
        {
            if (id != author.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _authorService.UpdateAuthor(author.Adapt<UpdateAuthorDto>());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await AuthorExists(author.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Authors/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _authorService.GetAuthorById(id);
            if (author == null)
            {
                return NotFound();
            }

            return View(author.Adapt<AuthorItemModel>());
        }

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            
            bool deleteSuccess =  await _authorService.DeleteById(id);
            if (!deleteSuccess)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> AuthorExists(Guid id)
        {
          return await _authorService.GetAuthorById(id) != null;
        }
    }
}
