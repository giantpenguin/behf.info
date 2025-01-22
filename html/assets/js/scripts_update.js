document.addEventListener('DOMContentLoaded', function() {
	"use strict";
	/* ----------------------------------------------------------------
	Dropdown
	---------------------------------------------------------------- */
	document.addEventListener('click', function (e) {
		var target = e.target;
		// if (target.classList.contains('dropdown-toggle') && target.getAttribute('data-bs-toggle') == 'dropdown') {
		// 	var dropdown = target.closest('.dropdown');
		// 	if (dropdown) {
		// 		dropdown.classList.add('show');
		// 		var menu = dropdown.querySelector('.dropdown-menu')
		// 		if (menu){
		// 			menu.classList.add('show');
		// 		}
		// 	}
		// }
		// else {
		// 	var dropdowns = document.querySelectorAll('.dropdown.show');
		// 	dropdowns.forEach(function(dropdown){
		// 	dropdown.classList.remove('show');
		// 	var menu = dropdown.querySelector('.dropdown-menu');
		// 		if (menu){
		// 		menu.classList.remove('show');
		// 		}
		// 	});
			// var searchOverlay = document.querySelector('.search-overlay');
			// if (searchOverlay) {
			// 	searchOverlay.style.display = 'none';
			// }
		// }
	});

	$('.search_trigger').magnificPopup({
        type: 'inline',
        midClick: true,
        mainClass: 'mfp-fade',
        removalDelay: 160,
        callbacks: {
           beforeOpen: function() {
             this.st.mainClass = this.st.el.attr('data-effect');
           },
            open: function() {
               $.magnificPopup.instance.close = function() {
                if (this.currItem) {
                        this.currItem.el.focus();
                    }
                  $.magnificPopup.proto.close.call(this);
               };
               this.content.focus();
           }
       }
     });
});