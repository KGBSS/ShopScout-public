export function clearSelection() {
    document.querySelectorAll('rect[id^=tempLine-').forEach(el => el.remove());
    document.querySelector('.properties-container').scrollTop = 0;
}

function intersectionList() {
    const list = [];
    for (let i = 70; i <= 100; i+=2) {
        list.push(i / 100);
    }
    return list;
}

export function initObserver(id) {
    const panel = document.getElementById(id);

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            const isAtTop = entry.boundingClientRect.top <= 90;

            if (isAtTop) {
                panel.classList.add("is-full");
            } else {
                panel.classList.remove("is-full");
            }
        });
    }, {
        threshold: intersectionList(),
        root: null,
        rootMargin: "0px",
    });
    observer.observe(panel);
}
