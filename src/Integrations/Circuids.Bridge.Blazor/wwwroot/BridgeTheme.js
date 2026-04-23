let mediaQuery = null;
let listener = null;

export function getTheme() {
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        return 'Dark';
    }
    return 'Light';
}

export function initializeListener(dotNetObject) {
    if (listener) return;

    mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

    listener = (e) => {
        const theme = e.matches ? 'Dark' : 'Light';
        dotNetObject.invokeMethodAsync('NotifyThemeChanged', theme);
    };

    mediaQuery.addEventListener('change', listener);
}

export function disposeListener() {
    if (mediaQuery && listener) {
        mediaQuery.removeEventListener('change', listener);
        mediaQuery = null;
        listener = null;
    }
}
