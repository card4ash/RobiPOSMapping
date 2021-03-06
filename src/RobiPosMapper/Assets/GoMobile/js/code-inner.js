var $ = jQuery.noConflict();
$(function () {
    $('.form').find('input, select, textarea').on('touchstart mousedown click', function (e) {
        e.stopPropagation();
    })

    $('#header').animate({ 'top': '0' }, 400);
})

var swiperParent = new Swiper('.swiper-parent', {
    pagination: '.pagination',
    paginationClickable: true,
    onSlideChangeEnd: function () {

        $('#header').animate({ 'top': '0px' }, 400);

        ////Do something when you touch the slide
        //if (swiperParent.activeIndex != 0) {
        //    $('#header').animate({ 'top': '0px' }, 400);
        //}
        //if (swiperParent.activeIndex == 0) {
        //    $('#header').animate({ 'top': '0' }, 400);
        //}
    }
})

$('.menu').find('li a.swipeIt').each(function (i) {
    $(this).attr('onclick', 'swiperParent.swipeTo(' + (i + 1) + ')');
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
    $("#loadMore").click(function () {
        function fixheightposts() {
            swipernested.reInit();
            setTimeout(fixheightposts, 1000);
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

$('.gohome').click(function () {
    swiperParent.swipeTo(0);
});
jQuery(function ($) {
    $("#CommentForm").validate({
        submitHandler: function (form) {
            ajaxContact(form);
            return false;
        }
    });
    //$(".swipebox").swipebox();
});
