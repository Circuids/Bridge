export function getPlatform() {
    const ua = navigator.userAgent;

    if (/Android/i.test(ua)) return 'Android';
    if (/iPhone|iPad|iPod/i.test(ua)) return 'IOS';
    if (/Windows/i.test(ua)) return 'Windows';
    if (/Mac OS X|Macintosh/i.test(ua)) return 'Mac';
    if (/Linux/i.test(ua)) return 'Linux';

    return 'Web';
}

export function getPlatformVersion() {
    const ua = navigator.userAgent;

    // Windows: "Windows NT 10.0"
    const winMatch = ua.match(/Windows NT ([\d.]+)/);
    if (winMatch) return winMatch[1];

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
