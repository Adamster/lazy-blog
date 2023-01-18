using Lazy.DataContracts.Author;
using Lazy.Presentation.Models.Author;
using Lazy.Services.Author;
using Mapster;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Lazy.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthorController : ControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    // GET: api/<AuthorController>
    [HttpGet]
    public async Task<ActionResult<IList<AuthorItemDto>>> Get()
    {
        IList<AuthorItemDto> authors = await _authorService.GetAllAuthors();

        return Ok(authors);
    }

    // GET api/<AuthorController>/5
    [HttpGet("{id}")]
    public async Task<ActionResult<AuthorItemDto>> Get(Guid id)
    {
        var author = await _authorService.GetAuthorById(id);

        if (author == null)
        {
            return NotFound();
        }

        return Ok(author);
    }

    // POST api/<AuthorController>
    [HttpPost]
    public async Task<CreatedAtActionResult> Post([FromBody] CreateAuthorDto createAuthorModel)
    {
        AuthorItemDto author = await _authorService.CreateAuthor(createAuthorModel);
        return CreatedAtAction(nameof(Get), new { id = author.Id }, author);
    }

    // PUT api/<AuthorController>/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdateAuthorDto updateAuthorDto)
    {
        if (id != updateAuthorDto.Id)
        {
            return BadRequest();
        }

        var existingAuthor = await _authorService.GetAuthorById(id);
        if (existingAuthor is null)
        {
            return NotFound();
        }

        await _authorService.UpdateAuthor(updateAuthorDto);


        return NoContent();
    }

    // DELETE api/<AuthorController>/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existingAuthor = await _authorService.GetAuthorById(id);
        if (existingAuthor is null)
        {
            return NotFound();
        }

        await _authorService.DeleteById(id);

        return NoContent();
    }
}