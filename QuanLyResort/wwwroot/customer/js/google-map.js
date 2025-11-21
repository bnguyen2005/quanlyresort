function initMap() {
    // Kiểm tra xem Google Maps API đã load chưa
    if (typeof google === 'undefined' || typeof google.maps === 'undefined') {
        console.error('Google Maps API chưa được load');
        setTimeout(initMap, 100); // Retry sau 100ms
        return;
    }
    
    // Get the HTML DOM element that will contain your map 
    var mapElement = document.getElementById('map') || document.querySelector('.map');
    if (!mapElement) {
        console.warn('Không tìm thấy element #map');
        return; // không có vùng map trên trang hiện tại
    }

    // Địa chỉ Resort Deluxe: 123 Đường Biển Xanh, Thành phố Biển, Việt Nam
    // Sử dụng tọa độ trực tiếp (Nha Trang, Việt Nam) thay vì geocoding để tránh lỗi API key
    // Tọa độ: 12.2388, 109.1967 (Nha Trang - gần biển)
    var myLatlng = new google.maps.LatLng(12.2388, 109.1967);
    
    var mapOptions = {
        zoom: 16,
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

    // Create the Google Map using our element and options defined above
    var map = new google.maps.Map(mapElement, mapOptions);
    
    // Địa chỉ Resort Deluxe
    var address = '123 Đường Biển Xanh, Thành phố Biển, Việt Nam';

    // Sử dụng tọa độ trực tiếp thay vì geocoding (tránh lỗi API key)
    // Add marker
    var marker = new google.maps.Marker({
        position: myLatlng,
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
                 '<p style="margin: 8px 0 0 0; color: #666; font-size: 13px;">📧 support@resortdeluxe.vn</p>' +
                 '</div>'
    });
    
    // Show info window on marker click
    marker.addListener('click', function() {
        infoWindow.open(map, marker);
    });
    
    // Open info window by default
    infoWindow.open(map, marker);
}

// Expose initMap to global scope for Google Maps API callback
window.initMap = initMap;