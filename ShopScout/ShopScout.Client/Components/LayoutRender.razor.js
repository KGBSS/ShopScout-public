let svg;
let svgG;
const gridSize = 20;
let layout = {};
let _polyPoints = [];
let init = false;
let viewBox = { x: 0, y: 0, width: 1000, height: 1000 }; // initial visible area
let isPanning = false;
let startPoint = { x: 0, y: 0 };
let panStart = { x: 0, y: 0 };
let zoomFactor = 1.1;
const padding = 0.1;
const sideIndicatorOffset = 15;
const viewMargin = 100;
const endMargin = 150;
let initialPinchDistance = null;
let initialViewBoxSize = { width: 0, height: 0 };
let _productEditorData = {
    shelfId: null,
    imageUrl: null
};
let editorSignalController = null;

export function fitToScreen(width = 1000, height = 1000) {
    const svgClientRect = svg.getBoundingClientRect();
    const bbox = svgG.getBBox();
    const svgWidth = svgClientRect.width;
    const svgHeight = svgClientRect.height;

    const svgRatio = svgWidth / svgHeight;
    const bboxRatio = bbox.width / bbox.height;

    let viewBoxWidth, viewBoxHeight;

    if (bboxRatio > svgRatio) {
        viewBoxWidth = bbox.width;
        viewBoxHeight = bbox.width / svgRatio;
    } else {
        viewBoxHeight = bbox.height;
        viewBoxWidth = bbox.height * svgRatio;
    }

    // Apply padding to the dimensions
    const finalWidth = viewBoxWidth * (1 + padding);
    const finalHeight = viewBoxHeight * (1 + padding);

    // FIX: Calculate the center of the bounding box and 
    // offset the viewBox origin so the content is centered.
    const centerX = bbox.x + bbox.width / 2;
    const centerY = bbox.y + bbox.height / 2;

    viewBox.x = centerX - finalWidth / 2;
    viewBox.y = centerY - finalHeight / 2;
    viewBox.width = finalWidth;
    viewBox.height = finalHeight;

    svg.setAttribute("viewBox", `${viewBox.x} ${viewBox.y} ${viewBox.width} ${viewBox.height}`);
}

function clampView(target, viewDim, contentStart, contentDim, marginStart, marginEnd) {
    const limitMin = contentStart - marginStart;
    const limitMax = contentStart + contentDim + marginEnd;

    // rangeMax is the point where the right side of the view hits the end of content.
    // If this is -60, it's because limitMax is 60px smaller than viewDim.
    let rangeMax = limitMax - viewDim;
    let rangeMin = limitMin;

    // THE FIX:
    // If we want to be able to hit 0, 0 must be part of our allowed boundaries.
    // We expand the boundaries to include 0 regardless of zoom or content size.
    const absoluteMin = Math.min(rangeMin, rangeMax, 0);
    const absoluteMax = Math.max(rangeMin, rangeMax, 0);

    return Math.max(absoluteMin, Math.min(target, absoluteMax));
}

function setWallPos(el, x1, x2, y1, y2) {
    el.setAttribute("x1", x1);
    el.setAttribute("y1", y1);
    el.setAttribute("x2", x2);
    el.setAttribute("y2", y2);
}

