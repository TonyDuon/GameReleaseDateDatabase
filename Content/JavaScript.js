$(document).on("click", "#btnLike", function () {
    
    var id = $(this).data("id");
    var link = "/Home/LikeAction/" + id;
    var a = $(this);
    $.ajax({
        type: "GET",
        url: link,
        success: function (result) {
            //return the new voting result without refreshing
            $('.upvoteText#' + id).html(result);

            //re-enable the dislike button if disabled -> vote score is now back to zero
            if ($('[data-id="' + id + '"]#btnDislike').is(":disabled")) {
                $('[data-id="' + id + '"]#btnDislike').prop("disabled", false);
            }
            else {
                //disable the like button -> vote is now incremented
                a.prop('disabled', true);
            }
        },
        error: function (xmlHttpRequest, errorText, thrownError) {
            alert(xmlHttpRequest + " | " + errorText + " | " + thrownError);
        }
    });
});

$(document).on("click", "#btnDislike", function () {
    var id = $(this).data("id");
    var link = "/Home/DislikeAction/" + id;
    var a = $(this);
    $.ajax({
        type: "GET",
        url: link,
        success: function (result) {
            $('.upvoteText#' + id).html(result);
            
            //re-enable the like button if disabled -> vote score is now back to zero
            if ($('[data-id="' + id + '"]#btnLike').is(":disabled")) {
                $('[data-id="' + id + '"]#btnLike').prop("disabled", false);
            }
            else {
                //disable the like button -> vote is now decremented
                a.prop('disabled', true);
            }

        },
        error: function (xmlHttpRequest, errorText, thrownError) {
            alert(xmlHttpRequest + " | " + errorText + " | " + thrownError);
        }
    });
});
