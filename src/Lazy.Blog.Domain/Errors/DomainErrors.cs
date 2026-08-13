using Lazy.Domain.Shared;
using Microsoft.AspNetCore.Identity;

namespace Lazy.Domain.Errors;

public static class DomainErrors
{
    public static class Shelf
    {
        public static readonly Func<Guid, Error> BookNotFound = id => new(
            "Book.NotFound",
            $"The book with the identifier {id} was not found.");

        public static readonly Func<string, Error> BookSlugNotFound = slug => new(
            "Book.NotFound",
            $"The book with the slug {slug} was not found.");

        public static readonly Func<Guid, Error> CommentNotFound = id => new(
            "BookComment.NotFound",
            $"The book comment with the identifier {id} was not found.");

        public static readonly Error UnauthorizedCommentUpdate = new(
            "BookComment.NotAuthorized",
            "Book comment can be updated only by its author");

        public static readonly Error CommentEditWindowExpired = new(
            "BookComment.EditWindowExpired",
            "Book comment can no longer be edited or deleted");

        public static readonly Error InvalidStatus = new(
            "BookStatus.Invalid",
            "Status must be 'reading' or 'read'");

        public static readonly Func<Guid, Error> GenreNotFound = id => new(
            "Genre.NotFound",
            $"Genre {id} was not found");

        public static readonly Error GenreInUse = new(
            "Genre.InUse",
            "Genre cannot be deleted while it contains books");

        public static readonly Func<Guid, Error> AuthorNotFound = id => new(
            "Author.NotFound",
            $"Author {id} was not found");

        public static readonly Error AuthorInUse = new(
            "Author.InUse",
            "Author cannot be deleted while referenced by books");

        public static readonly Func<Guid, Error> SeriesNotFound = id => new(
            "Series.NotFound",
            $"Series {id} was not found");

        public static readonly Func<string, Error> SeriesSlugNotFound = slug => new(
            "Series.NotFound",
            $"Series {slug} was not found");

        public static readonly Error SeriesInUse = new(
            "Series.InUse",
            "Series cannot be deleted while it contains books");

        public static readonly Error SeriesAlreadyExists = new(
            "Series.Conflict",
            "A series with the same slug or title already exists");

        public static readonly Error CoverUploadFailed = new(
            "BookCover.UploadFailed",
            "One or more covers could not be processed or uploaded");

        public static readonly Error BookAlreadyExists = new(
            "Book.Conflict",
            "A book with the same slug or source identifier already exists");

        public static readonly Error GenreAlreadyExists = new(
            "Genre.Conflict",
            "A genre with the same slug or name already exists");

        public static readonly Error AuthorAlreadyExists = new(
            "Author.Conflict",
            "An author with the same normalized name already exists");

        public static readonly Error DuplicateRatingSourceWorkId = new(
            "RatingSync.DuplicateSourceWorkId",
            "Each source work identifier may occur only once");

        public static readonly Func<string, Error> InvalidData = message => new("Shelf.InvalidData", message);
    }
    public static class User
    {
        public static readonly Error EmailAlreadyInUse = new(
            "User.EmailAlreadyInUse",
            "The specified email is already in use");

        public static readonly Func<Guid, Error> NotFound = id => new Error(
            "User.NotFound",
            $"The member with the identifier {id} was not found.");

        public static readonly Error InvalidCredentials = new(
            "User.InvalidCredentials",
            "The provided credentials are invalid");

        public static readonly Error UnauthorizedUserUpdate = new(
            "User.NotAuthorizedUpdate",
            "User can be updated only by the owner");

        public static Error ChangePasswordFailed(IdentityError first)
        {
            return new(first.Code, first.Description);
        }
    }

    public static class UserToken
    {
        public static readonly Error InvalidToken = new(
            "UserToken.InvalidToken",
            "The provided token is invalid");

        public static readonly Error ExpiredRefreshToken = new(
            "UserToken.ExpiredToken",
            "The provided token is expired");

        public static readonly Error AccessTokenNotExpired = new(
            "UserToken.AccessTokenNotExpired",
            "The provided access token isn't expired yet");

        public static readonly Error NotFound = new(
            "UserToken.NotFound",
            "The provided token doesn't exists");

        public static readonly Error Invalidated = new(
            "UserToken.Invalidated",
            "The provided token is invalidated");

        public static readonly Error IsUsed = new(
            "UserToken.IsUsed",
            "The provided token is already used");


        public static readonly Error NotMatched = new(
            "UserToken.NotMatched",
            "The provided token doesn't match with access token");
    }

    public static class Post
    {
        public static readonly Func<Guid, Error> NotFound = id => new Error(
            "Post.NotFound",
            $"The post with the identifier {id} was not found.");

        public static readonly Func<Domain.ValueObjects.Post.Slug, Error> SlugNotFound = slug => new Error(
            "Post.NotFound",
            $"The post with the slug {slug.Value} was not found.");

