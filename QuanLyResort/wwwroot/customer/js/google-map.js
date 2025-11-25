function initMap() {
    try {
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
    } catch (error) {
        console.error('Error initializing map:', error);
        var mapElement = document.getElementById('map');
        if (mapElement) {
            mapElement.innerHTML = '<div style="padding: 40px; text-align: center; color: #666; background: #f8f9fa; border-radius: 8px;"><p style="font-size: 16px; margin-bottom: 10px; color: #dc3545;">⚠️ Lỗi khởi tạo bản đồ</p><p style="font-size: 14px; color: #555;">Địa chỉ HUFLIT: 806 Lê Quang Đạo, Trung Mỹ Tây, Quận 12, TP.HCM</p></div>';
        }
        return;
    }

    // Địa chỉ HUFLIT - Cơ sở Hóc Môn
    var huflitAddress = '806 Lê Quang Đạo, Trung Mỹ Tây, Quận 12, Thành phố Hồ Chí Minh, Việt Nam';
    // Tọa độ HUFLIT (gần đúng)
    var huflitLatlng = new google.maps.LatLng(10.8765, 106.6297);
    
    var mapOptions = {
        zoom: 13,
        center: huflitLatlng,
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
    var map;
    try {
        map = new google.maps.Map(mapElement, mapOptions);
    } catch (error) {
        console.error('Error creating map:', error);
        mapElement.innerHTML = '<div style="padding: 40px; text-align: center; color: #666; background: #f8f9fa; border-radius: 8px;"><p style="font-size: 16px; margin-bottom: 10px; color: #dc3545;">⚠️ Lỗi API Key</p><p style="font-size: 14px; color: #555;">Google Maps API key không hợp lệ. Vui lòng liên hệ admin.</p><p style="font-size: 14px; color: #555; margin-top: 10px;">Địa chỉ HUFLIT: 806 Lê Quang Đạo, Trung Mỹ Tây, Quận 12, TP.HCM</p></div>';
        return;
    }
    
    // Địa chỉ Resort Deluxe
    var resortAddress = 'Huflit Hốc Môn, Hồ Chí Minh';
    // Tọa độ Resort (mặc định - Nha Trang)
    var resortLatlng = new google.maps.LatLng(12.2388, 109.1967);
    
    var userLocation = null;
    var directionsService = new google.maps.DirectionsService();
    var directionsRenderer = new google.maps.DirectionsRenderer({
        map: map,
        suppressMarkers: false,
        polylineOptions: {
            strokeColor: '#c8a97e',
            strokeWeight: 5,
            strokeOpacity: 0.8
        }
    });
    
    // Marker cho HUFLIT
    var huflitMarker = new google.maps.Marker({
        position: huflitLatlng,
        map: map,
        title: 'HUFLIT - Cơ sở Hóc Môn',
        animation: google.maps.Animation.DROP,
        icon: {
            url: 'https://maps.google.com/mapfiles/ms/icons/blue-dot.png'
        }
    });
    
    var huflitInfoWindow = new google.maps.InfoWindow({
        content: '<div style="padding: 10px; max-width: 280px;">' +
                 '<h4 style="margin: 0 0 8px 0; color: #c8a97e; font-weight: 700;">🎓 HUFLIT - Cơ sở Hóc Môn</h4>' +
                 '<p style="margin: 0; line-height: 1.6; color: #555; font-size: 14px;">' + huflitAddress + '</p>' +
                 '</div>'
    });
    
    huflitMarker.addListener('click', function() {
        huflitInfoWindow.open(map, huflitMarker);
    });
    huflitInfoWindow.open(map, huflitMarker);
    
    // Hàm tính toán và hiển thị tuyến đường
    function calculateAndDisplayRoute(origin, destination) {
        directionsService.route({
            origin: origin,
            destination: destination,
            travelMode: google.maps.TravelMode.DRIVING,
            optimizeWaypoints: true,
            avoidHighways: false,
            avoidTolls: false
        }, function(response, status) {
            if (status === 'OK') {
                directionsRenderer.setDirections(response);
                
                // Hiển thị thông tin tuyến đường
                var route = response.routes[0];
                var leg = route.legs[0];
                
                var routeInfo = '<div style="padding: 10px; max-width: 300px;">' +
                    '<h4 style="margin: 0 0 8px 0; color: #c8a97e; font-weight: 700;">🗺️ Tuyến đường</h4>' +
                    '<p style="margin: 4px 0; font-size: 13px; color: #555;"><strong>Khoảng cách:</strong> ' + leg.distance.text + '</p>' +
                    '<p style="margin: 4px 0; font-size: 13px; color: #555;"><strong>Thời gian:</strong> ' + leg.duration.text + '</p>' +
                    '</div>';
                
                // Tạo info window cho route
                var routeInfoWindow = new google.maps.InfoWindow({
                    content: routeInfo,
                    position: leg.end_location
                });
                
                setTimeout(function() {
                    routeInfoWindow.open(map);
                }, 1000);
            } else {
                console.error('Directions request failed: ' + status);
                alert('Không thể tính toán tuyến đường. Vui lòng thử lại.');
            }
        });
    }
    
    // Lấy vị trí hiện tại của người dùng
    if (navigator.geolocation) {
        var locationButton = document.createElement('button');
        locationButton.textContent = '📍 Lấy vị trí của tôi';
        locationButton.style.cssText = 'position: absolute; top: 10px; right: 10px; z-index: 1000; padding: 10px 15px; background: #c8a97e; color: white; border: none; border-radius: 5px; cursor: pointer; font-weight: 600; box-shadow: 0 2px 5px rgba(0,0,0,0.2);';
        locationButton.onclick = function() {
            this.disabled = true;
            this.textContent = '⏳ Đang lấy vị trí...';
            
            navigator.geolocation.getCurrentPosition(
                function(position) {
                    userLocation = new google.maps.LatLng(
                        position.coords.latitude,
                        position.coords.longitude
                    );
                    
                    // Tạo marker cho vị trí người dùng
                    var userMarker = new google.maps.Marker({
                        position: userLocation,
                        map: map,
                        title: 'Vị trí của bạn',
                        animation: google.maps.Animation.DROP,
                        icon: {
                            url: 'https://maps.google.com/mapfiles/ms/icons/green-dot.png'
                        }
                    });
                    
                    var userInfoWindow = new google.maps.InfoWindow({
                        content: '<div style="padding: 10px;"><h4 style="margin: 0 0 8px 0; color: #c8a97e;">📍 Vị trí của bạn</h4><p style="margin: 0; font-size: 13px;">Đã lấy vị trí thành công</p></div>'
                    });
                    
                    userMarker.addListener('click', function() {
                        userInfoWindow.open(map, userMarker);
                    });
                    userInfoWindow.open(map, userMarker);
                    
                    // Tính toán và hiển thị tuyến đường
                    calculateAndDisplayRoute(userLocation, huflitLatlng);
                    
                    // Fit map để hiển thị cả 2 điểm
                    var bounds = new google.maps.LatLngBounds();
                    bounds.extend(userLocation);
                    bounds.extend(huflitLatlng);
                    map.fitBounds(bounds);
                    
                    locationButton.textContent = '✅ Đã lấy vị trí';
                    setTimeout(function() {
                        locationButton.disabled = false;
                        locationButton.textContent = '📍 Lấy lại vị trí';
                    }, 2000);
                },
                function(error) {
                    console.error('Geolocation error:', error);
                    alert('Không thể lấy vị trí của bạn. Vui lòng cho phép truy cập vị trí hoặc kiểm tra cài đặt trình duyệt.');
                    locationButton.disabled = false;
                    locationButton.textContent = '📍 Lấy vị trí của tôi';
                },
                {
                    enableHighAccuracy: true,
                    timeout: 10000,
                    maximumAge: 0
                }
            );
        };
        
        mapElement.parentElement.style.position = 'relative';
        mapElement.parentElement.appendChild(locationButton);
    } else {
        console.warn('Geolocation is not supported by this browser.');
    }
}

// Expose initMap to global scope for Google Maps API callback
window.initMap = initMap;