function initPanZoom(width = 1000, height = 1000) {
    init = true;

    fitToScreen(width, height);

    svg.addEventListener("wheel", e => {
        e.preventDefault();
        const { offsetX, offsetY, deltaY } = e;
        if (viewBox.width > 8000 && deltaY > 0) return;
        if (viewBox.width < 100 && deltaY < 0) return;

        const zoom = deltaY > 0 ? zoomFactor : 1 / zoomFactor;
        const pt = svg.createSVGPoint();
        pt.x = offsetX;
        pt.y = offsetY;
        const cursor = pt.matrixTransform(svg.getScreenCTM().inverse());

        const nextWidth = viewBox.width * zoom;
        const nextHeight = viewBox.height * zoom;

        let nextX = cursor.x - (cursor.x - viewBox.x) * zoom;
        let nextY = cursor.y - (cursor.y - viewBox.y) * zoom;

        const bbox = svgG.getBBox();
        nextX = clampView(nextX, nextWidth, bbox.x, bbox.width, viewMargin, endMargin);
        nextY = clampView(nextY, nextHeight, bbox.y, bbox.height, viewMargin, endMargin);

        viewBox.x = nextX;
        viewBox.y = nextY;
        viewBox.width = nextWidth;
        viewBox.height = nextHeight;

        svg.setAttribute("viewBox", `${viewBox.x} ${viewBox.y} ${viewBox.width} ${viewBox.height}`);
    }, { signal: editorSignalController.signal });

    svg.addEventListener("mousedown", e => {
        if (e.button === 2) {
            e.preventDefault();
            isPanning = true;
            startPoint = { x: e.clientX, y: e.clientY };
            panStart = { x: viewBox.x, y: viewBox.y };
        }
    }, { signal: editorSignalController.signal });

    svg.addEventListener("touchstart", e => {
        if (e.touches.length === 1) {
            isPanning = true;
            const t = e.touches[0];
            startPoint = { x: t.clientX, y: t.clientY };
            panStart = { x: viewBox.x, y: viewBox.y };
        }
        else if (e.touches.length === 2) {
            isPanning = false;
            const t1 = e.touches[0];
            const t2 = e.touches[1];

            initialPinchDistance = Math.hypot(t1.clientX - t2.clientX, t1.clientY - t2.clientY);
            initialViewBoxSize = { width: viewBox.width, height: viewBox.height };
        }
    }, { passive: false, signal: editorSignalController.signal });

    svg.addEventListener("touchmove", e => {
        e.preventDefault();
        if (e.touches.length === 1 && isPanning) {
            const t = e.touches[0];
            let dx = (t.clientX - startPoint.x) * (viewBox.width / svg.clientWidth);
            let dy = (t.clientY - startPoint.y) * (viewBox.height / svg.clientHeight);

            let targetX = panStart.x - dx;
            let targetY = panStart.y - dy;

            const bbox = svgG.getBBox();
            viewBox.x = clampView(targetX, viewBox.width, bbox.x, bbox.width, viewMargin, viewMargin);
            viewBox.y = clampView(targetY, viewBox.height, bbox.y, bbox.height, viewMargin, endMargin);
        }
        else if (e.touches.length === 2 && initialPinchDistance) {
            const t1 = e.touches[0];
            const t2 = e.touches[1];
            const currentDist = Math.hypot(t1.clientX - t2.clientX, t1.clientY - t2.clientY);

            const zoomRatio = initialPinchDistance / currentDist;

            const nextWidth = initialViewBoxSize.width * zoomRatio;
            const nextHeight = initialViewBoxSize.height * zoomRatio;

            if (nextWidth > 8000 || nextWidth < 100) return;

            const midX = (t1.clientX + t2.clientX) / 2;
            const midY = (t1.clientY + t2.clientY) / 2;

            const pt = svg.createSVGPoint();
            pt.x = midX - svg.getBoundingClientRect().left;
            pt.y = midY - svg.getBoundingClientRect().top;
            const cursor = pt.matrixTransform(svg.getScreenCTM().inverse());

            viewBox.x = cursor.x - (cursor.x - viewBox.x) * (nextWidth / viewBox.width);
            viewBox.y = cursor.y - (cursor.y - viewBox.y) * (nextHeight / viewBox.height);
            viewBox.width = nextWidth;
            viewBox.height = nextHeight;
        }
        svg.setAttribute("viewBox", `${viewBox.x} ${viewBox.y} ${viewBox.width} ${viewBox.height}`);
    }, { passive: false, signal: editorSignalController.signal });

    svg.addEventListener("touchend", () => { isPanning = false; initialPinchDistance = null; }, { signal: editorSignalController.signal });

    svg.addEventListener("contextmenu", e => e.preventDefault(), { signal: editorSignalController.signal });

    svg.addEventListener("mousemove", e => {
        if (!isPanning) return;

        // Compute movement in screen coordinates to compare against startPoint
        let dx = (e.clientX - startPoint.x) * (viewBox.width / svg.clientWidth);
        let dy = (e.clientY - startPoint.y) * (viewBox.height / svg.clientHeight);

        let targetX = panStart.x - dx;
        let targetY = panStart.y - dy;

        const bbox = svgG.getBBox();

        // Clamp the values
        let clampedX = clampView(targetX, viewBox.width, bbox.x, bbox.width, viewMargin, viewMargin);
        let clampedY = clampView(targetY, viewBox.height, bbox.y, bbox.height, viewMargin, endMargin);

        // If clamping occurred on X, reset the anchor to "discard" the delta overshoot
        if (clampedX !== targetX) {
            startPoint.x = e.clientX;
            panStart.x = clampedX;
            targetX = clampedX;
        }

        // If clamping occurred on Y, reset the anchor to "discard" the delta overshoot
        if (clampedY !== targetY) {
            startPoint.y = e.clientY;
            panStart.y = clampedY;
            targetY = clampedY;
        }

        viewBox.x = targetX;
        viewBox.y = targetY;
        svg.setAttribute("viewBox", `${viewBox.x} ${viewBox.y} ${viewBox.width} ${viewBox.height}`);
    }, { signal: editorSignalController.signal });

    svg.addEventListener("mouseup", () => { isPanning = false; }, { signal: editorSignalController.signal });
    svg.addEventListener("mouseleave", () => { isPanning = false; }, { signal: editorSignalController.signal });
}

