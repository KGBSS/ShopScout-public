// 1. Import Chart.js directly from CDN (ES Module version)
import Chart from 'https://cdn.jsdelivr.net/npm/chart.js/auto/+esm';

// Store instances to destroy them later (prevents memory leaks/glitches)
const charts = {};

// 2. Export the function so Blazor can call it
export function renderChart(canvasId, labels, data, label, color, type = 'line') {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    // Destroy existing chart if it exists on this canvas
    if (charts[canvasId]) {
        charts[canvasId].destroy();
    }

    // Create new chart
    charts[canvasId] = new Chart(ctx, {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: label,
                data: data,
                backgroundColor: type === 'bar' ? color : 'rgba(13, 110, 253, 0.1)',
                borderColor: color,
                borderWidth: 2,
                fill: type === 'line',
                tension: 0.3,
                pointRadius: 3
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: {
                y: { beginAtZero: true, grid: { display: false } },
                x: { grid: { display: false } }
            }
        }
    });
}