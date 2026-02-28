let svg;
let svgG;
const gridSize = 20;
let layout = {};
let wallsDict = {};
let shelvesDict = {};
let selectedTool = null;
let activeDrawing = null;
let allEventObjects = [];
let tIdCount = 0;
let _polyPoints = [];
let _objRef;
let _editorMode;
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
let shelfType = null;
let listenersAdded = false;
let initialPinchDistance = null;
let initialViewBoxSize = { width: 0, height: 0 };
let _productEditorData = {
    shelfId: null,
    imageUrl: null
};
let editorSignalController = null;

function updateGridRect() {
    const viewBox = svg.viewBox.baseVal;
    const gridRect = document.querySelector('#grid-rect');
    if (!gridRect) return; // in readonly mode, no grid

    let targetX = Math.max(0, viewBox.x);
    let targetY = Math.max(0, viewBox.y);
    if (viewBox.x >= 0 && viewBox.y >= 0) gridRect.classList.remove("border");
    else gridRect.classList.add("border");
    gridRect.setAttribute('x', targetX);
    gridRect.setAttribute('y', targetY);
    gridRect.setAttribute('width', viewBox.width);
    gridRect.setAttribute('height', viewBox.height);
}

export function fitToScreen(width = 1000, height = 1000) {
    const svgClientRect = svg.getBoundingClientRect();
    const bbox = svgG.getBBox();
    const svgWidth = svgClientRect.width;
    const svgHeight = svgClientRect.height;

    // 1. Default values for empty layout
    let finalWidth = width;
    let finalHeight = height;
    let centerX = width / 2;
    let centerY = height / 2;

    // 2. If content exists, calculate the "Zoom to Fit" dimensions
    if (bbox.width > 0 && bbox.height > 0) {
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

        // Apply padding and update center based on actual content
        finalWidth = viewBoxWidth * (1 + padding);
        finalHeight = viewBoxHeight * (1 + padding);
        centerX = bbox.x + bbox.width / 2;
        centerY = bbox.y + bbox.height / 2;
    }

    // 3. Calculate the top-left corner (min-x, min-y) 
    // by subtracting half the width/height from the center.
    const viewBoxX = centerX - finalWidth / 2;
    const viewBoxY = centerY - finalHeight / 2;

    svg.setAttribute("viewBox", `${viewBoxX} ${viewBoxY} ${finalWidth} ${finalHeight}`);

    // Update internal state if 'viewBox' is a global object you're tracking
    if (typeof viewBox !== 'undefined') {
        viewBox.x = viewBoxX;
        viewBox.y = viewBoxY;
        viewBox.width = finalWidth;
        viewBox.height = finalHeight;
    }

    updateGridRect();
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
        updateGridRect();
    }, { signal: editorSignalController.signal });

    svg.addEventListener("mousedown", e => {
        if (e.button === 2 || selectedTool == "None") {
            e.preventDefault();
            isPanning = true;
            startPoint = { x: e.clientX, y: e.clientY };
            panStart = { x: viewBox.x, y: viewBox.y };
        }
    }, { signal: editorSignalController.signal });

    svg.addEventListener("touchstart", e => {
        if (e.touches.length === 1 && (selectedTool === "None")) {
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
        updateGridRect();
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
        updateGridRect();
    }, { signal: editorSignalController.signal });

    svg.addEventListener("mouseup", () => { isPanning = false; }, { signal: editorSignalController.signal });
    svg.addEventListener("mouseleave", () => { isPanning = false; }, { signal: editorSignalController.signal });
}

