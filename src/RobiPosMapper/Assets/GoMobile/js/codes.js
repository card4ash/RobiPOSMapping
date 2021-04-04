var $ = jQuery.noConflict();
var ajaxErrorMessage = 'Failed to communicate with server. This may happen due to poor internet connection. Please try again or contact with support team.';
$(function () {
    $('.form').find('input, select, textarea').on('touchstart mousedown click', function (e) {
        e.stopPropagation();
    })
    $('#header').animate({ 'top': '0' }, 400);
})
var swiperParent = new Swiper('.swiper-parent', {
    //pagination: '.pagination',
    //paginationClickable: true,
    onSlideChangeEnd: function () {
        //Do something when you touch the slide
        $('#header').animate({ 'top': '0px' }, 400);
        if (swiperParent.activeIndex != 0) {
            $('#GoToFirst').show();
            $('#GoToPrevious').show();
        }
        else {
            $('#GoToFirst').hide();
            $('#GoToPrevious').hide();
        }

        if (swiperParent.activeIndex != swiperParent.slides.length-1) {
            $('#GoToNext').show();
            $('#GoToLast').show();
        }
        else {
            $('#GoToNext').hide();
            $('#GoToLast').hide();
        }
    }
})

$('.menu').find('li a').each(function (i) {
    $(this).attr('onclick', 'swiperParent.swipeTo(' + (i + 1) + ')');
});

$('#GoToFirst').click(function () {
    swiperParent.swipeTo(0);
});

$('#GoToPrevious').click(function () {
    swiperParent.swipeTo(swiperParent.activeIndex - 1);
});


$('#GoToLast').click(function () {
    //alert(swiperParent.slides.length);
    swiperParent.swipeTo(swiperParent.slides.length-1);
});

$('#GoToNext').click(function () {
    swiperParent.swipeTo(swiperParent.activeIndex + 1);
});

//Scroll Containers

$('.swiper-nested').each(function () {
    var swipernested = $(this).swiper({
        mode: 'vertical',
        scrollContainer: true,
        mousewheelControl: true,
        scrollbar: {
            container: $(this).find('.swiper-scrollbar')[0]
        }
    })


    $(".scrolltop").click(function () {
        swipernested.swipeTo(0);
    })
    $(".trigger").click(function () {
        function fixheighttrigger() {
            swipernested.reInit();
            setTimeout(fixheighttrigger, 1000);
        }
        setTimeout(fixheighttrigger, 1000);
    });
    $(".trigger_blog").click(function () {
        function fixheighttoogle() {
            swipernested.reInit();
            setTimeout(fixheighttoogle, 1000);
        }
        setTimeout(fixheighttoogle, 1000);
    });
    $(".tabsmenu li").click(function () {
        function fixheight() {
            swipernested.reInit();
            setTimeout(fixheight, 1000);
        }
        setTimeout(fixheight, 1000);
    });

   


    $('#IsElPos').click(function () {
        function fixheightposts() {
            swipernested.reInit();
            // setTimeout(fixheightposts, 1000);
        }
        setTimeout(fixheightposts, 1000);
    });

    $('#AddElMsisdn').on('click', function (e) {
        e.preventDefault();
        function fixheightposts() {
            swipernested.reInit();
            // setTimeout(fixheightposts, 1000);
        }
        setTimeout(fixheightposts, 1000);
    });

    $(document).on('click', 'button.RemoveElMsisdn', function (e) {
        function fixheightposts() {
            swipernested.reInit();
            // setTimeout(fixheightposts, 1000);
        }
        setTimeout(fixheightposts, 1000);
    });


    $('#IsSimPos').click(function () {
        function fixheightposts() {
            swipernested.reInit();
        }
        setTimeout(fixheightposts, 1000);
    });

    $('#AddSimPosCode').on('click', function (e) {
        e.preventDefault();
        function fixheightposts() {
            swipernested.reInit();
        }
        setTimeout(fixheightposts, 1000);
    });
  
    $(document).on('click', 'button.RemoveSimPosCode', function (e) {
        e.preventDefault();
        function fixheightposts() {
            swipernested.reInit();
        }
        setTimeout(fixheightposts, 1000);
    });


    $("#RetailerPhotoInput").change(function () {
        function fixheightposts() {
            swipernested.reInit();
        }
        setTimeout(fixheightposts, 1000);
    });

    $("#QrPhotoInput").change(function () {
        function fixheightposts() {
            swipernested.reInit();
        }
        setTimeout(fixheightposts, 1000);
    });

    $("#GeoServiceToggle").click(function (e) {
        e.preventDefault();
        function fixheightposts() {
            swipernested.reInit();
            // setTimeout(fixheightposts, 1000);
        }
        setTimeout(fixheightposts, 1000);
    });

    $("#SubmitRetailer").click(function (e) {
        e.preventDefault();
        function fixheightposts() {
            swipernested.reInit();
        }
        setTimeout(fixheightposts, 1000);
    });







    $(".post_details_page li").hide();
    $(".posts li").click(function () {

        p_ID = this.id;

        $(".post_details_page").find("li").each(function () {
            if (this.id == p_ID) {
                $(".posts_archive_page").hide();
                var detailspostid = $(".post_details_page li#" + this.id);
                detailspostid.show();
                swipernested.reInit();
                $('.backtoblog').click(function () {
                    detailspostid.hide();
                    $(".posts_archive_page").show();
                    swipernested.reInit();
                });

            }


        });

    });

})