function createWall(wall) {
    const line = document.createElementNS("http://www.w3.org/2000/svg", "line");
    setWallPos(line, wall.X1, wall.X2, wall.Y1, wall.Y2);
    line.setAttribute("stroke", "black");
    line.setAttribute("stroke-width", 2);
    line.setAttribute("class", "wall");
    line.setAttribute("data-id", wall.Id);
    svgG.appendChild(line);

    const hitLine = document.createElementNS("http://www.w3.org/2000/svg", "line");
    setWallPos(hitLine, wall.X1, wall.X2, wall.Y1, wall.Y2);
    hitLine.setAttribute("stroke", "transparent");
    hitLine.setAttribute("stroke-width", 20);
    hitLine.setAttribute("class", "hitWall");
    hitLine.setAttribute("data-id", wall.Id);
    svgG.appendChild(hitLine);
}

export function drawWalls(jsonData, productEditorData) {
    try {
        if (editorSignalController) {
            init = false;
            editorSignalController.abort();
        }

        editorSignalController = new AbortController();

        svg = document.getElementById("shopSVG");
        svgG = document.getElementById("layout-container");
        layout = JSON.parse(jsonData);

        _productEditorData = productEditorData;
    } catch (e) {
        console.error(e);
        return;
    }

    try {
        svgG.querySelectorAll('line, circle, polygon, path, image, g#product-preview, g > defs').forEach(e => e.remove());
    } catch (e) {
        console.error(e);
    }

    let poly = buildPolygon(layout.Wall);
    if (poly) {
        drawPoly(poly, true);
        layout.Shelf.forEach(s => {
            createShelf(s);
        });
        const entrance = layout.Entrance;
        if (entrance) {
            createEntrance(entrance);
        }
    }

    if (!init) {
        if (svgG.getBoundingClientRect().width > 0 && svgG.getBoundingClientRect().width > 0) {
            initPanZoom(svgG.getBoundingClientRect().width, svgG.getBoundingClientRect().height);
        } else {
            initPanZoom(svg.getBoundingClientRect().width, svg.getBoundingClientRect().height);
        }
    }

    renderProduct(layout.Shelf.find(x => x.Id == _productEditorData.shelfId) , _productEditorData.distFromP1);
}

