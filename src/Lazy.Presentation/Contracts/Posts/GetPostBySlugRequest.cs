using System.ComponentModel.DataAnnotations;

namespace Lazy.Presentation.Contracts.Posts;

public record GetPostBySlugRequest([Required]string Slug);