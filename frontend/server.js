const express = require('express');
const { createProxyMiddleware } = require('http-proxy-middleware');
const path = require('path');

const app = express();
const PORT = 3000;
const API_URL = process.env.API_URL || 'http://localhost:5170';

// Proxy /api requests to the C# backend
app.use('/api', createProxyMiddleware({
    target: API_URL,
    changeOrigin: true,
    pathRewrite: { '^/': '/api/' },
}));

// Serve static files
app.use(express.static(path.join(__dirname, 'public')));

app.listen(PORT, () => {
    console.log(`CarbonPulse Frontend running at http://localhost:${PORT}`);
    console.log(`Proxying API requests to ${API_URL}`);
});
