/**
 * download-helper.js
 * Place in: src/Slottet.Web/wwwroot/download-helper.js
 * Reference in: src/Slottet.Web/Components/App.razor (or index.html)
 *   <script src="download-helper.js"></script>
 *
 * Called from HistoryReport.razor via IJSRuntime to trigger a CSV download
 * without a server round-trip.
 */
window.downloadFileFromBase64 = function (filename, mimeType, base64Data) {
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: mimeType });

    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};