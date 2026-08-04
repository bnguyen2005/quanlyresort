#!/bin/bash
# Script để test webhook status endpoint

echo "🔍 Testing Webhook Status Endpoint..."
echo ""

curl -s http://localhost:5130/api/simplepayment/webhook-status | python3 -m json.tool || echo "❌ Endpoint not accessible"

echo ""
echo "✅ Test completed!"