function renderProduct(shelf, distFromP1) {
    if (!shelf || !_productEditorData.imageUrl) return;

    // 1. Setup Constants (Matching productHandler)
    const shelfOffset = 15;
    const calloutOffset = 60;
    const radius = 40;

    // 2. Extract and Normalize Coordinates
    let x1 = parseFloat(shelf.X1);
    let y1 = parseFloat(shelf.Y1);
    let x2 = parseFloat(shelf.X2);
    let y2 = parseFloat(shelf.Y2);

    // Normalize: Top-to-Bottom, then Left-to-Right
    if (y1 > y2 || (y1 === y2 && x1 > x2)) {
        [x1, y1, x2, y2] = [x2, y2, x1, y1];
    }

    const dx = x2 - x1;
    const dy = y2 - y1;
    const len = Math.sqrt(dx * dx + dy * dy);
    if (len === 0) return;

    // 3. Deconstruct the stored distFromP1
    // distFromP1 = sideDir * t * len
    const sideDir = distFromP1 >= 0 ? 1 : -1;
    const t = Math.abs(distFromP1) / len;

    // 4. Calculate Points
    const onLineX = x1 + t * dx;
    const onLineY = y1 + t * dy;

    const nx = -dy / len;
    const ny = dx / len;

    const anchorX = onLineX + (nx * sideDir * shelfOffset);
    const anchorY = onLineY + (ny * sideDir * shelfOffset);
    const largeX = onLineX + (nx * sideDir * (shelfOffset + calloutOffset));
    const largeY = onLineY + (ny * sideDir * (shelfOffset + calloutOffset));

    // 5. Create SVG Elements (Re-using the IDs or making them unique)
    const groupID = `prod-preview-${shelf.Id}`;

    // Check if already exists to avoid duplicates
    let productGroup = document.getElementById(groupID);
    if (productGroup) productGroup.remove();

    productGroup = document.createElementNS("http://www.w3.org/2000/svg", "g");
    productGroup.setAttribute("id", groupID);

    // Create ClipPath in Defs
    const defs = svgG.querySelector("defs") || svgG.appendChild(document.createElementNS("http://www.w3.org/2000/svg", "defs"));
    const clipId = `clip-${shelf.Id}`;
    let clip = document.getElementById(clipId);
    if (!clip) {
        clip = document.createElementNS("http://www.w3.org/2000/svg", "clipPath");
        clip.setAttribute("id", clipId);
        const circle = document.createElementNS("http://www.w3.org/2000/svg", "circle");
        circle.setAttribute("r", radius);
        clip.appendChild(circle);
        defs.appendChild(clip);
    }

    // Anchor
    const anchor = document.createElementNS("http://www.w3.org/2000/svg", "circle");
    anchor.setAttribute("r", 4);
    anchor.setAttribute("fill", "orange");
    anchor.setAttribute("cx", anchorX);
    anchor.setAttribute("cy", anchorY);

    // Handle
    const handle = document.createElementNS("http://www.w3.org/2000/svg", "line");
    handle.setAttribute("x1", anchorX); handle.setAttribute("y1", anchorY);
    handle.setAttribute("x2", largeX); handle.setAttribute("y2", largeY);
    handle.setAttribute("stroke", "orange");
    handle.setAttribute("stroke-dasharray", "4");

    // Image
    const img = document.createElementNS("http://www.w3.org/2000/svg", "image");
    img.setAttribute("href", _productEditorData.imageUrl);
    img.setAttribute("width", radius * 2);
    img.setAttribute("height", radius * 2);
    img.setAttribute("x", -radius);
    img.setAttribute("y", -radius);
    img.setAttribute("preserveAspectRatio", "xMidYMid slice");
    img.setAttribute("clip-path", `url(#${clipId})`);

    const border = document.createElementNS("http://www.w3.org/2000/svg", "circle");
    border.setAttribute("r", radius);
    border.setAttribute("fill", "none");
    border.setAttribute("stroke", "black");
    border.setAttribute("stroke-width", 2);

    // Assemble
    const previewG = document.createElementNS("http://www.w3.org/2000/svg", "g");
    previewG.setAttribute("transform", `translate(${largeX}, ${largeY})`);
    previewG.appendChild(img);
    previewG.appendChild(border);

    productGroup.appendChild(anchor);
    productGroup.appendChild(handle);
    productGroup.appendChild(previewG);
    svgG.appendChild(productGroup);
}

