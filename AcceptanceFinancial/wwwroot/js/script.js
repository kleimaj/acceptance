$(document).ready(function () {
  // Custome Scrollbar Start
  $(".navbar .navbar-toggler").click(function () {
    $(this).toggleClass("show");
  });
  $(".navbar .navbar-toggler").click(function () {
    $("body").toggleClass("body-show");
  });
  $(".navbar-collapse .navbar-nav .nav-item .nav-link").click(function () {
    $(".navbar-collapse").removeClass("show");
    $(".navbar .navbar-toggler").removeClass("show");
  });

  $('.navbar-toggler-icon').click(function () {
		$(this).toggleClass('open');
	});

  $('.people-slider').slick({
    infinite: true,
    arrows: false,
    slidesToShow: 1,
    slidesToScroll: 1,
    dots: true,
    prevArrow: '<button class="slide-arrow prev-arrow"></button>',
    nextArrow: '<button class="slide-arrow next-arrow"></button>',

  });

/*
  $('select').selectric({
    disableOnMobile: false
  });
*/
  $('.slider-possible').slick({
    dots: true,
    arrows: true,
    infinite: true,
    speed: 300,
    slidesToShow: 3,
    slidesToScroll: 1,
    prevArrow: '<button class="slide-arrow prev-arrow"></button>',
    nextArrow: '<button class="slide-arrow next-arrow"></button>',
    responsive: [
      {
        breakpoint: 1024,
        settings: {
          slidesToShow: 2,
          slidesToScroll: 2,
        }
      },
      {
        breakpoint: 991,
        settings: {
          slidesToShow: 1,
          slidesToScroll: 1,
        }
      },
      {
        breakpoint: 600,
        settings: {
          slidesToShow: 1,
          slidesToScroll: 1
        }
      },
      {
        breakpoint: 480,
        settings: {
          slidesToShow: 1,
          slidesToScroll: 1
        }
      }
    ]
  });

  AOS.init();
});


