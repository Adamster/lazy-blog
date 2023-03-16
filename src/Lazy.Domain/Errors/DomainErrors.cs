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

    public static class Post
    {
        public static readonly Func<Guid, Error> NotFound = id => new Error(
            "Post.NotFound",
            $"The post with the identifier {id} was not found.");

        public static readonly Func<Domain.ValueObjects.Post.Slug, Error> SlugNotFound = slug => new Error(
            "Post.NotFound",
            $"The post with the slug {slug} was not found.");
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
            "LastName.TooLong",
            "FirstName name is too long");
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
}