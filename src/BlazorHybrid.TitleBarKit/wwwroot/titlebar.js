// Only report geometry. Windows owns mouse capture, dragging, snapping,
// double-click and the caption context menu; no pointer events cross Blazor.
const observers = new WeakMap();

export function observe(element, receiver) {
    unobserve(element);
    let disposed = false;
    let previous = '';
    let frame = 0;
    let dpiQuery;
    const update = () => {
        frame = 0;
        if (disposed || !element.isConnected) return;
        const rect = element.getBoundingClientRect();
        const scale = window.devicePixelRatio || 1;
        const bounds = [rect.x, rect.y, rect.width, rect.height].map(v => Math.round(v * scale));
        const key = bounds.join(',');
        if (key === previous) return;
        previous = key;
        receiver.invokeMethodAsync('UpdateCaptionRegion', ...bounds).catch(error => {
            if (!disposed) console.error('TitleBarKit caption region:', error);
        });
    };
    const schedule = () => { if (!frame) frame = requestAnimationFrame(update); };
    const watchDpi = () => {
        dpiQuery?.removeEventListener('change', watchDpi);
        dpiQuery = matchMedia(`(resolution: ${window.devicePixelRatio}dppx)`);
        dpiQuery.addEventListener('change', watchDpi);
        schedule();
    };
    const resize = new ResizeObserver(schedule);
    resize.observe(element);
    window.addEventListener('resize', schedule);
    window.addEventListener('scroll', schedule, true);
    watchDpi();
    observers.set(element, () => {
        disposed = true;
        resize.disconnect();
        cancelAnimationFrame(frame);
        dpiQuery?.removeEventListener('change', watchDpi);
        window.removeEventListener('resize', schedule);
        window.removeEventListener('scroll', schedule, true);
    });
}

export function unobserve(element) {
    observers.get(element)?.();
    observers.delete(element);
}