function setShelfTypeSide(line, type, side) {
    const COLORS = ["#4A90E2", "#50E3C2", "#7ED321", "#F5A623", "#D0021B"];
    const ARROW_PATH = "m -9.039,6.679 c -0.78,0.79 -2.07,0.79 -2.86,0.007 -0.79,-0.78 -0.79,-2.07 -0.007,-2.86 L -1.43,-6.68 c 0.78,-0.79 2.07,-0.79 2.86,-0.007 L 11.91,3.82 c 0.78,0.79 0.78,2.07 -0.007,2.86 -0.79,0.78 -2.07,0.78 -2.86,-0.007 l -9.03,-9.05 z";

    const lineId = line.getAttribute("data-id");
    const color = COLORS[type];

    // Update line color
    line.setAttribute("stroke", color);

    // 1. Cleanup existing indicators
    svgG.querySelectorAll(`.shelf-side-indicator[data-id="${lineId}"]`)
        .forEach(el => el.remove());

    // 2. Extract and Normalize Coordinates (Always Top -> Bottom / Left -> Right)
    let x1 = parseFloat(line.x1.baseVal.value);
    let y1 = parseFloat(line.y1.baseVal.value);
    let x2 = parseFloat(line.x2.baseVal.value);
    let y2 = parseFloat(line.y2.baseVal.value);

    if (y1 > y2 || (y1 === y2 && x1 > x2)) {
        [x1, y1, x2, y2] = [x2, y2, x1, y1];
    }

    // 3. Geometric Constants
    const angle = Math.atan2(y2 - y1, x2 - x1);
    const mid = { x: (x1 + x2) / 2, y: (y1 + y2) / 2 };

    // 4. Indicator Factory
    const addIndicator = (isRight) => {
        // Left side (side 0) adds PI/2, Right side (side 1) subtracts PI/2
        const normalAngle = angle + (isRight ? -Math.PI / 2 : Math.PI / 2);

        const posX = mid.x + sideIndicatorOffset * Math.cos(normalAngle);
        const posY = mid.y + sideIndicatorOffset * Math.sin(normalAngle);
        const rotation = (normalAngle * 180 / Math.PI) + 90;

        const path = document.createElementNS("http://www.w3.org/2000/svg", "path");
        path.setAttribute("class", "shelf-side-indicator");
        path.setAttribute("d", ARROW_PATH);
        path.setAttribute("fill", color);
        path.setAttribute("data-id", lineId);
        path.setAttribute("transform", `translate(${posX}, ${posY}) rotate(${rotation})`);

        svgG.appendChild(path);
    };

    // 5. Execution Logic
    // side: 0=Left, 1=Right, 2=Both
    if (side === 0 || side === 2) addIndicator(false);
    if (side === 1 || side === 2) addIndicator(true);
}

function createShelf(shelf) {
    const line = document.createElementNS("http://www.w3.org/2000/svg", "line");
    setWallPos(line, shelf.X1, shelf.X2, shelf.Y1, shelf.Y2);
    line.setAttribute("stroke", "#fcba03");
    line.setAttribute("stroke-width", 5);
    line.setAttribute("class", "shelf");
    line.setAttribute("data-id", shelf.Id);
    line.setAttribute("stroke-linecap", "round");
    svgG.appendChild(line);
    setShelfTypeSide(line, shelf.Type, shelf.Side);
}

function createEntrance(wall) {
    const entrance = document.createElementNS("http://www.w3.org/2000/svg", "line");
    entrance.setAttribute("id", "entrance");
    entrance.setAttribute("stroke", "orange");
    entrance.setAttribute("stroke-width", 6);
    setWallPos(entrance, wall.X1, wall.X2, wall.Y1, wall.Y2);
    svgG.appendChild(entrance);
}

function isClosedPolygon(walls) {
    if (walls == null || walls.length === 0) return false;
    // Count how many walls touch each point
    const pointCount = {};
    for (const w of walls) {
        const p1 = `${w.X1},${w.Y1}`;
        const p2 = `${w.X2},${w.Y2}`;
        pointCount[p1] = (pointCount[p1] || 0) + 1;
        pointCount[p2] = (pointCount[p2] || 0) + 1;
    }

    // All points must connect to 2 
    return Object.values(pointCount).every(c => c === 2);
}

function buildPolygon(walls) {
    if (!isClosedPolygon(walls)) return null;

    // Build adjacency: point -> connected points
    const adj = {};
    for (const w of walls) {
        const p1 = `${w.X1},${w.Y1}`;
        const p2 = `${w.X2},${w.Y2}`;
        adj[p1] = adj[p1] || [];
        adj[p2] = adj[p2] || [];
        adj[p1].push(p2);
        adj[p2].push(p1);
    }

    // Start at any point
    const start = Object.keys(adj)[0];
    let current = start;
    let prev = null;
    const polygon = [];

    do {
        const [cx, cy] = current.split(",").map(Number);
        polygon.push({ x: cx, y: cy });

        // Walk to the next point
        const neighbors = adj[current];
        const next = neighbors.find(n => n !== prev);
        prev = current;
        current = next;
    } while (current && current !== start);

    return polygon;
}

function drawPoly(polyPoints, shelfEditor = false) {
    _polyPoints = polyPoints;
    svgG.querySelectorAll('line, circle').forEach(e => e.remove());
    const poly = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
    const pointsStr = polyPoints.map(p => `${p.x},${p.y}`).join(" ");
    poly.setAttribute("points", pointsStr);
    poly.setAttribute("stroke", "black");
    poly.setAttribute("stroke-width", 2);
    poly.setAttribute("fill", `url(#${shelfEditor ? "grid" : "linePattern"})`);
    svgG.appendChild(poly);
}
