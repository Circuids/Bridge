let resizeListener = null;
let orientationListener = null;

export function getSafeAreaInsets() {
    return JSON.stringify(readInsets());
}

export function initializeListener(dotNetObject) {
    if (resizeListener) return;

    const notify = () => {
        const json = JSON.stringify(readInsets());
        dotNetObject.invokeMethodAsync('NotifySafeAreaChanged', json);
    };

    resizeListener = notify;
    orientationListener = notify;

    window.addEventListener('resize', resizeListener);
    window.addEventListener('orientationchange', orientationListener);
}

export function disposeListener() {
    if (resizeListener) {
        window.removeEventListener('resize', resizeListener);
        resizeListener = null;
    }
    if (orientationListener) {
        window.removeEventListener('orientationchange', orientationListener);
        orientationListener = null;
    }
}

function readInsets() {
    const el = document.createElement('div');
    el.style.cssText = `
        position: fixed; visibility: hidden; pointer-events: none;
        padding-top: env(safe-area-inset-top, 0px);
        padding-right: env(safe-area-inset-right, 0px);
        padding-bottom: env(safe-area-inset-bottom, 0px);
        padding-left: env(safe-area-inset-left, 0px);
    `;
    document.body.appendChild(el);

    const style = getComputedStyle(el);
    const insets = {
        Top: parseFloat(style.paddingTop) || 0,
        Right: parseFloat(style.paddingRight) || 0,
        Bottom: parseFloat(style.paddingBottom) || 0,
        Left: parseFloat(style.paddingLeft) || 0,
    };

    document.body.removeChild(el);
    return insets;
}