        public static readonly Error UnauthorizedPostAccess = new(
            "Post.NotAuthorized",
            "Post can be updated only by post author");

        public static readonly Error PostAlreadyVoted = new(
            "Post.AlreadyVoted",
            "Post can be voted only once");
    }

    public static class Email
    {
        public static readonly Error Empty = new(
            "Email.Empty",
            "Email is empty");

        public static readonly Error TooLong = new(
            "Email.TooLong",
            "Email is too long");

        public static readonly Error InvalidFormat = new(
            "Email.InvalidFormat",
            "Email format is invalid");
    }

    public static class Title
    {
        public static readonly Error Empty = new(
            "Title.Empty",
            "Title is empty");

        public static readonly Error TooLong = new(
            "Title.TooLong",
            "Title is too long");
    }

    public static class Tag
    {
        public static readonly Error Empty = new(
            "Tag.Empty",
            "Tag is empty");

        public static readonly Error TooLong = new(
            "Tag.TooLong",
            "Tag is too long");

        public static readonly Func<string, Error> NotFound = value => new Error(
            "Tag.NotFound",
            $"The tag with the value {value} was not found.");
    }

    public static class Body
    {
        public static readonly Error Empty = new(
            "Body.Empty",
            "Body is empty");
    }

    public static class Summary
    {
        public static readonly Error Empty = new(
            "Summary.Empty",
            "Summary is empty");

        public static readonly Error TooLong = new(
            "Summary.TooLong",
            "Summary is too long");
    }

    public static class Slug
    {
        public static readonly Error Empty = new(
            "Slug.Empty",
            "Slug is empty");

        public static Error TooLong = new(
            "Slug.TooLong",
            "Slug value is too long");

        public static readonly Error SlugAlreadyInUse = new(
            "Slug.SlugAlreadyInUse",
            "The specified slug is already in use");
    }

    public static class DisplayName
    {
        public static readonly Error Empty = new(
            "DisplayName.Empty",
            "Display name is empty");

        public static readonly Error TooLong = new(
            "DisplayName.TooLong",
            "Display name is too long");
    }

    public static class UserName
    {
        public static readonly Error Empty = new(
            "UserName.Empty",
            "User name is empty");

        public static readonly Error TooLong = new(
            "UserName.TooLong",
            "User name is too long");

        public static readonly Error UserNameAlreadyInUse = new(
            "User.UserNameAlreadyInUse",
            "The specified username is already in use");

        public static readonly Func<string, Error> NotFound = username => new Error(
            "User.NotFound",
            $"The member with the username {username} was not found.");
    }

    public static class Avatar
    {
        public static readonly Error NotSupportedExtension = new(
            "Avatar.TypeNotSupported",
            "This file type isn't supported");

        public static readonly Error NotValidUrl = new(
            "Avatar.NotValidUrl",
            "This string isn't a valid url");

        public static readonly Error EmptyUrl = new(
            "Avatar.EmptyUrl",
            "Avatar url is empty");

        public static readonly Error EmptyFileName = new(
            "Avatar.EmptyFileName",
            "Avatar file name is empty");

        public static readonly Error UploadFailed = new(
            "Avatar.UploadFailed",
            "Avatar upload failed, please try again later");

        public static readonly Error ImageFileSizeTooLarge = new(
            "Avatar.ImageFileSizeTooLarge",
            $"Avatar file size is too large. Maximum is 7mb");
        
        public static readonly Error AvatarAlreadyEmpty = new(
            "Avatar.NotAvailable",
            "There is no avatar to delete");

        public static readonly Error DeleteError = new(
            "Avatar.DeleteError",
            "Avatar delete failed, please try again later");
    }

    public static class Comment
    {
        public static readonly Func<Guid, Error> NotFound = id => new Error(
            "Comment.NotFound",
            $"The comment with the id {id} was not found.");

        public static readonly Error UnauthorizedCommentUpdate = new(
            "Comment.NotAuthorizedUpdate",
            "Comment can be updated only by comment author");
    }

    public static class Biography
    {
        public static readonly Error TooLong = new(
            "Biography.TooLong",
            "Biography is too long");
    }

    public static class MediaItem
    {
        public static readonly Error DeleteFailed = new(
            "MediaItem.DeleteFailed",
            "Delete failed, please try again later");

        public static readonly Error NotFound = new(
            "MediaItem.NotFound",
            "Media item not found");

        public static readonly Error Unauthorized = new(
            "MediaItem.UnauthorizedAccess",
            "Media item access unauthorized");
    }

    public static class ArcadeScore
    {
        public static readonly Error GameKeyEmpty = new(
            "ArcadeScore.GameKeyEmpty",
            "The game key is required");

        public static readonly Error GameKeyTooLong = new(
            "ArcadeScore.GameKeyTooLong",
            "The game key is too long");
    }
}
