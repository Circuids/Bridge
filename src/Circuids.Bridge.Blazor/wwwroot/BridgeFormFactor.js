let resizeListener = null;
let currentFormFactor = null;

function getFormFactorObj() {
    const width = window.innerWidth;
    const height = window.innerHeight;
    let formFactor;

    if (width <= 767) {
        formFactor = 'Phone';
    } else if (width <= 1023) {
        formFactor = 'Tablet';
    } else {
        formFactor = 'Desktop';
    }

    return { FormFactor: formFactor, Width: width, Height: height };
}

export function getFormFactor() {
    return JSON.stringify(getFormFactorObj());
}

export function initialize(dotnetObject) {
    if (resizeListener) return;

    currentFormFactor = JSON.stringify(getFormFactorObj());

    resizeListener = async () => {
        const newFormFactor = JSON.stringify(getFormFactorObj());
        if (currentFormFactor !== newFormFactor) {
            currentFormFactor = newFormFactor;
            await dotnetObject.invokeMethodAsync('NotifyFormFactorChanged', newFormFactor);
        }
    };

    window.addEventListener('resize', resizeListener);
}

export function dispose() {
    if (resizeListener) {
        window.removeEventListener('resize', resizeListener);
        resizeListener = null;
        currentFormFactor = null;
    }
}
