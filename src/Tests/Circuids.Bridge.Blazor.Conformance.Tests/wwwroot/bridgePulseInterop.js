export function publishReport(reportJson) {
    window.__bridgePulseReportJson = reportJson;
    window.__bridgePulseReport = JSON.parse(reportJson);
}