/**
 * Sidebar Toggle Handler
 * Handles the visibility of logo and EquiBillBook text based on sidebar state
 * When sidebar is open: show both logo and text
 * When sidebar is closed: show only logo
 */

$(document).ready(function() {
    
    // Function to update brand visibility based on sidebar state
    function updateBrandVisibility() {
        var $body = $('body');
        var $sidebar = $('.main-sidebar');
        var $brandText = $('.brand-link .brand-text');
        var $brandIcon = $('.brand-link .brand-icon');
        
        // Check actual sidebar width to determine if it's expanded or collapsed
        var sidebarWidth = $sidebar.outerWidth();
        var isCollapsed = sidebarWidth <= 60; // Collapsed sidebar is typically around 55-60px wide
        
        //console.log('Sidebar width:', sidebarWidth, 'Is collapsed:', isCollapsed);
        
        if (isCollapsed) {
            // Sidebar is collapsed - show only logo
            //console.log('Hiding brand text (sidebar collapsed)');
            $brandText.hide();
            $brandIcon.show();
        } else {
            // Sidebar is expanded - show both logo and text
            //console.log('Showing brand text (sidebar expanded)');
            
            // Ensure brand text has content, if not, add fallback
            if ($brandText.length > 0 && $brandText.text().trim() === '') {
                $brandText.text('EquiBillBook');
            }
            
            $brandText.css({
                'display': 'inline',
                'visibility': 'visible',
                'opacity': '1'
            });
            $brandIcon.css({
                'display': 'inline-block',
                'visibility': 'visible',
                'opacity': '1'
            });
        }
    }
    
    // Initial setup on page load
    updateBrandVisibility();
    
    // Fallback: Check every 2 seconds to ensure correct state
    setInterval(function() {
        updateBrandVisibility();
    }, 2000);
    
    // Watch for clicks on the pushmenu toggle button
    $(document).on('click', '[data-widget="pushmenu"]', function() {
        // Small delay to allow AdminLTE to process the toggle first
        setTimeout(function() {
            updateBrandVisibility();
        }, 50);
    });
    
    // Watch for programmatic changes to sidebar state
    // This handles cases where the sidebar might be toggled programmatically
    var observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.type === 'attributes' && 
                (mutation.attributeName === 'class' || mutation.attributeName === 'style') && 
                (mutation.target.tagName === 'BODY' || mutation.target.classList.contains('main-sidebar'))) {
                setTimeout(function() {
                    updateBrandVisibility();
                }, 100);
            }
        });
    });
    
    // Start observing body and sidebar changes
    observer.observe(document.body, {
        attributes: true,
        attributeFilter: ['class', 'style'],
        subtree: true
    });
    
    // Handle window resize events (AdminLTE auto-collapses on small screens)
    $(window).on('resize', function() {
        setTimeout(function() {
            updateBrandVisibility();
        }, 100);
    });
    
    // Handle hover effects on sidebar
    $('.main-sidebar').on('mouseenter', function() {
        var $sidebar = $('.main-sidebar');
        var $brandText = $('.brand-link .brand-text');
        var sidebarWidth = $sidebar.outerWidth();
        
        //console.log('Mouse enter - Sidebar width:', sidebarWidth);
        
        // Always show brand text on hover (whether sidebar is collapsed or expanded)
        // Ensure brand text has content, if not, add fallback
        if ($brandText.length > 0 && $brandText.text().trim() === '') {
            $brandText.text('EquiBillBook');
        }
        
        $brandText.css({
            'display': 'inline',
            'visibility': 'visible',
            'opacity': '1'
        });
    });
    
    $('.main-sidebar').on('mouseleave', function() {
        // After mouse leave, check the actual sidebar state and update accordingly
        setTimeout(function() {
            updateBrandVisibility();
        }, 50);
    });
    
    // Clean up observer when page unloads
    $(window).on('beforeunload', function() {
        observer.disconnect();
    });
});
