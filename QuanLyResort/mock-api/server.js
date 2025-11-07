const express = require('express');
const cors = require('cors');
const app = express();
app.use(cors());

const rooms = [{"roomId":1,"roomNumber":"101","roomType":"Standard","roomTypeId":null,"roomTypeName":null,"floor":"1","pricePerNight":500000.00,"maxOccupancy":2,"description":"Standard room with garden view","amenities":"WiFi, TV, Air Conditioning, Mini Bar","isAvailable":true,"housekeepingStatus":"Ready","notes":null,"createdAt":"2025-10-20T18:23:57.4754956"},{"roomId":2,"roomNumber":"102","roomType":"Standard","roomTypeId":null,"roomTypeName":null,"floor":"1","pricePerNight":500000.00,"maxOccupancy":2,"description":"Standard room with garden view","amenities":"WiFi, TV, Air Conditioning, Mini Bar","isAvailable":true,"housekeepingStatus":"Ready","notes":null,"createdAt":"2025-10-20T18:23:57.4763115"},{"roomId":3,"roomNumber":"201","roomType":"Deluxe","roomTypeId":null,"roomTypeName":null,"floor":"2","pricePerNight":800000.00,"maxOccupancy":3,"description":"Deluxe room with sea view","amenities":"WiFi, Smart TV, Air Conditioning, Mini Bar, Balcony, Bathtub","isAvailable":true,"housekeepingStatus":"Ready","notes":null,"createdAt":"2025-10-20T18:23:57.476313"},{"roomId":4,"roomNumber":"301","roomType":"Suite","roomTypeId":null,"roomTypeName":null,"floor":"3","pricePerNight":1500000.00,"maxOccupancy":4,"description":"Suite with panoramic sea view","amenities":"WiFi, Smart TV, Air Conditioning, Mini Bar, Living Room, Jacuzzi, Balcony","isAvailable":false,"housekeepingStatus":"Clean","notes":null,"createdAt":"2025-10-20T18:23:57.4763254"},{"roomId":5,"roomNumber":"401","roomType":"Villa","roomTypeId":null,"roomTypeName":null,"floor":"Garden","pricePerNight":3000000.00,"maxOccupancy":6,"description":"Luxury villa with private pool","amenities":"WiFi, Smart TV, Air Conditioning, Full Kitchen, Private Pool, Garden, Multiple Bedrooms","isAvailable":true,"housekeepingStatus":"Ready","notes":null,"createdAt":"2025-10-20T18:23:57.4763259"}];

app.get('/api/rooms', (req, res) => {
  res.json(rooms);
});

const PORT = process.env.PORT || 5130;
app.listen(PORT, () => console.log(`Mock API running: http://localhost:${PORT}/api/rooms`));
