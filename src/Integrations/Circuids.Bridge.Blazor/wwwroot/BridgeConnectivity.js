let intervalId = null;
let isConnected = false;

export async function getNetworkStatus(testUrl) {
    const result = await fetchNetworkStatus(testUrl);
    isConnected = result;
    return result;
}

export function initializeListener(dotNetObject, intervalSeconds, testUrl) {
    if (intervalSeconds > 0) {
        intervalId = setInterval(async () => {
            const controller = new AbortController();
            const timeout = setTimeout(() => controller.abort(), 3000);

            const result = await fetchNetworkStatus(testUrl, controller.signal);
            clearTimeout(timeout);

            if (result !== isConnected) {
                isConnected = result;
                if (dotNetObject) {
                    dotNetObject.invokeMethodAsync('NotifyConnectivityStatusChanged', result);
                }
            }
        }, intervalSeconds * 1000);
    }
}

export function disposeListener() {
    if (intervalId !== null) {
        clearInterval(intervalId);
        intervalId = null;
    }
}

async function fetchNetworkStatus(testUrl, abortSignal) {
    try {
        if (!navigator.onLine) return false;

        const opts = { method: 'HEAD', mode: 'no-cors' };
        if (abortSignal) opts.signal = abortSignal;

        await fetch(testUrl, opts);
        return true;
    } catch {
        return false;
    }
}
