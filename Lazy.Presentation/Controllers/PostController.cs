using Lazy.DataContracts.Post;
using Lazy.Domain;
using Lazy.Presentation.Models.Post;
using Lazy.Services.Post;
using Mapster;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Lazy.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;

    public PostController(IPostService postService)
    {
        _postService = postService;
    }

    // GET: api/<PostController>
    [HttpGet]
    public async Task<ActionResult<IList<PostItemDto>>> Get()
    {
        IList<PostItemDto> posts = await _postService.GetPostList();
        var postsModel = posts.Adapt<IList<PostItemDto>>();

        return Ok(postsModel);
    }

    // GET api/<PostController>/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PostItemDto>> Get(Guid id)
    {
        PostItemDto? post = await _postService.GetPostById(id);

        if (post is null)
        {
            return NotFound();
        }

        return Ok(post.Adapt<PostItemModel>());
    }

    // POST api/<PostController>
    [HttpPost]
    public async Task<CreatedAtActionResult> Post([FromBody] CreatePostDto postToCreate)
    {
        var createdPost = await _postService.CreatePost(postToCreate);
        return CreatedAtAction(nameof(Get), new { id = createdPost.Id }, createdPost);
    }

    // PUT api/<PostController>/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdatePostDto updatedPost)
    {
        if (id != updatedPost.Id)
        {
            return BadRequest();
        }

        var existingPost = await _postService.GetPostById(id);
        if (existingPost is null)
        {
            return NotFound();
        }

        await _postService.UpdatePost(updatedPost);

        return NoContent();
    }

    // PUT api/<PostController>/5
    [HttpPut("{id}/publish")]
    public void Publish(Guid id)
    {
    }

    // PUT api/<PostController>/5
    [HttpPut("{id}/unpublish")]
    public void UnPublish(Guid id)
    {
    }

    // DELETE api/<PostController>/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existingPost = await _postService.GetPostById(id);
        if (existingPost is null)
        {
            return NotFound();
        }

        await _postService.DeletePost(id);

        return NoContent();
    }
}