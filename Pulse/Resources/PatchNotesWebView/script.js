function SendMessageToProgram() {
    window.chrome.webview.postMessage("msg");
}