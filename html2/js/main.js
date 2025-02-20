$(document).ready(function(){
    fixedHeaderOnScroll();
    fadeInOnScroll();
    $(window).scroll(function(){
        fixedHeaderOnScroll();
        fadeInOnScroll();
    });
    $("#btnHeroScroll").on('click', function(e){
        e.preventDefault();
        $([document.documentElement, document.body]).animate({
            scrollTop: $(this).closest('.section-hero').height()
        }, 200);
    });
});

function fadeInOnScroll(){
    $('.a-fadein').each(function(){
        const scroll = window.scrollY + window.innerHeight,
            distance = $(this).offset().top;
        if(scroll > distance){
            console.log(true)
            $(this).addClass('a-fadein--init');
        } 
    });
}

function fixedHeaderOnScroll(){
    var st = $('html').scrollTop()
        header = $('.site-navbar');
    if(st > 128){
        header.addClass('size-minimal');
        if(header.hasClass('on-hero')){
            header.removeClass('theme-hero');
        }
    }
    else{
        header.removeClass('size-minimal');
        if(header.hasClass('on-hero')){
            header.addClass('theme-hero');
        }
    }
}

$(document).ready(function(){
    $('.c-carousel').each(function(){
        const itemsXS = this.getAttribute('data-items-xs'),
            itemsMD = this.getAttribute('data-items-md'),
            itemsLG = this.getAttribute('data-items-lg'),
            itemLoop = this.getAttribute('data-loop'),
            itemCenter = this.getAttribute('data-center'),
            itemDrag = this.getAttribute('data-drag'),
            itemNav = this.getAttribute('data-nav'),
            itemDots = this.getAttribute('data-dots'),
            itemMargin = this.getAttribute('data-margin'),
            itemAutoHeight = this.getAttribute('data-auto-height');
        $(this).owlCarousel({
            responsive:{
                0:{
                    items: itemsXS !== null ? Number(itemsXS) : 1,
                    slideBy: itemsXS !== null && itemCenter == 'false' ? Number(itemsXS) : 1,
                },
                720:{
                    items: itemsMD !== null ? Number(itemsMD) : 1,
                    slideBy: itemsMD !== null && itemCenter == 'false' ? Number(itemsMD) : 1,
                },
                1280:{
                    items: itemsLG !== null ? Number(itemsLG) : 1,
                    slideBy: itemsLG !== null && itemCenter == 'false' ? Number(itemsLG) : 1,
                }
            },
            loop: (itemLoop == 'true'),
            center: (itemCenter == 'true'),
            mouseDrag: (itemDrag == 'true'),
            touchDrag: (itemDrag == 'true'),
            nav: (itemNav == 'true'),
            dots: (itemDots == 'true'),
            margin: itemMargin !== null ? Number(itemMargin) : 32,
            autoHeight: (itemAutoHeight == 'true'),
        });
    });
});