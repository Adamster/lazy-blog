using Lazy.Domain.Shared;

namespace Lazy.Domain.Errors;

public static class DomainErrors
{

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
            "UserToken.IsUsed",
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

    public static class FirstName
    {
        public static readonly Error Empty = new(
            "FirstName.Empty",
            "First name is empty");
        
        public static readonly Error TooLong = new(
            "FirstName.TooLong",
            "FirstName name is too long");
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
    
    public static class LastName
    {
        public static readonly Error Empty = new(
            "LastName.Empty",
            "Last name is empty");

        public static readonly Error TooLong = new(
            "LastName.TooLong",
            "Last name is too long");
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
            "Avatar file size is too large");
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
}