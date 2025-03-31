using Lazy.Domain.Primitives;

namespace Lazy.Domain.Entities
{
    public sealed class MediaItem : Entity, IAuditableEntity
    {

        public MediaItem(
            Guid id,
            string url,
            User user) 
            : base(id)
        {
            UserId = user.Id;
            UploadedUrl = url;
        }

        private MediaItem()
        {
        }

        public string UploadedUrl { get; set; } = null!;
        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;

        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

        public static MediaItem Create(User user, string url)
        {
            var mediaItem = new MediaItem(Guid.NewGuid(), url, user);
            return mediaItem;
        }

        public static MediaItem Create(Guid fileId, User user, string url)
        {
            var mediaItem = new MediaItem(fileId, url, user);
            return mediaItem;
        }
    }
}
