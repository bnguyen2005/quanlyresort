function initMap() {
    // Kiểm tra xem Google Maps API đã load chưa
    if (typeof google === 'undefined' || typeof google.maps === 'undefined') {
        console.error('Google Maps API chưa được load');
        setTimeout(initMap, 100); // Retry sau 100ms
        return;
    }
    
    // Kiểm tra xem jQuery đã load chưa
    if (typeof jQuery === 'undefined' || typeof $ === 'undefined') {
        console.error('jQuery chưa được load');
        setTimeout(initMap, 100); // Retry sau 100ms
        return;
    }
    
    // Địa chỉ Resort Deluxe: 123 Đường Biển Xanh, Thành phố Biển, Việt Nam
    // Tọa độ mặc định (có thể thay đổi sau khi geocode)
    // Đây là tọa độ gần biển ở Việt Nam (ví dụ: Nha Trang)
    var myLatlng = new google.maps.LatLng(12.2388, 109.1967);
    
    var mapOptions = {
        zoom: 15,
        center: myLatlng,
        scrollwheel: true,
        mapTypeControl: true,
        streetViewControl: true,
        fullscreenControl: true,
        styles: [
            {
                "featureType": "poi",
                "elementType": "labels",
                "stylers": [{"visibility": "off"}]
            }
        ]
    };

    // Get the HTML DOM element that will contain your map 
    var mapElement = document.getElementById('map') || document.querySelector('.map');
    if (!mapElement) {
        console.warn('Không tìm thấy element #map');
        return; // không có vùng map trên trang hiện tại
    }

    // Create the Google Map using our element and options defined above
    var map = new google.maps.Map(mapElement, mapOptions);
    
    // Địa chỉ Resort Deluxe
    var address = '123 Đường Biển Xanh, Thành phố Biển, Việt Nam';

    // Geocode địa chỉ và thêm marker
    $.getJSON('https://maps.googleapis.com/maps/api/geocode/json?address=' + encodeURIComponent(address) + '&key=AIzaSyBVWaKrjvy3MaE7SQ74_uJiULgl1JY0H2s', null, function (data) {
        if (data.status === 'OK' && data.results && data.results.length > 0) {
            var location = data.results[0].geometry.location;
            var latlng = new google.maps.LatLng(location.lat, location.lng);
            
            // Center map to the location
            map.setCenter(latlng);
            map.setZoom(16);
            
            // Add marker
            var marker = new google.maps.Marker({
                position: latlng,
                map: map,
                title: 'Resort Deluxe',
                animation: google.maps.Animation.DROP
            });
            
            // Add info window
            var infoWindow = new google.maps.InfoWindow({
                content: '<div style="padding: 10px; max-width: 250px;">' +
                         '<h4 style="margin: 0 0 8px 0; color: #c8a97e; font-weight: 700;">🏨 Resort Deluxe</h4>' +
                         '<p style="margin: 0; line-height: 1.6; color: #555;">' + address + '</p>' +
                         '<p style="margin: 8px 0 0 0; color: #666; font-size: 13px;">📞 +84 901 329 227</p>' +
                         '</div>'
            });
            
            // Show info window on marker click
            marker.addListener('click', function() {
                infoWindow.open(map, marker);
            });
            
            // Open info window by default
            infoWindow.open(map, marker);
        } else {
            console.warn('Geocoding failed, using default coordinates');
            // Fallback: Use default coordinates if geocoding fails
            var marker = new google.maps.Marker({
                position: myLatlng,
                map: map,
                title: 'Resort Deluxe',
                animation: google.maps.Animation.DROP
            });
            
            var infoWindow = new google.maps.InfoWindow({
                content: '<div style="padding: 10px;"><h4 style="margin: 0 0 8px 0; color: #c8a97e;">🏨 Resort Deluxe</h4><p style="margin: 0;">' + address + '</p></div>'
            });
            
            marker.addListener('click', function() {
                infoWindow.open(map, marker);
            });
            infoWindow.open(map, marker);
        }
    }).fail(function(jqXHR, textStatus, errorThrown) {
        console.error('Geocoding request failed:', textStatus, errorThrown);
        // Fallback if geocoding request fails
        var marker = new google.maps.Marker({
            position: myLatlng,
            map: map,
            title: 'Resort Deluxe'
        });
        
        var infoWindow = new google.maps.InfoWindow({
            content: '<div style="padding: 10px;"><h4 style="margin: 0 0 8px 0; color: #c8a97e;">🏨 Resort Deluxe</h4><p style="margin: 0;">' + address + '</p></div>'
        });
        
        marker.addListener('click', function() {
            infoWindow.open(map, marker);
        });
        infoWindow.open(map, marker);
    });
}

// Expose initMap to global scope for Google Maps API callback
window.initMap = initMap;