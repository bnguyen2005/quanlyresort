
var google;

function initMap() {
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
    if (!mapElement) return; // không có vùng map trên trang hiện tại

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
    }).fail(function() {
        // Fallback if geocoding request fails
        var marker = new google.maps.Marker({
            position: myLatlng,
            map: map,
            title: 'Resort Deluxe'
        });
    });
}

window.initMap = initMap;