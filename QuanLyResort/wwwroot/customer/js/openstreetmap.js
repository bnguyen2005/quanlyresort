function initMap() {
    // Get the HTML DOM element that will contain your map 
    var mapElement = document.getElementById('map') || document.querySelector('.map');
    if (!mapElement) {
        console.warn('Không tìm thấy element #map');
        return;
    }
    
    // Kiểm tra Leaflet đã load chưa
    if (typeof L === 'undefined') {
        console.error('Leaflet chưa được load');
        setTimeout(initMap, 100);
        return;
    }
    
    // Địa chỉ HUFLIT - Cơ sở Hóc Môn
    var huflitAddress = '806 Lê Quang Đạo, Trung Mỹ Tây, Quận 12, Thành phố Hồ Chí Minh, Việt Nam';
    // Tọa độ HUFLIT (10.8765, 106.6297)
    var huflitLatlng = [10.8765, 106.6297];
    
    // Initialize map centered at HUFLIT
    var map = L.map(mapElement).setView(huflitLatlng, 13);
    
    // Add OpenStreetMap tile layer
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
        maxZoom: 19
    }).addTo(map);
    
    // Custom icon cho HUFLIT
    var huflitIcon = L.divIcon({
        className: 'custom-marker',
        html: '<div style="background-color: #2196F3; width: 30px; height: 30px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 5px rgba(0,0,0,0.3); display: flex; align-items: center; justify-content: center; color: white; font-weight: bold; font-size: 18px;">🎓</div>',
        iconSize: [30, 30],
        iconAnchor: [15, 15],
        popupAnchor: [0, -15]
    });
    
    // Marker cho HUFLIT
    var huflitMarker = L.marker(huflitLatlng, { icon: huflitIcon }).addTo(map);
    
    var huflitPopup = L.popup({
        maxWidth: 300,
        className: 'custom-popup'
    }).setContent(
        '<div style="padding: 10px;">' +
        '<h4 style="margin: 0 0 8px 0; color: #c8a97e; font-weight: 700; font-size: 16px;">🎓 HUFLIT - Cơ sở Hóc Môn</h4>' +
        '<p style="margin: 0; line-height: 1.6; color: #555; font-size: 14px;">' + huflitAddress + '</p>' +
        '</div>'
    );
    
    huflitMarker.bindPopup(huflitPopup);
    huflitMarker.openPopup();
    
    // Routing control (sẽ được thêm khi có vị trí người dùng)
    var routingControl = null;
    var userMarker = null;
    var userLocation = null;
    
    // Hàm tính toán và hiển thị tuyến đường
    function calculateAndDisplayRoute(origin, destination) {
        // Xóa route cũ nếu có
        if (routingControl) {
            map.removeControl(routingControl);
        }
        
        // Tạo routing control mới
        routingControl = L.Routing.control({
            waypoints: [
                L.latLng(origin[0], origin[1]),
                L.latLng(destination[0], destination[1])
            ],
            routeWhileDragging: false,
            router: L.Routing.osrmv1({
                serviceUrl: 'https://router.project-osrm.org/route/v1'
            }),
            lineOptions: {
                styles: [
                    {
                        color: '#c8a97e',
                        opacity: 0.8,
                        weight: 5
                    }
                ]
            },
            createMarker: function(i, waypoint, n) {
                // Không tạo marker tự động, chúng ta đã có marker riêng
                return null;
            },
            showAlternatives: false
        }).addTo(map);
        
        routingControl.on('routesfound', function(e) {
            var routes = e.routes;
            if (routes && routes.length > 0) {
                var route = routes[0];
                var distance = (route.summary.totalDistance / 1000).toFixed(2); // km
                var duration = Math.round(route.summary.totalTime / 60); // minutes
                
                // Hiển thị thông tin tuyến đường
                var routeInfo = '<div style="padding: 10px; max-width: 300px;">' +
                    '<h4 style="margin: 0 0 8px 0; color: #c8a97e; font-weight: 700; font-size: 16px;">🗺️ Tuyến đường</h4>' +
                    '<p style="margin: 4px 0; font-size: 13px; color: #555;"><strong>Khoảng cách:</strong> ' + distance + ' km</p>' +
                    '<p style="margin: 4px 0; font-size: 13px; color: #555;"><strong>Thời gian:</strong> ~' + duration + ' phút</p>' +
                    '</div>';
                
                // Tạo popup cho route info
                if (userMarker) {
                    userMarker.bindPopup(routeInfo).openPopup();
                }
            }
        });
        
        routingControl.on('routingerror', function(e) {
            console.error('Routing error:', e);
            alert('Không thể tính toán tuyến đường. Vui lòng thử lại.');
        });
    }
    
    // Lấy vị trí hiện tại của người dùng
    if (navigator.geolocation) {
        var locationButton = document.createElement('button');
        locationButton.textContent = '📍 Lấy vị trí của tôi';
        locationButton.style.cssText = 'position: absolute; top: 10px; right: 10px; z-index: 1000; padding: 10px 15px; background: #c8a97e; color: white; border: none; border-radius: 5px; cursor: pointer; font-weight: 600; box-shadow: 0 2px 5px rgba(0,0,0,0.2); font-size: 14px;';
        locationButton.onclick = function() {
            this.disabled = true;
            this.textContent = '⏳ Đang lấy vị trí...';
            
            navigator.geolocation.getCurrentPosition(
                function(position) {
                    userLocation = [position.coords.latitude, position.coords.longitude];
                    
                    // Custom icon cho vị trí người dùng
                    var userIcon = L.divIcon({
                        className: 'custom-marker',
                        html: '<div style="background-color: #4CAF50; width: 30px; height: 30px; border-radius: 50%; border: 3px solid white; box-shadow: 0 2px 5px rgba(0,0,0,0.3); display: flex; align-items: center; justify-content: center; color: white; font-weight: bold; font-size: 18px;">📍</div>',
                        iconSize: [30, 30],
                        iconAnchor: [15, 15],
                        popupAnchor: [0, -15]
                    });
                    
                    // Tạo marker cho vị trí người dùng
                    if (userMarker) {
                        map.removeLayer(userMarker);
                    }
                    userMarker = L.marker(userLocation, { icon: userIcon }).addTo(map);
                    
                    var userPopup = L.popup({
                        maxWidth: 250,
                        className: 'custom-popup'
                    }).setContent(
                        '<div style="padding: 10px;"><h4 style="margin: 0 0 8px 0; color: #c8a97e;">📍 Vị trí của bạn</h4><p style="margin: 0; font-size: 13px;">Đã lấy vị trí thành công</p></div>'
                    );
                    
                    userMarker.bindPopup(userPopup);
                    userMarker.openPopup();
                    
                    // Tính toán và hiển thị tuyến đường
                    calculateAndDisplayRoute(userLocation, huflitLatlng);
                    
                    // Fit map để hiển thị cả 2 điểm
                    var bounds = L.latLngBounds([userLocation, huflitLatlng]);
                    map.fitBounds(bounds, { padding: [50, 50] });
                    
                    locationButton.textContent = '✅ Đã lấy vị trí';
                    setTimeout(function() {
                        locationButton.disabled = false;
                        locationButton.textContent = '📍 Lấy lại vị trí';
                    }, 2000);
                },
                function(error) {
                    console.error('Geolocation error:', error);
                    var errorMsg = 'Không thể lấy vị trí của bạn. ';
                    if (error.code === error.PERMISSION_DENIED) {
                        errorMsg += 'Vui lòng cho phép truy cập vị trí trong cài đặt trình duyệt.';
                    } else if (error.code === error.POSITION_UNAVAILABLE) {
                        errorMsg += 'Vị trí không khả dụng.';
                    } else {
                        errorMsg += 'Vui lòng thử lại sau.';
                    }
                    alert(errorMsg);
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

// Expose initMap to global scope
window.initMap = initMap;

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        // Wait a bit for Leaflet to be fully loaded
        setTimeout(initMap, 100);
    });
} else {
    setTimeout(initMap, 100);
}

