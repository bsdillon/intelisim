function readFarmJson(root, key, fallback) {
    const raw = root && root.dataset ? root.dataset[key] : "";
    if (!raw) return fallback;
    try {
        const parsed = JSON.parse(raw);
        return parsed == null ? fallback : parsed;
    } catch {
        return fallback;
    }
}

function renderFarmHistogram(root) {
    const samples = readFarmJson(root, "samples", []);
    root.replaceChildren();
    if (!Array.isArray(samples) || samples.length === 0) return;

    const width = 800;
    const height = 400;
    const margin = {top: 20, right: 30, bottom: 50, left: 55};
    const svg = d3.select(root)
        .append("svg")
        .attr("viewBox", `0 0 ${width} ${height}`)
        .attr("width", "100%")
        .attr("height", "auto")
        .style("font-size", "18px");

    const peak = d3.max(samples);
    const x = d3.scaleLinear()
        .domain([0, peak > 0 ? peak : 1])
        .nice()
        .range([margin.left, width - margin.right]);

    const bins = d3.histogram()
        .domain(x.domain())
        .thresholds(x.ticks(12))(samples);

    const y = d3.scaleLinear()
        .domain([0, d3.max(bins, d => d.length) || 1])
        .nice()
        .range([height - margin.bottom, margin.top]);

    svg.append("g")
        .attr("transform", `translate(0,${height - margin.bottom})`)
        .call(d3.axisBottom(x));

    svg.append("g")
        .attr("transform", `translate(${margin.left},0)`)
        .call(d3.axisLeft(y));

    svg.selectAll("rect")
        .data(bins)
        .join("rect")
        .attr("x", d => x(d.x0) + 1)
        .attr("y", d => y(d.length))
        .attr("width", d => Math.max(0, x(d.x1) - x(d.x0) - 2))
        .attr("height", d => y(0) - y(d.length))
        .attr("opacity", 0.75);
}

function renderFarmPopulation(root) {
    const series = readFarmJson(root, "series", []);
    root.replaceChildren();
    if (!Array.isArray(series) || series.length === 0) return;

    const width = 800;
    const height = 450;
    const margin = {top: 20, right: 30, bottom: 45, left: 55};
    const svg = d3.select(root)
        .append("svg")
        .attr("viewBox", `0 0 ${width} ${height}`)
        .attr("width", "100%")
        .attr("height", "auto")
        .style("font-size", "18px");

    const x = d3.scaleLinear()
        .domain(d3.extent(series, d => d.tick))
        .range([margin.left, width - margin.right]);

    const peak = d3.max(series, d => Math.max(d.sheep, d.wolves));
    const y = d3.scaleLinear()
        .domain([0, peak > 0 ? peak : 1])
        .nice()
        .range([height - margin.bottom, margin.top]);

    svg.append("g")
        .attr("transform", `translate(0,${height - margin.bottom})`)
        .call(d3.axisBottom(x))
        .selectAll("text")
        .style("font-size", "18px");

    svg.append("g")
        .attr("transform", `translate(${margin.left},0)`)
        .call(d3.axisLeft(y))
        .selectAll("text")
        .style("font-size", "18px");

    const line = d3.line()
        .x(d => x(d.tick))
        .y(d => y(d.value));

    for (const [name, values] of [
        ["sheep", series.map(d => ({tick: d.tick, value: d.sheep}))],
        ["wolves", series.map(d => ({tick: d.tick, value: d.wolves}))]
    ]) {
        svg.append("path")
            .datum(values)
            .attr("fill", "none")
            .attr("stroke", name === "sheep" ? "steelblue" : "darkred")
            .attr("stroke-width", 2)
            .attr("d", line);
    }
}

function paintFarmCharts(node) {
    if (!node || node.nodeType !== 1) return;

    if (node.id === "simulation-histogram") renderFarmHistogram(node);
    else {
        const histogram = node.querySelector("#simulation-histogram");
        if (histogram) renderFarmHistogram(histogram);
    }

    if (typeof renderFarmPopulation === "function") {
        if (node.id === "population-chart") renderFarmPopulation(node);
        else {
            const population = node.querySelector("#population-chart");
            if (population) renderFarmPopulation(population);
        }
    }
}

if (!window.__farmChartSwaps) {
    window.__farmChartSwaps = true;
    document.body.addEventListener("htmx:oobAfterSwap", (event) => {
        paintFarmCharts(event.target);
    });
    document.body.addEventListener("htmx:afterSwap", (event) => {
        paintFarmCharts(event.detail && event.detail.target);
    });
}
