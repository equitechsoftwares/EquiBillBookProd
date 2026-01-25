$(document).ready(function () {

    // HEADER STICKY
    ///////////////////////////////////////////////
    window.onscroll = function() {myFunction()};
    var header = document.getElementById("stickynavbar");
    var sticky = header.offsetTop;
    function myFunction() {
      if (window.pageYOffset > sticky) {
        header.classList.add("sticky");
      } else {
        header.classList.remove("sticky");
      }
    }

    // ANIMATE CSS
/*    new WOW().init();*/


    // SMOOTH SCROLL
    ///////////////////////////////////////////////
    $('a[href*="#"]')
    // Remove links that don't actually link to anything
    .not('[href="#"]')
    .not('[href="#0"]')
    .click(function(event) {
      // On-page links
      if (
        location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') 
        && 
        location.hostname == this.hostname
      ) {
        // Figure out element to scroll to
        var target = $(this.hash);
        target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
        // Does a scroll target exist?
        if (target.length) {
          // Only prevent default if animation is actually gonna happen
          event.preventDefault();
          $('html, body').animate({
            scrollTop: target.offset().top
          }, 1000, function() {
            // Callback after animation
            // Must change focus!
            var $target = $(target);
            $target.focus();
            if ($target.is(":focus")) { // Checking if the target was focused
              return false;
            } else {
              $target.attr('tabindex','-1'); // Adding tabindex for elements not focusable
              $target.focus(); // Set focus again
            };
          });
        }
      }
    });

    //ODOMETER 
    ////////////////////////////////////////////////
    if ($(".odometer").length) {
      $('.odometer').appear();
      $(document.body).on('appear', '.odometer', function(e) {
          var odo = $(".odometer");
          odo.each(function() {
              var countNumber = $(this).attr("data-count");
              $(this).html(countNumber);
          });
      });
    };

  // BANNER PANEL
  /////////////////////////////////////////////////////
  var swiper = new Swiper(".banner-slider", {
    spaceBetween: 30,
    effect: "fade",
    autoplay: {
      delay: 2000,
      disableOnInteraction: false,
    },
    pagination: {
      el: ".swiper-pagination",
      clickable: true,
    },
  });
    
    // CLIENT SLIDER
    ///////////////////////////////////////////////
    var swiper = new Swiper(".client-slider", {
      slidesPerView: 1,
      spaceBetween: 20,
      loop: true,
      loopFillGroupWithBlank: true,
      navigation: {
        nextEl: ".swiper-button-next",
        prevEl: ".swiper-button-prev",
      },
      autoplay: {
        delay: 2500,
        disableOnInteraction: false,
      },
      pagination: {
        el: ".swiper-pagination",
        clickable: true,
      },
      breakpoints: {
        640: {
          slidesPerView: 2,
        },
        768: {
          slidesPerView: 3,
        },
        1024: {
          slidesPerView: 6,
        },
      },
    });

    // UNIVERSITY SLIDER
    ///////////////////////////////////////////////
    var swiper = new Swiper(".university-slider", {
      slidesPerView: 1,
      spaceBetween: 10,
      loop: true,
      loopFillGroupWithBlank: true,
      navigation: {
        nextEl: ".swiper-button-next",
        prevEl: ".swiper-button-prev",
      },
      autoplay: {
        delay: 2500,
        disableOnInteraction: false,
      },
      pagination: {
        el: ".swiper-pagination",
        clickable: true,
      },
      breakpoints: {
        640: {
          slidesPerView: 2,
        },
        768: {
          slidesPerView: 3,
        },
        1024: {
          slidesPerView: 5,
        },
      },
    });

    

    // TESTIMONIAL SLIDER
    ///////////////////////////////////////////////
    var swiper = new Swiper(".testimonial-slider", {
      spaceBetween: 30,
      centeredSlides: true,
      autoplay: {
        delay: 2500,
        disableOnInteraction: false,
      },
      pagination: {
        el: ".swiper-pagination",
        clickable: true,
      },
      // navigation: {
      //   nextEl: ".swiper-button-next",
      //   prevEl: ".swiper-button-prev",
      // },
    });

    
    
    
});