using Lazy.DataContracts.Author;
using Lazy.Services.Author;
using Lazy.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Lazy.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthorController : ControllerBase
{
    private readonly IAuthorService _authorService;
    private readonly IUserService _userService;

    public AuthorController(IAuthorService authorService, IUserService userService)
    {
        _authorService = authorService;
        _userService = userService;
    }

    // GET: api/<AuthorController>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IList<AuthorItemDto>>> Get()
    {
        IList<AuthorItemDto> authors = await _authorService.GetAllAuthors();

        return Ok(authors);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> LoginAuthor([FromBody] AuthorCredentials authorCredentials)
    {
        var token = await _userService.Login(authorCredentials);
        return Ok(token); 
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