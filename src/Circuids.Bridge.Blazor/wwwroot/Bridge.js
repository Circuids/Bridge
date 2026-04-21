export function getPlatform() {
    const ua = navigator.userAgent;

    if (/Android/i.test(ua)) return 'Android';
    if (/iPhone|iPad|iPod/i.test(ua)) return 'IOS';
    if (/Windows/i.test(ua)) return 'Windows';
    if (/Mac OS X|Macintosh/i.test(ua)) return 'Mac';
    if (/Linux/i.test(ua)) return 'Linux';

    return 'Unknown';
}

function normalizeWindowsPlatformVersion(version) {
    if (!version || version === 'Unknown') {
        return version;
    }

    const segments = version.split('.').map(Number);
    const major = Number.isFinite(segments[0]) ? segments[0] : NaN;
    const build = Number.isFinite(segments[2]) ? segments[2] : 0;

    // UA-CH reports a Windows platform contract version. Map it to a canonical
    // base build for the detected Windows family.
    if (major >= 13) {
        return '10.0.22000';
    }

    if (major > 0 && major < 13) {
        return '10.0.10240';
    }

    // Legacy user-agent parsing may still surface NT-style values.
    if (major > 10 || (major === 10 && build >= 22000)) {
        return '10.0.22000';
    }

    if (major === 10) {
        return '10.0.10240';
    }

    return version;
}

export async function getPlatformVersion() {
    const uaData = navigator.userAgentData;

    if (uaData?.platform === 'Windows' && typeof uaData.getHighEntropyValues === 'function') {
        try {
            const values = await uaData.getHighEntropyValues(['platformVersion']);
            if (values?.platformVersion) {
                return normalizeWindowsPlatformVersion(values.platformVersion);
            }
        } catch {
            // Fall back to user agent parsing when high-entropy hints are unavailable.
        }
    }

    const ua = navigator.userAgent;

    // Windows fallback: "Windows NT 10.0".
    // Browsers often reduce this value, so userAgentData is preferred when available.
    const winMatch = ua.match(/Windows NT ([\d.]+)/);
    if (winMatch) return normalizeWindowsPlatformVersion(winMatch[1]);

    // Mac: "Mac OS X 10_15_7" or "Mac OS X 10.15.7"
    const macMatch = ua.match(/Mac OS X ([\d._]+)/);
    if (macMatch) return macMatch[1].replace(/_/g, '.');

    // Android: "Android 14"
    const androidMatch = ua.match(/Android ([\d.]+)/);
    if (androidMatch) return androidMatch[1];

    // iOS: "OS 17_0" or "OS 17.0"
    const iosMatch = ua.match(/OS ([\d._]+)/);
    if (iosMatch) return iosMatch[1].replace(/_/g, '.');

    return 'Unknown';
}
