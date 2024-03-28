


//filter Js
$(document).ready(function () {
    $('.filter-item').click(function () {
        const value = $(this).attr('data-filter')
        if (value == 'all') {
            $('.post-box').show('1000');
        }
        else {
            $('.post-box').not('.' + value).hide('1000');
            $('.post-box').filter('.' + value).show('1000');
        }
    });

    //add active to btn

    $('.filter-item').click(function () {
        $(this).addClass("active-filter").siblings().removeClass("active-filter");
    })
});

////header background change on scroll

//let header = document.querySelector('header')   
//window.addEventListener('scroll', () => {
//    header.classList.toggle("shadow", window.scrollY > 0);
//})


//Swiper
const swiper = new Swiper('.swiper', {
    // Optional parameters
    direction: 'vertical',
    loop: true,

    // If we need pagination
    pagination: {
        el: '.swiper-pagination',
    },

    // Navigation arrows
    navigation: {
        nextEl: '.swiper-button-next',
        prevEl: '.swiper-button-prev',
    },

    // And if we need scrollbar
    scrollbar: {
        el: '.swiper-scrollbar',
    },
});