function snap(value, viewOffset = 0) {
    if (viewOffset < 0) viewOffset = 0;
    return Math.round((value - viewOffset) / gridSize) * gridSize + viewOffset;
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

export function drawWalls(jsonData, objRef = null, readOnly = false, editorMode = "walleditor", productEditorData) {
    try {
        if (editorSignalController) {
            listenersAdded = false;
            init = false;
            editorSignalController.abort();
        }

        editorSignalController = new AbortController();

        if (jsonData == -1 || editorMode == "readonly") {
            wallsDict = {};
            allEventObjects = [];
            _polyPoints = [];
        }

        if (jsonData != -1) {
            svg = document.getElementById("shopSVG");
            svgG = document.getElementById("layout-container");
            layout = JSON.parse(jsonData);
            _objRef = objRef;
            selectedTool ??= "None";
        }

        if (editorMode == "producteditor" && productEditorData) {
            _productEditorData = productEditorData;
            layout.Shelf = layout.Shelf.filter(s => s.Id.toString() === _productEditorData.shelfId);
        }

        _editorMode = editorMode;
    } catch (e) {
        console.error(e);
        return;
    }

    try {
        svgG.querySelectorAll('line, circle, polygon, path, image, g#product-preview, g > defs').forEach(e => e.remove());
    } catch (e) {
        console.error(e);
    }

    if (readOnly || editorMode == "shelfeditor") {
        let poly = buildPolygon(layout.Wall);
        if (poly) {
            const showGrid = editorMode == "shelfeditor" || editorMode == "producteditor";
            drawPoly(poly, showGrid);
            layout.Shelf.forEach(s => {
                createShelf(s);
                createHandle(s, 'X1', 'Y1', 'Shelf');
                createHandle(s, 'X2', 'Y2', 'Shelf');
            });
            const entrance = layout.Entrance;
            if (entrance) {
                createEntrance(entrance);
            }
        }
    }

    if (!readOnly && _editorMode == "walleditor") {
        layout.Wall.forEach(wall => {
            createWall(wall);
            createHandle(wall, 'X1', 'Y1', 'Wall');
            createHandle(wall, 'X2', 'Y2', 'Wall');
        });
    }

    if (editorMode == "producteditor" && !listenersAdded) {
        listenersAdded = true;

        // Helper to extract the first touch or the mouse event
        const getEvent = (e) => e.touches ? e.touches[0] : e;

        const onMove = (e) => {
            if (selectedTool === "Product") {
                // Prevent scrolling on touch devices while interacting with the SVG
                if (e.type === "touchmove") e.preventDefault();

                // Pass the primary touch or mouse event to your handler
                productHandler(getEvent(e));
            }
        };

        const onClick = (e) => {
            // Prevent accidental double-firing on some mobile browsers
            if (e.type === "touchend") e.preventDefault();

            const productElement = document.getElementById("product-preview");
            if (productElement) {
                _objRef.invokeMethodAsync('ProductPlaced', parseFloat(productElement.dataset.distFromP1));
            }
        };

        // --- Mouse Listeners ---
        svg.addEventListener("mousemove", onMove, { signal: editorSignalController.signal });
        svg.addEventListener("click", onClick, { signal: editorSignalController.signal });

        // --- Touch Listeners ---
        // passive: false is required to allow e.preventDefault() for smoother dragging
        svg.addEventListener("touchmove", onMove, {
            passive: false,
            signal: editorSignalController.signal
        });
        svg.addEventListener("touchend", onClick, {
            signal: editorSignalController.signal
        });
    }

    if (!init) {
        if (svgG.getBoundingClientRect().width > 0 && svgG.getBoundingClientRect().width > 0) {
            initPanZoom(svgG.getBoundingClientRect().width, svgG.getBoundingClientRect().height);
        } else {
            initPanZoom(svg.getBoundingClientRect().width, svg.getBoundingClientRect().height);
        }
        if (readOnly) return;
    }

    if (!listenersAdded) {
        listenersAdded = true;
        svg.addEventListener("mousedown", e => {
            if (e.button != 0) return;
            if (selectedTool == "Wall") {
                if (!activeDrawing) {
                    registerDrawStart(e);
                } else {
                    stopDrawing(e);
                }
            } else if (selectedTool == "Shelf") {
                if (!activeDrawing) {
                    registerDrawStart(e, "Shelf");
                } else {
                    stopDrawing(e);
                }
            } else if (selectedTool == "Eraser") {
                ereaserTool(e);
            }
        }, { signal: editorSignalController.signal });

        svg.addEventListener("mousemove", e => {
            if (selectedTool == "Entrance") {
                entranceHandler(e);
            }

            if (activeDrawing) {
                drawHandler(e, activeDrawing);
            } else {
                for (let ev of allEventObjects) {
                    if (ev.dragging) dragHandler(e, ev);
                }
            }
        }, { signal: editorSignalController.signal });

        document.addEventListener("mouseup", e => {
            stopDrawing(e);
        }, { signal: editorSignalController.signal });

        document.addEventListener("keydown", e => {
            if (e.key === "Escape") {
                if (!activeDrawing) return;
                cancelDrawing();
            }
        }, { signal: editorSignalController.signal });

        svg.addEventListener("touchstart", e => {
            if (selectedTool === "None" || e.touches.length > 1) return;
            if (selectedTool === "Wall" || selectedTool === "Shelf") {
                if (!activeDrawing) registerDrawStart(e, selectedTool);
                else stopDrawing(e);
            } else if (selectedTool === "Eraser") {
                ereaserTool(e);
            }
        }, { passive: false, signal: editorSignalController.signal });

        svg.addEventListener("touchmove", e => {
            if (activeDrawing) {
                e.preventDefault();
                drawHandler(e, activeDrawing);
            } else {
                for (let ev of allEventObjects) {
                    if (ev.dragging) {
                        e.preventDefault();
                        dragHandler(e, ev);
                    }
                }
            }
        }, { passive: false, signal: editorSignalController.signal });

        document.addEventListener("touchend", e => {
            stopDrawing(e);
        }, { signal: editorSignalController.signal });
    }
}

function entranceHandler(e) {
    const rect = svgG.getBoundingClientRect();
    const [x, y] = xy(e, false);


    let entrance = document.getElementById("entrance");
    if (!entrance) {
        entrance = document.createElementNS("http://www.w3.org/2000/svg", "line");
        entrance.setAttribute("id", "entrance");        
        entrance.setAttribute("stroke", "orange");
        entrance.setAttribute("stroke-width", 6);
        svgG.appendChild(entrance);

        entrance.addEventListener("click", e => {
            let data = {
                id: "entrance",
                x1: Math.round(e.target.attributes.x1.value),
                y1: Math.round(e.target.attributes.y1.value),
                x2: Math.round(e.target.attributes.x2.value),
                y2: Math.round(e.target.attributes.y2.value)
            }
            _objRef.invokeMethodAsync('EntrancePlaced', data, layout.Wall, layout.Shelf);
        }, { signal: editorSignalController.signal });
    }

    let bestDist = Infinity;
    let bestSnap = null;
    let bestP1 = null;
    let bestP2 = null;

    for (let i = 0; i < _polyPoints.length; i++) {
        let p1 = _polyPoints[i];
        let p2 = i == _polyPoints.length - 1 ? _polyPoints[0] : _polyPoints[i + 1];3

        const dx = p2.x - p1.x;
        const dy = p2.y - p1.y;

        const t = Math.max(0, Math.min(1, ((x - p1.x) * dx + (y - p1.y) * dy) / (dx * dx + dy * dy)));

        const snapX = p1.x + t * dx;
        const snapY = p1.y + t * dy;

        const dist = Math.hypot(snapX - x, snapY - y);
        if (dist < bestDist) {
            bestDist = dist;
            bestSnap = { x: snapX, y: snapY };
            bestP1 = p1;
            bestP2 = p2;
        }
    }

    if (bestSnap) {
        const dx = bestP2.x - bestP1.x;
        const dy = bestP2.y - bestP1.y;

        const len = Math.hypot(dx, dy);

        const ux = dx / len;
        const uy = dy / len;

        const halfLen = 40 / 2;

        let x1 = bestSnap.x - ux * halfLen;
        let y1 = bestSnap.y - uy * halfLen;
        let x2 = bestSnap.x + ux * halfLen;
        let y2 = bestSnap.y + uy * halfLen;

        const distToP1 = Math.hypot(bestSnap.x - bestP1.x, bestSnap.y - bestP1.y);
        const distToP2 = Math.hypot(bestSnap.x - bestP2.x, bestSnap.y - bestP2.y);
        if (distToP1 < halfLen) {
            const factor = distToP1 / halfLen;
            x1 = bestSnap.x - ux * halfLen * factor;
            y1 = bestSnap.y - uy * halfLen * factor;
        }
        if (distToP2 < halfLen) {
            const factor = distToP2 / halfLen;
            x2 = bestSnap.x + ux * halfLen * factor;
            y2 = bestSnap.y + uy * halfLen * factor;
        }

        entrance.setAttribute("x1", x1);
        entrance.setAttribute("y1", y1);
        entrance.setAttribute("x2", x2);
        entrance.setAttribute("y2", y2);
    }
}

function productHandler(e) {
    const [x, y] = xy(e, false);
    const shelfOffset = 15;   // Distance of the anchor dot from shelf
    const calloutOffset = 60; // How much further out the large image sits
    const radius = 40;        // Much larger image for precision viewing

    let productGroup = document.getElementById("product-preview");
    let handle = document.getElementById("product-handle");
    let anchor = document.getElementById("product-anchor");

    if (!productGroup) {
        // 1. The Anchor Dot (The actual placement point)
        anchor = document.createElementNS("http://www.w3.org/2000/svg", "circle");
        anchor.setAttribute("id", "product-anchor");
        anchor.setAttribute("r", 4);
        anchor.setAttribute("fill", "orange");
        svgG.appendChild(anchor);

        // 2. The Handle (Line connecting anchor to large image)
        handle = document.createElementNS("http://www.w3.org/2000/svg", "line");
        handle.setAttribute("id", "product-handle");
        handle.setAttribute("stroke", "orange");
        handle.setAttribute("stroke-dasharray", "4");
        svgG.appendChild(handle);

        // 3. The Large Product Group
        productGroup = document.createElementNS("http://www.w3.org/2000/svg", "g");
        productGroup.setAttribute("id", "product-preview");

        const defs = svgG.querySelector("defs") || svgG.appendChild(document.createElementNS("http://www.w3.org/2000/svg", "defs"));
        const clip = document.createElementNS("http://www.w3.org/2000/svg", "clipPath");
        clip.setAttribute("id", "prod-clip-large");
        const clipCircle = document.createElementNS("http://www.w3.org/2000/svg", "circle");
        clipCircle.setAttribute("r", radius);
        clip.appendChild(clipCircle);
        defs.appendChild(clip);

        const img = document.createElementNS("http://www.w3.org/2000/svg", "image");
        img.setAttribute("href", _productEditorData.imageUrl);
        img.setAttribute("preserveAspectRatio", "xMidYMid slice");
        img.setAttribute("width", radius * 2);
        img.setAttribute("height", radius * 2);
        img.setAttribute("x", -radius);
        img.setAttribute("y", -radius);
        img.setAttribute("clip-path", "url(#prod-clip-large)");

        const border = document.createElementNS("http://www.w3.org/2000/svg", "circle");
        border.setAttribute("r", radius);
        border.setAttribute("fill", "none");
        border.setAttribute("stroke", "black");
        border.setAttribute("stroke-width", 2);

        productGroup.appendChild(img);
        productGroup.appendChild(border);
        svgG.appendChild(productGroup);
    }

    // --- SNAP LOGIC ---
    let shelf = layout.Shelf[0];
    const p1 = { x: parseFloat(shelf.X1), y: parseFloat(shelf.Y1) };
    const p2 = { x: parseFloat(shelf.X2), y: parseFloat(shelf.Y2) };

    if (p1.y > p2.y || (p1.y === p2.y && p1.x > p2.x)) {
        [p1.x, p1.y, p2.x, p2.y] = [p2.x, p2.y, p1.x, p1.y];
    }

    const dx = p2.x - p1.x;
    const dy = p2.y - p1.y;
    const lenSq = dx * dx + dy * dy;
    const len = Math.sqrt(lenSq);

    if (len === 0) return;

    const t = Math.max(0, Math.min(1, ((x - p1.x) * dx + (y - p1.y) * dy) / lenSq));
    const onLineX = p1.x + t * dx;
    const onLineY = p1.y + t * dy;

    // Normal vector for side-switching
    const nx = -dy / len;
    const ny = dx / len;

    let sideDir;

    // Check shelf.Side constraint
    // 0 = Both, 1 = Side A, 2 = Side B
    if (shelf.Side === 0) {
        sideDir = 1;
    } else if (shelf.Side === 1) {
        sideDir = -1;
    } else {
        // Fallback to cursor position if shelf is double-sided (Side === 0)
        sideDir = Math.hypot(onLineX + nx * 10 - x, onLineY + ny * 10 - y) <
            Math.hypot(onLineX - nx * 10 - x, onLineY - ny * 10 - y) ? 1 : -1;
    }

    // Actual Snap Point (The Anchor)
    const anchorX = onLineX + (nx * sideDir * shelfOffset);
    const anchorY = onLineY + (ny * sideDir * shelfOffset);

    // Large Image Point (Offset further so it doesn't block the view)
    const largeX = onLineX + (nx * sideDir * (shelfOffset + calloutOffset));
    const largeY = onLineY + (ny * sideDir * (shelfOffset + calloutOffset));

    // Update Elements
    anchor.setAttribute("cx", anchorX);
    anchor.setAttribute("cy", anchorY);

    handle.setAttribute("x1", anchorX);
    handle.setAttribute("y1", anchorY);
    handle.setAttribute("x2", largeX);
    handle.setAttribute("y2", largeY);

    productGroup.setAttribute("transform", `translate(${largeX}, ${largeY})`);

    // Store metadata for the final placement
    productGroup.dataset.distFromP1 = sideDir * t * len;
}

function updateWalls(changedWallIds, type) {
    let connectedWalls = layout[type].filter(wall => changedWallIds.includes(wall.Id.toString()));
    connectedWalls.forEach(wall => {
        let querySelector;
        let hitSelector;
        if (type == "Wall") {
            querySelector = `line.wall[data-id="${wall.Id}"]`;
            hitSelector = `line.hitWall[data-id="${wall.Id}"]`;
        } else if (type == "Shelf") {
            querySelector = `line.shelf[data-id="${wall.Id}"]`;
            hitSelector = `line.hitShelf[data-id="${wall.Id}"]`;
        } else return;

        const line = document.querySelector(querySelector);
        const hitLine = document.querySelector(hitSelector);
        setWallPos(line, wall.X1, wall.X2, wall.Y1, wall.Y2);
        setWallPos(hitLine, wall.X1, wall.X2, wall.Y1, wall.Y2);
        if (type == "Shelf") {
            setShelfTypeSide(line, wall.Type, wall.Side);
        }
    });
}

function setWallPos(line, x1, x2, y1, y2) {
    line.setAttribute("x1", x1);
    line.setAttribute("y1", y1);
    line.setAttribute("x2", x2);
    line.setAttribute("y2", y2);
}

function createHandleElements(cx, cy, wallId) {
    const handle = document.createElementNS("http://www.w3.org/2000/svg", "circle");
    handle.setAttribute("cx", cx);
    handle.setAttribute("cy", cy);
    handle.setAttribute("r", 5);
    handle.setAttribute("class", "handle");

    const hitArea = document.createElementNS("http://www.w3.org/2000/svg", "circle");
    hitArea.setAttribute("cx", cx);
    hitArea.setAttribute("cy", cy);
    hitArea.setAttribute("r", 10);
    hitArea.setAttribute("class", "hit-area");
    hitArea.setAttribute("fill", "transparent");
    hitArea.setAttribute("pointer-events", "all");
    hitArea.setAttribute("style", "cursor:pointer;");
    hitArea.setAttribute("data-wall-id", wallId);

    return [handle, hitArea];
}

function addPropertiesEvent(hitLine) {
    hitLine.addEventListener("click", e => {
        if (selectedTool !== "None" && selectedTool !== "Shelf") return;
        let selectedShelf = e.target;
        let shelf = layout.Shelf.find(s => s.Id.toString() === selectedShelf.getAttribute("data-id"));
        let shelfElement = document.querySelector(`line.shelf[data-id="${shelf.Id}"]`);
        _objRef.invokeMethodAsync('ShelfClicked', shelf);
        const borderLine = document.createElementNS("http://www.w3.org/2000/svg", "rect");
        borderLine.setAttribute("stroke", "crimson");
        borderLine.setAttribute("stroke-width", 3);
        borderLine.setAttribute("fill", "none");
        const gap = 5;
        const w = Math.hypot(shelf.X2 - shelf.X1, shelf.Y2 - shelf.Y1) + gap * 4;
        const h = parseInt(shelfElement.getAttribute("stroke-width")) + gap * 3;
        const x = shelf.X1 - (h / 2);
        const y = shelf.Y1 - gap * 2;

        const angle = Math.atan2(shelf.Y2 - shelf.Y1, shelf.X2 - shelf.X1) * (180 / Math.PI);

        borderLine.setAttribute("x", x);
        borderLine.setAttribute("y", y);
        borderLine.setAttribute("width", w);
        borderLine.setAttribute("height", h);
        borderLine.setAttribute("transform", `rotate(${angle} ${shelf.X1} ${shelf.Y1})`);
        borderLine.setAttribute("stroke-linejoin", "round");
        borderLine.setAttribute("rx", "10px");
        borderLine.setAttribute("ry", "10px");

        borderLine.setAttribute("id", "tempLine-" + shelf.Id);

        shelfElement.before(borderLine);
    }, { signal: editorSignalController.signal });
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
    svgG.appendChild(line);

    const hitLine = document.createElementNS("http://www.w3.org/2000/svg", "line");
    setWallPos(hitLine, shelf.X1, shelf.X2, shelf.Y1, shelf.Y2);
    hitLine.setAttribute("stroke", "transparent");
    hitLine.setAttribute("stroke-width", 20);
    hitLine.setAttribute("class", "hitShelf");
    hitLine.setAttribute("data-id", shelf.Id);  
    svgG.appendChild(hitLine);

    if (_editorMode != "producteditor") addPropertiesEvent(hitLine);
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

function createHandle(wall, propX, propY, itemType) {
    const [handle, hitArea] = createHandleElements(wall[propX], wall[propY], wall.Id);

    let handleObj = {
        walls: [{ wall, propX, propY, itemType }],
        handle,
        hitArea
    };

    if (itemType === "Wall") {
        const existing = getKey(wall[propX], wall[propY], itemType);
        if (existing && existing.walls[0].itemType === "Wall") {
            let connectedWalls = existing.hitArea.dataset.wallId.split(",");
            connectedWalls.push(wall.Id);
            existing.hitArea.dataset.wallId = connectedWalls;
            existing.walls.push({ wall, propX, propY, itemType });
            return;
        }
    }

    setKey(wall[propX], wall[propY], handleObj);

    svgG.appendChild(handle);
    svgG.appendChild(hitArea);

    makeDraggable(handleObj);
}

function xy(e, snapToGrid = true) {
    const rect = svg.getBoundingClientRect();
    const viewBox = svg.viewBox.baseVal;

    let touch = null;
    if (e.touches && e.touches.length > 0) {
        touch = e.touches[0];
    } else if (e.changedTouches && e.changedTouches.length > 0) {
        touch = e.changedTouches[0];
    }

    const clientX = touch ? touch.clientX : e.clientX;
    const clientY = touch ? touch.clientY : e.clientY;

    // Mouse position relative to SVG element
    const mouseX = clientX - rect.left;
    const mouseY = clientY - rect.top;

    // Convert to SVG coordinates based on current viewBox
    const svgX = viewBox.x + (mouseX / rect.width) * viewBox.width;
    const svgY = viewBox.y + (mouseY / rect.height) * viewBox.height;

    if (!snapToGrid) return [svgX, svgY];

    // Snap using current viewBox offset (to keep grid aligned)
    const grid = document.getElementById("grid-rect");
    
    let gridOffsetX;
    let gridOffsetY;

    if (grid) {
        gridOffsetX = (grid.getAttribute("x") % gridSize);
        gridOffsetY = (grid.getAttribute("y") % gridSize);
    }
    return [snap(svgX, viewBox.x - gridOffsetX), snap(svgY, viewBox.y - gridOffsetY)];
}

function ereaserTool(e) {
    const target = e.target;
    const isWall = target.classList.contains("wall") || target.classList.contains("hitWall");
    const dict = isWall ? wallsDict : shelvesDict;
    const layoutKey = isWall ? "Wall" : "Shelf";

    if (target.tagName === "line") {
        const itemId = target.getAttribute("data-id");
        const handleObjs = Object.values(dict).filter(h =>
            h.walls.some(w => w.wall.Id.toString() === itemId)
        );

        handleObjs.forEach(h => {
            if (h.walls.length > 1) {
                h.walls = h.walls.filter(w => w.wall.Id.toString() !== itemId);

                const connectedIds = h.hitArea.dataset.wallId
                    .split(",")
                    .filter(id => id !== itemId);
                h.hitArea.dataset.wallId = connectedIds;

                removeKey(h.handle.getAttribute("cx"), h.handle.getAttribute("cy"), h.walls[0].itemType);
                setKey(h.handle.getAttribute("cx"), h.handle.getAttribute("cy"), h);
            } else {
                h.handle.remove();
                h.hitArea.remove();
                h.walls.forEach(w => removeKey(w.wall[w.propX], w.wall[w.propY], w.itemType));
            }
        });

        layout[layoutKey] = layout[layoutKey].filter(w => w.Id.toString() !== itemId);
        target.remove();
        svgG.querySelectorAll(`line[data-id="${itemId}"], path[data-id="${itemId}"]`).forEach(l => l.remove());
    } else if (target.classList.contains("handle") || target.classList.contains("hit-area")) {
        const handleObj =
            Object.values(wallsDict).find(h => h.handle === target || h.hitArea === target) ||
            Object.values(shelvesDict).find(h => h.handle === target || h.hitArea === target);

        if (handleObj) {
            handleObj.walls.forEach(w => {
                document.querySelectorAll(`line[data-id="${w.wall.Id}"], path[data-id="${w.wall.Id}"]`).forEach(l => l.remove());

                const dictToUse = w.itemType === "Wall" ? wallsDict : shelvesDict;
                Object.values(dictToUse).forEach(neighbor => {
                    if (neighbor === handleObj) return;
                    if (!neighbor.walls.some(nw => nw.wall.Id === w.wall.Id)) return;

                    neighbor.walls = neighbor.walls.filter(nw => nw.wall.Id !== w.wall.Id);
                    const connectedWalls = neighbor.hitArea.dataset.wallId
                        .split(",")
                        .filter(id => id !== w.wall.Id);
                    neighbor.hitArea.dataset.wallId = connectedWalls;

                    if (neighbor.walls.length === 0) {
                        neighbor.handle.remove();
                        neighbor.hitArea.remove();
                        removeKey(neighbor.handle.getAttribute("cx"), neighbor.handle.getAttribute("cy"), w.itemType);
                    } else {
                        removeKey(neighbor.handle.getAttribute("cx"), neighbor.handle.getAttribute("cy"), w.itemType);
                        setKey(neighbor.handle.getAttribute("cx"), neighbor.handle.getAttribute("cy"), neighbor);
                    }
                });

                const layoutKey = w.itemType === "Wall" ? "Wall" : "Shelf";
                layout[layoutKey] = layout[layoutKey].filter(wall => wall.Id !== w.wall.Id);
                removeKey(w.wall[w.propX], w.wall[w.propY], w.itemType);
            });
            handleObj.handle.remove();
            handleObj.hitArea.remove();
        }
    }

    if (!isWall) {
        svgG.querySelectorAll(`.shelf-side-indicator[data-id="${target.dataset.id}"]`).forEach(ind => ind.remove());
    }
}

function cancelDrawing() {
    if (activeDrawing.startHandle && activeDrawing.startHandle.hitArea.dataset.wallId.length <= 2) {
        activeDrawing.startHandle.handle.remove();
        activeDrawing.startHandle.hitArea.remove();
    }
    activeDrawing.wall.remove();
    activeDrawing = null;
}


function stopDrawing(e) {
    if (!activeDrawing) return;

    const [x, y] = xy(e);

    if (activeDrawing.itemType == "Shelf" && !isPointInPolygon({ x, y }, _polyPoints)) {
        activeDrawing.wall.setAttribute("stroke", "#de0d0d")
        setTimeout(() => activeDrawing.wall.setAttribute("stroke", "#ffe291"), 500);
        return;
    }

    activeDrawing.wallObj.X2 = x;
    activeDrawing.wallObj.Y2 = y;

    if (activeDrawing.startPos.x === x && activeDrawing.startPos.y === y) {
        if (!activeDrawing.hasMoved) {
            activeDrawing.hasMoved = true;
            return;
        }
        cancelDrawing();
        return;
    }
    const rect = svgG.getBBox();
    const type = activeDrawing.itemType;

    if (
        x < 0 ||
        (x > rect.width + rect.x && type == "Shelf") ||
        y < 0 ||
        (y > rect.height + rect.y && type == "Shelf")
    ) return;

    const wallLine = [
        { x: activeDrawing.wallObj.X1, y: activeDrawing.wallObj.Y1 },
        { x: activeDrawing.wallObj.X2, y: activeDrawing.wallObj.Y2 }
    ];

    if (type == "Wall") {
        activeDrawing.wall.setAttribute("stroke", "black");
    } else if (type == "Shelf") {
        if (!isWallInsidePolygon(wallLine, _polyPoints)) {
            activeDrawing.wall.setAttribute("stroke", "#de0d0d")
            setTimeout(() => activeDrawing.wall.setAttribute("stroke", "#ffe291"), 500);
            return;
        }
        if (shelfType != null) {
            setShelfTypeSide(activeDrawing.wall, shelfType, 0);
        } else activeDrawing.wall.setAttribute("stroke", "#fcba03")
    }

    if (activeDrawing.startHandle.hitArea.dataset.wallId.split(",").length == 1) {
        activeDrawing.startHandle.handle.remove();
        activeDrawing.startHandle.hitArea.remove();
    }

    const lineHit = document.createElementNS("http://www.w3.org/2000/svg", "line");
    lineHit.setAttribute("stroke", "transparent");
    lineHit.setAttribute("stroke-width", 20);
    lineHit.setAttribute("class", type === "Wall" ? "hitWall" : "hitShelf");
    lineHit.setAttribute("data-id", activeDrawing.wallObj.Id);

    if (type == "Shelf") {
        addPropertiesEvent(lineHit);
        activeDrawing.wallObj.Type = shelfType ?? 0;
        activeDrawing.wallObj.Side = 0;
    }

    svgG.appendChild(lineHit);
    setWallPos(lineHit, activeDrawing.wallObj.X1, activeDrawing.wallObj.X2, activeDrawing.wallObj.Y1, activeDrawing.wallObj.Y2);

    createHandle(activeDrawing.wallObj, "X1", "Y1", activeDrawing.itemType);
    createHandle(activeDrawing.wallObj, "X2", "Y2", activeDrawing.itemType);

    layout[type].push(activeDrawing.wallObj);
    activeDrawing = null;
}

function isPointInPolygon(point, polygon) {
    const { x, y } = point;

    // Check if point is exactly on any polygon edge (treat as inside)
    for (let i = 0, j = polygon.length - 1; i < polygon.length; j = i++) {
        const xi = polygon[i].x, yi = polygon[i].y;
        const xj = polygon[j].x, yj = polygon[j].y;

        const onSegment =
            (Math.min(xi, xj) <= x && x <= Math.max(xi, xj)) &&
            (Math.min(yi, yj) <= y && y <= Math.max(yi, yj)) &&
            Math.abs((y - yi) * (xj - xi) - (x - xi) * (yj - yi)) < 1e-6;

        if (onSegment) return true; // ✅ consider on-wall as inside
    }

    // Regular ray-casting
    let inside = false;
    for (let i = 0, j = polygon.length - 1; i < polygon.length; j = i++) {
        const xi = polygon[i].x, yi = polygon[i].y;
        const xj = polygon[j].x, yj = polygon[j].y;

        const intersect =
            ((yi > y) !== (yj > y)) &&
            (x < (xj - xi) * (y - yi) / (yj - yi) + xi);

        if (intersect) inside = !inside;
    }

    return inside;
}

function isWallInsidePolygon(wall, polygon) {
    const [p1, p2] = wall;

    // If any wall endpoint is outside the polygon (and not on edge), reject
    if (!isPointInPolygon(p1, polygon) || !isPointInPolygon(p2, polygon)) {
        return false;
    }

    // Check if wall *crosses through* the polygon (shouldn’t happen)
    for (let i = 0; i < polygon.length; i++) {
        const q1 = polygon[i];
        const q2 = polygon[(i + 1) % polygon.length];
        if (segmentsIntersect(p1, p2, q1, q2)) {
            // Allow if the intersection happens only at shared endpoints (touching same line)
            if (!pointsEqual(p1, q1) && !pointsEqual(p1, q2) &&
                !pointsEqual(p2, q1) && !pointsEqual(p2, q2)) {
                return false;
            }
        }
    }

    return true;
}

function pointsEqual(a, b, eps = 1e-6) {
    return Math.abs(a.x - b.x) < eps && Math.abs(a.y - b.y) < eps;
}

// Example: segment intersection test
function ccw(a, b, c) {
    return (c.y - a.y) * (b.x - a.x) > (b.y - a.y) * (c.x - a.x);
}

function segmentsIntersect(a, b, c, d) {
    return ccw(a, c, d) !== ccw(b, c, d) &&
        ccw(a, b, c) !== ccw(a, b, d);
}

function registerDrawStart(e, toolType = "Wall") {
    if (toolType == "Shelf" && (e.target.tagName != "polygon" && !e.target.classList.contains('hit-area'))) return;
    const [x, y] = xy(e);
    let startHandle = getKey(x, y, toolType);

    const firstHandle = svgG.querySelector(".handle");
    tIdCount++;
    let tmpWall = document.createElementNS("http://www.w3.org/2000/svg", "line");
    setWallPos(tmpWall, x, x, y, y);
    if (toolType == "Wall") {
        tmpWall.setAttribute("stroke", "#6da4f7");
        tmpWall.setAttribute("stroke-width", 2);
        tmpWall.setAttribute("class", "wall");
    } else if (toolType == "Shelf") {
        tmpWall.setAttribute("stroke", "#ffe291");
        tmpWall.setAttribute("stroke-width", 5);
        tmpWall.setAttribute("class", "shelf");
    }

    const tmpId = "t" + tIdCount;
    tmpWall.setAttribute("data-id", tmpId);

    const tmpWallObj = {
        Id: tmpId,
        X1: x,
        Y1: y,
        X2: x,
        Y2: y
    };

    if (firstHandle) svgG.insertBefore(tmpWall, firstHandle);
    else svgG.appendChild(tmpWall);

    const walls = { wall: tmpWallObj, propX: 'X1', propY: 'Y1', itemType: toolType };

    if (!startHandle || toolType === "Shelf") {
        const [tmpHandle, tmpHitArea] = createHandleElements(x, y, tmpId);
        tmpHandle.setAttribute("stroke-width", "2");
        tmpHandle.setAttribute("stroke", "#fcba03");

        svgG.appendChild(tmpHandle);
        svgG.appendChild(tmpHitArea);

        startHandle = {
            walls: [walls],
            handle: tmpHandle,
            hitArea: tmpHitArea
        };
    }
    else {
        addWallId(startHandle.hitArea, tmpId);
    }

    activeDrawing = {
        startPos: { x, y },
        startHandle,
        wall: tmpWall,
        wallObj: tmpWallObj,
        hasMoved: false,
        itemType: toolType
    };
}

function makeDraggable(handleObj) {
    let prevX = parseInt(handleObj.handle.getAttribute("cx"));
    let prevY = parseInt(handleObj.handle.getAttribute("cy"));
    let eventObj = {
        dragging: false,
        drawing: false,
        prevX,
        prevY,
        handleObj
    };

    handleObj.hitArea.addEventListener('mousedown', e => {
        if (!activeDrawing) eventObj.dragging = true;
        document.body.style.cursor = "pointer";
    }, { signal: editorSignalController.signal });

    handleObj.hitArea.addEventListener('touchstart', e => {
        if (!activeDrawing) {
            eventObj.dragging = true;
            const [x, y] = xy(e);
            eventObj.prevX = x;
            eventObj.prevY = y;
        }
    }, { passive: false, signal: editorSignalController.signal });

    allEventObjects.push(eventObj);
    document.addEventListener('mouseup', e => {
        eventObj.drawing = false;
        eventObj.dragging = false;
        document.body.style.cursor = "default";
    }, { signal: editorSignalController.signal });

    document.addEventListener('touchend', e => {
        eventObj.drawing = false;
        eventObj.dragging = false;
        document.body.style.cursor = "default";
    }, { passive: false, signal: editorSignalController.signal });
}

function drawHandler(e, activeDrawing) {
    const [x, y] = xy(e);
    if (x === activeDrawing.startPos.x && y === activeDrawing.startPos.y) return;
    activeDrawing.wall.setAttribute("x2", x);
    activeDrawing.wall.setAttribute("y2", y);
}

function dragHandler(e, eventObj) {
    let dragging = eventObj.dragging;
    let prevX = eventObj.prevX;
    let prevY = eventObj.prevY;
    let handleObj = eventObj.handleObj;
    const type = handleObj.walls[0].itemType;

    if (!dragging) return;

    const [x, y] = xy(e);

    if (x === prevX && y === prevY) return;
    removeKey(prevX, prevY, type);

    if (type == "Shelf" && !isPointInPolygon({x,y}, _polyPoints)) return;

    const conflictHandle = getKey(x, y, type);
    if (conflictHandle) {
        //if (type != "wall" || conflictHandle.walls[0].wall.type != "wall") return;
        if (handleObj.walls.some(w => conflictHandle.hitArea.dataset.wallId.split(",").includes(w.wall.Id.toString()))) return;
        if (conflictHandle.hitArea.dataset.wallId.split(",").length == 1 && type == "Wall" && conflictHandle.walls[0].itemType == "Wall") {
            handleConflict(conflictHandle, handleObj, x, y);
        }
    }

    handleObj.walls.forEach(e => {
        e.wall[e.propX] = x;
        e.wall[e.propY] = y;
    });

    handleObj.handle.setAttribute("cy", y);
    handleObj.handle.setAttribute("cx", x);
    handleObj.hitArea.setAttribute("cy", y);
    handleObj.hitArea.setAttribute("cx", x);

    setKey(x, y, handleObj);

    eventObj.prevX = x;
    eventObj.prevY = y;

    updateWalls(handleObj.hitArea.dataset.wallId.split(','), type);
}

function addWallId(element, wallId) {
    let connectedWalls = element.dataset.wallId.split(",");
    connectedWalls.push(wallId);

    element.dataset.wallId = connectedWalls;
}

function handleConflict(conflictHandle, handleObj, x, y) {
    conflictHandle.handle.remove();
    conflictHandle.hitArea.remove();

    removeKey(x, y, handleObj.walls[0].itemType);

    addWallId(handleObj.hitArea, conflictHandle.walls[0].wall.Id);

    setKey(x, y, handleObj);

    handleObj.walls.push(conflictHandle.walls[0])
}

function getKey(x, y, type) {
    const key = `${x},${y}`;
    const dict = type === "Wall" ? wallsDict[key] : shelvesDict[key];
    return dict || null;
}

function setKey(x, y, handle) {
    const key = `${x},${y}`;
    if (handle.walls[0].itemType === "Wall") wallsDict[key] = handle;
    else shelvesDict[key] = handle;
}

function removeKey(x, y, type) {
    const key = `${x},${y}`;
    if (type === "Wall") delete wallsDict[key];
    else delete shelvesDict[key];
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

export function getPoly() {
    return buildPolygon(layout.Wall);
}

export function setTool(tool, subtool = null) {
    if (subtool !== null) {
        shelfType = subtool;
    } else {
        shelfType = null;
    }
    selectedTool = tool;
}

export function drawPoly(polyPoints, shelfEditor = false) {
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

export function getData() {
    return layout;
}

export function updateShelf(shelfJson) {
    let newShelfData = JSON.parse(shelfJson);
    const existingShelf = layout.Shelf.find(s => s.Id === newShelfData.Id);

    if (existingShelf) {
        document.querySelectorAll("rect[id^='tempLine-']").forEach(line => line.remove());

        existingShelf.Type = newShelfData.Type;
        existingShelf.Side = newShelfData.Side;
        existingShelf.X1 = newShelfData.X1;
        existingShelf.Y1 = newShelfData.Y1;
        existingShelf.X2 = newShelfData.X2;
        existingShelf.Y2 = newShelfData.Y2;

        updateWalls([existingShelf.Id.toString()], "Shelf");
        
        let shelfElement = document.querySelector(`line.shelf[data-id="${existingShelf.Id}"]`);

        setShelfTypeSide(shelfElement, existingShelf.Type, existingShelf.Side);
    }
}