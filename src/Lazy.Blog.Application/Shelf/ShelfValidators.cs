using FluentValidation;
using Lazy.Domain.Entities;

namespace Lazy.Application.Shelf;

public sealed class GetBooksQueryValidator : AbstractValidator<GetBooksQuery>
{ public GetBooksQueryValidator() { RuleFor(x => x.Offset).GreaterThanOrEqualTo(0); RuleFor(x => x.Limit).InclusiveBetween(1, 100); } }
public sealed class GetUserBooksQueryValidator : AbstractValidator<GetUserBooksQuery>
{ public GetUserBooksQueryValidator() { RuleFor(x => x.UserName).NotEmpty(); RuleFor(x => x.Status).Must(x => x is "read" or "reading"); RuleFor(x => x.Limit).InclusiveBetween(1, 100); } }
public sealed class SetBookStatusCommandValidator : AbstractValidator<SetBookStatusCommand>
{ public SetBookStatusCommandValidator() { RuleFor(x => x.BookId).NotEmpty(); RuleFor(x => x.Status).Must(x => x is "read" or "reading"); } }
public sealed class AddBookCommentCommandValidator : AbstractValidator<AddBookCommentCommand>
{ public AddBookCommentCommandValidator() { RuleFor(x => x.BookId).NotEmpty(); RuleFor(x => x.Body).NotEmpty().MaximumLength(BookComment.MaxLength); } }
public sealed class UpdateBookCommentCommandValidator : AbstractValidator<UpdateBookCommentCommand>
{ public UpdateBookCommentCommandValidator() { RuleFor(x => x.CommentId).NotEmpty(); RuleFor(x => x.Body).NotEmpty().MaximumLength(BookComment.MaxLength); } }
public sealed class ImportBookRatingsCommandValidator : AbstractValidator<ImportBookRatingsCommand>
{ public ImportBookRatingsCommandValidator() { RuleFor(x => x.Source).NotEmpty().MaximumLength(50); RuleFor(x => x.Items).NotEmpty().Must(x => x.Select(i => i.SourceWorkId).Distinct(StringComparer.Ordinal).Count() == x.Count).WithMessage("SourceWorkId values must be unique"); RuleForEach(x => x.Items).ChildRules(i => { i.RuleFor(x => x.SourceWorkId).NotEmpty().MaximumLength(100); i.RuleFor(x => x.Rating).InclusiveBetween(0, 10); i.RuleFor(x => x.RatingCount).GreaterThanOrEqualTo(0); }); } }
public sealed class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{ public CreateBookCommandValidator(){RuleFor(x=>x.Data).SetValidator(new ManageBookDataValidator(false));RuleFor(x=>x.Cover).NotNull().Must(x=>x.Length is >0 and <=20*1024*1024);}}
public sealed class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
{ public UpdateBookCommandValidator(){RuleFor(x=>x.Id).NotEmpty();RuleFor(x=>x.Data).SetValidator(new ManageBookDataValidator(false));When(x=>x.Cover is not null,()=>RuleFor(x=>x.Cover!).Must(x=>x.Length is >0 and <=20*1024*1024));}}
public sealed class ManageBookDataValidator : AbstractValidator<ManageBookData>
{ public ManageBookDataValidator(bool requireCoverUrl=true){RuleFor(x=>x.GenreId).NotEmpty();RuleFor(x=>x.AuthorId).NotEmpty();RuleFor(x=>x.Slug).NotEmpty().MaximumLength(160);RuleFor(x=>x.Title).NotEmpty().MaximumLength(300);RuleFor(x=>x.Series).MaximumLength(300);if(requireCoverUrl)RuleFor(x=>x.CoverUrl).NotEmpty().MaximumLength(1000);RuleFor(x=>x.Rating).InclusiveBetween(0,10);RuleFor(x=>x.RatingCount).GreaterThanOrEqualTo(0);RuleFor(x=>x.RatingSource).NotEmpty().MaximumLength(50);RuleFor(x=>x.SourceWorkId).MaximumLength(100);RuleFor(x=>x.SourceUrl).MaximumLength(1000);RuleFor(x=>x.SeriesTitles).NotNull();RuleForEach(x=>x.SeriesTitles).NotNull().NotEmpty().MaximumLength(300);}}
public sealed class CreateAuthorCommandValidator:AbstractValidator<CreateAuthorCommand>{public CreateAuthorCommandValidator(){RuleFor(x=>x.Name).NotEmpty().MaximumLength(300);}}
public sealed class UpdateAuthorCommandValidator:AbstractValidator<UpdateAuthorCommand>{public UpdateAuthorCommandValidator(){RuleFor(x=>x.Id).NotEmpty();RuleFor(x=>x.Name).NotEmpty().MaximumLength(300);}}
public sealed class CreateGenreCommandValidator : AbstractValidator<CreateGenreCommand>
{public CreateGenreCommandValidator(){RuleFor(x=>x.Slug).NotEmpty().MaximumLength(50);RuleFor(x=>x.Name).NotEmpty().MaximumLength(100);}}
public sealed class UpdateGenreCommandValidator : AbstractValidator<UpdateGenreCommand>
{public UpdateGenreCommandValidator(){RuleFor(x=>x.Id).NotEmpty();RuleFor(x=>x.Slug).NotEmpty().MaximumLength(50);RuleFor(x=>x.Name).NotEmpty().MaximumLength(100);}}
