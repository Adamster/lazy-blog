// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
console.log("site js executed")
var container = document.getElementById("infinite-scroll-container");
var loadingIndicator = document.getElementById("loading-indicator");
var pageNumber = 1;
var hasMore = true;

container.addEventListener("scroll", function () {
    debugger;
    if ((container.scrollHeight - container.scrollTop) <= container.clientHeight) {
        if (hasMore) {
            loadMore();
        }
    }
});

function loadMore() {
    loadingIndicator.style.display = "block";
    fetch("/home/index?pageNumber=" + pageNumber)
        .then(response => response.json())
        .then(data => {
            if (data.length === 0) {
                hasMore = false;
                loadingIndicator.style.display = "none";
                return;
            }

            for (var i = 0; i < data.length; i++) {
                var post = data[i];
                var card = document.createElement("div");
                card.classList.add("card");
                card.innerHTML = "<div class='card-header'>" +
                    "<h5 class='card-title'>" + post.Title + "</h5>" +
                    "</div>" +
                    "<div class='card-body'>" +
                    "<p class='card-text'>" + post.Description + "</p>" +
                    "<p class='card-text'>" + post.Content + "</p>" +
                    "</div>" +
                    "<div class='card-footer'>" +
                    "<small class='text-muted'>Author: " + post.AuthorName + " | Likes: " + post.LikeCount + " | Comments: " + post.CommentsCount + " | Date posted: " + post.DatePosted + "</small>" +
                    "</div>";
                container.appendChild(card);
            }
            pageNumber++;
            loadingIndicator.style.display = "none";
        });
}
